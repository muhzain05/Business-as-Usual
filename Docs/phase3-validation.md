# Phase 3 validation report

Date: 2026-10-10 (America/Denver). Project: `Unity/BusinessAsUsual`, Unity **6000.3.26f1**, URP **17.3.0**, Input System **1.20.0**.

## Live Editor and Play-mode results

**19 / 19 checks passed** in the running Unity Editor. [Machine-readable results](Diagnostics/Phase3/playmode-validation.json) record the test method, timestamp, booleans and measured outcomes.

| Check | Result |
|---|---|
| Scene load / Sunny spawn | Prototype scene loaded; active CharacterController present |
| Prefab references | Camera, Animator, carry anchor and valid Generic avatar configured |
| Skin/material import | 19 modules, 53 bone references each; all assigned URP Lit materials |
| Existing walk/run | All curve paths resolve; moving foot transforms; two cuff corrective curves per clip |
| Input bindings | Virtual keyboard W/Shift/Space/E values read through production InputActions |
| Gravity / ground | Settled at Y ≈ 0.02 m, grounded |
| Walking / sprinting | 2.067 / 3.683 m in one simulated second, including acceleration |
| Camera-relative movement / rotation | Camera yaw 90° moves +X; visual faces the movement direction |
| Jump / landing | Approximately 0.656 m rise, then grounded |
| Wall collision | Shop west wall blocks movement at X ≈ −11.92 |
| Interaction detection | Nearby box selected, out-of-range prompt absent, temporary wall occludes target |
| Box | Picks up, attaches to anchor, drops to clear ground and restores colliders/dynamic body |
| Door | Detected, opens approximately 100°, closes to original rotation |
| Workstation | Detection, use count increment and HUD message succeed |
| Camera obstruction | Boom shortens from 3.8 m to approximately 1.514 m at shop wall |
| Mouse orbit / pitch limits | Synthetic delta changes yaw and clamps pitch |

These checks execute in Play mode using a virtual Input System keyboard and deterministic calls to the **production** movement/camera/interaction methods. They establish integration and state behavior. They do **not** establish a human keyboard/mouse play session, Game-view focus behavior, subjective camera feel, frame-rate performance or complete animation quality. Human input was not tested. Screenshots rendered from cameras omit the IMGUI HUD.

The first camera test used a position whose camera boom did not reach the wall; it was corrected to test a wall within the boom distance. A temporary validation-only identifier conflict was corrected before the final successful compilation. Neither issue remains in the delivered runtime.

## Screenshots

- [Third-person player camera](Diagnostics/Phase3/player-camera.png)
- [Town overview](Diagnostics/Phase3/town-overview.png)

Both were captured from the live Unity Play-mode scene. Visual review confirms Sunny's orientation, recognizable appearance and proportions, material assignment, compact town layout and collision geometry. It does not certify the known knee/cuff defects as resolved.

## Build, Console and preservation

The Windows x64 development build **succeeded with 0 errors**, approximately 317 MB of output, in 276 seconds. [Build summary](Diagnostics/Phase3/windows-build.json). Launch `Unity/BusinessAsUsual/Builds/Phase3/BusinessAsUsual.exe` for the local standalone version; keep its sibling data/runtime folders alongside it. Close it with Alt+F4. Build outputs are ignored; there is no distribution or upload.

The build reports **486 warnings**, grouped into [41 message categories](Diagnostics/Phase3/build-warning-categories.txt). One is the pre-existing `com.unity.pipeline` package's missing RuntimePipelineConfig warning; that package's runtime feature is disabled. It is separate from URP, which rendered the prototype screenshots successfully. The other 485 warnings concern the pre-existing AI inference/Sentis shaders: unsupported convolution variants, signed/unsigned conversions and integer operation performance. No runtime prototype code uses inference or the Pipeline package. These packages were retained; warning-free builds are not claimed. Unity MCP marked the build call as an error because it saw warnings, but the saved Unity BuildReport confirms `Succeeded` and zero errors; the build was not rerun.

Final Console inspection returned **0 errors / 26 warning entries** (shader warnings can be grouped differently from the build report). Before building, the final Play-mode Console inspection returned no errors or warnings. The warnings were not manually cleared.

A hidden, headless standalone startup check initialized Unity, Input System and PhysX without a logged exception. [Startup log](Diagnostics/Phase3/player-smoke.log), [scope and termination record](Diagnostics/Phase3/player-startup-smoke.json). The player's `-quit` flag did not close the process; the agent stopped only the process it had launched after inspection. This is a startup check, not normal shutdown, graphical rendering or standalone input validation.

Preservation checks verified **all 9 original source assets, all 5 Blender projects, and all 3 new FBX exports** against recorded SHA-256 values. [Artifact verification](Diagnostics/Phase3/artifact-verification.json) also verifies prototype deliverables, Unity `.meta` files, documentation links, input/package configuration and cache/build ignores. Git still has zero commits, staged files and remotes. The Editor is left stopped, with the saved, clean `TownPrototype` scene open at build index 0.

## Manual acceptance checklist

Open the prototype, press Play and click the Game view. Check WASD in several camera directions, sprint transitions, jump/landing and wall behavior; orbit near buildings; approach the delivery box, carry it around a corner and drop it; open/close the shop door; use both workstations; inspect prompts and Esc/click capture behavior. Sunny's existing short-strided animations, knee folds and shorts clipping remain prototype limitations.

## Official API references

The implementation follows Unity's [InputAction API](https://docs.unity.cn/Packages/com.unity.inputsystem@1.10/api/UnityEngine.InputSystem.InputAction.html) for enabled actions and button reads, and [rig import reference](https://docs.unity.com/en-us/engine/6000.0/manual/assets-and-media/asset-types/models/importing/fbximporter-rig) for Generic direct-skeleton animation. The exact local package and Editor were used for compilation and live validation; documentation alone is not treated as validation.
