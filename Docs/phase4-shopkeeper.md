# Phase 4 — Daylight Market

## Play the complete shift

Open `Unity/BusinessAsUsual` with Unity 6000.3.26f1. Open `Assets/Shopkeeper/Scenes/ShopkeeperShift.unity`, press Play, then click the Game view. The original `TownPrototype` remains a separate scene.

WASD moves; mouse looks; Shift sprints; Space jumps. **E** uses the closest visible station. **Q** returns a carried stock crate to its dock. **Tab** opens the itemized receipt book. Esc releases the mouse; click to capture it again.

1. Walk to **Receiving**, in the rear right corner. Press E to receive the sealed delivery: six Sunbeam Cola, six Meadow Tea, and six Golden Crackers.
2. Approach the brown **Stock Crate**, in front of Receiving, and press E. The crate attaches to Sunny's existing carry anchor. Q safely returns it, including its remaining contents, to the same dock.
3. Carry it to the three labeled gameplay shelves. Press E at **Sunbeam Cola** (left center), **Golden Crackers** (right center), and **Meadow Tea** (rear center). Each stocks six units of its matching product. When all 18 are stocked, the empty crate automatically returns to the dock.
4. Follow the order card at the upper right. Each E press at a matching shelf collects one requested unit. Unrequested items and extra quantities are refused. Bring the complete order to **Checkout**, at the front left counter, and press E to sell and print its receipt.
5. Serve all three customers:

   | Customer | Order | Receipt total |
   |---|---|---|
   | Mara | 2 Cola + 1 Crackers | $5.10 |
   | Eli | 1 Tea + 2 Crackers | $5.20 |
   | Jo | 1 Cola + 2 Tea | $6.20 |

6. Approach **Shift Review**, the front right terminal, and press E. Completed tasks, itemized receipts, **$16.50 revenue**, and **+3 Town Stability contribution** are displayed. A JSON shift report is saved to `Application.persistentDataPath/shopkeeper-last-shift.json`.
7. Approach the front exit and press E to return to the unchanged TownPrototype. To replay, stop Play, reopen ShopkeeperShift if needed, and start Play again. A new session starts a fresh shift.

Customers are a deterministic queue of named order cards. There are no new generated characters, multiplayer systems, or criminal objectives. The stability contribution is recorded by the shift; a persistent town simulation is not implemented.

## Architecture and Graphify navigation

Graphify verified `Interactable`, `PlayerInteractor`, `CarryableBox`, `PrototypeBuilder`, and the original player prefab before work began. The main extension point is `Interactable`: Phase 4 stations and the stock crate participate in the unchanged proximity, visibility, prompt, and E-input system.

- `ShopDomain.cs`: reusable integer-quantity inventory ledger, atomic bundle transfers, ordered movement journal, product definitions, customer orders, task board, and duplicate-protected transaction/receipt book. Money uses integer cents. Logical sale transfers stock to the `sold` account rather than deleting it, so conservation remains directly auditable.
- `ShopShift.cs`: one job's lifecycle and deterministic data. Inventory flows `delivery → dock → carry → shelf → basket → sold`. Returning a crate moves remaining stock back to `dock`. Closing requires completed tasks and no stranded carried or basket stock.
- `ShopStation.cs`: delivery, shelf, checkout, close, and exit adapters. Only the new scene's exit performs runtime scene loading.
- `StockCrate.cs`: attaches to the original carry anchor, retains its inventory, restores its collider states, and returns safely to the dock. It is separate from the original Phase 3 carryable box, so no original input/controller changes were needed.
- `ShopHUD.cs`, `ShopStockDisplay.cs`, `ShopTillDisplay.cs`: task/order/receipt presentation, six marked stock units reflecting actual inventory, and checkout display.
- `ShopBuilder.cs`: separate-scene construction, using the original SunnyPlayer prefab. The legacy prototype HUD is disabled only on this scene instance; no original prefab is changed.
- `ShopArtBuilder.cs`: reassembles the generated retail art while retaining station components, gameplay collision and scene-instance wiring. Blender axis conversion is retained inside wrapper prefabs.
- `ShopValidation.cs`: editor-launched live Play Mode validation, scene accessibility, movement/collision, the complete shift, and actual return-to-town navigation.

Use `graphify query "shop shift inventory receipt station"`, qualified `graphify explain`, and `graphify path` for navigation. Serialized references still require Unity inspection. `graphify update .` refreshes the code graph without an API or semantic-generation cost.

## Original asset and visual improvement plan

The supplied reference targets dense, compact retail shelving, illustrated packaging, cream/terracotta finishes, a detailed register and scanner, baskets, cold shelves, and warm light. The initial functional scene intentionally used temporary fixtures; `Diagnostics/Phase4/functional-before.png` records that stage.

The visual pass uses original assets:

- **Procedural Blender modeling:** beveled shelving and price rails, punched upright details, ribbed refrigerator shelves and vents, counter cabinet/drawers, stepped cash register with keys/printer/cash drawer, scanner stand, basket lattice/handle, delivery crate, bottle/can/carton silhouettes.
- **Reusable Unity prefabs:** all 15 exported fixture/product models. Repeated products share meshes and materials with instancing enabled.
- **Purpose-built textures:** eight illustrated fictional brands with original graphics and barcode motifs, plus a seamless oak floor texture. Created locally with Pillow by `Scripts/create_shop_textures.py`; no external assets or image-generation services were used.
- **Unity assembly:** 873 packaged product instances, price labels, category signage, stocked side aisles, rear refrigerated assortment, impulse products, windows, wall trim, practical overhead lights, plants, checkout cable, and ceiling panels. The static scene audit records 1,158 mesh instances using 26 shared meshes and approximately 540,000 triangles; this is a geometry audit, not a measured frame-rate guarantee.

The isolated Blender asset scene was saved as `Assets/Environment/Shopkeeper/Working/Shopkeeper_AssetLibrary.blend`. Label images are packed. The live Blender scene was restored after export. `Scripts/Blender/build_shop_assets.py` regenerates the original meshes and exports; it never overwrites Sunny's source file. `Diagnostics/Phase4/model-inventory.json` records geometry budgets and material assignments.

The **SHIFT STOCK** row contains the real six units per gameplay SKU. The surrounding varied assortment is decorative and does not enter the shift ledger. This distinction lets the room be richly stocked while keeping all counted stock conserved.

## Validation and captures

In Unity, use **Business as Usual → Phase 4 → Validate Complete Shift**. It opens ShopkeeperShift and runs the production interactions in Play Mode. Results are written to `Docs/Diagnostics/Phase4/playmode-validation.json`. The validator uses deterministic motor calls and scene-position setup, so it is not evidence of a human keyboard/mouse playthrough.

Use **Phase 4 → Capture Shop Views** for cashier, overhead and product views. The overhead capture excludes roof geometry; art captures exclude Sunny for a clear environment comparison. The regular player camera is unchanged.

The original Phase 3 validator is rerun separately. Preservation is checked against SHA-256 hashes of every original Prototype file in `preservation-baseline.json`. `python Scripts/verify_phase4.py` checks preservation, validation and the conservation journal independently.

The expanded Phase 4 validation also tests synthetic Q input, player-sized capsule routes from spawn to every station, stock-display counts before and after sales, ceiling camera obstruction, and an actual E-triggered scene transition. The original Phase 3 regression passed all 19 checks. See the final machine-readable reports for the Phase 4 total and check details.

See [the visual comparison](phase4-reference-comparison.md) for screenshots and remaining gaps. This is an original stylized implementation inspired by the supplied examples; it is not a claim of matching their finished art quality.
