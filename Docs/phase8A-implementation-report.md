# Phase 8A: the living workday implementation

October 12, 2026. This is a **four-player technical work-only prototype**, with an integrated material chain and a human playtest procedure. It does not establish that the activities are enjoyable. Deception belongs to the separately scoped [Phase 8B proposal](Design/phase8B-scoped-plan.md).

User play exposed missing solid collision and inadequate onboarding in the initial build. See the [collision and onboarding correction](Diagnostics/Phase8A/usability-fix.md); the earlier gallery and validation summary describe the earlier build.

## Run and review

- Current Windows build: `Builds/Phase8A-Usability/BusinessAsUsual.exe`. The original build is retained.
- Start with one window: `powershell -ExecutionPolicy Bypass -File Scripts/launch_phase8a.ps1 -Mode host -Resolution 720`.
- Local four-window launch: `powershell -ExecutionPolicy Bypass -File Scripts/launch_phase8a.ps1 -Mode four -Resolution 720`.
- Separate-machine play: one host and three clients using the host IPv4 address; see the [four-human procedure](Design/phase8A-playtest.md).
- [Native gameplay screenshots, short previews and validation links](Diagnostics/Phase8A/review.html).
- [Baseline audit](Diagnostics/Phase8A/baseline-audit.md), [validation summary](Diagnostics/Phase8A/validation-summary.json), [build report](Diagnostics/Phase8A/build.json), and [preservation report](Diagnostics/Phase8A/preservation.json).

The project was inspected through Graphify, source reads, and Unity MCP. The original eight-page GDD and the expanded research informed scope; no GDD v2 was located. The intended original four-player/three-day MVP remains a later progression beyond this one-day work experiment.

## One item registry connects all three jobs

`WorkdayDomain` owns a single finite registry for this slice: replacement battery `PART-8A-001`, its packaging `PACK-8A-001`, old component `OLD-8A-001`, independent square bag `BAG-8A-001`, and three staged customer order units. There are seven persistent identities. Installation and disposal retain objects in installed or municipal custody, rather than deleting them or creating a different profession's copy.

The market receives the shipment, optionally verifies it, prepares it and releases it at dispatch. A worker carries the same battery and packaging to the garage receiving tray. The mechanic requests, receives, checks and installs the battery. Installation releases the existing packaging and old component for sanitation. Sanitation physically collects those batches, transports them to material chutes, chooses whether to inspect them, processes them and files partial manifests. Customer orders consume the corresponding finite counter unit into customer custody.

This is the active default town experience. Earlier aggregate SKU/task sequences remain available only in opt-in historical harness runs without `--bau-workday`. Their station commands are blocked in the living-workday mode. They are not simultaneous independent inventories for the new professions. Authored scenes, hierarchy IDs, Sunny sources, environment assets and previous builds remain intact; the new stations and visible objects are created in the existing workplace footprints at runtime.

## Choices affect another worker's responsibilities

| Profession | Choice | Concrete consequence |
|---|---|---|
| Shopkeeper | Customer priority or garage priority | Serving finite customer orders consumes work time while the garage request waits. Garage priority prevents serving pending customer work until dispatch; R changes priority. |
| Shopkeeper | Verify or dispatch unchecked | A verified parcel lets the mechanic skip intake checking. A fast unchecked dispatch moves the two-reading check to the garage. |
| Mechanic | Quick diagnosis or a full load check | Quick diagnosis takes one probe action and is sufficient with verified stock. Full checking takes two readings and resolves unchecked intake. |
| Mechanic | Quick swap or bench recovery | Quick swap takes one terminal turn and produces mixed treatment material. Bench recovery takes two turns and produces recoverable metal with additional supply value. |
| Sanitation | Square/paper-first or garage/metal-first route | Travel, waiting for garage output, and first processed material differ. Matching the declared first-material priority adds one supply point. |
| Sanitation | Direct processing or inspection | Inspection takes another handling action, creates a more informative partial copy, and adds supply value. Wrong chutes reject the action and retain the load. |

These are small prototype tradeoffs; human testing must determine whether they feel consequential enough. They are not a mature diagnosis simulation, a complete shop restocking redesign or a cart physics system.

## Physical handling and optional cooperation

Opening a hood, checking probes, turning a terminal, seating a replacement, carrying a parcel, and lifting/processing batches have visible prop states and owner-local hand/tool gestures. The terminal uses E to engage, then a left-mouse drag to turn 60 degrees. Camera look is suppressed while manipulating it, and Q cancels. Actions have short feedback and contextual E/R prompts; the new chain does not use held-E progress bars as its work mechanic.

A coworker can latch the receiving gate open for eight seconds before the carrier arrives. This saves the carrier's solo opening step and visibly changes the gate. Solo delivery remains possible. Shop orders, vehicle checks, square collection and bounded workstation resets are available independently. After 25 seconds, garage self-service can recover the same unheld shipment even if the market is staffed but unresponsive. It never mints a spare part. A held parcel must be returned or recovered after disconnect before self-service can move it.

Four distinct spawn positions and a four-person town capacity extend the existing session. Three lead profession slots remain, plus one assistant sharing a selected profession. Shop assistants dispatch/deliver; mechanic assistants perform intake checks; sanitation assistants transport batches to yard intake for the lead to sort. Their own support counters track delivery/intake, gate help and reset work, rather than copying the full lead progression. A half-finished two-reading intake check does not award completion. Assistants can take over an unstaffed lead slot after returning their load.

## Authority and future evidence boundaries

The existing `NetworkShop.Execute` boundary still authenticates player ownership and checks command sequence, current global revision, range, line of sight and permissions. The workday domain checks custody, profession, assistant permissions and material preconditions. Duplicate sequences, stale requests, unavailable items and wrong chutes cannot mint or consume objects.

The server-only append-only audit records item ID, source/destination custody, action, actor, sequence and server time. Inspections are explicit observed events. Ordinary broadcasts contain current presentation/work state and public shift progress, **not this audit**. Profession-specific paper copies are projected separately and delivered with recipient-targeted RPCs. Receipts omit later handling, service slips omit carrier identity, and sanitation manifests omit earlier handlers. The UI uses the corresponding profession's own copies.

`WorkClaim` is a separate future attributed-statement schema, not authoritative truth; claim authoring and secret data are not implemented. No hidden assignments are serialized. Current world presentation snapshots still describe shared physical object state broadly: Phase 8B must address observation privacy before relying on offscreen secrecy. Omitting an audit does not, by itself, make the complete network presentation secret-safe.

Disconnect recovery releases assignment slots and returns carried items to known staging. Rejoining receives current shared state and then selects an available assignment. New client IDs do not preserve personal support counters. There is no durable host migration or saved-session recovery.

## Workday, presentation and remaining usability

The workday starts at the first shipment receipt or garage request and lasts 480 seconds by default, independently of Phase 7B's ambient daylight clock. The launcher accepts 420–600 seconds for the original 7–10-minute range; development CLI allows shorter accelerated sessions. Early closure requires one customer order, the repair and all three waste batches. At timeout, each incomplete category removes two supply points. Supply starts at five; customer output, repair workflow, inspection and route priority affect it. Its capacity is 20, while the current finite content earns approximately 12–18 on a completed shift; it is not a completion percentage or a deduction verdict.

The staff desk now uses proportional paper/card primitives, licensed fonts and existing role icons, with mouse and numeric-key assignments. It avoids stretching the book artwork behind repeated profession rows. Native layout captures cover 1280×720, 1920×1080, 1366×768 and 1280×800 (16:10). A 1920×1200 window exceeded the active display's work area and was clamped, so that failed oversized-window attempt is retained separately. The handbook uses the approved paper language for the new tasks, workplace custody, receipts, service slips, manifests and public report. Status text has its own paper backing; completed mechanic work no longer prompts further installation. Roster counts update after other clients join.

The market's open-door walking threshold automatically crosses into the existing offset interior. The exit threshold also works without a second E. This preserves the connected scene, inventory and multiplayer state. It is a short automatic transition, not a seamless rebuilt interior. Existing collisions and environment layouts remain; the fitted standing area at the receiving tray was corrected after the first test found a helper approaching through the workshop wall.

Remaining scope and feel limitations: one fixed vehicle/part chain, three customer orders, three waste batches, simple prototype component/parcel meshes, procedural hands, shallow bounded reset work, no in-game voice, and fresh-session repetition. Four-window localhost success is not a cross-machine LAN or long-session performance guarantee. First-person latency, grip quality, workstation clarity, social congestion and post-task downtime require human review. No lighting, house or lamp overhaul was bundled into this gameplay pass, and no Meshy credits were spent.

## Validation and reproduction

The linked [summary](Diagnostics/Phase8A/validation-summary.json) provides the authoritative per-suite counts and raw files. Final four-process runs cover shop, mechanic and sanitation assistants; input/camera/listener ownership; keyboard assignment; mouse-drag terminals; inventory conservation; duplicate-command rejection; disconnect/rejoin; targeted records; independent fallback work; wrong-sort recovery; the market threshold; and closing the workday. Their run metadata identifies the compiled gameplay assembly. Early failed/intermediate captures remain separately named.

Domain tests exercise four dispatch/repair combinations plus 10,000 seeded action sequences, including invalid/repeated material actions, reserve timing, assistant staging and workday expiration. They verify correctness, not the entertainment value of those choices. Existing Windows town suites cover full/staffed work, entry, guidance, first-person controls and Phase 7B presentation in historical compatibility mode. Fresh unchanged editor validators cover Phase 3, complete shop shifts, reach/feedback, handbook UI and static town collision. Their accepted historical evidence was restored to the exact pre-task hashes after fresh results were archived here.

The final four-process runs passed 24/24 (shop assistant), 25/25 (sanitation assistant), and 23/23 (mechanic assistant/self-service), including the half-finished-check guard. The separate original-shop Phase 5 networking build passed 32/32 controlled regression checks. Domain output records 10,385 checks. Editor results are 19/19, 87/87, 57/57, 65/65 and 22/22. These counts describe automated conditions, not human fun. Historical compatibility runs preceded the final workday-only completion/roster refinements; the final three workday runs and Phase 5 regression use the reviewed gameplay assembly.

Run:

```powershell
dotnet run --project Tests/Phase8A/Phase8A.csproj
python Scripts/run_phase8a_final_workdays.py
python Scripts/run_phase8a_validation.py
```

The final-workday runner verifies the current active prototype. The broader runner also exercises historical compatibility; run suites sequentially because Windows players share preferences. Unity's `Business as Usual / Phase 8A / Build Windows Living Workday` menu reproduces the build. Unity may report existing inference-package shader/Pipeline warnings even when the build report succeeds with zero errors; check `build.json`, not only the MCP warning envelope.

Preservation is verified against the pre-8A manifest and Git index tree. Build-only settings/material/registry side effects are restored; the listed code additions, diagnostic outputs and AST graph refresh are intentional. No commits, pushes, staging or destructive reset occurred. The next decision should be based on the [four-human work-only playtest](Design/phase8A-playtest.md), followed by the scoped Phase 8B boundary.

The preservation audit verifies 5,814 baseline files, nine intentional existing code changes and no unexpected protected changes. The index tree remains `77fff87b7cbe089ce03bbd6725231e0f38c8a062`. The pre-task backup is `D:/Business as Usual Backups/pre-phase8A-2026-10-12`. AST graph refresh runs without LLM extraction or generation credits.
