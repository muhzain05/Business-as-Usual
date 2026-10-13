# Phase 5 network architecture

Authority plan recorded before gameplay implementation, 2026-10-10.

## Audit

Unity MCP verified Unity 6000.3.26f1, ShopkeeperShift, a clean scene, and the actual scene references. The editor was running Play mode at audit time. The scene has one SunnyPlayer with PrototypeInput, ThirdPersonMotor, PlayerInteractor, ShopHUD and ShopRestockReach; its child camera references that player's input. Stations, crate, shelf displays and till reference the single ShopShift. Shelf displays additionally reference the scene player's reach component. There are no gameplay assembly definitions; gameplay compiles in Assembly-CSharp. Meshy's editor assembly is separate. Multiplayer Center is installed, but NGO and Transport are not yet installed.

Phase 4.2 is integrated: ShopHUD uses ShopUIView and ShopUITheme with approved bundled fonts/art. Retain this interface. ShopDomain has integer inventory, atomic bundles and order-ID receipt deduplication. ShopShift owns delivery, carry, shelf, basket and sold accounts, tasks and three fixed orders. The basket is intentionally shared for cooperative fulfillment. Phase 3 supplies motor, camera, interaction detection, doors and basic workstations.

Single-player assumptions: every PrototypeInput enables keyboard/mouse and cursor capture; every motor polls input; every camera tracks its serialized player; ShopShift polls global Q; ShopHUD renders on every enabled instance; ShopShift.Carried identifies a crate but not its holder; shelf use assumes any player may use that crate. Serialized camera/HUD/reach references must be rebound for network instances.

Git contains 1198 staged paths, 11 of them additionally modified, and 33 untracked entries at audit. Preserve the index and all source art. Baseline staged entries and working-file hashes are under Diagnostics/Phase5. Do not commit or push.

## Chosen integration

Use NGO 2.7.0 and its Unity Transport dependency, direct IPv4 localhost/LAN UDP, port 7777. No cloud services. Create a separate ShopkeeperMultiplayer scene and SunnyNetworkPlayer prefab from the existing scene/player. Preserve the original single-player scene, rig, clips, meshes and materials. Small hooks route local interactions and suppress global Q only during multiplayer.

The host owns gameplay and movement. Each connection owns one NetworkObject player. Owner input sends bounded camera-relative movement, sprint and jump commands at 30 Hz. The host calls the existing ThirdPersonMotor.Step with its own elapsed time and authoritative CharacterController collision. Clients interpolate server NetworkTransform updates and drive the existing Speed parameter plus replicated grounded/jump/running presentation. No client position or inventory values are accepted. This first LAN implementation has input latency and no prediction; keep that limitation explicit.

Only the local owner enables PrototypeInput, PlayerInteractor, ShopHUD, camera and AudioListener. The original scene player is inactive in the network scene. Server-side player controllers remain active for physics, but their automatic input-driven motor Update is disabled. Remote reach animation is presentation only.

NetworkShop adapts the existing ShopShift rather than replacing its domain. Stable scene interaction IDs use hierarchy paths. Requests originate from the owned player, carry a monotonic sequence and observed shared revision/order ID, and execute serially on the host. The host checks current revision, range, line of sight, object availability, holder, busy state and legal transition. Two requests for one revision cannot both mutate stock. Checkout also matches the observed order ID. Existing atomic stock transfers and receipt deduplication remain the transaction boundary.

One authoritative crate holder and pose are replicated with shared snapshots. E picks up the crate, restocks the selected shelf while carrying, or drops the crate away from shelves on validated clear ground; Q returns remaining stock to its original dock. Drop moves the same crate and conserves its contents. Disconnect returns a held crate to the dock before player despawn. Decorative products remain local; only quantities, orders, receipts, journal, tasks, meaningful interactions and the two players are synchronized. Shared snapshots are reliable, revisioned and sent on mutation and join, not every frame. Clients restore read-only views into the existing ledger/UI. Receipt timestamps and exports originate on the host.

The reused Phase 3 graybox door/workstation are diagnostic fixtures. They are inactive in the saved multiplayer scene. `NetworkShop.Awake` activates them only when all standalone test-driver arguments are supplied, before collecting interaction targets. Ordinary sessions have no fixture renderers, colliders or interaction targets. This gate reads the command line directly and does not depend on component Awake ordering.

The existing `dock` inventory account means the available, unheld crate's contents in this slice, including a crate placed on the floor. A separate replicated `dropped` flag and world pose distinguish that state, and the handbook labels those quantities as dropped-crate stock. Pickup still uses the existing dock-to-carry transfer. Both server and client detach the scene crate before a holder despawns; client presentation parenting must not make it part of a player's destruction lifecycle.

Connection UI uses the existing handbook theme. Host/client/default-address controls appear before connection, plus a compact identity/status strip during play. A stopped/failed session reloads the network scene to restore a clean state; reconnection receives the current host snapshot. Host loss ends the client session; no host migration or persistence is promised. Network Exit cannot let one client independently load another scene.

## Verification plan

Run production Phase 3/4/4.1/4.2 validators with fresh evidence archived and previous reports restored. Add server invariant checks and a deterministic two-process Windows build harness exercising connection/ownership, actual transport movement, contention, range, stocking, sales, shared progression, drop, disconnect/reconnect and host loss. Capture both processes, compare authoritative snapshots, and report update/message counters. Automated input is not human playtesting. Record failures and limitations without claiming unperformed checks.

## Compatibility reference

Unity lists NGO 2.7.0 for Unity 6000.0: https://docs.unity3d.com/ja/6000.0/Manual/com.unity.netcode.gameobjects.html . Verify installed package source and compilation against the actual editor before building.
