# Phase 4 validation — October 10, 2026

**87/87 Phase 4 checks passed in Unity 6000.3.26f1 Play Mode.** The original Phase 3 regression also passed **19/19** checks. Unity MCP's final Console read returned zero errors and zero warnings. The editor was left out of Play Mode with ShopkeeperShift open, saved, and clean.

Evidence:

- [Final Play Mode results](Diagnostics/Phase4/playmode-validation.json): production interaction detection/use, receive-once behavior, carrying, synthetic Q return input, stocking, order picking, over-pick/wrong-product rejection, atomic sales, duplicate-sale protection, receipts, task completion, shift close, closed-state rejection, inventory conservation, walking, shelf collision, ceiling camera clearance, visual stock counts, clear capsule routes to every station, and actual E-triggered navigation to TownPrototype.
- [Original gameplay regression](Diagnostics/Phase4/phase3-regression.json): unchanged existing validator, including original movement, animations, camera, box handling, door and workstation interactions.
- [Independent verification](Diagnostics/Phase4/independent-verification.json): all 90 original Prototype files match baseline SHA-256 hashes; replaying the inventory journal gives 18 units delivered, nine sold and nine remaining. Each SKU ends with three on shelves and three in the sold account, with no stranded delivery, dock, carry or basket stock.
- [Receipts](Diagnostics/Phase4/playmode-validation.json): BAU-0001 $5.10, BAU-0002 $5.20, BAU-0003 $6.20. Revenue totals $16.50. All job tasks complete; stability contribution records +3.
- [Environment budget](Diagnostics/Phase4/environment-budget.json): 873 product instances, 1,158 mesh instances, 26 shared static meshes, 539,524 static triangles. This excludes the skinned character and is not a frame-time benchmark.
- [Model inventory](Diagnostics/Phase4/model-inventory.json): original Blender exports and material/geometry counts.
- [Visual reference comparison](phase4-reference-comparison.md): five final 1920×1080 in-engine views and explicit remaining quality gaps.

The test suite sets up player positions and drives production motor/interaction methods deterministically. It also tests a synthetic keyboard Q action and collider-aware grid connectivity. **No human keyboard/mouse playthrough or performance benchmark is claimed.** Customer service uses sequential named order cards; there are no customer NPCs or physical item-scanning animations. Stability is a recorded shift contribution, not an implemented persistent town simulation.

Reproduce with **Business as Usual → Phase 4 → Validate Complete Shift**, then `python Scripts/verify_phase4.py`. The original regression uses **Phase 3 → Validate in Play Mode**. Read [the complete play guide](phase4-shopkeeper.md) for manual play and receipt export location.

Source art, Unity source and metadata, original character source files, Blender collaborator file, generators, reports and Graphify navigation outputs are staged in the existing repository. Binary art uses the existing Git LFS attributes. No commits, pushes, paid assets, generation services, dependencies or new repository were introduced.
