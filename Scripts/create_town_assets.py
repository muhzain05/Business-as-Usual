"""Original Blender modular town art. Execute in Blender via MCP; no generation services."""
import bpy,math,json,random
from pathlib import Path
from mathutils import Vector
random.seed(61)
root=Path(r'D:\Business as Usual')
out=root/'Unity/BusinessAsUsual/Assets/Town/Art';out.mkdir(parents=True,exist_ok=True)
working=root/'Unity/BusinessAsUsual/Assets/Town/Working';working.mkdir(parents=True,exist_ok=True)
original_scene=bpy.context.window.scene
scene=bpy.data.scenes.new('Phase6_OriginalTownAssetLibrary');bpy.context.window.scene=scene
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
        mod=o.modifiers.new('Soft handmade edges','BEVEL');mod.width=min(bevel,min(size)*.3);mod.segments=3
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return material(o,mat)
def cylinder(name,pos,radius,depth,mat='Teal',rotation=(0,0,0),vertices=16):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=pos,rotation=rotation);o=bpy.context.object;o.name=name
    bevel=o.modifiers.new('Edge roll','BEVEL');bevel.width=.012;bevel.segments=2;bpy.ops.object.modifier_apply(modifier=bevel.name);return material(o,mat)
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
# Soft module meshes rather than raw Unity primitives.
cube('Wall block',(0,0,0),(1,1,1),bevel=.06);finish('WallPanel')
cube('Curb block',(0,0,.05),(1,.3,.1),'Plaster',.025);finish('Curb')
# Layered gable roof with individually rounded terracotta tiles.
verts=[(-3.8,-3.5,0),(3.8,-3.5,0),(-3.8,0,1.18),(3.8,0,1.18),(-3.8,3.5,0),(3.8,3.5,0)]
mesh('Roof underlay',verts,[(0,1,3,2),(2,3,5,4)],'Wood')
for side in (-1,1):
    for row in range(8):
        y=side*(row+.5)*.43;z=1.18-abs(y)*1.18/3.5
        for col in range(16):
            o=cube('Roof tile',(-3.56+col*.475+(row%2)*.045,y,z+.018),(.49,.49,.065),'Coral',.025);o.rotation_euler[0]=side*-math.atan(1.18/3.5)
    cube('Eave trim',(0,side*3.55,-.02),(7.8,.15,.18),'Teal')
for col in range(17):cylinder('Ridge cap',(-3.7+col*.45,0,1.23),.095,.46,'Coral',(0,math.pi/2,0),12)
finish('Roof')
# Shopfront: canonical doorway matches original graybox gap.
cube('West plaster',(-2,0,1.5),(3, .28,3),'Plaster',.06);cube('East plaster',(2.2,0,1.5),(2.6,.28,3),'Plaster',.06);cube('Lintel',(.2,0,2.7),(1.6,.28,.6),'Plaster')
for x in (-2.2,2.25):
    cube('Window painted surround',(x,-.19,1.6),(1.65,.12,1.55),'Sage');cube('Window glass',(x,-.265,1.63),(1.43,.035,1.27),'Glass',.04)
    cube('Window mullion',(x,-.31,1.63),(.05,.035,1.27),'Paper');cube('Window cross rail',(x,-.31,1.65),(1.43,.035,.05),'Paper');cube('Window sill',(x,-.29,.85),(1.8,.3,.10),'Teal')
for x in (-.6,1):cube('Door casing',(x,-.18,1.12),(.16,.16,2.24),'Sage')
cube('Door header',(.2,-.18,2.32),(1.8,.16,.17),'Sage');cube('Sign plaque',(0,-.32,3.07),(6.3,.19,.6),'Teal',.08)
for x in (-3.25,3.25):cube('Brick column',(x,-.19,.45),(.25,.12,.85),'Coral')
finish('Shopfront')
# Awning repeats scalloped canvas strips; compact canopy over storefront.
for i in range(18):
    x=-3.4+i*.4;mat='Sage' if i%2==0 else 'Paper';o=cube('Canvas canopy',(x,-.48,0),(.40,1.2,.08),mat);o.rotation_euler[0]=.16
    cylinder('Scalloped valance',(x,-1.08,-.18),.2,.075,mat,(math.pi/2,0,0),16)
for x in (-3.5,3.5):beam('Awning support',(x,0,-.48),(x,-1.1,-.05),.025,'Teal')
finish('Awning')
# Garage entrance trim with a smooth curved lintel and ribbed shutter header.
for x in (-2.85,2.85):cube('Workshop pier',(x,0,1.5),(1.45,.32,3),'Blue',.10)
for x in (-2.15,2.15):cube('Rounded garage jamb',(x,-.18,1.2),(.23,.22,2.4),'Paper',.09)
verts=[]
for j in range(17):
    angle=math.pi*j/16;x=2.15*math.cos(angle);z=2.17+.70*math.sin(angle)
    for depth in (-.32,-.1):
        verts.extend([(x,depth,z),(x,depth,z+.2)])
faces=[]
for j in range(16):
    a=j*4;b=(j+1)*4;faces.extend([(a,b,b+1,a+1),(a+2,a+3,b+3,b+2),(a,a+2,b+2,b),(a+1,b+1,b+3,a+3)])
mesh('Curved workshop header',verts,faces,'Paper')
cube('Garage sign',(0,-.20,3.15),(5.7,.19,.65),'Mustard',.08)
for i in range(14):cube('Shutter slat',(0,.02,2.4+i*.046),(4.1,.09,.034),'Steel',.008)
finish('GarageFront')
# Original utility van, no driving/physics.
cube('Van body',(0,0,.84),(1.36,2.55,1.1),'Blue',.19);cube('Cab roof',(0,-.50,1.56),(1.32,1.40,.40),'Blue',.15)
cube('Cargo roof',(0,.76,1.47),(1.32,1.2,.57),'Paper',.13);cube('Windshield',(0,-1.26,1.48),(1.08,.04,.49),'Glass',.06)
for side in (-1,1):
    cube('Side window',(side*.692,-.55,1.46),(.035,.72,.4),'Glass',.055);cube('Door handle',(side*.725,-.28,1.06),(.035,.15,.055),'Steel')
    for y in (-.80,.86):
        torus('Rubber tire',(side*.67,y,.37),.245,.085,'Rubber',(0,math.pi/2,0));cylinder('Wheel hub',(side*.756,y,.37),.17,.08,'Paper',(0,math.pi/2,0),16)
    cube('Van mirror',(side*.82,-.92,1.37),(.13,.11,.15),'Teal')
cube('Front bumper',(0,-1.35,.52),(1.36,.12,.17),'Steel');cube('Rear bumper',(0,1.35,.52),(1.36,.12,.17),'Steel')
for x in (-.46,.46):cylinder('Headlamp',(x,-1.3,.94),.12,.06,'Light',(math.pi/2,0,0))
for x in (-.22,-.08,.08,.22):cube('Front grille',(x,-1.335,.82),(.06,.04,.15),'Teal')
cube('License plate',(0,-1.416,.54),(.38,.035,.13),'Paper');finish('ServiceVan')
# Bag with folds and a knotted neck, low polygon budget.
o=sphere('Waste sack',(0,0,.02),(.25,.24,.28),'Bag',16,10)
for v in o.data.vertices:
    angle=math.atan2(v.co.y,v.co.x);v.co.x*=1+.07*math.sin(angle*5+v.co.z*13);v.co.y*=1+.05*math.cos(angle*7)
cylinder('Tied neck',(0,0,.31),.065,.16,'Bag',vertices=10);torus('Bag tie',(0,0,.31),.067,.012,'Mustard');cube('Collection ticket',(.02,-.24,.1),(.14,.018,.16),'Paper',.01);finish('WasteBag')
cube('Bin body',(0,0,.55),(.85,.78,1.05),'Sage',.10);cube('Bin lid',(0,0,1.12),(.98,.88,.11),'Teal',.05);cube('Bin badge',(0,-.425,.76),(.45,.025,.28),'Paper',.02)
for x in (-.36,.36):cylinder('Bin wheel',(x,.24,.13),.12,.055,'Rubber',(0,math.pi/2,0),12)
beam('Bin handle',(-.30,.38,1.02),(.30,.38,1.02),.035,'Steel');finish('WasteBin')
# Street furniture.
for i in range(5):cube('Seat slat',(0,-.25+i*.12,.46),(1.8,.09,.09),'Wood',.025)
for i in range(4):cube('Back slat',(0,.36,.65+i*.13),(1.8,.075,.09),'Wood',.025)
for x in (-.68,.68):
    for y in (-.2,.27):cube('Bench foot',(x,y,.21),(.10,.13,.42),'Teal')
    beam('Back upright',(x,.28,.4),(x,.40,1.12),.035,'Teal')
finish('Bench')
cylinder('Planter pot',(0,0,.28),.44,.56,'Coral',vertices=20);torus('Planter lip',(0,0,.54),.43,.04,'Plaster')
for i in range(12):
    angle=i*2.399;sphere('Shrub leaf',(math.cos(angle)*.27,math.sin(angle)*.27,.6+random.random()*.32),(.21,.21,.24),'LeafLight' if i%3==0 else 'Leaf',12,6)
finish('Planter')
cylinder('Tree trunk',(0,0,1.05),.13,2.1,'Wood',vertices=12)
for i in range(9):
    angle=i*2.399;r=.52 if i else 0;sphere('Tree crown',(math.cos(angle)*r,math.sin(angle)*r,2.35+(i%3)*.25),(.7,.65,.62),'LeafLight' if i%3==0 else 'Leaf',12,6)
finish('Tree')
# Clock landmark, clock face is modeled rather than a generic cube.
cylinder('Clock footing',(0,0,.09),.48,.18,'Plaster');cylinder('Clock stem',(0,0,1.34),.095,2.55,'Teal')
cylinder('Clock rim',(0,0,2.8),.53,.16,'Mustard',(math.pi/2,0,0),40);cylinder('Clock face',(0,-.10,2.8),.47,.03,'Paper',(math.pi/2,0,0),40)
for i in range(12):
    angle=i*math.pi/6;cube('Hour marker',(math.sin(angle)*.39,-.13,2.8+math.cos(angle)*.39),(.035,.015,.07),'Teal',.004)
beam('Clock minute',(0,-.16,2.8),(.07,-.16,3.12),.016,'Teal');beam('Clock hour',(0,-.17,2.8),(-.23,-.17,2.91),.024,'Teal');sphere('Clock finial',(0,0,3.42),(.09,.09,.09),'Coral');finish('Clock')
for x in (-.65,.65):cube('Board leg',(x,0,.95),(.12,.13,1.9),'Wood')
cube('Noticeboard',(0,0,1.48),(1.7,.16,1.16),'Teal',.06);cube('Board cork',(0,-.095,1.48),(1.48,.025,.95),'Wood',.015)
for i in range(5):
    x=-.53+(i%3)*.52;z=1.26+(i//3)*.47;o=cube('Pinned notice',(x,-.12,z),(.37,.02,.37),'Paper',.005);o.rotation_euler[1]=(i-2)*.035
    sphere('Notice pin',(x,-.14,z+.14),(.024,.014,.024),'Coral',8,4)
cube('Board canopy',(0,0,2.16),(1.95,.55,.17),'Coral',.06);finish('Noticeboard')
# Workshop furniture / pegboard / low-budget hand tools.
cube('Workbench top',(0,0,.9),(2.5,.78,.14),'Wood',.045);cube('Workbench cabinet',(-.72,0,.45),(.9,.65,.82),'Teal',.045)
for x in (.62,1.1):cube('Workbench leg',(x,0,.45),(.09,.65,.84),'Steel')
for i in range(3):cube('Cabinet drawer',(-.72,-.34,.27+i*.25),(.78,.04,.20),'Sage');cube('Drawer pull',(-.72,-.39,.3+i*.25),(.23,.035,.04),'Paper')
cube('Vice base',(.75,0,1.03),(.28,.28,.16),'Steel');cube('Vice jaw',(.75,-.02,1.17),(.30,.10,.15),'Teal');finish('Workbench')
cube('Tool pegboard',(0,0,0),(2.6,.09,1.15),'Mustard',.04)
for x in range(15):
    for z in range(5):cylinder('Pegboard hole',(-1.15+x*.165,-.051,-.43+z*.205),.015,.014,'Teal',(math.pi/2,0,0),6)
for i in range(6):
    x=-1.05+i*.40;beam('Tool handle',(x,-.13,-.3),(x,-.13,.22),.032,'Steel' if i%2==0 else 'Coral');torus('Wrench head',(x,-.13,.26),.075,.022,'Steel',(math.pi/2,0,0))
finish('ToolRack')
for x in (-.36,.36):
    for y in (-.35,.35):cube('Cart frame',(x,y,.5),(.065,.065,.7),'Teal');cylinder('Cart wheel',(x,y,.12),.10,.07,'Rubber',(0,math.pi/2,0),12)
cube('Cart platform',(0,0,.24),(.9,.9,.10),'Wood');beam('Cart handle',(-.4,.45,.95),(.4,.45,.95),.04,'Steel');finish('WasteCart')
for i in range(4):cube('Crate slat',(0,-.35,.08+i*.12),(.75,.06,.08),'Wood');cube('Crate slat',(0,.35,.08+i*.12),(.75,.06,.08),'Wood')
for x in (-.34,.34):
    for i in range(4):cube('Crate side',(x,0,.08+i*.12),(.06,.7,.08),'Wood')
cube('Crate floor',(0,0,.02),(.72,.70,.05),'Wood');finish('Crate')
cylinder('Lamp stem',(0,0,1.25),.04,2.5,'Teal');cylinder('Lamp foot',(0,0,.1),.16,.20,'Teal');beam('Lamp crook',(0,0,2.45),(0,-.35,2.65),.04,'Teal');sphere('Lamp hood',(0,-.4,2.55),(.24,.24,.12),'Teal');cylinder('Lamp glow',(0,-.4,2.47),.18,.035,'Light');finish('Lamp')
for x in (-.9,.9):cube('Fence post',(x,0,.65),(.10,.10,1.3),'Wood')
for i in range(10):cube('Fence picket',(-.81+i*.18,0,.55),(.13,.07,1.1),'Sage',.025)
for z in (.22,.80):cube('Fence rail',(0,.07,z),(1.9,.08,.08),'Wood');
finish('Fence')
cube('Window frame',(0,0,0),(1.5,.12,1.3),'Sage');cube('Glass',(0,-.09,0),(1.30,.04,1.10),'Glass',.03);cube('Mullion',(0,-.13,0),(.045,.035,1.1),'Paper');cube('Sash',(0,-.13,0),(1.30,.035,.045),'Paper');finish('Window')
cube('Drainpipe',(0,0,1.3),(.065,.08,2.6),'Steel',.025)
for z in (.45,1.25,2.1):torus('Pipe bracket',(0,0,z),.07,.012,'Teal')
finish('Drainpipe')
cube('Aircon case',(0,0,0),(.9,.30,.65),'Paper',.04)
for i in range(9):cube('Aircon vent',(0,-.17,-.24+i*.055),(.67,.03,.021),'Steel',.003)
finish('Aircon')
cube('Door frame',(0,0,0),(1.4,.11,2.2),'Sage',.04);cube('Door glass',(0,-.08,.23),(1.18,.035,1.50),'Glass');cube('Door kick',(0,-.105,-.81),(1.18,.04,.30),'Teal');cylinder('Door handle',(.48,-.17,-.27),.035,.34,'Mustard',vertices=12);finish('Door')
# Save only this isolated asset scene; original character file stays unchanged.
bpy.data.libraries.write(str(working/'DaylightTown_AssetLibrary.blend'),{scene},fake_user=True)
(out/'model-inventory.json').write_text(json.dumps(models,indent=2))
(out/'palette.json').write_text(json.dumps(palette,indent=2))
for i,o in enumerate([o for o in scene.objects if o.type=='MESH']):o.hide_set(False);o.location=(i%6*9,i//6*9,0)
globals()['BAU_PHASE6_ORIGINAL_SCENE']=original_scene.name
print(json.dumps(dict(models=len(models),triangles=sum(m['triangles'] for m in models),originalScene=original_scene.name,assetScene=scene.name)))
