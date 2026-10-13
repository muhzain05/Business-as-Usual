"""Rebuild in a fresh numbered output using the saved v001 baseline.
Example MCP code: BAU_REPAIR_VERSION='v003'; exec(open(this_file).read())
Calls use exact same source scripts, substituting only output version/project root.
"""
from pathlib import Path
import re
version=globals().get('BAU_REPAIR_VERSION','v002')
root=Path(globals().get('BAU_PROJECT_ROOT','D:/Business as Usual'))
assert re.fullmatch(r'v[0-9]{3}',version) and version!='v001'
pipeline=['phase2_repair_character.py','phase2_clean_face.py','phase2_refine_deformation.py',
          'phase2_extend_skin_interfaces.py','phase2_refine_shoulders.py','phase2_validate_repair.py']
for name in pipeline:
    text=(root/'Scripts/Blender'/name).read_text(encoding='utf-8').replace('v002',version).replace('D:/Business as Usual',root.as_posix())
    exec(compile(text,name,'exec'),{})
print('Built '+version+'. Inspect and render before running phase2_save_repair.py with the same version substitution.')
