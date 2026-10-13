# Business as Usual — Living Town & Gameplay Depth Brief

**Document status:** Design reference and Phase 7B side brief — not authorization to implement every feature.  
**Intended repository path:** `Docs/Design/living-town-gameplay-brief.md`  
**Prepared:** October 2026  
**Primary references:** Original *Working-Town Social Deduction — Game Design Doc* (October 4, 2026); approved Daylight Market vertical slice; Phase 6 town/jobs; Phase 7A first-person handoff and manual QA.

## 1. Creative direction

**Business as Usual** is a small-town, first-person, multiplayer social-deduction game where legitimate work generates the same movement, trade, access, and apparently suspicious behavior that a covert criminal can exploit.

Desired feel:

- **Schedule I–inspired:** tactile, rewarding first-person work; tangible products and tools; supply chains; routines that create decisions.
- **PEAK-inspired:** approachable physical interactions, expressive movements, cooperative mishaps, short funny moments, inviting stylized presentation.
- **Our unique hook:** imperfect information, overlapping responsibilities, private motivations, and a small dense town where people regularly encounter one another.

These are *high-level mechanics and emotional references*, not instructions to copy proprietary assets, exact minigames, UI layouts, or code.

**Working balance to playtest:** approximately 70% satisfying hands-on work and 30% playful physical comedy. This is a creative target, not a measured requirement. Social suspicion emerges from shared systems rather than scripted accusations.

## 2. Core design pillars — preserve these

From the original design document:

1. Every covert action must resemble an ordinary legitimate action.
2. Evidence should create arguments, not binary proof.
3. Everyone, including the eventual criminal, has a real public job.
4. Innocent personal secrets create reasons to behave oddly.
5. Completed work matters through a shared Town Stability / productivity system.
6. Keep the playable area *small, dense, and systemic*, promoting repeated chance encounters.
7. Keep crime stylized and game-like, not grim or combat-driven.

**Do not let this become a generic co-op job simulator.** Even when the crime loop is not yet playable, environments, tasks, and records should support the eventual deduction loop.

## 3. Current assessment (qualitative)

The present baseline reportedly includes two-player networking, a playable Shopkeeper shift, Mechanic and Sanitation roles, cross-role resources, retro-handbook UI, a stylized town, and Phase 7A first-person hands, collision work, live minimap, pause/settings, and grounded vehicles. Claude reported passing automated Phase 7A tests, but manual QA still identified remaining defects.

**Strengths:** functioning foundational systems; an established Daylight Market art direction; shared transactional state; coherent UI identity.

**Gaps:** job loops rely too heavily on short E interactions; limited meaningful player choice; several buildings act as scenery rather than systems; town atmosphere lacks enough life; first-person hands and remote foot sliding still need refinement; minimap objective markers do not reliably update.

**Manual QA blockers carried into Phase 7B:**
- Objective markers fail to update after tasks; one blue map marker is displaced.
- First-person fists/arms can be oversized or distracting.
- Foot sliding remains visible.
- 1080p visual sharpness and render scale require verification.
- Some materials, tree silhouettes, sky, and facade repetition look too artificial.

These issues are separate from job-design expansion and remain concrete acceptance criteria for Phase 7B.

## 4. Expanded profession design

Each playable job should provide (a) a tactile core loop, (b) at least two meaningful choices, (c) one cross-job dependency, (d) an opportunity for innocent mistakes, (e) an information advantage, and (f) an ordinary action that *could* later look suspicious.

### 4.1 Shopkeeper — tangible retail and resource gatekeeping

**Proposed hands-on loop:** accept deliveries → cut/open a box → move individual stock units into matching slots → physically scan or register a customer's products → issue a paper receipt → reconcile remaining inventory.

**Choices:** prioritize customer checkout versus shelf stock; fulfill a scarce cross-job part request now or hold it for another order; accept/reject damaged deliveries.

**Cross-job ties:** sells parts to Mechanic; generates packaging waste collected by Sanitation; receives supply crates from a delivery/warehouse point.

**Information advantage:** sees stock records, requests, purchase times, and anomalies, but not necessarily buyer identity in every record.

**Potential innocent ambiguity:** transferring a restricted-looking item can be an ordinary sale; adjusting a receipt may correct a genuine mistake.

**Avoid:** a fully automated checkout that removes the physical experience; giant menus of product text.

### 4.2 Mechanic — diagnosis instead of press-E repair

**Proposed hands-on loop:** receive a service ticket → inspect 2–3 accessible vehicle components → infer fault from observations → request/choose appropriate part → perform one tactile repair interaction (bolts, replacement, calibration) → sign service log.

**Choices:** inspect carefully versus quickly; select between compatible parts with different tradeoffs; borrow a shared tool or request a new part.

**Cross-job ties:** obtains parts from Shopkeeper or warehouse; produces used parts and cleanup waste for Sanitation; may inspect a delivery van.

**Information advantage:** vehicle access, service history, approximate movement and timestamps; not a magical identity-revealing tracker.

**Potential innocent ambiguity:** moving a vehicle for service, borrowing a tool, entering an unusual garage area, or amending a service note.

**Avoid:** complex vehicle-driving simulation or realistic mechanical engineering demands during MVP.

### 4.3 Sanitation — routes, handling, inspection and disposal

**Proposed hands-on loop:** review compact route card → collect physical bags using a trolley → transport limited loads → sort a few distinct categories → process disposal → stamp waste manifest.

**Choices:** route order; carry one large bag or multiple small items; inspect a questionable item versus stay on schedule.

**Cross-job ties:** collects shop packaging and garage waste; visits the shared alley, homes, and warehouse disposal point.

**Information advantage:** which sites produced waste, which bags were collected, and incomplete disposal records.

**Potential innocent ambiguity:** carrying opaque bags, disposing of objects, visiting the alley after hours, or moving a mistakenly sorted item.

**Avoid:** repetitive trips with no interactions, limited carrying, or consequences.

## 5. Town: a functional neighborhood, not an open world

The original document specifies **one tiny town block** connected through a central Town Square. Treat 20–30 seconds between key buildings as a rough design aspiration to test, not a hard fixed distance.

| Location | Existing or proposed use | Design affordances to support |
|---|---|---|
| Town Square | Public circulation, bulletin board, meetings | Recognizable landmark, sightlines, benches, clear paths |
| Daylight Market | Stock, checkout, supplies, receipts | Front shop, stockroom, loading area, readable shop signage |
| Garage | Repairs, parts, service records | Service bays, tool cabinet, parts shelf, van access |
| Sanitation yard | Bag collection, sorting, disposal | Trolley path, sorting station, disposal bay, waste manifest |
| Warehouse | Shared supplies and restricted-looking storage | Loading dock, lockers, observable entrance, plausible civilian access |
| Back Alley | Deliveries, waste, shortcuts, encounters | Multiple legitimate task reasons to visit, partial sightlines |
| Clinic/pharmacy | Future medical supplies/records | Facade, accessible reserved space, no deep job implementation yet |
| Apartments/houses | Personal errands, mail, atmosphere | Entrances, letterboxes, small varied facades; avoid empty sprawl |

**Design rule:** every substantial building or prop group should (i) support a current job, (ii) reserve space for a documented future feature, or (iii) contribute strongly to navigation, wayfinding, or story. Do not expand the world simply to showcase generated art.

## 6. Cross-job dependency matrix

| Source | Receives / affects | Physical shared object | Opportunity for interaction |
|---|---|---|---|
| Shopkeeper | Mechanic | Replacement part / request slip | Ask, hand over, log supply |
| Shopkeeper | Sanitation | Packaging waste / waste bag | Schedule collection, witness pickup |
| Mechanic | Sanitation | Used filter/part and disposal bag | Sort workshop waste |
| Warehouse | Shopkeeper / Mechanic | Stock crates or shared tools | Pickup, borrowing, incomplete logs |
| Sanitation | Alley / apartments | Collected bags, returnable bin | Chance encounters in shared areas |

**System design:** a request should resolve even if a profession is unoccupied; a documented fallback may be slower or costly, so jobs remain playable without perfectly staffed teams. Server validates all item ownership, range, transfers, and authoritative ledger changes.

## 7. Job depth should generate player stories

Prioritize systems with high replay value:

1. **Physical manipulation:** picking, placing, scanning, tightening, sorting, loading. Short, forgiving interactions with animation and tactile feedback.
2. **Limited capacity:** one carried crate, trolley slots, small tool inventory. Forces real tradeoffs without irritating micromanagement.
3. **Shared shortages and requests:** scarce but not permanently blocking items; player-to-player negotiation.
4. **Minor daily variation:** a late shipment, misfiled service ticket, wrong bin, or missing part, with no direct guilt indicator.
5. **Imperfect paper trails:** receipts, service slips, and disposal manifests that reveal *some* facts, never everything.
6. **Spatial overlap:** route objectives deliberately cross at market, square, garage and alley.
7. **Meaningful feedback:** satisfying audio, animation, stamped completion, changes visible in world, not only HUD numbers.

Examples of emergent situations to test:

- Mechanic requests a scarce part; Shopkeeper remembers giving one out already; Sanitation sees someone carrying a box through the alley. No single observation proves anything.
- A routine garage cleanup creates an opaque rubbish bag; another player notices it being transported after a service appointment.
- A late delivery forces two players into the loading bay while one tries to finish a personal task.

## 8. UI and visual language

**One design system, three job identities.** Preserve the approved retro-handbook look: warm paper, dark teal ink, mustard/green/coral accents, chunky legible type, thick rules, receipts, tickets, clips and stamps.

- **Persistent HUD:** current task as icon + short verb + progress; clear world marker; compact town map.
- **On demand:** full job handbook, receipts, records, detailed instructions.
- **Contextual:** small reticle prompt naming exact action, optional illustrated gesture, tactile progress indicator.
- **Profession-specific:** Shopkeeper receipts, Mechanic service card, Sanitation route sheet — shared fonts, colors, spacing and motion grammar.
- **Accessibility:** preserve text equivalents for icons, contrast, resolution scaling, UI size options.
- **Privacy:** private role info only to its owner; shared town status remains shared; never leak future covert goals on maps.

Avoid excessive text blocks or UI that covers the world during ordinary movement.

## 9. Atmosphere and environment-art guidance for Phase 7B

- Preserve compact layout, all collision fixes, first-person clearances, interactions, map correspondence, and network prefabs.
- Meshy T2 may generate **a coherent kit** of varied facades, houses, shopfronts, garage, sanitation structures and standout props; refine in Blender and integrate in Unity.
- Favor attractive, stylized but grounded architecture; avoid repetitive identical houses or malformed openings.
- Improve foliage silhouette variety, clouds, warm daytime look, evening lamp pools, tactile URP materials, road texture and directional signage.
- Time-of-day must synchronize host/client; don't couple artistic daylight progression to an unapproved meeting/round-state mechanic yet.
- Ambient town life through low-cost sound, lighting, shutters, signage and motion, **not** a large NPC simulation.
- Use exact existing Meshy credit ledger and previously approved *cumulative* credit cap; no automatic new allocation.
- Review from the playable first-person eye height and in actual Windows builds, not merely editor Scene view.

## 10. Priorities and scope boundaries

### During Phase 7B — IMPLEMENT

1. Fix the outstanding Phase 7A map marker, hands, foot-slide, material and 1080p visual QA issues.
2. Complete the approved Meshy-based architecture/foliage/street-art improvements.
3. Complete clouds, lighting, synchronized time-of-day and signs.
4. Preserve all current professions, interactions, networking, player movement and UI.
5. Provide **space and believable visual stations** for the deeper job loops described here, only where safe.
6. Document the present limitations and the next phase's actual engineering requirements.

### During Phase 7B — DESIGN ONLY

- Deep Shopkeeper checkout/stock manipulation loops.
- Detailed Mechanic inspection and repair minigames.
- Sanitation cart handling and richer sorting.
- New shared economy, inventory scarcity, and rotating daily events.
- Personal secrets, criminal actions, evidence alteration, escalating meetings and endgame.

**Do not implement these systems during the environment overhaul without separate approval.**

## 11. Suggested next gameplay milestone: Phase 8

**Phase 8 — Tactile Jobs & Social Interdependence (proposal, pending approval)**

Deliver one enhanced action per profession plus two concrete cross-job interactions:

- Shopkeeper: physically scan or stock individual items.
- Mechanic: inspect a component, choose replacement, perform a short repair.
- Sanitation: collect into a trolley and sort one meaningful category.
- Cross-job interactions: Mechanic requests a shop part; Sanitation collects shop/garage waste.
- Shared deterministic logs and visibly changing world state.
- Two-process 2-player playtest and eventual 4-player exploratory playtest.
- Validate that players actually cross paths and have to talk, rather than just move between HUD markers.

Later, implement the original document's MVP trajectory: **four players, three jobs, three-day round, public tasks plus private secrets, three covert objectives with at least two routes each, simple partial records, Town Stability, and meetings**. Do not expand to six full professions before testing whether players naturally begin lying to one another.

## 12. Required Codex deliverables

In addition to Phase 7B's existing visual and technical deliverables, produce:

1. **Actual gameplay-depth audit:** cite source files and observed current functionality, not guesses.
2. **Three playable-role redesign sheets:** loop, decisions, cross-job ties, plausible mistakes, information advantage, risks and estimated scope.
3. **Town location-to-gameplay map:** note active/unused/future locations and expected player traffic.
4. **Cross-job dependency table:** data owner, physical object, validation rule, fallback for unstaffed jobs.
5. **Prioritized Phase 8 proposal:** minimal vertical slice, test criteria and excluded features.
6. **Environment readiness notes:** which Phase 7B building props and layouts are already compatible with that proposal.

## 13. Acceptance questions

- Does the town feel cozy and cohesive **from first-person**, not just in a screenshot?
- Do workplaces contain real interactive affordances rather than generic decoration?
- Can two players understand where to go with minimal HUD reading?
- Does each new interaction create at least one decision or coordination opportunity?
- Do innocent behaviors naturally overlap with future suspicious behaviors?
- Are records partial, role-appropriate, and server-authoritative?
- Can tasks finish when the other profession is not staffed?
- Does the small map cause players to encounter one another regularly?
- Are prior manual QA failures actually fixed and human-verified?
- Has Phase 7B stayed within approved Meshy spending and preserved the gameplay baseline?

**Creative north star:** A warm, slightly chaotic neighborhood where the work is enjoyable on its own, the town feels alive, and ordinary routines create funny, suspicious, memorable stories between friends.
