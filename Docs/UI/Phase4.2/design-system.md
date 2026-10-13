# Staff Edition — design system

Original identity: a neighborhood market's employee handbook, with a sun-marked grocery bag, blocky ink headings, physical order slips and printed receipts. Packaging illustrations are functional product cues. Paper is opaque; subtle grain stays away from numeric detail. A 4 px ink outline and 7 × 8 px offset ink shadow give cards the feel of stacked printed stock. Routine gameplay keeps Sunny and the nearby shelf in the center clear.

Machine-readable tokens: [design-tokens.json](design-tokens.json). Visual examples: [design-system.png](Mockups/design-system.png).

| Token | Value | Use |
|---|---|---|
| Warm paper | #F6E9C9 | Handbook and task cards |
| Dark ink | #244047 | All essential text, outlines and key symbols |
| Muted green | #649B76 | Completion outline, tea package, decorative accent |
| Mustard | #EFC864 | Active page, key task tag, crate/task emphasis |
| Muted coral | #CE735A | Cola package, stamp accent, rejection marker accompanied by ink text |
| Light paper | #FFF7E5 | Receipt and inner cards |
| Paper rule | #D4C7A8 | Dividers and inactive progress track |

Use ink on opaque paper for essential content. Green and coral are accent colors rather than small text colors. Stamp words remain dark ink even when the stamped outline is colored. Progress, completion and errors also have words/counts or symbols; color is never their only signal.

Typography:

- **Dela Gothic One**: chunky headings, market masthead, report total and stamps. Limit to short phrases so its broad forms do not crowd data.
- **M PLUS Rounded 1c Regular / ExtraBold**: descriptions, names, input hints and numeric columns. Align amounts on the right, and allocate stable digit width when values update.
- **Yusei Magic**: one occasional margin instruction. Keep quantities, receipt calculations and action names in the readable companion face.

All downloaded candidates use SIL Open Font License 1.1, which permits bundling/embedding in commercial software with the license and copyright notices retained. Source licenses: [Dela](https://github.com/google/fonts/blob/main/ofl/delagothicone/OFL.txt), [Rounded M+](https://github.com/google/fonts/blob/main/ofl/roundedmplus1c/OFL.txt), [Yusei](https://github.com/google/fonts/blob/main/ofl/yuseimagic/OFL.txt). Copies are in `Fonts/`. Fonts were not modified or installed globally and were not imported into Unity. [font-audit.json](font-audit.json) records file hashes, cmap counts and sample coverage. All four files cover the current English strings, digits, dollar sign, multiplication sign, bullet, dash and sampled Japanese heading 店員手帳. This is sampled glyph evidence, not a claim of complete Japanese localization support.

Reference 1920 × 1080: safe margin 48 px; spacing 8/16/24/32/48 px. Main body 24–28 px, quantities 27–34 px, main headings 32–44 px, report hero amount 90 px. At 1280 × 720 these become approximately 16–19 px body and 18–23 px quantities. Microcopy is 20 px at reference size and never carries critical stock or currency data. Expanded pages use scrollable content after integration; do not shrink long receipts until unreadable.

| Reusable component | Presentation contract |
|---|---|
| Handbook spread and tabs | Opaque cream pages, inner ink rule, small binding circles. Tasks/Inventory/Receipts/Shift tabs; current page clearly marked. Preserve receipt-book Tab control. |
| Task card | Number, description, exact progress/target, optional completion symbol. Expanded book lists all six; compact card shows active work and total completed tasks. |
| Order slip | Order ID, customer/context, complete product list, basket/requested quantities, unit prices and order value. Explicit unpaid/needs-items state. |
| Printed receipt | Receipt ID, order ID, UTC timestamp, item quantities, unit prices, line totals, exact total, paid stamp, serrated edge. No invented tax, discounts or fees. |
| Inventory counter/card | SKU icon plus quantity and labeled account. Separate shelf stock, carried crate contents and order basket. Decorative shop assortment is excluded. |
| Interaction prompt | One real action, current E key, product/quantity context, room for status/rejection text. Busy stocking replaces the E action with activity feedback; no screen-space object over Sunny. |
| Completion stamp | Ink word plus green/coral outline; optional 180 ms print impact. Does not determine task completion or transfer items. |
| Status bar | Quiet track, solid green fill, exact numeric label. Update only when the source progress changes. |
| Shift report | Revenue, orders, tasks, stability and itemized receipt ledger; existing export/navigation status. No simulated multiplayer or town system. |

Motion after approval: a 150 ms paper slide/fade and a 180 ms stamp impact, with instant completion when motion is reduced. Persistent HUD does not bounce continuously. Restocking is the separate 650 ms Phase 4.1 skeletal reach; UI effects must not drive its inventory transaction.

Normal HUD: compact identity top left, customer slip top right, active task bottom left, revenue/receipt summary bottom right and one contextual prompt near the bottom center. Error/status messages temporarily extend that prompt or appear in a small paper notification; they must remain readable and must not cover Sunny. Expanded handbook is deliberately modal in appearance but must not silently pause or alter gameplay input. Any change to gameplay input while browsing requires explicit approval.

The review gallery provides six static full-size designs with a local screen selector. It is not an implemented responsive Unity interface. After approval, validate 1280 × 720, 1600 × 900 and 1920 × 1080 in-game, all customer baskets, busy/cancelled stocking, rejection messages, all three receipts, closed shift and exit.
