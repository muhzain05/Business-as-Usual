"""Phase 2 checkpoint only: extract original disconnected parts; no rig/weight repairs.

Run through Blender MCP with BAU_PROJECT_ROOT and optional BAU_VERSION globals.
Appends immutable Phase 1 references. Never overwrites a project or source asset.
"""
import bpy, json, hashlib, math
from pathlib import Path
from collections import defaultdict, Counter
from mathutils import Vector, Matrix
from mathutils.kdtree import KDTree

ROOT=Path(globals().get('BAU_PROJECT_ROOT','D:/Business as Usual'))
VERSION=globals().get('BAU_VERSION','v001')
PREFIX='P2_'+VERSION+'_'
OUT=ROOT/'Docs/Diagnostics/Phase2'/VERSION
PROJECT=ROOT/f'Assets/Characters/Sunny/Working/Sunny_Phase2_Extraction_{VERSION}.blend'
AUDIT=ROOT/'Assets/Characters/Sunny/Working/Sunny_Phase1_Audit.blend'
if PROJECT.exists() or any(s.name.startswith(PREFIX) for s in bpy.data.scenes):
    raise RuntimeError('Version exists; use a new BAU_VERSION instead of overwriting.')
inventory=json.loads((ROOT/'Docs/Diagnostics/asset-inventory.json').read_text(encoding='utf-8'))
for item in inventory:
    assert hashlib.sha256((ROOT/item['path']).read_bytes()).hexdigest()==item['sha256'],item['path']
OUT.mkdir(parents=True,exist_ok=True)
report={'version':VERSION,'phase':'extraction_checkpoint','approval_required_for':'body reconstruction and shared-rig changes',
        'phase1_project_sha256':hashlib.sha256(AUDIT.read_bytes()).hexdigest(),
        'sources':inventory,'modules':[],'rig_changed':False,'weights_repaired':False}
previous=bpy.context.window.scene

def component_lists(mesh):
    parent=list(range(len(mesh.vertices)))
    def find(i):
        while parent[i]!=i: parent[i]=parent[parent[i]]; i=parent[i]
        return i
    for edge in mesh.edges:
        a,b=map(find,edge.vertices); parent[b]=a
    result=defaultdict(list)
    for v in mesh.vertices: result[find(v.index)].append(v.index)
    return sorted(result.values(),key=len,reverse=True)

def action_curves(action):
    if hasattr(action,'fcurves'): return list(action.fcurves)
    return [c for layer in action.layers for strip in layer.strips if hasattr(strip,'channelbags')
            for bag in strip.channelbags for c in bag.fcurves]

def boundary_clusters(mesh,coordinates):
    uses=Counter(tuple(sorted(key)) for p in mesh.polygons for key in p.edge_keys)
    edges=[edge for edge,n in uses.items() if n==1]
    adj=defaultdict(set)
    for a,b in edges: adj[a].add(b); adj[b].add(a)
    unseen=set(adj); clusters=[]
    while unseen:
        first=min(unseen); stack=[first]; ids=set()
        while stack:
            i=stack.pop()
            if i in ids: continue
            ids.add(i); stack.extend(adj[i]-ids)
        unseen-=ids; points=[coordinates[i] for i in ids]
        clusters.append({'vertices':len(ids),'closed_loop':all(len(adj[i])==2 for i in ids),
            'source_vertex_ids':sorted(ids),'center':[sum(p[a] for p in points)/len(points) for a in range(3)],
            'bounds':{'min':[min(p[a] for p in points) for a in range(3)],
                      'max':[max(p[a] for p in points) for a in range(3)]},
            'edges':[list(e) for e in edges if e[0] in ids]})
    return clusters

with bpy.data.libraries.load(str(AUDIT),link=False) as (src,dst):
    dst.scenes=['BAU_Audit_'+label for label in ['Original','Rig','Walk','Run']]
refs={}
for label,scene in zip(['Original','Rig','Walk','Run'],dst.scenes):
    scene.name=PREFIX+'Reference_'+label; refs[label]=scene
    scene['bau_role']='unchanged appended Phase 1 reference'
original=next(o for o in refs['Original'].objects if o.type=='MESH')
walk_mesh=next(o for o in refs['Walk'].objects if o.type=='MESH' and o.vertex_groups)
walk_arm=next(o for o in refs['Walk'].objects if o.type=='ARMATURE')
run_arm=next(o for o in refs['Run'].objects if o.type=='ARMATURE')
mesh=original.data
scale=1.2/(max(v.co.z for v in mesh.vertices)-min(v.co.z for v in mesh.vertices))
coordinates=[(original.matrix_world@v.co)*scale+Vector((0,0,.6)) for v in mesh.vertices]
kd=KDTree(len(walk_mesh.data.vertices))
for v in walk_mesh.data.vertices: kd.insert(walk_mesh.matrix_world@v.co,v.index)
kd.balance()
matches=[kd.find(p) for p in coordinates]
mapping=[i for co,i,d in matches]
assert len(set(mapping))==len(mesh.vertices) and max(d for co,i,d in matches)<1e-5
report['normalization']={'scale':scale,'translation':[0,0,.6],'height':1.2,'up':'Z','forward':'-Y',
                         'note':'working geometry aligned to existing FBX reference; canonical height pending review'}
report['weight_transfer']={'method':'unique nearest rest-position mapping, no index-order assumption',
    'max_mapping_error':max(d for co,i,d in matches),'unique_matches':len(set(mapping)),
    'warning':'existing defective weights intentionally preserved for extraction validation'}
cs=component_lists(mesh)
assert [len(ids) for ids in cs]==[1640,1130,1130,929,929,793,589,187,185,106,76,76,43,42,42]
names=['UpperOutfit','Shoe_R','Shoe_L','ForearmHand_R','ForearmHand_L','Shorts','Head',
       'LowerLeg_R','LowerLeg_L','Mouth','Eye_R','Eye_L','Button_Top','Button_Middle','Button_Bottom']
normals=[n.vector.copy() for n in mesh.corner_normals]
source_edges={tuple(sorted(e.vertices)):e for e in mesh.edges}

scene=bpy.data.scenes.new(PREFIX+'Modules_Rest')
scene.unit_settings.system='METRIC'; scene.unit_settings.scale_length=1
scene['bau_role']='extraction only; defects retained; reconstruction awaits review'
arm=walk_arm.copy(); arm.data=walk_arm.data.copy(); arm.name='Sunny_'+VERSION+'_ReferenceRig'
scene.collection.objects.link(arm); arm.data.pose_position='REST'; arm.show_in_front=True
arm['bau_role']='unaltered clone of 53-bone Mixamo animation reference; not a redesigned rig'
modules=[]; max_uv_error=0; max_normal_error=0; max_position_error=0; max_weight_error=0
for cid,(name,ids) in enumerate(zip(names,cs)):
    lookup={old:new for new,old in enumerate(ids)}; idset=set(ids)
    faces=[p for p in mesh.polygons if p.vertices[0] in idset]
    assert all(all(v in idset for v in p.vertices) for p in faces)
    newmesh=bpy.data.meshes.new('Sunny_'+VERSION+'_'+name+'_Mesh')
    newmesh.from_pydata([coordinates[i] for i in ids],[],[[lookup[i] for i in p.vertices] for p in faces])
    for mat in mesh.materials: newmesh.materials.append(mat)
    for dst_poly,src_poly in zip(newmesh.polygons,faces):
        dst_poly.material_index=src_poly.material_index; dst_poly.use_smooth=src_poly.use_smooth
    for e in newmesh.edges:
        src_edge=source_edges[tuple(sorted(ids[i] for i in e.vertices))]
        e.use_seam=src_edge.use_seam; e.use_edge_sharp=src_edge.use_edge_sharp
    for source_uv in mesh.uv_layers:
        uv=newmesh.uv_layers.new(name=source_uv.name)
        for dst_poly,src_poly in zip(newmesh.polygons,faces):
            for dst_loop,src_loop in zip(dst_poly.loop_indices,src_poly.loop_indices):
                uv.data[dst_loop].uv=source_uv.data[src_loop].uv
    wanted_normals=[normals[i] for p in faces for i in p.loop_indices]
    newmesh.normals_split_custom_set(wanted_normals)
    newmesh.update()
    attr=newmesh.attributes.new('bau_source_vertex','INT','POINT')
    for entry,old in zip(attr.data,ids): entry.value=old
    obj=bpy.data.objects.new('Sunny_'+VERSION+'_'+name,newmesh); scene.collection.objects.link(obj)
    obj.parent=arm; obj.matrix_world=Matrix.Identity(4)
    for group in walk_mesh.vertex_groups: obj.vertex_groups.new(name=group.name)
    for new,old in enumerate(ids):
        for group in walk_mesh.data.vertices[mapping[old]].groups:
            obj.vertex_groups[group.group].add([new],group.weight,'REPLACE')
    mod=obj.modifiers.new('Reference Skin','ARMATURE'); mod.object=arm
    mod.use_deform_preserve_volume=False
    obj['bau_component_id']=cid; obj['bau_role']='extracted source component; weights not repaired'
    modules.append(obj)
    loops=boundary_clusters(newmesh,[v.co for v in newmesh.vertices])
    for loop in loops:
        loop['module_vertex_ids']=loop.pop('source_vertex_ids')
        loop['original_vertex_ids']=[ids[i] for i in loop['module_vertex_ids']]
    normal_error=max((a.vector-b).length for a,b in zip(newmesh.corner_normals,wanted_normals))
    uv_error=max([(newmesh.uv_layers[u.name].data[d].uv-u.data[s].uv).length
        for u in mesh.uv_layers for dp,sp in zip(newmesh.polygons,faces)
        for d,s in zip(dp.loop_indices,sp.loop_indices)] or [0])
    position_error=max((v.co-coordinates[old]).length for v,old in zip(newmesh.vertices,ids))
    weight_error=max([abs(g.weight-next(x.weight for x in walk_mesh.data.vertices[mapping[ids[v.index]]].groups if x.group==g.group))
        for v in newmesh.vertices for g in v.groups] or [0])
    max_normal_error=max(max_normal_error,normal_error); max_uv_error=max(max_uv_error,uv_error)
    max_position_error=max(max_position_error,position_error); max_weight_error=max(max_weight_error,weight_error)
    report['modules'].append({'id':cid,'name':obj.name,'semantic_name':name,'vertices':len(ids),
        'triangles':len(faces),'original_vertex_ids':ids,'original_polygon_ids':[p.index for p in faces],
        'material_names':[mat.name for mat in newmesh.materials],'uv_layers':[u.name for u in newmesh.uv_layers],
        'custom_normals':newmesh.has_custom_normals,'normal_error_max':normal_error,'uv_error_max':uv_error,
        'position_error_max':position_error,'weight_error_max':weight_error,'boundary_clusters':loops,
        'bounds':{'min':[min(v.co[a] for v in newmesh.vertices) for a in range(3)],
                  'max':[max(v.co[a] for v in newmesh.vertices) for a in range(3)]}})
assert sum(len(o.data.vertices) for o in modules)==len(mesh.vertices)
assert sum(len(o.data.polygons) for o in modules)==len(mesh.polygons)
assert max_normal_error<.001 and max_uv_error<1e-7 and max_position_error<1e-7 and max_weight_error<1e-7
report['extraction_validation']={'vertices_total':sum(len(o.data.vertices) for o in modules),
    'triangles_total':sum(len(o.data.polygons) for o in modules),'modules':len(modules),
    'max_uv_error':max_uv_error,'max_corner_normal_error':max_normal_error,
    'max_rest_position_error':max_position_error,'max_weight_error':max_weight_error,
    'material_slots_preserved':all(list(o.data.materials)==list(mesh.materials) for o in modules),
    'triangles_cut_or_created':0,'passed':True}

# A like-for-like source reference, normalized as an object transform only.
before=bpy.data.scenes.new(PREFIX+'Before_Normalized')
before.unit_settings.system='METRIC'
reference=original.copy(); reference.name='Sunny_'+VERSION+'_Before_Normalized'
reference.matrix_world=Matrix.Translation(Vector((0,0,.6)))@Matrix.Scale(scale,4)@original.matrix_world
before.collection.objects.link(reference)

# Visual review scene: copies of modules laid out without changing assembly alignment.
overview=bpy.data.scenes.new(PREFIX+'Modules_Overview')
for index,obj in enumerate(modules):
    display=obj.copy(); display.name='Review_'+VERSION+'_'+names[index]; display.parent=None
    for mod in list(display.modifiers): display.modifiers.remove(mod)
    overview.collection.objects.link(display)
    center=Vector([(min(v.co[a] for v in obj.data.vertices)+max(v.co[a] for v in obj.data.vertices))/2 for a in range(3)])
    x=(index%4-1.5)*.72; z=(3-index//4)*.6
    display.matrix_world=Matrix.Translation(Vector((x,0,z))-center)
    label=bpy.data.curves.new('ReviewLabel_'+names[index],'FONT'); label.body=names[index]
    label.align_x='CENTER'; label.size=.038
    text=bpy.data.objects.new('Label_'+VERSION+'_'+names[index],label); overview.collection.objects.link(text)
    text.location=(x,-.34,z-.26); text.rotation_euler=(math.pi/2,0,0)

# Data-only proposed reconstruction regions; no body surfaces are created.
report['proposed_regions']=[
    {'name':'Torso_Neck','bounds':{'min':[-.17,-.10,.42],'max':[.17,.13,.77]},
     'work':'concealed torso and short neck; shoulder rings join existing upper-limb boundaries; fit inside outfit'},
    {'name':'Pelvis','bounds':{'min':[-.17,-.08,.33],'max':[.17,.13,.46]},
     'work':'concealed hips, waist and crotch; join torso to two short thigh surfaces inside shorts'},
    {'name':'UpperArm_R','start':[-.171094,.03639,.625782],'end':[-.232353,.046062,.551625],
     'work':'extend existing proximal skin boundary beneath sleeve; retain visible forearm/hand shell'},
    {'name':'UpperArm_L','start':[.175781,.03639,.625782],'end':[.240584,.046622,.547337],
     'work':'extend existing proximal skin boundary beneath sleeve; retain visible forearm/hand shell'},
    {'name':'Thigh_R','start':[-.064986,.031702,.404229],'end':[-.116549,.019876,.277838],
     'work':'join pelvis to exposed upper leg boundary inside shorts; use knee loops only where needed'},
    {'name':'Thigh_L','start':[.068981,.031702,.404387],'end':[.121154,.019938,.278452],
     'work':'join pelvis to exposed upper leg boundary inside shorts; use knee loops only where needed'},
    {'name':'Ankle_Foot_Interfaces','work':'extend/cap concealed skin inside shoes; retain exterior shoe silhouette and existing visible lower legs'}]
report['skeleton_proposal']={'reference':arm.name,'bones':len(arm.data.bones),
    'strategy':'retain 53-bone animation reference names/rest matrices for first prototype; no bone edits at checkpoint',
    'head_control':'defer controller creation; existing mixamorig:Head remains deformation reference',
    'walk_action':walk_arm.animation_data.action.name,'run_action':run_arm.animation_data.action.name}
report['scenes']={'rest':scene.name,'before':before.name,'overview':overview.name,
                 'references':{k:s.name for k,s in refs.items()}}
bpy.context.window.scene=previous
(OUT/'modular-inventory.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({'scenes':report['scenes'],'validation':report['extraction_validation'],
                  'normalization':report['normalization'],'weights':report['weight_transfer']},indent=2))
