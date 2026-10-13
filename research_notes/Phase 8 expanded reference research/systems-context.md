# Systemic environments, moment-to-moment fun, and BAU context

Research date: October 12, 2026. Official reference pages were opened and read. Local original PDF was extracted and read in full (eight pages). This is research/design work, not a live Unity inspection or human playtest. **Verified** describes source mechanics; **Inference/proposal** describes our interpretation and BAU adaptations. Do not describe proposals as shipped behavior.

## What does the original BAU design require, and what is actually documented today?

### Takeaway
The original design already provides the stronger foundation: criminal and ordinary actions share affordances, each profession controls resources and information, innocents have reasons to lie, and work constrains investigation. The newer prototype documents describe an authority/presentation foundation and shallow fixed shifts, not a complete deduction game.

### Cited Findings
- Original PDF, pages 1–2: six friends/five days is the full concept; one criminal holds a normal public job and undertakes a multi-step cover-up without combat. All suspicious actions need legitimate equivalents; evidence is nonbinary; innocent secrets motivate lying; Town Stability makes work matter; dense encounters matter. [Original GDD](../../Working-Town%20Social%20Deduction%20%E2%80%94%20Game%20Design%20Doc.pdf) (report should link from Docs/Design using `../../Working-Town Social Deduction — Game Design Doc.pdf`).
- Original PDF, pages 3–5: professions are resource controllers and information sources. Receipts, storage logs, deliveries, camera sightings, vehicle logs and testimony are partial evidence. A record can omit identity, show only clothing, or omit buyer. Record alteration is a possible criminal objective. Poor productivity is circumstantial, not proof. [Original GDD](../../Working-Town%20Social%20Deduction%20%E2%80%94%20Game%20Design%20Doc.pdf).
- Original PDF, pages 5–8: no daily ejection. Meeting power escalates discussion → surveillance → search → arrest → final accusation in full five-day design. A single warrant is an unresolved alternative. Full map is a tiny seven-location town; square is hub; alley/storage/disposal are normal work destinations. First MVP is four players, three professions, three days, public tasks, personal secrets, three covert objectives with at least two routes each, simple records, stability, discussion. Days target 7–10 minutes; timing and stability frustration are open questions. [Original GDD](../../Working-Town%20Social%20Deduction%20%E2%80%94%20Game%20Design%20Doc.pdf).
- Existing proposal's audit documents fixed shop quantities/orders, prescribed mechanic faults/hold actions, explicitly known sanitation bag categories; genuine causal parts transfers and repair-generated waste already exist. Logical warehouse stock exists without a traversable warehouse. Homes remain scenery. [Phase 8 proposal](../../Docs/Design/phase8-living-town-proposal.md).
- The documented resource chain is warehouse → market counter → mechanic → installed, with municipal reserve fallback, authoritative quantities, snapshot recovery and read-only presentation. Warehouse/shared storage should initially reuse stockroom/locker/alley staging. [Phase 8 proposal](../../Docs/Design/phase8-living-town-proposal.md).
- Phase 7A documents owner-local first-person camera/hands, authority on movement/inventory/tasks, remote body/look replication, and no client prediction. Two-PC LAN/human comfort were not tested. [Phase 7A handoff](../../Docs/phase7A-handoff-to-codex.md).
- Phase 7B handoff documents two-process regression evidence; the 600-second ambient cycle is explicitly a visual cycle, not GDD day timer. Alley staging is scenery; no playable warehouse/home interior claimed. Human hand/input feel and actual LAN interaction remain unverified. [Phase 7B handoff](../../Docs/phase7B-handoff-to-user.md).
- Attachment provides broader Phase 8 staged context: one-day work/uncertainty/discussion experiment before full three-day MVP; four players across three professions; criminal at least two routes; private objectives must not leak. Current research request authorizes research document, not implementation of those stages. [Attachment](C:/Users/pc/.codex/attachments/6ab14d33-b642-4294-be5d-302f5073d7f0/Pasted%20text.txt).

### Inferences
- Fixed answers/quantities make successful work repeatable but erase prioritization, uncertainty and reciprocal negotiation. The documented cross-job ledger is the best reuse point because one accepted transfer can drive work, testimony and partial evidence together.
- Existing proposal defers secrets/records until after tactile slice; current user request requires research to strengthen the one-day deduction plan. Recommend a thin integrated work–secret–evidence slice early; polishing all jobs first risks another visually attractive disconnected demo. This changes sequencing recommendation, not current implementation status.
- Four players/three jobs need a tested assignment policy. Proposed fourth worker is a shift assistant within one existing profession, rotated between playtests, with useful distinct responsibilities and private knowledge; never automatically the criminal and never an extra permanent profession.
- Preserve fine-grained complete server audit for correctness and private playtest diagnosis, while generating coarser in-world records from the same accepted events. Ambiguity must be about provenance/context, not players fighting replication bugs.

### Gaps
- No live editor/runtime inspection was performed by this researcher; all implementation claims above are documented history, not newly verified state.
- Original PDF asks whether secrets create enough false suspicion, stability punishes too much, and short days work with four players. No human evidence resolves these questions.

## What mechanisms in the assigned references produce decisions, uncertainty and memorable interaction?

### Takeaway
Hitman contributes recombinable affordances and alternative approaches; Schedule I contributes material workflow causality; PEAK contributes mutual help and recoverable mishap opportunities; Goose Goose Duck and Among Us contribute spatial testimony and social reinterpretation. None is a template for BAU's win condition.

### Cited Findings

#### Hitman series (focus: World of Assassination and Freelancer)
**Verified:** IOI documents choosing timing, tools, starting equipment/disguise, exploring, overhearing conversations and exploiting environmental elements. Mastery unlocks new entry points/equipment. [IOI game features](https://ioi.dk/hitman/game-features). Freelancer adds gear-capacity/risk choices, recon, target identification and independently randomized variables; IOI explicitly contrasts emergent systemic solutions with scripted Mission Stories and warns payout objectives may not be feasible without appropriate planning. [Freelancer developer explanation](https://ioi.dk/hitman/blogs/freelancer-you-are-in-control).

**Inference:** Enjoyable decisions are preparation versus improvisation and safe access versus useful exposure. Knowledge grows through observation, while opportunities/identities require interpretation. Its alibi is behavioral camouflage in an NPC world, not human consensus. Cooperation is generally not the core loop. Multiple affordances, approaches and entry points sustain experimentation. Environment supplies options instead of empty travel.

**BAU proposal:** Use public work routines, believable access purposes, alternate handoff/disposal routes and reusable objects. Human coworkers replace large NPC crowds. Do not copy killing, disguise identity replacement, guaranteed scripted opportunities, mission ratings or impossible randomized covert routes.

#### Schedule I
**Verified:** Official developer store description specifies distinct production processes, discoverable recipes, customers/suppliers, distribution on foot/skate/vehicle, properties, workers/dealers, automated chains and co-op management. It also explicitly includes police/cartel danger and combat. [TVGS Steam page](https://store.steampowered.com/app/3164500/Schedule_I/).

**Inference:** Decisions concern product/workflow investment, distribution and allocation; friends can divide linked duties. Recipe knowledge and current stock/order state make cooperation useful. It is not an asymmetric social deduction game: criminal identity/alibi uncertainty is not its central structure. Production/distribution dependencies generate overlapping errands and unexpected coordination; progression supports replay beyond repeating a single shift. Buildings are productive infrastructure.

**BAU proposal:** Model receive → process → transfer → record as a causal chain for ordinary parcels/parts/waste, with immediate visible/sound feedback. Reuse a few workstations for legitimate and covert purposes. Avoid drugs, empire growth, combat, vehicles or labor automation removing player encounters. This official text verifies workflows but does not establish precise hand motions, so do not claim specific tactile mechanics without further footage.

#### PEAK
**Verified:** Four-player climbing with proximity chat; friends help one another up ledges and place ropes/spikes. Injuries reduce stamina; food/items support survival. The island rotates every 24 hours and can be retried that day. [Team PEAK Steam page](https://store.steampowered.com/app/3527290/PEAK/).

**Inference:** Enjoyment comes from deciding route, resources and who helps whom under shared consequences. Players see nearby terrain/help but may not know a safe next route or food consequence. Social alibis/secret betrayal are not formal core systems; mishaps alone are not evidence of hostile intent. Different layouts, obstacles and group abilities recombine. Terrain shapes who is near enough to help and hear.

**BAU proposal:** Short, optional assists—steady a tray, hold a gate, retrieve a dropped bolt—turn practical work into an encounter worth recalling. A recoverable spill can redirect traffic and produce a witness. Keep town work mistakes inexpensive; avoid fatal falls, survival attrition, forced group proximity or procedural town rebuilding. This is a design inference, not verified player-fun data.

#### Goose Goose Duck
**Verified:** Geese complete tasks/report bodies/vote ducks; ducks sabotage/eliminate; neutral players pursue separate secret objectives. Proximity voice, varied roles/maps/modes and task minigames are advertised. [Gaggle Steam page](https://store.steampowered.com/app/1568590/Goose_Goose_Duck/). Gaggle describes communication as core to its games. [Developer site](https://gaggle.fun/).

**Inference:** Players decide whom to accompany, what to disclose and when to challenge a story. Private team/objective knowledge differs from shared sightings; speech can misrepresent experience. Proximity makes an alibi partly a claim about who was near enough to witness/hear. Task interruptions and differing objectives create unexpected conversations; role/map variety changes expectations. Exact role-information rules vary and were not exhaustively verified here.

**BAU proposal:** Adapt local coworker conversation and secondary motives as flavor/ambiguity while keeping every innocent committed to shared town work. Neutral standalone victory roles, role bloat, lethal powers, frequent voting and a library of unrelated minigames conflict. Prototype with human voice first; platform proximity implementation remains separate engineering scope.

#### Among Us
**Verified:** Crew win through tasks or ejecting impostors; emergency meetings, cameras/Admin map and sabotage responses support investigation. Impostors fake tasks, use vents, sabotage and close doors to isolate targets. Innersloth explicitly recommends preparing alibis. [Innersloth official rules](https://www.innersloth.com/games/among-us/).

**Inference:** Decisions trade group safety against task efficiency and observation against progress. Crew have local sightings and task knowledge; impostors know their own intent; movement claims and testimony are incomplete/unreliable. Proximity and room routes make alibis challengeable. Sabotage alters planned paths, producing witnesses and conflicting accounts. Players' different choices/claims create round variation, but task imitation and killing are a poor fit for BAU.

**BAU proposal:** Retain work-versus-watch tension and comparable spatial testimony; substitute covert transfers and genuine criminal work for fake tasks/kills. Preserve all four players for end-of-day discussion. Avoid criminal-only vents/routes, public colleague tracking and binary visual innocence checks.

### Inferences
- A useful sandbox affordance has four properties: ordinary purpose, effect on work, externally observable trace, and more than one explanation. Example: a returns tray moves an actual part, clears a bay, exposes a box in transit and permits either honest correction or covert substitution.
- Mere physical handling is insufficient. Scanning is enjoyable only when attached to a decision about whose request comes first, what counts as a substitute, or whether to inspect a damaged label.
- Failure should change the social situation more than destroy progress. A spilled tray creates an offer of help, relocation, uncertain witness memory and a late receipt; it should not randomly erase scarce inventory.
- Replay comes from changing dependencies and routes while preserving learnable town geography. Shuffling every clue into noise prevents reasoning; fixing every task teaches guilt-by-location.

### Gaps
- These are rule/design-source findings, not controlled evidence that any one mechanic causes fun or replay retention. Proposed mechanisms need BAU playtests.
- Precise Schedule I manipulations and detailed Goose role semantics were not verified from a readable primary rules source; do not use community shorthand as a confirmed universal rule.

## How should the one-day slice combine these lessons?

### Takeaway
Build one conserved object chain, two covert routes, several normal reasons to use each route, a few partial perspectives and useful ordinary work. Test whether those systems create both voluntary help and defensible disagreement before broadening assets, professions or minigames.

### Cited Findings
- Existing proposal already recommends finite tray slots, single component replacement, independent tasks during supply waits, municipal fallback, recoverable mistakes, coarse manifests and no compulsory free physics. [Phase 8 proposal](../../Docs/Design/phase8-living-town-proposal.md).
- Original design insists every covert action have a civilian equivalent and every crime stage have alternate routes, with work productivity shown as circumstantial context. [Original GDD, pp. 1, 5–7](../../Working-Town%20Social%20Deduction%20%E2%80%94%20Game%20Design%20Doc.pdf).

### Inferences
**Concrete proposed additions (all unimplemented):**
1. A parcel/part/waste chain with immutable internal item identity, accepted transfer revision and conservation. Players see packaging/category/condition, not secret intent or perfect history.
2. Public request cards that send every profession through shared staging, with variable recipient/priority and at least two sensible route choices.
3. Private innocent secrets that require concealment of an ordinary mistake/favor, not mechanically indistinguishable full crime objectives; matched access, limited ambiguity.
4. Two covert parcel routes: disguised market return or workshop scrap collection. Both must remain feasible across generated requests and disconnect fallback.
5. Complementary clue records: receipt category + coarse time; garage slip records component action/service interval; sanitation manifest source zone + material band. Each omits a different field; compare, do not solve automatically.
6. Timestamped corrections that preserve original document plus amended value, without a player-facing guilt marker. Mechanic may correct a mistaken diagnosis; criminal may alter an eligible slip. Evidence of correction is context, not proof.
7. Selective inspection with opportunity cost: open one uncertain parcel/waste batch or keep throughput. Consent/access rules apply equally to innocents/criminal.
8. Asynchronous handoff shelves with visible waiting items and independent work; help yields speed/information, but refusal cannot hard-lock another shift.
9. One bounded shared incident—damaged delivery label changes routing, or a repair request changes priority—not unexplained random evidence.
10. Expressive optional assist and recoverable spill using snapped slots/kinematic presentation before replicated arbitrary physics.

**Ranking recommendation:** first object/route chain (very high fun/compatibility, moderate effort); then secrets + task cover (very high, moderate); complementary records + discussion (very high, moderate); tactile choice + optional assists (high, moderate); constrained incidents/request variation (high, low/moderate after chain). Engineering ratings are relative design estimates, not schedules. Cart/general physics and four-player session reliability can dominate effort; gate these independently.

**Example scenario A—late return:** Shopkeeper receives a mislabelled replacement, mechanic borrows it for a test, assistant sees a sealed box head toward alley. Innocent mechanic quietly corrects earlier diagnosis and avoids saying they wasted the town's time. Criminal routes a second similar parcel through returns. Receipt time/category, garage correction and two witnesses narrow alternatives but leave identity/intention uncertain.

**Example scenario B—helpful spill:** Sanitation cart clips a curb; two workers help reload snapped bags. Innocent shopkeeper pockets a reusable carton needed for a private favor. Criminal uses the normal restaging opportunity for a covert transfer. Observer can truthfully remember a worker carrying packaging and misattribute which bag it came from. No random item deletion; accepted ownership/history remains coherent internally.

**Example scenario C—priority conflict:** A normal urgent garage request competes with queued shop orders. Criminal does excellent legitimate work to earn trust, asks a colleague to carry a parcel, then completes remaining covert step elsewhere. Innocent assistant lies about delaying a supply request for a personal errand. End-of-day productivity is genuine but cannot clear criminal; partial manifest proves transfer occurred, not that the helper knew its purpose.

**Four-person prototype plan:** 60–90-second briefing, configurable 7–10-minute work phase, 3–4-minute evidence/discussion phase; no ejection and no final arrest during early diagnostic tests. Rotate one shared-profession assistant and criminal assignment independently. First run without criminal to evaluate ordinary fun; follow with covert routes/secrets; repeat with same town and changed dependency seed, then swap criminal route. Four active human clients are required for claims about social experience; scripted two-process tests cannot substitute.

**Measurements:** track working/walking/waiting/talking by sample observation; voluntary assists, actual encounters, invented vs evidence-backed accusations, innocent suspects, causal alternatives articulated, route availability, and whether each participant wants another round. Ask each person privately for a remembered event and confidence before reveal. Avoid a target false-accusation rate: suspicion without inferential progress is noise. Success should include players explaining two plausible accounts and identifying what additional observation would distinguish them.

**Interpretation rules:** if all four identify criminal from route alone, expand legitimate overlap; if all clues support every explanation, add discriminating item/time/condition linkage; if work gets abandoned, revise workload/stability opportunity cost; if waiting dominates, strengthen fallback; if comedy erases clues, make mishaps reversible and legible. These are proposed diagnostic responses, not playtest results.

### Gaps
- Four-player session architecture, voice transport, observer protocol and timing are not confirmed operational. Parent should label four-human multiplayer testing as a dependency/blocker until verified.
- Assistant assignment needs fairness and information balance tests; duplicating a whole job may reveal too much or leave one player idle.
- Secret/record privacy and authoritative conservation require actual network validation before interpretation of any human deduction outcome.
