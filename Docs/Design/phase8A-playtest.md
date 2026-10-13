# Phase 8A Rescue V2 — play the living workday

Launch one window from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File Scripts/launch_phase8a.ps1 -Resolution 720
```

The launcher now defaults to **solo** and uses `Builds/Phase8A-RescueV2/BusinessAsUsual.exe`. Choose Shopkeeper, Mechanic or Sanitation. Read the two illustrated introduction cards, pressing Enter to continue. H reopens help. Four windows launch only with explicit `-Mode four`; that mode is for technical testing.

For the main menu rather than immediate solo start, use `-Mode desk`. Main offers START SOLO / HOST WORKDAY / JOIN WORKDAY. A host uses `-Mode host`; a second computer uses `-Mode client -Address <host-ip>`. Network sessions need workers in all three lead jobs to complete every town outcome; four players can add an assistant. Unattended services are explicitly part of solo, not silent multiplayer substitutes.

## Learn from the game

WASD moves; mouse looks; Shift runs when unladen; E uses the aimed object; R offers an optional action; Q returns a carried load or parks the cart. Tab opens the compact JOB / ITEMS / RECORDS / TOWN clipboard. M opens the route map, J reopens the staff desk, Esc opens local pause/settings. Settings include volume, look sensitivity, FOV, reduced motion, display size, fullscreen/windowed mode, quality, V-sync, frame limit and inverted look.

Follow the mustard destination marker, then aim at the highlighted physical object. The nearby prompt gives the input and verb. The handbook's five pictures summarize your job; WHY supplies optional detail. There is no permanent instruction paragraph over the world.

## Mechanic

Open the wagon's hood by its latch. Check its battery posts with E; this diagnosis requests the replacement. R performs one optional load test. A sealed parcel arrives on the receiving bench in solo through a labelled unattended market service. In multiplayer the market prepares it and a worker carries it over.

At the clamp, E starts the wrench interaction. Hold left mouse and drag right until the turn completes; Q cancels. R changes quick swap versus bench recovery before turning: recovery needs two turns and yields metal, quick swap needs one and yields mixed treatment waste. Lift the old battery and put it on the scrap tray. Open the received carton and carry its replacement to the battery seat. E installs it, leaving the hood open. Test the repair, close the hood, and sign the service ticket.

## Shopkeeper

Receive the rear shipment. Stock verification is optional at dispatch with R; E prepares the garage parcel. Carry the single carton to the garage receiving bench. The open store doorway shows the town and exits when walked through; it uses an offset interior transition.

For checkout, lift the labelled stock box, match it to the printed shelf, lift the requested product from that stocked shelf, place it beside the scanner, and scan it. The three available products are Sunbeam Cola, Meadow Tea and Golden Crackers. Each is one conserved stock item; scanning removes that exact item from circulation. Wrong stock and repeated scans cannot manufacture a sale. Customer receipts record this handoff and omit earlier handlers.

## Sanitation

E at the cart handle begins pushing; walking moves the server-owned cart and turns its wheels. Q parks it. Carry a batch to the cart and E loads it, freeing your hands. Up to three actual batches fit. Aim at a particular loaded batch to lift it; R at the handle also lifts a batch.

Collect the square sack, the opened garage packaging, and the old battery. Solo's labelled mechanic service produces the same repair waste while you work. Take the cart to the yard. R at a chute inspects the held material; E at the matching paper, metal or mixed chute sorts it. A wrong chute retains the object. File all processed batches at the manifest desk. Your optional inspection and collection priority affect the record and town supply.

## Finish

The shared town needs at least one checkout, a tested and signed repair, and all three collection tickets. When those are complete, the square clipboard stand signs off the shift. The report shows the outcome and supply total; VIEW RECORDS opens your profession's copies. NEW WORKDAY starts fresh. The default workday is eight minutes; timeout also closes the shift and reports unfinished work. Solo pause freezes the solo clock; multiplayer pause is local and the town keeps running.

In solo, absent professions use explicitly labelled municipal services. They act on the same seven inventory IDs and cannot steal your held parcel. No hidden human teammates, cloned inventory, scripted guilt indicators, daily ejections or criminal win condition were added by this rescue.

## Playtest protocol

For a first-time player, give only the launch command and let the illustrated introduction teach the job. Record time to first correct interaction, wrong target attempts, missed markers, rejected-action recovery, and whether the shift can be completed without spoken instructions. Ask what each receipt proves and what it leaves uncertain. Then repeat with the other jobs and a real group.

Technical evidence and the before/after gallery are in [Rescue review](../Diagnostics/Phase8ARescueV2/review.html). Automated runs arrange safe starting positions and send real per-process keyboard/mouse input or authenticated production commands. They demonstrate functional paths, not human comprehension. No human usability or cross-machine LAN playtest has been performed.
