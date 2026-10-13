"""Deterministic v002 repair; run in Blender with the saved v001 extraction loaded.
Never edits source/v001 datablocks. Skeleton/rest pose are copied exactly.
"""
import bpy, bmesh, json, math, hashlib
from pathlib import Path
from mathutils import Vector
from collections import Counter, defaultdict

ROOT=Path('D:/Business as Usual'); OUT=ROOT/'Docs/Diagnostics/Phase2/v002'
PROJECT=ROOT/'Assets/Characters/Sunny/Working/Sunny_Phase2_Repair_v002.blend'
assert not PROJECT.exists(), 'Version already saved; choose a new version.'
assert 'P2_v002_Repair' not in bpy.data.scenes, 'Repair scene already exists.'
OUT.mkdir(parents=True,exist_ok=True)
sources=json.loads((ROOT/'Docs/Diagnostics/asset-inventory.json').read_text())
for item in sources: assert hashlib.sha256((ROOT/item['path']).read_bytes()).hexdigest()==item['sha256']
baseline=bpy.data.scenes.get('P2_v001_Modules_Rest')
if baseline is None:
    with bpy.data.libraries.load(str(ROOT/'Assets/Characters/Sunny/Working/Sunny_Phase2_Extraction_v001.blend'),link=False) as (src,dst):
        dst.scenes=[s for s in src.scenes if s.startswith('P2_v001_')]
    baseline=bpy.data.scenes['P2_v001_Modules_Rest']
scene=bpy.data.scenes.new('P2_v002_Repair'); scene.world=baseline.world
scene.render.fps=24; scene.unit_settings.scale_length=1
scene['bau_working_height']=1.2; scene['bau_scale_assumption']='one unit provisionally one metre'
copies={}
for old in baseline.objects:
    obj=old.copy(); obj.data=old.data.copy(); obj.name=old.name.replace('v001','v002')
    scene.collection.objects.link(obj); copies[old]=obj
for old,obj in copies.items():
    obj.parent=copies.get(old.parent)
    for mod in obj.modifiers:
        if mod.type=='ARMATURE': mod.object=copies.get(mod.object,mod.object)
rig=next(o for o in scene.objects if o.type=='ARMATURE'); rig.data.pose_position='REST'
bpy.context.window.scene=scene
modules={o.name.split('Sunny_v002_')[-1]:o for o in scene.objects if o.type=='MESH'}
report={'version':'v002','sources_verified':9,'skeleton_replaced':False,'new_body_surfaces':[],
        'jacket_strategy':'Rebuilt rounded quad shell using original silhouette and colour palette; original retained in v001 reference.',
        'limitations':[],'changes':[]}

def smooth(t):
    t=max(0,min(1,t)); return t*t*(3-2*t)
def blend(a,b,t):return {a:1-t,b:t}
def torso_weights(p):
    z=p.z
    levels=[(.44,'Hips'),(.52,'Spine'),(.60,'Spine1'),(.69,'Spine2'),(.755,'Neck')]
    if z<=levels[0][0]:return {'Hips':1}
    for (a,an),(b,bn) in zip(levels,levels[1:]):
        if z<=b:return blend(an,bn,smooth((z-a)/(b-a)))
    return {'Neck':1}
def arm_weights(p,side):
    a=rig.data.bones['mixamorig:'+side+'Arm'].head_local
    e=rig.data.bones['mixamorig:'+side+'ForeArm'].head_local
    w=rig.data.bones['mixamorig:'+side+'Hand'].head_local
    t=(p-a).dot(e-a)/(e-a).length_squared
    if t<.22:return blend('Spine2',side+'Arm',smooth((t+.35)/.57))
    if t<1.23:return blend(side+'Arm',side+'ForeArm',smooth((t-.73)/.5))
    u=(p-e).dot(w-e)/(w-e).length_squared
    return blend(side+'ForeArm',side+'Hand',smooth((u-.73)/.35))
def leg_weights(p,side):
    hip=rig.data.bones['mixamorig:'+side+'UpLeg'].head_local
    knee=rig.data.bones['mixamorig:'+side+'Leg'].head_local
    ankle=rig.data.bones['mixamorig:'+side+'Foot'].head_local
    t=(p-hip).dot(knee-hip)/(knee-hip).length_squared
    if t<.3:return blend('Hips',side+'UpLeg',smooth((t+.1)/.4))
    if t<1.25:return blend(side+'UpLeg',side+'Leg',smooth((t-.77)/.48))
    u=(p-knee).dot(ankle-knee)/(ankle-knee).length_squared
    return blend(side+'Leg',side+'Foot',smooth((u-.72)/.38))
def weights(obj,fn):
    obj.vertex_groups.clear()
    for v in obj.data.vertices:
        w=fn(v.co); total=sum(w.values())
        for name,value in w.items():
            if value>1e-6:
                name='mixamorig:'+name
                group=obj.vertex_groups.get(name) or obj.vertex_groups.new(name=name)
                group.add([v.index],value/total,'REPLACE')
def material(name,color):
    m=bpy.data.materials.new('Sunny_v002_'+name); m.use_nodes=True
    n=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    n.inputs['Base Color'].default_value=(*color,1); n.inputs['Roughness'].default_value=.78
    m.diffuse_color=(*color,1); return m
# Linear RGB equivalents of the original teal/cream, sampled design palette.
teal=material('JacketTeal',(.085,.205,.24)); cream=material('ShirtCream',(.72,.69,.63))
collar_mat=material('CollarCream',(.81,.78,.71)); skin=material('ConcealedSkin',(.68,.40,.23))
def create(name,verts,faces,mats,matids=None):
    mesh=bpy.data.meshes.new(name+'_Mesh'); mesh.from_pydata(verts,[],faces); mesh.update()
    obj=bpy.data.objects.new('Sunny_v002_'+name,mesh); scene.collection.objects.link(obj)
    for m in mats:mesh.materials.append(m)
    for p in mesh.polygons:p.use_smooth=True; p.material_index=matids[p.index] if matids else 0
    bm=bmesh.new(); bm.from_mesh(mesh); bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces)); bm.to_mesh(mesh); bm.free()
    obj.parent=rig; mod=obj.modifiers.new('Shared Mixamo skin','ARMATURE'); mod.object=rig
    obj['bau_role']=name; return obj
def shell(obj,thickness=.004,subdivision=1):
    # Smooth the rest cage before deformation; weights are interpolated on the same surface.
    arm=obj.modifiers[0]
    sub=obj.modifiers.new('Rounded garment cage','SUBSURF'); sub.levels=subdivision; sub.render_levels=subdivision
    bpy.context.view_layer.objects.active=obj
    bpy.ops.object.modifier_move_up(modifier=sub.name)
    sol=obj.modifiers.new('Visible clothing thickness','SOLIDIFY'); sol.thickness=thickness; sol.offset=-1; sol.use_even_offset=False; sol.thickness_clamp=.5
def tube(name,centers,radii,n=24,mat=skin,weightfn=torso_weights):
    verts=[]; faces=[]
    for k,c in enumerate(centers):
        c=Vector(c); axis=Vector(centers[min(k+1,len(centers)-1)])-Vector(centers[max(0,k-1)])
        axis.normalize(); u=Vector((0,1,0)); u=(u-axis*u.dot(axis)).normalized(); v=axis.cross(u).normalized()
        rx,ry=radii[k] if isinstance(radii[k],tuple) else (radii[k],radii[k])
        for i in range(n):
            a=2*math.pi*i/n; verts.append(c+u*(rx*math.cos(a))+v*(ry*math.sin(a)))
        if k:
            for i in range(n):j=(i+1)%n; faces.append(((k-1)*n+i,(k-1)*n+j,k*n+j,k*n+i))
    faces += [tuple(reversed(range(n))),tuple((len(centers)-1)*n+i for i in range(n))]
    obj=create(name,verts,faces,[mat]); weights(obj,weightfn); report['new_body_surfaces'].append(name); return obj

# Rounded torso with a real front opening and connected shoulder/sleeve topology.
old=modules['UpperOutfit']; scene.collection.objects.unlink(old); old.name='Sunny_v002_UpperOutfit_ExtractedReference'
# Retain the old mesh in its immutable v001 scene, not hidden in the repair assembly.
N=48; alpha=.25
profile=[(.430,.185,.127),(.442,.185,.127),(.49,.181,.122),(.55,.176,.115),(.59,.174,.108),(.63,.179,.103),(.67,.177,.101),(.70,.157,.094),(.727,.110,.080),(.744,.103,.076)]
verts=[]; faces=[]; ids=[]
for z,rx,ry in profile:
    for j in range(N+1):
        t=alpha+(2*math.pi-2*alpha)*j/N
        verts.append((rx*math.sin(t),.030-ry*math.cos(t),z))
# Four columns around either side, three levels: a quad shoulder opening.
holes=[(9,13,4,7,'Left'),(35,39,4,7,'Right')]
for k in range(len(profile)-1):
    for j in range(N):
        if any(a<=j<b and c<=k<d for a,b,c,d,_ in holes):continue
        faces.append((k*(N+1)+j,k*(N+1)+j+1,(k+1)*(N+1)+j+1,(k+1)*(N+1)+j)); ids.append(0)
for a,b,c,d,side in holes:
    loop=[c*(N+1)+j for j in range(a,b)]+[k*(N+1)+b for k in range(c,d)]+[d*(N+1)+j for j in range(b,a,-1)]+[k*(N+1)+a for k in range(d,c,-1)]
    center=sum((Vector(verts[i]) for i in loop),Vector())/len(loop)
    end=Vector((.237 if side=='Left' else -.234,.043,.548))
    axis=(end-center).normalized(); u=Vector((0,-1,0)); u=(u-axis*u.dot(axis)).normalized(); v=axis.cross(u)
    phase=math.atan2((Vector(verts[loop[0]])-center).dot(v),(Vector(verts[loop[0]])-center).dot(u))
    angles=[phase+2*math.pi*i/len(loop) for i in range(len(loop))]
    last=loop
    for t,radius in [(0.23,.071),(.55,.071),(.87,.068),(1,.068)]:
        ring=[]
        for h,angle in enumerate(angles):
            ring.append(len(verts)); verts.append(Vector(verts[loop[h]]).lerp(end+radius*(u*math.cos(angle)+v*math.sin(angle)),t))
        for i in range(len(last)):
            j=(i+1)%len(last); faces.append((last[i],last[j],ring[j],ring[i])); ids.append(0)
        last=ring
# Cream shirt: separate inner surface, joined as a module, never painted on jacket.
shirt_start=len(verts)
for z,rx,ry in [(.436,.146,.096),(.45,.146,.096),(.53,.147,.096),(.62,.148,.095),(.69,.145,.090),(.730,.080,.067),(.740,.079,.066)]:
    for j in range(33):
        t=-math.pi/2+math.pi*j/32
        verts.append((.070*math.sin(t),.030-ry-.006+.012*math.sin(t)**2,z))
for k in range(6):
    for j in range(32):
        i=shirt_start+k*33+j; faces.append((i,i+1,i+34,i+33)); ids.append(1)
# Soft fold-over collar, cream, with a clean front split and low-detail rounded tips.
base=len(verts)
for row in range(4):
    trow=row/3
    for j in range(N+1):
        t=alpha+(2*math.pi-2*alpha)*j/N
        rx=.101+(.145-.101)*trow; ry=.074+(.102-.074)*trow
        z=.752-.034*trow-.009*trow*max(0,math.cos(t))
        verts.append((rx*math.sin(t),.030-ry*math.cos(t)-.003,z))
for k in range(3):
    for j in range(N):i=base+k*(N+1)+j; faces.append((i,i+1,i+N+2,i+N+1)); ids.append(2)
jacket=create('UpperOutfit',verts,faces,[teal,cream,collar_mat],ids); modules['UpperOutfit']=jacket
def jacket_weights(p):
    if abs(p.x)>.154 and p.z>.525 and p.z<.710:
        side='Left' if p.x>0 else 'Right'; w=arm_weights(p,side)
        t=smooth((abs(p.x)-.154)/.055); tw=torso_weights(p)
        out={n:value*(1-t) for n,value in tw.items()}
        for n,value in w.items():out[n]=out.get(n,0)+value*t
        return out
    return torso_weights(p)
weights(jacket,jacket_weights); shell(jacket)
report['changes'].append('Upper outfit rebuilt with connected quad sleeve cages, cream inner shirt, split collar, front opening and 4 mm inward thickness.')

# Complete concealed body envelopes, retained limb exterior; no full-body remesh.
torso=tube('Body_Torso',[(0,.031,z) for z in [.36,.40,.44,.49,.55,.60,.65,.69,.73,.76]],
           [(.084,.12),(.095,.145),(.091,.151),(.088,.145),(.085,.139),(.079,.14),(.073,.144),(.065,.125),(.058,.065),(.055,.062)])
pelvis=tube('Body_Pelvis',[(0,.032,z) for z in [.325,.35,.38,.42,.455]],[(.055,.085),(.074,.118),(.088,.142),(.09,.146),(.086,.145)],weightfn=lambda p:{'Hips':1})
for side,tag in [('Left','L'),('Right','R')]:
    a=rig.data.bones['mixamorig:'+side+'Arm'].head_local.copy(); e=rig.data.bones['mixamorig:'+side+'ForeArm'].head_local.copy()
    tube('Body_UpperArm_'+tag,[a.lerp(e,t) for t in [-.1,0,.2,.45,.7,.9,1.12]],[.032,.034,.038,.040,.040,.039,.038],weightfn=lambda p,s=side:arm_weights(p,s))
    hip=rig.data.bones['mixamorig:'+side+'UpLeg'].head_local.copy(); knee=rig.data.bones['mixamorig:'+side+'Leg'].head_local.copy()
    tube('Body_Thigh_'+tag,[hip.lerp(knee,t) for t in [-.05,0,.2,.45,.68,.88,1.03]], [.06,.067,.070,.067,.059,.052,.047],weightfn=lambda p,s=side:leg_weights(p,s))
    ankle=rig.data.bones['mixamorig:'+side+'Foot'].head_local.copy(); toe=rig.data.bones['mixamorig:'+side+'Foot'].tail_local.copy()
    tube('Body_ConcealedFoot_'+tag,[ankle.lerp(toe,t) for t in [-.30,0,.3,.6,.90]],[.044,.044,.048,.047,.038],weightfn=lambda p,s=side:leg_weights(p,s))
    weights(modules['ForearmHand_'+tag],lambda p,s=side:arm_weights(p,s))
    weights(modules['LowerLeg_'+tag],lambda p,s=side:leg_weights(p,s))
    weights(modules['Shoe_'+tag],lambda p,s=side:{s+'Foot':1})
    report['changes'].append('Regional arm, knee and ankle weights corrected on retained '+tag+' exterior.')
report['limitations'].append('Underlying body is a modular set of overlapping closed envelopes, not a single welded production skin. Shoulder/pelvis joins need artist inspection for bare-body clothing swaps.')

# Shorts: retain exterior, open only downward-facing interior leg-cap triangles.
shorts=modules['Shorts']; bm=bmesh.new(); bm.from_mesh(shorts.data)
removed=[]
for f in list(bm.faces):
    c=f.calc_center_median(); f.normal_update()
    side='Left' if c.x>0 else 'Right'; cx=.115 if c.x>0 else -.115
    interior=((c.x-cx)/.067)**2+((c.y-.032)/.075)**2<1
    if c.z<.283 and f.normal.z<-.45 and interior:removed.append(f)
bmesh.ops.delete(bm,geom=removed,context='FACES'); bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces)); bm.to_mesh(shorts.data); bm.free()
def shorts_weights(p):
    side='Left' if p.x>0 else 'Right'
    t=smooth((.420-p.z)/.132)*smooth(abs(p.x)/.072)
    return blend('Hips',side+'UpLeg',t)
weights(shorts,shorts_weights)
sol=shorts.modifiers.new('Concealed cuff inner thickness','SOLIDIFY'); sol.thickness=.003; sol.offset=-1
report['shorts_cap_faces_removed']=len(removed)
report['changes'].append('Localized shorts cap openings; original outer cuffs and all other surfaces retained, with inward inner rims.')
for name in ['Head','Eye_R','Eye_L','Mouth']:weights(modules[name],lambda p:{'Head':1})
for name in ['Button_Top','Button_Middle','Button_Bottom']:
    obj=modules[name]; center=sum((v.co for v in obj.data.vertices),Vector())/len(obj.data.vertices)
    target=Vector((-.077,.030-.123,.630 if 'Top' in name else .553 if 'Middle' in name else .479))
    shift=target-center
    for v in obj.data.vertices:v.co+=shift
    attachment=jacket_weights(target)
    weights(obj,lambda p,w=attachment:w)
# Independent non-bone facial controls. Texture duplicate is cleaned in a follow-up script.
for name in ['Eye_R','Eye_L','Mouth']:
    obj=modules[name]; obj.shape_key_add(name='Basis')
    key=obj.shape_key_add(name='Blink' if name.startswith('Eye') else 'SmileWidth'); key.value=0
    center=sum((v.co for v in obj.data.vertices),Vector())/len(obj.data.vertices)
    for p in key.data:
        if name.startswith('Eye'):p.co.z=center.z+(p.co.z-center.z)*.10
        else:p.co.x=center.x+(p.co.x-center.x)*1.20
report['changes'].append('Head/face rigid Head attachment; independent eye Blink and mouth SmileWidth shape keys; buttons moved to consistent garment positions and reweighted.')
report['limitations'].append('Facial texture cleanup pending. Facial shape keys are a minimal prototype, not phoneme animation.')
for obj in scene.objects:
    obj.hide_set(False)
scene['bau_report']=str(OUT/'repair-build.json')
(OUT/'repair-build.json').write_text(json.dumps(report,indent=2))
print(json.dumps({'scene':scene.name,'objects':len(scene.objects),'report':report}))
