# Phase 8B — implementation report

The new build opens into a rendered neighborhood menu, then loads the town, assigns a work slip, checks workers into a staff hub, and opens one suspicious day. Solo is one player with explicitly unattended jobs. Multiplayer has three lead jobs and an optional assistant. The build and launch instructions are in [the playtest guide](phase8b-playtest.md).

## What changed

The menu has Start Solo, Host Game, Join Game, Settings, Credits and Quit. Its background is actual copied neighborhood geometry in a separate `Phase8BFrontEnd` scene. Original town/workplace scenes are preserved. A persistent Canvas handles the actual asynchronous scene load and network check-in; short illustrated panels cover work-slip preparation and existing workplace thresholds. The market interior remains an offset region of the town scene, so its threshold panel describes entry rather than claiming a separate scene download.

Settings expose separate master/music/SFX controls, look sensitivity, FOV, display mode, resolution, quality, reduced motion and existing performance preferences. They use saved local preferences. Display labels refresh after Unity's asynchronous mode change. Leaving a shift shuts down and destroys the old network session before loading the menu. Credits name Zain Asad, the implementation collaboration, tools and bundled font notices. Ambient music is a small original synthesized loop, rather than a finished soundtrack.

The workday retains the conserved parcel → repair → packaging/scrap → collection chain and tactile Rescue V2 work: hood animation, probes, held mouse clamp turn, carried batteries, shelf-labelled products, checkout scanning and a pushable collection cart. New temporary holds create two legitimate alternate routes. Returns and yard holding accept the same existing loads; the bay can release an unopened received parcel for a recheck before unpacking. No goods are minted by these actions. Municipal services fill absent lead jobs in solo and smaller sessions, with the same resource identities.

Each worker receives a private personal note: make dispatch room, correct a rushed service note, or return reusable packaging. One worker additionally receives a covert arrangement, selected independently of profession. Restage the sealed parcel, declare direct transfer on its ordinary slip, and do useful work. Solo labels this as practice. A direct-transfer slip is a declaration, not a verified route. The mechanic can amend a diagnosed service note; both entries remain in their professional papers.

Records → Pin Copy voluntarily discloses one selected paper with attribution. The physical staff noticeboard opens those shared copies during work; the kiosk's counter files the final report. No guilt meter, criminal-coloured prop, murder objective, automatic accusation, vote, meeting or ejection was introduced.

Destination cards now name an actual workplace or object and show signed turn guidance. Owner-local pins for private errands use the same map/highlight path without disclosing a private assignment. The old pole-and-board cart is hidden in this mode. A grounded staff kiosk and two supported returns cabinets make the new overlap points visibly part of the neighborhood.

## Information and authority

`SuspiciousDayDomain` holds readiness, day bands, shared statements and a server-only assignment dictionary. Public state contains neither the random seed nor the arrangement owner. Owner-targeted RPCs deliver personal/private copies; professional papers are also targeted. Shared papers contain only voluntarily selected copies. Live application identities preserve private progress when a client rejoins with a new network id.

Non-host clients receive item observations scoped by room, range, facing and line of sight. Unseen custody, handler and inspection state are redacted. A sealed battery uses its carton as the observation source, including garage intake; its hidden mesh has no independently meaningful position. The authoritative host retains truth in memory. This is a prototype information boundary, not anti-cheat, identity authentication or host migration.

Existing sender ownership, sequence, revision, rate, range, line-of-sight, profession and material guards remain in the production command boundary. A day starts only after all connected workers have chosen jobs and readied up. Professions stay fixed for the day; an unassigned/rejoining worker can reclaim an available job. A report or time limit closes the day. Only the worker sees their private progress in the summary.

## Asset/tool provenance

Two Meshy T2 assets cost **30 additional credits**: 5 preview + 10 refinement each. Cumulative spend is **170 / 300**, zero reserved, 130 remaining. Both refinements returned SUCCEEDED and 10 consumed credits. The [ledger](../meshy-credit-ledger.json) retains every task id; [provenance](../Diagnostics/Phase8B/Assets/provenance.json) records hashes and costs. The accepted Rescue V2 wagon, carton, hands and cart are reused rather than regenerated.

Blender cleanup applied transforms, grounded pivots, adjusted cabinet width, added receiving/writing supports and blank label surfaces, and exported FBX alongside editable `.blend` sources. Read-only source-scene audit counts 9,978 triangles for the kiosk and 6,856 for a cabinet. These are source-mesh triangle counts, not draw-call or frame-time claims. Materials use the restrained town palette and fitted gameplay colliders.

Unity MCP was unavailable; installed Unity 6000.3.26f1 built and ran the native Windows players through its CLI. Blender MCP could not connect; Blender 5.2.2 LTS performed the cleanup through its batch CLI. Figma's Starter MCP quota prevented a live file pass; editable native Unity layouts provide the implemented result. Higgsfield was unnecessary for the real rendered neighborhood background. None of these tool limits required abandoning implementation or requesting new paid generation.

## Design examples

1. **The promised favor.** A shopkeeper stages a parcel at returns to clear dispatch space. A sanitation worker sees the carton there while returning packaging later. A direct-transfer slip disagrees with that memory, but a legitimate restage and an innocent paperwork mistake remain possible explanations.
2. **The amended repair.** A mechanic first takes a quick reading, later performs a load check, and corrects the service note. A selected amendment preserves old and new wording. It supports a timing question without certifying whether the mechanic was covering a delay or responsibly correcting rushed work.
3. **A quiet yard detour.** An assistant moves the same sealed carton through yard holding, writes a direct-transfer declaration, then helps the receiving gate. The mechanic still needs the actual parcel; sanitation still needs the eventual waste. Useful work and the strange route share one resource chain, so the cover has a real consequence rather than a detached sabotage button.

See [validation](phase8b-validation-report.md), [native gallery](../Diagnostics/Phase8B/review.html) and [remaining issues](phase8b-remaining-issues.md). Homes, cozy interiors, leisure and the earned cosmetic economy are deliberately future phases in [the roadmap](phase8b-future-roadmap.md).
