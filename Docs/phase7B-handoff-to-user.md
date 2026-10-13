# Phase 7B: environment and presentation handoff

The Windows review build is `Builds/Phase7B/BusinessAsUsual.exe`. Run `Scripts/launch_phase7b.ps1 -Mode pair -Resolution 1080` for two local windows, or `-Mode desk` for the session menu. `host` and `client` modes accept `-Address` for a separate-machine session. The default is native 1920 × 1080; `-Resolution 720` uses 1280 × 720.

Review [raw before/after evidence and animation previews](Diagnostics/Phase7B/review.html), [validation results](Diagnostics/Phase7B/validation-summary.json), and [preservation checks](Diagnostics/Phase7B/preservation.json). Automated evidence does not substitute for a human keyboard/mouse or cross-machine LAN playtest.

## Implemented

- Five unique Meshy T2 assets: market, garage, cottage, tree and street lamp, with textured refinements. Cottages, foliage and furniture reuse those meshes within the existing compact scene. The two workplaces combine Meshy upper architecture with their approved lower shells: generated door openings were unsuitable, so tested doorways, collisions, target paths and reach space remain intact. Clean ceilings and upper wall bands close the roof seams.
- Matte, restrained materials; 2K base/normal textures with mipmaps, trilinear filtering and anisotropy. PBR source maps are archived; the runtime uses base/normal maps and consistent low metallic/smoothness values. This keeps the generated art compatible with the existing props and Sunny.
- A 600-second ambient cycle driven by NGO server time, including late joiners. It controls sun, readable day/evening sky, cloud tint/drift, gentle tree sway, four unshadowed street lights, fitted evening window accents and occasional clock chimes. It is a visual cycle, not the GDD's day/meeting timer.
- Owner-local hands derived from Sunny's forearm meshes. Hand vertices are refined around the wrist while arm reach is retained; prepared mesh assets work in player builds. Rest framing adjusts across FOV 60–95, and crate/bag grips follow the actual held object. Existing work gestures and tools remain available.
- Blender-authored walk/run derivatives, exported separately and used only by the town override controller. Stance motion matches the unchanged 2.2/4.2 m/s movement speeds. Final `.anim` copies remove armature-root transform tracks so Animator write defaults cannot override networked body facing. Original Sunny geometry, source rig, locomotion clips and base controller are preserved.
- Personal minimap objectives share one active-marker filter for compact and expanded maps. Completed, absent and locked targets are removed; interior objectives outside use the actual market portal. Icons clamp with their complete bounds. Colleague visibility remains proximity/line-of-sight based, with no private role targets. Both map images are rebaked.
- Both Windows graphics presets render at scale 1 with dynamic resolution off. Performance was previously excluded from Windows; it is now available beside High. A passive URP feature measures the actual game-camera target descriptor for the opt-in test driver, without adding render passes. Anti-aliasing has not been increased.
- Environment readiness preserves checkout/stockroom access, two garage bays and parts racks, sanitation sorting/loading space, square gathering space and noticeboard. Alley/yard staging markings add no interactions. Warehouse transfers can reuse existing storage/locker/staging space; no new warehouse interior or accessible home is claimed.

## Gameplay design deliverable

[Phase 8 living-town proposal](Design/phase8-living-town-proposal.md) contains the implementation-based depth audit, three expanded profession proposals, location map, dependency matrix, ranked improvements and a bounded next-phase slice. Each role has physical handling, decisions, cross-workplace visits, recoverable mistakes, distinctive information and innocent behavior that could appear suspicious.

The referenced `Design/living-town-gameplay-brief.md` was absent at task start. It now records the user's supplied side brief with that provenance. The original eight-page Working-Town GDD was read in full and remains the primary creative reference, alongside the Phase 7A handoff and manual QA addendum.

The proposed first gameplay slice is a finite shop tray/scanner and one vehicle's component interaction, reusing existing authoritative transfers. Sanitation cart work, secrets, crime, meetings, AI schedules and general physical inventory remain proposals requiring separate approval. No major job or criminal system was implemented in Phase 7B.

## Validation and limits

The linked validation summary records fresh historical editor validators, final two-process 720p/1080p checks, full/staffed/entry/guidance town shifts and original shop multiplayer regression. Final native-resolution checks read both screen size and the actual URP internal target at both quality levels. Task progression, marker retirement, role privacy, interior projection, carry disconnect recovery, replicated look/body facing and remote locomotion are tested through production authority paths.

The matched animation inspection samples 101 points in the middle of left-ankle stance at the production movement speeds, with both clips bound to the shared player hierarchy. Walk drift falls from approximately 0.970 m to 0.00048 m; run from 0.437 m to 0.00318 m over those sampled intervals. Actual town-prefab Animator bone motion and remote-player bone movement are checked separately. These are ankle contact measurements, not a guarantee about every sole vertex, transition, turn or perceived animation quality. Sunny's short legs require a fast cartoon cadence; inspect the slow-motion previews and the actual build to judge its feel. No root motion or movement-speed change conceals the result.

Short frame-time samples are archived in `Diagnostics/Phase7B/Performance`. They use two Windows Development players, fixed square views, uncapped frames and a live Unity editor on an RTX 3060 Ti. Each player contributes 79–80 sampled wall frame intervals after five seconds of warmup; `Time.unscaledDeltaTime` does not isolate CPU or GPU execution time. These are not release-build benchmarks or performance guarantees.

| Sample | Host mean / p95 (ms) | Client mean / p95 (ms) |
| --- | --- | --- |
| Phase 7A, 1080p PC | 5.58 / 7.32 | 4.56 / 5.87 |
| Phase 7B, 1080p High | 5.61 / 7.68 | 4.95 / 6.80 |
| Phase 7B, 720p Performance | 4.83 / 7.23 | 5.23 / 6.06 |

The 1080p mean rises approximately 0.6% for the host and 8.5% for the client in these short samples; client p95 rises approximately 15.7%. Windows scheduling and sampling noise limit interpretation, especially the separate 720p run. The old Windows build reports quality index 0 because Mobile was excluded; its included PC preset corresponds to the new High preset at index 1. The 720p result is a separate preset/resolution sample. No expensive AA setting was enabled.

Raw original screenshots remain available. Controlled atmosphere/animation previews are labeled separately from actual multiplayer player captures. Failed/intermediate test attempts are retained under descriptive filenames; final suites are the explicit files referenced by `validation-summary.json`.

The original Phase 5 validator was run unchanged and twice failed its final-shelf contention assumption: after the last restock auto-returns the crate, a later ordinary E may legitimately collect an order item. The source authority is unchanged and inventory remains conserved. `test_phase5_phase7b.py` retains the original full-shift sequence and both players' production E checks, while pinning the final contention pair to the same requested revision through the existing opt-in raw-command path. It tests actual stale-request rejection without relying on Windows scheduling. Original failed reports remain archived; the summary links the controlled result.

Human review should focus on hands at 60/74/95 FOV, load grip and view obstruction, the quick locomotion cadence, evening readability, and colleague interaction over a real LAN. Those usability judgments have not been human verified here.

## Credits, preservation and reproduction

Meshy expenditure is **75 of the cumulative 300-credit limit**, with **225 remaining**. Initial account balance 1095 → verified 1020. See [the task-level ledger](meshy-credit-ledger.json). No regeneration or automatic budget exhaustion was used.

Pre-phase files and staging inventory are backed up at `D:/Business as Usual Backups/pre-phase7B-2026-10-11`. Protected Sunny sources, shop gameplay/UI, authority scripts, legacy scenes and accepted diagnostic files are checked byte-for-byte. The Git index tree is unchanged. Build-only changes to player settings, network prefab registry and the two shop UI materials were restored. Phase 7B's quality/render pipeline, town scene, town player and new assets are intentional changes. No commit, push or staging operation was performed.

Unity menus: `Business as Usual / Phase 7B / Apply Environment and Presentation`, `Build Windows Town`, and `Build Phase 5 Regression`. The environment builder is idempotent and preserves functional targets. Blender derivative scripts are `Scripts/Blender/phase7b_export_environment.py` and `phase7b_locomotion.py`; original downloads and derivative `.blend` sources are under `Assets/Environment/Phase7B` and `Assets/Characters/Sunny/Working`.

Run `python Scripts/run_phase7b_regressions.py` sequentially; the Windows test players share preferences. `run_phase7b_profiles.py` gathers the short performance samples. The historical editor validators write their usual locations; `archive_phase7b_editor_evidence.py <Phase>` archives fresh evidence and restores the backed-up accepted files. The standalone C# evidence scripts in `Scripts/phase7b_*_evidence.cs` run through Unity MCP and never save their temporary inspection fixtures. `verify_phase7b_preservation.py`, `build_phase7b_review.py` and `summarize_phase7b_validation.py` verify/package the handoff. The project graph has been refreshed with `graphify update .` using AST extraction only.
