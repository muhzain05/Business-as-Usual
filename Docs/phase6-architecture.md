# Phase 6 architecture and authority plan

Audit recorded before implementation, October 11, 2026. Graphify identified the existing interaction, inventory, tasks, handbook and network adapter paths. Unity MCP verified the actual Shift/crate references, SunnyNetworkPlayer prefab, UnityTransport and 30 Hz ticks. Current Git index contains 1198 staged paths (1178 A, 20 AM); preserve it and all existing working files. Phase 4/4.2/5 documentation and original town builder were inspected. Prior multiplayer fixes are the current baseline: production E shelf dispatch and diagnostic fixture visibility.

## Extension boundaries

Retain InventoryLedger atomic transfers, StockMovement journal, TaskBoard/JobTask, Interactable/PlayerInteractor, ThirdPersonMotor/camera and ShopUITheme. Extend the existing NetworkShop command serialization, range checks, monotonic sequence, revision and snapshot boundary to include TownJobs. Keep ShopShift and its approved UI intact. TownJobs supplies role selection, mechanic/sanitation state and adapters, with an ordinary MonoBehaviour domain owned by the existing server adapter. No alternate transport, movement or inventory framework.

One player per profession. Six definition slots are supported, with only Shopkeeper, Mechanic and Sanitation selectable. Requests originate from the owned NetworkPlayer; the server checks assignment and task legality. Disconnect releases an assignment, cancels timed work, returns a carried bag to its source and preserves shift progress/resources for reassignment. Restarting the scene creates fresh state. Job choice has no connection to hidden roles.

Timed inspection, repair and sorting require holding E, remaining near the target and current server input. Completion consumes stock exactly once; cancellation does not. Records contain immutable IDs, fault/material, object/source location, actor and server UTC time. Decorative props remain local.

The parts ledger seeds a limited supply; shop packing transfers warehouse stock to the pickup counter, mechanic collection transfers it to their work account, and repair transfers it to installed stock. When Shopkeeper is unassigned the mechanic may collect from a documented municipal reserve, using the same stock. Receiving shop deliveries activates packaging waste; repairs activate garage waste. Sanitation collects and processes at least three marked route bags, including ordinary discarded objects. Extra cross-job waste is collectable without blocking shift closure when another profession is absent.

## Shared scene

Create a Phase 6 clone of TownPrototype, preserving its compact 32 x 28 m layout and workplace positions. Copy the approved Shopkeeper interior intact into a connected room at an offset; server-validated entry/exit links transport the same players within the same scene, retaining all job state. This accommodates the approved larger interior without expanding town streets or squeezing/rescaling its layout. Original scenes/prefabs/art remain unchanged. Clone SunnyNetworkPlayer for the town and bind the existing shop HUD only for a local shopkeeper; a single reusable profession handbook uses ShopUITheme and the same scale/control conventions for other jobs and selection.

## Validation

Fresh original regression runs are archived and original evidence restored. Use two independent Windows processes for different-job work, role authority, held-E progression/cancellation, shared transfers, stock conservation, bag ownership/drop/disconnect/reconnect, fallbacks, records, and shop completion. Capture the three interfaces at supported sizes and original/final town views. Inspect actual collision/navigation in Play Mode, report frame observations separately from geometry budgets. No human-controlled or two-PC LAN test claims. No paid generation, commits or pushes.
