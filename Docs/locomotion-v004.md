# Phase 2 v004 — locomotion review candidate

**Alternating locomotion restored; production quality gate remains open.** This version addresses v003's fixed feet and restricted, poorly alternating run. It preserves all v003 character geometry, weights, UVs, materials and corrective shapes. Residual shorts/calf contact, cuff folding and internal knee folds prevent a clean deformation pass. Stronger movement is not presented as proof that the character is ready for the next phase.

## Review the actual motion

Open [Sunny_Locomotion_v004.blend](../Assets/Characters/Sunny/Working/Sunny_Locomotion_v004.blend). `P2_v004_Locomotion` is the neutral working scene; `P2_v004_Walk_Review` plays frames **1–32** and `P2_v004_Run_Review` plays **1–24**, both at 24 fps. A matching closing key exists at 33/25; omit that duplicate endpoint from the playback range. `P2_v004_Baseline` contains an appended, unchanged v003 comparison copy. All file textures are packed.

The saved file was reopened through Blender MCP. [Reopen checks](Diagnostics/Phase2/v004/saved-file-reopen-check.json) confirmed both persisted actions, moving feet, 53-bone review rigs, packed textures and cuff drivers targeting the correct copied rig. Blender currently has the v004 project open in its neutral working scene.

Each preview below encodes **two complete cycles**, then repeats. GIF duration rounding alternates 40/50 ms intervals to approximate 24 fps. Comparison playback is synchronized by normalized cycle phase: the v003 clips are time-scaled to the v004 duration, not played at their original different frame counts.

| Requested view | v004 animated preview | Synchronized v003/v004 comparison |
|---|---|---|
| Walk side | [Two loops](Diagnostics/Phase2/v004/walk-side-preview-two-loops.gif) | [Comparison](Diagnostics/Phase2/v004/walk-side-comparison-two-loops.gif) |
| Walk front | [Two loops](Diagnostics/Phase2/v004/walk-front-preview-two-loops.gif) | [Comparison](Diagnostics/Phase2/v004/walk-front-comparison-two-loops.gif) |
| Run side | [Two loops](Diagnostics/Phase2/v004/run-side-preview-two-loops.gif) | [Comparison](Diagnostics/Phase2/v004/run-side-comparison-two-loops.gif) |
| Run three-quarter | [Two loops](Diagnostics/Phase2/v004/run-three_quarter-preview-two-loops.gif) | [Comparison](Diagnostics/Phase2/v004/run-three_quarter-comparison-two-loops.gif) |

All-frame sheets are available for [walk side](Diagnostics/Phase2/v004/walk-side-all-frames.png), [walk front](Diagnostics/Phase2/v004/walk-front-all-frames.png), [run side](Diagnostics/Phase2/v004/run-side-all-frames.png) and [run three-quarter](Diagnostics/Phase2/v004/run-three_quarter-all-frames.png). Fixed cameras and a stationary floor support contact inspection. Rendering and geometry sampling cover complete cycles; no claim about real-time playback performance is made.

## Diagnosis before animation edits

[The full v002/v003 diagnosis](Diagnostics/Phase2/v004/animation-diagnosis.json) records every integer-frame hip position, thigh/shin pitch, knee bend, ankle position, global foot quaternion and all curve paths/key counts/ranges.

- v003 forces both feet to their rest world orientation: measured foot rotation delta is **zero throughout both clips**. Shoes therefore remain flat even during swing.
- Walk ankle fore/aft travel drops from roughly **0.25 units** in v002 to **0.079–0.096** in v003; ankle lift drops from approximately 0.055–0.058 to 0.015–0.017 units.
- Run left/right ankle fore/aft correlation changes from **−0.924** to **−0.253**. The v003 minimum-reach correction changes foot target direction rather than preserving a coherent stance/swing path. Alternation is consequently weakened, despite successful curve playback.
- Source run knee flexion reaches approximately **145–148 degrees**. The short calf and oversized shoe cannot accommodate that imported recovery motion cleanly. v003 avoids it by restricting lift, flattening feet and limiting flexion, which explains the restrained gait.
- Both shoes already have exactly one influence: weight 1 to their corresponding Foot bone. The observed stiffness is not diagnosed as a shoe-weight defect.

## Animation changes; preserved character data

New in-place leg cycles use explicit left/right phases separated by half a cycle. Walking has a 62% stance interval and 38% swing; running has a 34% stance interval with flight intervals between alternating contacts. Stance feet travel rearward at constant speed, and eased swing paths bring them forward with sole clearance. Foot pitch continues smoothly from toe-off through swing to heel strike. An analytical two-bone solution retains the existing bone lengths and a forward knee pole; common hip height is fitted to reach rather than changing the authored foot path or stretching bones.

Walk foot travel is **0.14 units** and maximum sole lift **0.022**; run travel is **0.20** and maximum sole lift **0.034**. Hip bounce spans about **0.008 units walk / 0.016 run**. Modest lateral sway and forward lean retain the rounded cartoon character. These are restrained short-legged cycles, not a realistic human gait or a high-knee sprint.

The approved Mixamo upper-body pose vocabulary is sampled into the new actions and phase-aligned to the new left-foot contact. It retains opposite-arm swings, bent running elbows and existing torso/head motion. The original upper geometry, clothing, weights and materials are unchanged; only animation changes. No new arm pose range was invented to force the jacket to follow a different articulation.

The shared skeleton remains **53 bones**, with identical rest matrices, hierarchy and bone lengths. Neutral working height remains **1.2 units**. All module mesh coordinates, connectivity, UVs, corner normals, weights, smoothing, material references and shape-key coordinates match the appended v003 baseline. Cuff drivers are redirected to each copied rig. No mesh reconstruction, skin-weight changes or changes to original files were made in v004.

## Full-cycle checks

[Motion/deformation diagnostics](Diagnostics/Phase2/v004/locomotion-validation.json) evaluate 129 walk samples and 97 run samples, including every quarter-frame and the closing endpoint. [Contact and coordination checks](Diagnostics/Phase2/v004/contact-coordination.json) compare evaluated transforms against the authored paths.

| Check | Walk | Run |
|---|---:|---:|
| Knee bend range, combined sides | 14.7–62.0° | 9.0–68.7° |
| Foot rest-orientation delta, maximum | 6° | 9° |
| Left/right fore-aft correlation | −0.868 | −0.970 |
| Worst leg p99 edge ratio | 1.79 | 1.91 |
| Worst shorts p99 edge ratio | 1.68 | 1.99 |
| Maximum leg edge ratio | 2.09 | 2.22 |
| Maximum shorts edge ratio | 2.00 | 2.60 |
| Closing lower-body vertex error | 0 | 0 |
| Left/right shoe intersections | 0 | 0 |

Shoe edge ratios remain within approximately **0.003%** of rigid shape. No zero-area skin-cage faces were found. Sampled soles remain at least about **0.0003 units** above the floor; stance height error is below 0.000001 units. Running has both soles above 0.003 units at quarter-frame samples **10–12.25 and 22–24.25**, including five integer frames across both flight intervals. Walk retains contact without a flight phase.

These are **in-place** clips. Stance feet move backward while the character stays centered, as in a treadmill preview. The implied forward speeds are approximately 0.169 units/s walk and 0.588 units/s run. Adding that forward root travel cancels sampled stance ankle drift to within 0.000009 units/s. This verifies the authored stance path; it does not guarantee a perfectly locked sole contact point during heel/toe roll or correct contact at a different game movement speed. Engine integration and root-motion implementation were not performed.

The original severe v002 stretching is reduced: run leg p99 was approximately 3.2, compared with 1.91 here. Relative to v003, stronger movement increases run shorts p99 from approximately 1.72 to 1.99. Thus the retained geometry does not support the stronger gait without some renewed garment strain. Metrics and visible movement must be considered together.

## Remaining defects and exact review frames

- **Shorts contact/cuff folding persists.** Integer-frame shorts/calf crossings occur at walk **7–13 and 22–30**; run **1–9, 12–21 and 24–25**, where 25 is the duplicated closing pose. Each left/right count and all intervening quarter-frames are recorded in the diagnostic JSON. Raw crossings include concealed surfaces and are not all visible, but a clean clipping pass has not been demonstrated. The cuffs remain below/near the knee pivot and are carried mainly by thigh weights, while the calves bend independently. The crotch and rear-cuff folds need pose fitting beyond an animation-only repair.
- Worst measured strain is walk **25.5**, run **20.25**. Captures: walk [front](Diagnostics/Phase2/v004/Images/v004-walk-front-025.50.png), [side](Diagnostics/Phase2/v004/Images/v004-walk-side-025.50.png), [three-quarter](Diagnostics/Phase2/v004/Images/v004-walk-three_quarter-025.50.png); run [front](Diagnostics/Phase2/v004/Images/v004-run-front-020.25.png), [side](Diagnostics/Phase2/v004/Images/v004-run-side-020.25.png), [three-quarter](Diagnostics/Phase2/v004/Images/v004-run-three_quarter-020.25.png). The run views show an uneven pulled-up cuff and compressed crotch area. They must not be accepted simply because the feet alternate.
- **Internal knee folding remains:** nonadjacent leg triangle crossings are present under flexion, primarily in the inner joint/covered region. The stronger run has up to 39/34 raw left/right leg crossings in a sample. More visible foot travel has not proven this knee cage robust for all future poses. Further animation restriction would again weaken locomotion; localized pose-space knee and cuff correction is the appropriate next intervention.
- The oversized shoes retain their rigid appearance and do not collide with each other in these samples. Their inherited internal source-surface crossings remain; shoe geometry was intentionally preserved.
- The gait is still a prototype: the run is short-strided compared with a conventional sprint, and contact/roll timing needs animator review. The delivered loops restore observable stepping, lift and arm opposition, but are not claimed to meet the final lively/polished movement standard without review.

**Readiness:** motion-review candidate, not an approved final character. Manual lower-body pose-space fitting is still needed to satisfy the no-clipping requirement while retaining the stronger steps. No further blind geometry/weight approximation was applied, and the already-approved upper assets were protected.

## Reproduce and verify

Open the preserved v003 project in a fresh Blender session. Run through Blender MCP: [diagnosis](../Scripts/Blender/locomotion_diagnose_v004.py), [build](../Scripts/Blender/locomotion_build_v004.py), [validation](../Scripts/Blender/locomotion_validate_v004.py). The build verifies its v003 SHA-256 and refuses an existing v004 output. It can rebuild experimental actions before a project is saved; choose a new version for later revisions.

For [rendering](../Scripts/Blender/locomotion_render_v004.py), set `BAU_V004_RENDER_JOBS` to `(v003|v004, Walk|Run, side|front|three_quarter, frame)` tuples. Render both versions for walk 1–32 side/front and run 1–24 side/three-quarter, plus the documented failure poses. Run `python Scripts/analyze_locomotion_v004.py` and `python Scripts/render_locomotion_v004.py` for contact checks and the eight two-loop GIFs. [Save script](../Scripts/Blender/locomotion_save_v004.py) creates review scenes and saves the new project once.

Run `python Scripts/verify_locomotion_v004.py` to verify all nine original hashes, Phase 1/v001/v002/v003 project hashes, the new saved-file hash, preview frame counts/durations, LFS tracking, scripts and documentation links. [Saved project inventory](Diagnostics/Phase2/v004/saved-project.json) and the generated `artifact-verification.json` record preservation separately from artistic readiness. No commit, push, remote, Unity initialization or unrelated system work occurred.
