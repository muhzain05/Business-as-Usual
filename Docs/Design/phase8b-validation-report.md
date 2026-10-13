# Phase 8B — validation report

Validation used actual Windows Unity players, not browser recreations of the game. Native screenshots and consecutive frame captures are in the [before/after gallery](../Diagnostics/Phase8B/review.html). Technical automation is explicitly separated from human usability and fun.

## Recorded outcomes

| Check | Result | Practical evidence |
|---|---|---|
| Unity build | Succeeded, 0 errors, 486 inherited warnings | `Docs/Diagnostics/Phase8B/build.json` and `editor-build13.log` |
| Original native baseline | 23/23 | Phase 8A rescue menu/work-slip captures before this implementation |
| Native UI, 1280×720 | 20/20 | Six menu actions, credits, separate sound controls across a restart, actual loading, role briefing, private practice, named direction card, current staff/shared copies access, handbook and pause/settings/display bounds |
| Native extended front end | 18/18 | Initial 1920×1080 request, actual 1366×768 resize, fullscreen/windowed changes, saved look/FOV/reduced-motion/quality, host hub, leave/restart and unavailable-host recovery |
| Solo shopkeeper | 10/10 | Same parcel through returns; three actual shelf/order/checkouts; selected paper; completed shared shift |
| Solo mechanic | 9/9, plus final-state checks | Real W movement stopped by the car, E hood, yard detour, load check, service correction, held mouse clamp turn, battery removal/installation, voltage check, ticket and report |
| Solo sanitation | 9/9 | Returns detour, real W cart travel, collection of the same three batches, reusable packaging errand, material sorting, manifest, selected copy and report |
| Two actual processes | 23/23 | Menu host/join, readiness, private payloads, stale/replayed/range rejection, observation scoping, real client mouse repair, shared chain, targeted papers, physical noticeboard, voluntary sharing, live rejoin and staff/work return |
| Four actual processes | 23/23 | Three leads + assistant; one camera/listener/input/native Canvas each; same information/authority checks; physical noticeboard, shared resources and report; private progress and staff/work navigation survive rejoin |
| Pure suspicious-day domain | 519 assertions | 120 assignment seeds cover all three lead professions; two lawful routes, declaration versus observation, useful cover, continuity, corrections, received-parcel recheck and legacy isolation |
| Existing domain regressions | 10,385 + 367 assertions | Four legacy workflows/10,000 seeded commands and the physical workday remain valid |
| Preferences cleanup | Passed | Pre-task sound and quality restored through native controls; look .12, FOV 92, reduced motion off |

Full solo captures used build 8. Build 9 changed observation scoping; build 10 repaired display/session cleanup; build 11 completed sealed-carton intake scoping and added the physical noticeboard target. Builds 12–13 replace mid-day job selectors with the current work slip/shared copies, explain fixed daily jobs, and keep a rejoining worker's staff navigation in the already-open day. The final 720p UI, two-player and four-player runs use build 13. Extended front-end captures use build 10. All batches record their assembly SHA-256; these differences are preserved rather than presented as one identical binary. The final artifact check independently checks completed solo states, all three private practice tracks, travelled cart distance, budget, links and protected files.

The test harness is opt-in per process. Fixture teleports place workers near targets; work goes through the production ownership/sequence/revision/range/line-of-sight/profession/material checks. Solo tests additionally drive native E/R keys, virtual Input System mouse torque and W movement. UI tests click real Canvas buttons. Multiplayer uses separate connected applications and actual NGO RPCs, with menu Host/Join clicks. It does not simulate four human conversations.

## Defects found and corrected

The native audit found a small title overflow and corrected its allocated height. An early recorder read UTF-8 as Windows ANSI, so glyph evidence appeared corrupted although the native screenshot was correct; its reader now uses UTF-8 explicitly. A duplicate scene recorder reset the persistent recorder's state; cleanup is now instance-owned. Solo auto-start initially retried every frame and hit the command rate guard; it now retries at a bounded interval.

Received sealed parcels needed a lawful recheck pickup to keep the same covert route available across professions. A client visibility test then exposed the hidden battery mesh's unused position. Sealed-item observations now follow the carton in transit and at garage intake. That also restores the client's ability to aim at the actual clamp after an observed delivery. Fullscreen labels were stale after Unity's deferred display change; the panel now refreshes when actual display state changes. Session shutdown explicitly removes the old manager before a new menu/day load. The noticeboard has its own physical interaction, so comparing copies is available before final sign-off.

Earlier failed compilation, diagnostic and network attempts remain distinguishable from passing checks in build logs and failure evidence. Passing assertions are supported by inspected native menu, private slip, returns/yard fixtures, open hood, four-worker hub, declaration copy and report captures. No screenshot was retouched to hide a defect. GIF previews are assembled from original native frame sequences and are not presented as full-rate recordings.

## Preservation and limits

The pre-task baseline contains 9,227 hashes and editable originals. Final audit preserves the exact staged index bytes and entries; original scenes/materials/prefabs remain unchanged. Nine existing baseline files change intentionally: seven runtime integration files, the diagnostic UTF-8 reader and the credit ledger. New code/assets/reports are additive. No staging, commit, push, reset or discard was performed. Graphify's AST graph was updated without an LLM call; semantic community labels were not regenerated.

The build still reports inherited shader/pipeline warnings, and some player shutdown logs report allocator/graphics resources. These are not counted as successful cleanup proof. Longer soaks, release-build cleanup, packet-loss/latency tests, cross-machine LAN and first-time human play remain outstanding. Diagnostic snapshot-byte counters are approximate publication counters, not measured wire bandwidth.

The resulting prototype has technically verified work and ambiguity foundations. Whether the guidance is understandable without coaching and whether the jobs and suspicion are enjoyable requires the four-person protocol in [the playtest guide](phase8b-playtest.md). See [remaining issues](phase8b-remaining-issues.md) before describing it as a finished deduction game.
