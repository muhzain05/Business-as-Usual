# Sunny Phase 2 — extraction and reconstruction checkpoint

Date: 2026-10-10 (America/Denver). **Historical v001 extraction checkpoint; subsequently approved at the unchanged 53-bone Mixamo rig and 1.2-unit working height. The [v002 repair prototype](phase2-repair-report.md) failed its visual quality gate.** This is not the completed animation-ready character.

## Review artifacts

- [Versioned Blender project](../Assets/Characters/Sunny/Working/Sunny_Phase2_Extraction_v001.blend)
- [Extracted modules](Diagnostics/Phase2/v001/extracted-modules.png)
- [Proposed reconstruction boundaries](Diagnostics/Phase2/v001/reconstruction-boundaries.png)
- [Original vs extracted rest appearance](Diagnostics/Phase2/v001/before-after-extraction.png)
- [Walk/run extraction comparison](Diagnostics/Phase2/v001/walk-run-extraction-comparison.png)
- [Isolated head with baked face](Diagnostics/Phase2/v001/head-shell-baked-face.png)
- [Modular inventory, original vertex/polygon maps, and measured rings](Diagnostics/Phase2/v001/modular-inventory.json)
- [All-frame compatibility and strain measurements](Diagnostics/Phase2/v001/extraction-animation-validation.json)
- [Shorts underside review mask](Diagnostics/Phase2/v001/shorts-underside-review.json)

Open the new project and select `P2_v001_Modules_Rest` for the assembled character, `P2_v001_Modules_Overview` for labeled modules, or `P2_v001_Before_Normalized` for the original appearance at the same scale. Four appended `P2_v001_Reference_*` scenes preserve the source audit references. Earlier scenes are retained in the saved mainfile to preserve the existing session.

## Implemented scope

Appended the saved Phase 1 reference scenes into isolated Phase 2 copies rather than relying on editable live-scene state. Extracted the 15 original disconnected components into independently named mesh objects. No triangles were cut or added. Jacket, shirt, collar, and sleeves remain one upper-outfit module; hands remain connected to their exposed arm sections. Buttons remain separate objects.

Source geometry was normalized only in the new working assembly to the existing FBX reference: uniform factor 1.26224906082 and Z offset +0.6, producing 1.2 units in height, Z up, facing -Y, anatomical left at +X. This preserves proportions and allows existing animation comparisons. The source reference itself remains unchanged. The working height is proposed for approval, not an independently established final gameplay scale.

The working armature is an **unchanged copy** of the 53-bone Mixamo animation reference. Bone names, hierarchy, and rest matrices are retained. No bones, head controls, constraints, or root hierarchy were redesigned. Existing action paths resolve. Source skin weights were transferred by unique nearest rest-position correspondence; index order was explicitly not assumed. Defective weights are intentionally retained at this extraction checkpoint.

## Modular inventory

| Module | Vertices | Current interface / next work |
|---|---:|---|
| UpperOutfit | 1,640 | Neck ring 32; hem ring 40; sleeve rings 13 R / 15 L. Keep combined; repair regional weights and only failing joint topology |
| Shoe_R / Shoe_L | 1,130 each | Preserve exterior. R has a branched boundary cluster; inspect local gaps. Do not automatically fill every shoe opening |
| ForearmHand_R / L | 929 each | One closed proximal ring: 24 R / 26 L; extend concealed skin toward shoulder without cutting visible hands |
| Shorts | 793 | One 38-vertex waist ring; lower leg undersides are capped. Review local cap opening/inner rims for continuous body thighs |
| Head | 589 | Closed shell; keep separate and rigidly attached to Head after reweighting |
| LowerLeg_R / L | 187 / 185 | Two closed 24-vertex rings each; connect covered thighs above and concealed ankle/foot surfaces below |
| Mouth | 106 | Separate geometry; retains original material/UVs; expression work deferred |
| Eye_R / L | 76 each | Separate geometry; retains original material/UVs; expression work deferred |
| Button_Top / Middle / Bottom | 43 / 42 / 42 | Transfer stable garment-relative attachment weights after outfit repair |

All module origins remain in the common character coordinate frame. The overview scene contains display copies and text labels; its layout offsets are not production alignment changes. Its meshes reference the same extracted data, so use the assembled scene for physical alignment review.

## Extraction validation

| Check | Result |
|---|---|
| Total vertices / triangles | 7,897 / 15,304, unchanged |
| Modules | 15 |
| Rest positions / UVs / transferred weights | Maximum recorded differences are zero for the respective copied values |
| Original materials and packed images | Original material slots retained; no shader or source image edits |
| Custom corner normals | Preserved through explicit per-corner transfer; maximum vector difference 0.0008174, about 0.047 degrees, from re-encoding |
| Nearest-position weight correspondence | 7,897 unique matches; maximum matching error 0.000000481 units |
| Walk compatibility | All 26 integer frames; maximum surface position difference 0.000000537 units |
| Run compatibility | All 17 integer frames; maximum surface position difference 0.000000539 units |
| Shared reference hierarchy/rest matrices | Match checked against both animation references; no edits |
| Source preservation | All nine SHA-256 hashes verified; saved Phase 1 audit hash recorded and unchanged |
| Repository | Local Git LFS configured; no commit, remote, push, or publishing |

Front/side/back images use matching target dimensions, camera directions, distance, and material shading. Rest RGB mean absolute differences are less than 0.000635 on a 0–255 scale for each view. Visual inspection shows the same silhouette, colors, and texture placement. These image metrics confirm a faithful extraction, not good deformation.

All-frame measurements expose slightly larger extremes than the nine-frame Phase 1 sample: maximum walk p99 strain 1.90086 and maximum edge ratio 11.68973; run p99 2.22611 and maximum ratio 15.00217. The motion remains distorted around the outfit, pelvis, legs, and buttons. No improvement is claimed yet. Existing clips run on the copied rig, but that is only a compatibility check; no repaired-motion playback or controlled repaired poses exist yet.

## Proposed reconstruction boundaries

The diagram shows retained skin in cyan, exact existing boundary edges in magenta, and approximate concealed work envelopes in orange. It is a plan, not newly built geometry or an approved deletion selection.

1. **Torso and short neck:** build a rounded concealed shell inside the upper outfit, approximately X ±0.17, Y -0.10 to +0.13, Z 0.42–0.77 at the current scale. Match Sunny's compact chest/shoulder silhouette. Keep the head shell closed and separate; the neck overlaps beneath it rather than cutting the head.
2. **Pelvis:** reconstruct compact hips and crotch inside the shorts, approximately X ±0.17, Y -0.08 to +0.13, Z 0.33–0.46. Join to the torso and covered thighs. These boxes are initial envelopes; final surfaces must be fit against the actual outfit cross-sections.
3. **Upper arms:** extend the existing 24/26-vertex proximal skin rings beneath the sleeves to the shoulder connections. Ring centers are approximately (-0.2310, 0.0433, 0.5600) R and (+0.2280, 0.0434, 0.5577) L. Preserve visible forearm/hand surfaces and contour. Add only the hidden shoulder and elbow transitions needed for the short limbs.
4. **Covered thighs:** connect pelvis to each upper 24-vertex lower-leg ring, centered near (-0.11495, 0.03241, 0.27029) R and (+0.11477, 0.03238, 0.27026) L. Retain exposed calf dimensions. The shorts currently have no cuff boundary loops; continuous body thighs will intersect their closed undersides unless the copied garment receives a localized opening and inner-rim repair.
5. **Concealed ankle/foot interfaces:** extend the lower rings near Z 0.1146 into compact enclosed foot shapes inside the sneakers. Keep the visible shoe shape and ground-aligned rest soles. Do not replace Sunny's large shoes or short legs with realistic human proportions.

The shorts underside report flags 306 downward-facing triangles in the lower cuff region for inspection. This rule is deliberately a broad review candidate, **not permission to delete every flagged triangle**. Preserve external hem contours, inspect underside views, and form a proper inner rim/opening locally after approval. Likewise, the right shoe's branched boundary requires localized examination, not broad welding or full shoe retopology.

Body joins are between skin surfaces. Do not weld body vertices to clothing. Keep the upper outfit, shorts, and shoes independently replaceable. New hidden body surfaces may use a separate skin material/UV region matched to existing skin; original texture bytes remain untouched.

## Proposed technical implementation after approval

### Canonical skeleton and rest pose

Use the existing 53-bone `mixamorig:` animation reference as the first canonical deform skeleton, at the current 1.2-unit height and original lowered-arm rest pose. Keep the main Hips/Spine/Neck/Head, shoulder/arm/hand, and thigh/shin/foot/toe hierarchy and bind matrices. Retain unused terminal helpers initially to avoid breaking the supplied action paths. Document their role; no new locomotion root is needed for this phase.

Keep animator-facing controls separate from this skeleton where practical. Independent head control can target the existing Head pose channel after proving it composes with the original actions. Do not rename namespaces, rescale rest bones, or delete helpers just to make the hierarchy look simpler. If a joint placement change becomes necessary, present its measured before/after and retarget consequences before applying it.

### Geometry, retopology, and binding

Create the concealed torso/pelvis/limb continuations from the measured boundary rings, retaining visible surfaces and original silhouette. Add controlled joint rings rather than remeshing the entire body. Repair the copied shorts cuffs locally. Keep jacket/shirt/collar as one module.

Build a weight-only comparison candidate for the existing garment surfaces before replacing large patches. Then retopologize shoulders/cuffs, elbow transitions, pelvis/crotch, knees, and ankles only where the measured poses still fail. Project replacement surfaces onto the existing rest reference and transfer UVs/normals/material appearance. Keep existing head, hands, face, and shoe exteriors unless a localized defect requires repair.

Use localized skin weights: no thigh or Head influence on the upper outfit; torso primarily follows spine, shoulder areas clavicle/upper arm, with collar following the upper spine/neck as needed. Shorts waist/crotch should follow Hips with controlled one-sided thigh blends below. Lower legs should follow shin with smooth knee/ankle transitions. Bind head/eyes/mouth consistently to Head. Buttons should inherit weights interpolated from nearby repaired garment faces, avoiding independent thigh-driven motion. Aim for four influences per vertex, but verify any pruning against the supplied motion.

### Minimal face prototype

The isolated head still displays eyes and a smile when the separate eye/mouth objects are hidden. Those features are baked into the head appearance; the separate meshes alone cannot provide clean blinks or a new mouth pose without leaving the old expression visible.

Duplicate the relevant packed color/normal images into new versioned working textures and clean only the obsolete facial marks beneath the eye/mouth pieces, preserving cheek color and head shading. Check normal-map relief as well as color. Keep the original atlas untouched. Then prototype blink and one alternate mouth shape with shape keys on the existing separate facial modules, with neutral shape preserving the source look. Avoid a new facial bone hierarchy. Add controls only after checking that head motion, facial alignment, and expressions remain stable in both clips.

### Validation and acceptance

Compare front/side/back shaded and silhouette views against this exact baseline. Test full walk/run playback plus every-frame geometry checks; controlled arm raises, crouch, and isolated hip/knee/ankle bends follow. Measure joint stretch, garment strain, skin/garment intersections, visible gaps, foot heights and sliding, button attachment, and face alignment. Include closeups at the original worst frames and at newly measured worst frames.

Report per-region strain reductions and inspect visible results. If retopology changes the edge population, supplement raw edge ratios with common surface landmarks and triangle-area/surface comparisons; avoid claiming improvement merely because the old short edges disappeared. Keep before/after camera settings consistent. Document remaining flaws instead of labeling the asset animation-ready from successful import or playback alone.

Save subsequent milestones as new versions, preserving this v001 checkpoint and Phase 1. Do not initialize Unity, implement gameplay/networking/ragdolls, commit, push, or configure a remote.

## Reproduction

Through Blender MCP, execute each script with a namespace containing `BAU_PROJECT_ROOT` and `BAU_VERSION`. Use a new version if the matching scenes/project already exist; extraction and save scripts refuse overwrites.

```python
root = 'D:/Business as Usual'
version = 'v001'
path = root + '/Scripts/Blender/phase2_extract_modules.py'
exec(compile(open(path, encoding='utf-8').read(), path, 'exec'),
     {'BAU_PROJECT_ROOT': root, 'BAU_VERSION': version})
```

Run `phase2_validate_extraction.py`, then `phase2_plan_boundaries.py`. For captures, use `phase2_capture_views.py` with `BAU_CAPTURE` set to a documented mode; it prints arguments for MCP `look` and writes them to a capture JSON. Always run it again with `BAU_CAPTURE='restore'` after capturing. Screenshot preparation temporarily assigns the existing actions or hides objects; restoration restores scene, action/slot, pose mode, frame, and visibility.

Save screenshot PNGs with the filenames used by `Scripts/render_phase2_checkpoint.py`, then run `python Scripts/render_phase2_checkpoint.py --version v001` for the annotated diagram and comparison sheets. Rendered screenshot files originate from Blender MCP, not generated character imagery. Run `phase2_save_checkpoint.py` only after capture state is restored. It saves a new mainfile copy and verifies protected source/audit hashes. Finally run `python Scripts/verify_phase2_checkpoint.py --version v001`.

The diagram compositor uses Pillow already present in the environment. No new service, generation, or paid operation was used. The inventory retains original vertex and polygon IDs for later remap/repair scripts.

## Readiness and approval request

**Ready:** faithful modular extraction, reproducible source mapping, unchanged-animation compatibility, local LFS tracking, and a concrete reconstruction plan.

**Not ready:** repaired locomotion, complete base body, independent expression controls, validated clothed joints, clean cuffs/foot interfaces, or production export. Existing deformation defects and the baked-face issue remain.

Approve the proposed concealed-body boundaries, localized shorts cuff repair, unchanged Mixamo reference skeleton/rest pose at 1.2-unit working height, regional reweighting/targeted retopology, and copied-texture facial cleanup before implementation continues. This request follows the user's explicit Phase 2 checkpoint requirement; it is separate from any Git commit approval.
