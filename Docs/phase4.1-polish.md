# Phase 4.1 — labels and restocking polish

The approved shop geometry, Sunny model, Generic avatar, SunnyLocomotion controller, input bindings, domain ledger and shift calculations are preserved. No Meshy or paid generation was used. No commit, push, reset or staging action was performed.

## Label diagnosis and fix

Unity inspection found 192 legacy TextMesh renderers, no TMP text and no world-space Canvas. Every world label used the default GUI/Text Shader, transparent queue 3000, default sorting layer/order 0. Sign text is fixed to shelf/fixture label boards, fitted to their size and offset 12 mm toward the readable face. There is no billboard behavior.

The before capture reproduces distant black prices across Sunny's head, shirt and arms, as well as farther signs visible through shelves. The new shared `ShelfText.mat` uses `Business as Usual/Shelf Text`, an unlit URP font-atlas shader with alpha blending, backface culling, depth comparison `LEqual`, and no depth write. Opaque shelf boards, Sunny and walls now occlude the labels. Existing product, price, station and till text is retained, with all placements unchanged.

[Matching before/after comparison](Diagnostics/Phase4.1/label-before-after.png) · [Before](Diagnostics/Phase4.1/before-shelf-world.png) · [After](Diagnostics/Phase4.1/after-shelf-world.png).

These two world captures use the same camera, Sunny pose and empty starting inventory at 1920 × 1080. They exclude screen-space IMGUI. `current-hud-baseline.png` is a separately repainted Game view showing the existing interface, with all six tasks initialized. The early `before-gameplay.png` capture occurred before the Game view had repainted at the requested pose; it is retained for provenance, not used as the matching comparison. `after-hud.png` was captured during script iteration and is not the authoritative UI baseline.

## Restocking

`ShopRestockReach` is attached only to the shop scene's SunnyPlayer instance. Live inspection confirmed a Generic avatar with the named Spine2, RightArm, RightForeArm and RightHand bones. A lightweight two-bone reach blends over the Animator's evaluated upper-body pose and adds a five-degree upper-torso lean. It changes no leg bones, root transform, animation assets or locomotion parameters.

The reach lasts **0.65 seconds**: ease toward the shelf for 0.28 s, briefly hold, ease back over the remaining time. The real-stock row becomes visible at the reach peak. Existing movement remains available. Movement over the threshold cancels the overlay with a 0.12 s return; component disable restores the underlying bone pose immediately. Each frame restores the previous overlay before applying a fresh blend, preventing drift.

**Transaction boundary:** ShopShift.UseShelf still transfers stock synchronously exactly once when the shelf interaction is accepted. The animation is presentation only; it never calls inventory transfer, including on completion, cancellation or disable. ShopStockDisplay delays only visible placement until 0.28 s. A movement cancellation leaves the already accepted transfer intact, while Q returns only the crate's remaining contents. This preserves the approved domain behavior and deterministic transactions. Shelf interactions are rejected while the reach is active, including immediately after the final empty crate auto-returns, so a repeated E cannot become an unintended collection. The HUD displays a stocking activity label during this short lockout.

Preview: [Cola](Diagnostics/Phase4.1/reach-cola.gif) · [Tea](Diagnostics/Phase4.1/reach-tea.gif) · [Crackers](Diagnostics/Phase4.1/reach-crackers.gif). These are looping sequences of live camera renders; encoding gives an approximate preview, not a frame-rate benchmark.

Scene builders retain the new reach and stock-display wiring, and ShopArtBuilder reuses the depth-tested material for generated signs. The original SunnyPlayer prefab is unchanged. The stock row still updates only when counts change. No per-frame scene search or asset creation is added to the reach.

## Validation

| Suite | Result | Evidence |
|---|---|---|
| New live-time polish suite | 57/57 | [polish-validation.json](Diagnostics/Phase4.1/polish-validation.json) |
| Existing Phase 4 complete shift | 87/87 | [phase4-playmode.json](Diagnostics/Phase4.1/Regression/phase4-playmode.json) |
| Existing Phase 3 regression | 19/19 | [phase3-playmode.json](Diagnostics/Phase4.1/Regression/phase3-playmode.json) |
| Independent Phase 4 ledger/hash verification | Passed | [independent-verification.json](Diagnostics/Phase4.1/Regression/independent-verification.json) |

The polish suite uses actual Play-mode timing and production shelf interactions on all three SKUs. It checks delayed placement, twelve immediate repeated presses per shelf, one transfer per restock, full completion, auto crate return, deterministic movement cancellation, synthetic Q after interruption, disable/re-enable and final-shelf repeat protection. All inventory accounts conserve six units per SKU. The original Phase 4 validator now advances the presentation clock before its synchronous stock-row assertions; its original gameplay checks remain intact. It still verifies the complete shift and real exit-to-town scene transition.

Ninety protected Prototype files match the accepted preservation hashes. ShopDomain.cs, ShopShift.cs, Sunny assets, the player prefab and original locomotion sources are unchanged. Accepted diagnostic reports/screenshots were backed up before rerunning validators, the new results retained under Phase4.1/Regression, and only the validator-overwritten original artifacts restored to their saved bytes. Existing staged content remains staged. Git status refreshes cached filesystem statistics, so raw index-file hashes are not used to infer staging changes.

Unity automatically added eight default URP light-data records during camera rendering/scene serialization; those incidental records and references were removed without changing the original scene records. Unity also normalized the emission keywords of the paper/screen materials during baseline Play; their accepted keywords were restored. These are cleanup of this session's side effects, not shop changes.

`verify_phase3.py` verified source assets, Blender/export hashes, current live tests and project settings, then stopped at its legacy assertion requiring an empty staging area. This conflicts with the explicit instruction to preserve the existing staged project. The script was not changed and the staging area was not cleared. Its failure is a repository-state assumption, not a failing gameplay check; the actual Phase 3 suite passes 19/19.

The last successful Unity Console check found zero errors and warnings. An existing Unity AI relay connection error was present before work; no connector repair or package change was made. A temporary compile error in the new validator was corrected before final runs. After the final disk cleanup, Unity MCP stopped answering during the requested asset refresh/scene reload; its current live state could not be reverified. The Windows helper also returned an unavailable native-pipe error, so no desktop action was taken. The scene and source changes are saved, and the passing reports precede that final refresh. No editor process was terminated or restarted.

Validation is deterministic tool-driven Play Mode, with synthetic input where stated, not a human mouse/keyboard playthrough. No player build was produced during this polish pass. UI redesign integration and its responsive in-game checks remain pending visual approval.

## Review and reproduction

Unity menu: **Business as Usual / Phase 4.1 / Validate Polish and Capture Reaches**. The existing Phase 4 and Phase 3 validation menu items remain available. Camera preview capture is a diagnostic editor operation only.

The [Phase 4.2 review package](UI/Phase4.2/README.md) contains the UI audit, commercially usable font checks, design system and six original full-size mockups. The complete redesigned interface has not replaced the existing HUD. Approval of that visual package is the next gate specified in the request.
