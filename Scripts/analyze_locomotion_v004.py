"""Evaluated contacts/alternation against authored targets; no Blender mutation."""
import json,math
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Docs/Diagnostics/Phase2/v004';v=json.loads((OUT/'locomotion-validation.json').read_text());build=json.loads((OUT/'animation-build.json').read_text());result={}
def corr(a,b):
    a=[x-sum(a)/len(a) for x in a];b=[x-sum(b)/len(b) for x in b];d=math.sqrt(sum(x*x for x in a)*sum(x*x for x in b));return sum(x*y for x,y in zip(a,b))/d if d else 0
for clip,data in v['clips'].items():
    plan=build['clips'][clip];rows=data['samples'];N=data['cycle_frames'];goals=plan['samples'];error=0;contact=0;clear_error=0;speed_error=0;coord={}
    for r,g in zip(rows,goals):
        for side,tag in [('Left','L'),('Right','R')]:
            error=max(error,math.dist(r['legs'][side]['ankle'],g['legs'][side]['goal']))
            clear_error=max(clear_error,abs(r['metrics']['Shoe_'+tag]['min_z']-g['legs'][side]['clearance_target']-.0003))
            if g['legs'][side]['stance']:contact=max(contact,abs(r['metrics']['Shoe_'+tag]['min_z']-.0003))
    speed=plan['fore_aft_stride']/(plan['stance_fraction']*N/24)
    for i in range(len(rows)-1):
        for side in ['Left','Right']:
            if goals[i]['legs'][side]['stance'] and goals[i+1]['legs'][side]['stance'] and goals[i+1]['legs'][side]['phase']>goals[i]['legs'][side]['phase']:
                # Add implied forward root travel (-Y) to the centered treadmill cycle.
                dy=rows[i+1]['legs'][side]['ankle'][1]-rows[i]['legs'][side]['ankle'][1];speed_error=max(speed_error,abs(dy/((rows[i+1]['frame']-rows[i]['frame'])/24)-speed))
    for side in ['Left','Right']:
        coord[side]=corr([r['legs'][side]['ankle'][1] for r in rows[:-1]],[r['legs'][side]['arm_pitch'] for r in rows[:-1]])
    result[clip]={'ankle_target_max_error':error,'sole_clearance_max_error':clear_error,'stance_ground_height_max_error':contact,'left_right_fore_aft_correlation':corr([r['legs']['Left']['ankle'][1] for r in rows[:-1]],[r['legs']['Right']['ankle'][1] for r in rows[:-1]]),'same_side_foot_arm_correlation':coord,'implied_forward_speed_units_per_second':speed,'stance_virtual_speed_max_error':speed_error,'in_place_foot_sliding_note':'Stance feet move rearward because root stays centered. Adding implied forward root travel cancels ankle drift; heel/toe sole contact can still migrate during foot roll.'}
(OUT/'contact-coordination.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
