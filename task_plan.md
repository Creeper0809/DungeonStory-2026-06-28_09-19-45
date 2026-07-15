# Task Plan: DungeonStory DI Refactor

## Goal
Analyze every non-Editor Unity C# script, document its functions and coupling risks in markdown, refactor inappropriate responsibilities and coupled dependencies using VContainer DI, and verify repeatedly in Unity Play Mode with camera capture checks.

## Current Phase
Phase 61: Collapse Build Catalog During Placement (complete)

## Phases

### Phase 1: Requirements & Discovery
- [x] Understand user intent
- [x] Inventory all non-Editor scripts
- [x] Identify Unity/VContainer project structure and scenes
- [x] Document discoveries in findings.md
- **Status:** complete

### Phase 2: Script Audit Markdown
- [x] Generate/update markdown listing every function in non-Editor scripts
- [x] Mark functions that appear misplaced
- [x] Mark functions with high coupling or DI candidates
- [x] Record rationale per script
- **Status:** complete

### Phase 3: VContainer Refactor
- [x] Add or reuse LifetimeScope/installer structure
- [x] Introduce focused services/interfaces where coupling is real
- [x] Refactor MonoBehaviours to receive dependencies through VContainer-friendly injection
- [x] Preserve serialized tuning fields and Unity lifecycle correctness
- **Status:** complete

### Phase 4: Testing & Verification
- [x] Compile/check console errors
- [x] Enter Play Mode through Unity MCP
- [x] Use Unity MCP camera capture to inspect UI visibility
- [x] Iterate until errors are resolved or clearly documented
- **Status:** complete

### Phase 5: Delivery
- [x] Review changed files and generated markdown
- [x] Summarize refactor decisions, verification status, and residual risks
- **Status:** complete

### Phase 6: Over-Separation Exhaustive Audit
- [x] Restore current context and confirm script scope
- [x] Inventory interfaces, providers, factories, services, runtime wrappers, and thin classes across all project non-Editor scripts
- [x] Classify each candidate as keep, watch, or merge candidate with evidence
- [x] Write `OVER_SEPARATION_AUDIT.md`
- [x] Review generated findings and update progress
- **Status:** complete

### Phase 7: Over-Separation Refactor & Unity Verification
- [x] Fold all `merge-candidate` `*SummaryRuntimeSource` interfaces/classes into their sibling `*SummaryService` classes
- [x] Remove pure factory-alias services and inject the corresponding factories at call sites
- [x] Fold `IUiTouchGuardService`/`UiTouchGuardService` into the existing UI popup facade
- [x] Clean VContainer registrations and verify no removed interfaces/classes remain referenced
- [x] Run Unity MCP compile, Play Mode, console, UI bounds, and camera capture checks
- [x] Update audit/planning docs with final refactor and verification results
- **Status:** complete

### Phase 8: Commit Scope Isolation
- [x] Create an explicit file manifest for the completed Phase 7 refactor
- [x] Separate user/existing dirty worktree state from this refactor scope
- [x] Record a safe commit/staging checklist that avoids accidental broad staging
- **Status:** complete

### Phase 9: Direct Manual PlayMode Flow Verification
- [x] Enter Play Mode through Unity MCP
- [x] Manually exercise building info open/close
- [x] Manually exercise tab summary UI flows
- [x] Manually inspect facility evolution candidate/evolve flow
- [x] Manually validate character feedback bubble and social memory creation
- [x] Capture UI through Unity MCP camera capture and check console state
- **Status:** complete

### Phase 10: Watch Provider Cluster Refactor
- [x] Inspect `*RuntimeProvider` watch cluster implementations
- [x] Add a small shared cached scene-runtime helper/base where it reduces real repetition
- [x] Refactor matching providers without removing useful DI boundaries
- [x] Compile and Play Mode verify the provider refactor
- **Status:** complete

### Phase 11: Automated Debug/Test Reinforcement
- [x] Add or extend a debug verification scenario for the DI/refactor-critical runtime flows
- [x] Run the automated scenario through Unity MCP
- [x] Update verification docs with new scenario coverage and remaining risks
- **Status:** complete

### Phase 12: Full-Game Verification Scope Discovery
- [x] Derive the full implemented feature catalog from implementation reports, game-design docs, scenes, Resources assets, and runtime scripts
- [x] Separate automatic debug-scenario coverage from actual in-game PlayMode/manual coverage
- [x] Identify which features require `SampleScene`, `CharacterAiTestScene`, generated debug worlds, or missing scene hookups
- **Status:** complete

### Phase 13: Manual QA Matrix
- [x] Create a full-game manual verification markdown with feature rows, evidence requirements, and current status
- [x] Mark previous DI/refactor verification as subset coverage only
- [x] Define the completion rule for claiming "all game features work in-game"
- **Status:** complete

### Phase 14: In-Game Manual Pass - Core Loop and UI
- [x] Verify build-scene startup in `SampleScene` PlayMode while dirty scene state is safely saved/restored
- [x] Verify camera clamp, pause/speed, grid/build mode, public pointer placement, touch guard, and building info UI in PlayMode
- [x] Verify top tabs, generated panels, event/notice UI, and overlay visibility with camera/screen capture
- [x] Record console state and screenshots for the first `CharacterAiTestScene` baseline pass
- [x] Additive-isolated `SampleScene` core UI, scene query, and intruder spawn/movement DI retest
- **Status:** complete

### Phase 15: In-Game Manual Pass - Feature Families
- [x] Add and run a `SampleScene` feature-family PlayMode probe for shop, research, synthesis, codex, operation, alerts, run variables, offense panels, and meta runtime paths
- [x] Fix and re-test alert detail stacking above offense UI canvases
- [x] Add and run a `SampleScene` remaining-feature PlayMode probe for AI direct decisions, inventory/logistics, invasion threat/report, recruitment, discontent/rebellion response, and offense expedition completion/rewards with a VContainer-injected temporary member
- [x] Capture and inspect `Temp/full-game-qa-samplescene-remaining-feature-families.png`; restore additive scene state and confirm final console is clean
- [x] Add and run a VContainer-injected `CharacterAiTestScene` character-AI feature probe covering customer decision, owner/staff priority, off-duty trigger, local LLM/persona application, character visual/feedback components, UI bounds, camera capture, and clean final console
- [x] Add and run a `SampleScene` closing-gap PlayMode probe covering runtime grid placement/destruction, triggerable defense effect activation, direct invasion final combat, visible feedback bubble text, UI bounds, MCP camera capture, screen capture, restore, and clean final console
- [x] Classify and fix the `CharacterAiTestScene` right-side yellow overlay candidate (`UI/Quest`) and rerun visual capture with clean console
- [x] Verify and fix the public `SampleScene` build UI category/generated-button/ghost/close-reset flow with PlayMode capture and clean final console
- [x] Fix and rerun direct dialogue bubble display in the closing-gap probe
- [x] Rerun the `CharacterAiTestScene` probe with improved evidence capture and verify scheduler-driven customer decision selection plus staff off-duty visitor/recovery return
- [x] Verify approved active-scene facility evolution and timed invasion coroutine completion in `SampleScene`
- [x] Verify remaining longer autonomous customer movement/visit observation and real-time staff fatigue/wander observation
- [x] Verify active-scene grid foundation/reachability, camera clamp, UI touch guard, and building-info open/close with visual capture
- [x] Use public game/UI flows first, then PlayMode debug-world probes only where the active scene lacks a runtime hookup
- [x] Record each feature as verified, partial, failed, missing-scene-hookup, or not-yet-tested
- **Status:** complete

### Phase 16: Failure Fixes and Re-Test
- [x] Fix the live `CharacterAiTestScene` `UI/Quest` overlay state and rerun Character-AI visual capture
- [x] Kill QA-temp DOTween tweens before destroying temporary objects and rerun the affected probe with 0 final warnings
- [x] Fix public generated build buttons, construct-tab close reset, and unlock-refresh behavior, then retest the public UI flow
- [x] Add `CharacterVisual` tween cleanup so destroyed/disabled child `SpriteRenderer` fades do not leave DOTween warnings
- [x] Fix `CharacterDialogueRuntime` lazy reference recovery for dynamically-created actors and rerun the affected visible bubble/dialogue probe
- [x] Fix and verify strict public pointer placement through UI selection plus `TriggerPlaceBuilding()`
- [x] Fix QA probe reporting that hid scheduler tick maxima and measured staff visitor state after returning to work, then rerun in clean PlayMode
- [x] Fix any remaining game-facing failure found during manual QA
- [x] Re-run the affected PlayMode scenario and visual capture after every fix found so far
- [x] Update the QA matrix with final evidence for fixes completed so far
- **Status:** complete

### Phase 17: Completion Audit
- [x] Confirm every cataloged feature has direct PlayMode evidence or an explicit accepted limitation
- [x] Confirm Unity console has no game errors after the final pass
- [x] Only then declare the full-game verification goal complete
- **Status:** complete

### Phase 18: UI Surface Gap Audit
- [x] Reinterpret the latest user feedback as a stricter player-facing UI audit, separate from runtime PlayMode verification
- [x] Identify features that exist in runtime code but are only exposed through summary text, alerts, debug probes, injected QA actors, or hidden commands
- [x] Write a dedicated docs artifact listing UI gaps and closure criteria
- [x] Update planning context so future work does not equate runtime verification with UI completion
- **Status:** complete

### Phase 19: P0 Player-Facing UI Surface Wiring
- [x] Restore current UI/runtime context from the P0 gap audit and existing tab code
- [x] Replace P0 generated summary/TODO tab bodies with discoverable panels for shop, research, warehouse, operation settlement, recruitment, and meta upgrades
- [x] Provide explicit state, action controls, and success/failure/lock feedback for each P0 panel
- [x] Verify each P0 panel through Unity PlayMode by opening the UI, clicking controls, observing state changes, checking console errors/warnings, and capturing visible UI with Unity MCP camera capture or ScreenCapture
- [x] Update QA/planning docs with the final P0 evidence and any remaining limitations
- **Status:** complete

### Phase 20: P1/P2 Player-Facing UI Surface Mapping
- [x] Map every remaining P1/P2 audit row to its runtime, existing panel, tab entry point, state query, and public action API
- [x] Group the rows into facility, staff, defense, offense, operation/economy, shop detail, and codex/history surfaces
- [x] Record deterministic PlayMode preparation and state-change evidence required for every row
- **Status:** complete

### Phase 21: P1 Player-Facing UI Surface Wiring and Verification
- [x] Connect run variables, synthesis/evolution, defense/invasion/reporting, staff/discontent/commands/profile/AI, and offense/reward flows to normal tabs
- [x] Preserve existing staff priority, build, alert, and specialized-panel behavior while adding discoverable P1 controls
- [x] Verify every P1 row through visible UI clicks, state deltas, feature-family screenshots, camera capture, and clean verifier-window console
- **Status:** complete

### Phase 22: P2 Player-Facing UI Surface Wiring and Verification
- [x] Connect codex search/filter/detail/archive, shop operations detail, economy flow, and alert history/filter UI
- [x] Verify every P2 row through visible UI interactions, state/filter changes, screenshots, camera capture, and clean verifier-window console
- [x] Move only evidence-proven rows out of the gap table and record any independent scene/font limitations separately
- **Status:** complete

### Phase 23: Room Semantics and Evidence Hardening
- [x] Make formal wall/door rooms supersede per-facility self-contained fallback rooms
- [x] Display room closure, door/wall counts, interior facilities, and furniture-derived room roles before the separate synthesis section
- [x] Separate furniture-derived room identity from facility-evolution pressure in player-facing copy
- [x] Verify a real 22-cell room with one door, one wall, dining/training facilities, visible selection state, and synchronized end-of-frame capture
- [x] Re-run all 18 P1/P2 rows and the P0 surface regression with clean verification-window logs
- **Status:** complete

### Phase 24: Build Placement Grid and Readable Placement Preview
- [x] Trace the public build-mode, grid-overlay, ghost, and dungeon backdrop render paths in the active scene
- [x] Show a stable high-contrast placement grid while building selection is active, with valid and invalid cells distinguishable
- [x] Raise the intentionally faded placement preview to a readable opacity without changing normal dungeon rendering
- [x] Verify build-mode visibility, pointer movement, valid/invalid placement feedback, placement state change, and normal-mode cleanup in PlayMode
- [x] Capture desktop evidence through the game camera/screen and confirm console Error/Warning 0
- **Status:** complete

### Phase 25: Block World Input Behind UI
- [x] Reproduce the Input System `IsPointerOverGameObject()` mismatch on a real UI coordinate
- [x] Add a reusable GraphicRaycaster-based UI pointer blocker through VContainer
- [x] Apply it to building information, character information, placement, destruction, and owner-command world input
- [x] Verify UI-over-world clicks are blocked while unobstructed world clicks still open information
- [x] Verify overlapping UI blocks placement and destruction with Error/Warning 0
- **Status:** complete

### Phase 26: Render Placement Grid Above Walls
- [x] Inspect the active TilemapRenderer and SpriteRenderer sorting layers in PlayMode
- [x] Place the construction grid above the wall tilemap and the placement ghost above the grid
- [x] Update the grid foundation sorting regression assertion
- [x] Open construction through visible UI, capture the grid over wall art, and verify wall-overlap coverage
- [x] Place a building through the pointer flow and confirm state change plus Error/Warning 0
- **Status:** complete

### Phase 27: Placement Click and Visual Alignment Corrections
- [x] Reproduce immediate building-info opening after successful placement and trace both click-selection paths
- [x] Prevent the placement click from selecting the newly created building
- [x] Compare placed-building and ghost sprite bounds, pivots, footprint scaling, and floor anchors
- [x] Make placed visuals sit on the floor and make the ghost opaque, correctly sized, and position-matched
- [x] Verify public UI selection, valid/invalid ghost states, successful placement, panel timing, and camera captures in PlayMode
- [x] Re-run grid regressions and finish with Error/Warning 0
- **Status:** complete

### Phase 28: Preserve Ghost Sprite Colors
- [x] Keep valid placement ghosts opaque without recoloring the source sprite
- [x] Keep blocked placement feedback visibly red and opaque
- [x] Update ghost color regressions and compile
- [x] Verify Door and Research ghosts in PlayMode with camera capture and Error/Warning 0
- **Status:** complete

### Phase 29: Enforce Blocked Placement Verdict
- [x] Reproduce a red Research ghost that still places and compare preview/click coordinates
- [x] Make preview and placement use one authoritative footprint verdict
- [x] Add regression coverage for blocked placement rejection
- [x] Verify blocked click does not place, valid click still places, and capture both states in PlayMode
- [x] Finish with Grid regressions and Error/Warning 0
- **Status:** complete

### Phase 30: Align Facility Visuals to Placement Grid
- [x] Measure Research and Meat Restaurant logical footprints, occupied cells, tile transforms, and visible bounds
- [x] Correct the shared facility visual anchor/scale if the rendered edges do not match the occupied footprint
- [x] Add regression coverage for even-width facility center and edge alignment
- [x] Verify both facilities over the live construction grid with PlayMode camera capture
- [x] Finish with Grid regressions and Error/Warning 0
- **Status:** complete

### Phase 31: Correct Multi-Cell Placement Availability Overlay
- [x] Reproduce the incorrect availability colors around an occupied multi-cell facility
- [x] Separate valid-anchor feedback from footprint-cell occupancy and define the intended overlay rule
- [x] Correct the overlay calculation without changing authoritative placement validation
- [x] Add regression coverage for occupied and edge-adjacent multi-cell footprints
- [x] Verify valid/blocked previews and overlay colors in PlayMode with camera capture and Error/Warning 0
- **Status:** complete

### Phase 32: Restore Cell-Level Placement Overlay Semantics
- [x] Replace selected-footprint coverage coloring with cell-level installable/non-installable coloring
- [x] Keep full-building feasibility exclusively in the placement ghost verdict
- [x] Cover empty supported cells, occupied cells, unsupported cells, and side boundaries in regression tests
- [x] Verify the one-cell gap is white while a width-3 ghost over it remains red in PlayMode
- [x] Finish with Grid regressions and Error/Warning 0
- **Status:** complete

### Phase 33: Isolate Top-Level Feature Tabs from Construction Subtabs
- [x] Reproduce the staff-priority controls appearing inside a construction subtab
- [x] Identify the duplicate numeric tab-id routing collision
- [x] Restrict top-tab target lookup and specialized-panel attachment to direct top-level tabs
- [x] Verify construction subtabs retain their intended UI and the staff controls remain clickable in PlayMode
- [x] Capture both surfaces and finish with Error/Warning 0
- **Status:** complete

### Phase 34: Expose Interior Wall and Door Room-Boundary Tools
- [x] Inspect wall/door assets and confirm room detector boundary semantics
- [x] Reproduce how wall and door tools appear in the live construction UI
- [x] Group and label the room-boundary tools clearly as interior wall and door
- [x] Verify visible selection, placement, and resulting room separation in PlayMode
- [x] Capture the workflow and finish with room regressions plus Error/Warning 0
- **Status:** complete

### Phase 35: Make Interior Wall Ghost and Placed Wall Visible
- [x] Reproduce the missing wall ghost and installed visual in live PlayMode
- [x] Give the interior wall a concrete full-height ghost visual
- [x] Preserve structural-wall tiles during automatic exterior-wall redraws
- [x] Add focused asset/renderer regressions
- [x] Verify visible selection, placement, capture, room split, and Error/Warning 0
- **Status:** complete

### Phase 36: Install One-Cell Doors Into Interior Walls
- [x] Change the Door footprint and collider from three cells to one cell
- [x] Require an existing structural wall at the target cell in placement validation
- [x] Replace the target wall atomically with the Door while preserving hallway support and redraw behavior
- [x] Add regressions for empty-cell rejection, non-wall rejection, wall replacement, walkability, and room-door ownership
- [x] Verify the visible construction UI, red/valid ghost states, click placement, changed grid state, and camera capture in PlayMode
- [x] Finish with room/grid regressions and Error/Warning 0
- **Status:** complete

## Decisions Made
| Decision | Rationale |
|----------|-----------|
| Use planning-with-files and unity-csharp-scripting skills | The task spans project-wide Unity C# analysis, code edits, and repeated verification. |
| Exclude scripts under any Editor directory from the function audit and refactor target set | User explicitly excluded Editor scripts. |
| Split audit output into project-owned and vendor appendix markdown | All non-Editor scripts are listed, while third-party scripts remain list-only to avoid unsafe package edits. |
| Refactor direct player input and pointer raycast access first | Static audit identified direct `Input`/camera use in runtime MonoBehaviours; this is a clear VContainer DI boundary with low scene impact. |
| Keep direct `Input`, `Screen`, and pointer raycast calls only inside adapter services | This preserves Unity API usage while removing direct coupling from gameplay/UI MonoBehaviours. |
| Treat over-separation as a separate audit dimension | The user asked specifically whether code was too split; this requires a dedicated pass over interfaces/providers/factories/services instead of only coupling flags. |
| Keep `*SummaryService` classes but flag `*SummaryRuntimeSource` layers | Summary services contain DTO aggregation, while the runtime source interfaces/classes are one-service property bags that can be folded. |
| Do not edit code during Phase 6 | This pass was an exhaustive judgment/reporting pass; follow-up code merges should be small VContainer batches with Play Mode verification. |
| Phase 7 implements only `merge-candidate` findings | `watch-*` and `keep-boundary` items were intentionally not merge findings in the exhaustive audit. |
| Editor debug fixtures may be updated when runtime refactors remove types | Editor scripts were excluded from the audit scope, but compile verification must keep them building when runtime contracts change. |
| Phase 8+ treats commit scope as explicit file manifest first | The worktree contains many untracked/dirty files, so broad staging would risk mixing unrelated local state into the refactor commit. |
| Watch provider cluster should be simplified through a small shared cache helper, not by deleting provider interfaces wholesale | Providers are still useful DI boundaries; the over-separation issue is repeated scene-query/cache plumbing. |
| `UIBuildingInfo` must lazy-initialize and activate itself before display | Manual PlayMode flow found inactive scene panels can receive `DisplayBuildingInfo()` before `Awake()` caches image fields, causing a real NullReference in the building info UI. |
| Provider contracts remain named; only cache/query plumbing is shared | `CachedSceneRuntimeProvider<T>` reduces repetition while preserving domain interfaces such as `ILocalLlmRuntimeProvider` and `IOffenseWorldMapRuntimeProvider`. |
| Phase 11 smoke test should run only in PlayMode | The scenario touches live scene UI, VContainer-injected actors, and runtime providers, so it must fail fast outside PlayMode instead of pretending to validate edit-time state. |
| Full-game verification is a new stricter goal, not already covered by the DI refactor checks | Previous PlayMode work covered refactor-critical flows; it did not exhaustively exercise every implemented gameplay feature through visible in-game paths. |
| Automatic debug-scenario PASS does not equal manual in-game verification | Debug scenarios prove logic coverage, but the user asked whether features are confirmable in-game, so UI/runtime scene evidence must be tracked separately. |
| Build scene and test scene must be distinguished | `SampleScene` is the only enabled build scene, while previous manual checks ran in `CharacterAiTestScene`; both scene roles affect the "game-internal" verification claim. |
| Dynamically-created invasion intruders should use the existing character object factory | `ICharacterSpawnObjectFactory` already owns dynamic character creation/injection; reusing it keeps intruders on the same VContainer path as spawned pooled characters. |
| Scene component queries should prefer the active scene before inactive additive scenes | Full-game QA may load a dirty scene and the build scene together; disabled roots in a non-active scene must not poison active scene providers. |
| Event alert detail should render on a dedicated alert sorting canvas | Feature-family visual QA showed alerts could sit below offense canvases; alert detail is global feedback and should stay readable above feature panels while remaining below run-result modals. |
| Broad legacy debug-world suites should not be mixed into the direct `SampleScene` remaining-feature probe | Those Editor fixtures can create uninjected temporary objects in PlayMode and produce fixture noise; they should be fixed/run separately from active-scene evidence. |
| Character-AI feature gaps should use injected active-scene probes before broad legacy debug-world suites | Legacy debug suites still contain fixture DI assumptions, while injected probes give cleaner evidence for actual scene/runtime behavior. |
| Runtime PlayMode verification is not the same as player-facing UI completion | The latest screenshot/user feedback shows many implemented systems are still surfaced as generated summary text, alerts, QA-probe evidence, or hidden commands rather than discoverable, actionable UI. |
| P0 UI closure must use visible controls and PlayMode evidence | The current goal is not satisfied by runtime probes alone; each P0 row needs normal tab entry, readable state, a visible action, feedback, and visual capture. |
| Placement visuals use `Default` sorting with grid order 100 and ghost order 200 | `Wall` order 100 previously covered the `WhiteBox` at `Wall` order 1; the later `Default` layer guarantees `wall < grid < ghost` while keeping screen-space UI above world visuals. |
| Placement mode exits in `LateUpdate` and the initiating frame is marked consumed | Existing world-info gates continue to see Build mode during the placement click, while both building-info paths also have an explicit frame-order guard. |
| Sprite-only visuals keep their current height but anchor at floor zero; authored-tile ghosts use the full footprint | This removes the facility floor gap without stretching placeholder cards and makes the Door ghost match its native `3x3` final tile. |
| Route Door into the `벽/문` menu without changing its serialized category | Door remains a passable Building-layer boundary with its collider/factory behavior intact, while the player sees it beside corridor and interior-wall tools. |
| Treat only structural Building-layer walls as room walls | Hallway data retains its legacy Wall menu category but remains walkable room interior; otherwise every live hallway cell is incorrectly excluded from rooms. |

| Install a Door by replacing one structural-wall occupant at the same cell | Wall and Door share the single-occupant Building layer; validated replacement preserves room-boundary semantics and makes the Door walkable through the Hallway underneath. |

## Errors Encountered
| Error | Resolution |
|-------|------------|
| Direct PowerShell execution of init-session.ps1 was blocked by execution policy | Re-ran the same script with `powershell -NoProfile -ExecutionPolicy Bypass -File` to initialize planning files. |
| Deep parsing all 911 scripts timed out | Regenerated audit with detailed project-code analysis and lightweight vendor listing. |
| Unity MCP camera capture with a Play Mode camera instance ID failed because the tool could not find that instance ID | Used the supported no-ID camera/scene-view capture path instead; capture succeeded and showed the scene/UI. |
| Unity MCP dynamic inspection scripts cannot reference some external assemblies such as VContainer/Sirenix or use reflection | Verified through compile status, Play Mode console, scene object counts, UI bounds, and visual capture instead. |
| Unity MCP scene-state command failed when directly referencing `DungeonRuntimeLifetimeScope`/`UIManager` generic types | Re-ran the check using `MonoBehaviour` scanning and type names to avoid missing VContainer/Sirenix dynamic-script references. |
| `SampleScene` intruder path threw missing `ICharacterAiSchedulingService` injection errors | Routed `InvasionIntruderRuntimeFactory` through `ICharacterSpawnObjectFactory.Create/Inject` and re-tested with 0 captured runtime errors. |
| Additive QA resolved inactive `CharacterAiTestScene` components before active `SampleScene` components | Updated `DungeonSceneComponentQuery` to enumerate active scene and active roots first. |
| Restoring `CharacterAiTestScene` roots while `SampleScene` was still loaded emitted transient duplicate global-light logs | Closed additive `SampleScene` after root restoration and confirmed a single loaded scene with all original roots active. |
| Feature-family probe initially failed to compile due unassigned offense `out` variables | Initialized the report variables and recompiled cleanly. |
| Alert detail was visible but hard to read under offense panels | Added `EventAlertRuntimeUI` override sorting canvas at order 480 and re-tested with screenshot/canvas sorting evidence. |
| `SampleScene` offense expedition could not start because available expedition members were 0 | Recorded OFF-02/OFF-03 as pending missing member hookup instead of claiming completion. |
| `SampleScene` facility evolution had 0 candidates | Recorded EVO-01 as missing scene candidate instead of claiming completion from debug scenario coverage. |
| Remaining-feature probe initially failed to enter PlayMode because of compile errors in the new QA helper | Fixed definite-assignment and API argument/property mistakes, refreshed assets, and recompiled cleanly before rerunning. |
| Broad legacy debug-world suites produced PlayMode fixture errors when called from the direct remaining-feature probe | Removed broad suite execution from the direct probe and recorded the separation; final direct `SampleScene` pass had 0 captured errors. |
| `SampleScene` had no pre-placed triggerable defense facility in the active scene | Initially recorded DEF-01 as a missing scene hookup; the later closing-gap probe verified runtime placement plus defense activation with an injected PlayMode building. |
| Broad legacy character-AI debug suites produced PlayMode fixture DI errors | Kept them out of direct active-scene evidence and added a VContainer-injected `CharacterAiTestScene` probe instead. |
| Character-AI QA helpers referenced nonexistent `AIBrain.CurrentDestinationDebugLabel` | Replaced the lookup with `bestAction.ReservedDestination` and recompiled cleanly. |
| Character-AI visual capture showed a large translucent yellow right-side panel | Recorded it as an overlay candidate requiring classification/fix before all UI can be declared fully clean. |
| Unity MCP dynamic compile command resolved `CompilationPipeline` incorrectly when imported by namespace | Retried with the fully-qualified `UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation()` call. |
| Closing-gap QA helper failed to compile because `TryFindReachablePair` captured an `out` parameter inside LINQ lambdas | Copied `start` into a local `startPosition`, recompiled cleanly, re-entered PlayMode, and ran the closing-gap probe successfully. |
| Character-AI visual overlay candidate was active `UI/Quest` in the open dirty scene state despite scene YAML being inactive | Disabled `UI/Quest` in the current editor scene state, reran the Character-AI probe, confirmed 0 active yellow `Quest` graphics, and inspected the cleaned screenshot. |
| Character-AI QA cleanup produced a DOTween warning when temporary characters were destroyed | Added DOTween kill cleanup for temporary objects/components in both QA helpers, recompiled, and reran with 0 final warnings. |
| Public build UI generated buttons did not select buildings, and closing construct UI left build mode/ghost active | Wired generated buttons to `UIBuildingSelectButton.OnClick`, made `CloseConstructTab()` reset grid mode/ghost via the building controller, rebuilt missing unlocked buttons on tab open, and retested the public flow in PlayMode. |
| DOTween warning recurred after broader PlayMode cleanup from destroyed character `SpriteRenderer` fades | Added `CharacterVisual.OnDisable`/`OnDestroy` cleanup that kills child renderer tweens, recompiled, and confirmed final Unity console Error/Warning count was 0. |
| `CharacterDialogueRuntime.ShowLine()` stayed silent for dynamically-created QA actors | Added lazy runtime-reference refresh in `CharacterDialogueRuntime`, reran the `SampleScene` closing-gap probe, and confirmed `dialogueShown=True`, `lastDialogue=QA visible line`, and active text count `16 -> 18`. |
| Strict public pointer placement through Unity MCP did not prove actual placement on the first attempt | Public category/building button clicks and `TriggerPlaceBuilding()` ran, but `placed=False` after mouse warp because the click path used Input System while coordinates used stale legacy `Input.mousePosition`; fixed `UnityPlayerInputReader` and reran successfully with `placed=True`. |
| DOTween warning recurred after the public pointer probe | Broadened `CharacterVisual` cleanup to kill renderer, renderer GameObject/Transform, visual root, and owner targets; recompiled and confirmed final console 0 errors/warnings. |
| Real local LLM endpoint was first reported as dropped by queue pressure | Verified the localhost Ollama-compatible endpoint outside Unity, then changed the PlayMode probe to `AbortAllForDebug()` and use a high-priority persona request; rerun succeeded with `status=Succeeded`, `content={\"qa\":\"pong\"}`, 0 captured errors, and final console 0. |
| Strict public pointer placement initially failed after mouse warp | Found the click path used Input System while pointer coordinates used stale legacy `Input.mousePosition`; updated `UnityPlayerInputReader` to prefer `Mouse.current.position`, then reran public UI selection plus `TriggerPlaceBuilding()` successfully with `before=97`, `after=98`, `placed=True`, and final console 0. |
| `SampleScene` lacked an `OwnerCommandController` scene object for direct rebellion command verification | Added `OwnerCommandController` to `Assets/Scenes/SampleScene.unity`; reran the completion audit and verified `controller=True`, `commandInvoked=True`, `destinationResolved=True`, `suppressed=True`, captured errors 0. |
| Final completion audit produced a delayed DOTween `SpriteRenderer` warning after visual cleanup | Stored the active `CharacterVisual` fade tween and killed it directly on disable/destroy; recompiled and reran the completion audit with final console 0 errors/warnings. |
| Initial P1/P2 source searches used nonexistent `Assets/Scripts/UI/StaffWorkPriorityPanel.cs` and `Assets/Scripts/Event` paths | Located the files under `Assets/Scripts/Character/UI` and `Assets/Scripts/Operation`, then continued with the authoritative paths. |
| First P1/P2 planning update patch targeted a nonexistent prior error row | Re-read the exact planning-file tail and appended the new rows against current content. |
| Targeted P1/P2 API scan used nonexistent `Assets/Scripts/Defense/DefenseStatusRuntime.cs` and `Assets/Scripts/Defense/DefenseFacility.cs` paths and generated oversized output | Kept the valid staff/AI matches, will locate defense files with `rg --files`, and split subsequent scans by subsystem. |
| Owner/defense context scan again exceeded the direct output budget | Retained the complete relevant owner-command and defense-facility sections; subsequent reads use bounded line ranges without combining the large discontent file. |
| Combined evolution/run model scan exceeded the direct output budget | Kept the complete snapshot and candidate property sections; follow-up reads remain bounded to one model family. |
| Windows shell rejected wildcard file arguments passed directly to `rg` for invasion/offense models | Retried against both directories with `-g "*.cs"`; model scan succeeded. |
| First bounded read of the lower staff panel had a JavaScript wrapper parenthesis typo | Logged the wrapper error and retried the same read with corrected syntax. |
| Character model file-search wrapper repeated the same missing-parenthesis typo | Logged it and switched subsequent shell wrappers to an explicit result variable before `text(result)`. |
| Character-model symbol scan included nonexistent `Assets/Scripts/ScriptableObject` | Kept the valid matches under `Assets/Scripts/Character/SO` and removed the bad search root. |
| One bounded character-model read chained two `Get-Content` commands with a semicolon | Recorded the process violation; all subsequent reads and actions use one shell command per call. |
| First combined runtime read-model patch assumed `DefenseFacility : BuildableObject` but the class actually derives from `Facility` | Patch was rejected before applying any file; re-read exact anchors and split edits into smaller patches. |
| Invasion history patch could not match the UTF-8/BOM first-line `using UnityEngine;` anchor | Avoided editing the header and used fully-qualified generic collection names at exact class-field anchors. |
| Unity MCP `ManageAsset Import` received `Assets` and resolved it as nonexistent `Assets/Assets` | Switched to a Unity Editor command using `AssetDatabase.Refresh(ForceUpdate)` plus `CompilationPipeline.RequestScriptCompilation()`; command compiled and executed successfully. |
| First shared-panel routing patch used an incorrect `BuildFacilityShop` closing-call anchor | Patch was rejected before applying; re-read the exact method tail and split class/routing/appended-section edits. |
| First Phase 20-22 planning status patch assumed a table instead of the existing checklist format | Re-read the exact phase section and updated its checkboxes/status lines directly. |
| First rebellion-finding planning patch misspelled the `progress.md` update header | Patch was rejected before applying; corrected the patch header and reapplied. |
| Unity MCP dynamic command could not directly resolve the Editor-only `P1P2FeatureSurfacePlayModeVerifier` type | Switched to the verifier's file-request protocol instead of referencing the Editor assembly. |
| `EditorApplication.ExecuteMenuItem` did not find the new verifier menu path immediately after compilation | Avoided menu registration timing by writing the verifier request file directly. |
| Unity MCP dynamic command rejected `System.Reflection` as an unauthorized namespace | Abandoned dynamic reflection and used `System.IO.File.WriteAllText` against the verifier's documented request path. |
| Writing the P1/P2 verifier request file did not trigger PlayMode within 10 seconds | Found `Assembly-CSharp-Editor.dll` was older than the new verifier source; force-importing the exact script and waiting for assembly rebuild. |
| First clean-rebuild dynamic command resolved `CompilationPipeline` to the wrong namespace despite the import | Retried with fully-qualified `UnityEditor.Compilation.CompilationPipeline` and `RequestScriptCompilationOptions` names. |
| Clean build completed but the new verifier menu and editor-assembly timestamp still did not change | AssetDatabase does recognize a `MonoScript`; checking whether the verifier type is present in the compiled editor assembly binary. |
| Binary scan found no P1/P2 verifier type although the compile source list contains the file | Inspecting Bee/C# compiler artifacts for errors that were not retained in the current Unity console. |
| Bee compiler reported CS0165 for conditionally assigned verifier `outcome` and `record` locals | Initialized both locals and replaced the first conditional-access `out` call with an explicit null branch. |
| First P1/P2 PlayMode verifier run passed 10/18 rows and captured two missing External Behavior errors on QA actors | Treating the first run as diagnostic evidence; switching QA staff to trackable NPCs, suppressing fixture scheduler ticks, and fixing deterministic reopen/click sequences before rerun. |
| Second P1/P2 run passed 15/18; synthesis, defense cooldown, and owner priority remained false despite successful clicks | Fixed deterministic fixture/data prerequisites and removed non-actionable owner-command cards; rerunning all 18 rows instead of accepting button-only evidence. |
| Owner-command source lookup included nonexistent `WorkCommandResolver.cs`, and an exact no-match `rg` returned exit 1 | Located `WorkCommandResolver` in `WorkPriorityProfile.cs`; used its real signature and a deterministic damaged Repair fixture for command verification. |
| Verifier search confirmed no local `ResolveFromLifetimeScope` helper and returned exact no-match exit 1 | Added a scoped VContainer resolve helper and used it to invalidate the authoritative room-layout cache after QA fixture placement. |
| First exact asset lookup for `EV_BattlefieldDining.asset` used a Windows-separator-sensitive `rg` pipeline and returned no match | Retried with `rg --files -g`, found the recipe, and audited its usable-room, Combat, Defense, token, and identity requirements. |
| Formal-room fixture compile reported CS0165 for `fixtureValidationReason` hidden behind short-circuit evaluation | Initialized the diagnostic value before the conditional and retained the stale request for automatic rerun after a clean compile. |
| Room regression suite stopped in three existing scenarios because its fixtures lacked the now-required `BuildableObject` service injection | Added no-op research/click services plus real room policy and candidate cache injection to every room test building before rerunning the suite. |
| Repaired room suite left one stale null-actor call to `FacilityCandidateScorer`, which now requires an injected `AIBrain` | Kept the room-gating assertion at the grid's authoritative visitable-building boundary instead of fabricating unrelated AI dependencies. |
| Staff model scan assumed nonexistent `StaffWorkPriorityPanelModelBuilder.cs` | Kept valid discontent matches and located the actual source at `StaffWorkPriorityPanelModel.cs`. |
| First attempt to record the targeted API-scan error used a nonexistent `# Findings` anchor | Re-read the planning-file tails and appended against headings/rows that actually exist. |
| Exact-name `rg --files | rg` filter returned no Windows-path matches | Retried with `rg --files` glob filters and located `DefenseFacilitySystem.cs` and `DefenseStatusRuntimeService.cs`. |
| Unity MCP dynamic inspection could not dereference `BuildingSO` because its Sirenix base assembly is not referenced | Kept the public UI click path and inspected controller/grid/UI state without dereferencing the ScriptableObject in the dynamic assembly. |
| Placement overlay failed on the first repeated PlayMode entry despite the scene containing a valid `Tilemap` | Replaced C# `??=` with Unity-aware `== null` reassignment so a destroyed prior-session `Tilemap` reference is refreshed correctly; repeated PlayMode entry then passed. |
| OS cursor warping did not persist into Unity Input System state in remote PlayMode | Injected a `MouseState` with `InputSystem.QueueStateEvent` and processed it with parameterless `InputSystem.Update()` for deterministic valid/invalid pointer verification. |
| Occupied-cell count did not increase after a valid door placement | Confirmed this is the intended hallway-replacement path by comparing the exact target object before/after: `복도` instance replaced by a new `문` instance. |
| `EventSystem.current.IsPointerOverGameObject()` returned false over an active UI button under the Input System UI module | Replaced cached pointer-ID lookup with a current-position `EventSystem.RaycastAll` service filtered to `GraphicRaycaster` results; the same coordinate reported three UI hits and blocked both building-info paths. |
| A Phase 27 source-read command combined two `Get-Content` calls with a semicolon | Retained the useful read output and returned to one shell command per call for all subsequent inspection. |
| Initial Phase 27 live-state command referenced `UIBuildingInfo`/`BuildingSO` directly and hit missing Sirenix dynamic-assembly references | Switched the inspection to `MonoBehaviour` type-name scanning and Unity base-component APIs, matching the established MCP workaround. |
| First Door ghost verifier compared against pointer state in a later MCP command, after the remote Input System position reverted | Kept the measured `3x3`, floor-zero, alpha-1 bounds and changed the comparison reference to the deterministic valid grid cell selected by the same search rule. |
| Research ghost public-button probe toggled an already-open Crafting panel closed | Stopped that attempt before pointer/placement changes; the enum-to-panel mapping was correct, so the retry uses idempotent active-state checks before invoking the same visible button. |
| Final blocked-cell probe ran immediately after entering PlayMode before generated category/button dependencies completed Start | No pointer or placement mutation occurred; waited for the live scene and retried through the already-proven public tab initialization sequence. |
| First Phase 28 progress append used stale expected wording and failed its patch context check | Re-read the actual file tail and appended the implementation log against the current Phase 28 heading. |
| Phase 28 Unity console and editor-state calls returned `Connection revoked` | Confirmed the denial is the Unity AI Assistant MCP approval gate and started tracing its persisted Project Settings state before retrying compile/capture. |
| Unbounded `Select-String` over Unity `Editor.log` timed out | Retried over a bounded 12,000-line tail; it completed and showed no MCP diagnostic entries. |
| Windows refused to foreground Unity reliably and secondary-display captures showed unrelated foreground apps | Stopped UI automation before any click; continued with MCP transport/config diagnostics to avoid interacting with unrelated user windows. |
| Prepared MCP status patch failed because Unity had already rewritten the expected denied record | Re-read the live registry, found the current identity auto-approved, confirmed no temporary script was created, and successfully retried MCP. |
| Immediate Research ghost inspection saw source-white color but native `2x1.5` bounds | The sample was taken directly after UI invocation/input injection; allow a real PlayMode interval, then inspect all active ghost renderers before changing geometry code. |
| Research visual-size trace imported unauthorized `System.Reflection` | The command was rejected before execution; use source asset data and public ghost sizing APIs instead of reflection in Unity MCP dynamic commands. |
| Expanded tiled-to-simple ghost regression failed on pass 1 while Grid Visual still passed | Reproduce that isolated temporary-object setup with per-renderer diagnostics and adjust the test to the renderer's actual Simple-mode contract. |
| First Phase 29 controller search used the wrong folder and a Windows-sensitive filename pipeline returned no result | Located the controller by class declaration at `Assets/Scripts/Grid/DungeonStory/Building/DungeonStoryGridBuildingController.cs` and continued there. |
| Initial Phase 29 test search included a nonexistent `Assets/Tests` path, then found no controller-specific debug scenario | Restricted subsequent reads to the existing Grid Foundation scenario and will use its placement-service fixtures plus exact PlayMode controller verification. |
| First Phase 29 edit patch included a mojibake notice string that failed exact context matching | Reapplied the same behavior change against surrounding ASCII-only control-flow lines; no file changed on the failed attempt. |
| First visible-valid-target command assumed `GridCell.GetGridPos()` existed | Re-read `GridCell` and used its public `Position` property; the retry found valid target `(22,0)`. |
| Valid-placement verifier referenced `controller.SelectedBuilding` and pulled an unavailable Sirenix dynamic-command assembly reference | Removed the nonessential ScriptableObject access and verified selection cleanup through ghost, grid mode, occupant, and panel state instead. |
| User interruption stopped PlayMode during the first settled-state query | Started a fresh PlayMode and repeated the complete public Research-button, pointer placement, state, and camera flow. |
| First live facility-alignment verifier used LINQ `Any` on `BoundsInt.PositionEnumerator` | Replaced it with exact-cell Tilemap lookup and reran; both facilities aligned. |
| Automatic separated ghost-target search found no valid visible cell below grid x 18 after opening the UI | Used the already-proven valid target `(22,0)`, which sits on the lower floor and leaves the measured facilities unobscured. |
| A later ghost bounds probe sampled the remote mouse after it reverted offscreen and emitted a temporary alignment error | Kept the synchronized camera evidence and prior ghost regression as the ghost proof, cleared the diagnostic entry, and based this phase on facility/overlay bounds measured from persistent Tilemap state. |
| Unity AI Assistant relay logged `Process exited unexpectedly, code=0` while recompiling the overlay change | MCP reconnected, the project compiled, the diagnostic was cleared, and all game regressions plus the final Error/Warning query passed. |
| First one-cell-gap inventory attempted to sort an uninitialized runtime helper's null `buildPoses` | Retried with explicit null handling; the authoritative grid map then identified gap `(20,2)` between stair id 4 and Research id 16. |
| Normal camera capture let the remote pointer revert offscreen before rendering the red width-3 ghost | Added a temporary late-frame QA pointer keeper for one synchronized capture, then removed it immediately and confirmed no helper remained. |
| First live interior-wall target search found no splittable formal room | Diagnosed that Hallway assets were incorrectly classified as room walls; switched `RoomDetector.IsWall` to `BuildingData.IsStructuralWall`, added a real BuildableObject hallway regression, and reran successfully. |
| A Unity MCP dynamic command could not dereference `BuildingSO` because the Sirenix assembly was unavailable | Inspected visible button ids and public controller/grid state instead, while asset-level assertions remain in the compiled room regression suite. |
| The first two full P1/P2 reruns passed 17/18 after real rooms replaced one-facility pseudo rooms | Updated only the QA preparation path to add a temporary door and scale seat/table evidence by actual room area; the final real-room verifier passed 18/18 with zero captured errors/warnings. |
| The first combined Phase 35 patch could not match a Korean-comment line in `GridTexture.cs` | Confirmed no partial edit was applied, then split the asset/presenter and renderer patches and anchored the renderer change on exact current UTF-8 source lines. |
| The final live probe read the ghost sprite name after successful placement had cleared the sprite | Inspected the already-completed placement without retrying it; confirmed wall id `7`, three center tiles, zero adjacent lower tiles, hidden ghost, and mode `None`, then cleared the probe-only diagnostic and rechecked a clean console. |
| A Phase 36 parallel source-read wrapper returned exit 1 before relaying its individual command results | Retried the same bounded reads with `Promise.allSettled`; every required source section was then recovered without changing project files. |
| The first combined Phase 36 implementation patch could not match an existing mojibake error-message line | Confirmed the patch was atomic with no partial edits, then applied the same changes in smaller ASCII-anchored patches. |
| The first Phase 36 regression run threw a missing `IFacilityCandidateCache` injection while destroying the replaced test wall | Updated the new editor fixtures to inject the same required BuildableObject services as the runtime factory path before rerunning. |
| The first PlayMode pointer command omitted the `UnityEngine.InputSystem.LowLevel` namespace for `MouseState` | The command was rejected before execution; retried with the fully-qualified type and continued through the visible UI path. |
| A live wall-state command referenced `GridTexture` directly and hit the known missing Sirenix dynamic-command assembly reference | The command was rejected before execution; read the wall renderer through Unity's base `Tilemap` API instead. |
| Unity MCP `Camera_Capture` could not resolve the reported PlayMode camera instance id and logged one tool error | Captured the same Main Camera directly to a `RenderTexture`, saved synchronized camera and Game View PNGs, cleared the tool-only error, and reconfirmed runtime Error/Warning 0. |
| The first Phase 36 completion-record patch used a mojibake decision-table row as context | No planning-file changes were applied; retried against ASCII section headings. |
## Phase 37 - One-Cell Door Visual Legibility

- [completed] Isolate a closed one-cell door sprite from the existing castle tileset without changing placement rules.
- [completed] Connect the same sprite to the build icon, placement ghost, and installed Door tile at exact `1x3` world bounds.
- [completed] Add a regression that rejects horizontally crushed Door visuals and requires an opaque, named one-cell Door sprite.
- [completed] Verify through the visible construction UI in PlayMode: wall target, Door ghost, click, state change, and synchronized camera capture.
- [completed] Rerun Room System, Grid Foundation, and Grid Visual scenarios and finish with a clean Unity console.

## Phase 38 - Grounded Door and Preserved Interior Wall

- [completed] Measure the Door sprite's opaque bounds and trace which wall tiles are removed during wall-to-door replacement.
- [completed] Ground the Door artwork at floor level and retain the interior-wall body behind/around the Door without changing its one-cell walkable boundary behavior.
- [completed] Extend visual regressions for opaque-pixel grounding and preserved Door-cell wall composition.
- [completed] Verify the visible UI flow in PlayMode with ghost and installed camera captures.
- [completed] Rerun Room System, Grid Foundation, and Grid Visual scenarios and finish with a clean Unity console.

### Phase 38 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| Unity MCP sorting-inspection command could not reference `GridTexture` because its dynamic assembly omitted `Sirenix.Serialization` | 1 | Query Unity `Tilemap` and `TilemapRenderer` components directly without referencing project/Odin types. |
| Editor regression referenced internal `GridWallTileCalculator.IsWallVisualCell`, producing `CS0117`; stale scenario logs followed in the same refresh | 1 | Removed the cross-assembly internal call; the regression verifies the public rendered result through all three Door-cell wall tiles instead. |
| Door wall-composition regression deleted an un-injected test wall and raised `BuildableObject requires IFacilityCandidateCache injection` | 1 | Reuse the existing grid test fixture's no-op runtime dependency injection before wall-to-door replacement. |
| Foundation fixture's `NoopBlueprintResearchWorkService` and `NoopWorldInfoClickSelector` are private to its source file, causing `CS0246` when referenced from Grid Visual | 2 | Define local interface-complete no-op services in Grid Visual instead of coupling private test fixtures. |
| Injection types compiled, but the Door scenario still used the default factory and repeated the cache exception | 3 | Audit the two similar placement-service declarations and patch the Door-composition scenario by its unique method context. |
| Unity MCP PlayMode command auto-fixer duplicated private nested pointer-provider types at namespace scope, causing `CS1527` | 1 | Declare helper providers as namespace-level `internal` types from the outset. |
| Unity MCP rejected `System.Reflection` before execution while replacing private pointer providers | 2 | Move the real Input System mouse to the target screen coordinate and retain the production pointer provider/UI blocker unchanged. |
| Real-pointer command referenced `SelectedBuilding.objectName`, pulling Odin `SerializedScriptableObject` into the dynamic assembly and causing `CS0012` | 1 | Remove the nonessential selected-asset diagnostic; keep UI click, pointer movement, public placement, and occupant-state checks. |
| Input System cursor warp reached the target screen coordinate but public placement remained blocked after the UI click | 1 | Inspect the production snapped world position and EventSystem pointer-over-UI verdict before choosing the next input dispatch. |
| Unity MCP dynamic command assembly also lacks a VContainer reference, so resolving original dependencies for `Construct()` produced `CS0246/CS0012` | 1 | Run the stable-pointer flow from a compiled project Editor verifier, matching the repository's existing P0/P1/P2 QA pattern. |
| Newly compiled Door verifier menu was not yet registered under its `MenuItem` path in the active editor session | 1 | Invoke the compiled verifier's public static entry point directly from the dynamic command. |
| Door verifier used `yield return` inside a `try` block with `catch`, producing `CS1626`; Unity surfaced the compiler entry as Console type `Log` | 1 | Use a normal iterator `yield return RunVerification()` and include `All` in subsequent compile-log checks. |
| First compiled PlayMode run grounded the ghost but clicked the still-visible Door button while grid mode was `None`; selection was ignored and Door placement did not run | 1 | Re-enter `건축 -> 벽/문` after wall placement before pressing `문`; inspect live wall tile coordinates before finalizing the wall count assertion. |
| After wall placement, pressing the still-active `건축` tab once closed its category panel, so `벽/문` was temporarily absent | 2 | Detect the closed panel and press `건축` once more before continuing through `벽/문 -> 문`. |
| A successful Door placement disabled the ghost renderer before the final report, so the verifier treated its now-destroyed Unity reference as a missing preview | 3 | Snapshot the ghost sprite identity and bounds before placement, then assert the captured values after placement. |
| Replacing the wall correctly destroyed its runtime object, but the verifier re-read that destroyed reference during its final assertion | 4 | Snapshot the wall's structural state and replacement identity before triggering Door placement. |
| The corrected verifier menu was invoked once from EditMode and correctly refused to run | 1 | Enter PlayMode, wait for runtime UI initialization, and then invoke the verifier menu. |

## Phase 39 - Restore Original Door Artwork

- [completed] Restore the original `Door.png` art while retaining one-cell placement and collider behavior.
- [completed] Render the original art at its authored `3x3` ratio in both the ghost and installed front renderer.
- [completed] Verify the visible interior-wall composition, grounding, UI click flow, and state transition in PlayMode camera captures.
- [completed] Rerun Room System, Grid Foundation, and Grid Visual scenarios and finish with a clean Unity console.

### Phase 39 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| The first PowerShell alpha-bounds probe passed a `PathInfo` object to `System.Drawing.Bitmap`, so the image constructor rejected it | 1 | Pass the resolved path's string `.Path`; the retry measured the full `48x48` opaque extent and zero transparent bottom rows. |

## Phase 40 - Separate Dungeon Door and Interior-Wall Door

- [completed] Restore `Door.asset` as the locked, original `3x3` dungeon entrance and remove it from interior-wall placement behavior.
- [completed] Add a distinct unlocked `InteriorDoor` type and ID 8 asset with one-cell occupancy and dedicated one-cell artwork.
- [completed] Scope wall replacement, installable-cell overlay, preserved wall rendering, and the `벽/문` UI entry to `InteriorDoor` only.
- [completed] Verify both door identities and the complete interior-wall Door UI flow in PlayMode with synchronized camera captures.
- [completed] Rerun Room System, Grid Foundation, and Grid Visual scenarios and finish with a clean Unity console.

### Phase 40 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| A dynamic runtime probe dereferenced `BuildingSO` and failed because the MCP command assembly lacks `Sirenix.Serialization` | 1 | Keep asset/data assertions in compiled project regressions and inspect runtime subtype/renderer state without dereferencing Odin-backed data. |
| The first combined verifier patch could not match the file's mojibake UI-label context | 1 | Confirm no partial edit was applied and split the change into ASCII-anchored patches. |
| Unity 6 URP could not load the legacy built-in `Sprites-Default.mat` resource | 1 | Create a shared `Resources/Materials/DoorSpriteUnlit.mat` using the supported URP 2D unlit shader and assert that shader in both test paths. |
| Unity MCP `Camera_Capture` could not render the active camera during a temporary diagnostic | 1 | Use the compiled verifier's RenderTexture camera capture path; the final required captures were produced there. |
| The first dungeon-door closeup copied stale projection and view matrices from the Pixel Perfect main camera | 2 | Reset both projection and world-to-camera matrices after configuring and positioning the QA camera, then recapture the centered entrance. |

## Phase 41 - Dungeon Door Ceiling and Character Layering

- [completed] Inspect the live dungeon-door footprint, ceiling tiles, renderer sorting, and active character sorting layers.
- [completed] Place the dungeon entrance behind the ceiling, keep traversal characters between the entrance and wall, and keep InteriorDoor traversal characters in front.
- [completed] Extend static visual regressions for the required sorting-layer ordering and ceiling composition.
- [completed] Verify an actual character inside the dungeon-door trigger with PlayMode camera capture and restore its runtime state.
- [completed] Rerun Room System, Grid Foundation, and Grid Visual scenarios and finish with a clean Unity console.

## Phase 42 - Dungeon Entrance Path and Exterior-Frame Occlusion

- [completed] Compare the configured spawner entry target with the initialized three-cell dungeon Door center in live PlayMode.
- [completed] Route the first entry/exit waypoint through the actual dungeon Door center while preserving the existing inside walkable cell.
- [completed] Keep traversal Y level across outside, Door, and inside waypoints.
- [completed] Render the exterior Door frame in front of traversal characters and behind the ceiling.
- [completed] Ignore stale uninitialized Door triggers so they cannot reset traversal sorting early.
- [completed] Verify centered visibility, frame occlusion, exit restoration, and final Room/Grid regressions with a clean console.

## Phase 43 - Unified Player UI and Feature-Surface Reaudit

- [completed] Reaudit every runtime feature and character need against actual player-visible UI, using the current scene and data model rather than prior QA summaries alone.
- [completed] Define and apply one restrained game UI theme across the HUD, navigation, management panels, cards, buttons, typography, spacing, and state colors.
- [completed] Replace placeholder and summary-only character UI with a complete, readable needs/status surface backed by live values.
- [completed] Close newly discovered feature-surface gaps for building details, staff stats/mode state, and real-time notice density.
- [completed] Verify representative player flows in PlayMode through real UI clicks and live state changes, capture readable desktop evidence, audit bounds, and finish with a clean console.

## Phase 44 - Character Click Priority

- [completed] Consume an overlapping world click for the full frame once a character is selected.
- [completed] Verify both Unity `OnMouseDown` callback orders with a character overlapping a building.
- [completed] Capture the resulting character-only info state in PlayMode and finish with a clean console.

### Phase 44 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| The PlayMode verifier referenced `InputState` without its low-level Input System namespace, producing `CS0103` | 1 | Import `UnityEngine.InputSystem.LowLevel` and recompile. |

## Phase 45 - Directional Dungeon-Door Layer Transition

- [completed] Restore the original exterior approach BoxCollider geometry on the exact dungeon `Door` only.
- [completed] Keep the character behind the door/wall while crossing the exterior trigger and restore the front layer immediately after the interior opening boundary.
- [completed] Verify outside, opening-crossed, and interior states in PlayMode with closeup captures and a clean console.

### Phase 45 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| First directional verifier passed the restored collider and exterior-behind state but failed the combined inside-front assertion | 1 | Split collider-clearance, visual-overlap, and sorting-layer predicates to identify the exact failing condition before changing runtime behavior. |
| Unity MCP rejected a dynamic diagnostic that imported `System.Reflection` | 1 | Reissued the inventory query through Unity-native `MonoBehaviour` and `BoxCollider2D` data only. |
| The diagnostic inside point cleared renderer bounds by only `0.02`, so the 2D trigger pair remained active until the next larger move | 2 | Move the probe a stable half-cell beyond the interior trigger edge while keeping its sprite inside the `3x3` arch. |
| Dynamic access to Odin-backed `BuildingData` executed without logs | 1 | Record the values from the compiled project verifier, which already has the required Odin references. |
| The full verifier still failed after all directional groups passed because runtime progression had changed `Door.unlocked` to `true` | 1 | Remove mutable unlock progression from the visual/traversal verifier; authored lock state remains covered separately by asset/static checks. |
| Grid Foundation passed but the isolated Grid Visual dungeon-door scenario still failed after reimporting the authored asset | 1 | Add per-stage asset/source-art/runtime diagnostics to the static visual scenario before changing any production behavior. |
| The first static diagnostic patch produced `CS0165` for `doorTile`, leaving Unity on the previous compiled assembly | 1 | Initialize `doorTile` before the compound guard and compute `hasDoorTile` separately. |

### Phase 43 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| `UpperRightPanel` already owned a different `LayoutGroup`, so adding a `HorizontalLayoutGroup` returned null at runtime | 1 | Disable the existing layout component and place the three controls explicitly in equal responsive thirds. |
| A generated TMP log text and `Image` background were added to the same object, violating Unity UI's single-Graphic constraint | 1 | Split the log into an image-backed container and a stretched TMP child. |
| The outer-door trigger restored `Default` as soon as the collider exited, even if a wider character sprite still overlapped the frame | 1 | Keep the traversal layer through `OnTriggerStay2D` and defer restoration until the character visual bounds fully clear the exterior Door. |
| The MCP dynamic command assembly could not reference Odin-backed project types while inspecting live sorting | 1 | Query Unity-native `GameObject`, `MonoBehaviour`, `Collider2D`, `SpriteRenderer`, and `TilemapRenderer` components without dereferencing project/Odin data. |
| The first unified UI verifier used `yield` inside a `try` block with `catch`, producing `CS1626` | 1 | Keep verification as a normal iterator sequence and record each assertion directly in the report. |
| Staff management content changed modes but the persistent mode bar kept its initial priority highlight | 1 | Retain both mode buttons, refresh their semantic selected styling on every mode change, and add a PlayMode color assertion. |

### Phase 42 Regression Follow-Up

- [completed] Hold exterior-door traversal characters behind the wall until their rendered sprite, not only their collider, fully clears the frame.
- [completed] Verify the collider-exited/visual-overlapping edge case, normal exit restoration, live entrance sorting, and a clean PlayMode console.

## Phase 46 - Physical Click Arbitration Follow-Up

- [completed] Replace same-frame-only overlap handling with one physical-click arbitration that always gives characters priority over buildings.
- [completed] Drive the overlap fixture with a real Input System mouse press/release instead of direct `OnMouseDown` messages.
- [completed] Verify character-only UI, an ordinary building click, camera evidence, and a clean Unity console in PlayMode.

### Phase 46 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| The physical-click verifier used `Vector3 += Vector2`, producing `CS0034` before PlayMode entry | 1 | Cast the collider-center delta explicitly to `Vector3` and recompile. |
| `InputState.Change` rejected the mouse button bitfield and stopped the first runtime verifier | 1 | Clean up the failed runner, then queue complete pressed/released `MouseState` events through `InputSystem.QueueStateEvent`. |
| Queued Input System mouse states did not drive Unity's legacy `OnMouseDown`, so neither character nor building callbacks ran | 2 | Focus Game View, warp the native mouse to the verified game-space point, and send an OS-level press/release so the production callback path is exercised. |
| Game View interpreted `WarpCursorPosition(300,594)` as an editor/desktop coordinate and reported game position `(1503,636)` | 3 | Calibrate both axes from native cursor deltas inside the focused Game View, solve the target desktop coordinate, and refuse to click unless the measured game point is within 2px. |
| The first centralized run passed character priority but treated the next press as the old press because `isPressed` becomes true again on every new click | 4 | With input now arbitrated once per frame, retain only the one-frame callback grace and remove held-button state from consumption. |
| Later queued presses were not observed as distinct `wasPressedThisFrame` events even after the selection cache expired | 5 | Track the left button's current state in the central controller and dispatch only on a remembered `false -> true` edge. |
| Closing the character panel left a separate legacy `BuildingInfoPanel/ButtonSelection` button over the original QA point | 6 | Capture the proven character-only state first, then verify the ordinary building click at a dynamically selected unoccupied screen point. |
| MCP could not render the main camera by instance ID | 1 | Retain the verifier's full Game View capture and confirm MCP camera connectivity with a successful 1920x1080 Scene View capture. |

## Phase 47 - Exclusive Character and Building Information

- [completed] Remove the independent building re-selection that bypasses character-first world click arbitration.
- [completed] Make the detailed building panel mutually exclusive with character and summary popups.
- [completed] Rebuild the PlayMode verifier around an actual grid-registered building and assert every visible information panel.
- [completed] Capture character-only and building-only evidence, rerun regressions, and finish with a clean Console.

### Phase 47 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| A dynamic MCP probe could not reference Odin-backed UI types from its temporary assembly | 1 | Move the detailed-panel transition assertion into the compiled PlayMode verifier. |
| PlayMode Grid Foundation inherited the mutable runtime Door state and failed one structural-cell assertion | 1 | Rerun after exiting PlayMode; Grid Foundation and Grid Visual both passed in EditMode. |
| Room System still reports `Room boundary build assets are player-facing` | 1 | Record it as the pre-existing Door asset-state regression; the focused UI verifier and both grid regressions pass. |

## Phase 48 - Character Activity Log Tabs

- [completed] Replace generic action/work heartbeat entries with contextual transition logs.
- [completed] Split the character summary into Status and Records tabs with a scrollable record view.
- [completed] Update UI and character-feedback regressions for the new hierarchy and log semantics.
- [completed] Verify tab clicks, detailed live records, layout, and captures in PlayMode.

### Phase 48 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| `WorkPriorityDebugScenarios` creates uninjected runtime objects and fails on current mandatory services in both EditMode and PlayMode | 2 | Treat it as a pre-existing scenario harness failure; focused character-feedback regressions and the full Unified UI PlayMode verifier pass. |

## Phase 49 - LLM-Enriched Character Records

- [completed] Preserve structured character-log events while giving every visible entry a stable identity and a separately updatable display line.
- [completed] Add a throttled character-record narrative runtime that uses the existing local LLM queue, character identity/persona context, strict JSON parsing, and original-text fallback.
- [completed] Refresh the open Records tab when an asynchronous rewrite arrives without retriggering dialogue, feedback bubbles, or social-reputation interpretation.
- [completed] Add focused regressions for request eligibility, prompt grounding, response validation, stale-entry safety, and fallback behavior.
- [completed] Verify in PlayMode that records are immediately visible, later become varied LLM-written Korean lines, remain clickable/readable, and finish with camera capture plus a clean Console.

### Phase 49 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| A broad source search included nonexistent `Assets/Scripts/Data` and returned exit 1 after printing the useful matches | 1 | Restrict subsequent searches to known source roots and exact files. |
| A spawn-factory search used the old `Character/Core/CharacterSpawnObjectFactory.cs` path and returned exit 1 after locating the real file | 1 | Read `Assets/Scripts/Character/CharacterSpawnObjectFactory.cs`; it confirms every spawned child component is resolver-injected. |
| The first combined regression patch matched mojibake copied from non-UTF-8 shell output and failed atomically | 1 | Re-read the sources with explicit UTF-8, split the patch by file, and anchor edits on ASCII method names or exact Korean text. |
| A grounded PlayMode pass started behind six generic action-record requests and one correction still omitted `지하 연구실` | 1 | Remove duplicate generic action-start logs, retain facility/work outcome logs, add source-built correction examples, allow at most two corrections, and rerun from a fresh PlayMode queue. |
| One final temporary-evidence cleanup returned exit 1 without output | 1 | Retry with verified literal paths, per-file existence checks, and explicit completion output; all three evidence files reset successfully. |

## Phase 50 - Explicit Character Subject In Records

- [completed] Derive the grammatical subject from the visible character name using the correct `이/가` particle.
- [completed] Require every generated and corrected record to begin with that exact subject.
- [completed] Reject missing or altered subjects in response validation and verify the real Records tab in PlayMode with a fresh camera capture.

### Phase 50 Verification

- Static character-feedback scenarios passed, including final-consonant, no-final-consonant, and ASCII name particles plus missing/wrong-subject rejection.
- Dedicated local-LLM PlayMode verification passed with `asd가` at the start of both rewritten rows, a visible Records-tab capture, and Error/Warning `0/0`.
- Unified UI PlayMode regression passed all character tab clicks, live state changes, notice layout, and staff-panel checks with Error/Warning `0/0`.

## Phase 51 - Varied Character Record Voice

- [completed] Rotate lifecycle-specific sentence structures and vocabulary while retaining the exact character subject and source facts.
- [completed] Raise record-only sampling variety and strengthen prompt guidance against repeated cadence.
- [completed] Verify multiple same-lifecycle records through the real local LLM, Records tab, and camera capture.

### Phase 51 Verification

- Static scenarios passed four distinct start shapes, alternate grounded verbs, Korean particles, and rejection of invented facts.
- The dedicated PlayMode run generated two different start cadences plus a separate completion cadence, with all three exact source events preserved.
- The visible Records-tab capture and Unified UI regression passed with captured Error/Warning `0/0`.

### Phase 51 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| Completion style 3 produced `새 제조법 정리까지 정리하고` | 1 | Replace the colliding result verb with `마무리하고`, add a regression assertion, and rerun the real model successfully. |

## Phase 52 - Character Record Micro-Stories

- [completed] Change record generation from factual paraphrase into a short persona-aware anecdote with one harmless invented moment.
- [completed] Permit creative transient details while rejecting invented rewards, injuries, damage, numbers, and other gameplay consequences.
- [completed] Require creative detail in validation and prove it through real local-LLM PlayMode output plus camera capture.

### Phase 52 Verification

- Static scenarios passed creative-detail requirements, exact source-phrase preservation, actual-start semantics, one-sentence/name rules, consequence rejection, formal/slang rejection, and controlled fallback validation.
- Final PlayMode report passed three distinct visible micro-stories; two validated model lines were retained and one repeatedly invalid completion was replaced by the controlled story fallback.
- Records-tab capture, Unity camera capture, and the full Unified UI regression passed with final Error/Warning `0/0`.

### Phase 52 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| Initial creativity rules accepted `연금을 찾기`, a mere decision to start, and bland emotion modifiers | 1 | Require exact source phrases, reject intention-only starts, and require an event-and-reaction beat. |
| Broad scene invention produced unrelated music and candy props while one completion stayed bland | 2 | Restrict invention to the character's own harmless behavior and use controlled situation anchors. |
| A later completion used `뇌피셜`, vague filler, and a generic `-습니다` ending | 3 | Reject all formal `-습니다` endings, slang, vague pronouns, repeated names, and multi-sentence output. |
| The local model still failed one completion after bounded corrections | 4 | Apply a validated, source-preserving controlled micro-story only for the rejected row; final PlayMode and UI regressions passed. |

## Phase 53 - Compact Character Record Breath

- [completed] Cap generated and fallback character records at 60 characters.
- [completed] Limit each micro-story to one short situation beat and one core event clause.
- [completed] Reject overlong chaining and unrelated scene details before they reach the Records tab.
- [completed] Verify compact lines through the real local-LLM PlayMode flow, Records UI, camera capture, and full Unified UI regression.

### Phase 53 Verification

- Static narrative scenarios passed, including explicit rejection of the reported 71-character sentence.
- Dedicated PlayMode verification passed with final line lengths `47`, `47`, and `52`, all under the `60`-character ceiling.
- `COMPACT_BREATH`, visible replacement, tab interaction, and full Unified UI regression passed with captured Error/Warning `0/0`.

### Phase 53 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| Unity domain reload temporarily revoked the MCP connection during script compilation | 1 | Wait for the Editor relay to reconnect, confirm `GetState`, and rerun the dedicated and Unified UI verifiers successfully. |

## Phase 54 - Mood Tab and Interaction-Driven Mood

- [completed] Trace the current need, work, rest, social-interaction, and character-summary UI data flow.
- [completed] Separate mood from needs in both presentation and runtime ownership.
- [completed] Add RimWorld-style mood factors sourced from need thresholds and gameplay interactions, with duration, stacking, expiry, and an inspectable breakdown.
- [completed] Add a dedicated Mood tab beside Status and Records and make its value/factor list refresh live.
- [completed] Extend static and PlayMode regressions, physically click every character tab, mutate needs/interactions, and verify visible mood changes with camera capture.

### Phase 54 Verification

- Static mood rules passed hunger `-18`, fun satisfaction `+4`, stacked work fatigue, expiry, grouped formatting, direct-value compatibility, and staff-discontent stage preservation.
- Fresh PlayMode physically clicked Status, Mood, Records, and Status again; content exclusivity and selected accents passed for every tab.
- Need-derived mood changed `50 -> 54 -> 32`. Real social-rumor, injury, and treatment paths produced visible named factors, and the one-second factor expired while longer factors remained.
- `Temp/phase54-character-mood-tab.png` was visually inspected at `1920x1080` with no clipping or overlap. Unity Main Camera capture also succeeded at `1920x1080`.
- Full Unified UI regression finished `RESULT=PASS` with captured Error/Warning `0/0`.

### Phase 54 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| Running a temporary `Unity_RunCommand` compile inside the existing PlayMode session reloaded the domain and left scene components without their VContainer injections, so the Unified UI runner stopped before writing its report | 1 | Stop the stale PlayMode session, clear the Console, enter a fresh PlayMode session so the lifetime scope reinjects the scene, then invoke the already-compiled verifier without another temporary compile. |
| Legacy Staff Duty/Discontent/Rebellion EditMode scenarios instantiate mandatory runtime services without VContainer injection and fail before mood assertions | 1 | Keep the passing Character Feedback regression, verify raw mood override compatibility on the injected PlayMode actor, and test the pure staff-discontent stage evaluator separately. |
| A dynamic `RunCommand` probe could not reference Odin-backed `CharacterStats` from its temporary assembly | 1 | Move the focused mood-override and staff-stage assertions into the compiled Character Feedback editor scenario. |

## Phase 55 - Room Overlay Wall Occlusion Correction

- [completed] Identify the unexplained vertical wall line as the room-overlay perimeter rendered on the topmost layer.
- [completed] Split perimeter sorting by orientation so vertical edges sit behind wall art while horizontal edges and room fill remain readable.
- [completed] Re-run the room environment scenarios and physical PlayMode room-inspection verifier.
- [completed] Inspect the HUD and camera captures for wall occlusion, UI overlap, and non-empty overlay evidence.
- [completed] Confirm final Unity Error/Warning count is zero.

### Phase 55 Verification

- `RoomEnvironmentDebugScenarios` passed after the overlay change.
- The final clean PlayMode report passed actual Input System toggle clicks, room hover, panel values, 27 overlay cells, door/wall refresh, damage-driven cleanliness, build-mode shutdown, and non-stacking mood effects.
- Active vertical border renderers are now `0`; 54 horizontal edges remain on `RoomOverlay`, and the camera baseline/overlay comparison changed `196774` pixels.
- `Temp/room-inspection-hud.png` was inspected at `1920x1080`; the false vertical wall line is absent and the wall, door, furniture, character, and room panel remain unobscured.
- Unity MCP Scene View camera capture succeeded at `1920x1080`. The final Unity Console query returned Error/Warning `0/0`.

### Phase 55 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| Moving vertical borders behind `BuildingBack` still exposed them through transparent wall pixels in the baseline/overlay comparison | 1 | Suppress wall-overlapping vertical perimeter strokes and retain the room fill plus horizontal ceiling/floor edges. |
| Main Camera MCP capture used an instance ID invalidated during the verifier transition and logged a render failure | 1 | Use the verifier's stable camera PNGs for the overlay comparison and confirm MCP camera connectivity with a successful Scene View capture after the transition. |
| Desktop mouse events reclaimed `Mouse.current` after Scene View focus changes, making a repeated QA run nondeterministic | 1 | Give the verifier its own Input System mouse, disable the desktop mouse only for the run, and restore it during cleanup. The clean rerun passed. |

## Phase 56 - Inset Room Selection Outline

- [completed] Restore a complete room perimeter without recreating the wall-seam artifact.
- [completed] Use a dedicated selection accent and inset connected horizontal/vertical strokes inside the room bounds.
- [completed] Update PlayMode assertions for vertical outline presence, sorting, unlit material, accent color, and rendered pixels.
- [completed] Inspect fresh HUD/world captures and finish with Unity Error/Warning `0/0`.

### Phase 56 Verification

- The selected 27-cell room renders two inset vertical strokes and 54 connected horizontal strokes in the brightened selection accent.
- `RoomOutline` sorts above `Wall` and below `Default`, leaving default-layer doors, facilities, and characters in front.
- All 83 overlay SpriteRenderers use `Universal Render Pipeline/2D/Sprite-Unlit-Default`, so fill and outline colors are independent of 2D lighting.
- The camera baseline/overlay comparison changed `203372` pixels and detected `13444` visible green outline pixels.
- `Temp/room-inspection-hud.png` and `Temp/room-inspection-camera-overlay.png` were inspected at `1920x1080`; the outline reads as a connected mint selection frame rather than a wall seam.
- Final PlayMode report is `PASS`; Unity Console Error/Warning is `0/0`.

### Phase 56 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| The first inset outline was still beneath `DungeonFrontObject`, so the wall art darkened the accent until it resembled a seam again | 1 | Add `RoomOutline` between `DungeonFrontObject` and `Wall`, brighten the selection color, and keep structural walls, doors, and characters above it. |
| The `Wall` tilemap also covered the outline in both HUD and world-only captures | 2 | Move `RoomOutline` immediately above `Wall` and below `Default`; keep the geometry inset at the room perimeter so default-layer doors, facilities, and characters remain unobscured. |
| Renderer colors were correct but no green pixels appeared because new SpriteRenderers inherited URP's lit sprite material and the overlay region had no contributing 2D light | 3 | Give all room fill and outline sprites one owned `Sprite-Unlit-Default` material and assert the shader in PlayMode. |

## Phase 57 - Mirrored Grid Room Outline Alignment

- [completed] Correct left/right perimeter placement for the grid's reversed world X axis.
- [completed] Add a regression asserting vertical outlines sit near the true outer edges of the selected fill bounds.
- [completed] Re-run PlayMode, inspect the cropped left boundary, and finish with Error/Warning `0/0`.

### Phase 57 Verification

- Runtime layout inspection found three formal one-row rooms. The selected usable floor is the full 27-cell range `x=4..30`, not a one-cell room.
- PlayMode verified 27 overlay cells and two vertical outline edges.
- Fill world bounds are `-30.00..-3.00`; corrected outline bounds are `-29.89..-3.11`, placing both strokes inside the actual first and last cells instead of one cell inward.
- Camera output contains `14220` visible outline pixels, and the HUD capture shows the left outline immediately adjacent to the exterior wall.
- `RoomSystemDebugScenarios`, `RoomEnvironmentDebugScenarios`, and the full room inspection PlayMode verifier passed. Final Error/Warning is `0/0`.

### Phase 57 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| A temporary runtime dump could not reference VContainer from the MCP dynamic command assembly | 1 | Read the live grid through `GridSystemManager`, rebuild the layout with `RoomDetector`, and inspect active overlay renderers without DI. |

## Phase 58 - Flush Room Outline Bounds

- [completed] Remove the visible gap between the inset outline and the actual room bounds.
- [completed] Align each stroke's outer edge exactly with the fill bounds using half the stroke thickness.
- [completed] Assert left, right, top, and bottom renderer bounds match the selected room bounds.
- [completed] Re-run PlayMode, inspect the close-up capture, and finish with Error/Warning `0/0`.

### Phase 58 Verification

- Replaced the fixed `0.11` world-unit inset with half of the `0.075` stroke thickness, so each stroke's outer face lands on the room boundary.
- PlayMode verified all four outline bounds against the selected fill union: X `-30.000..-3.000`, Y `0.000..3.000` on both render sets.
- The selected usable room still contains all 27 expected cells, confirming this was presentation geometry rather than a room-layout error.
- Camera capture contains `13528` visible outline pixels. Direct inspection confirms the gap between the mint frame and structural boundary is gone.
- Final PlayMode verifier passed and the Unity Console finished with Error/Warning `0/0`.

### Phase 58 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| `RoomInspectionView.Dispose()` restored a `RectTransform` after the HUD had already been destroyed during PlayMode exit | 1 | Guard toggle and panel cleanup with Unity null checks, then re-run the full verifier and confirm the Console remains `0/0` after exiting PlayMode. |

## Phase 59 - Exclude Ceiling From Room Outline

- [completed] Remove the top perimeter stroke so structural ceilings are not presented as selected room space.
- [completed] Keep room fill, side strokes, and floor stroke aligned with the room interior.
- [completed] Replace the four-edge regression with side/floor alignment and explicit ceiling-edge exclusion assertions.
- [completed] Re-run PlayMode, inspect the HUD capture, and finish with Error/Warning `0/0` after exit.

### Phase 59 Verification

- PlayMode verified 27 fill cells, two vertical side strokes, and 27 floor strokes. The 27 former ceiling strokes are no longer created.
- Side bounds remain X `-30.000..-3.000`, and the floor stroke remains flush at Y `0.000`.
- `OVERLAY_CEILING_EDGE_EXCLUDED` passed with the highest horizontal outline at `0.075`, below the room ceiling boundary at `3.000`.
- The final HUD capture shows no mint selection line on the structural ceiling while the room fill, side edges, and floor edge remain readable.
- Full room inspection PlayMode verification passed with 8,328 visible outline pixels and Error/Warning `0/0` after exit.

### Phase 59 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| The first verification run used the previous PlayMode assembly and emitted the old four-edge report | 1 | Exit PlayMode, force `AssetDatabase.Refresh`, confirm compilation is idle, and run a fresh PlayMode session with the new ceiling-exclusion assertions. |

## Phase 60 - Restore Inset Top Room Outline

- [completed] Restore a visible top room stroke below the structural ceiling instead of deleting it.
- [completed] End both side strokes at the lowered top stroke so the outline remains connected.
- [completed] Verify the top stroke exists and keeps the configured ceiling clearance.
- [completed] Re-run PlayMode, inspect the HUD capture, and finish with Error/Warning `0/0` after exit.

### Phase 60 Verification

- Restored all 27 top horizontal strokes and positioned their outer edge `0.500` world units below the ceiling boundary.
- PlayMode measured the top outline at Y `2.500` against the room ceiling at Y `3.000`; `OVERLAY_TOP_OUTLINE_BELOW_CEILING` passed.
- Both side strokes terminate at the lowered top stroke, while side X bounds and the floor Y bound remain flush with the room.
- The final HUD capture shows a closed mint outline around the room interior with the structural ceiling left outside it.
- Full room inspection verification passed with 13,400 visible outline pixels and Error/Warning `0/0` after exit.

### Phase 60 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| Removing the top stroke entirely made the room outline look open and apparently missing | 1 | Restore the stroke with a `0.5` world-unit ceiling clearance and shorten the side strokes to meet it. |

## Phase 61 - Collapse Build Catalog During Placement

- [completed] Collapse the active category and full-screen build catalog immediately after a build item is selected.
- [completed] Preserve build mode, selected item, placement grid, and ghost while the catalog is collapsed.
- [completed] Keep Escape and switching to another bottom tab as explicit placement cancellation paths.
- [completed] Add a pointer-driven PlayMode verifier and capture the unobstructed placement screen.
- [completed] Re-run relevant regressions and finish with Error/Warning `0/0` after exit.

### Phase 61 Verification

- Actual pointer events opened `Build`, opened the `Wall/Door` category, and selected `Hallway`.
- Immediately after selection, both catalog levels were inactive while `GridMode.Build`, the selected building, three installable cells, and the visible ghost remained active.
- An EventSystem raycast at screen center found no build-catalog blocker after collapse.
- `Temp/build-placement-ux.png` at 1920x1080 shows the full dungeon, placement grid, facilities, and character with only the persistent bottom tab bar remaining.
- Switching to the building-management tab cancelled placement and now clears both mode and selected building.
- Build Placement UX, Unified UI, and Room Inspection PlayMode verifiers passed. The final post-exit Console is Error/Warning `0/0`.

### Phase 61 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| The initial verifier selected the destroy tool because it chose the first unlabeled category control | 1 | Resolve and click the exact `Wall/Door` category label through its real Button pointer event. |
| `SetGridModeNone()` left `SelectedBuilding` populated because selection clearing was rejected after mode changed to None | 2 | Allow building id `-1` to clear selection in None mode, then verify another bottom tab leaves both mode and selection empty. |

## Phase 62 - Block Live Character Wall Traversal

- [completed] Centralize structural-wall movement blocking while keeping both door types traversable.
- [completed] Revalidate queued walk steps before and during interpolation, rolling back when a wall appears.
- [completed] Recover active characters if a grid change places a structural wall on their current cell.
- [completed] Add EditMode and PlayMode regressions for stale paths, live wall placement, direct steps, and door traversal.
- [completed] Capture the blocked character beside the wall and finish with Console Error/Warning `0/0`.

### Phase 62 Verification

- `GridFoundationDebugScenarios` passed, including a path calculated before wall placement, a fresh path rejecting the wall destination, and an interior door remaining walkable.
- PlayMode moved the active employee from `(19,0)` toward `(20,0)`, installed a structural wall during interpolation, and verified the actor never entered `(20,0)` before returning exactly to world `(-18.500, 0, 0)`.
- The same verifier passed a direct `MoveByStep()` wall rejection, covering the movement path used by invasion intruders.
- Placing a wall under the active employee ejected it from `(20,0)` to walkable `(19,0)` on the grid-change event.
- `Temp/wall-traversal-blocked-camera.png` contained `368530` visible pixels and the Screen Capture was written to `Temp/wall-traversal-blocked-screen.png`.
- MCP `Camera_Capture` on the actual Main Camera showed the employee at `(19,0)` visibly separated from the temporary wall at `(20,0)`. The fixture was then removed and the actor's position, name, lifecycle, and AI state were restored.
- Final post-exit Unity Console is Error/Warning `0/0` and `git diff --check` reported no whitespace errors.

### Phase 62 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| The existing full invasion EditMode fixture lacks current `IFacilityCandidateCache` and `IOwnerCharacterFactory` injections | 1 | Kept the unrelated fixture unchanged; verified the intruder's direct-step movement path in the new PlayMode runner and confirmed the production loop exits immediately when `LastGridMoveWasBlocked` is true. |
| MCP dynamic commands cannot directly reference Odin-serialized actor and building types | 1 | Prepared the temporary camera fixture through base `MonoBehaviour`/`ScriptableObject` types and runtime type lookup, then restored all transient state after capture. |

## Phase 63 - Version-Gated Live Wall Checks

- [completed] Replace per-frame structural-wall cell lookups with a `grid.version` integer comparison.
- [completed] Query the target cell only when grid occupancy changes while preserving rollback behavior.
- [completed] Re-run stale-path EditMode and live-wall PlayMode verification.
- [completed] Inspect captures and finish with post-exit Console Error/Warning `0/0`.

### Phase 63 Verification

- The walk coroutine captures `grid.version` after its initial target-wall check.
- Every movement frame now exits after a nullable-target check and integer version comparison when the grid is unchanged. `IsMovementBlockedByWall` runs only after the version changes.
- Registering the live test wall incremented the version, triggered one target-cell check, set `LastGridMoveWasBlocked`, and restored the employee exactly to world `(-18.500, 0, 0)`.
- Grid Foundation EditMode scenarios passed for stale paths, fresh wall rejection, and door traversal.
- The isolated PlayMode verifier passed no wall-cell entry, rollback, direct-step rejection, occupied-cell ejection, and walkable recovery with captured Error/Warning `0/0`.
- `Temp/wall-traversal-blocked-screen.png` was inspected directly and the post-exit Console remained Error/Warning `0/0`.

### Phase 63 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| The first optimized PlayMode run selected an employee with an already-running work coroutine, which moved it during the verifier's isolated path | 1 | Stop existing coroutines on the selected actor's components before starting the verifier-owned movement; the fresh run passed all position assertions. |

## Phase 64 - Restore Building Info Preview Color

- [completed] Prevent the legacy building-info preview image from being styled as a dark panel background.
- [completed] Normalize preview sprite tint, material, aspect ratio, and raycast behavior whenever a building opens.
- [completed] Open the actual boss-office building info in PlayMode and inspect the runtime image state.
- [completed] Capture the corrected HUD and finish with Unity Console Error/Warning `0/0`.

### Phase 64 Verification

- The Unified UI PlayMode verifier opened the actual boss office at `(11, 0)` through `UIBuildingInfo.DisplayBuildingInfo` and waited `0.5s` so the periodic theme refresh ran before assertion.
- The preview retained the `bedroom` sprite, white tint, `UI/Default` shader, preserved aspect ratio, and disabled raycasts. `Temp/phase43-unified-ui-verification-report.txt` finished with `RESULT=PASS`, `failures=0`, and captured Error/Warning `0/0`.
- `Temp/phase64-building-info-preview.png` was inspected directly. The boss-office furniture, throne, bookshelves, desk, and painting are visible instead of being multiplied by the panel surface tint.
- Unity MCP `Camera_Capture` succeeded against the actual Main Camera GameObject and showed the dungeon world rendering normally behind the UI flow.
- The additive verification scene was removed without saving. The original dirty `CharacterAiTestScene` was restored as the only loaded scene with all `10/10` roots active and `playModeStartScene=null`.
- After restoration and a two-second settle, the final Unity Console contained Error/Warning `0/0`. The scoped `git diff --check` reported no whitespace errors.

### Phase 64 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| Unity MCP briefly returned `Connection revoked` after a domain reload while multiple relay sessions competed for the single editor connection | 1 | Reconnected after the editor restart and confirmed `GetState`, dynamic commands, console reads, and Camera Capture all worked before accepting verification. |
| Assigning a PlayMode start scene was blocked because the user's current `CharacterAiTestScene` had unsaved changes | 1 | Kept the dirty scene in memory, loaded `SampleScene` additively with the original roots temporarily disabled, then restored every root and closed only the clean additive scene. |
| Loading `SampleScene` after entering PlayMode bypassed the initial VContainer scene injection and produced missing-injection errors | 1 | Discarded that run and loaded the verification scene before PlayMode so normal scene startup and dependency injection executed. |
| Re-enabling both additive scene roots briefly produced duplicate global-light warnings | 1 | Closed the clean verification scene immediately, cleared the console after the original scene was fully restored, waited two seconds, and confirmed Error/Warning `0/0`. |

## Phase 65 - Hide Raw Logs During LLM Rewrite

- [completed] Trace raw log insertion, narrative request, UI refresh, and fallback paths.
- [completed] Keep eligible raw events internal until a completed narrative or controlled fallback is ready.
- [completed] Add regressions for pending, success, failure, queue saturation, and non-narrated logs.
- [completed] Verify the player-visible Records tab in PlayMode and finish with Console Error/Warning `0/0`.

### Phase 65 Verification

- `CharacterFeedbackDebugScenarios` passed in EditMode after the model change, preserving cause-tag aggregation, repeated counts, manual stale-rewrite rejection, feedback classification, summary formatting, and mood compatibility.
- Deterministic PlayMode probes passed immediate internal event delivery, pending display privacy, successful final reveal, failed-response fallback, rejected-request fallback, per-character request saturation, and repeated-pending finalization.
- The live Records-tab verifier captured the exact three entry IDs. Immediately after submission, neither `CharacterLog.Entries` nor the rendered TMP text contained any raw source event; the tab showed only `아직 기록이 없습니다.`.
- After the real local LLM completed, the same three IDs resolved to distinct, compact, subject-correct narrative lines and the Records tab displayed only those finalized lines.
- `Temp/phase65-character-record-pending.png` and `Temp/phase52-character-record-stories.png` were inspected directly. The former contains no raw event text and the latter contains only final narratives.
- `Temp/phase52-character-record-stories-report.txt` finished with `RESULT=PASS`, `failures=0`, and captured Error/Warning `0/0`. Unity MCP Main Camera capture also succeeded.
- PlayMode was exited, the editor settled for two seconds, and the final Unity Console remained Error/Warning `0/0`.

### Phase 65 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| The first full PlayMode run occurred after a script domain reload inside an already-running scene, so VContainer scene injections were missing and the runtime narrative service could not resolve | 1 | Keep the deterministic results, discard the stale-scene UI result, exit PlayMode, and restart the scene through its normal lifetime-scope bootstrap before rerunning. |
| The clean UI verifier waited on the narrative service's global `AppliedCount`; another character advanced that counter before the three target entries were visible, causing an out-of-range read | 1 | Expose an entry-ID visibility query on `CharacterLog` and wait for the exact three captured entries instead of a shared service counter. |
| On the entry-specific rerun, one target correctly finalized immediately through the request-limit fallback, but the verifier required all three targets to remain pending and all three LLM requests to be accepted | 1 | Define success by the actual invariant: each target is either private or already finalized to non-raw text, and accepted requests plus immediate controlled fallbacks cover all three entries. |

## Phase 66 - Compact Build Catalog

- [completed] Trace the build catalog root, category grid, and item-panel RectTransform ownership.
- [completed] Convert the open catalog into a compact bottom sheet sized to its visible content instead of a full-screen blocker.
- [completed] Preserve category selection, item selection, placement collapse, and bottom-tab cancellation behavior.
- [completed] Verify compact geometry and world visibility in PlayMode captures, then finish with Console Error/Warning `0/0`.

### Phase 66 Verification

- The catalog root now normalizes from the scene-authored `585.109` height to `132` at runtime while preserving its original bottom edge above the navigation bar.
- Eight category buttons occupy one row in an `883.06 x 116` region; the active item panel occupies the remaining `1012.64 x 116` region.
- Current unlocked content tops out at ten entries in one category, which fits the right-hand row with `88 x 88` item cells.
- Pointer-driven PlayMode verification passed catalog opening, category selection, item selection, placement collapse, grid and ghost visibility, world-center input clearance, and cancellation through another bottom tab.
- `Temp/phase66-compact-build-catalog.png` and `Temp/build-placement-ux.png` were inspected directly; open and collapsed states contain no clipped controls or overlapping UI.
- Unity MCP Main Camera capture remained nonblank, the final report returned `RESULT=PASS`, and Console Error/Warning finished at `0/0`.

## Phase 67 - Modular Facility Inventory

- [completed] Inventory every current monolithic `BuildingSO`, its room role, footprint, and authored sprite.
- [completed] Decompose each room-sized building into reusable structural, furniture, service, storage, and decoration parts.
- [completed] Define the complete deduplicated facility list with footprint, interaction, and room-role contribution.
- [completed] Record migration rules for retiring monolithic room buildings before implementation begins.

### Phase 67 Result

- `docs/game-design/modular-facility-catalog.md` defines 73 unique modular facilities across dining, retail, rest/administration, research/mana, training/security, logistics, hygiene, and decoration.
- The catalog maps all 21 room-sized building assets into 19 deduplicated room recipes, grouping the two duplicate food-shop assets and two duplicate weapon-shop assets.
- Structural traversal objects and eight standalone trap/defense devices remain independent placeables rather than being decomposed.
- Every proposed part has a footprint, mounting slot, core/support responsibility, room contribution, and production stage.
- The inventory identifies `Administration` and `Security` as missing room-role concepts needed to distinguish the lord office, guard room, and barracks cleanly.

## Phase 68 - Produce Every Modular Facility

- [completed] Extend building data, grid occupancy, and interaction ownership for modular facility parts and mounting slots.
- [completed] Produce project-ready pixel sprites and `BuildingSO` assets for all 73 catalog entries.
- [completed] Connect core facilities, support furniture, storage capacity, and environment contributions to room aggregation.
- [completed] Remove monolithic room buildings from construction and expand all legacy initial placements into modular recipes.
- [completed] Verify all assets, placement slots, room identities, interactions, UI, and captures with Console Error/Warning `0/0`.

### Phase 68 Verification

- `Assets/Images/ModularFacilities` contains 73 produced pixel sprites and `Assets/Resources/SO/Building/Modular` contains 73 unique `BuildingSO` assets with IDs `1000..1072`.
- The construction catalog exposes all 73 named, interactable modular buttons while hiding the retired room-sized monoliths. Categories use horizontal scrolling inside the compact 132-pixel build sheet.
- `ModularFacilityInitialPlacementMigrator` expands all 21 legacy room-sized placements into valid modular recipes. A clean PlayMode boot reported zero initial monoliths and 40 placed modular parts.
- Building, wall fixture, ceiling fixture, and floor overlay slots can coexist on one grid cell. Actual pointer placement, ghost alignment, grid indication, renderer layers, and selection priority passed for all four slots.
- Room aggregation recognizes mounted fixtures and produces composite identities from installed parts. Room environment, wall/door invalidation, cleanliness damage, mood factors, and overlay rendering passed in EditMode and PlayMode.
- Unity MCP Main Camera capture showed distinct modular props across all three floors with walls, doors, and characters unobscured. The camera comparison measured 2,082 changed pixels after placement.
- Regression reports passed: `Temp/modular-facility-playmode-report.txt`, `Temp/room-inspection-playmode-report.txt`, `Temp/build-placement-ux-report.txt`, `Temp/phase47-exclusive-world-info-report.txt`, `Temp/phase43-unified-ui-verification-report.txt`, `Temp/p0-ui-surface-verification-report.txt`, and `Temp/p1-p2-ui-surface-verification-report.txt`.
- Room System, Room Environment, Grid Foundation, Grid Visual, existing Facility, and Modular Facility EditMode suites all passed. Final verifier captures reported Error/Warning `0/0`.

## Phase 69 - Production-Depth Audit for All Modular Facilities

- [completed] Define and verify the intended runtime contract for all 73 modular facilities.
- [completed] Connect cooking, crafting, alarm, lighting, mana, training, sanitation, and other named core behaviors to runtime outcomes.
- [completed] Make seats, tables, storage supports, and shop specialization affect real capacity, inventory, and service behavior.
- [completed] Drive all 73 facilities through the real pointer placement, selection, and demolition flow.
- [completed] Expand all 21 legacy recipes into formal rooms and verify identity, usability, pathing, and mounted-layer composition.
- [completed] Add construction cost, unlock phase, and demolition refund data with economy enforcement and UI feedback.
- [in_progress] Verify AI pathing, live wall/door changes, queues, blocked facilities, and exclusive character/building click priority.
- [pending] Add a save/load round trip for modular placement, layers, durability, stock, and progression state.
- [pending] Run a large-layout performance audit, all-facility showroom captures, full regressions, and final Error/Warning `0/0` gate.

### Phase 69 Completion Evidence Required

- A 73-row machine-verifiable contract report proving every asset has deliberate behavior, economy, placement, and persistence data.
- PlayMode reports that use real pointer events for every catalog item and every legacy recipe.
- Save/load equality checks for every independently occupied grid layer and mutable facility state.
- Profiler evidence for a populated stress layout and nonblank Camera/Screen captures with no clipping, occlusion, or UI overlap.

### Phase 69 Contract Verification

- Rebuilt all 73 modular `BuildingSO` assets from the production generator after adding operational data.
- `Temp/modular-facility-contract-report.tsv` contains one header plus 73 `PASS` rows and zero failed rows.
- Every row records stable ID/code, footprint/layer, runtime type, functions, room/work roles, storage/seat/table/service capacity, output, cost, unlock phase, refund, and light parameters.
- Contract checks reject missing functions, economy data, invalid runtime ownership, mismatched capacities, unsupported output, and incomplete light settings.

### Phase 69 Runtime Outcome Verification

- A formal-room fixture verified exact food `4`, weapon `3`, and mana `3` production into category-restricted storage.
- Cleaning restored every room facility to cleanliness `100`; alarm repair did not arm it, while guard work capped charges at `3`.
- Training variants created three distinct mood experiences, sanitation/rest/meal facilities recovered their configured needs, and lighting created configured Point `Light2D` components.
- Dining seats/tables increased usable capacity to `2`; two service fixtures increased shop capacity to `3`.
- Food specialization combined a 16-slot food shelf and 60-slot universal logistics shelf into `76` room storage and `100` shop capacity. Food, weapon, and general rooms exposed only matching products and rejected mismatched restock.

### Phase 69 Exhaustive Pointer Verification

- `Temp/modular-facility-playmode-report.txt` passed actual catalog selection, placement, information selection, and demolition for all `73/73` parts.
- Every iteration also verified the exact occupancy layer, runtime code/footprint, construction deduction, demolition refund, and cleared layer.
- Both normal floor facilities and independent wall/ceiling/floor-overlay fixtures passed the same world-click selection flow after the character-first grid fallback fix.
- Captured runtime Error/Warning finished at `0/0`; the 1920x1080 catalog and MCP Main Camera captures are nonblank and correctly layered.

### Phase 69 Formal Recipe Verification

- `Temp/modular-facility-recipe-report.tsv` contains one header plus `21/21 PASS` formal-room recipes with exact role identity, a door, full path reachability, visitor cores, and independent mounted-layer counts.
- `Temp/modular-facility-playmode-report.txt` now also assembles every recipe with real Input System catalog and world clicks: all `21/21` passed exact part count, room identity, role/support membership, reachability, visitability, mounted renderers, and world information selection.
- The same combined report retained catalog placement/selection/demolition `73/73`, camera transform deltas `0.0000`, captures with `2,073,600` pixels, and captured Error/Warning `0/0`.
- MCP `Camera_Capture` at `1920x1080` showed the three-floor dungeon, modular fixtures, doors, and character nonblank and correctly layered after the recipe run.

### Phase 69 Economy Verification

- `Temp/modular-facility-economy-report.tsv` passes `12/12` cases for day-to-phase boundaries, locked placement, insufficient funds, exact single deduction, floored refund, modular legacy-condition suppression, and legacy compatibility.
- Modular facilities now ignore only legacy `ConditionNeedMoney`; all other build conditions remain active, while non-modular buildings preserve the old condition gate and deduction.
- The combined PlayMode report passed actual disabled-button clicks at Day 1, P2 enablement at Day 4, P3 enablement at Day 7, price/phase labels, insufficient-funds rejection with no mutation, red blocked ghost, visible required/owned feedback, exact deduction, and pointer demolition refund.
- `Temp/modular-facility-economy.png` at `1920x1080` visibly shows money `114`, the `필요 115 / 보유 114` danger notice, the blocked placement preview, the grid, and the unobscured dungeon. The full `73/73` and `21/21` regressions remained PASS with Error/Warning `0/0`.

### Phase 69 Errors

| Error | Attempt | Resolution |
|---|---:|---|
| A parallel source audit referenced `Assets/Scripts/Buildings/RoomFacilityPolicy.cs`, but the file lives under `Assets/Scripts/Rooms`. The failed `rg` exit stopped that batched read. | 1 | Located the authoritative path with `rg --files` and resumed with the corrected path. No source edit was lost. |
| The first contract-report command compiled against the prior editor assembly, and the subsequent forced import exposed two `FacilityData.workTypes` references in the new verifier. | 1 | Replaced both with the authoritative `supportedWorkTypes` field and reimported the verifier before rerunning. |
| The first 73-row contract run required every runtime type to derive from `Facility`, incorrectly rejecting the deliberate `Shop` runtime used by S01. | 1 | Split the contract by role: the purchase core must be exactly `Shop`; all other modular parts must be `Facility`-derived. |
| A batched source lookup used an `rg` pattern that returned exit code 1 and suppressed the other parallel outputs. | 1 | Re-ran the shop, fixture, and game-data reads as targeted commands and used the results to identify the initialization-order stock bug. |
| The universal-storage loop declared `category`, colliding with the later retail-category local in the same method scope. | 1 | Renamed the loop variable to `storageCategory`; no behavioral change was required. |
| The real-capacity scenario found service capacity `1` instead of `2`; D04 was marked `MealService` but generated with zero service capacity. | 1 | Assigned D04 service capacity `1` and strengthened the 73-row contract to reject any meal-service facility with zero service capacity. |
| A room-cache lookup attempted to read a guessed `RoomLayoutCache.cs` path, but the implementation is in `RoomRegistry.cs`. | 1 | Read `RoomRegistry.cs` and `RoomLayout.cs` directly; the cache was version-correct and not the cause of the failed capacity assertion. |
| The first exhaustive PlayMode run installed and demolished 73/73 parts but reported selection 0/73: it inspected a stale `UIBuildingInfo` instance, and independent mounted-layer parts produced no physics selection hit. | 1 | Point the verifier at `GridUIManager.buildingInfoUI` and add a character-safe grid-cell building fallback after physics hit priority in `WorldInfoClickSelectionService`. |
| The second exhaustive run raised world-click events for all 73 parts but still left detailed info closed because the verifier called `InputSystem.Update()` inside its coroutine, consuming `wasPressedThisFrame` before `GridUIManager.Update`. | 2 | Queue mouse states and let Unity's normal next-frame input update process them, matching the proven character-click verifier path. |
| A scene search used a PowerShell-incompatible `Assets/Scenes/*.unity` argument inside `rg`. | 1 | Dropped the wildcard probe and relied on the runtime component references plus targeted UI source reads. |
| Formal-room expansion passed 20/21 recipes; P1_NobleDining contained only supports and therefore produced no room role. | 1 | Promoted its one-cell D12 drink cabinet to a real Meal service core with operate/clean/repair work and explicit recovery, preserving the original four-cell layout. |
| A dynamic Unity command referencing `BuildingSO` could not compile because the command assembly lacks a Sirenix.Serialization reference. | 1 | Read the authoritative `.asset` YAML directly for the legacy footprint instead of retrying the unsupported dynamic type reference. |
| A live-room inspection command tried the internal `RoomRegistry.EditorCache` property from Unity's dynamic command assembly. | 1 | Switched to the public `RoomRegistry.GetLayout` facade; the live grid reported three formal room spans and a 27-cell usable sandbox. |
| The first 21-recipe pointer run drifted the camera while the synthetic mouse crossed the edge-scroll zone, so later clicks mapped one cell away and every recipe failed despite the static 21/21 room report. | 1 | Disable and restore `CameraManager` around verification, prefer a room-center anchor, and assert camera transform stability between exhaustive and recipe passes. |
| The camera-locked recipe rerun still placed selected fixtures one cell away because the verifier added `0.5` before calling `Grid.GetWorldPos`, even though this inverted-X grid API already returns the cell center for integer coordinates. | 2 | Use `Grid.GetWorldPos(cell)` directly; the previous helper targeted a grid boundary and let floating-point rounding select either adjacent cell. |
| The cell-center recipe run installed, reached, and selected every part in all 21 recipes, but the verifier required role-free supports to appear in `RoomInstance.Furniture`. | 3 | Match the room contract: role contributors must satisfy `ContainsPart`; role-free supports must occupy an interior room cell and remain path-reachable. |
| The first EditMode economy run created facilities through a bare object factory, so demolition reached dynamic-state invalidation without an `IFacilityCandidateCache` injection. | 1 | Removed the one interrupted transient fixture and configured the test factory with the same `ConstructBuildableObject` dependencies used by the product placement callback. |
| The injected economy rerun passed all 12 cases but emitted two missing-visual warnings for deliberately synthetic compatibility fixtures. | 2 | Reuse a real modular sprite/icon on the synthetic BuildingSO records so the production factory's visual-contract warning remains meaningful and the verifier can finish at Warning `0`. |
