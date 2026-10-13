# Phase 6 addendum: personal work in Daylight

The addendum replaces the shared staff instruction feed with personal job guidance, adds a paper town map and active work stamps, and upgrades the town to a reusable modeled neighborhood. It retains the tested two-player, three-profession slice and its existing resource economy. The original eight-page Working-Town Social Deduction design document was read before implementation; the comparison and scope decisions are in [the design audit](phase6-addendum-design-audit.md).

Open [the before/after review](Diagnostics/Phase6Addendum/review.html) to compare seven matched town cameras and all three job interfaces. Accepted Phase 6 evidence remains intact; the comparison's Before folder is a separate copy.

## Personal information and shared work

`TownGuidance` derives the owner's next step from authoritative job progression, carried resources and workplace state. Mechanic instructions follow service slips, inspection, the market transfer, repairs and filing. Sanitation instructions follow its own route, carried material, matching bin and manifest. Shopkeeper instructions follow entry, incoming garage requests, delivery, the crate, product shelves, checkout and closure.

`NetworkPlayer.SendFeedback` targets the acting client. A separate broadcast RPC retains the visible restocking reach on both copies of Sunny. Public town notices are neutral. The handbook never substitutes TownJobs.Message or ShopShift.Message for the owner's instructions. The public report shows profession task counts, productivity and Town Stability; the Records tab contains the owner's profession's slips, without actor IDs or full UTC audit strings.

These are presentation and action-feedback boundaries for the existing public jobs. The existing authority snapshot still synchronizes public inventory, work state and audit records. It is not a privacy protocol for future secrets: criminal objectives and personal secrets will need their own server-owned, targeted payloads.

## Compact paper interface and wayfinding

All three town jobs use the same approved paper art, fonts, palette and handbook tabs. Normal play has a short role card, segmented progress, compact north-up map, and an E prompt when relevant. Short temporary receipts and a check stamp reward completed work. Inventory is illustrated with product, battery/filter and material symbols. Deeper instructions stay in the book.

Tab opens/closes the handbook; J opens/closes the staff desk. These actions release the cursor and restore capture when closed. E, held E, Q, movement, look, running, jumping and Escape retain the existing controls. Clicking the job card or map also opens the book. Earlier scenes retain the original approved ShopHUD.

The map shows the local player, market, garage, yard, square landmark, alley and that player's objectives. It never tracks colleagues. The separate market interior projects to the market on the town diagram; it is not an interior floor plan. Physical shelf/delivery stamps provide detail inside the shop. Pins advance with real state changes, rather than a separate quest ledger.

Mechanic magnifiers and wrench stamps sit above the actual interaction points; dashed floor pads show where to stand. Secondary repair points remain visible after inspection while the mechanic obtains parts. World icons depth-test against buildings and use no colliders or networking objects. Permanent garage floor labels now identify bays and vehicles, without announcing faults to every role before inspection.

The normal paper cards occupy about 7.1% of a 16:9 screen. With contextual E and order cards, they occupy at most 11.1%, excluding a short-lived feedback toast. Gameplay captures were reviewed at both 1920?1080 and 1280?720.

## Original modeled town kit

Five original Blender models replace the old building shells and repeated wall/roof combinations. They reuse the existing town material palette:

| Model | Triangles | Use |
|---|---:|---|
| MarketShell | 7,784 | Complete hollow market exterior around the existing functional door |
| WorkshopShell | 6,712 | Open workshop frontage, walls, shutter details, finished ceiling and closed gables |
| TownHouseA | 6,332 | Repeated neighborhood house, porch, shutters and window boxes |
| TownHouseB | 6,420 | Matching second house variant with chimney and different paint |
| ParcelCart | 668 | Market/alley parcel detail |

The kit has FBX sources, normalized Unity meshes, reusable prefabs, and its own `Assets/Town/Working/DaylightTown_AddendumKit.blend`. Sunny's original Blender file and active scene remain preserved. The new importer explicitly refreshes mesh buffers when updating existing kit meshes; visual review caught this requirement during iteration.

The square gains public direction boards. Existing lamps, crossings, benches, pots, yard props and workstations retain their identity and function. Warmer bright daylight, softer shadows and restrained workshop fill improve covered work areas. There is no day/night system or map expansion. All 95 existing collider transforms and enabled states were preserved during assembly.

The active outdoor town uses 208 renderers, 24 shared material assets and 28 unique meshes: 346,824 instanced triangles and 83,646 unique triangles. The accepted baseline used 399,828 and 117,454 respectively. These counts exclude the connected approved shop and runtime Sunny. They do not establish draw-call cost or shipping performance; repeated houses currently share meshes and have no separate LOD tiers.

## Meshy and credits

The actual connector schema and balance were inspected before asset work. Smart Topology is exposed with `meshy-t2`; T2.5 is not exposed. The documented preview/refine costs are 5/10 credits, with explicit approval required before spending. The original Blender kit supplied the required cohesive buildings, so no paid generation, refinement or remesh calls were made. **Credits spent: 0.** There is no external generated asset dependency or licensing ambiguity in this kit.

## Alignment and remaining gaps

The GDD's square hub, small block, ordinary resource transfers and legitimate alley visits remain the basis of the town. Staffed market packing is a real dependency; repair debris and delivery packaging create real sanitation pickups. Personal guidance and partial paper presentation make these activities readable without an omniscient instruction feed or colleague locator. They prepare observation and ambiguity for later deception systems.

The result remains a job foundation validated through automation, not a playable social-deduction game. Hidden criminal assignment, private secrets, concealment routes, meetings/warrants, proximity voice and the day loop remain future scope. Clinic, warehouse and apartments are not functional locations yet. The GDD's four-player MVP and six-player full game exceed the current two-player slice. Faults and order queues remain scripted, and the town needs human pacing/observation playtests. The background houses are scenery beyond the existing playable bounds.

## Launch and verification

Close older game windows before opening a fresh ordinary host/client pair, then run from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File Scripts/launch_phase6.ps1 -Mode pair
```

The launcher uses the normal port 7777. Automated suites use separate ports 7785/7786 so they do not disconnect an existing ordinary session. Tests use real standalone host/client processes, production interaction dispatch, held input and synthetic keyboard events, with opt-in deterministic placement. They do not claim human input or two-PC LAN testing.

Final results and limitations are in [the validation report](Diagnostics/Phase6Addendum/validation.md). Fresh regressions are archived under Phase6Addendum/Regression and accepted earlier evidence is restored. SHA-256 preservation checks cover original and accepted Phase 6 assets, packages, project settings and the exact Git index. No commits or pushes were made.

The required `graphify update .` completed without LLM calls: 2,175 nodes, 4,017 edges and 120 communities. This refresh covers code relationships; no paid semantic relabeling was requested.
