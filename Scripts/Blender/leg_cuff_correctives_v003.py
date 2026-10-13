"""Lower-body-only pose correctives for posterior cuff clearance."""
import bpy
s=bpy.data.scenes['P2_v003_LegRepair'];rig=next(o for o in s.objects if o.type=='ARMATURE');obj=bpy.data.objects['Sunny_v003_Shorts']
def smooth(t):t=max(0,min(1,t));return t*t*(3-2*t)
assert not obj.data.shape_keys,'Run once on a fresh working copy.'
obj.shape_key_add(name='Basis')
for side in ['Left','Right']:
    key=obj.shape_key_add(name='RearCuffClearance_'+side);key.value=0
    for v,p in zip(obj.data.vertices,key.data):
        if (v.co.x>0)!=(side=='Left'):continue
        factor=smooth((v.co.y-.024)/.085)*smooth((.348-v.co.z)/.075)
        p.co.y+=.035*factor;p.co.z+=.018*factor
    driver=key.driver_add('value').driver;driver.type='SCRIPTED'
    var=driver.variables.new();var.name='qw';var.type='SINGLE_PROP';var.targets[0].id=rig
    var.targets[0].data_path='pose.bones["mixamorig:'+side+'Leg"].rotation_quaternion[0]'
    driver.expression='min(1,max(0,(2*acos(min(1,abs(qw)))-0.35)/1.1))'
print('Two dynamic rear-cuff clearance keys; upper-body geometry and weights unchanged.')
