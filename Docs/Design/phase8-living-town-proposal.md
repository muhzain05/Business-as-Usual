# Phase 8 proposal: ordinary work that creates stories

Status: design proposal for separate approval. No new job systems, physical inventories, criminal mechanics, personal secrets, NPC schedules, meetings or minigames are implemented by Phase 7B.

## References and scope

The primary reference is the original eight-page `Working-Town Social Deduction — Game Design Doc.pdf` (October 4, 2026), read in full. Its pillars take precedence over the inspirations: all suspicious actions need legitimate equivalents; evidence is partial; everyone works; innocents may lie; work matters; the map stays dense. The full concept is six players/five days; its proposed MVP is four players/three days. The current implementation is a two-process foundation, not a complete deduction round.

Supporting references: `../phase7A-handoff-to-codex.md`, `living-town-gameplay-brief.md`, the Phase 7B manual QA addendum, and the implementation paths below. Schedule I and PEAK are feel references supplied by the user, not instructions to copy their systems. Success means players make small choices, physically handle useful things and meet colleagues for believable reasons.

## Current gameplay-depth audit

This audit distinguishes executable behavior from scenery and future intent. Paths below are relative to `Unity/BusinessAsUsual/Assets/`.

| Priority | Finding from actual implementation | Player consequence | Proposed response |
|---|---|---|---|
| 1 | `Shopkeeper/Scripts/ShopShift.cs`: delivery transfers all 18 units in one action; a shelf action transfers all remaining units up to six. Customer products are picked one unit per E, checkout sells the entire basket. Three fixed orders and exact shelf requirements. | Movement and counting dominate; restocking has animation but little handling or judgment. Repetition increases without new decisions. | Let the player grasp a small product batch, orient it on a shelf, then scan and bag items. Keep ledger transactions authoritative and separate physical presentation from stock quantities. |
| 1 | `Town/Scripts/TownJobs.cs`: two preset vehicle faults/parts; hold E to inspect or repair at fixed stations; request/collect parts, file all eligible records and close. | Fault finding is a reveal followed by a prescribed wait. Players cannot diagnose, choose an intervention or manage bay priority. | Inspect two readable components, compare one test result, choose the repair and place one replacement. Start with one vehicle type and two faults. |
| 1 | `TownJobs`: sanitation picks a bag, holds E at its known matching bin, files sorted bags and closes after three records. Paper/Mixed/Metal are already supplied in bag state and guidance. | The correct bin is explicit; sorting provides almost no judgment. Bag carrying and route order provide the most physical/social part. | Physical cart staging, one bag at a time, choose route order, selectively inspect a visible item and sort by observable contents. Recover wrong sorting through a return tray. |
| 2 | The parts ledger already transfers warehouse → counter → mechanic → installed; a staffed shopkeeper must pack, while an unassigned role enables municipal reserve collection. Shop delivery exposes a market waste bag; repairs expose garage waste. | These are genuine causal cross-job dependencies and the strongest existing foundation. Staffing can also create a wait with little to do. | Preserve conservation, ownership and reserve fallback. Give the waiting worker independent inspection/cleanup work and a visible delivery shelf rather than a blocking prompt. |
| 2 | `TownJobs` exposes a logical `warehouse` account, but there is no traversable warehouse workplace. Homes are decorative beyond the established boundary. No errands use them. | Warehouse access and home visits are design intentions, not current social routes. | Reserve compact storage and an errand doorstep within existing footprints in a later phase; do not describe the scenery as playable interiors. |
| 2 | Square has clock, benches, noticeboard and one sanitation pickup. Alley has a pickup and delivery scenery. Shopkeeper work remains mostly in an offset interior at x≈−40; mechanics cross to the market locker. | Square/alley encounters are mainly sanitation-driven. Shopkeeper disappears from street sightlines for much of a shift. | Put a future returns/supplies handoff at the existing exterior market counter. Give each role a routine alley/storage errand. Keep offset interior portal association explicit. |
| 2 | `TownHandbook`, `TownGuidance`, `NetworkShop` provide private assignment guidance, public productivity, snapshots and own-role records. | The information boundary is usable. Records are work facts, not a functioning investigation or meeting loop. | Show partial paper trails with context and multiple plausible explanations. Never turn task markers into colleague trackers. |
| 3 | `ShopShift` and `TownJobs` complete fixed shifts, then offer little repeat work. There are no customer agents, vehicle movement, living resident schedules or personal objectives. | The attractive town can still feel static once chores end. | Vary a few ticket/order parameters and add optional ordinary errands after the first tactile prototype proves useful; use request cards, not NPC simulation. |
| 3 | `PlayerInteractor`, restock reach, local hand gestures, replicated locomotion and simple colliders already support readable body actions. | Presentation can make cooperative exchanges legible without a new physics/network framework. | Author handoff, brace, point and stumble recovery presentations. Keep physical comedy short and recoverable, with no compulsory ragdolls or griefing collisions. |

Some decisions exist today: sanitation route order and optional extra bags; inventory-aware order collection; mechanic inspection order; timing the staffed parts handoff. They are shallow because the answers and quantities are fixed. There is no claim that the current game already delivers the proposed richer loops.

## Expanded Shopkeeper

**Physical loop:** receive a labelled delivery → open one reusable crate → carry a small tray of products → place/fill visible shelf slots → assemble an order → pass products across a scanner → bag and hand off → retain a receipt. Use position/orientation and slot acceptance with forgiving snap zones. Scanning is a discrete spatial action, not a held progress bar. No NPC is required: order cards and another player's supply request are enough.

**Decisions:** (1) Replenish the fast-selling shelf or prepare the mechanic's requested part first. (2) Reserve limited shelf stock for a queued customer or provide an approved substitution. (3) Carry a mixed tray in one trip or keep products grouped for easier stocking. Prices, substitutions and stock caps need balance review; do not introduce punitive scarcity by default.

**Visit another workplace:** deliver a returns tray or confirm a part fit at the garage, and take packaging to sanitation staging. A normal delivery receipt sends the shopkeeper through the alley/storage point.

**Harmless mistakes:** rotate a carton badly, miss the scanner, put an item in the return tray or mis-shelve it. Corrections are quick; never silently duplicate, delete or charge twice for a product. Start with no breakage penalties.

**Private knowledge:** quantities received, requested item categories, incomplete purchases and missing stock. Customer-facing receipts omit permanent player identity and exact paths. Keep complete ledger events for server correctness, separate from the partial in-world record.

**Legitimate suspicious behavior:** entering storage with a sealed box, supplying gloves/tools, collecting a return in the alley, withholding stock for an ordinary request. Future hidden motives may use the same affordances, but are not part of this implementation.

**Cooperative/comic moment:** a colleague steadies a tray while the shopkeeper scans; a tilted carton spills into a recoverable tray. Avoid unrestricted physics item piles.

## Expanded Mechanic

**Physical loop:** read the service ticket → open a component cover → examine battery terminals or filter housing → perform one spatial test → choose the diagnosis → request a replacement → collect it at the market → unfasten/replace/refasten → run a simple completion check → file the service slip. Each step changes an observable component state. The wrench action uses a short turning gesture and alignment, not a long timer.

**Decisions:** (1) Diagnose first with a cheap quick test or spend an extra action on a more certain inspection. (2) Choose which bay to complete first while the other awaits a part. (3) Repair a recoverable connection or replace the component using a limited spare. Wrong diagnoses must be reversible, with clear feedback and no random invisible failure.

**Visit another workplace:** collect replacements from the market counter and carry old parts to sanitation. A tool return/loan later sends the mechanic to compact warehouse storage; do not give sole garage ownership of all tools.

**Harmless mistakes:** drop a captive bolt into a catch tray, fit a part backwards, choose a test on the wrong bay. Correct it locally; do not strand the vehicle or consume an extra ledger part before successful installation.

**Private knowledge:** actual faults, replaced parts, approximate signs of recent use. A vehicle service log can show a service/use interval without recording the driver's identity or criminal status. A delay might mean a wrong part, a favour or an honest repair problem.

**Legitimate suspicious behavior:** moving a service cart at closing, retrieving discarded parts, opening a tool cabinet, checking a vehicle nobody saw arrive. Vehicle driving is excluded from the first slice.

**Cooperative/comic moment:** a friend holds a panel steady, retrieves a bolt tray or passes a part across the bay. This helps throughput without making two-player presence mandatory.

## Expanded Sanitation

**Physical loop:** choose the route → steer a constrained collection cart → load two or three snapped bag slots → collect workplace packaging/old parts → unload at marked yard staging → optionally open one bag's inspection tray → sort a few visible representative items → route bag to its disposal cage → file a coarse manifest. Bags represent batches; avoid networked simulation of dozens of scraps.

**Decisions:** (1) Collect the close square pickup first or combine alley/market stops into one loaded route. (2) Use cart capacity for ordinary bags or wait for garage scrap. (3) Inspect an uncertain bag or prioritize throughput; inspection provides more context without guaranteeing guilt.

**Visit another workplace:** collect market packaging and garage replacement scrap; return a reusable crate to the market or a recoverable tool to shared storage. Route cards use the alley routinely, not only when something suspicious appears.

**Harmless mistakes:** clip a curb and tilt a load, place a bag in the wrong staging slot, sort a wrapper incorrectly. Catch trays/cart slots allow recovery. Avoid bag destruction, accidental evidence deletion and player trapping in this slice.

**Private knowledge:** collected material, approximate collection interval and selected discarded items. Bag contents may have ordinary explanations and may have changed hands. The public manifest lists source zone/material/time band, not a map of every holder.

**Legitimate suspicious behavior:** carrying opaque bags through the alley, opening waste packages, keeping a reusable discarded part, leaving at an unusual hour. Innocent work must routinely resemble future disposal routes.

**Cooperative/comic moment:** a friend catches a tilted bag or holds a gate as the cart squeezes past. Use a constrained cart and replicated discrete load slots before attempting free physics towing.

## Town location-to-gameplay map

Coordinates describe the current compact block, not a proposed larger town. The street and square should remain the quickest shared route. The market interior is an existing connected room at x≈−40, represented outside by the real entry trigger.

| Location | Current function | Phase 7B affordance / protected space | Future ordinary visits and ambiguous observations |
|---|---|---|---|
| Market exterior (x≈−8,z≈3–9) | Door/entry and parts pickup locker | Retained exact door/frame/collision, Meshy upper architecture, readable sign and open approach | Supply pickups, returns, handoffs; a box leaving the shop is ordinary |
| Market interior (x≈−40) | Checkout, shelves, stockroom, delivery/closing terminals and packing counter | Approved functional layout retained, no scenery in shelf/till reach envelopes | Scanning/bagging, trays, limited stock choices, receipts; packaging exits through a normal route |
| Garage (x≈8,z≈3–9) | Two service vehicles, inspect/repair points, request/file desk and parts workbench | Open ground-floor bay shell retained under Meshy roof, clear bay paint and existing storage racks | Component work, part handoffs, scrap pickup, contextual vehicle logs |
| Sanitation yard (x≈8,z≈−6…−10) | Route desk, three bins, bag sorting | Canopy/cart/storage retained; front loading markings leave route desk and bins reachable | Cart unloading, selective inspection, recoverable sorting; opaque bags are normal |
| Square (x≈0,z≈5–8) | Clock, benches, noticeboard, sanitation pickup and signpost | Gathering floor remains clear, clock chimes and evening light | Daily requests, informal meetings and witnesses; report/meeting system remains future |
| Alley (z≈10–12) | Sanitation pickup, carts/crates and rear-workplace access | Existing delivery staging marked; no new restricted door or criminal icon | Deliveries, packaging collection, returned tools; every role gets ordinary traffic |
| Warehouse / shared storage | Logical inventory account only | Alley staging and stockroom shelves can support an eventual compact transfer point; no new warehouse interior is claimed | Shared stock/tool retrieval with partial dispatch slips; allocate existing rear storage before adding any building |
| Homes / errands | Background cottages outside collision boundary | Meshy cottages, facades facing the town, evening window glows; remain scenery | Later repurpose one existing footprint/doorstep inside the block for an errand, with explicit approval; no town expansion |
| Main street/crossing | Fast circulation between yard and north workplaces | Existing collisions, curbs and crossing retained; restrained lamps, trees and clouds | Visible load transport and spontaneous help without public tracking UI |

Warehouse and home access need a later layout decision: turning background scenery into traversal space now would expand scope. The first Phase 8 prototype uses the existing exterior locker, stockroom and alley staging instead.

## Cross-job dependency matrix

Rows provide resources/information to columns. “Exists” means authority code already executes the dependency; all other entries are proposals.

| Provider → recipient | Shopkeeper | Mechanic | Sanitation |
|---|---|---|---|
| Shopkeeper | Own receipts and conserved stock exist | Requested battery/filter packed to real counter account exists; propose explicit tray handoff | Delivery creates market packaging bag (exists); propose scheduled staging and returnable crate |
| Mechanic | Parts request and consumption exist; propose fit confirmation/returned spare | Own fault/repair/service state exists | Repairs expose garage scrap (exists); propose recoverable tool return and capacity choice |
| Sanitation | Can already collect market waste once available; propose reusable packing returns | Can already collect garage waste once available; propose selected recovered item handoff | Collection/sorting/disposal records exist; propose cart and route choices |

Never require all three professions to be staffed for progress. Preserve the municipal reserve and disconnect recovery; add deterministic fallback request cards or pickup shelves for future handoffs. Cooperation should improve pace and information, while absent or slow colleagues must not deadlock a shift. No player can permanently lock another out of their job.

## Ranked high-impact improvements

1. **Physical product placement and scanner checkout.** Highest immediate gain because the most repeated current actions are shelves and order picking. Moderate implementation risk: finite slots and authoritative acceptance only. Validate delight and comfort before adding product types.
2. **Component diagnosis and replacement.** Adds real inference and distinctive mechanic skill. Moderate risk: two components/two faults, explicit reversible outcomes and single-part conservation.
3. **Visible cross-job handoffs.** Use existing parts and waste dependencies, shared counters and ordinary street routes. Low-to-moderate risk and high social payoff; asynchronous fallback prevents waiting from dominating.
4. **Sanitation cart, staging and selective inspection.** Distinct transport/route/capacity loop. Higher network/collision risk; use kinematic constraints and snapped slots first.
5. **Small ticket/order variation and optional errands.** Makes repeat shifts less memorized. Low-to-moderate risk once the physical loops are proven. No NPC schedules.
6. **Expressive cooperative gestures and recoverable mishaps.** Improves spectator readability and PEAK-like charm. Low risk for presentation; arbitrary ragdolls/player shoving have much higher risk and are excluded initially.
7. **Contextual partial records.** Necessary deduction preparation; avoid spending Phase 8 on surveillance before ordinary work is fun. Full server audit data remains separate from diegetic clues.
8. **Private secrets and criminal routes.** Essential to the eventual GDD, but explicitly deferred to a separately approved deduction slice after the ordinary work playtest.

## Proposed next gameplay phase

Approve a bounded **Phase 8A tactile-work vertical slice**, not all three expanded designs at once:

- One market tray with at most six slots; three existing product types; three short order cards; spatial scan and bag slots. No NPC customers or general free-physics inventory.
- One garage vehicle with two inspectable components and one replaceable part slot, reusing existing parts request/collection ledger flow. No driving or mechanical simulation.
- One visible market↔garage handoff and ordinary packaging staging for the existing sanitation role. Keep sanitation's current job available; prototype its cart separately only after this slice passes playtests.
- One reversible mishap per prototype (mis-shelved item / bolt catch tray), plus readable local/remote handoff animation.
- Existing handbook typography, paper cards and personal markers; no new mandatory HUD panel. Use current owner-local camera/hands and authoritative range/LOS commands.

Suggested checkpoints: (1) offline feel prototype with measured handling times and FOV comfort; (2) server validation and replica presentation; (3) two-process conservation, late join and disconnect tests; (4) four-person human playtest of two short shifts if the network/session scope is separately extended; (5) decide whether cart/route variation is the next slice. Four-player support is not assumed verified by current two-process tests.

Acceptance for this proposal: ordinary work produces at least one voluntary exchange per player/shift; at least two understandable decisions per implemented role; no long idle wait for a staffed supplier; errors recover in a few seconds without lost/duplicated stock; every route has an innocent explanation; peers can tell what a colleague is doing from body/load presentation. Measure time spent choosing/handling/travelling versus waiting, and ask players which encounter they remember. These are playtest goals, not current measured results.

**Explicit exclusions:** criminal assignment/actions, hidden personal goals, record tampering, warrants/meetings/arrests, proximity voice, NPC traffic/schedules, procedural city growth, new professions, driving, economy progression and skill trees. Designing partial evidence now does not authorize implementing a criminal system.

## Technical risks and reuse

`NetworkShop.Id` uses hierarchy paths: preserve functional names/parents or introduce a deliberate migration. `InventoryLedger` bundles already prevent partial stock transfers: physical slots must call accepted ledger mutations once and animate from acknowledged state. Do not make rigidbody ownership authoritative for item counts. `TownJobs` restores snapshot state and recovers disconnect loads; any tray/cart extensions need equivalent recovery and late-join state. Keep graphics/gesture clients read-only. Probe standing spots, collision, LOS and doorway clearance whenever props move. Record visible snapshots only for the local role; never expose secret targets through the minimap. Profile replica transforms before committing to general physics.
