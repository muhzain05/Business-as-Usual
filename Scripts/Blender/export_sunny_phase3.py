"""Run in background Blender on v004. Never saves or edits a source blend file."""
import bpy
import json
import hashlib
from pathlib import Path
from mathutils import Matrix

ROOT = Path('D:/Business as Usual')
OUT = ROOT / 'Unity/BusinessAsUsual/Assets/Prototype/Art/Sunny'
DIAG = ROOT / 'Docs/Diagnostics/Phase3'
SOURCE = ROOT / 'Assets/Characters/Sunny/Working/Sunny_Locomotion_v004.blend'
assert Path(bpy.data.filepath).resolve() == SOURCE.resolve()
assert hashlib.sha256(SOURCE.read_bytes()).hexdigest() == '694b527c44d88b09e09453058357e1f2edf997ff3b615e9013a2d08df990a861'
OUT.mkdir(parents=True, exist_ok=True)
DIAG.mkdir(parents=True, exist_ok=True)
for name in ['Sunny_v004.fbx', 'Sunny_Walk_v004.fbx', 'Sunny_Run_v004.fbx']:
    assert not (OUT / name).exists(), 'Refusing to overwrite an export'

source_scene = bpy.data.scenes['P2_v004_Locomotion']
source_rig = next(o for o in source_scene.objects if o.type == 'ARMATURE')
source_modules = [o for o in source_scene.objects if o.type == 'MESH' and not o.hide_render and not o.get('bau_nonproduction', False)]
bpy.context.window.scene = source_scene
materials = sorted({m for o in source_modules for m in o.data.materials if m}, key=lambda m: m.name)
image_paths = {}
material_report = []
for mat in materials:
    node = next(n for n in mat.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
    report = {'name': mat.name, 'color_linear': list(node.inputs['Base Color'].default_value), 'roughness': node.inputs['Roughness'].default_value, 'metallic': node.inputs['Metallic'].default_value, 'textures': {}}
    for tex in [n for n in mat.node_tree.nodes if n.type == 'TEX_IMAGE' and n.image]:
        im = tex.image
        if im.name not in image_paths:
            filename = 'Texture_' + str(len(image_paths)) + '.png'
            # save() writes the packed original pixels without render color management.
            im.filepath_raw = str(OUT / filename)
            im.file_format = 'PNG'
            im.save()
            image_paths[im.name] = filename
        destinations = [l.to_socket.name for l in tex.outputs['Color'].links]
        role = 'base' if 'Base Color' in destinations else ('normal' if any(l.to_node.type == 'NORMAL_MAP' for l in tex.outputs['Color'].links) else 'data')
        report['textures'][role] = image_paths[im.name]
    material_report.append(report)

# Sample driver values on the existing animated rig before making the export copies.
driver_samples = {}
for clip, end in [('Walk', 33), ('Run', 25)]:
    source_rig.data.pose_position = 'POSE'
    source_rig.animation_data.action = bpy.data.actions[source_scene['bau_' + clip + '_action']]
    source_rig.animation_data.action_slot = source_rig.animation_data.action.slots[0]
    driver_samples[clip] = {}
    for f in range(1, end + 1):
        source_scene.frame_set(f)
        source_scene.view_layers[0].update()
        driver_samples[clip][f] = {o.name: {k.name: k.value for k in o.data.shape_keys.key_blocks[1:]} for o in source_modules if o.data.shape_keys}
source_rig.animation_data.action = None
source_rig.data.pose_position = 'REST'
for b in source_rig.pose.bones:
    b.matrix_basis = Matrix.Identity(4)
source_scene.frame_set(1)

export_scene = bpy.data.scenes.new('Phase3_Export_Only')
export_scene.render.fps = 24
export_scene.unit_settings.system = 'METRIC'
export_scene.unit_settings.scale_length = 1
rig = source_rig.copy()
rig.data = source_rig.data.copy()
rig.name = 'SunnyRig'
rig.animation_data_clear()
export_scene.collection.objects.link(rig)
rig.data.pose_position = 'POSE'
copied_modules = []
for src in source_modules:
    # Evaluate subdivision/thickness in bind pose, retaining deform weights and UVs.
    arm_mods = [m for m in src.modifiers if m.type == 'ARMATURE']
    for m in arm_mods:
        m.show_viewport = False
    keys = src.data.shape_keys
    if keys and keys.animation_data:
        for d in keys.animation_data.drivers:
            d.mute = True
    if keys:
        for k in keys.key_blocks[1:]:
            k.value = 0
    source_scene.view_layers[0].update()
    deps = bpy.context.evaluated_depsgraph_get()
    mesh = bpy.data.meshes.new_from_object(src.evaluated_get(deps), preserve_all_data_layers=True, depsgraph=deps)
    obj = src.copy()
    obj.name = src.name.removeprefix('Sunny_v004_')
    obj.data = mesh
    obj.animation_data_clear()
    obj.modifiers.clear()
    obj.parent = rig
    obj.matrix_parent_inverse = src.matrix_parent_inverse.copy()
    export_scene.collection.objects.link(obj)
    if keys:
        obj.shape_key_add(name='Basis')
        for key in keys.key_blocks[1:]:
            key.value = 1
            source_scene.view_layers[0].update()
            deps = bpy.context.evaluated_depsgraph_get()
            evaluated = bpy.data.meshes.new_from_object(src.evaluated_get(deps), preserve_all_data_layers=True, depsgraph=deps)
            assert len(evaluated.vertices) == len(mesh.vertices)
            copied = obj.shape_key_add(name=key.name)
            for a, b in zip(copied.data, evaluated.vertices):
                a.co = b.co
            key.value = 0
            bpy.data.meshes.remove(evaluated)
    skin = obj.modifiers.new('Shared skeleton', 'ARMATURE')
    skin.object = rig
    skin.use_deform_preserve_volume = False  # Unity uses linear skinning.
    copied_modules.append((src, obj))
    for m in arm_mods:
        m.show_viewport = True

bpy.context.window.scene = export_scene
for obj in export_scene.objects:
    obj.hide_set(False)
    obj.select_set(True)
bpy.context.view_layer.objects.active = rig
exports = []
for clip, end, filename in [('Rest', 2, 'Sunny_v004.fbx'), ('Walk', 33, 'Sunny_Walk_v004.fbx'), ('Run', 25, 'Sunny_Run_v004.fbx')]:
    rig.animation_data_create()
    rig.animation_data.action = None if clip == 'Rest' else bpy.data.actions[source_scene['bau_' + clip + '_action']]
    if rig.animation_data.action:
        rig.animation_data.action_slot = rig.animation_data.action.slots[0]
    for b in rig.pose.bones:
        b.matrix_basis = Matrix.Identity(4)
    for src, obj in copied_modules:
        keys = obj.data.shape_keys
        if not keys:
            continue
        keys.animation_data_clear()
        for k in keys.key_blocks[1:]:
            for f in range(1, end + 1):
                k.value = 0 if clip == 'Rest' else driver_samples[clip][f][src.name][k.name]
                k.keyframe_insert('value', frame=f)
    export_scene.frame_start = 1
    export_scene.frame_end = end
    export_scene.frame_set(1)
    bpy.ops.export_scene.fbx(filepath=str(OUT / filename), use_selection=True, object_types={'ARMATURE', 'MESH'}, use_mesh_modifiers=False, add_leaf_bones=False, use_armature_deform_only=False, axis_forward='-Z', axis_up='Y', apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_space_transform=False, bake_anim=True, bake_anim_use_all_actions=False, bake_anim_use_nla_strips=False, bake_anim_use_all_bones=True, bake_anim_force_startend_keying=True, bake_anim_step=1, bake_anim_simplify_factor=0, path_mode='RELATIVE', embed_textures=False)
    exports.append({'file':filename,'clip':clip,'frames':[1,end],'sha256':hashlib.sha256((OUT/filename).read_bytes()).hexdigest(),'bytes':(OUT/filename).stat().st_size})
report = {'source':str(SOURCE.relative_to(ROOT)), 'source_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(), 'skeleton_bones':len(rig.data.bones), 'module_count':len(copied_modules), 'modules':[{'name':o.name,'vertices':len(o.data.vertices),'weighted_vertices':sum(bool(v.groups) for v in o.data.vertices),'blendshapes':[k.name for k in o.data.shape_keys.key_blocks] if o.data.shape_keys else []} for _,o in copied_modules], 'fps':24,'height':1.2,'exports':exports,'materials':material_report,'notes':['Subdivision/thickness evaluated in bind pose on export copies.','Existing cuff drivers sampled into blendshape curves.','Generic exact skeleton; no retargeting or source saves.','Unity linear skinning differs from Blender preserve-volume skinning.']}
(OUT/'material-manifest.json').write_text(json.dumps(material_report,indent=2),encoding='utf-8')
(DIAG/'sunny-export.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
