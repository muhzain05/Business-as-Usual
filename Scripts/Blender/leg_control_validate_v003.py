"""Controlled-pose strain, raw crossings and non-adjacent self-intersection checks."""
import bpy,json,math
from pathlib import Path
from mathutils.bvhtree import BVHTree
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v003'
# Reuse the same documented pose construction as the screenshots, without rendering.
exec(compile((ROOT/'Scripts/Blender/leg_render_v003.py').read_text().split('prior=bpy.context.window.scene;logs=[]')[0],'pose_helpers','exec'))
OUT=ROOT/'Docs/Diagnostics/Phase2/v003'
prior=bpy.context.window.scene;rows=[]
for label in ['Before','After']:
    scene=scenes[label];bpy.context.window.scene=scene
    rig=next(o for o in scene.objects if o.type=='ARMATURE')
    for pose in ['idle','hip_flexion','knee_bends','foot_lift','forward_step','crouch']:
        scene.frame_set(1);control(rig,pose);bpy.context.view_layer.update()
        geometry={};metrics={}
        for name in ['Shorts','LowerLeg_L','LowerLeg_R','Shoe_L','Shoe_R']:
            obj=next(o for o in scene.objects if o.type=='MESH' and o.name.endswith('_'+name))
            flags=[(m,m.show_viewport) for m in obj.modifiers if m.type!='ARMATURE']
            for m,v in flags:m.show_viewport=False
            bpy.context.view_layer.update();ev=obj.evaluated_get(bpy.context.evaluated_depsgraph_get());mesh=ev.to_mesh();mesh.calc_loop_triangles()
            coords=[ev.matrix_world@v.co for v in mesh.vertices];tris=[tuple(t.vertices) for t in mesh.loop_triangles]
            ratios=[]
            for e in obj.data.edges:
                a,b=e.vertices;l=(obj.data.vertices[a].co-obj.data.vertices[b].co).length
                if l>1e-7:ratios.append((coords[a]-coords[b]).length/l)
            ratios.sort();tree=BVHTree.FromPolygons(coords,tris,all_triangles=True,epsilon=1e-7)
            self_cross=sum(a<b and not set(tris[a]).intersection(tris[b]) for a,b in tree.overlap(tree))
            metrics[name]={'p99_edge_ratio':ratios[round((len(ratios)-1)*.99)],'max_edge_ratio':max(ratios),'min_edge_ratio':min(ratios),'nonadjacent_triangle_crossings':self_cross,'zero_area_faces':sum(p.area<1e-10 for p in mesh.polygons)}
            geometry[name]=tree;ev.to_mesh_clear()
            for m,v in flags:m.show_viewport=v
        rows.append({'version':label,'pose':pose,'metrics':metrics,'shorts_calf_crossings':{n:len(geometry['Shorts'].overlap(geometry[n])) for n in ['LowerLeg_L','LowerLeg_R']},'shoe_pair_crossings':len(geometry['Shoe_L'].overlap(geometry['Shoe_R']))})
bpy.context.window.scene=prior
(OUT/'controlled-pose-validation.json').write_text(json.dumps({'method':'Skin cage; nonadjacent raw triangle crossings are not a visibility test. Compare matching renders.','poses':rows},indent=2))
print(json.dumps({'poses_checked':len(rows),'report':str(OUT/'controlled-pose-validation.json')}))
