# A work slip, not a documentation window

[Research](phase8a-ui-ux-research.md) and [design system](phase8a-ui-design-system.md) define the rescue. The rejected two-page book is replaced by a single compact clipboard with interchangeable JOB / ITEMS / RECORDS / TOWN slips. Both the visual composition and the Unity UI implementation are new; old authority and record delivery remain behind the interface.

## Comparison

The [editable Figma comparison](https://www.figma.com/design/J2QJX5ZjjCNuMgbK2achnI?node-id=2-33) includes three distinct layouts with the same repair content: clipboard (one focal action and visible sequence), field notebook (side tabs, separate leaves), folded sheet (four narrow instructional columns). Clipboard wins because a player can reference one illustrated current step without turning pages. The notebook hides context during comparison; the folded sheet constrains records and quantities. A two-page view would only earn its width for a real document comparison.

![Three layouts and three actual font pairings](../Diagnostics/Phase8ARescueV2/handbook-concepts-and-type.png)

## New behavior

Tab opens the work slip. The selected tab remains attached to the same paper surface. JOB shows a five-step illustration strip, one dominant object/action and a short input hint. ITEMS shows carried objects. RECORDS shows only the local profession's copies. TOWN shows shared supply and outcomes. H opens illustrated induction. WHY is optional and replaces its own inset instead of covering content. Escape closes the innermost view.

Normal play carries none of these paragraphs. The physical latch, battery posts, shelf vacancy, carton and chute must explain where the action occurs. The paper illustration must match that object. If the world still needs a floating cream slab to explain an interaction, that interaction has not passed rescue acceptance.

## Verification status

The new clipboard is implemented as a native TMP/uGUI interface. Native 720p and 1080p captures compare the three real font options, with Bricolage + Atkinson selected. JOB, ITEMS, RECORDS and TOWN render without reported text overflow; actual pointer/keyboard flows cover solo start, profession choice, introduction and reopening views. Resizing and a 1920×1200 Unity render are captured separately. Records contain only the targeted profession's copies; public inventory snapshots omit audit truth.

All three solo professions complete a conserved shift. Native repair, stocking/checkout, cart handling, sorting and report evidence is linked from the [before/after gallery](../Diagnostics/Phase8ARescueV2/review.html). Technical automation does not establish first-time-player comprehension. The Figma Starter call limit prevented final post-fix screenshot retrieval; final Unity canvases remain editable and visually inspected. Known art/transition/performance limits are recorded alongside the evidence.
