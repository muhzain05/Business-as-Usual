"""Run after reopening the saved v004 project, in a separate MCP invocation."""
import bpy,json
from pathlib import Path
ROOT=Path('D:/Business as Usual');TARGET=ROOT/'Assets/Characters/Sunny/Working/Sunny_Locomotion_v004.blend'
assert Path(bpy.data.filepath).resolve()==TARGET.resolve()
win=bpy.context.window_manager.windows[0];checks={}
for clip,N in [('Walk',32),('Run',24)]:
    s=bpy.data.scenes['P2_v004_'+clip+'_Review'];win.scene=s;r=next(o for o in s.objects if o.type=='ARMATURE');samples=[]
    for f in [1,1+N//4,1+N//2]:
        s.frame_set(f);s.view_layers[0].update();samples.append({side:list(r.pose.bones['mixamorig:'+side+'Foot'].head) for side in ['Left','Right']})
    assert len(r.data.bones)==53 and r.animation_data.action
    assert samples[0]!=samples[1]
    shorts=next(o for o in s.objects if o.type=='MESH' and o.name.endswith('Shorts'))
    assert all(t.id==r for fc in shorts.data.shape_keys.animation_data.drivers for v in fc.driver.variables for t in v.targets)
    checks[clip]={'action':r.animation_data.action.name,'range':[s.frame_start,s.frame_end],'bone_count':53,'moving_feet':True,'cuff_drivers_target_review_rig':True};s.frame_set(1)
win.scene=bpy.data.scenes['P2_v004_Locomotion']
report={'opened_saved_file':bpy.data.filepath,'checks':checks,'all_file_images_packed':all(i.packed_file for i in bpy.data.images if i.source=='FILE')}
(ROOT/'Docs/Diagnostics/Phase2/v004/saved-file-reopen-check.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report))
