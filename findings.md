# Findings & Decisions

## Requirements
- Audit all Unity C# scripts except scripts inside Editor directories.
- Produce markdown that lists functions per script and notes misplaced responsibilities and coupled functions.
- Refactor code to reduce coupling using VContainer dependency injection.
- Validate in Unity via MCP, including Play Mode and camera capture for UI visibility.
- Run an exhaustive over-separation audit for project-owned non-Editor scripts, distinguishing useful boundaries from needless one-off wrappers.
- New active goal: manually and exhaustively verify every implemented game feature in PlayMode before claiming "all features work in game."
- Full-game verification must distinguish automatic debug-scenario coverage from direct in-game/manual UI and runtime evidence.
- Latest user feedback adds a stricter UI-surface requirement: implemented features must be discoverable, actionable, and readable through player-facing UI, not merely runtime-verifiable through PlayMode probes.

## Research Findings
- `Packages/manifest.json` already includes VContainer `jp.hadashikick.vcontainer` pinned to `1.19.0`.
- Build settings enable `Assets/Scenes/SampleScene.unity`.
- Total non-Editor C# files under `Assets` is 911; project-owned non-Editor scripts under `Assets/Scripts` plus `Assets/DataManager.cs` is 298.
- Third-party/vendor non-Editor scripts include Behavior Designer, TextMesh Pro examples, DOTween, DamageNumbersPro, and Sirenix. These should be listed for audit coverage but not edited unless directly required.
- `Assets/Scripts/Infrastructure/DungeonRuntimeLifetimeScope.cs` already registers many services and manually injects many scene MonoBehaviours through repeated `sceneQuery.All<T>()` loops.
- Existing VContainer pattern uses `[Inject]` methods on MonoBehaviours plus `builder.Register(...).As<...>()` services and `IObjectResolver.Inject(...)` for dynamically-created objects.
- Generated `SCRIPT_FUNCTION_AUDIT.md` for project-owned scripts and `SCRIPT_FUNCTION_AUDIT_VENDOR.md` for vendor/plugin scripts.
- After refactor, direct `Input.`/`Screen.width`/`Screen.height` usage under project non-Editor scripts is isolated to `Assets/Scripts/Infrastructure/PlayerInputServices.cs`.
- Unity Play Mode validation ran in the currently open `Assets/Scenes/CharacterAiTestScene.unity` scene with 1 camera, 1 overlay canvas, 1 `DungeonRuntimeLifetimeScope`, and 1 `UIManager`.
- UI bounds validation in Play Mode found 41 active RectTransforms and 0 oversized active UI rects at 1920x1080.
- Scene-view camera capture succeeded and visually showed the dungeon scene with UI labels visible.
- Current project-owned non-Editor script count is 299 after adding `PlayerInputServices.cs`.
- `OVER_SEPARATION_AUDIT.md` did not exist before this dedicated pass.
- Generated `OVER_SEPARATION_AUDIT.md` from the full 299-file project-owned non-Editor scope.
- Final strict over-separation counts: 165 interfaces, 522 classes, 369 layer/suffix class candidates, 10 merge-candidate interfaces, 10 merge-candidate classes, 45 watch interfaces, and 29 watch classes.
- True merge candidates are narrower than the first broad scan: six `*SummaryRuntimeSource` interfaces/classes, three pure factory-alias services (`CharacterFeedbackBubbleService`, `CharacterSocialMemoryService`, `FacilityEvolutionStateService`), and `IUiTouchGuardService`/`UiTouchGuardService`.
- `*SummaryService` classes such as `CodexRecordSummaryService`, `OffenseTabSummaryService`, `OperationTabSummaryService`, `ResearchCraftingSummaryService`, `BuildingManagementSummaryService`, and `InvasionDefenseSummaryService` should stay because they build null-safe DTO snapshots and reduce UI/runtime coupling.
- The `*RuntimeProvider` family is a watch cluster rather than an immediate deletion target; prefer a shared cached scene-runtime helper/base if more providers are added.
- Phase 7 implemented every `merge-candidate` from `OVER_SEPARATION_AUDIT.md`.
- All six `*SummaryRuntimeSource` interfaces/classes were removed and their summary services now inject `IDungeonSceneComponentQuery` or `DungeonSceneRuntimeReferences` directly.
- Pure alias services were removed from the DI graph: character social memory and feedback bubbles now inject factories directly; facility evolution state access now injects `IFacilityEvolutionStateComponentFactory`.
- `IUiTouchGuardService`/`UiTouchGuardService` was folded into `IUiPopupService`/`UiPopupService`.
- Static verification found zero remaining references to removed merge-candidate type names in `Assets/Scripts/**/*.cs`.
- Unity MCP verification after Phase 7: script compile passed, Play Mode ran in `Assets/Scenes/CharacterAiTestScene.unity`, runtime console showed 0 errors/0 warnings, UI bounds check found 41 active RectTransforms with 0 oversized and 0 invalid rects at 1920x1080, and both Scene View and specific `Main Camera` captures succeeded.
- The Git worktree is broad and dirty: many Phase 7 files are untracked, so commit preparation must use an explicit manifest and path-limited staging/review.
- `docs/code-audit/phase-8-commit-scope.md` now defines the safe staging scope for the current refactor batch and leaves Phase 10/11 follow-up files to be added after they exist.
- Direct manual PlayMode verification found a real UI lifecycle bug: `UIBuildingInfo.DisplayBuildingInfo()` could be called while the inactive panel had not run `Awake()`, so cached image fields were null.
- `UIBuildingInfo` now lazy-initializes its cached components, activates itself before opening, settles inactive after close, and safely handles optional image/text references.
- Manual summary-tab verification covered operation, defense, offense, research/crafting, and codex/record tabs; all rendered non-empty text and closed cleanly.
- Facility evolution candidate/evolve verification ran in PlayMode through `FacilityEvolutionDebugScenarios.RunAll(log: true)` because the currently open scene has no scene `FacilityEvolutionRuntime`/panel instance.
- Character memory/bubble verification used public runtime APIs after service removal: existing actors had `CharacterSocialMemory` and `CharacterFeedbackBubble`, `GetSourceTrust()` returned 1, and `Show(Joy)` set the bubble state to Joy.
- Unity MCP camera/scene capture was used, but a specific Main Camera capture does not include ScreenSpaceOverlay UI. A PlayMode `ScreenCapture` image at `Temp/manual-flow-ui-overlay.png` was used to inspect actual overlay UI visibility.
- The watch-provider-cluster issue was repeated scene-query/cache plumbing, not the existence of named provider contracts.
- `CachedSceneRuntimeProvider<T>` now centralizes cached `IDungeonSceneComponentQuery.First<T>(includeInactive: true)` lookup and required-runtime error policy.
- Fourteen named runtime providers were refactored onto the helper while keeping their domain-facing interfaces intact.
- Provider smoke after Phase 10 confirmed `LocalLlmRuntimeProvider` resolves `LocalLlmRequestQueue`, `SocialReputationRuntimeProvider` resolves `SocialReputationRuntime`, optional absent runtimes return false, and required absent panel runtime providers throw clear `InvalidOperationException`s.
- `RefactorFollowupDebugScenarios.RunPlayModeSceneSmoke()` now provides a repeatable PlayMode smoke for the refactor-critical paths: building info, summary tabs, character memory/bubble APIs, cached runtime providers, and facility evolution.
- The automated smoke passed through Unity MCP with 0 console errors/warnings after execution.
- Previous Unity verification covered a DI/refactor-critical subset only; it does not prove that every implemented game feature is visible and usable in-game.
- Build settings currently enable only `Assets/Scenes/SampleScene.unity`.
- Available project scenes include `Assets/Scenes/SampleScene.unity` and `Assets/Scenes/CharacterAiTestScene.unity`.
- Prior manual PlayMode checks were performed in `Assets/Scenes/CharacterAiTestScene.unity`, so build-scene startup still requires separate verification.
- The implementation evidence matrix and debug scenario runner provide automatic logic coverage, but direct manual PlayMode evidence is still missing for many feature families.
- Current debug-scenario inventory contains 31 `*DebugScenarios.cs` files with 248 named scenarios, which should seed but not replace the manual QA matrix.
- Created `docs/qa/full-game-manual-verification.md` to track direct manual/in-game evidence separately from automatic debug coverage.
- The first full-game QA baseline in `CharacterAiTestScene` confirmed core startup/UI objects exist, but many feature-family runtimes are absent from that scene.
- `CharacterAiTestScene` has 0 instances for facility shop, blueprint research, synthesis, evolution, codex, invasion, defense status, operating-day settlement, event alert, recruitment, discontent, run variable, meta progression, and offense runtimes.
- Core PlayMode interactions verified in `CharacterAiTestScene`: pause/speed, construct panel toggle, and top tabs 1-9 opened without console errors.
- Visual QA found an active empty `UI/Quest` overlay covering the upper-right game view.
- `Quest` is a scene object in both `SampleScene` and `CharacterAiTestScene`, not a scripted runtime feature; it has no children/text and was active by default.
- Fixed the empty overlay by setting `Quest` inactive in both scene files and re-tested the loaded PlayMode instance with a second screen capture.
- `SampleScene` single-scene-style PlayMode verification was initially pending because the active `CharacterAiTestScene` was already dirty before the pass; the later completion audit used `SampleScene` as the active PlayMode scene while preserving/restoring the dirty test scene.
- Static scene scan indicates `SampleScene` is the likely full-feature verification target: it contains most feature-family runtime components that are absent from `CharacterAiTestScene`.
- Additive-isolated `SampleScene` PlayMode verification found that dynamically-created invasion intruders bypassed the scene injection pass. `CharacterActor` and `AbilityMove` on those generated objects could reach movement without `ICharacterAiSchedulingService` injection.
- `InvasionIntruderRuntimeFactory` now reuses the existing `ICharacterSpawnObjectFactory.Create/Inject` path so dynamically-created intruders follow the same VContainer injection contract as pooled spawned characters.
- Additive verification also exposed that `DungeonSceneComponentQuery.First(includeInactive: true)` could return inactive roots from the first loaded scene before active roots in the active scene. This caused active `SampleScene` providers to resolve the disabled `CharacterAiTestScene` grid during QA.
- `DungeonSceneComponentQuery` now enumerates the active scene first, and active roots before inactive roots, while still allowing inactive lookup when requested.
- After the scene-query fix, active `SampleScene` grid resolution returned size `32x3` with 84 walkable cells, and invasion entry resolution returned `(4, 0)`.
- `FullGameManualQaRuntimeProbe` was added as an Editor/PlayMode helper for the `SampleScene` intruder path. It reported `spawned=True`, `intruderAssigned=True`, `entryResolved=True`, `capturedErrors=0`, and after 5 seconds at `Time.timeScale=8`, `errors=<none>`.
- `SampleScene` core UI retest after the fixes verified pause/speed `1 -> 0 -> 1 -> 2`, construct panel open/close, tabs 1-9 with visible text, and 0 invalid/oversized rects.
- Visual evidence for the `SampleScene` retest includes Unity MCP camera capture plus `Temp/full-game-qa-samplescene-after-intruder-fix.png`, which shows HUD, bottom tabs, codex panel, and notice feed.
- After additive QA, `CharacterAiTestScene` roots were restored from `SessionState`, additive `SampleScene` was closed, and the editor returned to one loaded scene with all 10 roots active.
- `FullGameManualQaRuntimeProbe.RunSampleSceneFeatureFamilyProbe()` now exercises several active `SampleScene` feature-family runtimes in PlayMode.
- Feature-family probe verified these runtime paths with 0 captured errors: event alert creation/detail/choice callback, daily facility shop purchase and money delta, blueprint research queue/work/completion, facility synthesis completion, codex import/update, operating-day settlement report, run variable activation/selection, offense world-map panel/target selection, and meta run result/upgrade purchase.
- The same probe found remaining in-game gaps: `FacilityEvolutionRuntime` exists but found 0 candidates in the current `SampleScene` state, and `OffenseExpeditionRuntime` exists but found 0 available expedition members.
- Visual QA found alert detail text could be obscured by offense panels because alert UI lived under the main UI canvas at sorting order 300, while offense panels use sorting orders 420 and 430.
- `EventAlertRuntimeUI` now creates an override-sorting nested canvas at sorting order 480, so alert detail renders above offense panels and below the run-result canvas at sorting order 500.
- Visual evidence after the alert stacking fix includes `Temp/full-game-qa-samplescene-feature-family-alert-sorting-fix.png`; Unity console after probe/fix/capture/restore had 0 errors and 0 warnings.
- `FullGameManualQaRuntimeProbe.RunSampleSceneRemainingFeatureProbe()` now adds direct active-`SampleScene` PlayMode evidence for several previously open feature families.
- Remaining-feature direct evidence: AI had 4 actors/4 brains, 2 runnable actors, and 2 direct decisions selecting work; inventory restocked 5 shop items from a scene warehouse; invasion threat/candidate/director/report paths ran; regular customer recruitment succeeded; staff discontent produced `LocalRebellion` and isolate/suppress responses succeeded; offense expedition completion/rewards succeeded with a VContainer-injected temporary member.
- Remaining-feature visual evidence includes `Temp/full-game-qa-samplescene-remaining-feature-families.png` and Unity MCP camera capture; final console after probe/capture/restore had 0 errors and 0 warnings.
- Remaining gaps after the remaining-feature pass, before the later closing-gap probe: autonomous customer AI behavior, owner/priority command UI, duty/off-duty/fatigue/wander, local LLM/persona/dialogue/mood impulse, character model/feedback visual gameplay proof, detailed grid/build placement, triggerable defense facility effects, approved active-scene facility evolution, and full intrusion breakthrough movement.
- Active `SampleScene` had defense reporting/status services but no pre-placed triggerable `DefenseFacility` in the remaining-feature pass; the later closing-gap probe verified runtime placement and defense activation with a temporary `P1_SpikeTrap`.
- Active `SampleScene` facility evolution initially produced 2 candidates but 0 approved candidates; the later timed-gap probe QA-prepared the same scene candidate and verified `approvedAfterPrepare=True`, `evolved=True`.
- Broad legacy character-AI debug-world suites currently produce fixture-only DI errors when run directly in PlayMode because several temporary objects are not VContainer-injected. They should not be treated as direct active-scene proof until the fixtures are repaired.
- `CharacterAiFeatureQaRuntimeProbe.Run()` now adds direct `CharacterAiTestScene` PlayMode evidence using VContainer-injected temporary characters and 0 captured runtime errors.
- Character-AI probe evidence: injected customer can run AI and decide a purchase action with a reserved destination; owner and staff priority work target assignment succeeds; low staff condition blocks work and triggers off-duty; fake-success persona trait, mood impulse, and macro intent application succeeds; character visual renderer and feedback bubble component are present.
- Character-AI probe remaining partials from the first pass: scheduler-driven autonomous behavior did not process in the final reported tick, the complete off-duty return/visitor cycle did not finish, no real local LLM endpoint response was exercised, and temporary visible bubble state did not apply.
- Character-AI visual evidence includes `Temp/full-game-qa-character-ai-feature-probe.png` plus Unity MCP camera capture. UI bounds were 39 active rects, 0 invalid, 0 oversized, but the screenshot showed a large translucent yellow right-side overlay candidate.
- `FullGameManualQaRuntimeProbe.RunSampleSceneClosingGapProbe()` now provides direct active-`SampleScene` PlayMode evidence for several former gaps.
- Closing-gap grid/build/defense evidence: the active scene grid is `32x3`; `P1_SpikeTrap` was placed at `(21, 0)` by the runtime placement service, resolved as a `DefenseFacility`, triggered through `DefenseFacilityResolver.TriggerAt`, produced 1 activation report, dealt 14 damage, and was destroyed afterward with 0 captured errors.
- Closing-gap invasion evidence: a QA intruder had a direct reachable 31-step path to the owner, was moved to the owner position, initialized `InvasionIntruderRuntime`, and `ApplyFinalCombat(owner)` damaged the owner `1260 -> 1250`. This proves target reachability/final-combat application but not the full timed coroutine breakthrough from entry to owner.
- Closing-gap visual feedback evidence: `CharacterFeedbackBubble.Show(Joy)` succeeded in `SampleScene` and active text count changed `16 -> 17`; `CharacterDialogueRuntime` existed but `dialogueShown=False`, so dialogue bubble proof remains open.
- Closing-gap visual evidence includes Unity MCP camera capture and `Temp/full-game-qa-samplescene-closing-gap-probe.png`, which showed readable HUD, speed buttons, bottom tabs, and facility labels. The large yellow right-side overlay seen in `CharacterAiTestScene` was not present in this `SampleScene` capture.
- The `CharacterAiTestScene` yellow right-side overlay was classified as active `UI/Quest`: an `Image` with color `(1, 0.876, 0, 0.357)`, rect `(1488,454.6)-(1910,940)`, and `raycastTarget=True`.
- Both scene files serialize `Quest` inactive, but the currently open dirty `CharacterAiTestScene` editor state had `UI/Quest activeSelf=True`. Disabling the live object removed the overlay in the rerun screenshot.
- Final Character-AI visual rerun after the overlay fix had 38 active rects, 0 invalid, 0 oversized, 0 active yellow `Quest` graphics, clean console, and `Temp/full-game-qa-character-ai-feature-probe.png` showed the right-side view unobstructed.
- QA helper cleanup now kills DOTween tweens on temporary objects/components before destroying them, preventing fixture-only warnings from destroyed temporary character visuals.
- Public `SampleScene` build UI retest found real game-facing bugs: generated building buttons were not wired to `UIBuildingSelectButton.OnClick`, closing the construct tab hid UI without clearing Build mode/ghost, and reopening after an unlock did not refresh generated building buttons.
- Public build UI fixes were verified in PlayMode: 8 category buttons, 24 building buttons, 23 generated buttons, generated button selection id `1`, mode `Build`, ghost visible, buildable position `(21, 0)`, close reset to `GridMode.None`, ghost hidden, clean visual capture at `Temp/full-game-qa-samplescene-public-build-ui-clean-open.png`, and final console clean.
- DOTween warning cleanup needed both QA-fixture cleanup and runtime `CharacterVisual` cleanup. The runtime visual component now kills child `SpriteRenderer` tweens in `OnDisable`/`OnDestroy`; final compile/console check returned 0 errors and 0 warnings.
- `CharacterDialogueRuntime.ShowLine()` initially stayed silent for dynamically-created QA actors. The likely runtime cause was a stale null cached `CharacterActor` reference when required components awakened before `CharacterActor` was fully available.
- `CharacterDialogueRuntime` now lazily refreshes `CharacterActor`, `CharacterLog`, and `CharacterVisual` references before display/log/update work. Closing-gap rerun in `SampleScene` confirmed `dialogueShown=True`, `lastDialogue=QA visible line`, active text count `16 -> 18`, 0 captured errors, and final Unity console 0 errors/warnings.
- Strict public pointer placement initially partially passed but did not prove placement: public category and generated building button clicks succeeded, selected id `1`, found buildable `(21, 0)`, mouse warp succeeded, pointer was not over UI, and `TriggerPlaceBuilding()` ran without exception, but `placed=False` and building count stayed `97 -> 97`.
- Root cause: the public click path used Input System `InputAction`, but pointer coordinates came from legacy `Input.mousePosition` through `UnityPlayerInputReader`; under MCP/InputSystem injection, `Mouse.current.position` was correct while legacy input was stale.
- `UnityPlayerInputReader.MousePosition` now prefers `Mouse.current.position` when available, keeping the fix inside the input adapter and preserving the VContainer boundary.
- Strict public pointer placement retest passed in active `SampleScene` PlayMode: public construct/category/generated-button selection chose id `1`, pointer resolved `(21, 0)`, `before=97`, `after=98`, `placed=True`, Unity MCP camera capture succeeded, and final console was 0 errors/warnings after restore.
- `CharacterVisual` tween cleanup was broadened after a DOTween warning recurred from destroyed `SpriteRenderer` fade state. Cleanup now kills renderer, renderer GameObject/Transform, visual root, and owning object/transform DOTween targets; final compile/console check returned 0 errors and 0 warnings.
- Rerunning the `CharacterAiTestScene` probe with max-tick reporting confirmed scheduler-driven customer decision selection: `schedulerRegistered=5`, `schedulerProcessedMax=1`, `behaviorTicksMax=1`, `treeTicks=0->1`, `scheduledDecided=True`, `directFallback=False`, action `휴식`, destination `휴식방`.
- The same retest confirmed staff off-duty visitor/recovery return with QA-aged elapsed time: `offDutyVisitorDuringOffDuty=True`, `returnReadyAfterRecover=True`, `canStartAfterRecover=True`, and `returnedToWorkAfterRecover=True`.
- The `SampleScene` timed-gap probe closed two more direct runtime gaps: active-scene facility evolution approved after QA-prepared runtime evidence and evolved to `2성 전투 식당`, and timed invasion coroutine reached `state=Finished` with no captured errors.
- The same timed-gap probe initially attempted a real local LLM queue request as a low-priority bubble request; it was accepted but completed `Dropped` with `Bubble request expired in the LLM queue`, revealing queue pressure rather than endpoint absence.
- A corrected endpoint retest verified the real configured Local LLM path: localhost Ollama `/api/tags`, `/v1/models`, and a direct `/v1/chat/completions` POST succeeded outside Unity, then active `SampleScene` PlayMode returned `LOCAL-LLM-ENDPOINT ... status=Succeeded; success=True; content={\"qa\":\"pong\"}; error=<none>; queued=11; running=0; timeouts=0; dropped=0`.
- The 2026-07-13 long observation probe closed the remaining AI behavior timing gaps: a VContainer-injected customer planned, moved, and completed a facility visit (`visitCount=1->0`, `visitedBuildings=0->1`), and VContainer-injected staff completed idle wander plus real work fatigue (`sleep=100->97`, `mood=100->99`) with 0 captured errors and clean final console.
- The 2026-07-13 Core/Grid/UI completion probe closed the active-scene grid/UI gaps: `GridFoundationDebugScenarios.RunAll(false)=True`, active grid reachability `(1, 0)->(30, 2)` with 84 reachable cells, camera clamp, UI touch guard block/release, and building-info open/close all passed in `CharacterAiTestScene` PlayMode with 0 captured errors and clean visual capture.
- Before the final completion audit, remaining blockers were build-scene startup safety, public owner/priority/death/run-end flows, model/species/profile inspection, combat-report defense contribution, rebellion direct UI command interaction, and accepted-limitations review. The final completion audit closed those rows under Unity Editor PlayMode.
- Directly rerunning broad legacy debug scenario suites in PlayMode is still invalid completion evidence for several remaining rows because those suites create uninjected temporary worlds and throw VContainer-dependent fixture errors; completion-audit coverage should stay on active-scene or VContainer-injected QA probes.
- The final 2026-07-13 completion audit closed the remaining manual QA rows in active `SampleScene` PlayMode: owner selection/run-end/profile, combat report defense contribution, rebellion suppression through `OwnerCommandController`, UI bounds, screen capture, Unity MCP camera capture, captured errors 0, and final Unity console 0 errors/warnings.
- `SampleScene` was missing an `OwnerCommandController` scene object even though the runtime type and VContainer injection path existed. Adding the component to the scene made the direct controller command path verifiable in game.
- The final completion audit briefly exposed a delayed DOTween `SpriteRenderer` warning after owner/temporary visual cleanup. Storing the active `CharacterVisual` fade tween and killing it directly on disable/destroy removed the warning in the rerun.
- The full-game verification claim is now true for the implemented feature catalog under Unity Editor PlayMode. Scope caveats remain: no standalone player executable was built, and some scene-lacking cases use VContainer-injected QA actors or QA-prepared active-scene state rather than pre-placed base-scene fixtures.
- The UI surface audit found a separate gap: many implemented feature families are still exposed through generated summary text, EventAlert/NoticeFeed messages, hidden commands, or QA/runtime calls. `docs/qa/ui-feature-surface-gap-audit.md` now lists those UI gaps and closure criteria.
- `UITabManager` currently specializes only the staff tab by attaching `StaffWorkPriorityPanel`; most other top tabs still receive generated panels whose body text comes from `UITabContentTextProvider`.
- `UITabContentTextProvider` explicitly lists "UI connection needed" style content for shop, warehouse, operation, defense, offense, research, and codex tabs, so those screens should be treated as placeholder summaries rather than completed UI.
- The existing generated tab panel shape can support P0 UI without scene prefab churn: `UITabGeneratedPanelFactory` creates a tab GameObject with `Title` and `Body`, and factories can disable the placeholder `Body` text while attaching an injected MonoBehaviour panel.
- P0 surfaces can reuse current runtime APIs for real state changes: `DailyFacilityShopRuntime.TryPurchaseDailyOffer`, `Shop.RestockFrom`, `StockSupplyService.TryPurchaseDelivery`, `OperatingDayEndedEvent.Trigger`, `RegularCustomerRuntime.TryRecruit`, `MetaProgressionRuntime.TryPurchaseUpgrade`, and `BlueprintResearchRuntime.ApplyResearchWork`.
- Recruitment and meta progression do not have dedicated top-level tabs in the current 0-9 tab map; operation tab integration is the least disruptive player-facing route for this P0 pass.

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| Start with inventory before editing | Need to avoid broad, speculative refactors and understand existing project patterns. |
| Prefer local VContainer usage from the project/package over external assumptions | The installed package and existing code determine the safest DI style. |
| Treat vendor/plugin scripts as audit-listed but not refactor targets | Editing third-party packages would create high upgrade risk and is not needed for project coupling reduction. |
| Added `IPlayerInputReader` and `IWorldPointerRaycaster` adapters | Keeps Unity `Input`, `Screen`, `Physics2D.Raycast`, and pointer camera conversion behind injectable services. |
| Injected `UIManager` from `DungeonRuntimeLifetimeScope` | `UIManager` now has a VContainer dependency and must participate in the existing scene injection pass. |
| Do not refactor code during the over-separation audit pass unless evidence is clear and narrow | User asked for exhaustive investigation; generating evidence first avoids collapsing useful boundaries accidentally. |
| Write `OVER_SEPARATION_AUDIT.md` in ASCII English | A first Korean body generation was corrupted by the PowerShell command path encoding, so the report was regenerated in ASCII to keep the artifact readable. |
| Fold `IUiTouchGuardService` into `IUiPopupService` rather than injecting `UIManager` directly | This removes the extra touch-only wrapper while preserving the UI facade that keeps MonoBehaviours from depending directly on the large `UIManager`. |
| Use factories directly for component `GetOrAdd` aliases | The removed services had no policy beyond forwarding `GetOrAdd`; the factories already own that responsibility. |
| Phase 8+ treats commit scope as an explicit file manifest first | The worktree contains many untracked/dirty files, so broad staging would risk mixing unrelated local state into the refactor commit. |
| Watch provider cluster should be simplified through a small shared cache helper, not by deleting provider interfaces wholesale | Providers are still useful DI boundaries; the over-separation issue is repeated scene-query/cache plumbing. |
| Actual commit creation should wait until Phase 9-11 are complete | The user asked for all four follow-ups together; committing after direct manual verification, provider refactor, and automated scenario reinforcement keeps the unit coherent. |
| `UIBuildingInfo` should own its inactive-panel lifecycle | The scene stores the panel inactive, and callers can invoke `DisplayBuildingInfo()` directly; the component must therefore initialize lazily and activate itself rather than assuming `Awake()`/`Start()` already ran. |
| Facility evolution manual verification can use a PlayMode debug world when the active scene has no runtime instance | This still exercises candidate generation and actual evolution replacement during PlayMode without permanently mutating the scene. |
| Keep provider interfaces but share provider implementation plumbing | The provider names still express domain dependencies, while `CachedSceneRuntimeProvider<T>` removes repeated scene lookup/cache code. |
| The refactor follow-up smoke belongs in Editor code | It orchestrates PlayMode scene checks and menu access, while runtime scripts stay free of test-only orchestration. |
| Do not claim full-game verification from compile/smoke tests alone | The user's latest question is about in-game confirmation, so every feature needs PlayMode/manual evidence or an explicit limitation. |
| Treat `SampleScene` and `CharacterAiTestScene` separately in verification | The enabled build scene and the previously tested scene are not the same, so both need scene-role evidence. |
| Hide the current empty `Quest` placeholder by default | An active blank placeholder blocks the game view and has no runtime content; it should stay inactive until a real quest UI exists. |
| Dynamic intruders should use the existing character object factory for DI | Reusing `ICharacterSpawnObjectFactory` avoids duplicating resolver loops and keeps dynamically-created intruders consistent with pooled spawned characters. |
| Scene queries should prioritize the active scene before inactive additive scenes | Full-game QA can legitimately load a dirty scene and a build scene together; runtime providers should not resolve disabled roots from a non-active scene ahead of the active gameplay scene. |
| Keep `FullGameManualQaRuntimeProbe` in Editor code | It is verification orchestration, not runtime gameplay behavior, and lets PlayMode QA capture intruder spawn/movement errors without broadening production APIs. |
| Feature-family QA probes may directly call public scene runtimes when no direct UI surface exists | This still exercises the active `SampleScene` runtime and event paths, while the QA matrix distinguishes it from full manual UI interaction. |
| Alert detail should use its own runtime sorting canvas | Event alerts are global feedback/choice UI and must remain readable above feature panels such as offense map/expedition; run-result modals still stay above alerts. |
| Do not mix broad legacy debug-world suites into direct active-scene QA probes | They can create uninjected temporary objects in PlayMode and confuse direct scene evidence with fixture failures; run or fix them separately. |
| Use injected active-scene probes for character-AI feature evidence | The legacy debug suites still carry fixture DI assumptions, while injected probes better match the runtime VContainer path and produce cleaner evidence. |
| Completion-audit owner command coverage requires a real scene controller | Runtime rebellion suppression was already callable, but the direct owner command path should be verified through `OwnerCommandController` in `SampleScene`; the scene now contains that component. |
| Final "all features confirmed" claim is scoped to Unity Editor PlayMode | The user asked for PlayMode/MCP validation; a standalone build executable was not created or run during this pass. |
| UI surface completion needs its own closure rule | A feature is not UI-complete until a player can enter it through normal UI, see the relevant state, execute the action with visible controls, see success/failure/lock feedback, and pass PlayMode visual capture. |

## Issues Encountered
| Issue | Resolution |
|-------|------------|
| PowerShell script execution policy blocked plan initialization | Used a scoped `ExecutionPolicy Bypass` script invocation. |
| Detailed parsing of all non-Editor files was too slow because vendor code dominates the file count | Keep detailed coupling analysis for project-owned scripts and lightweight function listing for vendor/plugin scripts. |
| Camera capture by specific Play Mode camera ID failed | Default camera/scene-view capture succeeded; the failed log is an MCP capture-tool error, not a game runtime error. |
| Dynamic MCP validation could not reference VContainer/Sirenix assemblies directly | Avoided those direct references and validated scene/canvas state through UnityEngine types and console checks. |
| Session catchup reported unsynced context for the new over-separation request | Re-read planning files and checked `git diff --stat`; continuing with Phase 6. |
| Phase 7 Unity MCP scene-state command failed when directly referencing `DungeonRuntimeLifetimeScope`/`UIManager` generic types | Re-ran the check using `MonoBehaviour` scanning and type names to avoid missing VContainer/Sirenix dynamic-script references. |
| Phase 9 `UIBuildingInfo.DisplayBuildingInfo()` initially threw NullReference during manual open | Fixed lazy initialization/activation in `UIBuildingInfo`, recompiled, and reran the open/close flow successfully. |
| Unity MCP rejected private-field reflection and full `System.Reflection.BindingFlags` use in dynamic scripts | Verified character memory/bubble behavior through public runtime APIs instead. |
| Some provider smoke checks return false because `CharacterAiTestScene` does not contain those runtimes | This is expected for optional providers; required providers were checked for clear missing-runtime exceptions instead of forcing scene changes. |
| The automated smoke requires PlayMode | It touches live scene objects and VContainer-injected actors, so it fails fast outside PlayMode. |
| First `SampleScene` intruder pass hit missing `ICharacterAiSchedulingService` injection on generated intruders | Routed `InvasionIntruderRuntimeFactory` through `ICharacterSpawnObjectFactory.Create/Inject` and re-tested the intruder path with 0 captured runtime errors. |
| Additive `SampleScene` pass initially resolved disabled `CharacterAiTestScene` grid components | Updated `DungeonSceneComponentQuery` active-scene/root priority and re-tested `SampleScene` grid/entry resolution successfully. |
| `FullGameManualQaRuntimeProbe` initially looked up `Grid` as a Unity object | Changed it to use `GridSystemManager.grid` through the active-scene query. |
| Unity MCP dynamic scripts could not directly reference Sirenix-based `GameManager` during UI probing | Used active-scene `MonoBehaviour` type-name scan plus `SendMessage` for pause/speed actions. |
| Restoring the original scene roots briefly emitted URP duplicate global-light errors while both scenes were loaded | Completed the restore and closed additive `SampleScene`; final editor state was one loaded `CharacterAiTestScene` with 10/10 active roots. |
| Feature-family probe initially had C# definite-assignment compile errors in the offense report path | Initialized all `out`-reported variables before conditional calls and recompiled cleanly. |
| Event alert detail appeared under offense panels during visual QA | Added alert runtime canvas sorting and re-tested with screenshot plus canvas sorting evidence. |
| `SampleScene` has no available expedition members for OFF-02/OFF-03 | Later verified offense expedition completion/rewards with a VContainer-injected PlayMode member in active `SampleScene`. |
| `SampleScene` initially had no approved facility evolution candidates in the current state | Added a QA-prepared active-scene evolution path and reran the timed-gap probe; EVO-01 now has direct PlayMode evidence with `approvedAfterPrepare=True` and `evolved=True`. |
| Remaining-feature QA helper initially had compile errors | Fixed definite-assignment and API/property mismatches, refreshed assets, and recompiled before entering PlayMode. |
| Broad legacy debug-world suites produced false fixture errors inside the direct remaining-feature probe | Removed broad suite execution from that probe; final direct active-scene pass finished with 0 captured errors. |
| Active `SampleScene` had no pre-placed triggerable defense facility during the remaining-feature pass | Initially recorded this as a missing scene hookup; the later closing-gap probe verified runtime placement plus defense activation with an injected PlayMode building. |
| Broad legacy character-AI debug suites produced fixture DI errors | Replaced them for now with a VContainer-injected active-scene probe and left fixture repair as separate work. |
| Character-AI QA helpers referenced nonexistent `AIBrain.CurrentDestinationDebugLabel` | Switched the report to `bestAction.ReservedDestination` and recompiled cleanly. |
| Character-AI visual capture showed a large translucent yellow right-side overlay | Later classified it as the live `UI/Quest` object and disabled the overlay in the open scene state before rerunning the visual pass. |
| Unity MCP dynamic compile request initially resolved `CompilationPipeline` incorrectly | Retried the compile request with `UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation()` fully qualified. |
| Closing-gap QA helper captured an `out` parameter in LINQ lambdas | Copied the `out start` value into a local `startPosition`, recompiled cleanly, and reran the PlayMode probe. |
| Additive scene restore emitted transient URP duplicate global-light logs again | Confirmed roots restored and additive `SampleScene` closed; after clearing transient logs, final console had 0 errors and 0 warnings. |
| `CharacterAiTestScene` yellow overlay candidate was active `UI/Quest` in the open dirty editor state | Disabled the live `UI/Quest`, reran the Character-AI visual pass, and confirmed the overlay was gone with 0 active yellow `Quest` graphics. |
| QA fixture cleanup produced a DOTween warning when destroying temporary characters | Killed DOTween tweens before destroying temp objects/components and reran the affected probe with 0 warnings. |
| Public build UI category/generated-button flow did not actually enter reliable build selection, and close left build state active | Wired generated buttons to `UIBuildingSelectButton.OnClick`, reset the building controller on construct close, regenerated missing unlocked buttons on open, and verified the public flow in PlayMode. |
| DOTween warning recurred from runtime character visual fade tweens | Added `CharacterVisual` disable/destroy cleanup for child renderer tweens and confirmed final Unity console Error/Warning count was 0 after compile. |
| `CharacterDialogueRuntime.ShowLine()` stayed silent for dynamically-created QA actors | Added lazy reference recovery to `CharacterDialogueRuntime`, reran the closing-gap probe, and confirmed `dialogueShown=True`, `lastDialogue=QA visible line`, and 0 captured errors. |
| Strict public pointer placement did not prove actual placement on the first attempt | Found the click/pointer backend mismatch, fixed `UnityPlayerInputReader` to prefer Input System mouse position, and reran successfully with public UI selection plus `TriggerPlaceBuilding()` placing a building. |
| DOTween warning recurred after the public pointer probe | Broadened `CharacterVisual` DOTween cleanup target coverage and confirmed final console 0 errors/warnings after compile. |
| Character-AI probe hid scheduler work by reporting only the last manual tick | Updated both QA probes to record max scheduler/BehaviorTree tick counts and tree tick deltas; clean PlayMode rerun proved one scheduled decision without direct fallback. |
| Staff off-duty probe measured visitor state after restoring OnDuty | Captured visitor state while still OffDuty and then QA-aged the elapsed timer before public recovery/return checks. |
| Timed-gap QA helper initially failed to compile and then failed to affect room score | Fixed literal/iterator compile issues, added a QA-only grid occupant fixture fallback, reduced SeatDensity evidence to stay inside recipe bounds, and reran successfully. |
| Real local LLM request expired in the runtime queue on the first endpoint attempt | Diagnosed queue pressure from the low-priority bubble request; verified the Ollama-compatible endpoint directly and changed the PlayMode probe to isolate the queue plus use a high-priority persona request. Rerun succeeded with `status=Succeeded`, `content={\"qa\":\"pong\"}`, and final console 0. |
| `SampleScene` lacked an `OwnerCommandController` object for direct rebellion command verification | Added `OwnerCommandController` to `Assets/Scenes/SampleScene.unity`, reran the completion audit, and verified `controller=True`, `commandInvoked=True`, `destinationResolved=True`, and `suppressed=True`. |
| Final completion audit produced a delayed DOTween `SpriteRenderer` warning after visual cleanup | Stored the active fade tween in `CharacterVisual`, killed it directly on disable/destroy, recompiled, reran the completion audit, and confirmed final console 0 errors/warnings. |

## Resources
- `SCRIPT_FUNCTION_AUDIT.md`
- `SCRIPT_FUNCTION_AUDIT_VENDOR.md`
- `docs/qa/ui-feature-surface-gap-audit.md`

## P0 UI Surface Closure Findings (2026-07-13)

- `UITabManager` can retain generated-tab ownership while replacing individual tab bodies through a specialized panel factory; no scene-prefab rewrite was required.
- The operation tab is a workable shared surface for settlement, recruitment, and meta progression when each section has independent state cards, buttons, and feedback.
- Player-facing closure requires successful state transitions, not just button discovery. The warehouse verifier therefore creates delivery capacity before clicking the real delivery button, proving stock and money changes rather than only a capacity-failure response.
- Research verification must exercise queue lifecycle actions in a stable order. `cancel -> start -> progress` proves all three UI commands even when a single +10-second work action could complete the active task.
- Unity MCP camera capture renders the game camera but omits Screen Space Overlay canvases. The base camera was verified with `Unity_Camera_Capture`; P0 overlays were verified with Unity `ScreenCapture` files.
- Large white regions in camera screenshots are existing world facility sign backgrounds and reproduce in direct Main Camera capture; they are not P0 panel leakage or a capture timing regression.
- The final verifier window recorded 0 errors and 0 warnings. One existing info-level ability-cache message can appear while the temporary recruitment candidate is injected and does not affect the tested UI or runtime state.
- Closing the additive QA scene before re-enabling the original scene roots prevents transient URP duplicate-global-light errors during verifier cleanup.
- Restoring the dirty `CharacterAiTestScene` still re-emits four existing `LiberationSans SDF` missing-Korean-glyph warnings for `SelectedOwnerText`; these are outside the P0 panel and are tracked separately from the clean verifier window.

## P1/P2 UI Surface Mapping (2026-07-13)

- The remaining audit contains 14 P1 rows and 4 P2 rows.
- Existing reusable panels are present for facility synthesis, facility evolution, offense world map/expedition, and codex, but several are not reachable from the matching bottom tab or are summary-only.
- New UI should extend the established injected generated-tab pattern instead of adding permanent scene objects or bypassing runtime services.
- Staff tab work must preserve `StaffWorkPriorityPanel`; adding status/discontent/profile/AI controls cannot regress the already-connected priority grid.
- Defense and offense surfaces need both live state and historical result views; transient NoticeFeed text alone does not satisfy closure.
- Run variables have direct public UI-safe actions (`StartRun`, `ActivateOperationVariable`, `SelectInvasionVariable`) and state collections, so the operation tab can show start/current/remaining-day cards without a new runtime layer.
- Offense world map and expedition runtimes already expose `ShowWorldMap()` / `ShowExpeditionPanel()`. The missing work is a normal tab launcher, visible selected-target/active-run state, and durable reward history.
- `OffenseRewardRuntime.State` exists, while rewards are currently granted automatically at expedition completion. A player-facing surface should present grant history/results rather than invent a second claim path that changes reward semantics.
- Synthesis and evolution both expose public candidate/selection/execute APIs; their existing text panels can be supplemented by generated material/facility/recipe buttons in the facilities tab.
- Defense/invasion has public live runtimes for threat, active intruders, status effects, and combat reports. The defense tab can be a unified live monitor plus explicit QA-verifiable actions that invoke existing public runtime paths.
- `InvasionThreatRuntime` exposes threat, stage, safety, pending candidate, full snapshot, and `AddThreat`; `InvasionDirectorRuntime` exposes active intruders and `TrySpawnIntruder`; each intruder exposes state/focus/actor.
- `InvasionCombatReportRuntime` currently retains only `CurrentReport`. Closing the archive requirement needs a bounded report history added at the event owner rather than reconstructed from alerts.
- Existing `StaffWorkPriorityPanel` clears and owns the staff tab body. Staff P1 controls must be integrated into that panel or added as an internal mode; attaching a second body-owning panel would destroy the priority grid.
- Existing offense panels already contain target selection, recon upgrade, member selection, and expedition start buttons. A launcher/status surface can reuse them and separately expose automatic reward grant history.

## P1 Staff API Confirmation (2026-07-13)

- `StaffDiscontentRuntime` exposes state/rules, staff processing, work blocking and efficiency reads, rebellion target detection, `DispatchAutoSuppress`, `TryIsolateRebel`, `TryCalmStaff`, and suppressed-rebel resolution. Staff-management buttons can call real runtime behavior.
- `WorkDutyController` exposes `CurrentState`, `IsOffDuty`, `BeginOffDuty`, `SetDutyState`, recovery, and work-readiness decisions. Work/rest controls can mutate the actual duty state.
- `LocalLlmRequestQueue` exposes queued/running/drop/timeout counts, endpoint readiness, and last error. Persona/mood UI can show actual LLM service state while character actions use `CustomerPersonaRuntime`.
- Defense facility/status implementations are in `Assets/Scripts/Defense/DefenseFacilitySystem.cs` and `Assets/Scripts/Defense/DefenseStatusRuntimeService.cs`; the earlier shorter filenames were only conceptual names.
- `DefenseFacility.Trigger(...)` is already the authoritative activation path and records `nextTriggerTime`, but no remaining-cooldown property is exposed. Add a read-only cooldown value for the defense monitor.
- `OwnerCommandController` exposes only `SelectedActor`; its facility priority and rebellion suppression issue methods are private pointer handlers. Add thin public selection/issue methods that delegate to the same resolver/work APIs so staff-tab buttons use the real command path.
- `AiDirectorRuntime` exposes last request, last macro-goal application, last mood-impulse application, and last error, plus public macro/mood request methods. These are enough for a visible AI state/action card.
- `DefenseStatusRuntime` currently stores status kind/value/remaining/stacks in a private nested list with no read model. Add immutable snapshots and `ActiveStatuses` so the defense monitor can prove status application/expiry state.
- Owner command public wrappers should return `bool` plus a message and leave pointer-driven behavior delegating to them; this keeps mouse and staff-tab command behavior identical and testable.
- `P0FeatureSurfacePanel` is structurally a reusable generated-tab surface: it owns the body scroll, avoids the 210px notice feed, centralizes cards/actions/feedback/font handling, and dispatches by tab id. Extend it to tabs 1/6/7/9 and append P2 detail sections to tabs 3/5 instead of duplicating another large UI builder.
- P1/P2 verification will reuse the proven P0 editor lifecycle: persist original scene/root state, disable non-sample roots, open `SampleScene` additively, capture only the verifier window, exit PlayMode, close sample, then restore original roots.
- The verifier will prepare only prerequisites, following the P0 pattern of VContainer-injected temporary actors and existing scene/runtime data. It will click active `Button` instances by stable `P1Action_*`/`P2Action_*` names and read authoritative state after each click.
- Existing `FullGameManualQaRuntimeProbe.PrepareFacilityForEvolutionQa` fills star grade, required tokens/metrics, identity evidence, and a room fixture. The new verifier can invoke it reflectively, then track/remove any `QA Evolution Fixture` objects.
- The first verifier launch did not run because `Assembly-CSharp-Editor.dll` remained older than the new verifier source/meta. Force-import the specific Editor script and wait for the editor assembly timestamp/state before launching.
- No project asmdef/asmref encloses `Assets/Scripts/UI/Editor`; the verifier belongs in `Assembly-CSharp-Editor`. A clean script build cache request is the next recovery step.
- AssetDatabase recognizes the verifier as a `MonoScript` at the correct path, while `GetClass()` is null (which can also occur for static/non-MonoBehaviour top-level types). Check the editor assembly binary for the verifier type before assuming import failure.
- `CompilationPipeline.GetAssemblies()` lists the verifier in `Assembly-CSharp-Editor.sourceFiles`, but the built DLL contains no verifier type string. The likely state is a failed new compile with the previous successful DLL retained; inspect Bee compiler artifacts.
- Synthesis preparation can select placed facilities matching the visible recipe's `materialBuildings`; no synthetic synthesis service is required.
- Existing QA character construction adds `CharacterActor`, movement/work abilities, `AIBrain`, injects through the active `LifetimeScope`, initializes data/runtime state, and activates lifecycle. Reuse this exact sequence for a temporary staff/member.
- Defense verification can initialize a temporary `DefenseFacility` with the real `P1_SpikeTrap.asset`; the UI/action under test is activation/cooldown/status, while grid placement is already a separate verified P0/build flow.
- Staff rebellion preparation requires no reflection: mood at/below the rule's rebellion threshold makes `ProcessStaff` produce `LocalRebellion` immediately. A second QA guard worker can receive real auto/manual suppression targets.
- Most P1/P2 runtimes are active-scene components, so the already injected `IDungeonSceneComponentQuery` can resolve them without expanding the panel constructor with one provider per feature.
- Keep the existing 1,043-line panel maintainable by making it `partial`: the original file retains P0 sections/helpers, while a new P1/P2 partial owns facilities/defense/offense/codex and appended shop/economy sections.
- Stable UI action names will map one-to-one to audit rows (`P1Action_*`, `P2Action_*`) so the PlayMode verifier clicks the same visible buttons a player uses.
- Authoritative component paths/APIs are now mapped for all non-staff rows: synthesis/evolution, run variables, threat/director/intruders/combat report, world map/expedition/rewards, codex, event alerts, settlement, and shop.
- Production read-model additions remain narrowly scoped to defense active-status/cooldown, bounded combat/expedition/settlement history, and shop product/checkout snapshots. All actions continue through existing runtime methods.
- Run start-variable snapshots already provide `ToSummaryText`; active variables expose definition/title/detail and remaining days. The operation card can show exact generated variables without formatting hidden internals.
- Run-variable actions accept catalog IDs directly (`ActivateOperationVariable`, `SelectInvasionVariable`), and the catalog exposes category lists. Operation cards can therefore activate real variables with deterministic UI names.
- Defense activation UI needs the registered `IDefenseStatusRuntimeService` injected into the shared surface so effects and status snapshots follow the same service path as invasion combat.
- Facility evolution candidates expose recipe, approval, reason, proposal metadata, and rejected hints. The facilities tab can present why each placed-facility candidate is executable or blocked.
- Facility synthesis recipes expose material building definitions and a display name; selection uses actual placed `BuildableObject` instances and `TrySynthesizeSelected`.
- `OffenseExpeditionResult` already contains outcome, power/danger/time, member snapshots, reward summaries, granted reward results, and `ToDetailText()`. Retain recent completed results in `OffenseExpeditionRuntime` rather than duplicating reward history.
- `InvasionCombatReport` already retains the threat snapshot, final state, contributions, damaged facilities, observations, and activation/synergy logs. Retain resolved reports in `InvasionCombatReportRuntime`.
- Offense world-map target snapshots already provide full detail text and world-map state exposes recon level/selected target/known targets.
- Add expedition history only after reward grant fields are populated, immediately before the completed event. Add invasion report history only after `Resolve`; both retain at most 20 newest completed records.
- `Shop.GetStock()` omits product names and quantities. Add immutable `ShopProductSnapshot` values built from private `RemainStock` plus authoritative `CreatePricedStock`, and expose `RequiresStaffedCheckout`/`UsesSelfService` for checkout state.
- Shop detail can show current/maximum stock, waiting checkout count, serving-worker state, and `GetCheckoutCrimeChance(1)` without creating gameplay actions that alter purchase semantics.
- `OperatingDaySettlementRuntime` keeps current-day revenue/visits/restock failures/consumption/incidents privately and only exposes the latest report. Add read-only current-ledger values plus a 20-report history; operation-tab filters then provide a real UI state transition without fabricating economy mutations.
- Shop and settlement read models are now implemented over authoritative private state: displayed prices use checkout pricing and current ledger values remain read-only.
- Shop worker pricing is exactly `1.2x` when a worker is assigned and `1.0x` otherwise; product snapshots must call `CreatePricedStock` so displayed prices match checkout.
- Codex snapshots already contain category/id/title/discovered/info-source lines; event records contain importance/category/repeat count/choices plus full detail text. No model changes are needed for category, importance, entry, or detail selection.
- Tab 9 will use internal modes (`Codex`, `Reports`, `Events`) plus category/importance/entry selection. These clicks provide observable UI-state transitions over real stored data.
- Preserve the staff priority table as its default mode. Add a compact mode bar above the existing scroll and reuse the same body/content ownership for a `Staff Management` card mode; this avoids competing panels clearing each other.
- The existing priority-table status already distinguishes expedition/off-duty/working/idle and includes the actor brain debug summary. Management cards should deepen this state and add actions, not replace or duplicate the default table.
- Character profile data is available through `CharacterIdentity.Profile`: species plus trait list, while `CustomerPersonaRuntime` owns persona/request state and `AbilityWork` owns duty state. Staff management can present real identity/profile/AI state per selected worker.
- `CustomerPersonaData` includes trait/flavor, behavior multipliers, and preferred facility tags; `RequestPersonaIfNeeded` is the real queue action. `AiDirectorRuntime.RequestMoodImpulse` supplies the complementary mood action/state.
- Management actor selection itself changes the visible profile/status/AI detail. Duty toggle, discontent processing, owner facility-priority, rebellion suppression, persona request, and mood request then provide real runtime command transitions.
- `AbilityWork` exposes duty state mutation plus current priority facility/suppress targets, allowing UI and verifier to assert command results directly. Trait labels/details come from `CharacterTraitSO`.
- `StaffDiscontentRecord` exposes stage, last mood, low-mood/rebellion days, permanent/departed/rebellion/threat/isolation/suppression flags. Management cards can show full complaint and rebellion state.
- Existing staff UI factory supports two-axis scroll, fixed layout elements, buttons, images, text, and fonts; no extra UI service is needed for the management mode.
- `StaffWorkPriorityRowModel` provides actor/work/display name, and its builder already returns active workers. The management mode can reuse exactly the same staff population as the priority table.
- User clarified the intended core facility loop: walls and doors define a room, while furniture/facilities inside determine the room's identity. This is distinct from `FacilitySynthesisRuntime` recipe-combining facilities.
- The intended runtime is partially present: `RoomDetector` finds closed door/wall regions, `RoomInstance` unions interior facility roles, and `RoomProfileBuilder` accumulates fixture tags, scores, density metrics, defense signals, identity pressure, and conflicts. `FacilityEvolutionRuntime` consumes that profile.
- The current product mismatch is real: room identity is exposed mainly as per-facility evolution context, not as a first-class selectable room with visible boundaries, dominant identity, score breakdown, and live feedback after furniture placement. `RoomDetector` also scans only left/right neighbors, so it models horizontal floor segments rather than a general four-direction enclosure.
- Formal room verification exposed a concrete mapping defect: facilities marked `selfContainedRoom` were added to both a wall/door room and a fallback room, and the fallback overwrote `RoomLayout`'s facility mapping. Formal enclosure now takes precedence; standalone facilities retain the fallback only when no formal room contains them.

## 2026-07-13 - P1/P2 UI Surface Closure Findings

- The intended room model is now explicit: a formal room is enclosed by wall/door boundary records, and its `RoomRole` is the aggregate of facilities/furniture mapped inside those cells. Facility synthesis remains a separate mechanic below the room section.
- `RoomDetector` previously added self-contained fallback rooms even when the same building already belonged to a formal wall/door room. Skipping that fallback fixes room ownership and keeps room-derived role, visitability, and evolution context consistent.
- The first room UI copy mixed furniture-derived roles (`식사 + 훈련`) with facility-evolution identity pressure (`Rest 100%`) under the same “성향” label. The final surface uses room roles for `방 성향` and labels the latter separately as `진화 압력`.
- `ScreenCapture.CaptureScreenshot()` occasionally produced partially rendered DX12 frames while many tab captures ran in one coroutine. Waiting for end-of-frame and saving `CaptureScreenshotAsTexture()` synchronously removed the gray/blank frame artifacts.
- Final P1/P2 evidence proves 18/18 audit rows, formal room card click/selection, 211 active rects with 0 invalid/oversized, and 0 captured errors/warnings. The P0 regression also retained state changes and 0 captured errors/warnings.
- Unity MCP disconnect was deterministic: the Unity server allowed one connection, while a stale npm Codex relay held the slot and the active VS Code relay was denied. Revoking the stale registration fixed approval ownership, but restarting this conversation's relay left its transport closed; local Editor request runners completed the remaining PlayMode verification.

## 2026-07-13 - Build Placement Visibility Request

- The supplied game crop shows no readable cell overlay during construction and the dungeon wall/floor art is visibly blended with the backdrop, consistent with a renderer/tile tint alpha or shared fade being applied below 1.0.
- The placement overlay must be scoped to active build selection, remain readable over brick sprites, distinguish valid from invalid cells, and disappear cleanly after placement/cancel.
- The transparency fix must target the dungeon render path independently from ghost/placement validity tint so restoring wall opacity does not remove intentional ghost feedback.
- `GridUIManager.Start()` calls `HideGrid()`, but no runtime code calls `ShowGrid()` and the manager does not subscribe to `GridSystemManager.OnGridModeChanged`; therefore the existing construction overlay cannot appear through the public build flow.
- Both playable scenes already contain a referenced `WhiteBox` Tilemap using the existing one-cell white-outline sprite, but its serialized global Tilemap color alpha is `0`, so merely activating the GameObject would still render nothing.
- `GridGhostObject` has `PreviewAlphaHardLimit = 0.18f`, and `SampleScene` serializes `maxPreviewAlpha: 0.18`; `WithPreviewAlpha()` clamps every green/red ghost to that value. The supplied transparency is intentional code behavior but below a readable threshold.
- Main Camera captures with placement mode disabled confirmed that the normal dungeon wall/floor render path is opaque. The supplied translucent object is the placement ghost, so the fix belongs in `GridGhostObject` rather than dungeon art/material tint.
- `WhiteBox` already provides a suitable white-outline tile, but its authored cells are incomplete for the current logical grid. Rebuilding the overlay from all runtime grid cells produces bounds `(-31, 0)` with size `(32, 9)` for the current 32x3 logical grid.
- The final placement overlay marks valid anchors bright white (`0.82` alpha) and blocked anchors faint red (`0.10` alpha). In the tested door selection this produced 17 valid and 79 blocked anchors across all 96 logical cells.
- The placement ghost now blends validity color with white and clamps alpha to `0.78..0.90`; the existing scene's serialized `0.18` therefore becomes a readable `0.78` without scene YAML churn.
- Fast PlayMode re-entry exposed a Unity fake-null edge case: `gridOverlayTilemap ??=` retained a destroyed previous-session object because CLR null and Unity's overloaded null differed. Explicit Unity null reassignment fixes repeated entry.
- Final public-flow evidence: an active generated building button entered Build mode, displayed the complete overlay, a blocked click preserved the target and cleaned up mode/overlay/ghost, and a valid click replaced `복도` with a new `문` instance before the same cleanup.
- Final PlayMode console had Error 0 / Warning 0, camera captures showed the white grid plus readable green/red previews, and `GridFoundationDebugScenarios.RunAll(true)` passed.

## 2026-07-13 - UI Click-Through Root Cause and Fix

- `GridUIManager` opened legacy building information on every left click in None mode without checking UI coverage. The placed-building `OnMouseDown` event independently opened `BuildingSummaryInfo`, so guarding only one path would not fix the visible popup.
- The existing placement check used `EventSystem.current.IsPointerOverGameObject()`. In the active Input System UI module, a tested button coordinate produced three real UI raycast hits while this cached pointer-ID API returned `false`; this explains why the apparent guard still allowed click-through.
- `EventSystemUiPointerBlocker` now raycasts the current `IPlayerInputReader.MousePosition` and accepts only `GraphicRaycaster` results, preventing Physics raycasters/world colliders from being misclassified as UI.
- The blocker is shared through VContainer by legacy building info, placed-building and character info events, build placement, destruction, and owner right-click commands.
- PlayMode proof at a UI coordinate: three GraphicRaycaster hits, legacy info `false -> false`, summary info `false -> false`. At an unobstructed stair coordinate: zero UI hits and both information paths changed `false -> true`.
- Overlap proof found an active UI button at `(362, 720)` directly over hallway instance `-4261614`; both placement and destruction were blocked and the same hallway instance remained intact. Final console Error 0 / Warning 0 and grid foundation scenarios passed.
- Final screen capture `Temp/ui-click-through-blocked.png` shows the construction category UI open over the dungeon with no building-information popup after the blocked click path was invoked.

## 2026-07-13 - Placement Grid Wall Occlusion

- The missing grid behind walls was a deterministic sorting defect, not missing tile generation: live inspection showed `WhiteBox` at `Wall / order 1` and the wall tilemap at `Wall / order 100`.
- The active placement overlay contained 288 authored runtime tiles, and 102 of them spatially overlapped wall tiles. Moving the overlay to the later `Default` layer makes those existing lines visible instead of rebuilding more geometry.
- Placement visuals now have an explicit render contract: wall tilemaps remain on `Wall`, the grid is `Default / 100`, and the ghost is at least `Default / 200`.
- The ghost layer resolver raises legacy serialized layers that sort below `Default`; this fixes both current scenes without requiring scene YAML churn and still allows a deliberately later custom layer.
- Public-flow proof opened the construction tab and clicked building id 1, yielding Build mode, visible grid, and 17/79 valid/blocked logical cells.
- State-change proof used the Input System pointer path to replace a real hallway with a door, after which Build mode and the grid cleaned up. Grid foundation regression and final console both passed cleanly.

## 2026-07-13 - Placement Interaction and Alignment Follow-Up

- The supplied screenshots show that grid visibility is fixed, but the placement click can immediately surface building information for the newly created object.
- The visible white placeholder facilities do not share the expected floor baseline, and the green ghost footprint is both translucent and offset/scaled relative to the target grid cells.
- Acceptance requires one click to perform only placement, the resulting building to rest on the same floor baseline as its logical footprint, and the ghost to match the final visual bounds at an opaque readable tint.
- Source search found two independent information paths: `GridUIManager.Update()` opens legacy info whenever mode is `None` and the left button went down, while `BuildableObject.OnMouseDown()` drives the placed-building summary path.
- Successful placement sets grid mode to `None` in the placement path. Depending on same-frame script ordering, the still-active mouse-down can therefore be reinterpreted as a normal information click after the new building exists.
- Placed and ghost sizing both route through `GridBuildingTileTransformCalculator`, whose current `DefaultVerticalVisualInset` is `0.5`; existing regressions explicitly expect half-unit gaps at both the bottom and top of a three-unit cell.
- Sprite-only placed facilities render through runtime Tilemap tiles, while their clickable `BoxCollider2D` spans almost the full logical height. The tile transform currently renders only `y=0.5..2.5` inside a `y=0..3` footprint, which explains the visible floating gap.
- Ghost code mirrors that symmetric inset but additionally follows the snapped target with a `25` unscaled lerp. The transient lag means its bounds need not match the exact cell at click time even when its eventual scale is similar.
- Existing visual tests encode the floating behavior as expected (`minY=0.5`, `maxY=2.5`), so the correction must deliberately update both implementation and regressions to a floor-anchored contract.
- The controller already owns both successful placement and `OnPlacedBuildingClicked`, so a frame-scoped placement-consumption flag can suppress the summary path without introducing another service. `GridUIManager` can read the same controller flag to suppress its legacy info path.
- Tilemap visuals include the tile anchor's base `+0.5` cell-center offset, and `Grid.GetWorldPos()` already includes the same base `+0.5`. Combined with the present even-width parity offset, ghost horizontal centering is correct; the earlier half-cell mismatch hypothesis was rejected after reading the grid conversion code.
- Single-cell ghosts also use follow lerp instead of snapping. Removing that lag is required for the preview bounds at click time to equal the committed visual bounds.
- The user's green preview corresponds to the three-wide `Door` asset (`width=3`, `height=1`). Its authored Tile uses an identity transform over the native `3x3` sprite, while the ghost alone shrinks that sprite to `3x2`; this is the concrete size mismatch visible in the screenshot.
- Representative sprite-only facilities are one logical floor high (`height=1`, rendered over three world tiles), with Research Lab width 4 and Rest Room width 3; the alignment fix must cover both parity cases.
- Live inspection of the user's current PlayMode state found `GridMode.None`, hidden ghost, and an active `BuildingSummaryInfo` on `UI/BuildingInfoPanel`, confirming the immediate-info symptom in the actual scene rather than only by static reasoning.
- Active building Tilemaps use anchor `(0.5, 0.5)` and local Y `-0.5`; the transform calculator's explicit local-offset compensation is therefore part of the committed visual contract and must remain in the shared alignment calculation.
- Direct asset inspection confirmed the Door and placeholder facility sprites all use centered pivots and `16` pixels per unit. Door is a square 48-pixel sprite, so its native bounds are exactly `3x3`; ghost distortion comes from shrinking that native height to the current inset height while retaining width 3.
- The facility placeholders visibly include their white card background in the sprite itself, so the reported floating gap is geometric placement, not transparent padding in the source PNG.
- Applying the existing one-unit total inset only above the sprite-only visual gives `y=0..2` instead of the old floating `y=0.5..2.5`; authored Door tiles remain native `y=0..3`, and their ghosts use that same full footprint.
- Final implementation keeps the existing sprite-only visual height (`footprint height - symmetric inset total`) but moves that visual down to `minY=0`; authored-tile buildings bypass this inset for the ghost so Door remains its native full `3x3`. This avoids unnecessarily stretching placeholder facilities while matching the real Door tile.
- Edit-time regression evidence now proves repeated ghost sprites are alpha 1 and span their supplied visual bounds from the floor, while sprite-only facility Tile transforms have `minY=0` and retain their intended visual height.
- `BuildingSummaryInfo` keeps its component GameObject active while toggling a child `Panel`; verification must read `summary.UI.activeInHierarchy`, not the component GameObject, to avoid a false positive.
- The corrected live Door ghost at grid `(21,0)` exactly occupies the expected world footprint and no longer trails the pointer; its RGBA is `(0.55, 1.0, 0.55, 1.0)`.
- Deferring `GridMode.None` until `LateUpdate` is sufficient to keep every existing world-info path gated during the placement click; the controller's frame-consumed flag independently protects both legacy and summary building paths from script-order variation.
- Live runtime tiles confirm the floor correction is not limited to tests: all seven current sprite-only facility tile types have `minY=0`, with 4-wide cards sized `4x2` and 3-wide cards sized `3x2`.
- The suppression is frame-scoped rather than permanent: a subsequent deliberate Door `OnMouseDown` opens the building summary normally.
- `GridConstructTab.ToggleSelectButton` is intentionally a toggle, not an idempotent select. Automated public-flow verification must inspect the target panel's current state before calling it, or it can close an already-open category.
- The sprite-only Research path independently matches after correction: the live ghost bounds are `(-26,0)..(-22,2)`, exactly the `4x2` runtime Research Tile size and floor anchor.
- Final evidence covers both rendering families: authored Door ghost/final footprint `3x3` and sprite-only Research ghost/final footprint `4x2`, each opaque and floor-aligned.
- The exact placement click changed a real hallway to a Door while both info panels stayed closed in the initiating and following frames; a later intentional click still opened the summary.
- Invalid placement feedback uses the same corrected geometry: blocked Door at `(11,0)` measured `(-12,0)..(-9,3)` with opaque red RGBA `(1.0,0.55,0.55,1.0)`.

## 2026-07-13 - Opaque Ghost Color Follow-Up

- Alpha and validity tint were coupled: raising the ghost to alpha 1 left the existing green multiplication active, turning white placeholder cards into solid bright-green panels.
- Correct behavior is orthogonal: valid ghosts preserve the source sprite RGB at alpha 1, while blocked ghosts alone receive an opaque red warning tint.
- The source-color correction is implemented, but Unity MCP currently returns `Connection revoked` for both console and editor-state calls; the denial originates from `com.unity.ai.assistant`'s MCP approval gate, not from script compilation.
- The bridge denies a direct connection when the persisted matching connection record has `ValidationStatus.Rejected`; MCP settings and connection history are serialized into a project-scoped Unity `EditorPrefs` JSON value.
- The actual connection history is persisted in `Library/AI.MCP/connections-v2.asset`. It contains one accepted Windows Codex CLI identity, while newer VS Code Codex identities are recorded as capacity/plan denials; the bridge maps all denied transport states to the same generic `Connection revoked` tool error.
- Both relevant Codex processes are still alive: global CLI PID `54368` matches the accepted registry record, while VS Code app-server PID `50280` matches the newest denied record. The active Unity bridge itself is healthy at editor PID `49148`; this is an approval/connection-selection issue, not a dead editor bridge.
- Codex config already marks the required `unity_mcp` tools as approved, but Unity keeps a separate connection-level gate. A duplicate legacy `unity-mcp` entry points at an unrelated DigitalTwin project; the active tool namespace is `unity_mcp`, so that stale entry is not being edited as part of this fix.
- The active MCP server process is relay PID `47876`, parented by VS Code Codex PID `50280`; the accepted global CLI has no relay child. Unity's editor relay remains connected and repeatedly survives domain reloads, so the denied transport is specifically the VS Code direct MCP process.
- Unity's own Settings `Accept` handler converts Pending/Rejected/CapacityLimit history to `Accepted`; an already denied transport then needs a reconnect. A script domain reload also resets stale in-memory connection census state, matching the recovery semantics without clicking unrelated desktop windows.
- The current VS Code identity transitioned to `Accepted / Auto-approved` while diagnostics were running; the next MCP state call succeeded immediately. No manual registry modification or temporary Editor script was ultimately applied.
- Live Door verification through the visible public button measured exact source color `RGBA(1,1,1,1)`, `3x3` bounds, and floor `minY=0`. Main Camera capture shows the placement grid and Door art without the former green multiplication.
- Research color correction is correct (`RGBA(1,1,1,1)`), but the current public selection path exposes a separate live geometry regression: the active sprite remains at native `2x1.5` instead of the intended `4x2`, with `minY=0.25`.
- `GridGhostObject.SetSize(4,2)` computes renderer scale `(0.67,0.5)` from Research sprite bounds `6x4`, yet `SpriteRenderer.bounds` remains `2x1.5`. Parent/lossy scale is identity, so the remaining cause is the renderer's draw-mode geometry/size state retained from the prior Door sprite, not footprint math or hierarchy scaling.
- Live confirmation showed `drawMode=Tiled`, `renderer.size=3x3`, and Research sprite bounds `6x4`. Normalizing preview renderers to `SpriteDrawMode.Simple` makes footprint scaling use the complete source sprite instead of stale tiled geometry.
- Final Door-to-Research public UI swap now passes live: Door is source-white Simple `3x3` at floor zero, then Research becomes source-white Simple `4x2` at bounds `(-26,0)..(-22,2)`. Camera capture visibly shows the white Research card seated on the bottom floor, with no green tint or stale Door sizing.
- Blocked Research remains distinct and opaque after the draw-mode fix: `RGBA(1,0.55,0.55,1)`, Simple `4x2`, floor `minY=0`. Camera capture visibly shows the full-size red warning card over the occupied lower-floor area.

## 2026-07-13 - Blocked Placement Enforcement Follow-Up

- A red preview followed by successful placement proves the visual and commit paths do not currently share one authoritative placement verdict; color rendering itself is behaving as instructed.
- Both preview and commit call the same `GridBuildingPlacementService.CanPlaceBuilding`, and `TryPlaceBuilding` revalidates before mutation. The remaining mismatch must therefore be the position sampled by the input callback versus the position last rendered by the ghost, not two validator implementations.
- The world-pointer provider does not cache frame state: every read converts `Mouse.current.position` through the main camera. A stale provider value is therefore unlikely; the next check must reproduce the real InputAction press and capture target cell, validator verdict, and occupant changes around that exact event.
- Core bounds, occupancy, and support checks are deterministic and side-effect free. Building-specific conditions are the only remaining validator inputs that could return different results between preview and click, so Research's configured conditions must be included in the reproduction.
- Research has no serialized build conditions, so its verdict is fully deterministic. Separately, `PlaceBuilding` clears the selected building and exits placement even when `placedCount == 0`; a rejected red ghost therefore disappears and exposes whatever building was already underneath, which can visually masquerade as a successful placement.
- Live inspection of the user's current PlayMode state found exactly one existing Research at grid center `(23,2)` while a separate active Research ghost is red, opaque, and `4x2`. This is the exact state needed to compare instance IDs across the blocked click.
- The first instance comparison showed no creation or replacement, but the remote Input System pointer had reverted offscreen while the ghost retained its last rendered location. Exact-event verification must first queue and render the blocked pointer, then press at that same persisted screen coordinate.
- At exact screen `(806.4,642.4)` mapped to existing Research grid `(23,2)`, the controller verdict is false, the ghost is red, and Research count/instance remains `1 / -4337540` after the press probe. No new Research object is present; the reported effect is consistent with reveal/exit behavior rather than validator bypass.
- Directly invoking the real placement entry point at that same false/red target kept Research count and instance unchanged but changed `ghostHidden false -> true`. Root cause is now proven: failed placement exits selection and reveals the existing Research, creating a false success impression.
- After the fix, the same blocked target keeps the sole Research instance unchanged, leaves `ghostHidden=false`, and remains in `GridMode.Build`. Camera capture shows the red `4x2` preview still covering the existing Research instead of disappearing and faking success.
- The valid counterpart remains intact: public Research button selection plus target `(22,0)` reports `IsBuildableAt=true`, changes Research count `1 -> 2`, replaces the target occupant with a new id-16 instance, hides the ghost, and settles to `GridMode.None`.
- Successful placement opens no building-info panel (`0` active immediately and after settling). The final camera capture shows the new Research seated on the bottom floor with no placement ghost or grid overlay.
- Final edit-mode regressions passed (`GridFoundation=true`, `GridVisual=true`) and the Unity Error/Warning query returned 0 entries while stopped and idle.

## 2026-07-13 - Facility/Grid Alignment Follow-Up

- Research and Meat Restaurant are both serialized as logical `4x1` facilities. Their intended floor-aligned visual size remains `4x2` inside the three world-unit-high floor cell.
- Before the fix, both facilities occupied world x `-24.00..-20.00`, but their rendered sprite tiles covered `-23.50..-19.50` with center `-21.50`: an exact `+0.5` cell horizontal error.
- The even-width `BuildingBackWidth` Tilemap already carries local offset `(0.5,-0.5)`, while `GridBuildingTileTransformCalculator` independently added the even-width half-cell center correction. The Tilemap x offset was not passed into or subtracted by the calculator.
- The shared sprite-tile cache and transform calculation now use the full Tilemap local offset. After the fix, Research and Meat Restaurant footprint, overlay, and sprite bounds all equal `-24.00..-20.00`, with center `-22.00`.
- Vertical behavior is intentionally unchanged: each one-floor facility is anchored to the floor and uses two visible world units inside the three-unit logical floor height.
- Live construction-grid camera capture confirms both facility edges meet the same overlay grid lines. Final Grid Foundation and Grid Visual regressions passed with Unity Error/Warning 0.

## 2026-07-13 - Multi-Cell Availability Overlay Follow-Up

- The authoritative Research validator was correct: the live layout has 13 valid anchor positions. The visible bug was that `GridUIManager` colored only each valid anchor cell, even though a Research placement covers four cells from `anchor.x - 2` through `anchor.x + 1`.
- At valid anchor `(22,0)`, the real footprint is `(20..23,0)`. The old overlay left cells 20 and 21 blocked-colored even though the valid ghost necessarily covers them, creating the user's partial-valid appearance.
- The overlay now retains the same authoritative `IsBuildableAt(anchor)` predicate but expands every valid anchor through `BuildingSO.GetGridPosList(anchor)` and displays the union of all valid footprint cells.
- Live Research selection still has 13 valid anchors, but now reports 22 buildable coverage cells: 11 on y0, 6 on y1, and 5 on y2. This is the actual area that at least one valid Research placement can occupy.
- For valid target `(22,0)`, all four footprint cells `(20..23,0)` are white at alpha 0.82; cells 20 and 21 correctly remain invalid as anchors while still appearing as valid coverage. Existing Research cells `(21..24,2)` remain blocked-colored.
- Camera captures confirm a valid white Research ghost lies wholly within the white coverage and the occupied Research produces a full red ghost. A blocked click preserves count `1 -> 1`, the same occupant, and Build mode.
- Final Grid Foundation and Grid Visual regressions passed with Unity Error/Warning 0 and the editor stopped/idle.

## 2026-07-13 - Cell-Level Overlay Semantics Correction

- The user explicitly rejected selected-building footprint coverage as the grid meaning. The final contract is: grid colors describe individual cell installability; the ghost alone describes whether the complete selected building fits.
- Cell installability is now independent of selected width, height, and building conditions. A cell is white when it is inside horizontal padding, free in the selected placement layer, and either on y0 or supported by any occupant immediately below.
- Occupied cells, unsupported upper cells, and side-padding cells are blocked-colored. Destroy mode continues to highlight occupied building cells.
- The reported gap is a real single cell at `(20,2)`, world x `-20..-19`, between stair id 4 at x19 and Research id 16 at x21. It is now white because the cell itself is installable.
- Mana Storage remains a width-3 building. Centering it on `(20,2)` requires `(19,2),(20,2),(21,2)`, so its authoritative full-building verdict and ghost remain red while the middle grid cell stays white.
- Synchronized live verification measured left/gap/right colors as blocked/white/blocked, `fullBuildingValid=false`, and an opaque red `3x2` ghost centered at `(-19.5,4)`. Camera capture confirmed the same visible state.
- The temporary capture helper was removed. Final Grid Foundation and Grid Visual regressions passed with Unity Error/Warning 0 and the editor stopped/idle.

## 2026-07-13 - Top-Level and Construction Tab ID Collision

- The unexpected `직원 작업 우선순위` controls were not a debug overlay. `UITabManager` treated every descendant `UITab` with numeric id `2` as the top-level staff tab, but `BuildingCategory.Shop` also uses id `2`.
- The same unscoped routing attached `P0FeatureSurfacePanel` to construction category tabs whose ids overlapped top-level feature ids `1` and `3..7`.
- Top-level target lookup, generated-tab existence checks, and specialized-panel attachment now use only `UITab` objects whose direct parent is the `UITabManager` transform. Construction subtabs remain in the broader close/cleanup inventory.
- Fresh PlayMode inspection found exactly one top-level staff panel, eight top-level P0 panels, and zero specialized panels under `GridConstructTab` after all construction categories were generated.
- Clicking the visible construction `상점` button opened four building choices with zero staff-priority text. Clicking the visible `직원` top button opened the staff panel; mode switching worked, a priority value changed `1 -> 2`, and title/mode-bar overlap was false.
- The top-tab routing sweep passed ids `1..9`, the full P1/P2 UI verifier passed `18/18`, active UI bounds reported `invalid=0` and `oversized=0`, and captured/final Error/Warning counts were zero.

## 2026-07-13 - Interior Wall and Door Room Boundaries

- The interior-wall asset already existed as id `7`, but its icon was null, so the live Wall category rendered it as an unlabeled white button. Door id `1` was assigned to `BuildingCategory.None`, whose panel has no player-facing category button.
- The construction UI now exposes one `벽/문` group with labeled `복도`, `문`, and `내벽` tools. Door is routed there only at the menu layer; its serialized category remains unchanged so its passable Building-layer and collider/factory behavior are preserved.
- `RoomDetector.IsWall` previously treated every `category == Wall` object as a room boundary. Since live Hallway objects also use that legacy category, all hallway cells were excluded and the scene produced only self-contained facility pseudo rooms.
- Room detection now uses `BuildingData.IsStructuralWall` for real assets while retaining the category fallback for synthetic test occupants. The untouched scene immediately changed from no formal hallway rooms to three formal floor rooms.
- Clicking the visible `내벽` tool and placing one cell at `(20,0)` produced wall id `7`, changed formal room count `3 -> 4`, separated adjacent cells into room ids `1/2`, and made the wall cell non-walkable.
- Clicking the visible `문` tool placed a three-cell Door centered at `(26,0)`. Its left/right cells resolved to room ids `2/3`, both rooms referenced the same door, and the door remained walkable.
- Room roles are content-driven after boundary detection: the warehouse room reported `Storage`, the dining/shop/training floor reported `Dining, Shop, Training`, and the upper floor reported `Rest, Training, Research, Mana` from the facilities inside each boundary.
- Final evidence: boundary menu, wall ghost, placed wall, door ghost, and placed door captures; Room System, Grid Foundation, and Grid Visual scenarios passed; P1/P2 UI verification passed `18/18` with UI bounds valid and captured errors/warnings `0/0`.

## 2026-07-13 - Interior Wall Visual Follow-Up

- The reported invisibility was real. The selected wall ghost was active and `IsHidden=false`, but every ghost renderer had `sprite=null` because `Wall.asset.sprite` was unset.
- Placement created wall id `7`, but `DrawWall()` immediately removed its three explicit wall tiles because the exterior-wall calculator only retained walls generated in empty cells next to occupied cells. The generic floor-cap pass then left only `sprite3 1_7` at the top of the occupied cell.
- `Wall.asset` now uses its authored `sprite3 1_5` wall segment as both placement sprite and icon. Structural-wall ghosts use the complete `1x3` cell footprint rather than the inset facility footprint.
- `GridWallTileCalculator` now includes structural Building-layer wall cells in its authoritative tile set, and the floor-cap pass preserves the wall tile at the top of those cells.
- Fresh PlayMode measurement found one active ghost renderer with sprite `sprite3 1_5`, world bounds exactly `1x3`, alpha `1`, and sorting `Default/200`. The capture visibly shows the full-height wall preview.
- After public UI placement at `(20,0)`, the Wall tilemap contained `sprite3 1_5` at y `0,1,2`; the capture visibly shows the installed full-height wall. Rooms changed `3 -> 4`, adjacent room ids differed, and the wall remained non-walkable.
- Structural walls are excluded as sources for automatic exterior-wall expansion, so a standalone interior wall keeps its own three tiles without painting extra lower wall tiles into the cells on either side.
- Room System, Grid Foundation, and the new structural-wall Grid Visual regression passed. PlayMode and post-stop Unity Error/Warning counts were both zero.

## 2026-07-13 - One-Cell Door-on-Wall Correction

- The live Door asset is serialized as `3x1`, and `Door.Initialization()` independently forces a width-3 collider with an offset of two world units. This is the source of the three-cell tool behavior.
- Door and interior wall both occupy `GridLayer.Building`, so a Door cannot be layered onto an existing wall through the current one-occupant-per-layer grid model.
- The appropriate domain operation is a validated replacement: a Door target must contain exactly one structural wall occupant, then that wall is unregistered/deleted and the Door is registered at the same one-cell position.
- A Door still needs the Hallway occupant underneath to remain walkable. The placement service already knows how to create missing hallway support for non-wall buildings after the wall has been removed.
- The current room snapshot prioritizes Door classification over Wall classification and excludes a Door cell from interior flood fill, allowing both adjacent rooms to reference the same boundary Door after replacement.
- The live Door tool now exposes only structural-wall cells as white installable cells. A hallway-only cell is blocked-colored and produces an opaque red `1x3` ghost; the structural-wall cell is white and produces an opaque source-colored `1x3` ghost with `minY=0`.
- Public-path placement at `(20,0)` replaced wall id `7` with Door id `1`. The Door owns exactly one cell, has a `1x2.9` trigger collider, leaves zero lower wall tiles, and retains the authored Door tile in `BuildingBack`.
- Replacing the wall changed walkability from false to true while room count remained four. The cells on either side stayed in distinct rooms and both rooms referenced the same Door.
- A real blocked click at empty hallway cell `(21,0)` left the Building-layer occupant null, retained Build mode, and kept the red ghost visible. The successful click opened no building-info panel and normal frame cleanup hid the ghost and returned mode to None.

## 2026-07-13 - One-Cell Door Visual Legibility

- The old Door sprite was a broad `48x48` arch authored for a three-cell opening. Scaling it to one-third width made the installed one-cell Door read as a dark slit even though its placement state was correct.
- The existing castle `env_objects.png` sheet contains a closed barred door that can be isolated as a `32x64` bottom-pivoted sprite. It remains legible at the required one-cell width without the previous horizontal crushing.
- `env_objects_DoorClosed` is now shared by the Door build icon, placement ghost, and installed tile. The tile transform renders it at exact `1x3` world bounds and both preview and installed renderers remain fully opaque.
- The visual regression now requires the named sprite, exact source rect, shared sprite references, alpha `>= 0.99`, exact rendered bounds, and a minimum axis-scale ratio that rejects the old compressed arch.
- Fresh PlayMode verification used the visible construction, wall/door, interior-wall, and Door controls. At `(22,0)`, the preview measured `1x3`, placement replaced `Wall/BuildableObject` with `Door/Door`, and the resulting collider measured `1x2.9` with trigger enabled.
- The final camera captures show a readable closed gothic door both as the one-cell ghost and after placement. Room System, Grid Foundation, and Grid Visual scenarios all passed, and the final Unity Error/Warning counts were `0/0`.

## 2026-07-13 - Grounded Door and Interior-Wall Follow-Up

- The latest camera evidence shows that the sprite rectangle is aligned to the cell floor, but its lowest opaque door pixels begin above that floor, so the artwork still reads as floating.
- Wall-to-door replacement currently removes the structural wall occupant and its lower wall tiles. The Door remains functional, but the one-cell wall body that should frame the installed Door is no longer visible.
- `GridBuildingPlacementService.TryPlaceBuilding()` deletes the replaced wall visual after registering the Door, and `GridWallTileCalculator.GetWallTilePositions()` only preserves structural-wall occupants. Consequently the next wall redraw removes all three wall tiles from a Door cell.
- Door functionality must remain a real `Door` occupant for walkability and room-boundary sharing. Wall preservation therefore belongs in the rendering calculator, not in occupancy or placement validation.
- Pixel inspection of `env_objects_DoorClosed` found opaque bounds `x=0..31`, `y=16..63` inside its `32x64` sprite rect. The bottom 16 transparent pixels become exactly `0.75` world units at the current tile scale.
- Moving the sprite pivot from rect-bottom to normalized `y=0.25` grounds the first opaque row at world floor zero while leaving the opaque door `2.25` units tall. The preserved wall can then remain visible in the upper `0.75` of the three-unit cell.
- Installed Door tiles use the authored `Door.asset` tile matrix, while the ghost uses `GridGhostObject.SetSize()`. A sprite-import pivot change alone is not sufficient because each path applies its own positioning/scaling logic.
- `GridBuildingTileTransformCalculator` centers an entire sprite bound and compensates for its pivot. The Door correction therefore needs an explicit opaque-bottom visual offset shared by both the tile and ghost paths, or a tightly cropped sprite with corresponding transforms.
- The authored Door tile currently has scale `(0.125, 0.1875)` and Y translation `0`, which places the full transparent-inclusive `1x3` rectangle on the cell floor. A `-0.75` visual Y offset grounds its opaque pixels without changing scale or occupancy.
- The existing visual regression checks only total rendered size and cannot detect this transparent-bottom float. Phase 38 needs to assert the Door-specific offset and wall-tile composition as well.
- `DungeonStoryGridBuildingController` calls `NotifyGridObjectChanged()` after successful placement and is subscribed to redraw walls immediately. Once Door cells participate in wall visual calculation, their background wall tiles are restored in the same placement flow.
- The Door remains the only Building-layer occupant; preserving wall tiles is visual-only and does not reintroduce the one-occupant conflict that originally required wall-to-door replacement.
- Scene sorting order places `DungeonBackObject` before `Wall`, while the Door tile currently renders on `BuildingBack`. Restoring all three wall tiles without changing Door rendering would cover the Door completely.
- The placement ghost already renders on `Default`, which is after `Wall` and therefore visible. The installed Door needs its own front-of-wall renderer rather than a global tilemap sorting change that would affect every building.
- `Door` inherits the runtime-created `BuildableObject` and owns its collider GameObject. A child/local SpriteRenderer can provide a Door-only front visual and will share the Door object's lifecycle without changing grid occupancy.
- `GridBuildingObjectFactory` positions odd-width Door objects at the grid cell anchor and gives their collider a local center at `y=1.5`. A child renderer centered at local `x=0` with Y offset `-0.75` aligns to the existing one-cell object and grounds its opaque pixels at local/world floor zero.
- Editor tests compile in a separate assembly and cannot use the calculator's internal helper. The wall-composition regression should assert the resulting three Tilemap cells, which is also the more meaningful public behavior.
- `GridFoundationDebugScenarios` already establishes the correct editor fixture: a `GridBuildingFactory` callback injects no-op blueprint/world services plus facility cache and room policy into every runtime-created test building. Grid Visual should use the same pattern before exercising wall replacement.
- The reusable services (`FacilityCandidateCacheStore`, `RoomFacilityPolicyService`) are public, but the two no-op interface implementations are private nested classes. Grid Visual needs equivalent local no-op implementations with the four `IWorldInfoClickSelector` methods and the two `IBlueprintResearchWorkService` methods.
- Exact line audit confirmed the injection factory was accidentally attached to `VerifyStructuralWallKeepsFullHeightRender`; `VerifyDoorStaysGroundedInPreservedWall` still used the two-argument default placement service. The unique method-context patch now corrects the actual failing fixture.
- With the corrected fixture, the full Grid Visual suite passes the new grounded Door and preserved wall composition scenario.
- Fresh PlayMode exposes the visible `건축` button at screen center `(95.55, 30)`, matching the previously verified UI path and confirming the runtime UI initialized normally after the rendering changes.
- Dispatching a left-pointer click to that visible button opened seven active construction categories; `벽/문` is visible and interactable at `(90.27, 311.22)`.
- Pressing the visible `벽/문` category exposed `내벽` at `(665.61, 119.58)` and `문` at `(555.61, 119.58)` in the live selection surface.
- The public placement handler reads a private injected `IWorldPointerPositionProvider` and guards through `IUiPointerBlocker`. Both interfaces are minimal and can be temporarily replaced with fixed-world/no-UI test providers while still invoking the production `TriggerPlaceBuilding()` path.
- Unity MCP disallows reflection in dynamic commands. The cleaner PlayMode route is to move `Mouse.current` to `Camera.WorldToScreenPoint(targetWorld)`, letting the existing input reader, pointer provider, ghost presenter, and UI blocker all observe the same real screen position.
- Cursor warp reported the intended target screen point `(864,348)`, but immediate and delayed `TriggerPlaceBuilding()` calls left `(22,0)` empty in Build mode. The next check must distinguish pointer-coordinate propagation from EventSystem UI blocking rather than assuming the warp reached every input consumer.
- Live diagnostics showed `Mouse.current=(0,0)`, production snapped world `(-35,-3.94)`, invalid cell `(35,-1)`, and `overUi=false`; `(22,0)` remained buildable. The warp was not persistent across MCP commands, while UI blocking was not the cause.
- The active scene's VContainer `LifetimeScope` exposes all original controller dependencies through `Container.Resolve<T>()`. Recalling the public `Construct(...)` method with those dependencies plus a fixed pointer/no-UI blocker avoids forbidden reflection and preserves the production placement service.
- Unity MCP's dynamic command compiler omits VContainer references, but project Editor assemblies already use VContainer and frame-driven QA runners. A compiled Door visual verifier is the supported place to hold a stable pointer across frames and capture synchronized runtime evidence.
- Unity reports some script compiler errors through console entries typed `Log`, so an Error-only query can return zero despite a failed Editor assembly. Compile gates must inspect `All` entries or editor compilation state plus compiler-message text.
- The first compiled PlayMode run successfully pressed `건축`, `벽/문`, `내벽`, and `문`. The Door ghost measured rect `1x3`, transparent-inclusive min `-0.75`, opaque bottom `0.00`, and max `2.25`, proving the grounding correction works visually.
- After successful wall placement, the controller resets mode to `None` but leaves the selection buttons visible. `SelectBuildingById()` ignores clicks in mode `None`, so the Door sequence must explicitly re-enter construction mode even when its button remains on screen.
- Live post-run state retained the fixed pointer and snapped to target floor `(-21.5,0)`, but no wall occupant existed. The verifier supplied raw pointer `cellFloor + 1.5`; placement uses raw `grid.GetXY(pointer)` while ghost positioning uses the separately snapped floor, so the center offset likely targeted the next grid row during placement.
- `Grid.GetXY()` floors world Y and divides by the three-unit cell height, so raw `y=1.5` still resolves to row `0`; the pointer coordinate was valid. No additional runtime exception was logged, narrowing the failed wall placement to a controller guard or selected-state transition rather than placement-service failure.
- Interior walls are draggable. The first public placement input only calls `TryBeginDragSelection`; a second input completes the one-cell drag and reaches `GridBuildingPlacementService`. The first verifier run stopped after the begin-drag phase, explaining `wallBuildable=true` with no occupant and mode still `Build`.
- The construction tab is a visual toggle independent of the placement mode reset. After wall placement, one `건축` press can close the still-open category panel; reopening requires a second press before `벽/문` becomes visible again.
- The final public PlayMode flow passed through the visible `건축 -> 벽/문 -> 내벽`, then reopened `건축 -> 벽/문 -> 문` before placing at `(22,0)`.
- Wall rendering stayed present for all three vertical cells before the Door preview, during the preview, and after installation (`3/3/3`). The Door remains the sole Building-layer occupant and the wall is visual composition only.
- Both ghost and installed Door measured transparent-inclusive bounds `minY=-0.75`, `maxY=2.25`, while their first opaque pixels measured exactly at floor `Y=0`. The visible Door height is therefore `2.25`, leaving the upper `0.75` of interior wall visible.
- The placed result retained the named `env_objects_DoorClosed` sprite, the Door-only front sorting, a one-cell `1x2.9` trigger collider, and reset placement mode to `None`.
- Final camera evidence is stored at `Temp/phase38-door-ghost-camera.png` and `Temp/phase38-door-placed-camera.png`. Room System, Grid Foundation, and Grid Visual all passed afterward with no Console warnings or errors.

## 2026-07-14 - Original Door Artwork Restoration

- User feedback correctly identified that Phase 37 replaced the established `Assets/Images/Using/Door.png` art rather than limiting the change to placement and wall composition.
- The original `Door` sprite is `48x48` at 16 PPU with authored world bounds `3x3`, a bottom-center pivot `(24,0)`, and no transparent rows below its opaque pixels.
- The one-cell requirement belongs to grid occupancy and the `1x2.9` trigger collider. The original decorative arch can remain `3x3`, centered over that one-cell boundary, without reverting placement behavior to a three-cell footprint.
- Because the original sprite is already bottom-pivoted and grounded, its visual Y offset is `0`; the previous `-0.75` correction is specific to the replacement `env_objects_DoorClosed` slice and must not be reused.
- `GridPlacementGhostPresenter` previously derived preview size directly from the one-cell occupancy, which would crush a restored `3x3` Door. A visual-size override keeps occupancy and artwork dimensions independent without changing generic building behavior.
- The installed Door's child front renderer can use the same `3x3` source bounds at scale one. It remains centered on the one-cell Door object while the preserved wall tilemap stays behind it.
- The fresh public PlayMode flow passed with `ghostSprite=Door` and `doorSprite=Door`; both measured exact world bounds `3x3`, opaque bottom `Y=0`, and top `Y=3`.
- Wall tile counts remained `3/3/3` before preview, during preview, and after replacement. The placed object still had a `1x2.9` trigger collider and reset the grid to `None` mode.
- Visual inspection of the Phase 39 camera captures confirms the original wide arch and surrounding masonry are restored without the black gap or narrow replacement-door silhouette.
- Final related regressions all pass after restoration, and the original Door tile plus `env_objects.png.meta` now match their pre-replacement asset state exactly.

## 2026-07-14 - Dungeon Door and Interior-Wall Door Separation

- The previous implementation conflated two domain objects: the existing ID 1 `Door` is the authored `3x3` dungeon entrance, while the player-built wall opening must be a separate one-cell door.
- `BuildingSO.IsDoor` is already inheritance-based and is suitable for shared room-boundary behavior. A new `IsInteriorDoor` discriminator is required for wall replacement and construction-overlay rules.
- Existing wall replacement, installable-cell overlay, wall-tile preservation, and `벽/문` routing currently use generic `IsDoor` or exact `typeof(Door)` checks. Those responsibilities must move to the new interior-door subtype only.
- The original dungeon Door should return to width 3 and `unlocked=false`; a new free ID 8 asset can be the unlocked one-cell `InteriorDoor` shown in construction UI.
- The previously isolated `env_objects_DoorClosed` art remains a viable dedicated interior-door sprite: its `32x64` source has 16 transparent bottom pixels, so the one-cell `1x3` rect needs the measured `-0.75` visual offset while the dungeon Door remains untouched.
- The generic `Door` class now provides only shared boundary/pass-through behavior. `InteriorDoor` owns the extra front renderer required to appear above the retained wall tilemap.
- Dungeon Door placement no longer enters the one-wall replacement branch; only `IsInteriorDoor` can replace one structural wall cell. Both remain `IsDoor` for room-boundary detection.
- Runtime inspection in `CharacterAiTestScene` found several exact `Door` instances carrying stale `DoorVisual` children from the previously conflated implementation. Those children were not serialized in the scene asset, but persisted through the editor's scene-reuse PlayMode workflow.
- Exact `Door` activation now removes only the two legacy interior-visual child names. `InteriorDoor` instances are excluded and retain their dedicated `InteriorDoorVisual` renderer.
- The reused Character AI test scene does not automatically redraw the dungeon Door tilemap after code/domain changes. A dedicated exact-`Door` front renderer guarantees the original `3x3` entrance remains visible without sharing any sprite or layout with `InteriorDoor`.
- The dungeon entrance and interior-wall door require opposite hallway composition rules. `InteriorDoor` keeps the underlying wall/hallway body for a one-cell opening, while the exact dungeon `Door` must clear all hallway visuals across its three-cell footprint because the authored arch opening is transparent.
- Initial placement and reused-scene startup are separate rendering paths. Clearing the dungeon hallway in `DrawBuilding()` fixes fresh placement, while the exact-door exception in `SynchronizeHallwayVisuals()` fixes PlayMode sessions that preserve the existing grid and tilemaps.
- Runtime-created `SpriteRenderer` components default to `Sprite-Lit-Default` under this URP 2D setup. The exterior entrance can be outside the lit dungeon area, so both door renderers use the shared `DoorSpriteUnlit` material to preserve authored pixel colors.

## 2026-07-15 - Building Info Preview Tint

- `DungeonUiThemeRuntime` styles the building panel while it is hidden. At that time the preview `Image` has no sprite, so generic panel styling previously classified it as a background and assigned the dark `Surface` color.
- Assigning the building sprite later did not reset `Image.color`; Unity therefore multiplied the authored icon by the stale dark panel tint, which made the boss-office preview appear almost black.
- The stable fix has two guards: the theme skips the designated preview image, and `UIBuildingInfo` normalizes sprite tint, material, type, aspect preservation, and raycast behavior every time a building opens.
- The production preview state remains correct after the periodic theme refresh, so the result does not depend on update order or the panel's previous hidden state.

## 2026-07-15 - Raw Character Log Exposure

- `CharacterLog.AddLog()` writes the raw cause tag directly into the same `log` list exposed by `Entries`, then invokes `OnLogAdded`, and only afterward submits the LLM narrative request.
- `CharacterSummeryInfo` subscribes to `OnLogAdded` and immediately rebuilds the Records tab from `CharacterLog.Entries`, so every narratable raw line is player-visible while the asynchronous rewrite is pending.
- Hiding the issue only in `CharacterSummeryInfo` would leave other future consumers vulnerable. Visibility must be represented by the log model so every player-facing reader receives only finalized entries while diagnostic source data remains available internally.
- `OnNarrativeResult` currently returns immediately on an unsuccessful LLM result, leaving the raw line permanently visible. Request rejection, unavailable runtime, and the per-character three-request limit also return `false` without producing a player-safe fallback.
- The required lifecycle is atomic from the display model's perspective: eligible raw event stays internal while pending, then exactly one finalized narrative is published; every terminal failure publishes the existing controlled local fallback instead of the raw source text.
- `OnLogAdded` is also consumed by character feedback symbols, dialogue generation, and social-rumor interpretation. Those systems need the immediate structured/raw event, so delaying that event would create unrelated AI regressions.
- The display boundary should therefore live in `CharacterLog.Entries`: keep `OnLogAdded` immediate for internal consumers, exclude pending entry IDs from player-facing enumeration, and reveal an entry only when `TryUpdateDisplayLine` atomically installs its final text.
- A second identical event changes the same entry to an aggregated `xN` line and makes the first LLM response stale. That update must explicitly remove the pending-display flag so the entry cannot remain hidden forever.
- `CharacterLogNarrativePlayModeVerifier` currently names raw exposure `IMMEDIATE_VISIBLE` and requires all three source events to appear in the Records text before the LLM completes. The regression encoded the unwanted behavior and must be inverted to require zero pending raw visibility.
- The corrected clean PlayMode run passed exact entry-ID tracking: immediately after three requests, `Entries` contained none of their raw lines and the open Records tab displayed only the empty-state message. After completion, the same IDs resolved to three distinct finalized narratives.
- A request that cannot enter the LLM queue because the per-character pending limit is already reached is not a pending failure. It must finalize immediately through the controlled fallback, and the visibility invariant is that any visible value differs from both `DisplayLine` and `OriginalMessage`.

## 2026-07-15 - Build Catalog Empty Space

- The open build catalog currently stretches from near the top of the viewport to the bottom navigation even when it contains only an eight-button category grid and one small item button.
- Category and item content are generated by `GridConstructTab`; the oversized dark area is therefore a layout-container ownership problem rather than missing catalog content.
- `GridConstructTab.MakeSelectButton()` and `GridConstructButtonFactory` only instantiate category buttons and category panels. Neither assigns anchors, preferred height, layout groups, or a content fitter to the catalog root.
- Category panels are parented directly to the full `GridConstructTab`, while category buttons use its first child. Any full-stretch dimensions serialized in the scene/prefabs therefore become both the visible dark surface and the UI raycast footprint regardless of actual content.
- Both `SampleScene` and `CharacterAiTestScene` serialize `ConstructTab` at `1919.7 x 585.109`, bottom-center anchored with center Y `352.135`. This intentionally starts just above the bottom navigation but extends roughly 585 canvas pixels upward even when most of that surface is empty.
- The visible category grid has four rows, so the current fixed height is substantially larger than the controls it owns. A compact bottom-sheet height should be derived from the category rows plus padding and the selected-category item strip, with a viewport-relative cap for smaller screens.
- The category container is `383.49 x 401.64` with a `GridLayoutGroup` using `180 x 100` cells in two columns and four rows. Its useful height is therefore about 402 canvas pixels, leaving roughly 183 pixels of the current 585-pixel catalog root unrelated to category content.

## 2026-07-15 - Compact Build Catalog Result

- A two-row `236`-unit draft passed geometry checks but still consumed an unnecessary row on low-height displays.
- The final layout uses all eight category slots horizontally and reserves the right side for the selected category's buildings.
- The largest current unlocked category contains ten items. With `88`-unit cells and `6`-unit gaps, all ten fit in the final right-hand row at the reference canvas width.
- Runtime normalization is preferable to scene serialization here because both gameplay scenes share the oversized authored values and one scene contains unrelated user changes.
- Preserving the root's calculated bottom edge while changing its height keeps the compact sheet attached to the bottom navigation without altering authored anchors.

## 2026-07-15 - Monolithic Building Inventory

- The project currently contains 36 `BuildingSO` assets: 19 unlocked and 17 locked or legacy entries.
- Room-sized facility families are food service, retail, research, rest, training/barracks, mana storage, warehouse, toilet/washroom, weapon shop, general shop, and the legacy lord bedroom.
- Most facility and defense presentation assets are single `96 x 64` placeholder sprites rather than authored modular sprite sheets. The lord bedroom is a single `144 x 48` finished-room sprite.
- Structural and traversal objects (`Hallway`, `Wall`, `InteriorDoor`, dungeon `Door`, `Stair`, `Elevator`) should remain separate from the facility decomposition.
- Traps and standalone defense devices are already semantically independent placeables; only compound defense rooms such as guard room and barracks need decomposition.
- `P1_Toilet` and `P1_Washroom` currently reuse the rest-room sprite, confirming they need dedicated fixtures rather than another room-sized variant.
- Existing room roles map to Meal, Purchase, Rest, Training, Research, Mana, Logistics, Toilet, and Hygiene. New parts should contribute one or more of these roles while room classification remains an aggregate of enclosed contents.
- The design document already names the core evidence model: tables and chairs for Dining/Seat, meat counter for Dining/Meat, training dummy for Training, weapon rack for Combat, chandelier for Luxury, basin for Hygiene, toilet fixture for Toilet/Sanitation, and decorative props for lineage pressure.
- Existing reusable visual material is limited. `WeaponStore.png` contains recognizable weapon-rack, chest, and counter/display elements, while `EmployeeBedroom.png` is only another text placeholder.
- `env_objects.png` is the main candidate sprite sheet for generic castle furniture and decorations; its imported slices need inventory before deciding which new sprites require original production.
- Visual inspection showed `env_objects.png` contains structural arches, gates, columns, masonry, and doors, not an interior-furniture library.
- Legacy `foodstore.png`, `Hospital.png`, `restroom.png`, and `EmployeeBedroom.png` are text cards rather than usable object art. `hamburger.png` is only a tiny item-like icon.
- The weapon-store sheet is imported as two full-width 96 x 48 layers rather than individual props, so reuse requires reslicing or redrawing its rack, chest, and counter components.
- Runtime already supports modular evidence through `BuildingSO.Evolution`: each placed part can contribute tags, scores, and metrics independently while `RoomProfile` aggregates every fixture in the enclosed room.
- Existing normalized metrics explicitly support `SeatCount`, `TableCount`, `LargeTableCount`, `CounterCount`, and `PrivateSeatCount`; the modular list should populate these rather than inventing parallel counters.
- Core interaction behavior still lives on `Shop` or `Facility`. The migration therefore needs a core-versus-support distinction so decorative chairs and shelves do not each behave like an entire legacy service building.
- Available identity terms cover Dining, Cooking, Meat, Luxury, Service, Training, Combat, Defense, Storage, Hygiene, Rest, Research, Mana, Logistics, Brutal, Noble, Quiet, Security, Ritual, and related lineage signals. The facility catalog can use the existing vocabulary.
- The final catalog contains 73 unique IDs: 12 dining, 8 retail, 10 rest/administration, 10 research/mana, 10 training/security, 7 logistics, 7 hygiene, and 9 environment/decor parts.
- Nineteen decomposition recipes cover all 21 room-sized assets because the legacy food-shop pair and weapon-shop pair are duplicate families.
- Wall, ceiling, and overlay mounting must be implemented before all catalog items can coexist ergonomically; using the single current Building occupancy slot would make decorations compete with usable floor fixtures.

## 2026-07-15 - Modular Runtime Contract Audit

- `GridCell` already supports one occupant per `GridLayer`, so additional mount layers can provide independent occupancy without replacing the core grid container.
- Serialized values `Hallway=0`, `Building=1`, and `Character=2` must remain unchanged. New mount layers must be appended and `GetTopOccupant` must use explicit priority rather than raw enum order.
- The current tilemap sprite renderer sends every non-hallway sprite to the same BACK tilemap cell. Distinct mount layers would overwrite each other's visual unless mounted props use separate tilemaps or per-object `SpriteRenderer`s.
- `GridBuildingObjectFactory` already creates a GameObject for every placed building but does not render sprites on it. It is the natural point to add mounted-prop SpriteRenderers while leaving existing floor-building tile rendering intact.
- `RoomProfileBuilder` already scans all grid occupants inside a room, so support props on new layers will contribute evolution data automatically.
- `RoomEnvironmentEvaluator` currently filters fixtures to `GridLayer.Building`; it must include all modular mount layers so decorations affect beauty, hygiene, damage, and crowding.
- `RoomDetector` intentionally uses only role-bearing fixtures for room roles. Support props can keep `FacilityRole.None` while still contributing through `Evolution` data.
- Final runtime evidence distinguishes the objects directly: dungeon `Door` bounds are `3x3` with zero wall and hallway tiles under its three occupied cells; installed `InteriorDoor` bounds are `1x3` over one occupied wall cell with all three wall tiles retained.

## 2026-07-14 - Dungeon Door Ceiling and Traversal Sorting

- The three ceiling tiles above the dungeon Door already existed as `floor` tiles on the `Wall` Tilemap. They looked absent because `DungeonDoorVisual` rendered later on `Default/90` and covered them.
- Active characters also rendered on `Default`, while the old Door trigger moved them to `OutsideObject`; that put traversing characters behind the entire dungeon composition.
- The required render order is `DungeonBackObject Door < DungeonMiddleObject traversal character < Wall ceiling < Default character`. The dedicated dungeon Door now uses `DungeonBackObject/101`, and its trigger uses `DungeonMiddleObject` until exit.
- `InteriorDoor` is a different case: its visual must sit just above the retained wall tile on `Wall/101`, while characters remain on `Default` for the whole overlap.
- Static visual coverage now asserts three preserved ceiling tiles and the complete sorting-layer ordering, not only renderer dimensions.
- The compiled PlayMode verifier instantiates the real `CharacterPrefab`, overlaps its actual 2D trigger collider with each Door, captures the overlap, verifies the renderer layer, moves it outside, verifies restoration, and destroys the probe.
- Final runtime values were dungeon ceiling `3/3`, dungeon overlap `DungeonMiddleObject`, dungeon exit `Default`, interior overlap `Default`, and interior exit `Default`.
- The corrected dungeon capture at `Temp/phase41-dungeon-door-character-camera.png` visibly shows the ceiling in front of the arch and characters in front of the Door.

## 2026-07-14 - Dungeon Entrance Path and Exterior-Frame Occlusion

- Live PlayMode showed the configured inside target at grid `(4,0)` / world `(-3.5,0)`, while the initialized three-cell dungeon Door center was grid `(2,0)` / world `(-1.5,0)`. The character was crossing the exterior frame two cells left of the opening; there was no Y-coordinate rise.
- The inside target is still valid for normal dungeon navigation, so only the first entry/exit waypoint now resolves from the initialized exact `Door` in the live grid. The final path is `(10,0) -> (-1.5,0) -> (-3.5,0)`.
- The exterior arch is a foreground frame with a transparent opening. Its renderer now uses `Wall/99`, traversal characters use `DungeonMiddleObject`, and ceiling tiles remain `Wall/100`; characters are visible through the opening but occluded by pillars and masonry.
- Reused PlayMode state contained several exact `Door` components with null `BuildingData`. Their trigger exits could reset a character to `Default` while it was still inside the real entrance, so layer changes are now limited to initialized Doors.
- Grid Foundation now covers live Door-center resolution, and Grid Visual covers `traversal character < Door frame < ceiling < default character` ordering.
- PlayMode passed with `entryPath=(10,0)->(-1.5,0)->(-3.5,0)`, overlap `DungeonMiddleObject`, frame overlap preserved, and exit `Default`.
- `Temp/phase42-dungeon-door-frame-occlusion-camera.png` shows the right-side traversal character hidden by the exterior pillar while the centered character remains visible through the opening.

## 2026-07-14 - Unified UI Reaudit Started

- The supplied current-game capture exposes a placeholder `New Text` header, raw gray panel styling, fluorescent default bars, and only four visible needs (`기분`, `배고픔`, `재미`, `졸림`). This is not an acceptable final player surface even though the earlier feature-gap audit marked its enumerated systems connected.
- The existing `ui-feature-surface-gap-audit.md` verifies many action paths, but it explicitly treats style and information density as separate follow-up work and does not prove that every present-day character need/status field is displayed.
- Phase 43 therefore reopens the audit at the data-model-to-live-UI level: every runtime state needs a discoverable surface, and every surface must also pass visual hierarchy, readability, bounds, click, and state-change checks.
- Live PlayMode inspection proved `CharacterStats.EnsureStats()` owns six conditions, while `CharacterSummeryInfo` in both gameplay scenes has only four serialized slider references. `excretion`, `hygiene`, and the generated log reference are null before runtime setup.
- The current character panel is only `495x281`; its generated log occupies the same lower region as the four need rows. The baseline capture visibly shows log text crossing the bars and the panel clipping all additional needs.
- Health, maximum health, injury severity, species, role, and lifecycle are available on current runtime components but absent from this character surface. These are new Phase 43 surface gaps, not styling-only defects.
- The primary Canvas is `1920x1080` Screen Space Overlay. Its money/time blocks, three top-right controls, ten bottom navigation buttons, building panel, and construction surface still use white Unity default sprites/colors, while generated feature tabs already use a separate dark style. A shared semantic theme is needed to make authored and generated UI read as one game.
- The corrected character view now presents all six condition values, current/max health, injury state, species, role, lifecycle, and recent logs within a fixed `460x700` left-side tool panel. The final measured generated meter values and bounds are valid; old serialized sliders remain inactive and no longer overlap the generated view.
- A real `Button.OnPointerClick` on the bottom `운영` control opened tab id 5 and changed the selected button target color to `#3D8B70FF`. The generated P0/P1/P2 panel now uses the full width, an opaque common palette, and a taller `520px` work surface.
- `BuildableObject` exposes damage, facility level, current users, reservations, capacity, room/facility roles, supported work types, worker requirements, position, and stock. The previous `BuildingSummaryFormatter` surfaced only shop/warehouse stock, and `UIBuildingInfo` hid all five authored detail rows because its `explains` dictionary was always empty.
- The new building presentation is shared by both building-detail paths. Live id 14 evidence displayed `정상`, `Lv.1`, `(10,2)`, `0/3`, reservation `0`, role `휴식`, work `운영/수리/청소`, and required workers `0`.
- The staff profile already surfaced species and traits but omitted the nine computed base stats. Those stats are now included in the selected-worker profile card instead of remaining runtime-only values.

## 2026-07-14 - Exterior Door Visual-Clearance Regression

- The exterior Door, its ceiling, and an entering live character were correctly ordered as `DungeonMiddleObject:0 < Wall:99 < Wall:100`; the remaining edge case was temporal rather than a missing sorting layer.
- `Door.OnTriggerExit2D` restored `Default` immediately when the physics collider stopped overlapping. A character sprite wider than its collider could therefore remain over the frame after the physics exit and render above the outer wall.
- Exterior traversal now reapplies the behind-wall layer during trigger stay and waits until `CharacterActor.VisualRenderer.bounds` fully clears the Door visual bounds plus a small clearance before restoring `Default`.
- Collider lookup now resolves `CharacterActor` from parents, so child colliders cannot bypass traversal sorting.
- The separate `InteriorDoor` still opts out of exterior traversal-layer changes; this correction applies only to the original three-cell dungeon entrance.

## 2026-07-14 - Unified UI Reaudit Closed

- The P0 and P1/P2 feature surfaces still expose all previously audited actions after the theme and information-density changes. Final reruns passed P0 with `140` active RectTransforms and P1/P2 `18/18` with `191` active RectTransforms; both had invalid/oversized `0` and captured Error/Warning `0/0`.
- The original notice prefab used a `42px` content-sized TMP label and allowed up to 15 simultaneous objects. P1/P2 action bursts therefore covered much of the world and other panels even though the messages faded correctly.
- Runtime notice layout now owns a top-center `520x360` clipped lane. Each toast is `56px`, uses `18px` wrapped/ellipsized text, an opaque shared-theme surface, no raycast targets, and a CanvasGroup fade. New messages retain at most six visible items.
- Staff management mode content rebuilt correctly, but the static mode bar never refreshed its initial selected color. Persisting the mode buttons and restyling them on mode change fixes the content/highlight mismatch.
- The unified verifier proves six character needs, live meter updates, no placeholder text, compact notice geometry, actual close clicks, the management selected state, all nine staff stats, and clean captures.
- Facility name rectangles visible in the world are baked placeholder sprites under `Assets/Images/Placeholders/Facilities`; they are not TMP/UI surface gaps and require a separate world-art replacement pass.

## 2026-07-14 - Character Click Priority Follow-Up

- Character and building colliders can both receive `OnMouseDown` for one overlapping click.
- The building callback already asks `IWorldInfoClickSelector` to prioritize a character, but the character callback may open its popup first. That newly opened popup makes `IUiPointerBlocker` return true during the building callback in the same frame, causing the building event to pass through.
- A successful character selection therefore needs to consume every remaining world callback in that frame before the selector reevaluates current UI state.
- The first visual pass showed only the character panel and zero building click events in both callback orders. The verifier is tightened further by relocating the overlap to screen X `300`, which starts as world space but becomes covered by the opened character panel; this directly exercises the UI-state transition that caused the leak.
- The strict final run passed at screen `(300,594)`: the point began outside UI, the character popup opened synchronously over it, and the following building callback still produced `buildingClicks=0`. Reversing callback order also produced zero building clicks and the same character-only UI state.

## 2026-07-14 - Dungeon Door Directional Layer Follow-Up

- The committed original `Door.Initialization()` configured a `BoxCollider2D` with `size=(3,1)` and `offset=(2,0.5)`. That exterior-side trigger was removed while the dungeon door and one-cell interior door were being separated.
- Current scene dungeon doors instead carry the generic one-cell `size=(1,2.9)`, `offset=(0,1.5)` collider. It begins too close to the opening, so a character standing against the exterior right wall remains on the front layer.
- The later visual-bounds clearance coroutine also keeps a character behind until the entire sprite clears the full `3x3` arch. With the original directional trigger restored, the correct transition is the trigger's interior-side exit, not full-artwork clearance.
- Final runtime diagnostics showed all directional groups passing. The only unrelated integration failure was `BuildingData.unlocked=true` after PlayMode progression initialization, so mutable unlock state is not a valid prerequisite for a door rendering/traversal regression.
- The final exact-door trigger is the original `size=(3,1)`, `offset=(2,0.5)`. On the exterior/right wall side the character is `DungeonMiddleObject` behind `Wall/99`; after the interior/left trigger edge it immediately returns to `Default`, even while its sprite still overlaps the `3x3` arch.
- `InteriorDoor` remains independent at `size=(1,2.9)`, `offset=(0,1.5)` and does not change character layers.
# Phase 46 - Character/Building Click Arbitration

- Directly invoking both private `OnMouseDown` methods did not represent Unity's real input dispatch and allowed the Phase 44 verifier to pass while the live bug remained.
- A frame cache alone cannot define a physical click when multiple colliders and popup activation participate in the same press.
- `WorldInfoClickInputController` now owns the left-button edge. `WorldInfoClickSelectionService` receives one collider set, prefers `CharacterActor`, otherwise selects the visually foremost `BuildableObject`, and records the decision before publishing UI events.
- Legacy collider callbacks remain compatible but route through the same service, so callbacks arriving before or after the central tick consume the same frame decision.
- The legacy `UI/BuildingInfoPanel/ButtonSelection` is a separate Screen Space Overlay raycast target outside the generated building summary root; it can remain over the original QA coordinate after the summary root closes, so the building-only regression uses a dynamically unoccupied screen point.

# Phase 47 - The Unchecked Building Detail Path

- The player-visible center modal is `UIBuildingInfo`, not the `BuildingSummaryInfo.UI` child used by the Phase 46 pass predicate.
- `WorldInfoClickInputController` correctly chose the overlapping character, but `GridUIManager.Update()` handled the same mouse-down independently through `GetBuildingByMousePos()` and opened `UIBuildingInfo` anyway.
- Moving a building collider during verification invalidated the relationship between its world position and the grid registry, so that verifier could not exercise the real `GridUIManager` detail path.
- A correct visual assertion must inspect three surfaces: `CharacterSummeryInfo.UI`, `BuildingSummaryInfo.UI`, and the active/alpha state of `UIBuildingInfo`.
- The canonical building result is the full `UIBuildingInfo` modal. Opening it closes the transient `BuildingSummaryInfo` popup so two building presentations cannot stack invisibly.

# Phase 48 - Character Log Signal and Presentation

- `CharacterLogUtility.ToCauseTag()` converted every work start into the literal `행동 시작`, discarding the work type and target before the UI ever received it.
- `WorkDutyController.CheckActionWork()` wrote a progress line once per second. Since the log compressor grouped identical causes, the player saw `작업 진행 xN` instead of meaningful transitions.
- Character identity is already established by the selected panel, so repeating `[character name]` inside every row adds noise rather than context.
- Status meters and activity history are different reading tasks. Separate tabs give both surfaces the full panel width and prevent the history from shrinking underneath seven meter rows.
## 2026-07-14 - LLM-Enriched Character Record Findings

- `LocalLlmRequestQueue` is the single serialized local-LLM transport and already owns concurrency, queue capacity, priorities, JSON mode, timeouts, and callback results through `ILocalLlmRuntime`.
- `SocialReputationRuntime`, `CharacterDialogueRuntime`, and `CharacterFeedbackBubble` subscribe to `CharacterLog.OnLogAdded`; an LLM rewrite must not emit that event again or it would duplicate downstream gameplay/feedback work.
- `CharacterLog` currently stores only display strings. A parallel stable entry ID and a display-only change event can preserve existing serialized data while allowing an asynchronous response to update exactly the originating row.
- Records should show the structured original immediately. Only meaningful transitions should enter the LLM queue, and failed, dropped, timed-out, malformed, or stale responses should leave that original untouched.
- Character prompt context can be grounded with `CharacterIdentity.DisplayName`, species, role, generated persona trait/flavor, and the exact original event. The model must return one compact JSON object so the existing transport contract remains intact.
- Free-form generation alone was not reliable enough: the local model could soften the prose while dropping `새 제조법` or inventing `재료`/`일진`. Exact source anchors, transition semantics, grounded content-word validation, and bounded correction are required before replacing a visible row.
- Successful wait/roam actions and generic non-work action starts created duplicate, low-value records and consumed the LLM queue. Facility/ability completion and failure records already carry the meaningful result, so those generic start records should remain omitted.
- `WorkDebugLog` must not fabricate `업무 · 대상 없음`; when no work type or target exists, the separate failure/duty-state record is the authoritative user-facing event.
- The final accepted completion line was `지하 연구실에서 연금 연구를 마치고 새 제조법 정리를 마무리했다.`; it preserved every required source phrase and used a natural non-report ending.

## Phase 50 - Record Subject Finding

- Omitting the selected character name made an individually viewed Records tab look tidy, but it also removed the grammatical subject from every generated sentence. The visible record itself must carry `캐릭터명+이/가`; panel context is not a substitute for a complete activity sentence.

## Phase 51 - Record Variety Finding

- The prompt had four style seeds, but lifecycle branches ignored them and always returned one start/completion shape. Corrections also supplied one fixed sentence, so the local model was being steered back into the same cadence even at nonzero temperature.

## Phase 52 - Anecdote Finding

- Rotating syntax still produces polished task summaries, not situations. A genuinely entertaining record needs permission and a requirement to invent one low-stakes transient beat, while gameplay-changing consequences remain forbidden and the original event anchors stay mandatory.

## Phase 54 - Mood Separation Requirement

- Mood is not a physiological/entertainment need and must not be displayed in the Needs group.
- Mood should be a derived character state affected by need satisfaction/deprivation plus work, rest, facility, and social events.
- The player needs a dedicated Mood tab that explains the current value through active positive and negative factors, rather than showing only an unexplained meter.
- Initial target UI structure is Status / Mood / Records, with health and needs remaining on Status and mood value plus active factors moving to Mood.
- `CharacterSummeryInfo.OnStatChange` currently binds `CharacterCondition.MOOD` through the same meter path as FUN/HUNGER/SLEEP/EXCRETION/HYGIENE, which directly causes the reported grouping error.
- `Facility`, `WorkDutyController`, `OffenseExpeditionService`, and staff-discontent recovery currently mutate `MOOD` directly. A compatibility bridge is needed so existing gameplay calls become named mood factors instead of silently disappearing.
- The generated character view is owned by `CharacterSummaryRuntimeLogFactory`; adding the third tab there avoids prefab churn and matches the established runtime UI pattern.
- `CharacterStats` already owns ticking needs and the stat-change event, so it is the lowest-risk owner for a derived mood snapshot without adding another required prefab component.
- Proposed aggregate is neutral base `50` plus deterministic need-threshold factors plus expiring interaction factors, clamped to `0..100` and mirrored to the legacy `MOOD` dictionary entry for existing AI/discontent readers.
- Interaction factors need a stable ID, Korean label, signed offset, expiry, stack count, and capped stacking. Reapplying a stable work-fatigue ID should refresh/stack within a cap instead of creating an unbounded row per tick.
- Existing facility recovery combines need restoration and direct mood gain. The need restoration should affect mood automatically, while the facility experience itself can add one named timed factor based on the facility roles.
- Explicit event integration points exist for facility use, off-duty waiting, repeated work fatigue, expedition resolution, and `CharacterSocialMemory.HearRumor`; mood factors can be applied at those points without inferring gameplay from display logs.
- The existing Unified UI verifier assumes six status meters and only two tabs. It must be upgraded to assert five needs on Status, a separate Mood tab, factor rows, tab exclusivity, live need-derived mood change, and a timed interaction factor.
- `CharacterActor` already delegates stat operations to `CharacterStats`; adding mood-factor delegation there keeps facility/social/action callers concise and preserves the current actor-facing API style.
- Many existing AI and QA paths directly assign `stats[MOOD]` to stage low/high-mood scenarios. Recalculating on every dictionary read would invalidate those scenarios. The mood runtime should detect an external value differing from its last calculated value, reinterpret it as a base-mood override, and then apply future need/factor deltas from that baseline.
- Only `CharacterSummaryRuntimeLogFactory` calls the summary binding methods, so their signatures and hierarchy can be changed together without widening compatibility shims.
- The first visual capture shows three equal-width tabs, a dedicated mood meter, base/offset summary, and grouped factor text without clipping or overlap at `1920x1080`.
- The Main Camera capture renders the world correctly but, as expected for Screen Space Overlay UI, the Game View screenshot is the authoritative visual evidence for the mood tab itself.

## 2026-07-14 - Room Overlay Wall-Line Finding

- The unexplained vertical wall mark came from the room overlay perimeter, not the wall tile or room-layout data.
- Lowering the vertical stroke below `BuildingBack` was insufficient because the stroke remained visible through transparent pixels in the wall sprite.
- Room cells already meet structural walls on their left/right perimeter, so vertical strokes add no useful information there. Suppressing them removes the false wall seam while the translucent fill and horizontal ceiling/floor edges still communicate the selected room.
- MCP editor focus can overwrite a synthetic pointer on the physical mouse. A dedicated temporary Input System mouse makes the PlayMode UI verifier deterministic while preserving the real UI/input action path.

## 2026-07-14 - Visible Room Outline Finding

- A complete perimeter can remain readable without resembling wall damage when it is inset from the structural boundary and uses a dedicated selection accent instead of the room's neutral storage color.
- Runtime-created SpriteRenderers inherit URP's lit sprite material in this project. Their serialized color can be correct while the rendered result becomes nearly black in areas without a contributing 2D light.
- The room overlay therefore owns one unlit sprite material shared by fill and outline renderers. `RoomOutline` sits above `Wall` and below `Default`, so the frame is visible over wall art while doors, facilities, and characters stay in front.
- Render-property assertions alone were insufficient. The verifier now compares baseline and overlay camera pixels and requires a visible green-dominant pixel count.

## 2026-07-14 - Mirrored Room Outline Geometry

- `Grid.GetWorldPos` computes world X as `origin.x - grid.x`; increasing grid X moves left on screen.
- The first outline implementation treated `Vector2Int.left/right` as world-left/right. Both vertical strokes therefore landed on the inner side of their boundary cells, visually shrinking the selected room by one cell at each end.
- Room detection was correct: the active formal room spans all 27 cells from grid X 4 through 30. Only the outline geometry was mirrored.
- Vertical edge placement and horizontal corner insets must map grid negative-X to world-right and grid positive-X to world-left.

## 2026-07-14 - Flush Room Outline Bounds

- The remaining floating appearance was not a room-detection offset. The outline center was deliberately inset by `0.11` world units, leaving a visible strip between the stroke and the room boundary.
- Centering each stroke half its own thickness inside the room makes the outer face exactly flush with the fill union while keeping the full line inside the selected room.
- A renderer-bounds regression now compares left, right, top, and bottom extents directly; checking only edge centers was too loose to catch a small visual gap.
- Unity scene teardown can destroy `UpperRightPanel` before VContainer disposes `RoomInspectionView`. UI restoration during `Dispose()` must use Unity object null checks, not assume the original HUD still exists.

## 2026-07-14 - Ceiling-Free Room Selection Outline

- A closed four-sided room frame visually claimed the structural ceiling as part of the selected floor space, even when its geometry was mathematically flush.
- Omitting only top horizontal perimeter strokes keeps the useful side and floor boundaries while leaving the ceiling visually structural and unselected.
- Vertical strokes already reserve half a stroke at top-edge corners, so removing the top stroke does not leave lines protruding into the ceiling.
- PlayMode edits can execute against the previous loaded assembly until the editor returns to Edit Mode and refreshes assets; report keys and renderer counts are useful stale-assembly checks.

## 2026-07-14 - Inset Top Room Outline

- Excluding the ceiling should not mean deleting the room's top outline. Without that stroke, the selection reads as an open U shape and users reasonably perceive the border as missing.
- The intended visual model is a closed room-interior frame: retain the top stroke but lower its outer edge by `0.5` world units from the structural ceiling.
- Side strokes must use the same clearance in their top inset so they terminate at the lowered horizontal stroke without entering the ceiling.

## 2026-07-14 - Build Catalog Placement UX

- The build item button enabled placement but left both the category popup and full-screen build catalog active, visually covering most of the world during the task that needs the most spatial context.
- The popup stack must be unwound normally: close the active category first, then close the parent catalog while suppressing only that one close's placement cancellation.
- Hiding GameObjects directly would leave stale popup entries and break later Escape or reopen behavior.
- Placement cancellation had a separate ordering bug: `SetGridModeNone()` changed mode before clearing the selected building, while `SelectBuildingById` rejected all changes in None mode. Allowing id `-1` in None mode makes cancellation complete.
- A collapsed placement state should preserve four independent signals: Build mode, selected data, grid overlay, and ghost visibility. Catalog inactivity alone is not enough.

## 2026-07-14 - Live Wall Traversal Root Cause

- `Grid.IsWalkable` rejected an existing wall, but terminal path conditions could still admit a structural wall as the final step.
- `AbilityMove` consumed a previously calculated queue without checking the target again during interpolation. A wall installed after path creation therefore did not invalidate the actor's transform movement.
- Structural-wall blocking must exclude `Door` data explicitly; both the one-cell interior door and the three-cell dungeon entrance remain traversable.
- Runtime walk movement now checks before a step, during every interpolated frame, and before committing the final position. A newly blocked move rolls back to the exact prior valid world position.
- `MoveByStep()` is also called directly by invasion movement. A persistent blocked-result flag is required so that caller exits before applying on-enter defense triggers or consuming later path steps.
- Characters are not grid occupants, so placement can occur on their current cell. Character lifecycle instances must listen for grid-object changes and eject active actors from a newly occupied structural-wall cell.

## 2026-07-14 - Version-Gated Wall Revalidation

- Grid occupancy already exposes a monotonically increasing `Grid.version` updated by occupant registration and removal.
- A moving character does not need to query its destination cell while that version is unchanged. The steady-state loop can use one integer comparison per moving frame.
- After any version change, the movement checks its target once and stores the new observed version. Unrelated construction therefore costs one cell lookup for that change, not one lookup on every later frame.
- The initial pre-step wall check remains necessary because a queued step can already be stale before interpolation begins.
- PlayMode movement verification must stop pre-existing actor coroutines; disabling the brain does not retroactively stop a work coroutine already hosted by another character component.

## 2026-07-15 - Modular Facility Runtime Findings

- Modular props need independent grid slots, not only visual offsets. A single Building slot cannot represent floor furniture, wall fixtures, ceiling fixtures, and floor overlays that intentionally share one cell.
- Mounted sprites must render above structural wall tiles but below characters. In this project, wall and ceiling fixtures use the `Wall` layer at order 102, while floor overlays use `DungeonHallway` at order 110.
- Scene-authored monolith references can be retired without saving a dirty scene by expanding stable legacy asset names into modular recipes inside initial placement.
- Full asset production needs both catalog-count verification and real pointer placement. ScriptableObject existence alone does not catch hidden buttons, wrong mounting layers, ghost mismatches, or renderer occlusion.
- Existing editor fixtures that instantiate runtime buildings directly must follow the same required dependency-injection contract as PlayMode. Otherwise they validate an obsolete construction path rather than the product runtime.

## 2026-07-15 - Modular Facility Production-Depth Audit

- The existing modular verifier proves 73 catalog buttons and 73 runtime instances, but its real pointer placement path covers only one representative item from each of the four independent grid layers.
- `Facility.ApplyUseRecovery` currently maps broad room roles to generic need recovery. It does not distinguish named cooking devices, training equipment, mana devices, sanitation fixtures, or crafting stations.
- Only the modular sales counter is a `Shop` runtime object, and only the large storage shelf owns a real warehouse inventory. Most seats and storage supports currently contribute room metrics or traits without changing real service capacity or inventory.
- All generated modular assets are currently forced unlocked for verification. Production progression needs explicit construction cost, unlock phase, and refund data instead of relying on the temporary `unlocked = true` catalog state.
- Legacy recipe tests prove footprint containment and independent-layer non-overlap, but do not yet instantiate every recipe as a formal PlayMode room and exercise its pathing and service contract.
- No general world save/load round trip was found for modular placements and mutable building state. Existing state snapshots are feature-local and do not prove dungeon persistence.

## 2026-07-15 - Exhaustive Modular Pointer Verification

- The original PlayMode verifier covered only four representative layers. The exhaustive loop now drives all 73 catalog buttons, placement ghosts, world placement clicks, world selection clicks, detailed-info matching, demolition clicks, cost deductions, refunds, and layer release.
- Calling `InputSystem.Update()` manually inside a coroutine consumed `wasPressedThisFrame` before `GridUIManager.Update`; queueing state and letting Unity advance the next frame is required for an authentic UI click path.
- Physics selection misses independent mounted fixtures when trigger hits are unavailable. A grid-cell fallback after character-first physics priority selects the same top building without restoring the old character/building double-click bug.
- The final exhaustive report passed placed/selected/demolished `73/73` with captured Error/Warning `0/0`.
- `modular-facility-catalog.png` and MCP Main Camera are visually nonblank and correctly layered. The intermediate placement ScreenCapture panned away from the dungeon because edge pointer input drove the camera, so a camera-locked final HUD capture is still required.

## 2026-07-15 - Formal Modular Recipe Verification

- A reliable recipe PlayMode test needs a disposable formal room in the live grid. Clearing its runtime furniture through the production placement service preserves tilemap visuals and avoids saving the dirty scene.
- `Grid.GetWorldPos(Vector2Int)` already returns the center of an integer cell. Passing `cell + 0.5` moves to a boundary on this inverted-X grid and can round into either adjacent cell.
- Synthetic pointer verification must disable and restore `CameraManager`; otherwise edge scrolling changes the screen-to-grid mapping while a click coroutine is in flight.
- `RoomInstance.Furniture` intentionally contains only facilities that contribute a room role. Role-free supports belong to the room through interior occupancy, environment collection, and path reachability, not `RoomInstance.ContainsPart`.
- Static room assembly and real pointer assembly now agree for all 21 recipes, including compound Rest+Administration and Rest+Security identities.

## 2026-07-15 - Modular Facility Economy

- Operational construction cost and legacy `ConditionNeedMoney` are two independent deduction mechanisms. A modular asset carrying both must use only the operational contract, or it can be rejected and charged twice.
- Compatibility should be narrow: suppress `ConditionNeedMoney` only for an operationally modular building, not every build condition and not legacy buildings.
- Unlock UI availability must be re-evaluated when the observed day changes. Day 1, 4, and 7 are the meaningful boundary values for the current three phases.
- Money shortage should keep placement active for retry, leave occupancy and balance unchanged, tint the ghost red, and state both required and owned amounts.
- The installability overlay intentionally describes layer/cell availability; affordability belongs to the selected ghost and feedback, matching the user's earlier requirement not to paint the whole grid red based on the selected footprint or price.

## 2026-07-14 - Building Info Preview Dark Tint

- `UIBuildingInfo` assigns `BuildingSO.icon` directly to a standard Unity UI `Image`; there is no preview camera or world lighting involved.
- `DungeonUiThemeRuntime.StyleBuildingPanel()` treated every sprite-less non-button image as a panel surface. While the info panel was closed, the preview image had no sprite and was tinted with `DungeonUiTheme.Surface`.
- `DisplayBuildingInfo()` later changed only the sprite, so Unity multiplied the icon by the retained dark surface tint and made the pixel art appear almost black.
- The preview needs both a theme exclusion and display-time normalization because theme refresh and panel initialization order can vary.
