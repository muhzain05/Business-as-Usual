# Phase 5 validation

Completed on October 11, 2026 (America/Denver). Launch instructions are in [phase5-launch.md](phase5-launch.md); authority and integration details are in [phase5-network-architecture.md](phase5-network-architecture.md).

## Results

### Graybox prop correction

The black panel reported in front of the shelves was the `Shared Door Test Fixture`, using `Graybox_Dark` material at world bounds centered on (-3.70, 1.10, 6.50). The saved multiplayer scene now disables both diagnostic fixtures, and the scene builder preserves that setting. Runtime initialization enables them only for a fully configured automated test driver. An ordinary Editor host check verified zero active diagnostic fixtures and zero fixture interaction targets: [check](Diagnostics/Phase5/BlackBoxFix/ordinary-play.json), [rendered shelf view](Diagnostics/Phase5/BlackBoxFix/shop-without-test-door.png). The original shop scene and materials are unchanged. The Windows build succeeded with zero errors; fresh two-process validation passed **32/32** checks, including host/client E restocking and the full shift: [transport report](Diagnostics/Phase5/BlackBoxFix/TwoProcess/validation.json), [state and build verification](Diagnostics/Phase5/BlackBoxFix/fix-summary.json).

### Restocking correction after player feedback

The original build dropped a held crate on every E press before inspecting the selected shelf. The original transport tests stocked shelves through direct commands and missed the player interaction dispatcher. The expanded suite reproduced this failure in the old executable, then passed **32/32** checks in the rebuilt executable: [before](Diagnostics/Phase5/RestockFix/Before/validation.json), [after](Diagnostics/Phase5/RestockFix/After/validation.json). E now restocks the nearest selected shelf while holding the crate and drops it away from shelves; the HUD reflects that choice. Host and client restocking, contested shelf input, normal E pickup/drop, empty-crate return and the complete $16.50 shift passed using two real Windows processes. The test driver invokes the production E handler; this remains automated testing rather than physical keyboard input. Fresh states, logs and screenshots are kept in `Diagnostics/Phase5/RestockFix/After`. The initial results and fingerprints below describe the earlier build.

| Validation | Result | Evidence |
|---|---:|---|
| Two independent Windows host/client processes | 30/30 | [Transport checks](Diagnostics/Phase5/TwoProcess/validation.json) |
| Phase 3 original gameplay regression | 19/19 | [Fresh report](Diagnostics/Phase5/Regression/Phase3/playmode-validation.json) |
| Phase 4 original shift regression | 87/87 | [Fresh report](Diagnostics/Phase5/Regression/Phase4/playmode-validation.json) |
| Phase 4.1 labels/reach regression | 57/57 | [Fresh report](Diagnostics/Phase5/Regression/Phase4.1/polish-validation.json) |
| Phase 4.2 approved UI regression | 65/65 | [Fresh report](Diagnostics/Phase5/Regression/Phase4.2/ui-validation.json) |
| Ordinary Editor host/disconnect/single-player entry | 8/8 | [Lifecycle checks](Diagnostics/Phase5/editor-session-lifecycle.json) |

These are automated tests. There was no human-controlled keyboard/mouse playtest or two-PC LAN test. The transport suite launches two separate Windows Development builds using actual NGO/Unity Transport localhost UDP. Its opt-in test driver sends owned movement and interaction commands and uses server teleports for deterministic test placement. No in-process simulation stands in for the two-process run.

The transport checks cover late joining an already received delivery; two independent player ownership/spawns; one local input, HUD, camera and listener per process; jump and running state; both characters actually moving; remote position agreement; no per-frame shift snapshots; server range rejection; duplicate command rejection; contested pickup; holder-specific carrying UI; holder-only return; stale delayed commands; clear-ground drop and stock conservation; production local E pickup detection; carrier disconnect recovery; client reconnection; shared door open/close and workstation use; simultaneous stocking; automatic empty-crate return; three shared orders with simultaneous checkout and no duplicate receipts; completed tasks/revenue; and host loss ending the client session.

Both processes finish with receipts BAU-0001/0002/0003, totals $5.10/$5.20/$6.20, shared revenue **$16.50**, all **six tasks** complete and **+3 Town Stability**. Nine units are sold, nine remain on shelves, and every SKU still sums to its original six units across the ledger. Receipt timestamps, movements, quantities and task progress agree between the two processes.

## Evidence and update behavior

The [completed host state](Diagnostics/Phase5/TwoProcess/completed-host.json) and [completed client state](Diagnostics/Phase5/TwoProcess/completed-client.json) retain ownership, positions, quantities, journal, receipts, progress and connection counters before host shutdown. [Host gameplay](Diagnostics/Phase5/TwoProcess/host-cooperative.png), [client gameplay](Diagnostics/Phase5/TwoProcess/client-cooperative.png), [host shift report](Diagnostics/Phase5/TwoProcess/host.png), and [client shift report](Diagnostics/Phase5/TwoProcess/client.png) are captures from their respective standalone processes. Carrying screenshots additionally show that only the holder gets the local carrying card.

Movement input and animation state are capped at 30 updates/second; NetworkTransform uses interpolation and a 0.015 m position threshold. Shared snapshots are reliable and sent on mutations and joins. The completed test recorded **28 shared snapshots** and **74,894 UTF-8 payload bytes**, including join/rejoin state. These are application snapshot counters, not total wire bytes, bandwidth measurements or packet-loss statistics. The idle/movement check confirmed that no shift snapshot was sent per rendered frame. Decorative merchandise is not represented by individual NetworkObjects.

Final Windows builds succeed with zero build errors. Unity's existing Pipeline package warns that no RuntimePipelineConfig is configured; this unused runtime pipeline remains disabled. The first builds also emitted warnings from the existing Sentis/inference shaders. Standalone logs have no C# gameplay exceptions in the successful transport run. The graphics driver logs an inability to query its optional D3D12 debug info queue; rendering and capture still succeeded.

## Failures found and corrected

Intermediate results are preserved under `Diagnostics/Phase5/Attempts`. The first process startup exceeded the original test deadline; startup now allows 60 seconds. Windows file sharing caused diagnostic/control-file races; reports use atomic replacement and the controller retries sharing violations. A local E pickup test initially stood closer to Checkout than to the dropped crate; its placement now approaches the crate from the clear side, respecting production nearest-interaction selection.

The real carrier-disconnect test exposed a client presentation lifecycle bug: the crate's visual parenting made it a child destroyed with the player. Both server and client now detach it before player despawn. The subsequent full run validates stock recovery, the surviving crate, reconnection and completion. Screenshot review also found the connection card overlapping expanded handbook pages; connection identity is moved to the bottom margin while a page is open. The network shift-report exit instruction now directs players to disconnect.

The final UI repeat initially failed 13 checks in a background editor because synthetic keyboard/mouse devices lost focus. The native window-focus helper was unavailable. The suite was rerun in Unity with temporary `InputSettings.BackgroundBehavior.IgnoreFocus`, passed 65/65, and the original `ResetAndDisableNonBackgroundDevices` setting was restored before building. No production input setting was changed. Ordinary Editor lifecycle checks separately exercised the production Stop/reload and Single Player transitions without test-driver flags: one manager in the disconnected menu and no manager in the original shop.

Capture review found that an existence-only screenshot wait could accept a prior run's PNG and shut down before the asynchronous fresh capture completed. The harness now removes prior capture files and verifies each new timestamp while both peers are still connected. The final gallery uses that live-session capture pass.

## Preservation

[Preservation comparison](Diagnostics/Phase5/preservation.json) compares the recorded pre-task file hashes and Git index entries. Staged entries are unchanged; original Sunny/Meshy/Blender files, rig, meshes, clips, animation controller, textures, materials, approved UI artwork/fonts/design source and original ShopkeeperShift scene are retained. The original town scene is retained. Existing diagnostic evidence was backed up, fresh runs archived, then original evidence restored.

Gameplay changes to existing source are limited to the interaction gateway, state restoration methods, local/shared HUD context and suppressing global Q during networking. New network scene/prefab/scripts and launch/test helpers are separate. Package manifest/lock entries add NGO 2.7.0 and Transport 2.6.0; build settings add the multiplayer scene. Unity build/validation side effects on two material keywords and the preloaded input-settings list are restored to their exact pre-task bytes after final validation. No source art regeneration, service integration, repository creation, commit or push occurred.

The Phase 3/4 artifact verifiers hard-code original source hashes and/or an empty Git index. They are not reported as passing: this task intentionally adds small prototype source hooks and preserves the user's nonempty index. Fresh production gameplay/polish/UI regressions provide behavioral verification, and the Phase 5 preservation report independently verifies protected assets and staged entries.

Graphify was used for the audit and its code graph updated using AST extraction, without LLM/API cost. Semantic indexing of new documentation is deferred; the code graph reflects networking dependencies.

## Known limitations

- Exactly two players, direct IPv4 LAN/localhost UDP 7777. Actual two-PC firewall/routing has not been tested.
- Server-driven movement has no prediction/reconciliation or lag compensation. Character/camera collisions use the existing prototype behavior; tight aisles and counters can shorten camera distance.
- The shared customer basket is cooperative, not a separate basket for each employee. Only the crate holder can stock while a crate is held. If shared revision changes before a command arrives, the player receives feedback and retries.
- The slice has one meaningful stock crate, one shared shift, deterministic existing products/orders and fixed compatible builds. It is not production authentication, anti-cheat, saved-session persistence, dynamic-content compatibility or a lobby system.
- Host loss ends the session. Reconnection to a surviving host restores its current shift; host migration and session continuation after host shutdown are absent.
- The multiplayer Town Exit does not independently load a different scene. Disconnect and choose Single Player to use the original town/shop flow. The original single-player scene remains functional.
- Test control files/teleports are opt-in through explicit `--bau-role`, `--bau-evidence` and `--bau-control` arguments; ordinary player launch does not activate them. This is a Development prototype build, not a hardened public release.
