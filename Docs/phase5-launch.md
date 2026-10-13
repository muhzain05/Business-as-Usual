# Launch the two-player prototype

Build output: `D:\Business as Usual\Builds\Phase5\BusinessAsUsual.exe`. Keep the executable together with its `_Data`, `MonoBleedingEdge`, DLLs and other build files. It is a Windows Development build.

## Same PC

Run the executable twice. In the first window choose **START HOST**. In the second leave the address as **127.0.0.1** and choose **START CLIENT**. Two Sunny characters spawn separately in Daylight Market. Click a game window to control its local player. Esc releases the mouse so you can switch windows.

Alternatively, from PowerShell in the project root:

```powershell
& '.\Scripts\launch_phase5.ps1' -Mode host
& '.\Scripts\launch_phase5.ps1' -Mode client
```

The launcher opens a visible game window and selects the requested mode. Run without `-Mode` to open the connection desk. If your PowerShell policy blocks scripts, use the executable and its buttons; no policy change is required.

## Two PCs on a LAN

Copy the complete `Builds/Phase5` directory to the other Windows PC. Start Host on the first PC. Use that PC's LAN IPv4 address (from `ipconfig`, for example `192.168.1.69`) in the other PC's connection desk and choose Start Client. The host listens on all IPv4 interfaces, UDP port **7777**. Allow the game on your private network if Windows Firewall prompts. Both PCs must be on a reachable LAN; there is no internet relay, port-forwarding setup, matchmaking or cloud account requirement.

```powershell
& '.\Scripts\launch_phase5.ps1' -Mode client -Address '192.168.1.69'
```

Replace the example with the host's actual IPv4 address. The slice admits two players, including the host.

## Controls and cooperative shift

- WASD moves; mouse orbits; Shift runs; Space jumps.
- **E** uses the closest reachable station or picks up the stock crate. While holding the crate, E restocks the selected shelf; away from a shelf, E places it on clear ground. **Q** returns a held crate and its remaining contents to the delivery dock.
- Esc releases the pointer; clicking outside the handbook restores capture. Tab opens/closes the receipt book. The connection card's Disconnect button is available with the pointer released.
- The identity/connection card names the local Sunny. Only the holder sees the carrying card. Tasks, quantities, the customer basket, orders, revenue and receipts are shared.

Receive the delivery at the rear desk, pick up the crate, and use each labeled shelf to restock six units. The carrier stocks; their colleague cannot take or stock from the same held crate. An empty crate returns automatically. Collect the requested products from shelves into the shared basket, then use Checkout. Either player can collect or checkout. Repeat for Mara, Eli and Jo, then use the Closing Terminal.

Both windows should finish with **three unique receipts**, **$16.50** revenue, **six completed tasks** and **+3 Town Stability**. Each SKU started with six units: three sold and three remain on its shelf. The printed receipt IDs/timestamps and journal should agree in both windows.

To verify contention, have both players try to take the same crate. Exactly one should carry it; the other receives feedback. To verify recovery, disconnect the carrier before restocking finishes. The host retains the shift and returns the crate to its dock. Reconnect through the connection desk to receive the current shared state. Disconnecting the host ends the client session; there is no host migration.

The automated test harness enables an existing Phase 3 door and workstation as shared interaction fixtures; these graybox props are disabled during normal play and in the saved scene. Door toggles and workstation uses are server validated and shared in test mode. The multiplayer Town Exit does not independently transition one client to another scene. Disconnect, then select Single Player if you want the original town navigation.

## Single player and rebuilding

Choose **SINGLE PLAYER** at the connection desk, or use `launch_phase5.ps1 -Mode single`. This loads the unchanged original `ShopkeeperShift` scene and its original controls; Q retains its original behavior. In Unity you can also open `Assets/Shopkeeper/Scenes/ShopkeeperShift.unity` directly.

Open `Assets/Multiplayer/Scenes/ShopkeeperMultiplayer.unity` for Editor host testing. The menu **Business as Usual / Phase 5 / Build Windows Prototype** rebuilds the Windows executable. **Create Multiplayer Scene** regenerates only the Phase 5 scene and prefab from the original scenes; stop Play and preserve unsaved scene work first. Do not regenerate it if you have made custom edits to those generated Phase 5 assets.

## Repeat automated transport validation

Stop any previous prototype instances and run:

```powershell
python Scripts/test_phase5_network.py
```

This launches two real Windows processes on localhost and writes reports, logs and screenshots under `Docs/Diagnostics/Phase5/TwoProcess`. It drives network input/commands and uses opt-in server teleports to set up test cases. Ordinary play does not enable the test harness. Its result is automated integration testing, not a human keyboard/mouse playtest. Startup has a 60-second allowance. A successful test closes its game processes.

LAN routing and human-controlled comfort should still be tested on your own two-PC setup. This first server-driven movement slice has no client prediction, host migration, saved-session restoration or internet service integration.
