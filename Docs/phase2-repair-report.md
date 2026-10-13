# Sunny Phase 2 — v002 repair prototype and failed quality gate

Date: 2026-10-10. Reconstruction was approved after the v001 extraction checkpoint, with an added requirement to repair the jacket's neutral geometry. **The v002 prototype is saved and reviewed, but Phase 2 is not complete and Sunny is not ready for the next phase.** Several visible defects remain, and some leg metrics became worse. This project is an experimental working copy, not a production asset.

## Review the actual result

- [Versioned Blender project](../Assets/Characters/Sunny/Working/Sunny_Phase2_Repair_v002.blend)
- [Jacket: original vs prototype, front / side / back](Diagnostics/Phase2/v002/jacket-front-side-back-comparison.png)
- [Whole character: neutral front / side / back](Diagnostics/Phase2/v002/rest-front-side-back-comparison.png)
- [Full 26-frame walk comparison](Diagnostics/Phase2/v002/walk-full-comparison.gif)
- [Full 17-frame run comparison](Diagnostics/Phase2/v002/run-full-comparison.gif)
- [Every walk frame on one sheet](Diagnostics/Phase2/v002/walk-all-frames-sheet.png)
- [Every run frame on one sheet](Diagnostics/Phase2/v002/run-all-frames-sheet.png)
- [Controlled arm raise, crouch, knee bend and ankle flex](Diagnostics/Phase2/v002/controlled-poses-comparison.png)
- [Independent eye / mouth controls](Diagnostics/Phase2/v002/Images/after-face-prototype.png)
- [Bare-body review: unfinished joins and texture mismatches are visible](Diagnostics/Phase2/v002/Images/after-body-unclothed-review.png)
- [Modular inventory](Diagnostics/Phase2/v002/modular-inventory.json)
- [All-frame quantitative diagnostics](Diagnostics/Phase2/v002/deformation-validation.json)
- [Preservation and saved-project hash](Diagnostics/Phase2/v002/saved-project.json)
- [Artifact integrity, LFS and repository checks](Diagnostics/Phase2/v002/artifact-validation.json)

Open `P2_v002_Repair` for the neutral assembly. `P2_v002_Walk_Review` and `P2_v002_Run_Review` contain the original clips on the copied shared rig, with their exact 1–26 and 1–17 frame ranges at 24 fps. Their bones are hidden in the viewport for unobstructed review; the rigs remain present and editable. `P2_v002_Render_Before` / `After` are presentation copies with matching cameras and lights. The v001 scenes remain available as references.

The GIFs contain every integer animation frame, repeated at approximately 24 fps. They are preview renders, not engine playback or a subframe motion-blur test. All screenshots use the same fixed orthographic cameras, resolution and studio lighting for both versions. Material shaders are evaluated by Blender EEVEE; there is no floor in the images, so numeric foot-ground checks must also be considered.

## Jacket inspection and implementation

The original upper outfit has a lumpy collar, uneven front-opening edges, irregular sleeve rims, and a cream inner ring at the cuffs. Its jacket, collar and shirt surfaces are welded into one triangulated component. The silhouette references remain useful, but exact source matching is not a jacket-quality goal.

Rebuilt the upper outfit as one module with separate internal surfaces/material regions: a teal open-front shell, a cream shirt panel, and a split fold-over cream collar. The torso has horizontal quad rings; each sleeve bridges an actual shoulder opening through four quad rings to an open cuff. A subdivision modifier rounds the cage before skinning. An inward 0.004-unit thickness modifier supplies visible edges at the opening, hem and cuffs. This version has no added wrinkles or ornamental detail.

The original defective upper outfit is preserved in v001. Its three buttons are retained, repositioned onto the new front panel, and given consistent garment-relative weights. The source buttons themselves are still irregular; no polished button remodeling is claimed.

The neutral jacket is smoother and its opening/hem more regular. The new collar is flatter and more angular than the desired final clothing, and the sleeve-root shape is still unsatisfactory under large rotations. **The jacket does not meet the requested visually polished, animation-correct standard.** Its flat teal/cream materials approximate the source palette, but do not preserve the original texture variation. Texture/material fidelity therefore remains an unfinished requirement.

## Body, shorts, weights and face

- Added concealed torso, pelvis, upper-arm, thigh and foot envelopes. Extended the retained arm and calf boundary rings to form connected concealed limb surfaces. Visible source vertices were kept in place in the neutral pose. Torso-to-arm and pelvis-to-thigh connections still use separate overlapping modules; the bare-body review exposes gaps. This is not a complete welded clothing-swap body.
- Kept preliminary arm/thigh envelope objects as nonproduction references; the retained skin extensions supersede them in the assembly. The inventory includes these references explicitly rather than implying they are final body modules.
- Removed only 65 downward-facing interior shorts-cap triangles selected within the leg-opening ellipses. The outer cuffs and remaining source surfaces were retained. An inward 0.003-unit thickness modifier supplies inner rims. The resulting openings are irregular and need proper cuff topology; they are not validated final repairs.
- Replaced head/thigh leakage in the upper outfit with spine, neck, clavicle and arm weights. Head, eyes and mouth follow `mixamorig:Head` rigidly. Shorts now blend pelvis and their corresponding upper leg. Retained limbs received regional elbow, knee and ankle weights. Deform weight sums are normalized within 0.000001.
- Used the existing clavicle bones to stabilize sleeve roots and coherent sleeve-ring weights. Preserved-volume skinning is enabled in the prototype, but did not solve the remaining joint deformation.
- Tried reversible covered-skin masks during investigation. **All mask modifiers are disabled in the final saved review and final comparisons**, so clipping and gaps are not concealed. They remain marked as an unsuccessful experiment and can be removed in a later version.
- Cleaned 10,795 dark baked eye/smile pixels on copied head color/normal images, preserving the original images and cheek colors. Saved and packed the copied maps in `Working/Textures/v002`. Separate eyes have independent `Blink` shape keys; the mouth has `SmileWidth`. Saved defaults are open eyes and neutral smile. These are simple prototype controls, not lip-sync or a facial rig.

## Compatibility and quantitative result

The canonical skeleton still contains all **53 Mixamo bones**, with exact original names, parent hierarchy and rest matrices. Maximum rest-matrix difference from v001 is **0.0**. Both clips' bone paths resolve. The 1.2-unit character height and one-unit-as-one-metre provisional assumption are unchanged. No skeleton replacement or gameplay scale change occurred.

Diagnostics evaluate all 26 walk and 17 run integer frames. For strain, subdivision, thickness and visibility modifiers are temporarily disabled; skinning remains active. Ratios compare deformed edge lengths to each mesh's own neutral cage. Rebuilt/extended topology differs from the original, so ratios are descriptive and cannot alone establish visual improvement. Counts across different edge sets are not directly comparable. Every-frame data and polygon-area ratios are retained in the JSON.

| Mesh / clip | Original worst p99 edge ratio | Prototype worst p99 | Original maximum edge ratio | Prototype maximum |
|---|---:|---:|---:|---:|
| Upper outfit / walk | 2.202 | 1.759 | 11.690 | 3.192 |
| Upper outfit / run | 2.909 | 2.233 | 15.002 | 3.190 |
| Shorts / walk | 1.604 | 1.894 | 4.213 | 2.327 |
| Shorts / run | 1.811 | 2.445 | 5.103 | 3.344 |
| Left calf + new thigh / walk | 1.779 | 2.025 | 1.975 | 2.539 |
| Left calf + new thigh / run | 2.020 | 3.188 | 2.234 | 4.225 |
| Right calf + new thigh / walk | 1.819 | 1.997 | 2.123 | 2.577 |
| Right calf + new thigh / run | 2.204 | 3.218 | 2.476 | 4.155 |

The jacket's maximum stretch is lower and its head/thigh/pelvis influence sum is zero. Shorts maxima are lower but p99 is worse. The extended legs are worse under both metrics. This mixed result fails the quality gate; it must not be presented as an overall successful deformation repair.

Neutral whole-character silhouette intersection-over-union is 0.966 front, 0.946 side and 0.965 back under matching cameras. Head, hand and shoe proportions remain recognizable, while the intended jacket reconstruction changes the torso outline. These image metrics do not certify the new silhouette artistically or establish color fidelity.

With Z=0 provisionally treated as ground, the lowest shoe vertex reaches −0.121 units during walk and −0.112 during run. The original baselines reach −0.124 and −0.112 respectively. The ground-placement defect was largely inherited and has **not** been corrected. There is no IK, root-motion correction or floor-contact validation pass. Controlled poses are joint stress tests, not grounded animation poses.

## Remaining defects and next technical work

1. Sleeve roots fold and expose skin or gaps during running and arm raises. The current loft/cage and automated weights cannot reliably follow the short arms' large rotations. Artist-guided shoulder topology, sleeve fit and weight painting are needed; pose correctives may be appropriate after the cage is sound.
2. Shorts cuffs and knees have obvious clipping, uneven rims and pinched shapes. Rebuild the local cuff rings around the actual limb interfaces, then retopologize the knee transition and test skin/garment together. The current opening mask is an experiment, not a final pattern.
3. Bare-body shoulder/pelvis joins are unfinished; foot stubs overlap the calves. Concealed skin colors and old cuff-colored texels do not blend. Complete and weld the local interfaces with deliberate UV/material transitions before clothing-swap use.
4. Preserve original material character on the rebuilt garment through deliberate UV/baking or a reviewed stylized material, rather than treating flat palette matching as sufficient.
5. Correct foot contact using reviewed animation/retargeting changes on new working copies while retaining the canonical skeleton, and validate all frames against a visible floor.
6. Facial controls work as a minimal geometric prototype. Blink compression does not create eyelids or eyelash shapes; head patch shading still needs close-up review under varied lighting.

Automation produced a reviewable reconstruction and weight experiment, but did not reliably complete the necessary local anatomy/clothing topology. Further broad procedural substitutions would risk worsening the approved character. No major redesign, skeleton replacement, generic-human substitution or full-body remesh was applied. The v001 baseline remains the protected reference; v002 is explicitly rejected for next-phase use.

## Reproducibility and preservation

Blender scripts are in `Scripts/Blender/`:

1. `phase2_repair_character.py` — copy v001, build outfit and envelopes, regional weights and face keys.
2. `phase2_clean_face.py` — copied head-map cleanup.
3. `phase2_refine_deformation.py` — coherent sleeve/elbow transitions.
4. `phase2_extend_skin_interfaces.py` — retained boundary extensions and disabled visibility experiments.
5. `phase2_refine_shoulders.py` — existing clavicles and torso-only hem attachment.
6. `phase2_validate_repair.py` — full clips, bone compatibility, controlled poses and inventory.
7. `phase2_render_comparisons.py` / `phase2_render_controlled.py` — matching EEVEE render jobs.
8. `phase2_save_repair.py` — source/protected-project checks, playable review scenes and overwrite refusal.

Run the build in a fresh output version using `phase2_build_version.py`; for example, pass `BAU_REPAIR_VERSION='v003'` and `BAU_PROJECT_ROOT` in its execution namespace. It replaces only version/root literals when running the same six build/validation scripts and retains the v001 reference paths. Existing saved versions are refused. For rendering and saving the new version, use the same root/version substitution on those scripts. Pass `BAU_RENDER_JOBS` as `(Before|After, rest|jacket|walk|run, front|side|back, frame)` tuples. The report compositor is `Scripts/render_phase2_repair_report.py` (v002 outputs). This workflow uses the saved extraction and Blender Python; no new paid models or assets are required.

All nine original source files and both protected audit/extraction projects were SHA-256 checked before saving. The new project is 96,356,167 bytes; SHA-256 `d00d1f0dc2da21469b11262277dfd1857438d7cc1676bdb113a7bf51249b4926`. Working head textures are packed and also supplied as collaborator assets. Git LFS tracking covers `.blend`, `.fbx`, `.png`, `.jpg`, `.zip` and the new `.gif` previews. No commit, push, remote creation, publishing, Unity initialization, gameplay, networking or ragdoll work occurred.
