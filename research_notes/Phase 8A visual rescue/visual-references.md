# Phase 8A Rescue V2 — Visual references and play readability

Research date: 12 October 2026. Scope: PEAK, inKONBINI, A Short Hike, Animal Crossing: New Horizons, Unpacking, Japanese retail packaging, and illustrated workplace communication. **Verified** below means a developer/publisher description or an inspected published image. **Observation** means a visual reading of that particular image, not a claim about every screen. **Proposal/inference** means our BAU design. No implementation was changed. Reference images downloaded in `reference-inspection/` are inspection material, not licensed game assets; the public report should link to the originals unless reproduction permission is established.

## 1. Which reference patterns make the next action understandable?

### Takeaway

Teach an action by putting a recognizable object, a short verb, and the result together. The strongest transferable pattern is a small job slip with a large current-step illustration and a few clearly ordered steps, supported by the same physical shape in the world.

### Cited Findings

- **PEAK — verified mechanics:** The developer-authored store page supports both solo play and up to four scouts, ledge assistance, ropes and climbing spikes, and physical survival items. Its social fun depends on observable acts of assistance in a shared environment, rather than a list of role statistics. [Developer store page](https://store.steampowered.com/app/3527290/PEAK/)
- **PEAK — observed art:** Inspected official screenshots show rounded characters with strong clothing/accessory silhouettes, broad environmental forms, and expressive faces. The inspected images hide HUD elements; they establish world-art personality, not a verified compact-HUD layout. [Inspected official forest-bridge screenshot](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/3527290/84436aac3675c39a9e21a14a4a9a3a8a7d9dea26/ss_84436aac3675c39a9e21a14a4a9a3a8a7d9dea26.1920x1080.jpg?t=1790591892). [Publisher press kit](https://landfall.se/peak-press-kit)
- **PEAK — verified accessibility:** Landfall explicitly documents offline single-player, key rebinding, and reduced head bob. This is evidence that cooperative identity can coexist with an accessible solo entry point. [Landfall FAQ](https://landfall.se/faq)
- **inKONBINI — verified workflow:** Its developers describe stocking, tidying displays, ordering goods, exploring nearby surroundings, and customer conversations. Warm lighting and an early-1990s Japanese setting serve the rhythm of ordinary tasks. [Developer store page](https://store.steampowered.com/app/2723430/inKONBINI_One_Store_Many_Stories/)
- **inKONBINI — observed screenshot:** The inspected storeroom image uses a tiny basket illustration and Tab glyph in one lower corner. The employee, crate, shelf contents, desk tools, and wall clipboard remain recognizable, supported objects. Product families have different silhouettes and packages, but repeat orderly within a family. This is a useful visual hierarchy: functional background order, one contextual control, and a believable handling prop. [Inspected official screenshot](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2723430/3faf1890ffd79b3e0aea23f92ff33c675ca99aaf/ss_3faf1890ffd79b3e0aea23f92ff33c675ca99aaf.1920x1080.jpg?t=1788165673)
- **A Short Hike — verified navigation:** The developer describes both following marked trails and exploring freely, with other hikers and hidden treasures along the way. [Developer website](https://ashorthike.com/)
- **A Short Hike — observed screenshot:** An orange-brown trail and bridge stand out against green vegetation and turquoise water; the flying character is readable through body silhouette and a curved motion trail. The image contains no instruction paragraph or permanent marker stack. This establishes environmental composition, not a claim that the whole game has no HUD. [Inspected developer screenshot](https://ashorthike.com/images/screen3.png)
- **Animal Crossing — verified information architecture:** Nintendo separates recipe reading, material gathering, and crafting into three steps and describes the NookPhone as a place to keep distinct collections and recipes. [Nintendo game page](https://www.nintendo.com/en-gb/Games/Nintendo-Switch-games/Animal-Crossing-New-Horizons-1438623.html)
- **Animal Crossing — observed recipe image:** The recipe screen puts the object name and a large net illustration on the left, material icon/name and Have/Need values on the right, and the primary action below. Its cream panel is rounded, with a clear selected action and visible back/scroll glyphs. Both areas have functional content. [Inspected Nintendo recipe screenshot](https://www.nintendo.com/eu/media/images/08_content_images/games_6/nintendo_switch_7/nswitch_animalcrossingnewhorizons/NSwitch_AnimalCrossingNewHorizons_Islandlife_DIY_Slider_Scr01.jpg)

### Inferences

**Recommended BAU information architecture:** a compact single-page field slip, with four attached tabs: JOB / ITEMS / RECORDS / TOWN. JOB is the default. A small profession strip gives identity; the current action illustration occupies the most useful area; four to six thumbnail steps show context. ITEMS shows what is physically carried and available locally. RECORDS presents only the player's permitted slips. TOWN is an explicit expanded map. This adapts category separation and object-first explanation without reproducing a NookPhone or another game's book.

Compare three layouts using identical mechanic content:

| Proposed BAU layout | Strength | Weakness | Decision |
|---|---|---|---|
| Single-page field slip | One focal area, fits a short action sequence, lowest visual overhead | Records need another tab | Recommended default |
| Clipboard with interchangeable slips | Customer orders and service tickets feel physically related | Large clip can consume useful space | Use a small top-edge clip, never over text |
| Two-page manual | Could compare two records side by side | Invites empty second page, excessive width, binder obstruction | Reserve only for an explicit compare view if needed |

**Proposed compact HUD:** top-left profession icon + `Check Battery` + `2/5`; top-right small local map; near reticle a modest target highlight; bottom-center `E Check Battery` only when relevant. Carrying adds one small object thumbnail/count. A completion stamp replaces the previous objective briefly, then yields to the next. Do not maintain a second large NEXT panel, workday explanation, roster bar, and profession progress card simultaneously.

**Proposed mechanic lesson:** the handbook strip is `[hood] OPEN → [battery/probes] TEST → [two batteries/swap] REPLACE → [gauge] VERIFY → [ticket/stamp] FINISH`. The live world initially highlights the actual hood handle. Opening exposes the battery and reveals the next target. The player must be able to point to the same object shown in the diagram. Detailed electrical reasoning is optional expandable material, not required to perform the first action.

**Proposed solo communication:** label Start Solo plainly in the main menu. When an unattended service dispatches a package, use a physical incoming package and a receipt stamped `TOWN DELIVERY`, plus a brief arrival signal. Never label an automated action as another human's work.

### Gaps

- These are source and screenshot inspections, not human usability tests or full hands-on playthroughs. No quantitative claim about novice comprehension is supported.
- A PEAK guidebook exists in official Steam announcement data, including localization and render-resolution fixes, but the exact tutorial page layout was not inspected from a primary-source image. Do not claim a particular illustrated-guidebook composition is verified. [Steam news API](https://api.steampowered.com/ISteamNews/GetNewsForApp/v0002/?appid=3527290&count=100&maxlength=0&format=json)
- The official retail galleries are selected images; hidden HUD cannot prove a game's normal interface is absent.

## 2. What makes physical objects understandable and satisfying?

### Takeaway

World objects should carry the explanation through recognizable form, placement, and state changes. A small interaction that visibly changes an object is more useful to BAU than an elaborate animation disconnected from the transaction.

### Cited Findings

- **Unpacking — verified object language:** The developer describes pulling possessions out of boxes, fitting them into homes, arranging familiar household items, and discovering a character through possessions rather than explicit narration. [Developer website](https://www.unpackinggame.com/)
- **Unpacking — verified access:** The same site says reading is not required to complete the game, required actions do not demand dragging/holding/multiple simultaneous buttons, invalid-outline color can change, and room-transition animation can be disabled. [Developer accessibility statement](https://www.unpackinggame.com/#accessibility)
- **Unpacking — observed image:** The inspected bedroom screenshot uses distinct silhouettes for radio, ball, books, toys, and boxes; furniture presents visible receiving surfaces, and objects actually rest on those surfaces. The open cardboard box and packing material communicate the source of the action. [Inspected developer screenshot](https://unpackinggame.com/wp-content/uploads/2021/09/Screenshot02.png)
- **Unpacking — verified feedback example:** Witch Beam artist/programmer Tim Dawson explains that an alarm clock's display changes once placed in a valid location; moving it resets the display. He also notes that some personal objects are familiar to knowledgeable players but mysterious to others. [Developer article](https://blog.playstation.com/2022/05/06/objects-tell-stories-in-unpacking-arriving-on-ps4-and-ps5-may-10/)
- **inKONBINI — verified sensory direction:** The developer describes tactile stocking/tidying routines and subtle everyday sound rather than complex optimization. [Developer description](https://store.steampowered.com/app/2723430/inKONBINI_One_Store_Many_Stories/)

### Inferences

**Proposed first-person object contract:** every active object must answer three questions without a paragraph: What is it? Where does it belong? What changed? A replacement battery needs a handle, terminals, label, and plausible receiving tray. A hood needs a recognizable seam and hinge. A paper bag needs a tied/open state and category marker. A stock box needs a printed product silhouette matching the shelf tag. A floating cream slab answers none of these questions.

**Proposed physical feedback:** use a brief handle reach, supported load pickup, visible removal from the source, correct grip, settling into the destination, and one completion cue. Animate toward the physical contact point. The state change must align with the server-confirmed action. A rejected action should leave the item where it was and show a small specific cue (`Hands full`, `Open hood first`), not play a misleading successful placement.

**Proposed accessibility and convenience:** keep a simple input option for manipulation even if a more tactile gesture is available. For example a terminal diagram can show a short tool arc and press/hold alternatives, without requiring tiny mouse motions for basic solo completion. Give reduced-motion options for camera bob and paper animations; retain object state changes because they explain the work.

**Proposed acceptance checks:** from the normal standing camera, verify wheels touch the floor, battery bottoms touch trays, receipts touch desks, shelf products touch shelves, and cart bags sit inside the cart. Inspect approach, pickup, carrying, placement, and post-placement. A parked object should not slowly interpolate from an arbitrary initial origin after the scene is visible. Thumbnail illustrations must resemble production props, so a car redesign must update the hood/battery drawings as well.

**Pitfalls:** Unpacking's full-room overview cannot be transferred literally into a first-person task; BAU needs clear target visibility and nearby signage. A Short Hike's color-separated routes suggest world navigation, but BAU's private future objectives cannot become public arrows. PEAK's mistakes are humorous partly because the world reacts; BAU should not fake accidents by flinging critical inventory irretrievably out of bounds. Charm does not excuse ambiguous controls or wrong scale.

### Gaps

- No source establishes universal ideal interaction duration or BAU animation speed. The user's 0.12–0.3 second UI target is a starting constraint to test, not a reference-game measurement.
- Screenshots cannot demonstrate input latency, actual object collision, animation blending, or network correctness. Those require the delivered player build.

## 3. How should Japanese retail and training-card influences become a coherent BAU language?

### Takeaway

Borrow disciplined labels and illustrated spatial explanation, not ornamental Japanese text. Let common functional templates carry personality through materials, modest stamps, and product-family graphics.

### Cited Findings

- MUJI states three principles: material selection, process simplification, and simplified packaging. It explains the light-beige paper produced by omitting bleaching and its use in labels and packaging. [MUJI principles](https://www.muji.com/jp/about/en/)
- MUJI's packaging team describes developing packaging alongside the product and using common packaging bodies with stickers for differing product information/barcodes. [Ryohin Keikaku packaging interview](https://careers.muji.com/jp/story/package-material/)
- **Observed Meiji packaging:** The inspected official image uses cream pouches, strong product names, chocolate/product illustrations, a narrow category color band, and separately aligned quantities. The image demonstrates hierarchy across related products rather than an arbitrary rainbow of equally dominant labels. [Inspected Meiji package image](https://www.meiji.co.jp/products/brand/simplepackage/images/pkg_mv.png), [brand page](https://www.meiji.co.jp/products/brand/simplepackage/)
- JISHA's illustrated hazard-prediction training material organizes workplace situations into categories including transport, cleaning, and machine work, with situational illustrations rather than isolated symbols alone. [JISHA illustration overview](https://members.jisha.or.jp/login/intro/kyt/kyt_index.html)
- Japan Standards Association identifies public-information pictograms as a distinct symbol system and provides background on their standardization and transport use. [JSA public-information symbol overview](https://webdesk.jsa.or.jp/common/W10K0010/?page_id=JIS_Z_8210_1&post_type=book_common)

### Inferences

**Proposed BAU palette:** warm cream is the document ground, dark teal the text/outline, mustard the current selection, green a completion accent paired with a check/stamp, coral an exception paired with a clear symbol/label. Profession accents change small strips and illustrations, not the whole template. Reserve black/teal for crisp numerical values. Paper grain must not obscure glyphs or substitute for a coherent grid.

**Proposed icon grammar:** each profession has one identity icon; each action has an object-plus-motion illustration. Use a battery with probe tips for testing, two batteries with exchange arrows for replacing, a bag moving into a labeled bin for sorting. A wrench alone identifies mechanic work but cannot differentiate request, diagnosis, removal, and verification. Keep input glyphs separate from illustrations and use actual bindings.

**Proposed instruction-card construction:** a numbered action label, one three-quarter object illustration, one direction arrow, and one clear result. Highlight the target contact point in the same restrained mustard used for selection. For the hood, include the hinge/body in the drawing so the motion is understandable. For a package transfer, show box → supported tray, never box → abstract hovering slab. Keep detailed reference tags on an inspectable item or record, rather than printing registry IDs at head height in the world.

**Proposed labels and receipts:** build one shared label body with product silhouette/name, quantity, destination symbol, and a small secondary identification field. A shipping label and shelf tag should agree on the object image. A receipt puts what/quantity/when in aligned rows, with local document scope and actual omissions preserved. `Checked` can describe a completed work check; it must not become a future automatic honesty/guilt indicator.

**Proposed consistency checks:** inspect every new icon at 24, 32, and 48 px; reject symbols needing tiny interior detail to be distinguished. Check selected/focused buttons without relying solely on hue. Make the page illustration large enough to read at 720p, while maintaining clear text hierarchy. Use a single uniform UI scale and independent responsive content constraints; do not stretch drawings or binder decoration to fill ultrawide space. A narrow panel should stack supporting information instead of scaling X and Y differently.

### Gaps

- The cited safety sources do not license their illustrations as BAU production assets; draw original work-specific diagrams. These sources inform explanation methods, not an assertion of formal safety-sign compliance.
- Retail examples are specific brands. They do not justify treating all Japanese design as one aesthetic or using untranslated Japanese as filler.
- Font licenses, exact font pairings, and Unity layout implementation are covered by the adjacent research/implementation work, not this note.
