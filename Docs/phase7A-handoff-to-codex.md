# Phase 7A → Codex handoff: first-person gameplay and technical polish

Phase 7A converts Daylight Town to first person for the local player and fixes collision and noclipping. It also grounds the mechanic vans, replaces the static town diagram with a live minimap, and adds a local pause menu with persistent settings. Existing jobs, multiplayer authority and approved UI are preserved. **Phase 7B (the Meshy environment overhaul) was not started.** Nothing was committed or pushed.

## Launch

The build is `Builds/Phase7A/BusinessAsUsual.exe`. It is a Windows Development build and the Phase 5/6 builds are untouched. For a host plus client on this PC, run this from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File Scripts/launch_phase7a.ps1 -Mode pair
```

The Phase 6 modes still work: `desk`, `host`, `client` and `-Address`. The game uses UDP 7777. Close any older game window first, because one was holding 7777 during this phase.

| Control | Action |
|---|---|
| Mouse | Look (sensitivity and invert-Y in Settings) |
| WASD / Shift / Space | Walk / run / jump |
| E / hold E | Use what the crosshair aims at; inspect, repair, sort |
| Q | Return carried load |
| Tab / J / M | Handbook / staff desk / expanded town map |
| Esc | Close the open handbook, otherwise pause menu |

## What changed

### New files

| Path | Purpose |
|---|---|
| `Assets/FirstPerson/Scripts/FirstPersonView.cs` | Owner-only first-person camera: yaw/pitch, eye height 1.0 m, vertical smoothing, small head bob, crosshair. It hides the owner's body as shadow-only, moves held loads in front of the view and sets `PlayerInteractor.aim`. |
| `Assets/FirstPerson/Scripts/FirstPersonHands.cs` | Owner-only viewmodel built at runtime from `Sunny_v004.fbx`. Only the `ForearmHand_L/R` meshes are shown, posed by two-bone IK. Poses: idle, walk/run sway, jump/land, press, reach, pickup/place, carry crate, carry bag, inspect (magnifier), repair (wrench), sort and restock. |
| `Assets/FirstPerson/Scripts/AvatarPresentation.cs` | Every Sunny copy. Cadence scales with speed, footsteps follow travelled distance, plus an airborne tuck, landing squash, idle breathing and a remote head turn toward the replicated look direction. |
| `Assets/FirstPerson/Scripts/PauseMenu.cs` | Local pause: Resume, Settings, Controls, Leave Session, Quit Game. Uses the handbook paper theme. |
| `Assets/FirstPerson/Scripts/GameSettings.cs` | PlayerPrefs-backed preferences (`bau.settings.*`) and their engine application. |
| `Assets/FirstPerson/Scripts/TownMinimap.cs` | Live paper minimap: compact HUD view, full map (M / handbook) and market floor plan. |
| `Assets/FirstPerson/Scripts/TownAudio.cs` | Synthesized effect palette: steps, land, pickup/place, tool, paper, click. Uses no audio assets. |
| `Assets/FirstPerson/Editor/Phase7ABuilder.cs` | Idempotent menu steps for the prefab, collision, door, vans and map bake, plus the Phase 7A build menus. |
| `Assets/FirstPerson/Editor/Phase7AValidation.cs`, `Phase7ACapture.cs` | Static and Play Mode validation, and first-person renders. |
| `Assets/FirstPerson/Art/ContactShadow.png/.mat`, `Tool_Metal.mat`, `Tool_Handle.mat` | Van contact shadow; viewmodel tool materials in the town palette. |
| `Assets/Town/Resources/Town/Map/DaylightTownMap.png` (768×672), `DaylightMarketMap.png` (520×600) | Stylized top-down maps baked once in the editor at 24 and 40 px/m. Not power-of-two scaled. |
| `Scripts/test_phase7a_network.py`, `Scripts/launch_phase7a.ps1` | Two-process Phase 7A harness; launcher. |

### Modified files

| Path | Change |
|---|---|
| `Assets/Prototype/Scripts/PlayerInteractor.cs` | Adds optional `aim` + `aimConeDegrees` (32°). A target inside the view cone wins over a nearer one. With `aim == null` (all earlier scenes) behaviour is unchanged. |
| `Assets/Prototype/Scripts/PrototypeInput.cs` | Adds `Blocked` (suspends gameplay input) and `EscapeHandledExternally`. `GameplayActive` now also requires `!Blocked`. |
| `Assets/Multiplayer/Scripts/NetworkPlayer.cs` | Owner enables first person, hands and pause when the prefab has them, otherwise keeps the orbit camera. `InputServerRpc` gains `pitch`. New `LookYaw`/`LookPitch` NetworkVariables. A standing Sunny turns to its look yaw on the server. Adds `CarryingLoad` and gesture/audio hooks on press, server feedback and restock RPC. |
| `Assets/Multiplayer/Scripts/NetworkTestDriver.cs` | Opt-in harness only: first-person/pause/map/settings report, actions (`look`, `pause`, `pausepage`, `map`, `settings`, `display`, `reloadsettings`, `leave`, `realinput`, `capturecursor`), more virtual keys (Esc/W/E/M, held), and teleports that face +Z. Inert without all three test flags. |
| `Assets/Town/Scripts/TownHandbook.cs` | Uses `TownMinimap` (with fallback to the old diagram if the maps are missing). Adds the M key and `CloseFromEscape()`. Keys and clicks are ignored while paused. The facing shown is the first-person yaw. The HUD hint lists M and Esc. |
| `Assets/Town/Prefabs/SunnyTownPlayer.prefab` | Adds FirstPersonView, FirstPersonHands (`armsSource` = `Sunny_v004.fbx`), PauseMenu and AvatarPresentation. Camera near clip is 0.03. Still exactly one Camera and one AudioListener. |
| `Assets/Town/Scenes/DaylightTown.unity` | Collision, door, van and contact-shadow changes below. |
| `Scripts/test_phase5_network.py`, `test_phase6_network.py`, `test_phase6_guidance.py` | Optional `--exe` (defaults unchanged) so the suites can run against later builds. |

## First-person architecture

```
Owner process                                   Host (authority)
FirstPersonView ─ yaw/pitch ─► NetworkPlayer.InputServerRpc(move, run, jump, yaw, pitch, E held)
   │  camera = existing child Camera                └► ThirdPersonMotor.Step + CharacterController (unchanged)
   │  body renderers → ShadowsOnly                     LookYaw/LookPitch, Speed, Grounded … → all copies
   ├► PlayerInteractor.aim (view-cone targeting) ─► E → NetworkShop.Execute (range + LOS, unchanged)
   ├► FirstPersonHands  ◄─ reads Holder / HasBag / IsWorking / RestockClientRpc / FeedbackClientRpc
   └► PauseMenu → PrototypeInput.Blocked (local only; never Time.timeScale)
Every copy: AvatarPresentation (cadence, steps, air pose, remote head turn)
```

- **Ownership.** Only the owner enables input, camera, listener, hands and pause. Remote copies render the full Sunny. Each process has exactly one camera, one listener and one viewmodel, and the harness checks this in both processes.
- **Authority.** Movement, inventory and work remain server-side and unchanged. Hand gestures never send commands. Carry poses follow `NetworkShop.Holder` and `TownJobs.HasBag`, work poses follow `TownJobs.IsWorking`, and restock follows `RestockClientRpc`. The E press shows only a short press anticipation.
- **Look.** Mouse look is local and instant. The yaw also steers camera-relative movement as before.
- **Held loads.** The owner's `Interactor.carryAnchor` is moved in front of the view: `FirstPersonView.carryOffset = (0, 0.5, 0.5)` from the feet. The server only reads the anchor for presentation, and other processes still see the load at the walking body's anchor.
- **Wall safety.** There is no second camera. Hands pull back when a 0.08 m sphere cast finds geometry within 0.55 m. Viewmodel renderers cast no shadows and use `updateWhenOffscreen`.
- **Tuned values.** `rigScale 1.0`, `shoulderOffset (0, −0.24, 0.12)`, `restRight/Left (±0.21, −0.2, 0.38)`, FOV 74 by default (60–95 adjustable), pitch −78…80°.

## Animation changes

The original rig, clips and Animator controller are unchanged. `AvatarPresentation` adds the following at runtime:

- **Cadence.** `animator.speed` rises from 1 to 1.45 (walk) or 1.85 (run) with speed. The v004 clips imply ~0.17 m/s (walk) and ~0.59 m/s (run) of foot travel against game speeds of 2.2/4.2 m/s, so foot slide is *reduced, not removed* (see remaining issues).
- **Airborne.** A tuck (thighs/knees up, arms out) plus a landing squash, driven by the replicated `Grounded`.
- **Idle breathing.** A small chest motion in the otherwise single-frame idle clip.
- **Head look.** Remote head/neck turn toward the replicated look (±65° yaw, ±35° pitch).
- **Facing.** A standing Sunny turns on the host to face where its player looks. Moving Sunny still faces the movement direction, which avoids backward moonwalking because there are no strafe or backward clips.

Blender MCP was not needed. No clip, mesh or rig file was modified.

## Collision and noclipping: root causes fixed

The Phase 6 modeled kit replaced visuals but kept the old graybox collision. Measured with temporary mesh-collider probes against the modeled shells:

1. **Market door misaligned by 0.40 m.** The MarketShell frame opening spans x −8.91…−7.49 (centre −8.20), but the hinge, collision gap and entry trigger were centred on −7.80. Players could walk into the visible wall east of the door. The hinge and the `Enter Daylight Market` trigger moved 0.40 m west, and the front walls and lintel now match the opening exactly.
2. **Door interaction point floated 1.19 m off the door.** The `Handle` kept a local offset authored for the old scaled panel (1.4×2.2×0.12). After the panel scale became 1, an open door's point sat in the vestibule, captured first-person aim, and E closed the door on the player. The handle now sits on the leaf (`(0.45, −0.15, −0.12)` local). The door art was also mirrored, with its handle on the hinge side; it is flipped visually only, with the collider untouched.
3. **Walls thinner than the modeled facades.** Market and garage front, side and rear colliders were refitted to the measured shell surfaces (market facade outer face 3.20; garage inner faces 4.61, 11.39, 9.37).
4. **17 solid props had no collision.** Box colliders were added (fitted from the mesh, 2% inset) to the waste cart, crate stacks, parts workbench, waste bin, tool racks, canopy posts, parcel carts and the square signpost. Two props that host interaction points (the parts workbench and the garage parts-slip crate) are on **Ignore Raycast**: they still block movement but never block interaction or line of sight.
5. **Camera clipping.** The eye sits at the capsule centre (radius 0.24) with a near clip of 0.03. Every collision walk also checks that a 4 cm sphere at the eye touches no geometry, including the modeled shells.

All three workplaces were exercised in the harness: the market entrance and interior, the garage bays and the sanitation yard. The approved shop interior's colliders were not changed; it is checked through interaction accessibility and the regressions.

## Mechanic vehicles

Both `Town Workplaces/Service Vehicle N` roots now sit on the workshop floor (y 0.05) with uniform scale 0.85; they had been 0.86/0.84/0.84. The van child is offset so the lowest tyre rests 4 mm into the floor (measured tyre bottom y = 0.046). The box collider is fitted to the body (top 1.51; previously 1.635, above the 1.48 roof). A soft `Contact shadow` quad sits under each van, because the covered garage gets no direct sun shadow and that absence was the main "floating" cue. Repair and inspection points were unchanged: each has a free standing spot within the server's 1.8 m range with clear line of sight, and both repair and inspection were completed in first person in the harness.

## Minimap

- **Baked once.** Maps are rendered once in the editor (Business as Usual → Phase 7A → Bake Minimap) from an orthographic camera, then posterized, tinted to the handbook palette and inked with Sobel edges. Runtime only draws UV windows, so there is no per-frame texture work. World signs and notices are hidden during the bake.
- **Compact HUD.** A player-centred, north-up 15 m window. The "you" stamp sits on a mustard disc and rotates with the camera yaw. Landmarks are labelled MARKET, GARAGE, YARD, SQUARE and ALLEY. Objectives pulse when active and clamp to the edge when off-window. Inside the market it switches to the interior floor plan.
- **Expanded map (M).** The whole region with a legend. It does not release the mouse.
- **Objectives** come from the existing personal `TownGuidance` (job-specific inspect/repair/parts/bag/bin/shelf markers). There is no new quest data.
- **Privacy.** A colleague marker appears only within 14 m *and* with clear line of sight, i.e. only someone you could already see. The map never reveals hidden roles or the private information of other jobs. Future criminal objectives must not be added to `TownGuidance` for other players.

## UI and settings

- **Pause is local.** It releases the cursor, sets `PrototypeInput.Blocked`, and the host keeps simulating (`Time.timeScale` is untouched). Resume relocks the cursor unless the staff desk needs it.
- **Leaving or quitting while carrying** first asks the authority to return the load (waiting up to 1.2 s), then disconnects. Server-side disconnect recovery remains the backstop.
- **Settings** are master volume, sound effects, mouse sensitivity, invert-Y, field of view, display mode (fullscreen/windowed), resolution, graphics quality (labelled "Performance" and "High" for the project's two levels) and frame rate (VSync, 30/60/120/144 cap, unlimited).
- **Music volume is deliberately hidden**: the project has no music, and only working settings are shown (`PauseMenu.MusicAvailable`).
- **Graphics options apply only in players.** In the Editor they would rewrite `QualitySettings.asset`.
- **Controls** card in the same paper style.

## Test results

All checks are automated. No human keyboard/mouse playthrough or two-PC LAN session is claimed.

| Suite | Result | Evidence |
|---|---:|---|
| Phase 7A static (collision rays, door/frame, props, vans, accessibility, floating points, settings, map, prefab) | 22/22 | `Docs/Diagnostics/Phase7A/Editor/static-validation.json` (the earlier failing run of the floating-point check is in `static-validation-before-handle-fix.json`) |
| Phase 7A Play Mode, editor NGO host (ownership, 14 collision walks with eye-clip probe, aim targeting, pause, minimap regions) | 29/29 | `Docs/Diagnostics/Phase7A/Editor/playmode-validation.json` |
| Phase 7A two-process 1280×720 (ownership both sides, look replication, collision in build, all hand poses from server state, no double consumption, remote walk/jump, minimap tracking and line-of-sight colleagues, real Esc/W input isolation, local pause while colleague moves, settings persistence/VSync/windowed resolution in player, leave-while-carrying) | 43/43 | `Docs/Diagnostics/Phase7A/TwoProcess/validation.json` + PNGs |
| Phase 7A two-process 1920×1080 | 43/43 | `Docs/Diagnostics/Phase7A/TwoProcess1080/` |
| Phase 6 complete town on Phase 7A build | 35/35 | `Docs/Diagnostics/Phase7A/Regression/Phase6Full/` |
| Phase 6 staffed cooperation + navigation | 20/20 | `…/Regression/Phase6-staffed/` |
| Phase 6 market entrance/exit | 4/4 | `…/Regression/Phase6-entry/` |
| Phase 6 guidance | 22/22 | `…/Regression/Phase6Guidance/` |
| Phase 5 shop multiplayer (rebuilt, still third person) | 32/32 | `…/Regression/Phase5Shop/` |
| Phase 3 / 4 / 4.1 / 4.2 editor validators | 19/19, 87/87, 57/57, 65/65 | `…/Regression/Phase3`, `Phase4`, `Phase4.1`, `Phase4.2` |

Phase 3–4.2 validators write into their accepted folders. Fresh results were archived under `Phase7A/Regression`, and the accepted files were restored byte-for-byte from the pre-phase backup. Before/after images are in `Docs/Diagnostics/Phase7A/Before` and `After`; first-person previews are in `After/` and the TwoProcess folders (`*-fp-*.png`, `pause-*.png`, `map-expanded.png`, `remote-host-*.png`).

The market-entry regression failed once against an intermediate build. That failure led to fix 2 above; it was not worked around in the test.

## Remaining issues

- **Client input latency.** There is no client-side prediction, so the client feels movement latency of about one round trip plus interpolation. Look is local and instant. This is more noticeable in first person than it was in third person.
- **Foot slide** is reduced by cadence scaling but remains, because the v004 clips are very short-strided. The fix is new longer-stride walk/run clips in Blender, which also needs another look at the deformation limits recorded in `locomotion-v004.md`.
- **Procedural hands.** There are no authored first-person clips, and the pose quality is "readable", not hand-keyed. Only forearm/fist meshes are shown: no sleeve cuff and no finger animation.
- **Remote jump.** The pose is subtle. There are no jump/land clips.
- **Display options** were automated for windowed resolution only. Exclusive fullscreen on real monitors and multi-monitor setups need a human check.
- **Two-PC LAN, human comfort/sensitivity testing and long-session performance** were not performed.
- **Earlier scenes keep the third-person Phase 3–5 controls:** `TownPrototype`, `ShopkeeperShift` and `ShopkeeperMultiplayer`. Esc there still only releases the mouse.
- **Build side effects.** Building rewrites `ProjectSettings.asset` (Input System `preloadedAssets`) and `Shop_Paper.mat`/`Shop_Screen.mat` (`_EMISSION` keyword list). Saving the player prefab makes NGO re-add it to `DefaultNetworkPrefabs.asset`. All were restored to their pre-phase bytes after the last build; expect them to reappear after future builds.
- **Editor log.** The running Unity Editor is logging to `Docs/Diagnostics/Phase6/editor-recovery.log` (its Phase 6 launch argument), so that file keeps growing while the Editor is open. A copy from before this phase is in the backup.

## Phase 7B constraints: what Codex must preserve

1. **Do not move, rename or delete gameplay objects.** `NetworkShop` target IDs are hierarchy paths, so keep everything under `Town Workplaces/*`, `Graybox Town/Shop/Shop Door/Hinge/Door Panel/Handle`, the `Daylight Market Interior/Functional Fixtures` stations and `Shop Shift`.
2. **The `Graybox Town` children are now the town's collision.** Their renderers are disabled and their transforms are fitted volumes. When replacing building art, refit these walls to the new meshes. `Phase7AValidation` probes `MarketShell` and `WorkshopShell` by name; update those names if the shells are replaced.
3. **Keep the market door aligned.** Collision gap, hinge, door leaf and the `Enter Daylight Market` trigger must match the doorway (`market_door_gap_matches_frame`, `market_door_leaf_and_entry_centered_in_frame`).
4. **Every new solid prop inside the playable bounds (±15.8 × ±13.8 m) needs a simple collider.** A prop that contains an interaction point goes on Ignore Raycast. Interaction points must stay within 0.35 m of their own object, with a free standing spot in 1.8 m range and clear line of sight.
5. **Vehicle replacements** need a ground pivot, uniform scale, tyres on the floor (y 0.05) and a body-fitted collider. Keep repair/inspection points outside the collider and reachable.
6. **Re-bake the minimap after any town art change** (Phase 7A → Bake Minimap), and keep `TownMinimap.TownWorld` / `MarketWorld` in sync if bounds change.
7. **Keep SunnyTownPlayer as is:** one Camera and one AudioListener, plus FirstPersonView, FirstPersonHands (armsSource), PauseMenu and AvatarPresentation. Do not add overlay cameras without updating the ownership checks (`cameras == 1`). If the CharacterController (h 1.2, r 0.24) changes, revisit `eyeHeight`, `carryOffset` and the viewmodel offsets.
8. **Do not change authority code** in `NetworkShop`, `TownJobs`, `InventoryLedger` or the command/range checks. Keep presentation (hands, audio, map) read-only.
9. **Do not spend Meshy credits without approval.** None were spent in 7A.
10. **After 7B, run all of the following and restore validator evidence and build side effects as described above:**
    - Phase 7A → Run Static Validation, then Play Mode Validation.
    - `python Scripts/test_phase7a_network.py`
    - The Phase 6 suites with `--exe Builds/Phase7A/BusinessAsUsual.exe`: full, `--scenario staffed`, `--scenario entry`, and guidance.
    - Phase 5 with the Phase 5 regression build.
    - The Phase 3/4/4.1/4.2 editor validators.

## Tools and preservation

- **Unity MCP** was used throughout: scene/prefab inspection and edits through the idempotent builder, Play Mode, builds and validators.
- **Graphify** was used to navigate; `graphify update .` was run after the code changes (2,487 nodes, 4,767 edges).
- **Blender and Meshy MCP** were available but not needed. Meshy credits spent: 0.
- **Backup.** Before any edit, `Assets`, `ProjectSettings`, `Packages`, `Docs` and `Scripts` were copied to `D:\Business as Usual Backups\pre-phase7A-2026-10-11` (includes `git-status.txt` and the index tree hash).
- **Git index unchanged.** The index tree hash is identical before and after (`77fff87b7cbe089ce03bbd6725231e0f38c8a062`). No commit, push, merge or reset was made. Original Sunny art, clips, the TownPrototype/Shopkeeper scenes and accepted Phase 3–6 evidence are unchanged.
