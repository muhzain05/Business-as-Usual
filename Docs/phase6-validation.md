# Phase 6 validation

The Windows town build is `Builds/Phase6/BusinessAsUsual.exe`. [The play guide](phase6-testing.md) explains two-window launch, each profession and direct parts cooperation. [The review gallery](Diagnostics/Phase6/review.html) presents the matched environment comparison, job handbooks and actual two-player navigation captures.

## Results

Fresh original-scene Unity Play-mode regressions and independent Windows multiplayer processes passed:

| Suite | Passed | Evidence |
|---|---:|---|
| Phase 3 | 19/19 | [JSON](Diagnostics/Phase6/Regression/Phase3/playmode-validation.json) |
| Phase 4 | 87/87 | [JSON](Diagnostics/Phase6/Regression/Phase4/playmode-validation.json) |
| Phase 4.1 | 57/57 | [JSON](Diagnostics/Phase6/Regression/Phase4.1/polish-validation.json) |
| Phase 4.2 | 65/65 | [JSON](Diagnostics/Phase6/Regression/Phase4.2/ui-validation.json) |
| Phase 5 | 32/32 | [JSON](Diagnostics/Phase6/Regression/Phase5/validation.json) |
| Phase 6 complete town | 35/35 | [JSON](Diagnostics/Phase6/TwoProcess1080/validation.json) |
| Phase 6 staffed cooperation and navigation | 20/20 | [JSON](Diagnostics/Phase6/Staffed1080/validation.json) |
| Phase 6 market entrance and exit | 4/4 | [JSON](Diagnostics/Phase6/MarketEntry720/validation.json) |

The initial functional checkpoint passed 32/32 before the major environment work: [functional evidence](Diagnostics/Phase6/FunctionalCheckpointComplete/validation.json). The final town run captures all three professions at 1920 × 1080, completes two repairs, six waste bags and the original three shop orders, records $16.50 and +9 shared Town Stability. The 1280 × 720 complete-town run is retained in [TwoProcess](Diagnostics/Phase6/TwoProcess/validation.json); the final prompt-filter refinement leaves the same UI layout. The staffed scenario separately requires actual Shopkeeper packing before mechanic collection and walks both players through five town segments.

## What was checked

The town suite checks exclusive assignments, reserved-slot rejection, private job permissions and prompt visibility, owner-only input/camera/audio/UI, release/movement cancellation, simultaneous Mechanic and Sanitation work, finite real parts transfers, unstaffed reserve fallback, record completion, no duplicate parts or repair consumption, bag pickup/drop/wrong-bin rejection, disconnect/rejoin recovery, shop-generated packaging and repair-generated scrap, stock conservation, all three shift contributions and shared-state equality. Normal production E dispatch restocks every shop shelf. Carried stock cannot exit the interior. The diagnostic black fixtures are excluded from the town.

Existing Phase 5 additionally checks range validation, stale/replayed requests, contested pickup, authoritative movement/jump/running, door/workstation synchronization, local host/client E restocking, duplicate-receipt prevention and host shutdown. A separate market-entry check opens the actual exterior door with E, walks through it with production movement, enters the approved interior with E and exits while preserving both assignments and the shared session. Phase 4.2 exercises original keyboard actions, actual queued GUI clicks, handbook state and scaling. New job captures demonstrate shared approved paper, typography, tabs and compact HUD conventions; they do not substitute for human usability testing.

## Method and limits

These are automated checks. The Windows suites use two independent NGO/Unity Transport players communicating over localhost UDP. Deterministic test placement reaches stations; consequential interactions use the production authority boundary and normal E/held input. Navigation checks use the production movement controller, with controlled start positions. Test controls require all three explicit test flags and are inert in an ordinary launch. No human keyboard/mouse playthrough or two-PC LAN session is claimed.

Both final Windows Development builds reported `Succeeded errors=0`. Unity's build integration also reported pre-existing Pipeline/Sentis warnings; the real build reports and generated managed assemblies were verified. A fresh final-town Unity Play-mode startup also verified six bags, two service requests, clean initial job state and depth-tested active signs: [editor smoke evidence](Diagnostics/Phase6/editor-town-smoke.json). The editor console contained no errors, and the saved town was left stopped. [Final evidence](Diagnostics/Phase6/final-validation.json) records result paths, modification times and SHA-256 fingerprints of both executables and gameplay assemblies. [Environment notes](phase6-environment.md) distinguish geometry budgets and short frame observations from profiler measurements.

## Preservation

[The preservation check](Diagnostics/Phase6/preservation.json) confirms the exact Git index is unchanged and no protected existing assets changed. Fresh earlier-phase evidence was archived under Phase6/Regression and the accepted original diagnostic files restored. Original TownPrototype/Shopkeeper scenes, Sunny model/rig/animations and approved shop assets remain intact. Shared source changes are confined to NetworkPlayer, NetworkSession, NetworkShop, the opt-in NetworkTestDriver and InventoryLedger's generic account restoration. New town scripts, art, prefabs, scene and documentation live separately. EditorBuildSettings includes the new town scene. Build/import material and ProjectSettings side effects were restored to exact prior bytes. No commit or push was made.
