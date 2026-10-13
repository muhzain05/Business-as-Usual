"""Sample existing clips and compare rest skeletons; no geometry edits."""
import bpy, json, math
from pathlib import Path
from collections import Counter, defaultdict

ROOT=Path(globals().get('BAU_PROJECT_ROOT', 'D:/Business as Usual'))
prior=bpy.context.window.scene
result={'clips':{},'component_weights':[],'rig_split_weights':{}}
def canonical(name): return name.split(':')[-1]
def percentile(xs,q): return sorted(xs)[min(len(xs)-1,round((len(xs)-1)*q))]
def components(m):
    par=list(range(len(m.vertices)))
    def find(i):
        while par[i]!=i: par[i]=par[par[i]]; i=par[i]
        return i
    for e in m.edges:
        a,b=map(find,e.vertices); par[b]=a
    cs=defaultdict(list)
    for v in m.vertices: cs[find(v.index)].append(v.index)
    return sorted(cs.values(),key=len,reverse=True)
rig=next(o for o in bpy.data.scenes['BAU_Audit_Rig'].objects if o.type=='ARMATURE')
walk=next(o for o in bpy.data.scenes['BAU_Audit_Walk'].objects if o.type=='ARMATURE')
rb={canonical(b.name):b for b in rig.data.bones}; wb={canonical(b.name):b for b in walk.data.bones}
result['skeleton_comparison']={'rig_only':sorted(rb.keys()-wb.keys()),'clip_only':sorted(wb.keys()-rb.keys()),
    'common':[{'bone':n,'head_delta':(rb[n].head_local-wb[n].head_local).length,
               'tail_delta':(rb[n].tail_local-wb[n].tail_local).length,
               'orientation_degrees':math.degrees(rb[n].matrix_local.to_quaternion().rotation_difference(wb[n].matrix_local.to_quaternion()).angle)} for n in sorted(rb.keys()&wb.keys())]}
rm=next(o for o in bpy.data.scenes['BAU_Audit_Rig'].objects if o.type=='MESH')
coincident=defaultdict(list)
for v in rm.data.vertices: coincident[tuple(round(x,6) for x in v.co)].append(v)
weight_discontinuities=0
for vs in coincident.values():
    if len(vs)<2: continue
    weights=[{g.group:g.weight for g in v.groups} for v in vs]
    if any(max([abs(w.get(k,0)-weights[0].get(k,0)) for k in set(w)|set(weights[0])] or [0])>1e-4 for w in weights[1:]): weight_discontinuities+=1
result['rig_split_weights']={'coincident_positions':sum(len(v)>1 for v in coincident.values()),'weight_discontinuity_positions':weight_discontinuities}
try:
    for label in ['Rig','Walk','Run']:
        scene=bpy.data.scenes['BAU_Audit_'+label]; bpy.context.window.scene=scene
        arm=next(o for o in scene.objects if o.type=='ARMATURE')
        mesh=next(o for o in scene.objects if o.type=='MESH' and o.vertex_groups)
        original_frame=scene.frame_current; original_pose=arm.data.pose_position
        a=arm.animation_data.action
        lo,hi=map(int,a.frame_range)
        frames=sorted(set(round(lo+(hi-lo)*i/8) for i in range(9)))
        entry={'frames':[],'sample_frames':frames,'bone_scale_deviation_max':0,'bone_location_max':0}
        try:
            arm.data.pose_position='REST'; bpy.context.view_layer.update()
            dg=bpy.context.evaluated_depsgraph_get(); ev=mesh.evaluated_get(dg); base=ev.to_mesh()
            rest=[v.co.copy() for v in base.vertices]; ev.to_mesh_clear()
            edges=[tuple(e.vertices) for e in mesh.data.edges]
            lengths=[(rest[i]-rest[j]).length for i,j in edges]
            arm.data.pose_position='POSE'
            for frame in frames:
                scene.frame_set(frame); bpy.context.view_layer.update()
                dg=bpy.context.evaluated_depsgraph_get(); ev=mesh.evaluated_get(dg); posed=ev.to_mesh()
                coords=[v.co.copy() for v in posed.vertices]
                ratios=[((coords[i]-coords[j]).length/L,ei) for ei,((i,j),L) in enumerate(zip(edges,lengths)) if L>1e-7]
                values=[t[0] for t in ratios]
                worst=sorted(ratios,reverse=True)[:10]
                entry['frames'].append({'frame':frame,'min_edge_ratio':min(values),'median_edge_ratio':percentile(values,.5),
                    'p95_edge_ratio':percentile(values,.95),'p99_edge_ratio':percentile(values,.99),'max_edge_ratio':max(values),
                    'edges_over_1_5':sum(v>1.5 for v in values),'edges_under_0_5':sum(v<.5 for v in values),
                    'worst_edges':[{'ratio':ratio,'rest_midpoint':list((rest[edges[ei][0]]+rest[edges[ei][1]])/2),
                         'vertex_groups':[{mesh.vertex_groups[g.group].name:round(g.weight,4) for g in mesh.data.vertices[v].groups} for v in edges[ei]]} for ratio,ei in worst]})
                entry['bone_scale_deviation_max']=max(entry['bone_scale_deviation_max'],max(abs(x-1) for b in arm.pose.bones for x in b.scale))
                entry['bone_location_max']=max(entry['bone_location_max'],max(b.location.length for b in arm.pose.bones))
                ev.to_mesh_clear()
        finally:
            arm.data.pose_position=original_pose; scene.frame_set(original_frame); bpy.context.view_layer.update()
        result['clips'][label]=entry
    scene=bpy.data.scenes['BAU_Audit_Walk']; mesh=next(o for o in scene.objects if o.type=='MESH' and o.vertex_groups)
    for ci,ids in enumerate(components(mesh.data)):
        weights=Counter()
        for i in ids:
            for g in mesh.data.vertices[i].groups: weights[mesh.vertex_groups[g.group].name]+=g.weight
        result['component_weights'].append({'id':ci,'vertices':len(ids),'average_weights':{k:v/len(ids) for k,v in weights.most_common()}})
    original=next(o for o in bpy.data.scenes['BAU_Audit_Original'].objects if o.type=='MESH')
    cs=components(original.data); assignment={v:i for i,ids in enumerate(cs) for v in ids}
    geometry={'vertices':[list(v.co) for v in original.data.vertices], 'faces':[list(p.vertices) for p in original.data.polygons],
              'component_by_vertex':[assignment[v.index] for v in original.data.vertices]}
    (ROOT/'Docs/Diagnostics/component-geometry.json').write_text(json.dumps(geometry),encoding='utf-8')
finally: bpy.context.window.scene=prior
(ROOT/'Docs/Diagnostics/deformation-data.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps({'clips':{k:{'scale_deviation':v['bone_scale_deviation_max'],'location_max':v['bone_location_max'],
         'frames':[{q:f[q] for q in ['frame','p99_edge_ratio','max_edge_ratio','edges_over_1_5','edges_under_0_5']} for f in v['frames']]} for k,v in result['clips'].items()},
         'rig_split_weights':result['rig_split_weights'],'component_weights':result['component_weights']},indent=2))
