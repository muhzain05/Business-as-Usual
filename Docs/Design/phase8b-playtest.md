# Phase 8B — play the first suspicious day

Open `Builds/Phase8B/BusinessAsUsual.exe`, or run:

```powershell
powershell -ExecutionPolicy Bypass -File Scripts/launch_phase8b.ps1 -Resolution 720
```

This opens **one main-menu window**. Choose **Start Solo** to learn a job. Pick a profession, read the two illustrated cards, and enter the shift. Unstaffed jobs use labelled town services. Solo's extra errand is labelled practice; there are no simulated human accusations.

Use WASD and the mouse. Aim at the highlighted physical object and press E. R performs the labelled alternate action. The mechanic's clamp uses E, then held left mouse and a sideways turn. Q returns a load or parks the cart. Tab opens your work slip; its private tab holds your personal note. Esc opens settings/pause. Named destination cards distinguish a workplace from a decorative home.

Complete customers, a tested repair, and three collection tickets. The staff kiosk's noticeboard opens voluntarily shared copies with E during work; its counter files the final town report when the work is complete. The report returns to the main menu. A shift also closes at its time limit with unfinished work shown honestly.

## Four-player test, without four windows per person

Use one matching build on each person's computer. One person chooses **Host Game**; three choose **Join Game** and enter the host's LAN IPv4 address. Use the same UDP port (default 7777). Choose Shopkeeper, Mechanic, Sanitation and a helping profession. Read the brief and ready up. The host opens the day when all connected workers are ready.

Every worker gets a private personal note. One receives an additional arrangement independent of their job. Ordinary returns and yard holds work for everyone: carry a load there, E to restage/collect, R to change the sealed parcel's declared slip. A sealed parcel can also be lifted from garage intake for a recheck before unpacking. These actions do not certify intent. A service-note correction retains the earlier entry. Records → Pin Copy discloses only the selected professional paper, with attribution.

Run a first 8-minute day without coaching. Ask each worker privately what they think their next action is at arrival, after the first handoff, and halfway through. Record wrong destinations, blocked actions, time spent without useful work, and whether the correction/hold felt natural. Do not explain a suspicious movement while it happens.

After the report, let players compare accounts and selected copies. Ask: what did you observe, what did a paper claim, and what remains uncertain? Look for at least two plausible explanations for the same detour. Ask which interaction they would repeat and whether they want a second day. Rotate jobs and the arrangement on a second run. Stop adding deduction systems until this loop is understandable and enjoyable.

## Validation distinction

The automated localhost test uses two or four separate Windows processes to check synchronization, ownership, private payloads and reconnection. It uses fixture teleports and production RPCs. Solo interaction runs additionally use native E/R, mouse torque and real movement. These are technical tests; no four-person human playtest or cross-machine LAN result is claimed.

See the implementation/validation reports and the [native before/after gallery](../Diagnostics/Phase8B/review.html). Homes, leisure and the economy are in [the future roadmap](phase8b-future-roadmap.md).
