"""Read-only full-cycle v002/v003 curve and evaluated-transform comparison."""
import bpy,json,math
from pathlib import Path
from mathutils import Vector
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v004';OUT.mkdir(parents=True,exist_ok=True)
def curves(a):return [c for l in a.layers for s in l.strips if hasattr(s,'channelbags') for b in s.channelbags for c in b.fcurves]
def span(xs):return [min(xs),max(xs),max(xs)-min(xs)]
def corr(a,b):
    aa=[x-sum(a)/len(a) for x in a];bb=[x-sum(b)/len(b) for x in b];d=math.sqrt(sum(x*x for x in aa)*sum(x*x for x in bb));return sum(x*y for x,y in zip(aa,bb))/d if d else 0
prior=bpy.context.window.scene;report={'versions':{},'shoe_attachment':{},'conclusion':'Animation limitation, not a shoe skin-weight failure. v003 flattens global foot orientation, suppresses ankle travel and lift, and limits knee flexion. Source run extreme flexion is unsuitable for the short calf and oversized shoes without adaptation.'}
for version in ['v002','v003']:
    report['versions'][version]={}
    for clip,count in [('Walk',26),('Run',17)]:
        scene=bpy.data.scenes['P2_'+version+'_'+clip+'_Review'];rig=next(o for o in scene.objects if o.type=='ARMATURE');oldframe=scene.frame_current
        bpy.context.window.scene=scene;rows=[]
        for f in range(1,count+1):
            scene.frame_set(f);bpy.context.view_layer.update();row={'frame':f,'hip':list(rig.pose.bones['mixamorig:Hips'].head),'legs':{}}
            for side in ['Left','Right']:
                a,b,c=[rig.pose.bones['mixamorig:'+side+n] for n in ['UpLeg','Leg','Foot']];v=a.tail-a.head;w=b.tail-b.head;q=c.matrix.to_quaternion()
                row['legs'][side]={'ankle':list(c.head),'thigh_pitch':math.degrees(math.atan2(-v.y,-v.z)),'shin_pitch':math.degrees(math.atan2(-w.y,-w.z)),'knee_bend':math.degrees(v.angle(w)),'foot_rest_rotation_delta':math.degrees(q.rotation_difference(c.bone.matrix_local.to_quaternion()).angle),'foot_global_quaternion':list(q)}
            rows.append(row)
        summary={side:{'ankle_'+axis:span([r['legs'][side]['ankle'][i] for r in rows]) for i,axis in enumerate('xyz')} for side in ['Left','Right']}
        for side in ['Left','Right']:
            for key in ['thigh_pitch','shin_pitch','knee_bend','foot_rest_rotation_delta']:summary[side][key]=span([r['legs'][side][key] for r in rows])
        summary['left_right_ankle_y_correlation']=corr([r['legs']['Left']['ankle'][1] for r in rows],[r['legs']['Right']['ankle'][1] for r in rows])
        summary['hips_z']=span([r['hip'][2] for r in rows])
        action=rig.animation_data.action
        report['versions'][version][clip]={'action':action.name,'samples':rows,'summary':summary,'curves':[{'path':c.data_path,'index':c.array_index,'keys':len(c.keyframe_points),'value_range':span([k.co.y for k in c.keyframe_points])} for c in curves(action)]}
        scene.frame_set(oldframe)
scene=bpy.data.scenes['P2_v003_LegRepair']
for side,tag in [('Left','L'),('Right','R')]:
    shoe=next(o for o in scene.objects if o.name=='Sunny_v003_Shoe_'+tag)
    influences={shoe.vertex_groups[g.group].name for v in shoe.data.vertices for g in v.groups if g.weight>1e-6}
    report['shoe_attachment'][side]={'influences':sorted(influences),'all_vertices_weight_one':all(len(v.groups)==1 and abs(v.groups[0].weight-1)<1e-6 for v in shoe.data.vertices)}
    assert influences=={'mixamorig:'+side+'Foot'}
bpy.context.window.scene=prior
(OUT/'animation-diagnosis.json').write_text(json.dumps(report,indent=2))
print(json.dumps({v:{c:d['summary'] for c,d in clips.items()} for v,clips in report['versions'].items()}))
