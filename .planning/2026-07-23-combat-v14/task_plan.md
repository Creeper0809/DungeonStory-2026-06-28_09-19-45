# Combat Life Loop and Tactical Control V14

## Goal

Implement and verify the approved V14 combat completion plan on top of the existing V13 shared combat, defense, offense, wildlife, item, work-order, and save systems.

## Phases

| Phase | Scope | Status |
|---|---|---|
| 1 | Audit current V13 contracts, compile state, dependency wiring, and verification harnesses | Complete |
| 2 | Implement downed lifecycle, physical capacities, field stabilization, rescue carry, bed treatment, and AI exclusion | Complete |
| 3 | Implement combat affiliation, ordered shot traces, forced-fire interception, corner-peek cover, and shared firing solutions | Complete |
| 4 | Implement combat stance, direct/group commands, tactical reservations, and pooled tactical overlays | Complete |
| 5 | Implement ammunition retry/resupply, fallback weapon logic, defense coordination, hunt reuse, and offense controls | Complete |
| 6 | Implement editable equipment-maintenance policies, physical repair delivery, work progress, output, and manual repair | Complete |
| 7 | Generalize combat presentation and add pause-safe weapon, hit, cover, suppression, downed, rescue, and treatment feedback | Complete |
| 8 | Add V14 persistence, health/combat/defense/operation UI, and invalid-reference recovery diagnostics | Complete |
| 9 | Add and run focused EditMode and pointer-driven PlayMode verification, performance checks, captures, and full regressions | Complete |

## Product Decisions

- Direct tactical commands are available only while characters are in combat stance.
- Forced fire can intercept and injure allies, protected neutral visitors, and non-designated wildlife.
- Rescue uses field stabilization followed by physical carry to a reserved treatment bed.
- Equipment repair uses editable named policies plus manual repair commands.
- Existing art, item sprites, and audio are reused with pooled code-driven presentation.
- V14 saves intentionally reject V13 and older saves.

## Verification Gate

1. Runtime and Editor assemblies compile with Console Error 0 / Warning 0.
2. Focused EditMode scenarios cover downing, rescue, treatment, affiliation, firing, cover, resupply, coordination, maintenance, and V14 restore.
3. Pointer-driven PlayMode verification covers combat stance and every direct command without UI click leakage.
4. A real defense run reaches downed, stabilization, carry, treatment, return, resupply, and repair without state injection as completion evidence.
5. Offense exposes and executes reload, switch weapon, fire mode, prediction, injury, and downed return rescue.
6. Tactical overlays and combat feedback remain readable at 1600x900 and 900x1600 and pause with game time.
7. Existing Combat, Defense, Offense, Wildlife, AI, Grid, CharacterClick, UnifiedUi, and Save regressions remain green.

## Errors Encountered

| Error | Attempt | Resolution |
|---|---|---|
| New rescue flow had three definite-assignment compiler errors | First Unity refresh after adding medical runtime | Initialized short-circuit `out` locals before condition evaluation; runtime assembly compiles again |
| Direct rescue inherited the paused AI action cancellation token | First pointer-driven rescue verification | Direct combat rescue now owns an independent coroutine token while automatic rescue still enforces the selected AI action |
| Unified UI verifier waited for the preparation-scene button while the gameplay QA fallback used a different button | Unified UI regression | The verifier now supports both `PreparationStartRunButton` and `StartPartyConfirm` and reports the real readiness state |
| Delayed stale-input cleanup removed a newly-created verification mouse | Unified UI rerun after compilation | Verification-device cleanup now runs only outside PlayMode |
| Exterior runtime markers were captured as catalog buildings with negative IDs | First full V14 save round trip | Modular building persistence now captures and clears only real persistent buildings; runtime markers and construction sites stay with their owning save systems |
| Wildlife restore rejected every saved position because old actors still occupied their cells until end-of-frame | Second full V14 save round trip | Wildlife actors synchronously detach from Grid and AI registry before deferred GameObject destruction |
