# Tactile workflows and workplace documents — Phase 8A Rescue V2

Research date: 12 October 2026. **Verified** means supported by the linked publisher, developer, standards body, or inspected official image. **Proposed BAU design** means our adaptation, not a claim about the reference game. Research is intentionally about communicating and enjoying ordinary work; no criminal system is proposed here.

## 1. Which reference workflows communicate meaningful physical work?

### Takeaway

Give every action a recognizable object, an understandable change, and an observable result. A repair interaction should happen on the car; an order should move products through a counter. Generic hovering plates cannot explain either activity.

### Cited Findings

- **Schedule I — verified:** the developer describes distinct production processes, customer distribution, purchasable premises, employee automation, and cooperative operation. These are connected activities rather than a single isolated progress meter. The official manufacturing montage visibly shows a container tipped above a vessel at a furnished workstation. This is a visual observation, not verification of the exact input scheme or simulation. [Developer site and montage](https://www.scheduleonegame.com/); [press kit](https://www.scheduleonegame.com/presskit/schedule-one/index.html).
- **Papers, Please — verified:** the developer describes inspection, searching, and fingerprinting based on presented documents. The desktop booth screenshot visibly separates the checkpoint, person-facing booth, and document work area. Documents look like particular forms with dated headings, a seal, and page numbering. [Official press kit](https://papersplea.se/presskit/); [inspected booth screenshot](https://papersplea.se/img/Shot02-Booth.png).
- **Good Job! — verified:** Nintendo explicitly describes multiple solutions to office puzzles, including wheeling a projector through a door or propelling it through a wall. Speed and damage affect replay scoring; duties include connecting internet and clearing goo. It supports one or two people on one console. This verifies playful material consequences, not online first-person physics behavior. [Nintendo product page](https://www.nintendo.com/us/store/products/good-job-switch/).
- **LOCKDOWN Protocol — verified:** the publisher describes real-time first-person cooperation to complete tasks within time while dissidents secretly obstruct progress. This verifies the combination of spatial work and social communication. The source does not establish a particular task's input gestures, tool animations, or network latency behavior. [Publisher-authored Steam page](https://store.steampowered.com/app/2780980/LOCKDOWN_Protocol/).
- **Dale & Dawson — verified:** Specialists complete objectives, Slackers mimic productivity, and the Manager observes and decides whom to fire. The publisher identifies proximity voice discussion and also normal coffee breaks among productive employees. Ordinary conduct can therefore support competing explanations. [Publisher-authored Steam page](https://store.steampowered.com/app/2920570/Dale___Dawson_Stationery_Supplies/).

### Inferences

**Proposed BAU design: mechanic.** Place a latch on the actual hood, two clearly modeled battery posts inside the engine bay, a reachable replacement shelf, and a tool tray supported by a correctly scaled bench. Use the sequence **OPEN / TEST / REPLACE / VERIFY / CLOSE**. Show a single action at the object currently usable, with its illustration repeated on the service slip. The hood rotates around its rear hinge; opening exposes the component rather than producing a separate floating board. A diagnostic gauge moves, the old battery leaves its socket, the replacement fills the socket, and a service stamp follows a successful final test. These are our proposed feedback contracts.

**Proposed BAU design: meaningful choices.** Let workers choose which waiting job to handle first, whether to inspect an incoming part before carrying it, and whether to delegate delivery. Optional thorough checks should reveal extra work information, not merely require an unexplained second key press. The primary repair path must be readable and completable; alternatives become visible only once understood. A menu choice should describe the consequence: **Quick Test** versus **Full Test**, with an appropriate tool gesture for each.

**Proposed BAU design: shopkeeper.** A carton has an openable flap and product shapes inside. Shelf vacancies show where stock belongs; a preview snaps to a credible shelf position. An order card depicts the requested product and quantity. Checkout removes the delivered item, changes the count, and prints one receipt. Dispatch ends with a labeled package resting on the dispatch counter. Never imply a parcel was delivered while it remains in the player's hands.

**Proposed BAU design: sanitation.** Distinguish paper, mixed waste, and metal by silhouettes and words as well as restrained color. Bags sit on the ground or cart; the cart has a handle, visible load slots, and a physical delivery point. Sorting moves the item through a chute and changes its contents. Route order remains a choice. Small recoverable mishaps—an awkward load obstructing a passage or an untidy cart—can create conversation; routine work should not turn into constant destructive slapstick.

**Proposed BAU design: credible suspicious work.** A worker checking an unfamiliar vehicle, carrying a returned package, or reprinting a damaged label should have a legitimate explanation. Show the action in the world; do not broadcast the explanation as truth. Adapt Dale & Dawson's ambiguity of normal behavior, while rejecting managerial ejection as BAU's basic work loop.

### Gaps

- No reference game was installed or played during this focused research. Specific mouse gestures, animation durations, and task collision behavior remain unverified.
- LOCKDOWN and Dale & Dawson's parsed store pages supplied reliable descriptions but insufficient task screenshots. Do not claim their exact HUD organization was inspected.

## 2. Which paper interface best supports BAU's work, inventory, and records?

### Takeaway

Recommend a **single clipboard with interchangeable work slips**. Keep the current illustrated task immediately readable; open records and inventory as distinct slips instead of spreading sparse content across a decorative book.

### Cited Findings

- **Papers, Please — verified design lesson:** Lucas Pope describes abandoning free document arrangement on phones in favor of legible document views and a navigation rack. Stamping retains a temporary dedicated desk available when a relevant document is focused. The redesign also relocates rulebook controls and removes its spiral decoration. This is a concrete example of changing the interface structure to fit its use rather than preserving the original furniture. [Developer mobile-port postmortem](https://dukope.com/devlogs/papers-please/mobile/).
- **Workplace instruction — verified:** Lean Enterprise Institute's reproduced TWI material separates important steps, key points, and reasons, delivered progressively. Its Job Instruction sequence prepares the learner, presents work, lets them try, and follows up. NIST describes Job Instruction as standardizing how work is taught. [TWI cards and breakdown-sheet examples](https://www.lean.org/lexicon-terms/training-within-industry-twi/); [NIST overview](https://www.nist.gov/mep/training-within-industry-twi).
- **Shipping labels — verified:** GS1 separates recognizable text and graphics from machine-readable codes. Its illustrated label examples place sender/recipient information above transport details and a distinct identification section. A unit identifier connects the physical parcel with its corresponding records. These are hierarchy principles; BAU need not implement a real commercial SSCC. [GS1 guideline, sections 2–3 and label examples](https://ref.gs1.org/guidelines/logistic-label/).

### Inferences

All rows below are **proposed BAU layouts**, not reference-game features.

| Concept | Useful qualities | First-person cost | Decision |
|---|---|---|---|
| Clipboard work slip | One visual surface; task diagram, quantities, and completion stamp fit naturally; slips can change per role | Too many stacked sheets can obscure navigation; clipboard clip must stay outside content | Recommended primary structure |
| Small field notebook | Good for dated observations and short record entries; portable character | Page turns slow comparisons; tiny handwriting invites small illegible text | Useful visual treatment for records, not the main task view |
| Folded instruction sheet | Strong sequential diagrams and route overview; few elements | Folding can hide a relevant step; wide diagrams struggle at narrow windows | Use for initial job induction or an optional route card |
| Two-page handbook | Can compare two genuine documents side by side | Wastes area when content is sparse; spine and rings can interfere | Reserve paired views for actual document comparison |

**Proposed hierarchy.** The slip header contains the profession symbol, job name, and destination. Its largest usable area contains the current object illustration. Beneath that, show one short action label and the input glyph. A small numbered sequence communicates where that action fits; completed steps receive ticks, and the active step has both a shaped marker and color. Put a compact quantity or gauge next to the related illustrated object. An optional **Why?** disclosure explains the exception or tradeoff. Keep tabs attached to the clipboard's edge, clear of the illustration and close control.

**Proposed record system.** A receipt is a narrow list: town business, date/time, item description, quantity, subtotal/total if relevant, and receipt number. A shipping label emphasizes **TO: GARAGE**, a battery icon, quantity, and parcel number. A service ticket emphasizes vehicle/bay, reported fault, observed reading, action taken, and completion. Inventory tags emphasize object identity, count, and current custody. These documents answer different questions and should visibly differ while sharing town typography and ink colors.

**Proposed evidence boundary.** A parcel label describes intended destination and stated contents; it does not prove what was inside at a later time. A receipt records a transaction rather than every movement afterward. A mechanic's ticket records that mechanic's work observation rather than an omniscient verdict. Reprinted labels should retain a traceable number and clear **REPRINT** mark if that behavior is introduced. An unchecked field remains **Not recorded**, distinct from **Failed**. No red guilt flag, correct suspect highlight, or automatic contradiction verdict belongs in these work documents.

**Proposed retro personality.** Use paper margins, small ruled fields, chunky headings, a restrained stamp, and hand-drawn tool silhouettes. Make every decorative element earn its space. The TWI influence should become **see one action, try it, receive feedback** rather than a dense historic training form copied onto the screen. A fictional town form can look printed without simulated stains reducing letter contrast.

### Gaps

- This comparison is design judgment. A paper mockup does not establish first-person usability; test all three leading concepts at 720p using identical mechanic content.
- Press-kit images are linked and attributed for reference. Their availability is not permission to reuse game art in BAU. Label diagrams likewise inform original composition rather than supplying production textures.

## 3. How should tactile feedback stay clear, responsive, and private in multiplayer?

### Takeaway

Physical feedback should explain an action's state: available, attempted, accepted, completed, or rejected. Private reading stays on the owner's screen; public object motion communicates only what another nearby worker could observe.

### Cited Findings

- **Verified networking principle:** Unity distinguishes visual anticipation from authoritative state, and describes immediate non-gameplay animation/audio as a way to hide round-trip delay while the server determines the gameplay outcome. The cited documentation is versioned, so it supports the principle rather than prescribing BAU's installed package API. [Unity client anticipation](https://docs-multiplayer.unity3d.com/netcode/2.1.1/advanced-topics/client-anticipation/); [Unity latency patterns](https://docs-multiplayer.unity3d.com/netcode/1.11.0/learn/dealing-with-latency/).

### Inferences

**Proposed response contract.** On input, immediately depress the button, begin a hand reach, or move the tool toward the contact point. On server acceptance, move the shared component and play its completion sound. A checked step or service stamp appears only after confirmed completion. A rejection returns the local hand to rest and shows the specific recovery action, such as **Hands Full — Place Parcel**. Avoid **Not available yet**, which describes the implementation's state without explaining the player's next move.

**Proposed animation scope.** A page/tab transition can occupy approximately 0.12–0.3 seconds as requested, while input takes effect immediately. A hood needs a mechanically coherent travel arc rather than an arbitrary UI duration. Carrying needs hands around the object's actual dimensions; a parcel should not hide the socket while installation instructions demand reading it. A dropped object settles on a physical support. Receipt sound accompanies the paper emerging, not a message appearing elsewhere. Reduced motion should suppress paper bounce and camera flourish while preserving essential object-state changes.

**Proposed privacy rules.** Show the handbook interior only to its owner. Other players may see a neutral clipboard or a person reading, never their selected record text, private annotations, or future secret objective. Profession records are delivered only to authorized readers; hiding a tab while sending its content to every client is insufficient. Public parcel labels can be inspected at close range. HUD markers must follow only that viewer's work context. Opening a document must not pause the shared town or silently reserve every nearby object.

**Proposed solo clarity.** Start Solo opens one window and uses the same item identities and resource counts. A labeled town delivery service brings the requested package; a visible service notice states **Town Courier — Arriving**. The report attributes unattended processing to a service rather than an invented human colleague. Every profession must finish its own shift without waiting for nonexistent player-owned work. Multiplayer replaces unattended work where appropriate but preserves the same custody transitions.

**Proposed acceptance tests.** First, a new solo mechanic identifies and opens the correct hood from the world cue, tests the visible battery, replaces it, verifies, and completes the ticket without external explanation. Second, repeat at 720p and with resizing: no hidden tabs, illegible quantities, intersecting binder, or stretched illustrations. Third, two players attempt the same pickup: one shared item remains, both receive understandable outcomes, and the losing player's animation recovers. Fourth, inspect another player's screen and packets to verify private records remain private. Finally, record an uninterrupted human-scale walk through each workstation; every prop rests on a real surface, and ordinary movement reaches every interaction. Report test evidence separately from subjective fun observations.

### Gaps

- Timing values, legibility, grip proportions, and solo fallback pacing need actual player testing. The research does not certify the current build's authority or privacy implementation.
- A source game's success does not prove a proposed BAU adaptation is enjoyable. The first acceptance gate is a coherent, understandable solo repair, followed by a human usability session.
