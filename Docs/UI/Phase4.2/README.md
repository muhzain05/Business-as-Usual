# Business as Usual — Staff Edition UI review

Status: six visual prototypes ready for approval. The existing ShopHUD remains in place. Only the Phase 4.1 busy prompt was added; the complete interface redesign is not integrated.

Open [the review gallery](review.html), [all six at a glance](six-mockups-overview.png), or [the style plate](Mockups/design-system.png). Every screen is 1920 × 1080. These are original mockups composited over a Unity camera render, not screenshots of implemented redesigned UI. Font files and their licenses are local, so the gallery requires no paid or remote service.

| Mockup | State and purpose |
|---|---|
| [Normal HUD](Mockups/01-normal-hud.png) | Delivery received; 18 carried units; first customer; compact task/order/revenue cards. |
| [Expanded handbook](Mockups/02-task-handbook.png) | All six approved tasks, exact targets, and counted inventory by product. |
| [Customer order](Mockups/03-customer-order.png) | Mara: 1/2 Cola collected, 0/1 Crackers; two items remaining; $5.10 order value, unpaid. |
| [Printed receipt](Mockups/04-printed-receipt.png) | Accepted fixture BAU-0001, itemized $3.60 + $1.50 = $5.10; source UTC timestamp. Three receipts are available in this end-of-shift browsing state. |
| [Shift report](Mockups/05-shift-report.png) | Three orders, six tasks, $16.50 revenue, +3 Town Stability, nine units remaining. |
| [Interaction prompt](Mockups/06-interaction-prompt.png) | Collect exactly one Cola: shelf 5/6, basket 1/2. Character and immediate shelf stay clear. |

The same room image provides a layout comparison. Mockup counts describe their documented representative states; the background is an unstocked shelf-state render and is not presented as matching every mockup's inventory. The receipt timestamp comes from the accepted Phase 4 report, not an invented new transaction.

No UI reference images were attached to the request received in this session. The written references to PEAK, Japanese retail labels, SUSU, receipts and instruction manuals guided the composition. Existing project environment captures were inspected. There is no claim of having reviewed missing UI images or copied their layout. All icon, bag, package and stamp artwork was drawn locally for this project.

Figma verification: plugin discovery reported Figma installed and ENABLED, but no Figma tools were exposed in this session's callable tool catalog. No Figma file was created. The local gallery is the reviewable fallback.

See [the audit](ui-audit.md), [the design system](design-system.md), and [Phase 4.1 changes and validation](../../phase4.1-polish.md).

Approval requested for the visual direction, normal-HUD density, handbook structure, typography and paper/stamp treatment. After approval, implement presentation using the current IMGUI framework, retain Tab as the receipt-book binding, and revalidate the complete gameplay flows. New gameplay input bindings require separate approval.
