"""Run in Blender MCP after create_town_assets.py. Modify only this phase's asset library."""
import bpy,json
from pathlib import Path
path=Path(r'D:\Business as Usual\Unity\BusinessAsUsual\Assets\Town')
saved_scene=bpy.context.window.scene
library=bpy.data.scenes['Phase6_OriginalTownAssetLibrary']
bpy.context.window.scene=library
try:
    source=bpy.data.objects['TownAsset_ServiceVan']
    indices={i for polygon in source.data.polygons if source.data.materials[polygon.material_index].name=='Town_Glass' and sum(source.data.vertices[i].co.y for i in polygon.vertices)/len(polygon.vertices)<-1 for i in polygon.vertices}
    if indices and max(source.data.vertices[i].co.y for i in indices)>-1.2:
        for i in indices:source.data.vertices[i].co.y-=.18
    export=source.copy();export.data=source.data.copy();export.location=(0,0,0);library.collection.objects.link(export);export.hide_set(False)
    bpy.ops.object.select_all(action='DESELECT');export.select_set(True);bpy.context.view_layer.objects.active=export
    bpy.ops.export_scene.fbx(filepath=str(path/'Art/ServiceVan.fbx'),use_selection=True,object_types={'MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=False,apply_unit_scale=True,use_mesh_modifiers=True,path_mode='AUTO')
    bpy.data.objects.remove(export,do_unlink=True)
    if 'TownAsset_RoofDistant' not in bpy.data.objects:
        roof=bpy.data.objects['TownAsset_Roof'];light=roof.copy();light.data=roof.data.copy();light.name='TownAsset_RoofDistant';light.location=(0,0,0);library.collection.objects.link(light);light.hide_set(False)
        bpy.ops.object.select_all(action='DESELECT');light.select_set(True);bpy.context.view_layer.objects.active=light
        mod=light.modifiers.new('Distant silhouette budget','DECIMATE');mod.ratio=.07;bpy.ops.object.modifier_apply(modifier=mod.name)
        bpy.ops.export_scene.fbx(filepath=str(path/'Art/RoofDistant.fbx'),use_selection=True,object_types={'MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=False,apply_unit_scale=True,use_mesh_modifiers=True,path_mode='AUTO')
        light.data.calc_loop_triangles();inventory=json.loads((path/'Art/model-inventory.json').read_text());inventory.append(dict(name='RoofDistant',triangles=len(light.data.loop_triangles),materials=[m.name for m in light.data.materials],size=list(light.dimensions)));(path/'Art/model-inventory.json').write_text(json.dumps(inventory,indent=2));light.hide_set(True)
    bpy.data.libraries.write(str(path/'Working/DaylightTown_AssetLibrary.blend'),{library},fake_user=True)
    print('Original van windshield visible; distant roof variant saved.')
finally:bpy.context.window.scene=saved_scene
