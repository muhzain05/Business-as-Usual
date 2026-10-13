# Business as Usual

## Phase 8B: the first suspicious day

**Ordinary jobs, extraordinary suspicion.** Physical shopkeeper, mechanic and sanitation work intersects with partial records, lawful detours, private personal notes and one covert arrangement. There are no daily ejections or automatic guilt indicators.

Start Solo opens one game window. Labelled town services cover unstaffed jobs. Host Game and Join Game let each person use one window on their own computer. Automated solo and localhost multiplayer checks passed; a four-person human playtest and cross-machine LAN test remain outstanding.

See the [play guide](Docs/Design/phase8b-playtest.md), [implementation report](Docs/Design/phase8b-implementation-report.md), [validation report](Docs/Design/phase8b-validation-report.md), [remaining issues](Docs/Design/phase8b-remaining-issues.md) and [screenshot gallery](Docs/Diagnostics/Phase8B/review.html).

### Clone, build and play

Install Git LFS before cloning. This private repository requires GitHub access.

```powershell
git lfs install
git clone https://github.com/muhzain05/Business-as-Usual.git
Set-Location Business-as-Usual
git lfs pull
```

Open `Unity/BusinessAsUsual` in Unity **6000.3.26f1**. The front end is `Assets/Phase8B/Scenes/Phase8BFrontEnd.unity`. Keep Unity `.meta` files beside their assets. Build from the repository root, replacing the editor path with your installation:

```powershell
& 'D:/Unity/Editors/6000.3.26f1/Editor/Unity.exe' -batchmode -quit -projectPath "$PWD/Unity/BusinessAsUsual" -executeMethod BusinessAsUsual.Phase8B.Editor.Phase8BBuilder.Build -logFile "$PWD/phase8b-build.log"
powershell -ExecutionPolicy Bypass -File Scripts/launch_phase8b.ps1 -Resolution 720
```

Standalone domain tests require the .NET 10 SDK:

```powershell
dotnet run --project Tests/Phase8B/Phase8B.csproj
```

The following sections preserve earlier character and gameplay checkpoints. Their descriptions may be superseded by Phase 8B; use the current reports linked above for the latest behavior.

A stylized 3D multiplayer social-deduction game for **4–6 players**. Players work ordinary jobs in a small town while one secretly covers up a crime. Charming characters, expressive faces, exaggerated animation, and physical comedy guide the art.

## Historical Phase 4 checkpoint

**Phase 4: playable Shopkeeper shift.** Open `Unity/BusinessAsUsual` in Unity **6000.3.26f1**, open `Assets/Shopkeeper/Scenes/ShopkeeperShift.unity`, press Play, and click the Game view. Receive the rear-right delivery, carry its crate, restock all three labeled shelves, collect and check out three customer orders, then close the shift at the front-right terminal. **E** interacts, **Q** returns the stock crate, and **Tab** shows receipts. See [the full play guide](Docs/phase4-shopkeeper.md) and [screenshots/reference comparison](Docs/phase4-reference-comparison.md).

**Phase 3 remains available.** Open `Assets/Prototype/Scenes/TownPrototype.unity`. WASD moves Sunny; mouse looks; Shift sprints; Space jumps; E interacts/drops a box; Esc releases the mouse. See [the launch and implementation guide](Docs/phase3-first-playable.md) and [validation report](Docs/phase3-validation.md).

The prototype uses the existing **v004 locomotion and preserved v003 geometry**, 53-bone skeleton and 1.2-unit height. Shorts contact and knee folds remain; the final character quality gate has not passed. These limitations are accepted for this gameplay prototype, not production approval. No multiplayer, full jobs, criminal objectives or ragdolls are implemented.

Start with [the character audit](Docs/character-audit.md), then [the pipeline](Docs/character-pipeline.md), [art direction](Docs/art-direction.md), and [game design](Docs/game-design.md).

## Repository layout

```text
Assets/
  Characters/Sunny/
    Source/
      Original/       untouched original .blend
      Rigged/         untouched character FBX
      Animations/     untouched walk/run FBXs with skin
      Textures/       supplied PNGs
      Archives/       original download ZIP
    Working/          Blender audit and future working projects
    Processed/        future validated modular geometry
    Rigged/           future approved shared-skeleton characters
  Animations/         future approved reusable clips
  Textures/           future processed/shared textures
  Materials/          future reusable material assets
Scripts/
  Blender/            scripts executed through Blender MCP
Docs/
  Diagnostics/        inventory, machine-readable findings, and screenshots
```

`Assets/` is the repository art directory. The existing engine project is `Unity/BusinessAsUsual`; all imported/generated Unity prototype assets and their `.meta` files live under its `Assets/Prototype/`. Unity caches and local builds are ignored.

## Review the character

Open `Assets/Characters/Sunny/Working/Sunny_Phase1_Audit.blend` in Blender 5.2.2 LTS. Select `BAU_Audit_Original`, `BAU_Audit_Rig`, `BAU_Audit_Walk`, or `BAU_Audit_Run` from the scene selector. Animation scenes retain the imported auxiliary Icosphere; temporarily hide it to view the character unobstructed. The audit project contains imported copies and packed textures. The pre-existing default scene is also retained.

For Phase 2, open `Assets/Characters/Sunny/Working/Sunny_Phase2_Extraction_v001.blend`. `P2_v001_Modules_Rest` contains 15 assembled modules and an unchanged copied reference rig; `P2_v001_Modules_Overview` displays labeled parts. Existing defective weights remain as the comparison baseline. The Phase 1 file and original exports are preserved.

For the repair prototype, open `Assets/Characters/Sunny/Working/Sunny_Phase2_Repair_v002.blend`. Use `P2_v002_Repair`, `P2_v002_Walk_Review` or `P2_v002_Run_Review`. These are reviewable experiments with documented failures, not approved production assets.

For the targeted leg revision, open `Assets/Characters/Sunny/Working/Sunny_LegRepair_v003.blend`. Use `P2_v003_LegRepair`, `P2_v003_Walk_Review` or `P2_v003_Run_Review`. The [v003 report](Docs/leg-deformation-repair.md) describes reproduction, validation and the remaining lower-body blocker. Run `python Scripts/verify_leg_repair_v003.py` to verify protected hashes and deliverable integrity.

For restored motion, open `Assets/Characters/Sunny/Working/Sunny_Locomotion_v004.blend`. Use `P2_v004_Walk_Review` or `P2_v004_Run_Review` at 24 fps. Review [the v004 animations and remaining defects](Docs/locomotion-v004.md); run `python Scripts/verify_locomotion_v004.py` for preservation and artifact checks.

Original files were reorganized without changing bytes. [The inventory](Docs/Diagnostics/asset-inventory.json) records their previous paths, current paths, sizes, and SHA-256 digests. Do not save repairs over `Source/`.

## Git and collaboration

The project source, Unity assets and settings, original and working art, tests, research and diagnostic evidence are versioned on `main`. Generated caches, compiled test outputs, local player builds, editor state, credentials and rolling Blender backups are excluded.

Git LFS is configured for Blender, FBX, GLB, PNG, JPG, ZIP and GIF files in `.gitattributes`. Install it and run `git lfs pull` after cloning to fetch the actual binary content. Historical audit reports record the state at their checkpoint, including earlier uncommitted states.

## Scripts and verification

Run `python Scripts/verify_phase1.py` from the project root to verify source hashes, expected documents, diagnostic JSON, screenshots, and the saved Blender project. It performs no Blender modifications.

Run `python Scripts/verify_phase2_checkpoint.py --version v001` to check the versioned extraction, protected source/Phase 1 hashes, all-frame equivalence reports, images, documentation links, and LFS attributes.

Blender scripts are intended for `mcp__blender__execute_blender_code`, using `exec(compile(open(path, encoding='utf-8').read(), path, 'exec'))`. Import into a fresh session or one without existing `BAU_Audit_` scenes; the import script refuses duplicates. The diagnostic scripts restore frames and pose settings. See the pipeline for details and limitations. The one-time repository organizer must not be used as a routine build command.

## Phase boundary

Phase 3 gameplay integration is authorized using imperfect v004 prototype animations. Character production approval remains open. Historical checkpoint and audit reports are retained. Git commit approval is separate. Asset usage/license terms and provenance beyond the supplied Meshy exports remain to be recorded by the owner before wider distribution.
