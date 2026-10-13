# Game design — initial scope

## Established concept

Business as Usual is a stylized 3D multiplayer social-deduction game for 4–6 players. The setting is a small town. Players perform ordinary jobs while one player secretly covers up a crime.

The desired tone combines charming characters, expressive faces, exaggerated animations, and physical comedy. Character readability should support social interaction and clear physical acting.

## Boundaries and unresolved decisions

Unity 6000.3.26f1 with URP is initialized at `Unity/BusinessAsUsual`. Phase 3 implements a local third-person exploration prototype with box, door and workstation interactions. Job selection, objectives, evidence rules, accusation rules, victory conditions, round structure, networking architecture, and crime details remain unspecified. This prototype does not establish those systems.

Current production work concerns repository setup and a reusable character pipeline. The pipeline should accommodate interchangeable clothing, expressive faces, shared humanoid animation, and later ragdolls. No gameplay, multiplayer, or ragdoll behavior is implemented in Phase 1.

Phase 3 adds [the first playable vertical slice](phase3-first-playable.md). Multiplayer, full job/task systems, criminal objectives and ragdoll physics are explicitly deferred. The workstation only emits feedback and an event; it is not a job progression system.

## Next design input

After character audit review, establish intended camera distance, silhouette/readability expectations, and the first ordinary-job animation needs. These inputs should guide character validation without prematurely defining major game systems.
