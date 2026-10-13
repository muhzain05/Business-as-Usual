"""Use existing clavicle bones for sleeve roots, with gradual upper-arm blending."""
import bpy
j=bpy.data.objects['Sunny_v002_UpperOutfit']
def smooth(t):t=max(0,min(1,t));return t*t*(3-2*t)
def put(i,w):
    for g in j.vertex_groups:g.remove([i])
    for n,v in w.items():
        if v>1e-6:
            g=j.vertex_groups.get('mixamorig:'+n) or j.vertex_groups.new(name='mixamorig:'+n);g.add([i],v,'REPLACE')
for v in j.data.vertices:
    if v.index<490 and .55<v.co.z<.715:
        side='Left' if v.co.x>0 else 'Right';t=smooth((abs(v.co.x)-.11)/.055)*smooth((v.co.z-.55)/.035)
        # Retain spine motion centrally. Clavicles stabilize the sleeve root.
        if t:
            w={'Spine2':1-t,side+'Shoulder':t};put(v.index,w)
for a,b,c,d,side,start in [(9,13,4,7,'Left',490),(35,39,4,7,'Right',546)]:
    loop=[c*49+i for i in range(a,b)]+[k*49+b for k in range(c,d)]+[d*49+i for i in range(b,a,-1)]+[k*49+a for k in range(d,c,-1)]
    for i in loop:put(i,{side+'Shoulder':1})
    for k,t in enumerate([.25,.60,.90,1]):
        for i in range(14):put(start+k*14+i,{side+'Shoulder':1-t,side+'Arm':t})
hips=j.vertex_groups.get('mixamorig:Hips');spine=j.vertex_groups.get('mixamorig:Spine')
if hips:
    for v in j.data.vertices:
        h=next((x.weight for x in v.groups if x.group==hips.index),0)
        if h:
            old=next((x.weight for x in v.groups if x.group==spine.index),0)
            spine.add([v.index],old+h,'REPLACE');hips.remove([v.index])
print('Original clavicle bones now stabilize sleeve roots; no skeleton changes.')
