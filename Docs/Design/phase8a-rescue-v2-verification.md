# Phase 8A Rescue V2 — verification and limits

The rescue is implemented in a separate Windows development build. [Launch/playtest instructions](phase8A-playtest.md), [native before/after gallery](../Diagnostics/Phase8ARescueV2/review.html), [research](phase8a-ui-ux-research.md), [design system](phase8a-ui-design-system.md), and [editable Figma](https://www.figma.com/design/J2QJX5ZjjCNuMgbK2achnI).

## Delivered

One-window solo launch; illustrated job selection and induction; compact clipboard/HUD/map/prompts; targeted work slips, inventory and receipts; pause/settings/report; Meshy T2 wagon, engine, open collection cart and carton; Blender-fitted and articulated bonnet/wheels; smaller grounded workbenches; real battery handling; matched stocking and checkout; three-batch cart collection/sorting; seven authored glove gestures with tools; bounded remote foot planting; shared server authority and labelled solo services. Original scenes, materials, prefabs and prior builds are retained. Ordinary jobs and partial evidence remain the core identity.

## Native results

| Suite | Passed | Evidence |
|---|---|---|
| FinalUI720Validated | 23/23 | [FinalUI720Validated/checks.json](../Diagnostics/Phase8ARescueV2/FinalUI720Validated/checks.json) |
| FinalUI1080Validated | 30/30 | [FinalUI1080Validated/checks.json](../Diagnostics/Phase8ARescueV2/FinalUI1080Validated/checks.json) |
| FinalMechanic | 17/17 | [FinalMechanic/checks.json](../Diagnostics/Phase8ARescueV2/FinalMechanic/checks.json) |
| FinalRetailGrounded | 11/11 | [FinalRetailGrounded/checks.json](../Diagnostics/Phase8ARescueV2/FinalRetailGrounded/checks.json) |
| FinalSanitationValidated | 10/10 | [FinalSanitationValidated/checks.json](../Diagnostics/Phase8ARescueV2/FinalSanitationValidated/checks.json) |
| FinalTwoProcess | 13/13 | [FinalTwoProcess/checks.json](../Diagnostics/Phase8ARescueV2/FinalTwoProcess/checks.json) |
| FinalFourProcess | 20/20 | [FinalFourProcess/checks.json](../Diagnostics/Phase8ARescueV2/FinalFourProcess/checks.json) |
| FinalArtGrounded | 6/6 | [FinalArtGrounded/checks.json](../Diagnostics/Phase8ARescueV2/FinalArtGrounded/checks.json) |

Physical-domain assertions: **367**. Legacy regression: **10,385** including four workflows and 10,000 seeded commands. Build: **Succeeded**, 0 errors, 486 warning(s). Build/assembly fingerprints are in validation-summary.json and per-run metadata. Mechanic and four-process evidence precede the final staff-screen routing and diagnostic sampling-only revision; later suites record the final assembly. Earlier failed attempts remain as history and are excluded from this acceptance table.

Tests use real Windows standalone players. Fixtures teleport to safe work points; interaction either uses virtual keyboard/mouse devices belonging to that player process or the authenticated production RPC boundary. Solo mechanic checks real W collision against car and bench and actual mouse clamp turning. Retail crosses the doorway carrying the same parcel. Sanitation pushes a loaded cart, selects actual batches and files them. Two/four processes check conserved custody, replay rejection, disconnect recovery, profession privacy, and exactly one owned camera/listener/input/canvas. Remote walking exercises the presentation foot plant; it is not proof that sliding is eliminated at all speeds.

Native 720p/1080p screens and 1366×768 resizing report no text-bound/overflow defects in tested states. Six 1920×1200 images are Unity offscreen compositions, not native desktop-window tests. Original PNGs are unmodified. GIFs are downsampled real capture sequences. Screenshots were visually inspected; automated bounds alone were not used to judge art. Final polish corrects cracker UVs on a separate mesh/material, resizes imported fixture wrappers only once, narrows the grip for stocked retail units, distinguishes municipal from human colleagues in waiting instructions, and reuses/disposes the highlight material. FinalRetailGrounded and FinalArtGrounded exercise the last assembly; the other suites retain their earlier fingerprints and source-revision context.

## Performance

Observed native single-player UI/resizing sample on NVIDIA GeForce RTX 3060 Ti, 11th Gen Intel(R) Core(TM) i7-11700F @ 2.50GHz, Direct3D12: 12000 sampled frames, median 3.26 ms, p95 6.94 ms. The final photographed frame reported 995 draw calls, 710 batches and 826,133 triangles. Rendering counters were available. This mixed menu/capture/resizing run is not a controlled GPU benchmark; do not convert it to a promised framerate. The first 12,000 post-warmup frame intervals are retained.

Diagnostics sample percentiles at most once per second and are opt-in for the test driver. Cart movement is replicated at 10 Hz independently of inventory revision; wheel motion follows measured displacement. The town-view camera updates at 10 Hz only while inside the store. The inherited town still has many materials and repeated props; broad batching/LOD/occlusion work remains outside this focused rescue. Multi-process frame times are contention measurements on one machine, not multiplayer hardware requirements.

## Remaining defects and limits

- **No human usability test.** Automated completion does not prove that an unfamiliar player understands the route or every affordance. The documented first-time-player protocol remains required before calling onboarding validated with players.
- **No cross-machine LAN test.** Two and four native processes use loopback on this PC. Firewalls, packet loss and real voice/social interactions are unverified.
- **Store transition.** A framed live town view replaces the blank doorway; walking crosses to the same town with conserved inventory. The interior remains offset, and the view is fixed rather than a seamless, parallax-correct architectural portal.
- **Art.** Primary props are grounded and scale coherent in inspected views. Meshy wagon texture/details are stylized and contain uneven generated paint; engine material treatment is matte. Authored gloves are segmented FK meshes, not a skinned hand rig or physics finger contact. Tools follow authored poses; attachments and generic sorting/checkout gestures remain approximate. Existing secondary store props and decorative packaging can still have awkward labels.
- **Remote animation.** Short standing/walking captures were inspected and foot planting is active. Running, jumps, slopes, network jitter and turn transitions need a dedicated animation playtest; no perfect-contact claim.
- **Figma limit.** Sixteen editable production screens and reusable components exist. Production icon/button fixes were applied, but the Figma Starter MCP quota blocked retrieving the final post-fix render. Earlier Figma screenshots are intermediate. Final native Unity layouts provide the inspected editable equivalent.
- **Prototype scope.** Three retail units, one repair, three waste batches and one workday are intentionally finite. Municipal services stand in for absent solo professions and use the same objects; they are not fully simulated NPC coworkers. Two humans without the third lead cannot complete the whole town through hidden services. The assistant supports existing tasks but is not a fourth full profession.
- **Build warnings.** The installed unrelated Pipeline package reports missing RuntimePipelineConfig and disables that package in player builds. The game's URP rendering is active in the recorded native images. Existing Sentis shader warnings appear when affected assets are rebuilt; the final build's raw warning count is recorded above. This rescue did not repair those unrelated packages.

## Credits and preservation

Meshy cumulative usage is **140/300 credits**, **160 remaining**, no reservations. Rescue added 65: wagon 15, engine 15, rejected closed-cart preview 5, open cart 15, carton 15. Rejected generation is charged and retained in the ledger. Blender authored glove clips, bonnet cleanup and wheel separation incurred no Meshy credits. [Ledger](../meshy-credit-ledger.json).

The pre-rescue baseline contains 6,834 hashes/editable originals at `D:\Business as Usual Backups\pre-phase8A-rescue-v2-2026-10-12`. [Preservation audit](../Diagnostics/Phase8ARescueV2/preservation.json) records changes and confirms index stage entries unchanged: **True**. No commit/push/reset/stage/discard was performed. Original scene/material/prefab hashes are unchanged. Build-only preloadedAssets mutation was restored from this baseline. The AST graph is refreshed separately without LLM API cost. [Asset provenance and imported triangle counts](../Diagnostics/Phase8ARescueV2/asset-provenance.json) distinguish generated sources from Blender cleanup and original project assets.
