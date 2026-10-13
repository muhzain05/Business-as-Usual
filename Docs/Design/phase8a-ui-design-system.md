# Phase 8A Rescue V2 workday design system

Status: Rescue V2 runtime implemented; native acceptance and remaining limits are recorded in the review gallery. [Research](phase8a-ui-ux-research.md) informs every decision below. [Editable Figma kit](https://www.figma.com/design/J2QJX5ZjjCNuMgbK2achnI).

## One language across the town

A cream work slip carries a single current action and its illustration. JOB / ITEMS / RECORDS / TOWN attach to that surface. Main menu, staff desk, orders, service tickets, collection slips, pause/settings and end-of-day report use the same type, spacing, buttons and feedback. Normal play retains only profession/action, five small progress marks, a compact map, one distant destination and a nearby object prompt. There is no persistent NEXT paragraph.

| Token | Value | Purpose |
|---|---|---|
| Paper | #F7EBCD | Work slips |
| Ink | #203F40 | Reading, outlines |
| Light | #FFF9E9 | Inset cards / buttons |
| Dark | #152E2D | Backdrop / signs |
| Mustard | #E7B647 | Current action / selection |
| Coral | #D36F55 | Corrective message |
| Green | #3E735D | Confirmed completion |
| Rule | #D6C5A1 | Quiet divisions |

Use words and shapes with colors. A failed transaction receives corrective copy rather than a success stamp. Public progress does not infer guilt. Private records are read from targeted profession copies; the UI never owns or duplicates inventory.

## Type and geometry

Three actual heading pairings are compared in Figma using identical repair content. Selected: Bricolage Grotesque 96pt ExtraBold display, Bricolage 12pt Bold labels, Atkinson Hyperlegible Regular reading and true Bold numeric/control values. Dela and Fredoka remain comparison assets. Preserve exact OFL notices and pinned source hashes.

1280 × 720 reference canvas; CanvasScaler uses uniform Scale With Screen Size and a 0.5 width/height match. No separate X/Y scaling. Display 40–44, paper title 32, action 24–28, body 18, prompt/numeral 20, nonessential status 14–16. Use an 8-pixel spacing grid with 24-pixel document inset. Larger text may wrap in its allocated region; essential instructions are not ellipsized. Amounts receive dedicated columns rather than relying on proportional default digits.

The handbook is a 640 × 514 single work slip. Its clip is above the reading region. Five illustrated thumbnail steps lead to a current-action image/verb/control. Optional WHY replaces the action inset with explanation; it cannot overlap the diagram. Maps preserve region aspect ratio. Icons preserve aspect ratio and come from one original geometric source that also exports editable SVG.

## Components and motion

Shared ScriptableObject skin supplies fonts, colors and feedback timing. Reusable canvas, paper-panel and reading-text prefabs supply Unity components. Button focus/press states, tab selection, input keycaps, step tiles, record rows and result slips follow these foundations. The Figma kit has semantic aliases, scoped variables, text styles and component-state variants.

Paper entry is 0.18 seconds and never delays input. Focus changes shape/contrast; reduced motion removes decorative movement. Success text follows server acceptance, while a reach/tool movement may respond locally. Hood, item and grip animations must relate to their actual target and are validated in later stages.

## Screens and acceptance

Main: explicit START SOLO, HOST WORKDAY and JOIN WORKDAY. Staff: three illustrated profession cards plus multiplayer-only assistant selection. Induction: control glyphs and five illustrated verbs, no paragraphs. Inventory: actual carried resource IDs and quantities. Records: one readable ticket with optional pagination. Town: supply and shared outcomes. Pause/settings: local preferences with real focus and reduced motion. Report: actual completion rather than a generic success assertion.

Required native captures: 1280 × 720, 1920 × 1080, 1366 × 768, 1920 × 1200 and resized windows. Inspect rendering plus real text bounds, clipping, hierarchy, font atlases, aspect, sorting and button interactions. Where desktop size prevents a native client area, distinguish an offscreen player render from a window-size test. No human usability claim without participating players.

## Delivered runtime and evidence

`Assets/RescueV2/Resources/RescueV2/WorkdayCanvas.prefab`, `Components/PaperPanel.prefab`, `Components/ReadingText.prefab` and `WorkdaySkin.asset` are the shared editable Unity sources. Runtime builders use those panel/text foundations for buttons, tabs, glyphs, step tiles, record rows and feedback. Button hover/focus/press/disabled states come from RescueButtonFeedback. Twenty-two original pictograms retain editable SVG counterparts; six licensed static fonts retain their OFL/source manifests. Final choice is Bricolage + Atkinson; Dela + Atkinson and Fredoka + Atkinson are actual native comparisons.

The Figma file contains foundations, three layout comparisons, three typography comparisons, reusable variants and 16 editable production screens (87 instances, 170 text nodes). Production icon scale/button-height fixes were applied. Figma's Starter MCP call limit prevented fetching the final post-fix render; the final Unity canvases and original vector sources are the fully inspected editable equivalent. Earlier Figma images remain labelled as intermediate evidence, not final acceptance captures.

The native screen matrix covers main/join, staff desk, induction controls/job, HUD, handbook tabs, map, pause, settings/display, controls and report at 720p and 1080p. Same-process resizing covers 1366×768, 1280×720 and 1920×1080. Six 1920×1200 compositions are genuine Unity offscreen renders because this desktop cannot fit a native 1200px window. See the [raw validation and gallery](../Diagnostics/Phase8ARescueV2/review.html); no browser reconstruction is presented as game evidence.
