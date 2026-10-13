"""Prepare a reproducible MCP look capture, or restore its temporary scene state.

BAU_CAPTURE: rest_before, rest_after, modules, head, walk_before, walk_after,
run_before, run_after, or restore. Always call restore after look.
"""
import bpy, json
from pathlib import Path

ROOT=Path(globals().get('BAU_PROJECT_ROOT','D:/Business as Usual'))
VERSION=globals().get('BAU_VERSION','v001')
MODE=globals().get('BAU_CAPTURE','rest_after')
KEY='bau_phase2_capture_restore'
OUT=ROOT/'Docs/Diagnostics/Phase2'/VERSION
inventory=json.loads((OUT/'modular-inventory.json').read_text(encoding='utf-8'))

if MODE=='restore':
    state=bpy.app.driver_namespace.pop(KEY)
    for name,hidden,render in state['visibility']:
        obj=bpy.data.objects.get(name)
        if obj: obj.hide_set(hidden); obj.hide_render=render
    if state['armature']:
        arm=bpy.data.objects[state['armature']]
        arm.animation_data.action=state['action']; arm.animation_data.action_slot=state['slot']
        arm.data.pose_position=state['pose']
    scene=bpy.data.scenes[state['scene']]; scene.frame_set(state['frame'])
    bpy.context.window.scene=bpy.data.scenes[state['previous_scene']]
    print('Temporary capture settings restored.')
else:
    if KEY in bpy.app.driver_namespace: raise RuntimeError('Restore the previous capture before preparing another.')
    if MODE=='rest_before': scene=bpy.data.scenes[inventory['scenes']['before']]
    elif MODE=='modules': scene=bpy.data.scenes[inventory['scenes']['overview']]
    elif MODE.endswith('_before'):
        scene=bpy.data.scenes[inventory['scenes']['references'][MODE.split('_')[0].title()]]
    else: scene=bpy.data.scenes[inventory['scenes']['rest']]
    previous=bpy.context.window.scene
    bpy.context.window.scene=scene
    arm=next((o for o in scene.objects if o.type=='ARMATURE'),None)
    state={'previous_scene':previous.name,'scene':scene.name,'frame':scene.frame_current,
        'visibility':[(o.name,o.hide_get(),o.hide_render) for o in scene.objects],
        'armature':arm.name if arm else None,'action':arm.animation_data.action if arm else None,
        'slot':arm.animation_data.action_slot if arm else None,'pose':arm.data.pose_position if arm else None}
    bpy.app.driver_namespace[KEY]=state
    if MODE=='modules': targets=[o.name for o in scene.objects]
    elif MODE=='head': targets=[f'Sunny_{VERSION}_Head']
    elif MODE.endswith('_before') and MODE!='rest_before':
        targets=[next(o for o in scene.objects if o.type=='MESH' and o.vertex_groups).name]
    else: targets=[o.name for o in scene.objects if o.type=='MESH']
    for obj in scene.objects:
        if obj.name not in targets: obj.hide_set(True); obj.hide_render=True
    if MODE.startswith(('walk','run')):
        label=MODE.split('_')[0].title()
        reference=bpy.data.scenes[inventory['scenes']['references'][label]]
        reference_arm=next(o for o in reference.objects if o.type=='ARMATURE')
        arm.animation_data.action=reference_arm.animation_data.action
        arm.animation_data.action_slot=reference_arm.animation_data.action_slot
        arm.data.pose_position='POSE'; scene.frame_set(20 if label=='Walk' else 9)
    elif arm: arm.data.pose_position='REST'
    bpy.context.view_layer.update()
    settings={'mode':'angles','target':targets,'views':['front','right','back'] if MODE.startswith('rest') else ['front'],
              'shading':'material','distance':5.5 if MODE=='modules' else 1.4 if MODE=='head' else 2.5,
              'max_size':1000 if MODE.startswith('rest') else 1200 if MODE=='modules' else 700}
    (OUT/('capture-'+MODE+'.json')).write_text(json.dumps({'scene':scene.name,'frame':scene.frame_current,
        'look_settings':settings,'restore_required':True},indent=2),encoding='utf-8')
    print(json.dumps(settings))
