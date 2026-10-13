"""Run through Blender MCP in a live Blender; append/import copies only."""
import bpy, json
from pathlib import Path

ROOT = Path(globals().get('BAU_PROJECT_ROOT', 'D:/Business as Usual'))
SOURCE = ROOT / 'Assets/Characters/Sunny/Source'
prior_scene = bpy.context.window.scene
if any(s.name.startswith('BAU_Audit_') for s in bpy.data.scenes):
    raise RuntimeError('Audit scenes already exist; refusing duplicate import.')
with bpy.data.libraries.load(str(next((SOURCE / 'Original').glob('*.blend'))), link=False) as (src, dst):
    dst.scenes = list(src.scenes)
for i, scene in enumerate(dst.scenes):
    scene.name = 'BAU_Audit_Original' + (f'_{i}' if i else '')
    scene['bau_source'] = 'Original Meshy blend (appended copy)'
for category, pattern, label in [('Rigged', '*.fbx', 'Rig'), ('Animations', '*Walking*.fbx', 'Walk'),
                                  ('Animations', '*Running*.fbx', 'Run')]:
    scene = bpy.data.scenes.new('BAU_Audit_' + label)
    scene['bau_source'] = str(next((SOURCE / category).glob(pattern)).relative_to(ROOT))
    bpy.context.window.scene = scene
    bpy.ops.import_scene.fbx(filepath=str(next((SOURCE / category).glob(pattern))))
bpy.context.window.scene = prior_scene
print(json.dumps({s.name: [(o.name, o.type) for o in s.objects] for s in bpy.data.scenes}))
