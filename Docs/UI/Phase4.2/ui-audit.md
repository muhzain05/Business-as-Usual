# UI audit — source and live Unity cross-check

Inspected project: `Unity/BusinessAsUsual`, Unity 6000.3.26f1, active scene `Assets/Shopkeeper/Scenes/ShopkeeperShift.unity`. The starting scene was clean, not playing, and had five roots. Baseline world screenshots were captured before edits. The Game view baseline was captured separately after explicit repaint; see Phase 4.1 capture notes for scope and differences.

Graphify query expanded from actual graph vocabulary: `hud shopkeeper order receipt interaction shift input inventory shelf animator`. Its extracted path connects ShopHUD → ShopShift → TransactionBook, and ShopStation → ShopShift → UseShelf. Source reads and live component inspection confirmed those relationships. Graphify does not prove serialized scene references; Unity supplied that evidence.

| Surface | Source / data | Reskin boundary |
|---|---|---|
| Tasks and persistent HUD | `Assets/Shopkeeper/Scripts/ShopHUD.cs`, `OnGUI`; `ShopShift.Tasks.Tasks` | Change drawing, cached styles and layout. Keep task IDs, progress and targets. |
| Customer order | `ShopHUD.OnGUI`; `ShopShift.CurrentOrder`, `OrderIndex`, `ShopShift.Products`, `Stock.Count("basket", sku)` | Preserve customer, order identity, requested quantity, collected quantity and unit price. Order value must not be labeled earned revenue. |
| Receipt book and latest receipt | `ShopHUD`; `TransactionBook.Receipts`, `Receipt`, `ReceiptLine` in `ShopDomain.cs` | Preserve IDs, order/customer, timestamp, quantity, integer unit/line/total cents. Scroll long content; don't crop receipts. |
| Interaction prompt | `ShopHUD`; `PlayerInteractor.Current`, `ShopStation.Prompt`, `StockCrate.Prompt` | Keep actual eligible closest-visible action, production range/linecast and E behavior. Busy reach uses its activity label. |
| Shift report | `ShopShift.Close`, `ShiftSummary`; HUD expanded on `Closed` | Preserve closure conditions, receipt data, $16.50 calculation and stability contribution. Present export failures if reported by Message. |
| Stock shelf row | `ShopStockDisplay.LateUpdate` | Six visible units reflect ledger counts, updated only on count changes. Phase 4.1 delays visible placement to reach peak. |
| Physical till screen | `ShopTillDisplay.LateUpdate` | Latest receipt amount, distinct from shift revenue. World label now depth-tested. |
| HUD status feedback | `ShopShift.Message`, `PlayerInteractor.Status` | Retain rejected-action explanations, crate status and completion/navigation instruction. Design a short paper notification or inline prompt message. |

Live inspection found **zero Canvas objects, zero TMP_Text objects and 192 legacy TextMesh objects**. There is no shop UI Canvas, UI prefab, UXML or USS to replace. ShopHUD is attached to the shop's SunnyPlayer scene instance; PrototypeHUD is disabled there. Fixture and product prefabs are 3D assets, not UI prefabs. `Assets/Prototype/Prefabs/SunnyPlayer.prefab` and all original Prototype sources remain unchanged.

Current HUD: immediate-mode IMGUI, fixed 1280 × 720 reference layout scaled by the smaller screen axis. It draws large translucent task and order boxes and a bottom prompt, with a center receipt overlay. Task/order formatting and GUI.Label calls are intermixed, but domain transactions remain in ShopShift/ShopDomain. Existing heading bounds can clip long titles; long receipt lists lack scrolling. These are presentation issues suitable for the approved redesign.

World-label diagnosis: all TextMesh renderers used `GUI/Text Shader`, queue 3000, default sorting layer and order 0. No billboarding code was found. ShopArtBuilder.Sign places fixed text 12 mm in front of opaque label boards and fits the text to their size. The baseline reproduces prices through Sunny and farther signage through shelves. Changing to the local unlit URP text shader with `ZTest LEqual`, `ZWrite Off`, alpha blending and backface culling fixes occlusion without removing product, price or station information. Sign placement and environment geometry were retained.

Sunny rig: Animator reports `isHuman=false`, avatar Sunny_v004Avatar, controller SunnyLocomotion, root motion disabled. Live bone names include mixamorig:Spine2, RightArm, RightForeArm and RightHand. This supports a small Generic-rig procedural upper-body reach; no humanoid Avatar rebuild, model replacement or locomotion rewrite is needed. Blender was unnecessary for this scoped implementation.

| Binding | Production source | Keep |
|---|---|---|
| WASD | PrototypeInput.Move composite | Movement |
| Mouse delta | PrototypeInput.Look | Camera |
| Left/right Shift | PrototypeInput.Sprint | Sprint |
| Space | PrototypeInput.Jump | Jump |
| E | PrototypeInput.Interact → PlayerInteractor.TryInteract | Closest visible station/use |
| Q | ShopShift return action | Return remaining carried stock |
| Tab | ShopHUD receipt action | Open/close receipt book |
| Esc / left mouse click | PrototypeInput cursor actions | Release / capture mouse |

Safe implementation proposal after approval: retain IMGUI for this small UI. Extract a read-only presentation snapshot from current objects, keyed by inventory journal length, order/receipt/task state and Message, while leaving mutations behind existing production methods. Cache textures, fonts, GUIStyles, stamp sprites and formatted receipt rows; don't build textures or entire card trees per frame. Format money from integer cents, using decimal or quotient/remainder formatting. Wrap GUI matrix/color changes in restoration. Add scroll bounds and widescreen safe margins. Tab still opens receipt pages; task/inventory pages can be exposed inside the expanded book through page tabs with the existing cursor-release behavior. Prototype labels do not authorize a new keyboard binding.

Phase 4.2 integration remains pending visual approval. Current mockups are layout/art/data examples; UI scaling and interaction behavior must be verified again once implemented.
