# Daylight Town — Phase 6 play guide

Windows build: `Builds/Phase6/BusinessAsUsual.exe`. For two windows on this PC, run `powershell -ExecutionPolicy Bypass -File Scripts/launch_phase6.ps1 -Mode pair`. This opens an ordinary host and client with no automated gameplay. Alternatively open the executable twice and choose Start Host in one, Start Client in the other, using `127.0.0.1`. For two PCs, enter the host's LAN IPv4 address on the client and allow the game through the host's Windows firewall for UDP 7777. Two-PC LAN play still needs human verification.

Choose different jobs at the staff desk. J opens profession selection; a profession can belong to only one player. Finish an active shift before changing jobs. Only Shopkeeper, Mechanic and Sanitation are enabled; the job framework reserves three further slots. Town Stability accumulates from completed shifts. Disconnect releases the role and returns carried work while preserving this host's progress. Closing the host ends the session; a fresh session starts fresh shifts.

WASD moves, Shift runs, Space jumps, mouse looks. E interacts; **hold E** and stay still for inspection, repair and sorting. Releasing E or moving cancels timed work. Q safely returns a carried load. Tab expands the handbook. Esc releases the mouse; click outside handbook controls to capture it again. Each process accepts input only when its window is active.

## Mechanic

Enter Mikan Motor on the right side of the square. Press E at the rear service desk to receive two tickets. Hold E at each van's side inspection point: the delivery van needs a battery and the municipal vehicle needs an oil filter. File the parts request at the garage's right rear station. Collect at the parts counter outside Daylight Market. If a Shopkeeper is assigned, that colleague must pack the request inside the shop; if nobody is assigned, collection uses the limited municipal reserve. Hold E at each front repair point. Return to the service desk once to file both records and again to close the shift. Records appear in Tab → Records. Repairs generate two bags for Sanitation.

## Sanitation

At the covered yard south of the street, press E at the route desk to receive the route. Collect marked bags at the square, back alley and yard. Paper goes in Paper; the discarded umbrella and wrappers go in Mixed; replaced parts go in Metal. Hold E at the matching bin until sorting finishes. Drop with E in a clear space, or Q to return a bag. Return to the route desk to file the disposal manifest, then again to close after at least three recorded bags. You may also collect shop delivery packaging and garage waste before closing. Tab shows bag status, disposal records and the shift report.

## Shopkeeper

At Daylight Market, press E to open the exterior door, walk through it, then press E at the entrance station to enter the approved shop interior while keeping the same network session. Receive delivery, pick up the crate, and press E at each shelf to restock. Fulfill the existing three customer orders and use the closing terminal. The approved Shopkeeper handbook remains available inside. The parts packing station near the interior's front-right side supplies a colleague's filed garage request. Delivery also produces a Paper bag outside. Return carried stock before using the exit.

For all three jobs with two people, begin Mechanic + Sanitation. The mechanic can finish using the unstaffed reserve, then switch to Shopkeeper. Sanitation can collect the newly generated bags before closing. For direct parts cooperation, begin Shopkeeper + Mechanic: inspect and request parts, then have the Shopkeeper pack them.

## Reproduce automated validation

Open `Assets/Town/Scenes/DaylightTown.unity` in the existing Unity project. Use Business as Usual → Phase 6 → Build Windows Town. Run `python Scripts/test_phase6_network.py --width 1920 --height 1080 --output Docs/Diagnostics/Phase6/TwoProcess1080`, then `python Scripts/test_phase6_network.py --scenario staffed --width 1920 --height 1080 --output Docs/Diagnostics/Phase6/Staffed1080`. These spawn two independent Windows game processes, drive normal E dispatch and held input, and use opt-in test placement to reach stations. Their JSON reports, process logs and screenshots distinguish automated checks from manual testing.

The exterior-door/interior-entry check is `python Scripts/test_phase6_network.py --scenario entry --output Docs/Diagnostics/Phase6/MarketEntry720`. It uses E and real movement to enter the market; no direct placement inside substitutes for entry.

Original reusable Blender source is `Assets/Town/Working/DaylightTown_AssetLibrary.blend`; FBX exports, processed meshes and environment prefabs are under `Assets/Town`. New gameplay uses the existing ledger, tasks, interaction and network snapshot authority. Decorative town assets are local. Original town/shop scenes and approved Sunny assets are preserved.
