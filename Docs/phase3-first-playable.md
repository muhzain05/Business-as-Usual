# Phase 3 — first playable prototype

Date: 2026-10-10 (America/Denver). This phase authorizes a local gameplay prototype using Sunny v004 despite the documented remaining character quality issues. It does not approve the character for production.

## Launch and controls

1. Open the existing Unity project `Unity/BusinessAsUsual` in **Unity 6000.3.26f1**.
2. Open `Assets/Prototype/Scenes/TownPrototype.unity` from the Project window.
3. Press Play, then click the Game view to capture the mouse.
4. Use WASD to move relative to the camera, mouse to orbit, Shift to sprint, Space to jump, and E to interact. Esc releases the mouse; click resumes capture. Stop Play to reset the scene.

The delivery box is ahead and left of the spawn. Approach it until the E prompt appears; press E to carry, then E again to place it on clear ground. The shop is on the left: approach its door and press E to open it; approach the open door's handle to close it. The garage is on the right: walk through its open entrance and use the rear workbench. The sanitation sorting table is across the street. Walk around either end of the buildings to reach the back alley.

A local Windows build is also available at `Unity/BusinessAsUsual/Builds/Phase3/BusinessAsUsual.exe`. Keep its sibling runtime/data folders in place. Controls are the same; Alt+F4 closes the standalone player. The build's existing-package warnings and the limited headless startup check are documented in the validation report.

## Deliverables and layout

All engine assets are inside `Unity/BusinessAsUsual/Assets/Prototype/`:

- `Scenes/TownPrototype.unity`: compact 32 × 28 metre graybox, town square, shop, garage, sanitation pad, back alley, benches, bins and street lamps.
- `Prefabs/SunnyPlayer.prefab`: CharacterController, self-contained orbit camera, input actions, interaction component, HUD and imported Sunny visual/Animator.
- `Scripts/`: runtime controller, camera, Input System actions, interaction base class and implementations.
- `Art/Sunny/`: v004 FBX model/locomotion exports and copied textures.
- `Materials/`: URP materials, including conversion of the copied packed metallic/roughness map.
- `Animation/SunnyLocomotion.controller`: Idle/Walk/Run speed blend tree.
- `Editor/`: guarded scene builder and live Play-mode validation utility.

The prototype is first in Editor Build Settings. The original SampleScene remains available and unchanged. No networking, job progression, evidence, criminal objectives, ragdolls or generated/downloaded art was added.

## Character integration

Source: `Assets/Characters/Sunny/Working/Sunny_Locomotion_v004.blend`, SHA-256 `694b527c44d88b09e09453058357e1f2edf997ff3b615e9013a2d08df990a861`. Export ran in a separate background Blender process. The live Blender session and prior files were not saved over.

The exact 53-bone skeleton and 1.2-unit neutral height are retained. **Generic** is intentional: direct playback on the supplied skeleton needs no humanoid retargeting, T-pose enforcement or uncertain finger mapping. The Generic avatar is valid. Nineteen visible modules are exported; four marked nonproduction limb-envelope references are omitted. Bind-pose subdivision and clothing thickness are evaluated into export copies; deform weights, UVs, facial blendshapes and sampled cuff-driver animation remain present. Walk spans frames 1–33 (1.333 seconds), run 1–25 (1 second), including their matching closing keys, at 24 fps. Root motion is disabled.

Original packed color/data/normal images were copied out. The repaired head uses its existing cleaned color/normal images. Existing flat palette materials on the repaired jacket/cuffs/legs are preserved; original garment texture fidelity was already incomplete in v004. URP receives metallic in R and smoothness in A from the existing data map's B metallic / G roughness channels. Imported materials are explicitly remapped to URP Lit.

Known prototype compromises: knee/shorts clipping and cuff folds persist; Unity uses linear skinning rather than Blender's preserve-volume skinning; bind-pose evaluation of thickness is an approximation of its animated Blender modifier order. Sunny's short-strided in-place loops visibly slide at the faster town traversal speeds. Idle holds the neutral pose, and jump uses the existing locomotion visuals without a dedicated jump clip. No character repair or redesign was performed.

## Runtime architecture and tuning

`PrototypeInput` owns enabled/disposed InputActions for keyboard/mouse; the template's global actions are retained. Cursor capture gates gameplay input. `ThirdPersonMotor` uses CharacterController.Move for gravity, grounding, collision and jump, and rotates the visual toward camera-relative movement. Its speeds, acceleration, turn rate, jump height, gravity and respawn boundary are Inspector fields. The camera exposes sensitivity, pitch limits, boom distance, target height and collision radius, with sphere-cast obstruction avoidance. Layer 2 excludes Sunny from the camera/interaction casts. Full imported influences are enabled at runtime.

`PlayerInteractor` uses a bounded nearby-collider query, distance selection and line-of-sight checks. `Interactable` provides the prompt, eligibility and interaction contract. `CarryableBox` attaches to the visual's carry anchor, disables its colliders while carried, and restores physics when dropped on clear ground. While carrying, E always drops before other interactions. `SimpleDoor` rotates a hinged panel and updates its prompt. `BasicWorkstation` emits a UnityEvent and local use count/HUD feedback; future systems can subscribe without changing detection/input. The HUD uses lightweight IMGUI; world labels use TextMesh.

## Reproduction and validation

The scene and prefab are already saved; do not rerun the builder on top of them. `Business as Usual > Phase 3 > Create First Playable (new scene only)` refuses an existing prototype or unsaved scene. To reproduce in a clean copy, use `Scripts/Blender/export_sunny_phase3.py` with background Blender on the unchanged v004 file, then `python Scripts/prepare_sunny_unity_maps.py`, then the guarded Unity builder. The exporter refuses existing FBXs.

To rerun automated checks, save and stop Play, then choose `Business as Usual > Phase 3 > Validate in Play Mode`. It opens the prototype, enters Play, performs temporary runtime checks, writes `Docs/Diagnostics/Phase3/playmode-validation.json` and camera screenshots, and stops Play without saving test state. It tests synthetic Input System reads and production motor/camera/interaction methods; it does **not** claim a human play session.

See [the validation report](phase3-validation.md) for measured outcomes and remaining manual checks. Export metadata and hashes are recorded in [sunny-export.json](Diagnostics/Phase3/sunny-export.json).

## Preservation and Git

The repository remains on its existing `main`, with no commits, staged files or remotes. Original source assets and all earlier Blender versions remain protected. `.gitignore` excludes Unity Library, Temp, Logs, Obj/obj, UserSettings and Builds; generated IDE solution/project files are also ignored. Source assets, Unity scenes/prefabs/materials/scripts and their `.meta` files remain eligible for version control. No commit or push was performed.
