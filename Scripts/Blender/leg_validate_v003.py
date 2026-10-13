"""Complete integer and quarter-frame lower-body checks; preserve approved upper motion."""
import bpy,json,math,hashlib
from pathlib import Path
from mathutils.bvhtree import BVHTree
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v003'
before=bpy.data.scenes['P2_v003_Before'];after=bpy.data.scenes['P2_v003_LegRepair']
br=next(o for o in before.objects if o.type=='ARMATURE');ar=next(o for o in after.objects if o.type=='ARMATURE')
names=['Shorts','LowerLeg_L','LowerLeg_R','Shoe_L','Shoe_R']
bo={n:next(o for o in before.objects if o.name=='Before_v003_'+n) for n in names}
ao={n:bpy.data.objects['Sunny_v003_'+n] for n in names}
assert len(ar.data.bones)==53
assert max(abs(b.matrix_local[i][j]-br.data.bones[b.name].matrix_local[i][j]) for b in ar.data.bones for i in range(4) for j in range(4))==0
def evaluate(scene,objects):
    scene.view_layers[0].update();deps=bpy.context.evaluated_depsgraph_get();results={};geometry={}
    for name,obj in objects.items():
        # Evaluate skin cage without thickness; masks are already disabled in source and result.
        flags=[(m,m.show_viewport) for m in obj.modifiers if m.type!='ARMATURE']
        for m,state in flags:m.show_viewport=False
        ev=obj.evaluated_get(deps);mesh=ev.to_mesh();ratios=[]
        coords=[ev.matrix_world@v.co for v in mesh.vertices]
        for e in obj.data.edges:
            i,j=e.vertices;L=(obj.data.vertices[i].co-obj.data.vertices[j].co).length
            if L>1e-7:ratios.append((coords[i]-coords[j]).length/L)
        ratios.sort();mesh.calc_loop_triangles();triangles=[tuple(t.vertices) for t in mesh.loop_triangles]
        geometry[name]=(coords,triangles)
        zero=sum(mesh.polygons[p.index].area<1e-10 for p in obj.data.polygons)
        results[name]={'p99_edge_ratio':ratios[round((len(ratios)-1)*.99)],'max_edge_ratio':max(ratios),
                       'min_edge_ratio':min(ratios),'edges_over_1_5':sum(x>1.5 for x in ratios),'zero_area_faces':zero,
                       'min_z':min(v.z for v in coords)}
        ev.to_mesh_clear()
        for m,state in flags:m.show_viewport=state
    # Raw triangle crossings are diagnostic: shoe/ankle overlap below the collar is intentional.
    trees={n:BVHTree.FromPolygons(p,t,all_triangles=True,epsilon=1e-7) for n,(p,t) in geometry.items()}
    for n,tree in trees.items():
        tris=geometry[n][1]
        results[n]['nonadjacent_triangle_crossings']=sum(a<b and not set(tris[a]).intersection(tris[b]) for a,b in tree.overlap(tree))
    intersections={}
    for a,b in [('Shoe_L','Shoe_R'),('LowerLeg_L','LowerLeg_R'),('Shorts','LowerLeg_L'),('Shorts','LowerLeg_R')]:
        intersections[a+'__'+b]=len(trees[a].overlap(trees[b]))
    return results,intersections,geometry
report={'version':'v003','clips':{},'bone_rest_hierarchy_unchanged':True,'bones':53,'height':1.2,
        'method':'Base skin cages. All integer frames, plus quarter-frame sampling. Raw crossing counts include hidden surfaces; inspect rendered views. Topology differs, so strain ratios alone cannot establish visual quality.'}
upper=['Head','Eye_L','Eye_R','Mouth','UpperOutfit','ForearmHand_L','ForearmHand_R','Body_Torso','Button_Top','Button_Middle','Button_Bottom']
for label in ['Walk','Run']:
    ref=next(o for o in bpy.data.scenes['P2_v002_'+label+'_Review'].objects if o.type=='ARMATURE')
    br.animation_data.action=ref.animation_data.action;br.animation_data.action_slot=ref.animation_data.action_slot;br.data.pose_position='POSE'
    ar.animation_data.action=bpy.data.actions[after['bau_'+label+'_action']];ar.animation_data.action_slot=ar.animation_data.action.slots[0];ar.data.pose_position='POSE'
    lo,hi=map(int,ar.animation_data.action.frame_range);result={'integer_frames':[],'quarter_frames':[],'upper_max_world_position_difference':0}
    for f in [lo+i*.25 for i in range((hi-lo)*4+1)]:
        frame=int(f);sub=f-frame;row={'frame':f}
        positions={}
        for scene,rig,objects,tag in [(before,br,bo,'before'),(after,ar,ao,'after')]:
            bpy.context.window.scene=scene;scene.frame_set(frame,subframe=sub)
            metrics,crossing,geo=evaluate(scene,objects)
            row[tag]=metrics;row[tag+'_intersections']=crossing
            row[tag+'_knee_degrees']={side:math.degrees((rig.pose.bones['mixamorig:'+side+'UpLeg'].tail-rig.pose.bones['mixamorig:'+side+'UpLeg'].head).angle(rig.pose.bones['mixamorig:'+side+'Leg'].tail-rig.pose.bones['mixamorig:'+side+'Leg'].head)) for side in ['Left','Right']}
            # Approved upper world-space surfaces must remain identical under the original motion.
            dg=bpy.context.evaluated_depsgraph_get();points=[]
            for name in upper:
                obj=next(o for o in scene.objects if o.name==('Before_v003_' if tag=='before' else 'Sunny_v003_')+name)
                ev=obj.evaluated_get(dg);m=ev.to_mesh();points.extend(ev.matrix_world@v.co for v in m.vertices);ev.to_mesh_clear()
            positions[tag]=points
        difference=max((p-q).length for p,q in zip(positions['before'],positions['after']))
        result['upper_max_world_position_difference']=max(difference,result['upper_max_world_position_difference']);assert difference<1e-5,difference
        (result['integer_frames'] if sub==0 else result['quarter_frames']).append(row)
    result['worst_frame_by_after_strain']=max(result['integer_frames'],key=lambda row:max(row['after'][n]['max_edge_ratio'] for n in ['Shorts','LowerLeg_L','LowerLeg_R']))['frame']
    result['worst_frame_by_before_strain']=max(result['integer_frames'],key=lambda row:max(row['before'][n]['max_edge_ratio'] for n in ['Shorts','LowerLeg_L','LowerLeg_R']))['frame']
    result['summary']={tag:{n:{k:max(row[tag][n][k] for row in result['integer_frames']) for k in ['p99_edge_ratio','max_edge_ratio','edges_over_1_5','zero_area_faces']} for n in names} for tag in ['before','after']}
    result['floor_min_after']=min(row['after'][n]['min_z'] for row in result['integer_frames']+result['quarter_frames'] for n in ['Shoe_L','Shoe_R'])
    result['shoe_collision_frames']=[row['frame'] for row in result['integer_frames']+result['quarter_frames'] if row['after_intersections']['Shoe_L__Shoe_R']]
    result['shorts_skin_crossing_frames']=[row['frame'] for row in result['integer_frames'] if row['after_intersections']['Shorts__LowerLeg_L'] or row['after_intersections']['Shorts__LowerLeg_R']]
    report['clips'][label]=result
ar.data.pose_position='REST';after.frame_set(1);br.data.pose_position='REST';before.frame_set(1);bpy.context.window.scene=after
(OUT/'leg-validation.json').write_text(json.dumps(report,indent=2))
print(json.dumps({'clips':{name:{'summary':v['summary'],'worst_after':v['worst_frame_by_after_strain'],'worst_before':v['worst_frame_by_before_strain'],'shoe_collision_frames':v['shoe_collision_frames'],'shorts_skin_crossing_frames':v['shorts_skin_crossing_frames'],'min_sole_z':v['floor_min_after'],'upper_world_error':v['upper_max_world_position_difference']} for name,v in report['clips'].items()}}))
