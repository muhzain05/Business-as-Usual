# Character pipeline

## Current implementation checkpoint

**Phase 3 integration update:** the user authorized the first playable Unity prototype using the imperfect v004 repaired character. Its existing 53-bone skeleton is imported as Generic for direct playback; no retargeting or further character repair is required. Original source and previous Blender files remain preserved. See [the prototype integration guide](phase3-first-playable.md) and [validation](phase3-validation.md). The historical Phase 2 production-quality failures below remain valid and are not revoked by prototype integration.

Phase 2 reconstruction was approved at the unchanged 53-bone Mixamo skeleton and 1.2-unit working height. The v002 repair prototype now contains a rebuilt upper outfit, partial concealed body, local shorts/skin-interface experiments, regional weights and facial controls. **It failed the visual quality gate and is not ready for the next phase.** See [the repair report](phase2-repair-report.md), including worsening leg metrics, clipping/gaps and full-frame comparisons. Keep v001 as the protected extraction reference. Subsequent repair builds use `phase2_build_version.py` with a fresh `BAU_REPAIR_VERSION`.

Phase 2 v001 implements faithful extraction of the 15 source components and a position-based transfer of existing reference weights. It preserves original UVs/materials, explicitly transfers custom normals, and checks all 26 walk and 17 run frames against unchanged references. It does not repair deformation yet. See [the concrete reconstruction plan](phase2-reconstruction-checkpoint.md) and `Scripts/Blender/phase2_*.py` for the reproducible extraction, validation, capture, boundary-review, and save steps. The original Meshy files and Phase 1 project remain protected by SHA-256 checks.

Use a new `BAU_VERSION` for each subsequent milestone. Keep the current combined upper outfit. The approved canonical rig is the unchanged 53-bone Mixamo animation reference; only artist-control, localized weight, topology, and concealed-body work needed for the prototype should follow. Source scale normalization is confined to working copies.

## Source preservation and provenance

Keep downloaded exports and textures in `Assets/Characters/<Character>/Source/`. Do not edit, overwrite, or destructively process these files. Record path, byte size, SHA-256, origin, and eventually usage rights. Keep supplied archives accessible. The Sunny inventory contains the original .blend, character FBX, two animation FBXs, four PNGs, and download ZIP; hashes verified after organization.

## Inspect copies

Use the verified Blender MCP add-on 1.8 and protocol 13 in Blender 5.2.2 LTS. `get_addon_status` checks the connection; `get_scene_info` reports scene state; `execute_blender_code` runs the diagnostic scripts; `look` captures measured views.

For another collaborator's checkout, pass its absolute root as `BAU_PROJECT_ROOT` in the script namespace: `exec(compile(script_text, script_path, 'exec'), {'BAU_PROJECT_ROOT': checkout_path})`. The Blender scripts default to the current project's `D:/Business as Usual` path; repository paths inside them are relative to that root. This also avoids depending on Python variables persisting between MCP calls.

`Scripts/Blender/import_audit_copies.py` appends the source .blend into a clearly named audit scene and imports each FBX into its own empty audit scene. It restores the previously active scene and refuses duplicate imports. Source geometry is not assumed to be in the open scene. Do not use factory reset or load a source file over unsaved user work.

`Scripts/Blender/audit_character.py` writes topology, component bounds, materials, image packing, transforms, bone hierarchy/rest matrices, weights, and action/keyframe data. Counts of connected components refer to actual vertex/edge connectivity. Coincident positions are reported separately and are not welded.

`Scripts/Blender/audit_deformation.py` evaluates the existing clips at nine evenly distributed frames, compares posed edge lengths to evaluated rest geometry, reports weight distribution by component, and compares canonical bone names/rest transforms. It restores active scene, frame, and armature pose mode. It also exports measured geometry for the standalone component diagram.

`Scripts/Blender/controlled_checks.py` repeats the diagnostic comparison between linear blend skinning and Preserve Volume, restoring modifier settings afterward. `Scripts/render_component_report.py` uses Pillow outside Blender to draw the measured component projections. It does not edit geometry.

The saved `Sunny_Phase1_Audit.blend` contains all four independent audit scenes. Its imported meshes are unchanged. Clip frame ranges in the audit scenes were set to their imported actions for convenient playback. Temporary pose evaluation and screenshot visibility changes were restored. Packed images are retained for portable review.

## Proposed reusable architecture

One canonical humanoid skeleton should drive a base body, upper outfit, lower outfit, and two shoes. Head shell and facial pieces should share a head attachment and independent head animation controls. All skinned modules use the same rest matrices, bone mapping, proportions, and joint interfaces. Future nondeforming artist controls should be distinct from the exported deform skeleton.

Use `Hips → Spine → Spine1 → Spine2 → Neck → Head`, shoulder/arm/forearm/hand chains, and thigh/shin/foot/toe chains as the starting humanoid mapping. Resolve nonstandard finger names and terminal helper bones deliberately. A future locomotion root may be added after root-motion policy is approved. Do not simply rename bones and reuse curves without verifying action paths and bind matrices.

| Module | Current state | Proposed work |
|---|---|---|
| Base body | Only exposed forearms/hands and lower legs exist as independent skin pieces; no complete body under clothing | Reconstruct concealed torso, upper arms, pelvis, and covered limb surfaces; match existing silhouette |
| Head and face | Head shell, both eyes, and mouth are disconnected components | Extract on a copy; clean attachments and remove unrelated bone influence; prototype expressions |
| Upper outfit | Jacket, shirt, and collar form one connected component; buttons are separate | Extract the outfit as a unit first; independent layers require segmentation plus reconstruction of hidden side/back surfaces |
| Lower outfit | Shorts are their own component | Extract; repair pelvis/knee interface topology and weighting |
| Shoes | One connected component per shoe | Extract; preserve rounded sneaker silhouette; validate ankle/toe behavior |
| Hands | Each hand is connected to its forearm | Keep as body pieces initially; separate wrists only after proposing cuts and overlap surfaces |
| Facial expressions | No shape keys or facial control rig | Compare shape keys vs replaceable face geometry on working copies; preserve source face as reference |
| Future ragdolls | No physical rig implemented | Design skeleton for later physical mapping; defer constraints, colliders, and runtime behavior |

Triangle meshes can deform; quadrilateral topology is not a prerequisite for runtime animation. Retopology is recommended where joints need intentional loops, cleaner weights, or less pinching. Preserve the original UV/material reference for texture transfer and side-by-side visual checks.

## Repair and validation gates after approval

1. Approve silhouette, provisional size, rest pose, canonical bone map, and proposed separation boundaries.
2. Extract only existing disconnected components on a versioned working copy; verify triangle counts, UVs, normals, and materials remain equivalent.
3. Build the missing base body and hidden garment surfaces. Retopologize joints and layer junctions selectively before broad reweighting.
4. Rebind modules to the shared skeleton. Remove cross-region weight leakage, validate rigid head/face behavior and button attachments, and decide on the maximum influence count for the eventual engine configuration.
5. Evaluate a controlled pose suite and full walk/run playback, including shoulders, crouch, hip flexion, knees, and ankles. These are proposed validation poses, not new gameplay systems.
6. Compare shaded and wireframe views, silhouette, seams, garment intersections, texture fidelity, rest/posed strain, and loop continuity. Thresholds are triage measures, not automatic artistic pass/fail rules.
7. Export only approved versions into `Processed/`, `Rigged/`, and reusable `Assets/Animations/`. Record settings and export hashes. Verify engine import/retargeting only after an engine project is authorized.

## Limits

Phase 1 samples clips rather than proving every frame or every possible pose. It measures edge stretch, not volume loss, physical collision, self-intersection, or perceptual quality automatically. Extreme ratios can reflect very short rest edges. World scale differs between source and FBX; no canonical production height is approved. No runtime skeleton mapping, LOD, ragdoll, or Unity validation has occurred.
