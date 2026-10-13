# Business as Usual — Living Town & Deeper Job Design

Source: user-supplied Phase 7B side brief, October 11, 2026. The referenced file was absent at task start; this records the supplied intent rather than claiming an earlier document was read.

The original `Working-Town Social Deduction — Game Design Doc.pdf` remains the primary creative reference. Use this alongside `Docs/phase7A-handoff-to-codex.md` and the Phase 7B manual QA addendum.

## Intended feel

- Schedule I: tactile first-person tasks, satisfying production workflows, convincing town activity.
- PEAK: charming interactions, physical comedy, social cooperation, expressive character behavior.
- Business as Usual: legitimate work conceals suspicious activity; incomplete information and meaningful cross-job interactions produce stories.

This is design and environment readiness work. Finish Phase 7B environment, Meshy, lighting, animation, minimap and rendering improvements first. Do not implement major job systems, NPC AI, criminal mechanics or extensive task minigames during Phase 7B.

## Required analysis and proposals

Audit the actual implementation for repetitive E interactions, tasks without decisions, unused buildings, missing dependencies, locations without a reason to visit, social encounter opportunities and reusable foundations. Prioritize findings by player impact.

Propose expanded Shopkeeper, Mechanic and Sanitation roles. Each must have a physical task loop, at least two meaningful decisions, a reason to visit another workplace, harmless mistakes, exclusive information and legitimate behavior that could look suspicious. Avoid sequences of progress-bar clicks.

Store ideas: physical scanning/checkout, replenishment, requests, inventory/supply choices, receipts/purchase history. Garage ideas: component inspection, diagnosis, manipulating/replacing parts, market supplies, vehicle usage records. Sanitation ideas: push cart, load/transport bags, physically sort, selectively inspect discarded items, disposal records.

## Compact town readiness

Support checkout and stockroom; garage bays and parts storage; sanitation loading and sorting; square gatherings and noticeboard; warehouse transfers; alley deliveries; homes/personal errands. Make buildings attractive with deliberate purposes. Do not expand beyond the compact town.

Low-cost atmosphere may include shutters, lit windows, chimes, signs, swaying trees, moving clouds, work soundscapes, delivery and collection staging. Avoid complex NPC schedules or full AI simulation.

## Deduction compatibility

Ordinary work must send innocent players through shared and potentially suspicious places. Alleys, warehouse/storage and sanitation cannot be criminal-only routes. Records remain partial clues with plausible innocent explanations, never automatic guilt identification. Plan private personal secrets and hidden objectives without implementing them yet.

## Deliverables and boundaries

Deliver a current gameplay-depth audit; three profession designs; location-to-gameplay map; dependency matrix; ranked improvements; scoped next-phase gameplay proposal. Phase 7B implementation is limited to approved visual/technical fixes and safe environmental affordances, including room layout, prop placement and clear interaction space. Preserve multiplayer, current jobs, UI design, original Sunny sources, staged/uncommitted work and the cumulative 300 Meshy-credit limit. No commits or pushes without approval.
