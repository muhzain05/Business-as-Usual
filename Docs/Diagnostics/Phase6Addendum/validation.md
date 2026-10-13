# Phase 6 addendum validation

**363/363 automated checks passed.** Both Windows builds succeeded with zero errors.

| Suite | Passed | Report |
|---|---:|---|
| Phase3 | 19/19 | [playmode-validation.json](Regression/Phase3/playmode-validation.json) |
| Phase4 | 87/87 | [playmode-validation.json](Regression/Phase4/playmode-validation.json) |
| Phase4.1 | 57/57 | [polish-validation.json](Regression/Phase4.1/polish-validation.json) |
| Phase4.2 | 65/65 | [ui-validation.json](Regression/Phase4.2/ui-validation.json) |
| Phase5 | 32/32 | [validation.json](Phase5/validation.json) |
| Town full 1080p | 35/35 | [validation.json](TownFull1080/validation.json) |
| Staffed cooperation & navigation | 20/20 | [validation.json](Staffed1080/validation.json) |
| Real market entry 720p | 4/4 | [validation.json](MarketEntry720/validation.json) |
| Personal guidance & controls 1080p | 22/22 | [validation.json](Guidance1080/validation.json) |
| Personal guidance & controls 720p | 22/22 | [validation.json](Guidance720/validation.json) |

Standalone suites use actual NGO/UTP host/client processes on localhost with production E dispatch, held input, real keyboard InputAction bindings for Tab/J, and opt-in deterministic placement. Editor regressions use live Play mode and synthetic input. No human or two-PC LAN test is claimed.

The original animation capture initially stopped advancing while the editor was unfocused; its first resumed run missed synthetic Q input (56/57). The full repeat with runtime background frames and IgnoreFocus passed 57/57. The failed attempt is retained in FailedAttempts/polish-background-input.json. An initial guidance launch also found an existing host on port 7777; final automated suites use isolated 7785/7786.

The build bridge reports existing RuntimePipelineConfig and bundled Sentis shader warnings as a tool error even when BuildPipeline succeeds. The saved Phase5-build.json / Phase6-build.json report Succeeded and errors=0; both report 486 warnings. This pass did not alter the unrelated AI/inference packages. Fresh standalone logs contain none of the checked runtime exception, unsupported-shader or port-bind failures.

Preservation passed against both the original Phase6 baseline and the accepted Phase6 addendum baseline: exact Git index unchanged; no protected asset bytes changed. Build/import side effects in Shop_Paper.mat, Shop_Screen.mat and ProjectSettings.asset were restored. The original shop, Sunny, earlier scenes, packages and previous evidence remain preserved. The saved town scene and six authorized existing C# sources changed; new kit/guidance assets are additive.

All 95 existing collider transforms and enabled states were retained. Seven environment pairs use identical capture cameras. Matched job views and all guidance captures were inspected at 1080p; new guidance was also inspected at 720p. The review gallery verifies all 28 linked images.

## Performance scope

The navigation suite recorded 30 reported frame-time samples per process: host median 7.6 ms / p95 9.6 ms, client median 6.4 ms / p95 8.6 ms. These are short development-build samples with Unity and validation activity on the same machine, not profiler draw-call measurements or a shipping benchmark. Raw samples are in Staffed1080/frame-samples.json.

## Remaining design and review work

The slice remains two players and three public jobs. Hidden criminal assignment, private secret objectives, concealment/evidence systems, meetings/warrants, proximity voice and the multi-day loop remain future work. Future secret state needs a distinct targeted protocol; current public work snapshots remain shared. Clinic, warehouse and apartments are not functional yet. Faults and customer queues remain scripted. Neighborhood houses are decorative and currently have no LOD tiers. The town diagram projects shop-interior objectives onto the market rather than providing an interior floor plan. Human pacing/observation and two-PC LAN testing remain open.

**Meshy credits spent: 0.** The kit is original Blender geometry; the exposed schema lacks T2.5. No paid generation or batch was started.

[Before/after review](review.html) ? [Implementation and GDD alignment](../../phase6-addendum.md)
