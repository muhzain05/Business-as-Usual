"""Reusable original shop meshes in an isolated Blender scene; export Unity FBX kit."""
import bpy, math, json
from pathlib import Path
from mathutils import Vector
ROOT=Path('D:/Business as Usual')
OUT=ROOT/'Unity/BusinessAsUsual/Assets/Shopkeeper/Art'
OUT.mkdir(parents=True,exist_ok=True)
old_scene=bpy.context.window.scene
scene=bpy.data.scenes.get('BAU_Shop_AssetLibrary') or bpy.data.scenes.new('BAU_Shop_AssetLibrary')
bpy.context.window.scene=scene
for obj in list(scene.objects):
    bpy.data.objects.remove(obj,do_unlink=True)
materials={}
def material(name,color,texture=None):
    m=bpy.data.materials.get('Shop_'+name) or bpy.data.materials.new('Shop_'+name);m.use_nodes=True
    p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=.45
    if texture:
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=bpy.data.images.load(str(ROOT/'Unity/BusinessAsUsual/Assets/Shopkeeper/Textures'/texture),check_existing=True);m.node_tree.links.new(t.outputs['Color'],p.inputs['Base Color'])
    materials[name]=m;return m
for name,c in [('Cream',(.87,.82,.67)),('Terracotta',(.66,.23,.13)),('Teal',(.08,.32,.3)),('Metal',(.38,.43,.42)),('Dark',(.055,.07,.06)),('Screen',(.07,.31,.2)),('Paper',(.97,.92,.78)),('Blue',(.17,.32,.51)),('Glass',(.25,.46,.42)),('Cardboard',(.53,.33,.16))]:material(name,c)
brands=['Sunbeam','Meadow','Golden','Cloud','Orchard','Tide','LittleFox','Garden']
for brand in brands:material(brand,(1,1,1),brand+'.png')
parts=[];stats=[]
def cube(name,pos,size,mat='Cream',bevel=.025):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.name=name;o.scale=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.data.materials.append(materials[mat])
    if bevel:
        m=o.modifiers.new('Soft manufactured edges','BEVEL');m.width=bevel;m.segments=3
        bpy.ops.object.modifier_apply(modifier=m.name)
    parts.append(o);return o
def cyl(name,pos,radius,height,mat='Metal',verts=24):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=radius,depth=height,location=pos);o=bpy.context.object;o.name=name;o.data.materials.append(materials[mat]);parts.append(o);return o
def lathe(name,rings,mat):
    verts=[];faces=[];N=24
    for z,r in rings:
        for i in range(N):a=2*math.pi*i/N;verts.append((r*math.cos(a),r*math.sin(a),z))
    for j in range(len(rings)-1):
        for i in range(N):faces.append((j*N+i,j*N+(i+1)%N,(j+1)*N+(i+1)%N,(j+1)*N+i))
    faces.extend([tuple(reversed(range(N))),tuple((len(rings)-1)*N+i for i in range(N))]);mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);scene.collection.objects.link(o);mesh.materials.append(materials[mat]);parts.append(o)
    uv=mesh.uv_layers.new(name='Label UV')
    for poly in mesh.polygons:
        for loop in poly.loop_indices:
            v=mesh.vertices[mesh.loops[loop].vertex_index].co
            uv.data[loop].uv=(.5+math.atan2(v.y,v.x)/(2*math.pi),(v.z-min(z for z,r in rings))/(max(z for z,r in rings)-min(z for z,r in rings)))
    # Explicit wrap seam prevents stretched labels on the last radial face.
    for poly in mesh.polygons[:(len(rings)-1)*N]:
        us=[uv.data[l].uv.x for l in poly.loop_indices]
        if max(us)-min(us)>.5:
            for l in poly.loop_indices:
                if uv.data[l].uv.x<.5:uv.data[l].uv.x+=1
    return o
def export(name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts:o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
    scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False,path_mode='STRIP')
    stats.append({'name':name,'vertices':len(o.data.vertices),'triangles':sum(len(p.vertices)-2 for p in o.data.polygons),'materials':[m.name for m in o.data.materials]});parts.clear();o.hide_set(True)
# Gondola: beveled steel uprights, punched details, thin shelves and price rails.
cube('plinth',(0,0,.1),(1.8,.72,.2),'Terracotta',.055)
for x in [-.85,.85]:
    cube('upright',(x,.28,.91),(.055,.075,1.65),'Cream',.01)
    for z in [.38,.52,.66,.8,.94,1.08,1.22,1.36,1.5]:cube('upright slots',(x,.237,z),(.016,.007,.027),'Dark',.002)
cube('back panel',(0,.33,.86),(1.68,.035,1.36),'Cream',.012)
for z in [.27,.62,.97,1.32]:
    cube('shelf deck',(0,0,z),(1.76,.66,.035),'Cream',.012);cube('front rail',(0,-.33,z+.01),(1.76,.025,.07),'Terracotta',.007)
    for x in [-.62,-.2,.2,.62]:cube('price card',(x,-.347,z+.008),(.19,.012,.062),'Paper',.002)
cube('header',(0,.27,1.65),(1.8,.1,.2),'Teal',.035)
export('Shelf')
# Open illuminated refrigerator: ribbed shelves and rounded housings, bottles visible.
cube('fridge back',(0,.31,1.05),(1.8,.14,2.1),'Cream',.06)
for x in [-.88,.88]:cube('insulated sides',(x,0,1.05),(.11,.75,2.1),'Cream',.04)
cube('compressor base',(0,0,.12),(1.8,.78,.24),'Terracotta',.04)
cube('light box',(0,0,2.05),(1.8,.8,.2),'Teal',.04)
cube('light strip',(0,-.33,1.93),(1.6,.055,.04),'Paper',.01)
for z in [.32,.72,1.12,1.52]:
    cube('cold shelf',(0,0,z),(1.6,.65,.025),'Cream',.006)
    for x in [i*.08-.72 for i in range(19)]:cube('shelf wire',(x,0,z+.018),(.009,.6,.012),'Metal',.002)
    cube('price rail',(0,-.34,z+.025),(1.62,.022,.065),'Terracotta',.006)
for x in [i*.12-.7 for i in range(13)]:cube('vent',(x,-.397,.13),(.055,.008,.04),'Dark',.004)
export('Fridge')
# Cabinet and laminate counter with drawers, brass pulls, raised checkout trim.
cube('counter cabinet',(0,0,.48),(3.4,.82,.96),'Terracotta',.055)
cube('laminate top',(0,0,1.01),(3.52,.94,.1),'Cream',.045)
cube('toe kick',(0,-.42,.09),(3.32,.05,.18),'Dark',.01)
for x in [-1.12,0,1.12]:
    cube('drawer face',(x,-.425,.77),(1.06,.035,.3),'Cream',.02);cube('pull',(x,-.458,.77),(.24,.035,.028),'Metal',.008)
    cube('lower door',(x,-.425,.36),(1.06,.035,.46),'Terracotta',.025)
cube('blue edge',(0,-.476,1),(3.48,.028,.035),'Blue',.005)
export('Counter')
# Cash register: tilted keybed, stepped display, printer slot, cash drawer, keys.
cube('drawer',(0,0,.08),(.65,.49,.16),'Metal',.024);cube('cash drawer face',(0,-.252,.08),(.6,.026,.11),'Dark',.009);cyl('drawer lock',(0,-.269,.08),.017,.012,'Paper').rotation_euler[0]=math.pi/2
cube('register body',(0,.015,.23),(.62,.43,.28),'Cream',.04)
keybed=cube('keybed',(0,-.1,.345),(.46,.23,.024),'Metal',.01);keybed.rotation_euler[0]=math.radians(12)
for x in range(7):
    for y in range(4):cube('key',(-.145+x*.045,-.18+y*.047,.366+y*.008),(.032,.034,.018),'Teal' if x<2 else 'Dark',.006)
cube('display housing',(.13,.12,.48),(.34,.11,.21),'Cream',.025);cube('display',(.13,.058,.48),(.27,.008,.12),'Screen',.008)
cube('receipt slot',(-.19,.055,.38),(.15,.025,.025),'Dark',.005);cube('receipt',(-.19,.07,.44),(.13,.008,.13),'Paper',.002)
export('Register')
# Handheld scanner and stand; retail basket lattice and folding handle.
cyl('scanner stand',(0,0,.022),.12,.044,'Dark');cyl('scanner post',(0,0,.13),.022,.22,'Metal')
cube('scanner grip',(0,0,.26),(.055,.055,.21),'Cream',.025).rotation_euler[1]=-.3
cube('scanner head',(.03,0,.39),(.16,.12,.09),'Teal',.035);cube('red scan lens',(.11,0,.39),(.007,.09,.05),'Terracotta',.008)
export('Scanner')
cube('basket bottom',(0,0,.025),(.48,.34,.05),'Blue',.02)
for z in [.09,.17,.25]:
    for y in [-.17,.17]:cube('basket rim',(0,y,z),(.5,.014,.014),'Blue',.004)
    for x in [-.25,.25]:cube('basket rim',(x,0,z),(.014,.35,.014),'Blue',.004)
for x in [i*.062-.245 for i in range(9)]:
    for y in [-.17,.17]:cube('basket slat',(x,y,.145),(.012,.018,.24),'Blue',.004)
for y in [i*.055-.165 for i in range(7)]:
    for x in [-.25,.25]:cube('basket slat',(x,y,.145),(.018,.012,.24),'Blue',.004)
cube('handle',(0,0,.37),(.44,.024,.026),'Metal',.008)
for x in [-.22,.22]:cube('handle arms',(x,0,.31),(.024,.024,.14),'Metal',.007)
export('Basket')
cube('crate',(0,0,.23),(.55,.43,.46),'Cardboard',.018);cube('packing tape',(0,0,.465),(.09,.435,.008),'Paper',.002)
cube('shipping label',(0,-.222,.25),(.3,.012,.16),'Paper',.002)
export('Crate')
# Eight silhouette/label variants with real cylindrical UVs and bevels.
for i,brand in enumerate(brands):
    if i in [0,1,4]:
        lathe('bottle',[(0,.038),(.01,.045),(.16,.045),(.2,.023),(.235,.02)],'Terracotta' if i==0 else 'Glass')
        lathe('wrap label',[(.047,.046),(.145,.046)],brand)
        cyl('cap',(0,0,.24),.024,.018,'Terracotta' if i==0 else 'Cream')
    elif i in [5,6,7]:
        lathe('can',[(0,.047),(.012,.05),(.17,.05),(.18,.047)],brand)
        cyl('lid',(0,0,.181),.048,.006,'Metal');cyl('rim',(0,0,.008),.05,.009,'Metal');cube('pull tab',(0,0,.186),(.015,.035,.004),'Dark',.004)
    else:
        o=cube('carton',(0,0,.13),(.14,.075,.26),brand,.006)
        # Full illustrated face on each box side.
        uv=o.data.uv_layers.active
        for poly in o.data.polygons:
            normal=poly.normal
            for l in poly.loop_indices:
                v=o.data.vertices[o.data.loops[l].vertex_index].co
                if abs(normal.y)>.9:uv.data[l].uv=((v.x if normal.y>0 else -v.x)/.14+.5,v.z/.26+.5)
                elif abs(normal.x)>.9:uv.data[l].uv=(v.y/.075+.5,v.z/.26+.5)
        if i==3: cube('folded top',(0,0,.276),(.13,.065,.036),'Cream',.004)
    export('Product_'+brand)
working=ROOT/'Assets/Environment/Shopkeeper/Working';working.mkdir(parents=True,exist_ok=True)
# Pack original label images into the collaborator source; save a copy only.
bpy.ops.file.pack_all()
for index,obj in enumerate(scene.objects):
    obj.hide_set(False);obj.location=(index%5*2.5,index//5*2.5,0)
bpy.data.libraries.write(str(working/'Shopkeeper_AssetLibrary.blend'),{scene},fake_user=True)
bpy.context.window.scene=old_scene
(ROOT/'Docs/Diagnostics/Phase4/model-inventory.json').write_text(json.dumps(stats,indent=2))
print(json.dumps(stats));print('Restored original Blender scene:',old_scene.name)
