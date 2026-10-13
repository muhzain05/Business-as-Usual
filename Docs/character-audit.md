# Sunny — Phase 1 character audit

Audit date: 2026-10-10 (America/Denver). Inspection performed in the running Blender 5.2.2 LTS through Blender MCP add-on 1.8, protocol 13. Source files remain byte-identical; no geometry repair or destructive separation was performed.

Phase 1 is approved. Subsequent extraction findings, including capped shorts cuffs and baked facial marks on the head shell, are documented in the [Phase 2 reconstruction checkpoint](phase2-reconstruction-checkpoint.md). The findings below remain the historical Phase 1 baseline; Git LFS has since been configured locally and extraction has since been implemented on a separate v001 project.

## Outcome

Sunny is a useful visual and proportion reference, with several ready-to-extract components. It is **not yet a reusable modular animation asset**. The strongest deformation evidence is cross-region skin weighting, reinforced by irregular triangulated joint surfaces. Jacket/shirt layering and a complete hidden base body require reconstruction. The character and animation exports also differ in vertex connectivity, bone namespaces, terminal helper bones, and material setup.

The principal bone rest transforms match closely, and the static rig action deforms essentially identically to rest. A gross bind-pose, axis, or animated-scale mismatch is not supported by these checks. Correct weights alone still would not provide missing garment surfaces, proper joint topology, or a complete modular body.

## Asset inventory and repository

All nine supplied files were moved into `Assets/Characters/Sunny/Source/` with SHA-256 verification before and after each move. [Asset inventory](Diagnostics/asset-inventory.json) contains exact filenames, previous/current paths, byte sizes, and hashes.

| Source category | Contents | Size in decimal MB |
|---|---|---:|
| Original | `Meshy_AI_Sunny_Little_Friend_1010060047_texture.blend` | 6.16 |
| Rigged | `Meshy_AI_Sunny_Little_Friend_biped_Character_output.fbx` | 3.02 |
| Animations | `…_Animation_Walking_withSkin.fbx` | 14.00 |
| Animations | `…_Animation_Running_withSkin.fbx` | 13.99 |
| Textures | Base color PNG | 3.93 |
| Textures | Normal PNG | 2.55 |
| Textures | Roughness PNG | 0.98 |
| Textures | Metallic PNG | 0.02 |
| Archives | Original rigged download ZIP | 37.57 |

Local Git was initialized on `main`. No commit, remote, or push was made. Git LFS 3.7.1 is installed and recommended for .blend, FBX, texture, and archive revisions before the first approved commit. It was not configured. Required source assets are not ignored. The source files plus the 44.4 MB working audit project already justify avoiding repeated full binary revisions in ordinary Git history.

Working project: `Assets/Characters/Sunny/Working/Sunny_Phase1_Audit.blend`. Scenes are `BAU_Audit_Original`, `BAU_Audit_Rig`, `BAU_Audit_Walk`, and `BAU_Audit_Run`. The pre-existing default scene is retained; it was restored as the active scene in the live session after saving a copy. No source .blend was opened over the user's session or overwritten.

## Mesh structure and topology

| Asset | Character vertices | Character triangles | Connected components | Boundary edges |
|---|---:|---:|---:|---:|
| Original .blend | 7,897 | 15,304 | 15 | 484 |
| Character FBX | 10,651 | 15,304 | 247 | 5,504 |
| Walk FBX character | 7,897 | 15,304 | 15 | 484 |
| Run FBX character | 7,897 | 15,304 | 15 | 484 |

Each animation FBX additionally contains an unbound `Icosphere`: 42 vertices, 80 triangles, one closed component. Thus its whole-scene mesh total is 7,939 vertices and 15,384 triangles. It is not part of the character and visibly obstructs captures unless hidden. Its provenance/purpose is unknown.

All character faces are triangles. Original: 23,198 edges, no loose vertices, no zero-area faces at the diagnostic threshold, and no edges with more than two incident faces. The 484 one-face boundary edges are real open surface boundaries; they must not be mistaken for a watertight mesh. No full self-intersection, winding, or collision analysis was performed.

The raw FBX arrays independently confirm 10,651 vertices in the character export versus 7,897 in each animation character; this difference predates Blender import. The rig has 2,754 coincident vertex duplicates at six decimal places, distributed over 2,377 duplicated positions. Three such positions have weight differences greater than 0.0001. Export seam/surface splitting is a probable explanation for much of the fragmentation, but the original exporter configuration is unknown. Do not merge by distance or separate all 247 components indiscriminately: UVs, normals, and weight continuity need review first.

The original has mainly valence 5–7 triangle vertices, plus irregular poles up to valence 11. In the animation mesh, 87 triangles have longest-edge-to-altitude ratio above 10; the maximum is 164.10 and the 95th percentile is 5.42. This is a skinny-triangle diagnostic, not a complete deformation quality score. Triangles are acceptable for games, but these surfaces do not provide deliberate shoulder, pelvis, and knee loops suitable for predictable exaggerated animation.

## What is welded and what can be separated

Component IDs below are stable for the saved diagnostic report, sorted by vertex count. Left/right refer to the character's anatomy: positive X is its left. Semantic labels were checked against source shaded views and isolated measured projections.

| IDs | Geometry | Vertices | Boundary edges | Classification |
|---|---|---:|---:|---|
| 0 | Jacket, shirt front, collar/sleeves as one upper-outfit surface | 1,640 | 100 | Outfit can be extracted as a whole; independent jacket/shirt require segmentation and hidden-surface reconstruction |
| 1, 2 | Right and left shoes | 1,130 each | 47, 43 | Disconnected; extractable, with open surfaces to validate at ankle interfaces |
| 3, 4 | Right and left forearm/hand | 929 each | 24, 26 | Disconnected from clothing; hand is connected to forearm, requiring an explicit cut for independent hands |
| 5 | Shorts (the supplied lower clothing) | 793 | 38 | Disconnected; extractable; pelvis deformation repair recommended |
| 6 | Head shell | 589 | 0 | Closed disconnected shell; extractable |
| 7, 8 | Right and left exposed lower-leg sections | 187, 185 | 48 each | Disconnected open skin sections; insufficient as a full body |
| 9 | Raised curved mouth | 106 | 32 | Disconnected; extractable; expression deformation needs a design |
| 10, 11 | Raised eyes | 76 each | 22 each | Disconnected; extractable; attach/express independently |
| 12, 13, 14 | Three jacket buttons | 43, 42, 42 | 12, 11, 11 | Disconnected; extractable but need stable garment attachment |

Head, shoes, hands/forearms, shorts, and upper outfit are **not welded to one another**. The upper outfit's shirt and jacket are connected within one component. The forearm and hand are connected within each limb component. The eye and mouth geometry is separate from the head. Cheek color is visible in the shaded texture reference; no separate cheek components were identified.

Extracting disconnected components on a copy can preserve existing triangles, UVs, and visible surfaces. It does not make those components closed, create cloth thickness, or fill hidden body regions. Independent jacket and shirt assets require more than a material split because all source triangles use one material slot and their surfaces are connected. A complete shirt beneath the jacket and a complete torso/pelvis/upper-arm body are not supplied as independent components. Reconstruct those surfaces while retaining the current visible silhouette.

## Materials, textures, dimensions, and coordinates

Original: one material (`Material_0`), one UV map, packed 2048×2048 color, combined data, and normal images. Base color uses sRGB; roughness/metallic are separated from the data image's green/blue channels, and normals use a Normal Map node with Non-Color input.

Character FBX: one material with packed 2048×2048 base color and normal images, but no connected roughness or metallic image inputs. Animation character materials connect base color, normal, roughness, and metallic; the Icosphere has a separate material. These are actual material differences between supplied exports, not proof of lost source textures. The four external supplied PNGs are retained alongside the embedded/packed images.

Imported image paths reference absent `.fbm` paths or are blank. All images used by the audited materials are packed and have valid 2048×2048 dimensions, so the missing external paths do not prevent current Blender review. Future engine export should use explicit, portable external texture paths and consistent color/data conventions. No original shader or image was rewritten.

Original bounds: X ±0.319092, Y −0.191650 to +0.193604, Z ±0.475342. Dimensions: **0.638184 × 0.385254 × 0.950684** Blender units. Its origin is centered vertically, and its source scene is metric with unit scale 1.

FBX character rest bounds: X ±0.402774, Y −0.241910 to +0.244377, Z 0 to 1.200000. Dimensions: **0.805548 × 0.486287 × 1.200000**. Object scales are 1 and rotations effectively zero after import. The character faces −Y with Z up and X across its shoulders; anatomical left is +X. The FBX shape is approximately 1.26225 times the original scale, with a ground-aligned vertical offset. The FBX audit scenes have unit scale 1 with unit system NONE; this is not an approved production height or a verified engine convention.

## Rig and animation findings

Character FBX: `SmartRigArmature`, 43 bones, predominantly `smartrig:` names. Both animation FBXs: `target_character` armatures, 53 bones, predominantly `mixamorig:` names. Main chain: Hips → Spine → Spine1 → Spine2 → Neck → Head. Both have shoulder → arm → forearm → hand and upper leg → leg → foot → toe branches. Full parent hierarchies, head/tail coordinates, deform flags, and rest matrices are recorded in [character-data.json](Diagnostics/character-data.json).

Nonstandard helper/finger names include `headfront`, `Bone_032`, `Bone_033`, `Bone_034`, `Bone_037`, and `Bone_038`. The left finger naming does not mirror the right thumb naming cleanly. These are not evidence of an expressive facial rig. No shape keys, facial controls, or pose constraints were found in the audited character setup. Head animation exists via the ordinary Head bone, but there is no separate artist-facing head controller.

For all 43 shared canonical bone names, head positions agree within about 0.000001 Blender units and reported orientation differences are zero at the diagnostic precision. Animation rigs add ten terminal `_end` bones. Some terminal/helper tail lengths differ, especially HeadTop_End and headfront (about 0.14453 units), plus Bone_037 and Bone_038 (about 0.031 units). These are mapping and export consistency concerns; main-joint bind orientation mismatch is not supported as the primary distortion cause.

Each character mesh is parented to its matching armature with an Armature modifier. Preserve Volume is disabled. All bound vertices have positive deform weights summing to 1 within 0.0001; no negative or unmatched deform-group weights were found. This validates basic binding, not weight placement. The animation meshes have 157 vertices with five or six influences, and the character FBX has 202; decide the final influence limit with the eventual engine settings.

| Action | Frames | F-curves | Keyframes | Meaning |
|---|---|---:|---:|---|
| `SmartRigArmature\|Scene` | 2–2 | 439 | 439 | Static rest-like keyed action; not locomotion |
| Walking (long imported action name in JSON) | 1–26 | 539 | 8,148 | Existing walk clip |
| Running (long imported action name in JSON) | 1–17 | 539 | 5,288 | Existing run clip |

Actions contain location, quaternion rotation, and scale channels on bones, plus object transform channels. No NLA tracks were found. Imported audit scenes use 24 fps. Verify intended sampling/timing before production export; a frame range is not a guaranteed runtime loop duration. The audit copy sets animation scene playback ranges to these actions; the initial raw scene report predates that convenience change.

## Distortion evidence and probable causes

Nine evenly spaced frames were sampled for each locomotion clip. Strain is posed edge length divided by evaluated rest edge length, excluding rest edges shorter than 0.0000001 units. These ratios flag problem areas; a large ratio on a tiny edge is not equivalent to a large visible displacement.

- Static rig frame 2: 99th percentile ratio 1.000003, maximum 1.000013, and no edges stretching above 1.5 or compressing below 0.5. This argues against a gross static bind mismatch.
- Walk: sampled 99th percentiles 1.58–1.89; 315–615 edges per sampled frame stretch above 1.5. Maximum sampled ratio 11.14 at frame 7.
- Run: sampled 99th percentiles 1.86–2.22; 644–888 edges per sampled frame stretch above 1.5. Maximum sampled ratio 14.96 at frame 9; 204 edges compress below half length there.
- The worst sampled edge is in the upper-clothing surface around rest position (0.04715, −0.11680, 0.54083). Its two endpoints have mixed spine, shoulder, and thigh weights, with differing regional blends.
- Upper outfit mean influence is approximately 27.7% Head and 24.2% combined thighs, versus about 7.0% combined Spine/Spine1/Spine2. Collar influence alone cannot explain all outfit behavior; spatial weight cleanup is needed.
- Shorts average about 82.5% combined thigh influence and only 2.7% Hips. That strongly encourages the crotch/waist to follow diverging thighs rather than a stable pelvis.
- Exposed lower-leg components average about 50.5–51.4% foot, 24.9–25.2% thigh, and only 22.9–23.3% shin influence. This is a probable contributor to unstable knee/ankle deformation.
- Buttons have widely mixed shoulder, spine, and thigh weights. The bottom button averages 62.9% right-thigh influence, making garment-relative sliding likely.

Animated bone scales differ from 1 by less than 0.000001 at sampled frames. At the two controlled comparison frames, only Hips has appreciable local translation. Together with aligned main rest matrices and identity object scales, these checks do not support changing global scale or rotating the skeleton as a repair.

A temporary Preserve Volume comparison at walk frame 20 increased p99 strain from 1.89 to 1.95 and edges over 1.5 from 615 to 773. At run frame 9 it increased p99 from 2.22 to 2.52 and edges over 1.5 from 888 to 1,190, although compressed edges decreased. Both modifier settings were restored. This shows a skinning-algorithm tradeoff, not a standalone solution; eventual engine deformation must also be checked.

Probable causes are therefore **regional weight leakage, unsuitable joint/garment topology, insufficiently independent garment surfaces, and broad linear-blend compression under extreme motion**. Existing retargeted motion amplitudes on these exaggerated proportions may compound the effect; no manual retarget controls or original Meshy preview settings were available to prove that contribution. Namespace/terminal-bone inconsistencies threaten future clip sharing but do not explain a demonstrated main-joint rest mismatch, because none was found.

## Problems, severity, and repair strategy

| Problem | Severity | Recommended repair |
|---|---|---|
| Jacket, pelvis, and leg deformation; cross-region weighting | High: locomotion quality blocker | After topology/interface work, reweight by anatomical/garment region; test the same clips and controlled joint poses |
| No complete body beneath outfits | High: modularity blocker | Reconstruct concealed body, preserving visible proportions and shared seam interfaces |
| Jacket/shirt/collar share a connected outfit surface | High for independent clothing layers | Keep the outfit intact initially; propose cuts, then reconstruct hidden garment surfaces and thickness where needed |
| Irregular triangulation, skinny triangles, open interfaces | Medium overall, high near failing joints | Targeted shoulder, pelvis, knee, and ankle retopology; preserve/bake UV appearance; do not assume every boundary is an error |
| Character FBX fragmented into 247 components with coincident vertices | Medium | Compare seams/normals/UVs/weights before any selective welding; use original or animation connectivity as a safer extraction reference |
| Different skeleton namespaces and helper layout | Medium | Establish canonical bone mapping and action-path conversion, preserve bind matrices, test round-trip export |
| No expression system or head control rig | High for expression requirements, expected incompleteness | Prototype separate facial geometry controls or shape keys; add artist head controls without disturbing shared deformation bindings |
| Five/six-influence vertices | Medium, engine-dependent | Choose and validate a target limit; prune only after measuring deformation loss |
| Auxiliary Icosphere in both animation exports | Medium for review/export cleanliness | Exclude from future production exports after its purpose is confirmed; retain original exports untouched |
| Material setup differs; external paths are stale but images packed | Medium for portability/appearance | Standardize future processed materials and texture paths; preserve embedded/source appearance as reference |
| Original/FBX size and origin differ | Medium for pipeline consistency | Approve one world height and foot-origin convention; apply normalization only on working/export copies |

## Risks, tradeoffs, and next implementation phase

Disconnected-component extraction is low risk to existing triangles, but open interfaces can expose holes under motion. Blind seam welding can damage hard normals, UV splits, or intentional overlaps. Reweighting existing triangles is faster than retopology but cannot create a hidden body or better joint loops. Retopology and reconstructed clothing improve reuse but require texture transfer, silhouette comparison, and renewed binding. Layered outfits add geometry and increase clipping risk; a combined upper outfit is a simpler first modular unit.

Approve a **Phase 2 modular prototype on working copies**: establish the canonical skeleton/height, extract the 15 existing components as needed, reconstruct the base body and necessary garment surfaces, retopologize failing joints, and rebind with localized weights. Demonstrate one stable upper outfit, shorts, shoes, and independently controlled head/face on the shared rig before expanding the wardrobe. Compare against the saved references and diagnostic clips. Do not implement ragdolls, multiplayer, or gameplay during this character repair phase.

The proposed changes are reviewable here and have not been applied. Git commit approval is also still required. No Unity project, new character, external AI service, remote repository, or credit-spending operation was created.

## Evidence and limitations

- [Original shaded reference](Diagnostics/original-views.png)
- [Measured component diagram](Diagnostics/connected-components.png)
- [Walk samples](Diagnostics/walk-frames.png) and [run samples](Diagnostics/run-frames.png)
- [Auxiliary sphere obstruction](Diagnostics/auxiliary-obstruction.png)
- [Mesh/material/rig/action data](Diagnostics/character-data.json)
- [Sampled deformation and component weights](Diagnostics/deformation-data.json)
- [Controlled skinning checks](Diagnostics/controlled-checks.json)
- [Raw FBX geometry counts](Diagnostics/fbx-structure.json)

Screenshots with auxiliary geometry hidden are diagnostic views only; visibility was restored. The run contact sheet crops part of the head at its selected framing, so use the original views for full silhouette reference. Sampling does not prove every animation frame is clean, and numerical strain does not independently measure volume, intersections, collision, or perceptual quality. No engine retargeting/export, facial prototype, full body reconstruction, texture rebake, or destructive mesh repair was performed. Source license/account terms were not supplied or independently verified.
