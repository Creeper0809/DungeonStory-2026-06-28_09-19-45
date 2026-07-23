# Progress

## 2026-07-23

- Started V14 implementation.
- Loaded the game-ai and Unity C# scripting guidance.
- Reviewed the existing persistent project plan and confirmed V13 completion evidence.
- Created an isolated V14 plan to avoid overwriting the long-running legacy task record.
- Confirmed Unity MCP connectivity and a pre-change Console state with no C# compile errors.
- Added `CharacterLifecycleState.Downed` and non-blocking `GridLayer.DownedCharacter`.
- Added physical-capacity querying and changed move/work injury calculation to use the lower of global injury and relevant body capacity rather than multiplying both.
- Added medical orders, downed cancellation, field stabilization, medicine/infection handling, physical rescue carry, treatment-bed reservation, repeated treatment, recovery, and runtime save snapshots.
- Added `AIRescue`/`AbilityRescue` and wired rescue into owner/staff action sets.
- Connected downing to defense disengagement and survival medicine/infection services.
- Fixed three definite-assignment errors found by Unity and verified `CharacterMedicalRuntime` compiles through MCP.
- Added shared affiliation, firing-solution, ordered shot-trace, forced-fire interception, high-cover corner peek, and tactical overlay services.
- Added combat stance ownership, pointer-driven single/group commands, combat-position reservations, reload, weapon switch, fire mode, hold fire, cover movement, and rescue commands.
- Added ammunition retry and physical resupply, backup weapon fallback, defense target distribution, ranged firing-position scoring, hunting reuse, and offense reload/switch/prediction controls.
- Added editable equipment-maintenance policies, per-character assignment, real equipment repair orders, material delivery, work progress, durability restore, and output hauling.
- Generalized combat presentation for melee/ranged/throw/reload/downed/rescue/treatment feedback using game-time-driven pooled VFX.
- Added V14 save data and restore ordering for health, medical orders, carried patients, tactical commands/reservations, equipment maintenance policies, and repair orders.
- Added and passed pointer-driven Combat V14 PlayMode verification for multi-selection, combat stance, reload, fire mode, hold fire, downing, exact-target rescue, stabilization, physical carry, treatment, and recovery.
- Added three verified captures: `combat-v14-command-bar.png`, `combat-v14-rescue-carry.png`, and `combat-v14-treatment.png`.
- Fixed the Unified UI regression driver and passed all start-party, character, mood, records, staff, and building preview checks with zero captured errors or warnings.
- Fixed modular building persistence so separate runtime markers and construction sites do not corrupt full save restore.
- Fixed wildlife replacement so saved animals synchronously release Grid occupancy before restore.
- Passed the full V14 game save round trip with zero restore warnings.
- Passed Combat, Wildlife, AI naturalness, runtime composition, Defense, Offense, Grid, CharacterClick, UnifiedUi, and 100-character scheduler regressions.
- Final focused Combat V14 report: `RESULT=PASS`, `failures=0`, Console `Error 0 / Warning 0`.
