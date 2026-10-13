# Phase 4.2 — Approved Staff Edition UI integration

The six approved retro-handbook designs are integrated into `ShopkeeperShift`. The working scene and its serialized `ShopHUD.shift` / `ShopHUD.player` references are retained. Unity MCP responded successfully before implementation; the initial scene was clean and the editor was idle. Temporary discovery failures during assembly reloads recovered without restarting Unity or discarding a scene.

## Implementation

| Surface | Integrated behavior |
|---|---|
| Compact HUD | Identity card, carried quantities, complete current order, active work/task totals, exact revenue and receipt count. The normal center remains clear around Sunny. |
| Employee handbook | Six tasks with exact progress, inventory accounts, selected page tabs and completion rules. Stock remaining at the dock gets an additional conditional stockroom row. |
| Customer order slip | All requested products, basket/requested counts, unit prices, line values, unpaid order value and missing quantities. Mara, Eli and Jo were captured separately. |
| Printed receipt | Selectable receipt list; IDs, customer/order, actual UTC timestamp, quantities, unit/line/total amounts, paid stamp and serrated paper. All three receipts were exercised. |
| Interaction prompt | Closest production interaction, E cue and shelf/crate/basket context. The existing restocking activity replaces the action while busy. Rejection/status messages appear in a temporary opaque paper note at the right edge. |
| Shift report | Revenue, customer/task totals, stability contribution, receipt ledger, remaining stock, exit instruction and existing export-failure feedback. Opens once upon closure and can be dismissed. |

The existing IMGUI framework is retained as proposed in the approved audit. `ShopUITheme` centralizes palette, typography, paper/key/tab/illustration drawing and cached styles/textures. `ShopUIView` formats a read-only snapshot when ledger/order/task/message state changes. Money is formatted from integer cents. GUI matrix/color changes are restored after drawing. No hierarchy or textures are rebuilt each frame.

Original bag, packaging, paper grain and stamp artwork was exported from the approved mockup primitives. The four approved font files and their SIL OFL notices are bundled unchanged under `Assets/Shopkeeper/UI/Resources/Handbook/Fonts`. Source mockups, review gallery, design documentation and fonts in `Docs/UI/Phase4.2` remain unchanged.

Paper enters with a 150 ms, 12-reference-pixel easing movement. Stamps settle over 180 ms. The `ShopHUD.reducedMotion` inspector option makes these instantaneous. These effects do not initiate inventory transactions or alter the Phase 4.1 skeletal reach.

## Controls and scaling

Tab retains its receipt-book open/close binding; it closes any expanded page. Esc still releases the cursor. With the cursor released, click the task card, order card, revenue card, handbook tabs or receipt list to browse. UI consumes clicks over its own bounds so they do not recapture the cursor. Clicking outside restores the existing capture behavior. WASD, mouse look, sprint, jump, E and Q retain their production actions. Opening a page does not change cursor mode or pause gameplay; captured-cursor gameplay remains active until the existing Esc action releases it.

Layout uses the approved 1920 × 1080 coordinate system and centered uniform scaling. All six states were rendered at **1920 × 1080 and 1280 × 720**. Layout bounds were additionally checked for 1600 × 900 and 2560 × 1080; these additional sizes were not screenshot-tested. At 1280 × 720, 24 px body text becomes 16 px and 27 px quantities become 18 px. Microcopy and contextual duplicate counts are smaller. Supported receipt contents fit at full size; a scroll area handles more than two future receipt lines, but those future receipts were not tested.

## Validation

| Suite | Result | Evidence |
|---|---|---|
| Phase 4.2 live UI | **65/65** | [UI checks](Diagnostics/Phase4.2/ui-validation.json) |
| Phase 3 regression | **19/19** | [Fresh gameplay report](Diagnostics/Phase4.2/Regression/Phase3/playmode-validation.json) |
| Phase 4 gameplay | **87/87** | [Fresh gameplay report](Diagnostics/Phase4.2/Regression/Phase4/playmode-validation.json) |
| Phase 4.1 polish | **57/57** | [Fresh polish report](Diagnostics/Phase4.2/Regression/Phase4.1/polish-validation.json) |
| Phase 4 independent verifier | Passed | [Conservation and 90 Prototype file hashes](Diagnostics/Phase4.2/Regression/Phase4/independent-verification.json) |

The UI suite uses live Unity Play mode, production interactions, synthetic Input System keys/mouse and queued Game view GUI clicks. It checks Tab behavior, actual handbook/tab and receipt clicks, released/captured cursor conflicts, unchanged gameplay input/time, view refreshes, partial baskets, all order values, receipt arithmetic, repeated browsing without transactions and shift completion/dismissal. It also clears nonserialized presentation caches/actions and re-enables the component to verify recovery. Phase 4.1 separately retests every shelf, movement interruption and repeated interactions. This is automated editor validation and visual inspection; **no human usability testing or new standalone player build was performed**.

The first UI run retained in [ui-validation-first-pass.json](Diagnostics/Phase4.2/ui-validation-first-pass.json) was 60/63: a tiny negative floating-point layout offset was fixed, and two test assumptions were corrected because the second E after delivery legitimately selects the newly available crate. The next 64/64 run is retained in [ui-validation-second-pass.json](Diagnostics/Phase4.2/ui-validation-second-pass.json).

A later editor assembly reload while Play mode continued cleared the nonserialized drawing cache, causing `DrawHUD` to raise a null-reference error. Play mode was stopped; initialization now recreates the theme and Tab action in `OnEnable`, with a guarded drawing fallback. A dedicated recovery check was added, and all live suites were rerun on the final version. This is UI cache recovery; it does not add gameplay save/restore behavior for editor hot reload.

The [final Console check](Diagnostics/Phase4.2/console-final.json) reports **0 errors and 0 warnings** after the final suites and animation capture. Unity is stopped on the clean `ShopkeeperShift` scene, with `ShopHUD.shift` still referencing `Shop Shift`, `ShopHUD.player` referencing `SunnyPlayer`, all four fonts available, and zero missing script components. The Game view was restored to its initial free-aspect setting.

`Scripts/verify_phase3.py` still stops at line 52 because it asserts an empty Git index. Your pre-existing staged changes were deliberately preserved, so that repository-state assertion fails. Its preceding source/export hashes, fresh 19 gameplay checks and project/input checks completed. The script was not modified, and later assertions were not claimed as run. See [verifier log](Diagnostics/Phase4.2/phase3-artifact-verifier.log).

## Visual comparison and practical differences

Use the [approved-versus-Unity gallery](Diagnostics/Phase4.2/review.html) or [six integrated screenshots](Diagnostics/Phase4.2/six-integrated-overview.png). The gallery includes actual 1920 × 1080 and smaller images, plus before/after screenshots of the same fresh-shift state, Sunny position and camera. [PNG dimensions](Diagnostics/Phase4.2/screenshot-dimensions.json) are verified from the files.

The approved palette, fonts, main card positions, binding spine, illustrations and receipt styling are reproduced. Remaining differences are explicitly:

- Unity font baseline/rasterization and scaled icons differ slightly from Pillow’s mockup rendering. Small keys, pills and tab corners are square rather than the mockups’ small rounded corners; main paper cards retain rounded corners.
- Receipts add two small paper navigation buttons at the right. Expanded order pages add a close/cursor hint. The needs-items badge is wider to accommodate readiness text.
- Status/rejection notes and the conditional stockroom inventory row extend states not illustrated in the six approved examples. The center remains clear during ordinary gameplay.
- Receipt timestamps are from the new live session. Actual shelves update as stock is placed and collected; the mockups reuse a static unstocked background. These differences represent actual gameplay state.
- The mockups’ bottom “UI PROTOTYPE” review strip is absent from the game.

Additional captures show [Eli’s order](Diagnostics/Phase4.2/order-eli-1920x1080.png), [Jo’s order](Diagnostics/Phase4.2/order-jo-1920x1080.png), [stockroom quantities](Diagnostics/Phase4.2/inventory-stockroom-1280x720.png), [rejection feedback](Diagnostics/Phase4.2/status-incomplete-order-1280x720.png) and [busy stocking](Diagnostics/Phase4.2/busy-stocking-1280x720.png). The [paper/stamp preview](Diagnostics/Phase4.2/paper-stamp-preview.gif) uses actual Game view frames; its timing metadata is saved separately.

The preview contains 15 captures at 1280 × 720 over approximately 0.66 seconds, beginning at panel opening, with five distinct rendered frames. The GIF adds a 900 ms final review hold; [capture metadata](Diagnostics/Phase4.2/animation-preview.json) distinguishes this hold from the recorded animation timing.

## Preservation and reproduction

The [workspace comparison](Diagnostics/Phase4.2/workspace-after.json) compares every pre-existing Unity asset, project setting and approved design artifact to its pre-implementation hash, and compares staged path/blob/mode entries. The only intended pre-existing asset change is `ShopHUD.cs`. The new capture helper was created before the snapshot and is annotated separately in that comparison. Scenes, Sunny, Prototype assets/controllers, domain/shift/station calculations, shelf text corrections and restocking source/assets remain unchanged. Previous diagnostics were backed up, new regression evidence was archived under Phase 4.2, and the original diagnostic files were restored.

The final comparison confirms unchanged staged entries. All 103 backed-up Phase 3, Phase 4 and Phase 4.1 diagnostic files were checked against their restored originals, with no differences. See the [final validation summary](Diagnostics/Phase4.2/validation-summary.json).

Run `Business as Usual / Phase 4.2 / Validate UI and Capture States` in Unity from a clean, stopped scene to repeat the live UI suite. Screenshot capture uses Unity’s Game view fixed-resolution support and `ScreenCapture`, including IMGUI. The editor-only helper follows the [Unity GameView reference](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GameView/GameView.cs) and [Game view input-event API](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/EditorGUIUtility.bindings.cs). It does not save a scene.

Graphify assisted the dependency audit and its AST graph was updated after code changes. No Meshy generation, paid asset, multiplayer work, commit or push occurred.
