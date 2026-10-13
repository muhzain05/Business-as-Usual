# Targeted leg repair — v003 review

**Improved, but the lower-body quality gate has not passed.** All other Phase 2 modules retain their approved geometry, weights, materials and animation channels. This version repairs leg-only skinning and adapts the motion to Sunny's proportions; rear-cuff contact and deep-flexion collapse still require artist-guided lower-body work. No original export or earlier project was overwritten. No commit or push was made.

## Deliverables and review

Open [Sunny_LegRepair_v003.blend](../Assets/Characters/Sunny/Working/Sunny_LegRepair_v003.blend). `P2_v003_LegRepair` opens in neutral rest pose. Select `P2_v003_Walk_Review` (frames 1–26) or `P2_v003_Run_Review` (1–17), at 24 fps, for complete repaired playback. `P2_v003_Before` retains the saved v002 lower-body baseline. Presentation scenes are separate copies; they do not alter working geometry.

- Complete comparisons: [walk front](Diagnostics/Phase2/v003/walk-front-comparison.gif), [walk side](Diagnostics/Phase2/v003/walk-side-comparison.gif), [walk three-quarter](Diagnostics/Phase2/v003/walk-three_quarter-comparison.gif); [run front](Diagnostics/Phase2/v003/run-front-comparison.gif), [run side](Diagnostics/Phase2/v003/run-side-comparison.gif), [run three-quarter](Diagnostics/Phase2/v003/run-three_quarter-comparison.gif).
- Worst-frame views: walk [11](Diagnostics/Phase2/v003/walk-worst-frame-011-comparison.png) and [12](Diagnostics/Phase2/v003/walk-worst-frame-012-comparison.png); run [10](Diagnostics/Phase2/v003/run-worst-frame-010-comparison.png) and [15](Diagnostics/Phase2/v003/run-worst-frame-015-comparison.png). Each contains front, side and three-quarter views, previous version above repaired version.
- Controls: [standing idle](Diagnostics/Phase2/v003/idle-comparison.png), [hip flexion](Diagnostics/Phase2/v003/hip_flexion-comparison.png), [knee bends](Diagnostics/Phase2/v003/knee_bends-comparison.png), [foot lift](Diagnostics/Phase2/v003/foot_lift-comparison.png), [forward step](Diagnostics/Phase2/v003/forward_step-comparison.png), [crouch](Diagnostics/Phase2/v003/crouch-comparison.png), [whole-character neutral context](Diagnostics/Phase2/v003/full_rest-comparison.png).
- [Module inventory and saved-file hash](Diagnostics/Phase2/v003/saved-project.json), [all-frame geometry diagnostics](Diagnostics/Phase2/v003/leg-validation.json), [controlled-pose diagnostics](Diagnostics/Phase2/v003/controlled-pose-validation.json), [original lower-body diagnosis](Diagnostics/Phase2/v003/lower-body-diagnosis.json).

## Diagnosis and changes

The imported run bends the knees approximately 145–148 degrees. Sunny's shin is about 0.121 units long; the oversized shoe is about 0.391 units front to back. Those poses lift and rotate an unusually large shoe through the short calf/shorts region. The shorts hem is below the knee pivot in rest pose. Existing calf extensions and weight transitions stretch unevenly; rigid shoes mainly suffer from unsuitable foot motion rather than shoe skin stretching. Imported bone positions and hierarchy were retained.

Only the lower body changed:

1. Both leg modules use local 18-ring, 24-vertex quad cages spanning covered thigh, knee, calf and ankle. Thigh/shin blending is consistent around each ring. Shin-to-foot weight leakage was removed: calf geometry follows thigh/shin, and shoe plus concealed foot follow Foot rigidly. This keeps a calf extension inside the shoe collar.
2. The shorts upper region and crotch above Z=0.278 were retained. The band below that plane was replaced with three connected cuff rings ending at Z=0.249. Pelvis/crotch weights use Hips; each cuff uses its own thigh. Two driven rear-cuff clearance shape keys provide local clearance during flexion. This is a local cuff replacement, not a full garment reconstruction.
3. New walk/run actions replace lower-limb rotation/scale channels only. Analytical two-bone targeting reduces fore/aft ankle travel and lift, limits run knee flexion to 65 degrees, and keeps oversized feet approximately level. Existing hip/root and all upper-body channels are unchanged. The result is a restrained stylized gait; it needs animation review, particularly the reduced run stride and persistent original hip bounce. Linear quaternion curves avoid inter-frame overshoot.
4. Source calf UVs were projected onto the new cages, but reprojection exposed baked cuff colours. A flat skin palette is used on the new legs and flat navy on the new cuff band. These lower-body colours approximate the originals; exact local texture fidelity is **not** claimed. Shoe geometry/materials and all approved upper materials remain unchanged.

The shared skeleton still has 53 bones, identical rest matrices and hierarchy, at the existing 1.2-unit working height. No skeleton replacement, rescaling or upper-body repair was performed.

## Validation results

Every integer frame of both clips was rendered in three matching views. Geometry and approved upper world positions were evaluated at every integer frame and quarter-frame: 101 walk samples and 65 run samples. Six controlled poses were rendered in the same three views. This checks complete supplied clips, not just selected good frames. These are fixed-camera frame sequences/GIFs, not a real-time performance benchmark or a guarantee of seamless loop boundaries.

Worst per-frame p99 edge-length ratio (1 means unchanged length):

| Region | Walk v002 → v003 | Run v002 → v003 |
|---|---:|---:|
| Shorts | 1.89 → 1.57 | 2.45 → 1.72 |
| Left leg | 2.02 → 1.66 | 3.19 → 1.85 |
| Right leg | 2.00 → 1.74 | 3.22 → 1.85 |

Topology differs, so these ratios support the visual comparison; they cannot establish a pass alone. Walk maximum leg edge ratio remains 2.03 and run 2.15. No zero-area faces were found in the sampled skin cages. The shoe edge ratio stays within approximately 0.003% of rigid shape. No left/right shoe intersections were detected in any of the 166 animation samples. Worst sole penetration is approximately 1.6 mm walk / 0.7 mm run with one unit provisionally treated as one metre. Approved upper world-space position difference is exactly zero across the same samples. Protected raw mesh, UV, corner-normal, weight, smoothing and material hashes match the v002 baseline.

## Remaining defects and manual work

The render comparisons show recognizable connected-looking calves and stable shoe shapes instead of the earlier elongated/twisted legs. They also show why this is still a review candidate:

- **Shorts/calf crossings:** walk integer frames **7–12 and 19–24**; run **1–2, 4–10 and 13–17**. Quarter-frame counts and affected left/right modules are recorded individually in the JSON. Raw triangle crossings include concealed surfaces and are not all visible, but rear cuffs remain too close to the flexing calves. Inspect walk 11/12 and run 10/15 in the supplied views. The suspected cause is the hem lying below the knee pivot, combined with insufficient posterior opening clearance and simple thigh/pelvis blending. Local rear-cuff and crotch pose fitting is needed; the current automated correctives do not establish collision-free clothing.
- **Deep-flexion failure:** controlled hip flexion, knee bends, foot lift and crouch produce local shorts or leg self-intersections. The crouch visibly compresses the leg openings; knee bends fold the inner knee surface. In crouch, the shortest sampled leg edge falls to about 8% of rest length. Idle and the forward-step control have no shorts/calf crossings, but the deeper controls fail. These control images are frame 1 by construction; pose definitions and actual knee angles are recorded in the diagnostics. Artist-guided inner-knee topology and pose-space correctives are required before approving the full range of motion.
- **Inherited shoe surface intersections:** raw nonadjacent self-crossings exist even in neutral shoes, including nested details from the source. Shoe geometry was preserved; rigid motion does not repair those pre-existing internal surfaces. No claim of watertight shoes is made.
- **Appearance/motion review:** new calf/cuff palette and local silhouette need final matching. The adapted run is visibly too restrained and flat-footed compared with the original; it needs hand-tuned foot arcs and stance/swing timing, not acceptance solely on lower strain numbers. No floor/contact locking or new root/hip motion was introduced because those channels influence the approved upper body.

Further blind weight smoothing, enlarging cuffs, hiding intersecting surfaces or reducing every test pose would conceal the remaining failure rather than reliably repair it. The automated local prototype therefore stops at this documented limitation. **Sunny's legs are not ready for production or the next phase.** The next focused intervention is manual lower-body pose fitting and knee/cuff correctives using these exact failure cases, with the upper body and skeleton protected.

## Reproduction

In a fresh Blender session, open the unchanged v002 project without saving over it. Execute these files through Blender MCP in order: [leg_repair_v003.py](../Scripts/Blender/leg_repair_v003.py), [leg_cuff_correctives_v003.py](../Scripts/Blender/leg_cuff_correctives_v003.py), [leg_validate_v003.py](../Scripts/Blender/leg_validate_v003.py), [leg_control_validate_v003.py](../Scripts/Blender/leg_control_validate_v003.py). The build refuses an existing v003 output; choose a new version in the scripts for reruns. The optional build can append required baseline scenes if absent.

Use [leg_render_v003.py](../Scripts/Blender/leg_render_v003.py) with `BAU_LEG_RENDER_JOBS` tuples `(Before|After, walk|run|control_name, front|side|three_quarter, frame)`. Render all walk 1–26 and run 1–17, then the six controls and `full_rest`. Run `python Scripts/render_leg_report_v003.py` for comparison sheets and GIFs. [leg_save_v003.py](../Scripts/Blender/leg_save_v003.py) creates review scenes, asserts protected geometry/hierarchy and saves a new file once. It refuses overwrite. `python Scripts/verify_leg_repair_v003.py` verifies preservation and artifact integrity; its success is separate from the failed visual quality gate.
