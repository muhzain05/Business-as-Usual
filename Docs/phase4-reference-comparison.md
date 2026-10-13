# Phase 4 — visual reference comparison

The supplied chat images are the primary visual target: a cashier-facing compact convenience store, a close-up of a beverage refrigerator, a detailed checkout/register/basket, and colorful stocked cold shelves. The images themselves remain chat attachments; the repository captures below are actual Unity renders of original assets.

## Cashier-facing store

![Completed shop, looking from checkout toward the store](Diagnostics/Phase4/cashier-store.png)

The composition uses a foreground register, scanner, basket and impulse products; center gondolas; side-wall stock; and refrigerators at the rear. Cream, teal and terracotta fixtures, warm lights and illustrated fictional brands provide the cozy color direction. Rounded manufactured edges, drawers, register keys, vents, rails and basket lattice replace the initial functional blocks.

## Packaging and refrigeration

![Refrigerator and original product packaging](Diagnostics/Phase4/refrigerator-detail.png)

![Product assortment and aisle](Diagnostics/Phase4/product-detail.png)

Eight original packaging designs cover bottles, cans and cartons. Their artwork includes Sunbeam, Meadow, Golden, Cloud, Orchard, Tide, Little Fox and Garden branding. Repeated stock shares meshes and materials. The refrigerators have wire shelves, vents, category headers and practical light strips. Price cards and shelf rails add small-scale retail detail.

## Checkout equipment

![Register, scanner and basket](Diagnostics/Phase4/checkout-detail.png)

The modeled register includes a keypad, cash drawer/lock, display housing, receipt slot and paper. The scanner has a grip, optical head, stand and cable; the basket has open lattice and a handle. The till amount follows the last completed transaction during Play Mode. The digital receipt book provides readable itemized sales records.

## Layout and accessibility

![Overhead shop layout, roof excluded for clarity](Diagnostics/Phase4/overhead-layout.png)

Checkout is front left, shift review front right, receiving and the stock crate rear right. The two center gondolas have stocked reverse faces. The front sides of the three labeled gameplay shelves carry the counted **SHIFT STOCK** rows. Side displays and cold-wall assortment are decorative. Collision is assigned to whole fixtures instead of hundreds of small packages, preserving navigation and reliable interactions.

## Reference comparison and remaining gaps

| Reference characteristic | Current result | Remaining gap |
|---|---|---|
| Dense colorful assortment | Hundreds of products, eight designs, stocked wall displays and double-sided gondolas | Repetition is more visible; fewer shapes, sizes, bagged snacks, prepared foods and small accessories |
| Detailed shelf construction | Beveled decks, price rails, uprights, punched details, ribs and vents | Less wear, trim variation and irregular hand-painted surface detail |
| Polished cashier equipment | Modeled register, keys, drawer, scanner/cable, lattice basket, receipt and live till amount | Fewer decals and subtle materials; no physical per-item scanning animation or receipt-feed animation |
| Warm, intimate lighting | Warm practical fixtures, directional shadows, illuminated cold shelves and oak floor | Realtime lighting is simpler; no bespoke baked indirect light, reflection pass or tuned ambient occlusion |
| Compact believable store | Working retail layout with clear circulation, windows, signage and plants | Room proportions and open areas differ from the reference; more architectural dressing would improve intimacy |
| Characterful customers | Three named, sequential customer order cards and a complete service loop | No visible customer NPCs, dialogue, queues or bespoke performances |
| Close-up packaging quality | Original illustrated textures and modeled bottle/can/carton silhouettes | English fictional branding and flatter graphics; below the reference's close-up painted detail |

The result is a functional original stylized store with detailed assets, rather than the temporary graybox recorded in [functional-before.png](Diagnostics/Phase4/functional-before.png). **It is still below the reference's finished art quality.** The highest-impact remaining work is a broader handcrafted product set, prepared-food displays, more localized surface treatment, a lighting/reflection pass, and visible customers. None is presented as already implemented.

## Evidence and reproduction

- Gameplay validation: [playmode-validation.json](Diagnostics/Phase4/playmode-validation.json).
- Inventory and source preservation: [independent-verification.json](Diagnostics/Phase4/independent-verification.json).
- Model geometry/material budgets: [model-inventory.json](Diagnostics/Phase4/model-inventory.json).
- Play and rebuild instructions: [phase4-shopkeeper.md](phase4-shopkeeper.md).

Screenshots use dedicated capture viewpoints without altering Sunny's player camera. Art captures exclude Sunny; the overhead view excludes the roof. They do not substitute for a human gameplay session. Automated runtime tests exercise the production interaction methods and deterministic controller traversal.
