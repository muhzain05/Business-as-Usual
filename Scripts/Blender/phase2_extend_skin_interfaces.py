"""Extend retained skin boundary rings; conservative reversible clothing occlusion masks.
All exterior vertex positions/UVs retained. Masks can be disabled for clothing swaps.
"""
import bpy,bmesh,json,math
from mathutils import Vector
from collections import defaultdict
from pathlib import Path
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v002'
s=bpy.data.scenes['P2_v002_Repair'];r=next(o for o in s.objects if o.type=='ARMATURE')
def smooth(t):t=max(0,min(1,t));return t*t*(3-2*t)
def setweights(obj,fn):
    obj.vertex_groups.clear()
    for v in obj.data.vertices:
        for n,w in fn(v.co).items():
            if w>1e-6:
                g=obj.vertex_groups.get('mixamorig:'+n) or obj.vertex_groups.new(name='mixamorig:'+n)
                g.add([v.index],w,'REPLACE')
def extend(obj,target,radius):
    bm=bmesh.new();bm.from_mesh(obj.data)
    adj=defaultdict(list)
    for e in bm.edges:
        if e.is_boundary:
            a,b=e.verts;adj[a].append(b);adj[b].append(a)
    clusters=[];unseen=set(adj)
    while unseen:
        first=next(iter(unseen));stack=[first];group=set()
        while stack:
            v=stack.pop()
            if v in group:continue
            group.add(v);stack.extend(adj[v])
        unseen-=group
        if all(len(adj[v])==2 for v in group):clusters.append(group)
    group=max(clusters,key=lambda g:sum(v.co.z for v in g)/len(g))
    start=min(group,key=lambda v:v.index);loop=[start];previous=None;current=start
    while True:
        nxt=next(v for v in adj[current] if v!=previous)
        if nxt==start:break
        loop.append(nxt);previous,current=current,nxt
    center=sum((v.co for v in loop),Vector())/len(loop);target=Vector(target)
    offsets=[v.co-center for v in loop];last=loop
    for t in [.12,.30,.50,.70,.90,1]:
        c=center.lerp(target,t);ring=[]
        for offset in offsets:
            scale=(1-t)+t*radius/max(.001,offset.length)
            ring.append(bm.verts.new(c+offset*scale))
        for i in range(len(last)):
            j=(i+1)%len(last)
            f=bm.faces.new((last[i],last[j],ring[j],ring[i]));f.material_index=len(obj.data.materials);f.smooth=True
        last=ring
    f=bm.faces.new(tuple(last));f.material_index=len(obj.data.materials)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
    obj.data.materials.append(next(m for m in bpy.data.materials if m.name.startswith('Sunny_v002_ConcealedSkin')))
    return len(loop)
def occlusion(obj,fn,name):
    group=obj.vertex_groups.new(name='CoveredSkin_'+name)
    ids=[v.index for v in obj.data.vertices if fn(v.co)]
    if ids:group.add(ids,1,'REPLACE')
    mod=obj.modifiers.new('Clothed visibility — disable for swaps','MASK');mod.vertex_group=group.name;mod.invert_vertex_group=True
    obj['bau_occlusion_note']='Reversible visibility of skin beneath clothing; disable mask for bare-body review.'
    return len(ids)
result={'interfaces':{},'occlusion_masks':{},'limitation':'Torso/pelvis and proximal shoulder joins remain modular overlaps; masks are visible and reversible, not deleted skin.'}
for side,tag in [('Left','L'),('Right','R')]:
    calf=bpy.data.objects['Sunny_v002_LowerLeg_'+tag]
    hip=r.data.bones['mixamorig:'+side+'UpLeg'].head_local
    result['interfaces']['thigh_'+tag]=extend(calf,hip,.060)
    knee=r.data.bones['mixamorig:'+side+'Leg'].head_local;ankle=r.data.bones['mixamorig:'+side+'Foot'].head_local
    def legfn(p,side=side,hip=hip.copy(),knee=knee.copy(),ankle=ankle.copy()):
        t=(p-hip).dot(knee-hip)/(knee-hip).length_squared
        if t<.35:
            b=smooth((t+.05)/.4);return {'Hips':1-b,side+'UpLeg':b}
        if t<1.25:
            b=smooth((t-.75)/.5);return {side+'UpLeg':1-b,side+'Leg':b}
        u=(p-knee).dot(ankle-knee)/(ankle-knee).length_squared;b=smooth((u-.75)/.35)
        return {side+'Leg':1-b,side+'Foot':b}
    setweights(calf,legfn)
    arm=bpy.data.objects['Sunny_v002_ForearmHand_'+tag]
    shoulder=r.data.bones['mixamorig:'+side+'Arm'].head_local
    result['interfaces']['upperarm_'+tag]=extend(arm,shoulder,.030)
    elbow=r.data.bones['mixamorig:'+side+'ForeArm'].head_local;wrist=r.data.bones['mixamorig:'+side+'Hand'].head_local
    def armfn(p,side=side,elbow=elbow.copy(),wrist=wrist.copy()):
        u=(p-elbow).dot(wrist-elbow)/(wrist-elbow).length_squared
        b=smooth((u+.04)/.36);h=smooth((u-.78)/.3)
        return {side+'Arm':1-b,side+'ForeArm':b*(1-h),side+'Hand':b*h}
    setweights(arm,armfn)
    # The source proximal skin includes cream sleeve-coloured fragments. Hide only its covered section.
    def coveredarm(p,elbow=elbow.copy(),wrist=wrist.copy()):
        return (p-elbow).dot(wrist-elbow)/(wrist-elbow).length_squared<.12
    result['occlusion_masks'][arm.name]=occlusion(arm,coveredarm,'Jacket')
    result['occlusion_masks'][calf.name]=occlusion(calf,lambda p:p.z>.305,'Shorts')
    for role in ['Body_UpperArm_','Body_Thigh_']:
        obj=bpy.data.objects['Sunny_v002_'+role+tag];result['occlusion_masks'][obj.name]=occlusion(obj,lambda p:True,'RedundantEnvelope')
for name in ['Body_Torso','Body_Pelvis']:
    obj=bpy.data.objects['Sunny_v002_'+name];result['occlusion_masks'][obj.name]=occlusion(obj,lambda p:True,'ClothedTorso')
for obj in s.objects:
    for mod in obj.modifiers:
        if mod.type=='ARMATURE':mod.use_deform_preserve_volume=True
(OUT/'skin-interfaces.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))

# Keep the failed occlusion trial reversible and off in final review.
for obj in s.objects:
    for mod in obj.modifiers:
        if mod.type=="MASK":mod.show_viewport=False;mod.show_render=False
    if any(n in obj.name for n in ["Body_UpperArm_","Body_Thigh_"]):
        obj["bau_nonproduction"]=True;obj.hide_set(True);obj.hide_render=True
