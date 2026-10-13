"""Convert the copied Blender metallic/roughness data to URP's channel layout."""
import json
from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parents[1]
folder = root / 'Unity/BusinessAsUsual/Assets/Prototype/Art/Sunny'
materials = json.loads((folder / 'material-manifest.json').read_text())
for data in sorted({m['textures']['data'] for m in materials if 'data' in m['textures']}):
    source = Image.open(folder / data).convert('RGBA')
    red, green, blue, alpha = source.split()
    # Blender source: roughness G, metallic B. URP: metallic R, smoothness A.
    smoothness = green.point(lambda x: 255 - x)
    Image.merge('RGBA', (blue, blue, blue, smoothness)).save(folder / data.replace('.png', '_MetallicSmoothness.png'))
print('Converted copied data maps; original textures untouched.')
