"""Temporary skinning comparison; restores frame, pose, and modifier settings."""
import bpy, json
from pathlib import Path

ROOT=Path(globals().get('BAU_PROJECT_ROOT', 'D:/Business as Usual'))
prior=bpy.context.window.scene
report={'topology':{},'algorithm_comparison':{},'motion_channels':{}}
try:
    for label,frame in [('Walk',20),('Run',9)]:
        scene=bpy.data.scenes['BAU_Audit_'+label]; bpy.context.window.scene=scene
        mesh=next(o for o in scene.objects if o.type=='MESH' and o.vertex_groups)
        arm=next(o for o in scene.objects if o.type=='ARMATURE')
        mod=next(m for m in mesh.modifiers if m.type=='ARMATURE')
        old=(scene.frame_current,arm.data.pose_position,mod.use_deform_preserve_volume)
        try:
            aspects=[]
            for p in mesh.data.polygons:
                a,b,c=[mesh.data.vertices[i].co for i in p.vertices]
                L=max((a-b).length,(b-c).length,(c-a).length)
                twice_area=(b-a).cross(c-a).length
                if twice_area>2e-12: aspects.append(L*L/twice_area)
            aspects.sort()
            report['topology'][label]={'longest_edge_to_altitude_p95':aspects[round((len(aspects)-1)*.95)],
                'max':max(aspects),'triangles_over_aspect_10':sum(x>10 for x in aspects)}
            arm.data.pose_position='REST'; bpy.context.view_layer.update()
            ev=mesh.evaluated_get(bpy.context.evaluated_depsgraph_get()); temp=ev.to_mesh()
            rest=[v.co.copy() for v in temp.vertices]; ev.to_mesh_clear()
            arm.data.pose_position='POSE'; scene.frame_set(frame)
            metrics={}
            for preserve in [False,True]:
                mod.use_deform_preserve_volume=preserve; bpy.context.view_layer.update()
                ev=mesh.evaluated_get(bpy.context.evaluated_depsgraph_get()); temp=ev.to_mesh()
                ratios=[]
                for e in mesh.data.edges:
                    i,j=e.vertices; length=(rest[i]-rest[j]).length
                    if length>1e-7: ratios.append((temp.vertices[i].co-temp.vertices[j].co).length/length)
                ratios.sort(); ev.to_mesh_clear()
                metrics['preserve_volume' if preserve else 'linear_blend']={
                    'p99':ratios[round((len(ratios)-1)*.99)],'max':max(ratios),
                    'edges_over_1_5':sum(x>1.5 for x in ratios),'edges_under_0_5':sum(x<.5 for x in ratios)}
            report['algorithm_comparison'][label]={'frame':frame,'metrics':metrics}
            report['motion_channels'][label]=[{'bone':b.name,'location':list(b.location),'scale':list(b.scale)}
                for b in arm.pose.bones if b.location.length>1e-5 or max(abs(x-1) for x in b.scale)>1e-5]
        finally:
            mod.use_deform_preserve_volume=old[2]; arm.data.pose_position=old[1]
            scene.frame_set(old[0]); bpy.context.view_layer.update()
finally:
    bpy.context.window.scene=prior
(ROOT/'Docs/Diagnostics/controlled-checks.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('Controlled checks written; temporary settings restored.')
