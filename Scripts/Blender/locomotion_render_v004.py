"""Matching full-character previews with normalized-cycle v003 synchronization."""
import bpy,math,json
from pathlib import Path
from mathutils import Vector
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v004/Images';OUT.mkdir(parents=True,exist_ok=True)
def setup(label,source):
    name='P2_v004_Render_'+label
    if name in bpy.data.scenes:return bpy.data.scenes[name]
    s=bpy.data.scenes.new(name);s.render.engine=source.render.engine;s.render.resolution_x=480;s.render.resolution_y=640;s.render.resolution_percentage=100;s.render.fps=24;s.render.image_settings.file_format='PNG'
    s.world=bpy.data.worlds.new(name+'_World');s.world.use_nodes=True;bg=next(n for n in s.world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs[0].default_value=(.07,.07,.07,1);bg.inputs[1].default_value=.6
    copies={}
    for old in source.objects:
        if old.get('bau_nonproduction'):continue
        obj=old.copy();obj.name=name+'_'+old.name
        if old.type=='ARMATURE' or old.name.endswith('Shorts'):obj.data=old.data.copy()
        s.collection.objects.link(obj);copies[old]=obj;obj.hide_render=False
    for old,obj in copies.items():
        obj.parent=copies.get(old.parent)
        for m in obj.modifiers:
            if m.type=='ARMATURE':m.object=copies.get(m.object,m.object)
        if obj.type=='MESH' and obj.data.shape_keys and obj.data.shape_keys.animation_data:
            for fc in obj.data.shape_keys.animation_data.drivers:
                for v in fc.driver.variables:
                    for t in v.targets:
                        if t.id in copies:t.id=copies[t.id]
    data=bpy.data.cameras.new(name+'_Camera');cam=bpy.data.objects.new(data.name,data);s.collection.objects.link(cam);data.type='ORTHO';data.ortho_scale=1.45;s.camera=cam
    for i,(pos,power,size) in enumerate([((-2,-3,3),180,3),((2,-1,2),100,3),((0,3,2),150,2)]):
        d=bpy.data.lights.new(name+'_Light'+str(i),'AREA');d.energy=power;d.shape='DISK';d.size=size;o=bpy.data.objects.new(d.name,d);s.collection.objects.link(o);o.location=pos;o.rotation_euler=(Vector((0,0,.6))-o.location).to_track_quat('-Z','Y').to_euler()
    # A shared stationary grid makes sliding and ground contact visible.
    mesh=bpy.data.meshes.new(name+'_Floor');mesh.from_pydata([(-2,-2,-.0004),(2,-2,-.0004),(2,2,-.0004),(-2,2,-.0004)],[],[(0,1,2,3)]);mesh.update();o=bpy.data.objects.new(mesh.name,mesh);s.collection.objects.link(o)
    mat=bpy.data.materials.new(name+'_FloorMaterial');mat.use_nodes=True;nodes=mat.node_tree.nodes;bs=next(n for n in nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Roughness'].default_value=.85
    checker=nodes.new('ShaderNodeTexChecker');checker.inputs['Color1'].default_value=(.09,.10,.11,1);checker.inputs['Color2'].default_value=(.13,.14,.15,1);checker.inputs['Scale'].default_value=28;tex=nodes.new('ShaderNodeTexCoord');mat.node_tree.links.new(tex.outputs['Generated'],checker.inputs['Vector']);mat.node_tree.links.new(checker.outputs['Color'],bs.inputs['Base Color']);mesh.materials.append(mat)
    return s
scenes={label:setup(label,bpy.data.scenes[name]) for label,name in [('v003','P2_v004_Baseline'),('v004','P2_v004_Locomotion')]}
prior=bpy.context.window.scene
for label,clip,view,f in globals().get('BAU_V004_RENDER_JOBS',[]):
    s=scenes[label];bpy.context.window.scene=s;r=next(o for o in s.objects if o.type=='ARMATURE');N=32 if clip=='Walk' else 24
    if label=='v004':action=bpy.data.actions[bpy.data.scenes['P2_v004_Locomotion']['bau_'+clip+'_action']];sample=f
    else:
        original=next(o for o in bpy.data.scenes['P2_v003_'+clip+'_Review'].objects if o.type=='ARMATURE');action=original.animation_data.action;lo,hi=action.frame_range;sample=lo+(f-1)/N*(hi-lo)
    r.animation_data.action=action;r.animation_data.action_slot=action.slots[0];r.data.pose_position='POSE';s.frame_set(int(sample),subframe=sample-int(sample))
    target=Vector((0,0,.61));d={'front':Vector((0,-3,.15)),'side':Vector((3,0,.2)),'three_quarter':Vector((3,-3,.5))}[view];s.camera.location=target+d;s.camera.rotation_euler=(target-s.camera.location).to_track_quat('-Z','Y').to_euler()
    stamp=f'{int(f):03}' if f==int(f) else f'{f:06.2f}'
    s.render.filepath=str(OUT/f'{label}-{clip.lower()}-{view}-{stamp}.png');bpy.ops.render.render(write_still=True)
bpy.context.window.scene=prior
print(json.dumps({'renders':len(globals().get('BAU_V004_RENDER_JOBS',[]))}))
