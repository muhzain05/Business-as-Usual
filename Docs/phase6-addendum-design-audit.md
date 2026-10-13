# Phase 6 design alignment audit

Reference: the original eight-page `Working-Town Social Deduction — Game Design Doc.pdf`, dated October 4, 2026. Read in full, including the day-escalation diagram on page 2 and the seven-location map on page 7. Text extraction and rendered review pages are retained under `Diagnostics/Phase6Addendum/DesignReference`. This audit precedes implementation of the addendum.

| Design reference | Current divergence | Correction in this pass |
|---|---|---|
| Public profession with private task layers, pp. 2–4 | One TownJobs.Message supplies every player's expired staff-note fallback. | Personal guidance comes from the local assignment and work state. Action feedback goes only to the acting client. Shared productivity remains a separate public summary. |
| Professions control resources and information, pp. 3–4 | Mechanic and Sanitation play correctly, but their screens resemble lists of co-op chores. | Short illustrated job cards, role-specific resource slips and own records; a distinct shared shift ledger shows public task counts without an omniscient instruction feed. |
| Partial paper trails, pp. 4–5 | Full actor/UTC data and unrelated global record counts dominate presentation. | Keep immutable authority/audit data; show each profession's own paper trail with short time and work fields. No suspect labels, other-player tracking, private-secret fields or criminal claims. |
| Dense hub and legitimate crossover, pp. 3, 6–7 | The compact layout and real stock/waste dependencies are sound, but locations and next stops are difficult to read. | Keep the block, workplaces and existing resource flow. Use a north-up paper minimap, personal objective pins, clear repair-bay points and public signposts to support encounters at the market, square, garage and alley. |
| Work matters; public end-of-day report, p. 5 | Town Stability is awarded at shift closure, with little visible reward during work. | Add a live public productivity presentation derived from actual task progress and tactile local completion feedback. Preserve the tested shift rewards and economy. |
| Small, stylized, ordinary work, pp. 1, 7–8 | Generic shell modules and repetitive roof-heavy houses feel artificial; the HUD covers too much scenery. | Model a reusable building kit around retained collision footprints, improve warm daytime lighting and street character, and greatly reduce the normal HUD. |

The GDD's full game is six players/five days, while its first MVP is four players/three days. Phase 6 remains the authorized two-player, three-profession foundation. Secret personal objectives, random criminal assignment, cover-up routes, warrants/meetings, proximity voice and the complete day loop are still future scope. This correction prepares appropriate information boundaries and socially overlapping routes; it does not claim that social deduction is playable yet. The clinic, warehouse and apartments from the GDD remain future functional locations, not invented playable jobs in this pass.

## Implementation sequence

1. Preserve the accepted Phase 6 assets, evidence and exact Git index.
2. Remove the shared-instruction fallback; target private action feedback while retaining broadcast restocking animation.
3. Bind role-specific objective cards, a compact map and local world pins to existing authoritative state. Indicate real inspect/repair points; reveal faults only after inspection in mechanic guidance.
4. Use one compact HUD and shared handbook treatment for all three jobs. Retain approved paper, typography and controls; deeper instructions remain in the expanded book.
5. Add reusable modeled market/garage/house shells, clearer public street signs and pleasant daytime light without expanding the block or obstructing the proven interactions.
6. Run two-process functional/cooperation/navigation checks, add information and guidance checks, inspect screenshots at 1080p/720p, and rerun affected earlier regressions.

## Meshy inspection

Meshy's exposed generation schema supports Smart Topology with `meshy-t2`, triangle topology and direct 100–15,000 triangle targets; it does not expose the requested T2.5 identifier. Text mesh previews cost 5 credits on that route, with a further 10 credits for refinement/textures. The connector requires explicit cost confirmation before a credit-consuming call. The account read reported 1,095 credits and no reusable models in its default library. These read-only calls consumed no generation credits. A cohesive original Blender kit is the first option, using the existing shared palette and lower repeated-asset budgets. Paid generation will only be proposed if inspection shows a specific asset would improve the result; no automatic fallback to a different paid model is authorized here.
