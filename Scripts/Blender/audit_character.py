"""Non-destructive scene diagnostics; writes JSON, restores pose/frame state."""
import bpy, json, math
from pathlib import Path
from collections import Counter
from mathutils import Vector

ROOT = Path(globals().get('BAU_PROJECT_ROOT', 'D:/Business as Usual'))
def vec(v): return [round(float(x), 6) for x in v]
def bounds(points):
    return {'min': [min(p[i] for p in points) for i in range(3)],
            'max': [max(p[i] for p in points) for i in range(3)]} if points else None
def curves(action):
    if hasattr(action, 'fcurves'): return list(action.fcurves)
    result = []
    for layer in action.layers:
        for strip in layer.strips:
            if hasattr(strip, 'channelbags'):
                for bag in strip.channelbags: result.extend(bag.fcurves)
    return result
def mesh_info(o):
    m = o.data
    m.calc_loop_triangles()
    parent = list(range(len(m.vertices)))
    def find(x):
        while parent[x] != x:
            parent[x] = parent[parent[x]]; x = parent[x]
        return x
    for e in m.edges:
        a,b = map(find, e.vertices); parent[b] = a
    components = {}
    for v in m.vertices: components.setdefault(find(v.index), []).append(v.index)
    incidence = Counter()
    deg = Counter()
    for e in m.edges:
        for v in e.vertices: deg[v] += 1
    for p in m.polygons:
        for key in p.edge_keys: incidence[tuple(sorted(key))] += 1
    deform_names = {b.name for mod in o.modifiers if mod.type == 'ARMATURE' and mod.object
                    for b in mod.object.data.bones if b.use_deform}
    groups = {g.index: g.name for g in o.vertex_groups}
    totals, influences, dominant = [], [], Counter()
    unweighted = 0; invalid_groups = Counter(); negatives = 0
    component_reports = []
    for v in m.vertices:
        weights = [(groups.get(g.group,'?'), g.weight) for g in v.groups if g.weight > 1e-6]
        dw = [(n,w) for n,w in weights if n in deform_names]
        totals.append(sum(w for n,w in dw)); influences.append(len(dw))
        if deform_names and not dw: unweighted += 1
        for n,w in weights:
            if n not in deform_names: invalid_groups[n] += 1
        negatives += sum(g.weight < 0 for g in v.groups)
        if dw: dominant[max(dw, key=lambda t:t[1])[0]] += 1
    for ids in sorted(components.values(), key=len, reverse=True):
        dom = Counter()
        for i in ids:
            ws = [(groups.get(g.group,'?'), g.weight) for g in m.vertices[i].groups if g.weight > 1e-6]
            if ws: dom[max(ws,key=lambda t:t[1])[0]] += 1
        component_reports.append({'vertices':len(ids), 'local_bounds':bounds([vec(m.vertices[i].co) for i in ids]),
                                  'dominant_groups':dict(dom.most_common(8))})
    positions = Counter(tuple(round(x,6) for x in v.co) for v in m.vertices)
    return {'name':o.name, 'mesh':m.name, 'vertices':len(m.vertices), 'edges':len(m.edges),
            'polygons':len(m.polygons), 'triangles':len(m.loop_triangles),
            'polygon_sides':dict(Counter(len(p.vertices) for p in m.polygons)),
            'boundary_edges':sum(incidence[tuple(sorted(e.vertices))]==1 for e in m.edges),
            'nonmanifold_edges_excluding_boundary':sum(incidence[tuple(sorted(e.vertices))]!=2 and incidence[tuple(sorted(e.vertices))]!=1 for e in m.edges),
            'loose_vertices':sum(deg[v.index]==0 for v in m.vertices),
            'zero_area_faces':sum(p.area < 1e-12 for p in m.polygons),
            'valence_histogram':dict(Counter(deg[v.index] for v in m.vertices)),
            'coincident_vertex_excess_6dp':sum(n-1 for n in positions.values() if n>1),
            'components':component_reports,
            'world_bounds':bounds([vec(o.matrix_world @ v.co) for v in m.vertices]),
            'location':vec(o.location), 'rotation_radians':vec(o.rotation_euler), 'scale':vec(o.scale),
            'parent':o.parent.name if o.parent else None,
            'materials':[s.material.name if s.material else None for s in o.material_slots],
            'material_face_counts':dict(Counter(p.material_index for p in m.polygons)),
            'uv_layers':[u.name for u in m.uv_layers], 'shape_keys':list(m.shape_keys.key_blocks.keys()) if m.shape_keys else [],
            'modifiers':[{'type':mod.type,'target':mod.object.name if mod.type=='ARMATURE' and mod.object else None,
                          'preserve_volume':mod.use_deform_preserve_volume if mod.type=='ARMATURE' else None} for mod in o.modifiers],
            'vertex_groups':list(groups.values()),
            'weights':{'unweighted_deform_vertices':unweighted, 'sum_min':min(totals,default=0),
                       'sum_max':max(totals,default=0), 'nonunit_sums':sum(abs(t-1)>1e-4 for t in totals) if deform_names else None,
                       'influence_histogram':dict(Counter(influences)), 'nondeform_groups':dict(invalid_groups),
                       'dominant_bones':dict(dominant), 'negative_weights':negatives}}
def arm_info(o):
    return {'name':o.name, 'location':vec(o.location), 'scale':vec(o.scale), 'rotation_radians':vec(o.rotation_euler),
            'bones':[{'name':b.name,'parent':b.parent.name if b.parent else None,'deform':b.use_deform,
                      'head':vec(b.head_local),'tail':vec(b.tail_local), 'length':b.length,
                      'rest_matrix':[vec(row) for row in b.matrix_local],
                      'connected':b.use_connect} for b in o.data.bones],
            'constraints':{p.name:[c.type for c in p.constraints] for p in o.pose.bones if p.constraints},
            'action':o.animation_data.action.name if o.animation_data and o.animation_data.action else None,
            'nla_tracks':[t.name for t in o.animation_data.nla_tracks] if o.animation_data else []}

previous = bpy.context.window.scene
report={'blender':bpy.app.version_string,'scenes':{},'materials':{},'images':[],'actions':[]}
try:
    for scene in bpy.data.scenes:
        if not scene.name.startswith('BAU_Audit_'): continue
        bpy.context.window.scene=scene
        bpy.context.view_layer.update()
        report['scenes'][scene.name]={'source':scene.get('bau_source'),'unit_system':scene.unit_settings.system,
                                    'unit_scale':scene.unit_settings.scale_length,'fps':scene.render.fps,
                                    'frame_range':[scene.frame_start,scene.frame_end],
                                    'meshes':[mesh_info(o) for o in scene.objects if o.type=='MESH'],
                                    'armatures':[arm_info(o) for o in scene.objects if o.type=='ARMATURE']}
    used_mats={s.material for sc in bpy.data.scenes if sc.name.startswith('BAU_Audit_') for o in sc.objects
               if o.type=='MESH' for s in o.material_slots if s.material}
    used_images=set()
    for mat in used_mats:
        nodes=mat.node_tree.nodes if mat.use_nodes else []
        imgs=[n.image for n in nodes if n.type=='TEX_IMAGE' and n.image]
        used_images.update(imgs)
        report['materials'][mat.name]={'images':[im.name for im in imgs],
             'nodes':[n.type for n in nodes], 'links':[(l.from_node.type,l.from_socket.name,l.to_node.type,l.to_socket.name) for l in mat.node_tree.links] if mat.use_nodes else []}
    for im in used_images:
        path=bpy.path.abspath(im.filepath)
        report['images'].append({'name':im.name,'filepath':im.filepath,'resolved_path':path,
                                'packed':bool(im.packed_file),'exists':Path(path).is_file(),
                                'size':list(im.size),'colorspace':im.colorspace_settings.name})
    for a in bpy.data.actions:
        cs=curves(a)
        report['actions'].append({'name':a.name,'frame_range':list(a.frame_range),'fcurves':len(cs),
             'keyframes':sum(len(c.keyframe_points) for c in cs),'channels':dict(Counter(c.data_path.split('.')[-1] for c in cs)),
             'paths':sorted(set(c.data_path for c in cs))})
finally: bpy.context.window.scene=previous
(ROOT/'Docs/Diagnostics/character-data.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({'scenes':{s:{'meshes':[{k:m[k] for k in ['name','vertices','triangles','polygon_sides','boundary_edges','nonmanifold_edges_excluding_boundary','world_bounds','weights','shape_keys']}|{'component_sizes':[c['vertices'] for c in m['components']]} for m in data['meshes']],
                            'armatures':[{'name':a['name'],'bones':len(a['bones']),'action':a['action']} for a in data['armatures']]} for s,data in report['scenes'].items()},
                  'images':report['images'],'actions':[{k:a[k] for k in ['name','frame_range','fcurves','keyframes','channels']} for a in report['actions']]},indent=2))
