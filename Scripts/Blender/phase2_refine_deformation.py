"""Refine cage weights explicitly by garment rings, not transverse vertex height."""
import bpy, json, math
from pathlib import Path
from mathutils import Vector
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v002'
s=bpy.data.scenes['P2_v002_Repair'];bpy.context.window.scene=s
rig=next(o for o in s.objects if o.type=='ARMATURE')
def smooth(t):t=max(0,min(1,t));return t*t*(3-2*t)
def setv(obj,vid,w):
    for g in obj.vertex_groups:g.remove([vid])
    for n,x in w.items():
        if x>1e-6:
            g=obj.vertex_groups.get('mixamorig:'+n) or obj.vertex_groups.new(name='mixamorig:'+n)
            g.add([vid],x,'REPLACE')
j=bpy.data.objects['Sunny_v002_UpperOutfit']
for v in j.data.vertices:
    p=v.co
    if v.index<490 and .55<p.z<.715:
        side='Left' if p.x>0 else 'Right';t=smooth((abs(p.x)-.12)/.058)*smooth((p.z-.55)/.045)
        if t:
            old={j.vertex_groups[x.group].name.split(':')[-1]:x.weight*(1-t) for x in v.groups}
            old[side+'Arm']=old.get(side+'Arm',0)+t;setv(j,v.index,old)
# Sleeve cross-sections move coherently with the upper arm; the cuff precedes the elbow.
for side,start in [('Left',490),('Right',546)]:
    for k in range(4):
        t=[.70,.90,1,1][k]
        for i in range(14):setv(j,start+k*14+i,{'Spine2':1-t,side+'Arm':t})
for side,tag in [('Left','L'),('Right','R')]:
    obj=bpy.data.objects['Sunny_v002_ForearmHand_'+tag]
    e=rig.data.bones['mixamorig:'+side+'ForeArm'].head_local
    w=rig.data.bones['mixamorig:'+side+'Hand'].head_local
    for v in obj.data.vertices:
        u=(v.co-e).dot(w-e)/(w-e).length_squared
        elbow=smooth((u+.04)/.36);hand=smooth((u-.78)/.3)
        setv(obj,v.index,{side+'Arm':1-elbow,side+'ForeArm':elbow*(1-hand),side+'Hand':elbow*hand})
    obj=bpy.data.objects['Sunny_v002_Body_UpperArm_'+tag]
    for v in obj.data.vertices:
        setv(obj,v.index,{side+'Arm':1})
# Open eyes / neutral mouth are the saved defaults, not the prototype extrema.
for n in ['Eye_R','Eye_L','Mouth']:
    for k in bpy.data.objects['Sunny_v002_'+n].data.shape_keys.key_blocks:
        if k.name!='Basis':k.value=0
for o in s.objects:o.hide_set(o.type=='ARMATURE')
rig.data.pose_position='REST';s.frame_set(1)
print('Explicit sleeve and elbow transition refinement applied; all body modules visible.')
