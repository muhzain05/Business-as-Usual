"""Original Blender modular town art. Execute in Blender via MCP; no generation services."""
import bpy,math,json,random
from pathlib import Path
from mathutils import Vector
random.seed(61)
root=Path(r'D:\Business as Usual')
out=root/'Unity/BusinessAsUsual/Assets/Town/Art/Addendum';out.mkdir(parents=True,exist_ok=True)
working=root/'Unity/BusinessAsUsual/Assets/Town/Working';working.mkdir(parents=True,exist_ok=True)
original_scene=bpy.context.window.scene
scene=bpy.data.scenes.new('Phase6Addendum_EnvironmentKit');bpy.context.window.scene=scene
palette={'Plaster':(.83,.75,.60,1),'Sage':(.28,.48,.37,1),'Teal':(.075,.19,.21,1),'Coral':(.64,.28,.18,1),'Mustard':(.88,.64,.22,1),'Wood':(.42,.25,.12,1),'Glass':(.10,.23,.26,1),'Steel':(.39,.46,.44,1),'Rubber':(.09,.12,.12,1),'Leaf':(.37,.56,.27,1),'LeafLight':(.53,.65,.34,1),'Paper':(.96,.90,.72,1),'Blue':(.29,.48,.52,1),'Bag':(.19,.30,.27,1),'Light':(1,.79,.38,1)}
mats={}
for name,color in palette.items():
    mat=bpy.data.materials.new('Town_'+name);mat.use_nodes=True
    shader=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');shader.inputs['Base Color'].default_value=color;shader.inputs['Roughness'].default_value=.7
    if name=='Glass':shader.inputs['Roughness'].default_value=.24
    if name=='Steel':shader.inputs['Metallic'].default_value=.35
    mats[name]=mat
objects=[];models=[]
def material(obj,name):obj.data.materials.append(mats[name]);objects.append(obj);return obj
def cube(name,pos,size,mat='Plaster',bevel=.035):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.name=name;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        mod=o.modifiers.new('Soft handmade edges','BEVEL');mod.width=min(bevel,min(size)*.3);mod.segments=1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return material(o,mat)
def cylinder(name,pos,radius,depth,mat='Teal',rotation=(0,0,0),vertices=16):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=pos,rotation=rotation);o=bpy.context.object;o.name=name
    bevel=o.modifiers.new('Edge roll','BEVEL');bevel.width=.012;bevel.segments=1;bpy.ops.object.modifier_apply(modifier=bevel.name);return material(o,mat)
def sphere(name,pos,size,mat='Leaf',segments=16,rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,radius=1,location=pos);o=bpy.context.object;o.name=name;o.scale=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    for p in o.data.polygons:p.use_smooth=True
    return material(o,mat)
def torus(name,pos,major,minor,mat='Rubber',rotation=(0,0,0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=major,minor_radius=minor,major_segments=20,minor_segments=8,location=pos,rotation=rotation);o=bpy.context.object;o.name=name;return material(o,mat)
def mesh(name,verts,faces,mat):
    data=bpy.data.meshes.new(name);data.from_pydata(verts,[],faces);data.update();o=bpy.data.objects.new(name,data);scene.collection.objects.link(o);return material(o,mat)
def beam(name,a,b,r=.035,mat='Teal'):
    a,b=Vector(a),Vector(b);o=cylinder(name,(a+b)/2,r,(b-a).length,mat);o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();return o
def finish(name):
    global objects
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();o=bpy.context.object;o.name='TownAsset_'+name;scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),use_selection=True,object_types={'MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=False,apply_unit_scale=True,use_mesh_modifiers=True,path_mode='AUTO')
    o.data.calc_loop_triangles();models.append(dict(name=name,triangles=len(o.data.loop_triangles),materials=[m.name for m in o.data.materials],size=list(o.dimensions)));objects=[];o.hide_set(True);return o

def window(x,y,z,width=1.35,height=1.4,paint='Sage',back=False):
    side=1 if back else -1
    cube('Painted window casing',(x,y,z),(width+.2,.16,height+.2),paint)
    cube('Recessed dark glass',(x,y+side*.10,z),(width,.045,height),'Glass',.015)
    cube('Center mullion',(x,y+side*.14,z),(.05,.03,height),'Paper',.008)
    cube('Cross sash',(x,y+side*.14,z),(width,.03,.05),'Paper',.008)
    cube('Projecting sill',(x,y+side*.11,z-height/2-.07),(width+.35,.34,.13),'Teal',.025)
    # One reflection slat gives the window a readable stylized highlight.
    cube('Glass highlight',(x-width*.27,y+side*.128,z),(.06,.008,height*.68),'Blue',0)
def roof(width,depth,eave,peak,repeat=10):
    w=width/2+.25;d=depth/2+.28
    cube('Finished plaster ceiling',(0,0,eave-.045),(width+.08,depth+.08,.12),'Plaster',.015)
    verts=[(-w,-d,eave),(w,-d,eave),(-w,0,peak),(w,0,peak),(-w,d,eave),(w,d,eave)]
    mesh('Solid terracotta roof',verts,[(0,2,3,1),(2,4,5,3)],'Coral')
    for side in (-1,1):
        for row in range(5):
            y=side*(row+.5)*d/5;z=peak-abs(y)*(peak-eave)/d
            for col in range(repeat):
                o=cube('Overlapping tile',(-w+(col+.5)*2*w/repeat,y,z+.025),(2*w/repeat+.018,d/5+.025,.055),'Coral',.012);o.rotation_euler[0]=side*-math.atan((peak-eave)/d)
        cube('Painted eave',(0,side*d,eave-.035),(width+.7,.18,.17),'Teal',.025)
    for side in (-1,1):cube('Eave closure',(0,side*depth/2,eave+.04),(width+.12,.28,.22),'Teal',.02)
    cube('Ridge coping',(0,0,peak+.06),(width+.6,.19,.17),'Coral',.03)
    for side in (-1,1):
        x=side*width/2
        mesh('Solid filled gable',[(x-.09,-d,eave),(x-.09,d,eave),(x-.09,0,peak),(x+.09,-d,eave),(x+.09,d,eave),(x+.09,0,peak)],[(0,2,1),(3,4,5),(0,1,4,3),(1,2,5,4),(2,0,3,5)],'Plaster')
        for sign in (-1,1):beam('Gable bargeboard',(x,sign*d,eave),(x,0,peak),.048,'Paper')
def shell(width,depth,height,paint):
    for side in (-1,1):
        cube('Side plaster wall',(side*width/2,0,height/2),(.22,depth,height),paint,.035)
        cube('Corner pilaster',(side*(width/2-.09),-depth/2-.08,height/2),(.26,.26,height),'Paper',.025)
    cube('Rear wall',(0,depth/2,height/2),(width,.23,height),paint,.035)
    for side in (-1,1):cube('Weathered baseboard',(side*width/2,0,.17),(.28,depth,.34),'Teal',.025)
    cube('Back baseboard',(0,depth/2,.17),(width,.29,.34),'Teal',.025)
    for x in (-width*.25,width*.25):window(x,depth/2+.12,1.7,1.1,1.1,back=True)
    cylinder('Rain pipe',(width/2+.15,depth/2-.2,1.5),.047,2.95,'Teal',vertices=12)
    cube('Gutter',(0,depth/2+.24,height),(width+.3,.14,.11),'Teal',.02)
# Market: a complete reusable hollow shell with the original functional doorway kept clear.
shell(7,6,3.32,'Plaster')
cube('Market west front',(-2,-3,1.66),(3,.25,3.32),'Plaster');cube('Market east front',(2.2,-3,1.66),(2.6,.25,3.32),'Plaster')
cube('Door lintel',(.2,-3,2.88),(1.6,.25,.88),'Plaster')
for x in (-2.1,2.25):window(x,-3.18,1.6,1.55,1.45)
for x in (-.64,1.04):cube('Door frame',(x,-3.2,1.16),(.1,.16,2.32),'Sage')
cube('Door header',(.2,-3.2,2.36),(1.8,.16,.1),'Sage')
cube('Market sign backing',(0,-3.29,3.12),(6.1,.16,.47),'Teal')
roof(7,6,3.32,4.42,12);finish('MarketShell')
# Garage: broad open frontage exposes the same two vehicles, desks and work points.
shell(7,6,3.39,'Blue')
for side in (-1,1):
    cube('Garage front pier',(side*3.1,-3,1.695),(.8,.32,3.39),'Blue')
    cube('Workshop corner stripe',(side*3.1,-3.2,1.1),(.64,.035,.16),'Mustard',.01)
    cylinder('Open shutter guide',(side*2.65,-3.24,1.35),.045,2.6,'Steel',vertices=12)
cube('Garage lintel',(0,-3,3.0),(6.9,.32,.78),'Blue')
cube('Workshop sign backing',(0,-3.25,3.17),(5.4,.16,.49),'Teal')
for i in range(3):cube('Rolled shutter slat',(0,-3.24,2.56+i*.07),(5.25,.11,.055),'Steel',.008)
roof(7,6,3.39,4.35,10);finish('WorkshopShell')
# Two repeatable houses: doors, shutters, window boxes and porches are modeled into the kit.
for variant in (0,1):
    paint='Plaster' if variant==0 else 'Sage';width=5.6;depth=4.8
    shell(width,depth,3.4,paint)
    cube('House front',(0,-depth/2,1.7),(width,.23,3.4),paint)
    for x in (-1.72,1.72):
        window(x,-2.55,1.95,1.15,1.35,'Teal' if variant else 'Sage')
        for side in (-1,1):
            cube('Shutter',(x+side*.72,-2.61,1.95),(.28,.09,1.53),'Teal' if variant else 'Sage')
            for j in range(5):cube('Shutter louver',(x+side*.72,-2.67,1.5+j*.22),(.25,.018,.04),'Paper',0)
        cube('Window box',(x,-2.77,1.06),(1.2,.4,.23),'Wood');sphere('Box foliage',(x,-2.79,1.22),(.53,.19,.17),'Leaf',12,6)
    cube('Door surround',(0,-2.56,1.2),(1.16,.17,2.5),'Paper');cube('Painted entry door',(0,-2.67,1.14),(.93,.065,2.29),'Coral' if variant else 'Teal')
    cube('Door inset',(0,-2.713,1.65),(.55,.012,.55),'Glass',.015);sphere('Door handle',(.30,-2.76,1.03),(.035,.035,.035),'Mustard',8,4)
    cube('Porch step',(0,-2.97,.12),(1.45,.84,.24),'Plaster');cube('Porch hood',(0,-2.9,2.54),(1.6,.85,.12),'Teal')
    roof(width,depth,3.42,4.46,7)
    if variant:cube('Chimney',(1.6,.8,4.13),(.48,.48,1.15),'Coral');cube('Chimney cap',(1.6,.8,4.71),(.65,.65,.13),'Teal')
    finish('TownHouse'+('A' if variant==0 else 'B'))
# Small social square detail kit: municipal parcel cart, bulletin pole and street bicycle rack.
for x in (-.32,.32):
    cylinder('Cart wheel',(x,.16,.22),.18,.08,'Rubber',(0,math.pi/2,0),12)
cube('Delivery cart bed',(0,0,.40),(.78,.93,.12),'Teal');cube('Mail crate',(0,.02,.72),(.62,.65,.53),'Wood')
for z in (-.26,.28):cube('Parcel straps',(0,z,.994),(.66,.034,.025),'Paper',0)
for x in (-.32,.32):beam('Trolley handle',(x,.41,.4),(x,.54,1.18),.026,'Steel')
beam('Hand grip',(-.32,.54,1.18),(.32,.54,1.18),.027,'Teal');finish('ParcelCart')
manifest=root/'Docs/Diagnostics/Phase6Addendum/environment-models.json';manifest.write_text(json.dumps({'origin':'Original modeled Blender geometry, shared town palette','creditsSpent':0,'models':models},indent=2))
bpy.data.libraries.write(str(working/'DaylightTown_AddendumKit.blend'),{scene},fake_user=True)
bpy.context.window.scene=original_scene
print(json.dumps(models,indent=2));print('Restored original scene: '+original_scene.name)
