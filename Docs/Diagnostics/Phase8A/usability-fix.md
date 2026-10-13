# Phase 8A collision and onboarding correction

The initial workday build disabled solid colliders when retiring legacy interactables. This removed collision from shelves, counters, sorting bins and service fixtures. Existing compatibility tests exercised the legacy mode; workday fixture tests mostly teleported players and missed this failure. User play exposed the problem.

The living workday now retires only legacy interaction triggers and excludes old jobs from targeting. Solid authored furniture retains its colliders. Sorting-chute and manifest interaction points sit outside the solid fixtures. Paper work labels have no solid collider, so they do not create floating obstacles in aisles. Physical items retain their fitted colliders when not carried.

Choosing a profession opens a two-page introduction: controls, the work chain, first destination and multiplayer dependencies. Enter advances/starts; H reopens; Escape closes. Gameplay input stops locally while reading. One next destination replaces the cluster of work markers. A persistent NEXT card explains the action, including parcel delivery, two-reading intake checks, terminal turning, batch inspection and manifest filing. Shop progress counts delivery rather than preparation alone.

The launcher defaults to one host window. Four local windows remain an explicit technical test option; a full shift still requires the other professions. There is no criminal role in this workday.

## Launch the corrected build

Close the old game window first. It remains on its original executable and does not update in place.

```powershell
powershell -ExecutionPolicy Bypass -File Scripts/launch_phase8a.ps1 -Mode host -Resolution 720
```

The launcher uses `Builds/Phase8A-Usability/BusinessAsUsual.exe`. Choose a job, press Enter through the introduction, then follow the NEXT card. The [playtest procedure](../../Design/phase8A-playtest.md) explains joining colleagues and the work chain.

## Verification

- **21/21 collision/onboarding checks** in two actual Windows processes at 1280 ? 720: real W-key InputActions against shelves, counters, walls, bins, vehicles and desks; tutorial input blocking/reopening; one next objective; local E targeting; carrying instructions; accessible receiving tray. Teleports arrange clear fixture starts; sustained walking uses production input and movement. [Checks and positions](FinalUsability/checks.json), [build identity](FinalUsability/run-metadata.json).
- **24/24 four-process workday checks**: shipment, delivery, real mouse terminal turns, repair, all three waste batches and manifest filing, targeted records, duplicate rejection, disconnect/rejoin recovery, walking entry and shared shift closure. [Checks](FinalUsabilityFour/checks.json), [completed authoritative snapshot](FinalUsabilityFour/completed-host.json).
- **10,385 domain checks** passed again; the domain was unchanged.
- Windows build succeeded with zero errors and one existing `Unity.Pipeline` warning about its absent RuntimePipelineConfig. [Build report](build.json). The final two native suites and current DLL have matching SHA-256: `2c65c5f1f25502b2fdc559d3d2404c4d2971aa1c95b1453d1a8444c7a315639b`.
- Editor left on DaylightTown, not playing, not compiling, scene clean. The build's temporary preloaded-assets change was restored to the pre-fix baseline. The original build and earlier evidence were retained. Existing staged changes were not restaged or committed.

Intermediate test directories retain failures that exposed the desk point and paper-label obstacles. Only the two final suites above validate this build. These checks establish specific functional behavior; they do not establish human enjoyment or cross-machine LAN performance.

## Current in-game screens

![First-workday controls](FinalUsability/host-intro-controls.png)

![Shopkeeper introduction and first destination](FinalUsability/host-intro-shopkeeper.png)

![Carried parcel with delivery instructions](FinalUsability/host-parcel-next-step.png)
