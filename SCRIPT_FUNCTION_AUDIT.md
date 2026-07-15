# Script Function Audit

- Generated: 2026-07-12
- Scope: all non-Editor `Assets/**/*.cs` scripts
- Total non-Editor scripts: 912
- Project-owned scripts audited in detail: 299
- Vendor/plugin scripts listed separately: 613

## Review Policy
- Project-owned code is under `Assets/Scripts/**` plus `Assets/DataManager.cs`; these files are valid refactor targets.
- Vendor/plugin code is included for function coverage but is not edited because package upgrades would overwrite or conflict with changes.
- Coupling flags are static-analysis hints; concrete refactors are made only after reading the affected source.
- Factory/UI/provider contexts are judged differently: object creation in a factory and UI types in UI code are usually appropriate, while the same signals in domain objects are refactor candidates.

## Completed Refactor Notes
- Added `IPlayerInputReader` and `IWorldPointerRaycaster` under `Assets/Scripts/Infrastructure/PlayerInputServices.cs`.
- Registered those services in `DungeonRuntimeLifetimeScope`.
- Replaced direct runtime `Input` calls in `UIManager`, `CameraManager`, `GridUIManager`, `OwnerCommandController`, `WorldInfoClickSelectionService`, and `SceneCameraWorldPointerPositionProvider` with injected abstractions.
- Fixed `UIManager` value-change unsubscription by replacing anonymous lambdas with stable handlers.

## High-Priority Coupling Signals

| Score | File | Type | Function | Line | Signals | Initial judgment |
|-------|------|------|----------|------|---------|------------------|
| 3 | `Assets/Scripts/Meta/RunResultPanelFactory.cs` | `RunResultPanelFactory` | `CreateDefaultPanel` | 25 | Manual resolver/injection, Runtime object creation, UI type dependency | manual resolver use is acceptable only in composition roots/factories; UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 3 | `Assets/Scripts/UI/UITabGeneratedPanelFactory.cs` | `UITabGeneratedPanelFactory` | `Create` | 50 | Manual resolver/injection, Runtime object creation, UI type dependency | manual resolver use is acceptable only in composition roots/factories; UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Buildings/UI/UIBuildingInfo.cs` | `UIBuildingInfo` | `Awake` | 40 | Lifecycle contains dependency/workflow logic, UI type dependency | UI type usage fits this UI/factory context. |
| 2 | `Assets/Scripts/Character/AI/LocalLlmRequestQueue.cs` | `LocalLlmRequestQueue` | `Update` | 169 | Coroutine/timing orchestration, Lifecycle contains dependency/workflow logic | timed workflow may deserve a service if it coordinates domain state. |
| 2 | `Assets/Scripts/Character/CharacterSpawner.cs` | `CharacterSpawner` | `Start` | 49 | Coroutine/timing orchestration, Lifecycle contains dependency/workflow logic | timed workflow may deserve a service if it coordinates domain state. |
| 2 | `Assets/Scripts/Character/Core/CharacterActor.cs` | `CharacterActor` | `Start` | 232 | Coroutine/timing orchestration, Lifecycle contains dependency/workflow logic | timed workflow may deserve a service if it coordinates domain state. |
| 2 | `Assets/Scripts/Character/UI/OwnerSelectionOptionButtonFactory.cs` | `OwnerSelectionOptionButtonFactory` | `Create` | 22 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanelUiFactory.cs` | `StaffWorkPriorityPanelUiFactory` | `CreateUiObject` | 46 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/GameManager.cs` | `GameManager` | `Start` | 30 | Coroutine/timing orchestration, Lifecycle contains dependency/workflow logic | timed workflow may deserve a service if it coordinates domain state. |
| 2 | `Assets/Scripts/Grid/DungeonStory/UI/GridConstructButtonFactory.cs` | `GridConstructButtonFactory` | `CreateCategoryPanel` | 36 | Manual resolver/injection, Runtime object creation | manual resolver use is acceptable only in composition roots/factories; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Grid/DungeonStory/UI/GridConstructButtonFactory.cs` | `GridConstructButtonFactory` | `CreateCategoryButton` | 55 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Grid/DungeonStory/UI/GridConstructButtonFactory.cs` | `GridConstructButtonFactory` | `CreateBuildingSelectButton` | 87 | Manual resolver/injection, Runtime object creation | manual resolver use is acceptable only in composition roots/factories; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Infrastructure/DungeonRuntimeLifetimeScope.cs` | `DungeonRuntimeLifetimeScope` | `InjectSceneComponents` | 298 | Manual resolver/injection, Scene query coupling | manual resolver use is acceptable only in composition roots/factories; scene query is acceptable as a provider/composition boundary. |
| 2 | `Assets/Scripts/Infrastructure/DungeonRuntimeLifetimeScope.cs` | `DungeonRuntimeLifetimeScope` | `CaptureSceneRuntimeReferences` | 543 | Scene query coupling, UI type dependency | scene query is acceptable as a provider/composition boundary; non-UI code depends on UI types. |
| 2 | `Assets/Scripts/Offense/OffensePanelFactory.cs` | `OffensePanelFactory` | `CreateWorldMapPanel` | 25 | Manual resolver/injection, UI type dependency | manual resolver use is acceptable only in composition roots/factories; UI type usage fits this UI/factory context. |
| 2 | `Assets/Scripts/Offense/OffensePanelFactory.cs` | `OffensePanelFactory` | `CreateExpeditionPanel` | 71 | Manual resolver/injection, UI type dependency | manual resolver use is acceptable only in composition roots/factories; UI type usage fits this UI/factory context. |
| 2 | `Assets/Scripts/Offense/OffensePanelUiFactory.cs` | `OffensePanelUiFactory` | `CreateOverlayCanvas` | 8 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Offense/OffensePanelUiFactory.cs` | `OffensePanelUiFactory` | `CreatePanel` | 23 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Offense/OffensePanelUiFactory.cs` | `OffensePanelUiFactory` | `CreateVerticalRoot` | 41 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Offense/OffensePanelUiFactory.cs` | `OffensePanelUiFactory` | `CreateText` | 66 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Offense/OffensePanelUiFactory.cs` | `OffensePanelUiFactory` | `CreateButton` | 89 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Operation/EventAlertCanvasProvider.cs` | `EventAlertCanvasProvider` | `GetOrCreateCanvas` | 19 | Runtime object creation, UI type dependency | non-UI code depends on UI types; object creation is a factory boundary candidate. |
| 2 | `Assets/Scripts/Operation/EventAlertUiFactory.cs` | `EventAlertUiFactory` | `CreateAlertButton` | 8 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Operation/EventAlertUiFactory.cs` | `EventAlertUiFactory` | `CreateChoiceButton` | 59 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Operation/EventAlertUiFactory.cs` | `EventAlertUiFactory` | `CreateText` | 100 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Operation/EventAlertViewUiFactory.cs` | `EventAlertViewUiFactory` | `CreateRuntimeRoot` | 45 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Operation/EventAlertViewUiFactory.cs` | `EventAlertViewUiFactory` | `CreateButtonRoot` | 58 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Operation/EventAlertViewUiFactory.cs` | `EventAlertViewUiFactory` | `CreateDetailPanel` | 148 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/Operation/EventAlertViewUiFactory.cs` | `EventAlertViewUiFactory` | `CreateText` | 198 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/UI/CharacterSummaryRuntimeLogFactory.cs` | `CharacterSummaryRuntimeLogFactory` | `Ensure` | 24 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/UI/RuntimePanelFactories.cs` | `CodexPanelFactory` | `CreateDefaultPanel` | 43 | Manual resolver/injection, UI type dependency | manual resolver use is acceptable only in composition roots/factories; UI type usage fits this UI/factory context. |
| 2 | `Assets/Scripts/UI/RuntimePanelFactories.cs` | `FacilitySynthesisPanelFactory` | `CreateDefaultPanel` | 93 | Manual resolver/injection, UI type dependency | manual resolver use is acceptable only in composition roots/factories; UI type usage fits this UI/factory context. |
| 2 | `Assets/Scripts/UI/RuntimePanelFactories.cs` | `FacilityEvolutionPanelFactory` | `CreateDefaultPanel` | 143 | Manual resolver/injection, UI type dependency | manual resolver use is acceptable only in composition roots/factories; UI type usage fits this UI/factory context. |
| 2 | `Assets/Scripts/UI/RuntimePanelFactories.cs` | `RuntimePanelFactoryUtility` | `CreateOverlayCanvas` | 174 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/UI/RuntimePanelFactories.cs` | `RuntimePanelFactoryUtility` | `CreatePanel` | 184 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/UI/RuntimePanelFactories.cs` | `RuntimePanelFactoryUtility` | `CreateSummaryText` | 206 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/UI/UITabGeneratedPanelFactory.cs` | `UITabGeneratedPanelFactory` | `CreateText` | 106 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 2 | `Assets/Scripts/UI/UITabTopButtonFactory.cs` | `UITabTopButtonFactory` | `CreateButton` | 21 | Runtime object creation, UI type dependency | UI type usage fits this UI/factory context; object creation fits this factory/context boundary. |
| 1 | `Assets/Scripts/Buildings/Facility.cs` | `Facility` | `Interact` | 23 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Buildings/Shop.cs` | `Shop` | `Interact` | 81 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Buildings/Shop.cs` | `Shop` | `RunCheckoutService` | 312 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Buildings/Shop.cs` | `Shop` | `WaitForServingWorker` | 343 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Buildings/Stair.cs` | `Stair` | `Traverse` | 11 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Buildings/UI/UIBuildingInfo.cs` | `UIBuildingInfo` | `DisplayBuildingInfo` | 52 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Buildings/UI/UIBuildingSelectButton.cs` | `UIBuildingSelectButton` | `ApplyButtonSize` | 51 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Buildings/UI/UIBuildingSelectButton.cs` | `UIBuildingSelectButton` | `SetIcon` | 59 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Buildings/UI/UIBuildingSelectButton.cs` | `UIBuildingSelectButton` | `ResolveIconImage` | 78 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/Ability/AbilityMove.cs` | `AbilityMove` | `StartExitDungeon` | 97 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Ability/AbilityMove.cs` | `AbilityMove` | `StartEnterDungeon` | 116 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Ability/AbilityMove.cs` | `AbilityMove` | `CancelActiveMovement` | 152 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Ability/AbilityMove.cs` | `AbilityMove` | `StartTrackedActionMovement` | 161 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Ability/AbilityMove.cs` | `AbilityMove` | `ExitDungeon` | 423 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Ability/AbilityShopping.cs` | `AbilityShopping` | `StartSopping` | 147 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Ability/AbilityShopping.cs` | `AbilityShopping` | `BuyItem` | 355 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Ability/AbilityWork.cs` | `AbilityWork` | `StartWorking` | 213 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Ability/AbilityWork.cs` | `AbilityWork` | `StartCheckActionWork` | 487 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Ability/AbilityWork.cs` | `AbilityWork` | `StopActiveWorkRoutine` | 609 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Ability/AbilityWork.cs` | `AbilityWork` | `StopActiveWorkCheckRoutine` | 620 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/AI/AiDirectorContextSceneQuery.cs` | `AiDirectorContextSceneQuery` | `Capture` | 30 | Scene query coupling | scene discovery should be isolated behind providers/build callbacks. |
| 1 | `Assets/Scripts/Character/AI/CharacterAiDecisionPipeline.cs` | `CharacterAiFacilityLookup` | `FindFacility` | 63 | Scene query coupling | scene discovery should be isolated behind providers/build callbacks. |
| 1 | `Assets/Scripts/Character/AI/CharacterDialogueBubbleFactory.cs` | `CharacterDialogueBubbleFactory` | `Create` | 21 | Runtime object creation | object creation fits this factory/context boundary. |
| 1 | `Assets/Scripts/Character/AI/CharacterSocialMemoryFactory.cs` | `CharacterSocialMemoryFactory` | `GetOrAdd` | 19 | Manual resolver/injection | manual resolver use is acceptable only in composition roots/factories. |
| 1 | `Assets/Scripts/Character/CharacterSpawner.cs` | `CharacterSpawner` | `EnsureRuntimeState` | 62 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/CharacterSpawner.cs` | `CharacterSpawner` | `TrySpawnCharacter` | 117 | Manual resolver/injection | manual resolver use should move to a composition root or factory. |
| 1 | `Assets/Scripts/Character/CharacterSpawnObjectFactory.cs` | `CharacterSpawnObjectFactory` | `Create` | 22 | Runtime object creation | object creation fits this factory/context boundary. |
| 1 | `Assets/Scripts/Character/CharacterSpawnObjectFactory.cs` | `CharacterSpawnObjectFactory` | `Inject` | 32 | Manual resolver/injection | manual resolver use is acceptable only in composition roots/factories. |
| 1 | `Assets/Scripts/Character/Core/CharacterStats.cs` | `CharacterStats` | `ChangeStatByTick` | 91 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Core/CharacterVisual.cs` | `CharacterVisual` | `CreateVisualRoot` | 98 | Runtime object creation | object creation fits this factory/context boundary. |
| 1 | `Assets/Scripts/Character/Core/CharacterVisual.cs` | `CharacterVisual` | `HideForTraversal` | 214 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Core/CharacterVisual.cs` | `CharacterVisual` | `StopTraversalVisibilityTimer` | 245 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |
| 1 | `Assets/Scripts/Character/Core/CharacterVisual.cs` | `CharacterVisual` | `RestoreTraversalVisibilityNow` | 256 | UI type dependency | non-UI code depends on UI types. |
| 1 | `Assets/Scripts/Character/Core/CharacterVisual.cs` | `CharacterVisual` | `CaptureCanvasVisibility` | 332 | UI type dependency | non-UI code depends on UI types. |
| 1 | `Assets/Scripts/Character/Core/CharacterVisual.cs` | `CharacterVisual` | `SetTraversalVisible` | 346 | UI type dependency | non-UI code depends on UI types. |
| 1 | `Assets/Scripts/Character/Core/CharacterVisual.cs` | `CanvasVisibilityState` | `CanvasVisibilityState` | 432 | UI type dependency | non-UI code depends on UI types. |
| 1 | `Assets/Scripts/Character/Core/CharacterVisualRootFactory.cs` | `CharacterVisualRootFactory` | `EnsureVisualRoot` | 12 | Runtime object creation | object creation fits this factory/context boundary. |
| 1 | `Assets/Scripts/Character/Core/OwnerCharacterFactory.cs` | `OwnerCharacterFactory` | `CreateOwner` | 33 | Runtime object creation | object creation fits this factory/context boundary. |
| 1 | `Assets/Scripts/Character/Core/OwnerCharacterFactory.cs` | `OwnerCharacterFactory` | `InjectOwnerRuntime` | 96 | Manual resolver/injection | manual resolver use is acceptable only in composition roots/factories. |
| 1 | `Assets/Scripts/Character/UI/CharacterFeedbackBubbleFactory.cs` | `CharacterFeedbackBubbleFactory` | `GetOrAdd` | 19 | Manual resolver/injection | manual resolver use is acceptable only in composition roots/factories. |
| 1 | `Assets/Scripts/Character/UI/CharacterFeedbackBubbleViewFactory.cs` | `CharacterFeedbackBubbleViewFactory` | `CreateTextView` | 59 | Runtime object creation | object creation fits this factory/context boundary. |
| 1 | `Assets/Scripts/Character/UI/CharacterFloatingIcon.cs` | `GameManagerFloatingIconFeedbackService` | `ResolveGameManager` | 60 | Scene query coupling | scene discovery should be isolated behind providers/build callbacks. |
| 1 | `Assets/Scripts/Character/UI/OwnerSelectionOptionButtonFactory.cs` | `IOwnerSelectionOptionButtonFactory` | `Create` | 9 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanel.cs` | `StaffWorkPriorityPanel` | `EnsureLayout` | 94 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanel.cs` | `StaffWorkPriorityPanel` | `ResolveHost` | 155 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanel.cs` | `StaffWorkPriorityPanel` | `BuildHeader` | 248 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanel.cs` | `StaffWorkPriorityPanel` | `BuildWorkerRow` | 272 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanel.cs` | `StaffWorkPriorityPanel` | `CreateRow` | 308 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanel.cs` | `StaffWorkPriorityPanel` | `CreateLabelCell` | 321 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanel.cs` | `StaffWorkPriorityPanel` | `CreatePriorityCell` | 346 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanel.cs` | `StaffWorkPriorityPanel` | `CreateCellObject` | 374 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanel.cs` | `StaffWorkPriorityPanel` | `AddCellText` | 384 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanelUiFactory.cs` | `IStaffWorkPriorityPanelUiFactory` | `EnsureRectTransform` | 8 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanelUiFactory.cs` | `IStaffWorkPriorityPanelUiFactory` | `AddImage` | 10 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanelUiFactory.cs` | `IStaffWorkPriorityPanelUiFactory` | `AddButton` | 17 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanelUiFactory.cs` | `IStaffWorkPriorityPanelUiFactory` | `AddText` | 18 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanelUiFactory.cs` | `StaffWorkPriorityPanelUiFactory` | `EnsureRectTransform` | 33 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanelUiFactory.cs` | `StaffWorkPriorityPanelUiFactory` | `AddImage` | 53 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanelUiFactory.cs` | `StaffWorkPriorityPanelUiFactory` | `AddButton` | 117 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/UI/StaffWorkPriorityPanelUiFactory.cs` | `StaffWorkPriorityPanelUiFactory` | `AddText` | 124 | UI type dependency | UI type usage fits this UI/factory context. |
| 1 | `Assets/Scripts/Character/Work/StaffWorkforceQueryService.cs` | `StaffWorkforceRuntimeQueryService` | `FindActiveWorkers` | 19 | Scene query coupling | scene discovery should be isolated behind providers/build callbacks. |
| 1 | `Assets/Scripts/Character/Work/WorkCommandHandler.cs` | `WorkCommandHandler` | `SuppressPriorityTarget` | 138 | Coroutine/timing orchestration | timed workflow may deserve a service if it coordinates domain state. |

## Project-Owned Function Inventory

### `Assets/DataManager.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `DataManager` | `DataManager` | `ctor` | `public` | None |
| 19 | `DataManager` | `BuildAllData` | `void` | `private` | None |

### `Assets/Scripts/Buildings/BuildableObject.cs`

- Functions detected: 34
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 60 | `BuildableObject` | `Start` | `void` | `public virtual` | None |
| 64 | `BuildableObject` | `ConstructBuildableObject` | `void` | `public` | None |
| 81 | `BuildableObject` | `SetGrid` | `void` | `public virtual` | None |
| 86 | `BuildableObject` | `Initialization` | `void` | `public virtual` | None |
| 103 | `BuildableObject` | `GetMovementWorldPosition` | `Vector3` | `public virtual` | None |
| 119 | `BuildableObject` | `TryGetNearestWalkableWorldPosition` | `bool` | `public` | None |
| 138 | `BuildableObject` | `DestroySelf` | `void` | `public` | None |
| 153 | `BuildableObject` | `SetDamaged` | `void` | `public` | None |
| 164 | `BuildableObject` | `SetFacilityLevel` | `void` | `public` | None |
| 176 | `BuildableObject` | `SupportsFacilityRole` | `bool` | `public` | None |
| 181 | `BuildableObject` | `SupportsWork` | `bool` | `public` | None |
| 186 | `BuildableObject` | `CanVisit` | `bool` | `public` | None |
| 232 | `BuildableObject` | `TryBeginUse` | `bool` | `public` | None |
| 246 | `BuildableObject` | `EndUse` | `void` | `public` | None |
| 255 | `BuildableObject` | `TryReserveVisit` | `bool` | `public` | None |
| 274 | `BuildableObject` | `RefreshVisitReservation` | `void` | `public` | None |
| 284 | `BuildableObject` | `ReleaseVisitReservation` | `void` | `public` | None |
| 297 | `BuildableObject` | `TryReserveWorker` | `bool` | `public` | None |
| 319 | `BuildableObject` | `RefreshWorkerReservation` | `void` | `public` | None |
| 330 | `BuildableObject` | `HasWorkerReservationForOther` | `bool` | `public` | None |
| 336 | `BuildableObject` | `ReleaseWorkerReservation` | `void` | `public` | None |
| 348 | `BuildableObject` | `CanAssignWork` | `bool` | `public` | None |
| 380 | `BuildableObject` | `GetActiveVisitReservationCountExcept` | `int` | `private` | None |
| 394 | `BuildableObject` | `PruneExpiredVisitReservations` | `void` | `private` | None |
| 429 | `BuildableObject` | `PruneExpiredWorkerReservation` | `void` | `private` | None |
| 446 | `BuildableObject` | `TryFindNearestWalkableCell` | `bool` | `private` | None |
| 482 | `BuildableObject` | `GetWorkUrgency` | `float` | `public virtual` | None |
| 516 | `BuildableObject` | `isVisitable` | `bool` | `public virtual` | None |
| 521 | `BuildableObject` | `OnMouseDown` | `void` | `private` | None |
| 531 | `BuildableObject` | `MarkFacilityDynamicStateDirty` | `void` | `protected` | None |
| 536 | `BuildableObject` | `ResolveFacilityCandidateCache` | `IFacilityCandidateCache` | `private` | None |
| 542 | `BuildableObject` | `ResolveRoomFacilityPolicy` | `IRoomFacilityPolicy` | `private` | None |
| 548 | `BuildableObject` | `ResolveBlueprintResearchWorkService` | `IBlueprintResearchWorkService` | `private` | None |
| 554 | `BuildableObject` | `RequireWorldInfoClickSelector` | `IWorldInfoClickSelector` | `private` | None |

### `Assets/Scripts/Buildings/BuildingManagementSummaryQuery.cs`

- Functions detected: 16
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `BuildingManagementSummary` | `BuildingManagementSummary` | `ctor` | `public` | None |
| 28 | `ShopManagementSummary` | `ShopManagementSummary` | `ctor` | `public` | None |
| 42 | `WarehouseManagementSummary` | `WarehouseManagementSummary` | `ctor` | `public` | None |
| 72 | `IBuildingManagementSummaryService` | `CaptureBuildings` | `BuildingManagementSummary` | `-` | None |
| 73 | `IBuildingManagementSummaryService` | `CaptureShops` | `ShopManagementSummary` | `-` | None |
| 74 | `IBuildingManagementSummaryService` | `CaptureWarehouses` | `WarehouseManagementSummary` | `-` | None |
| 87 | `BuildingManagementSummaryRuntimeSource` | `BuildingManagementSummaryRuntimeSource` | `ctor` | `public` | None |
| 101 | `BuildingManagementSummaryService` | `BuildingManagementSummaryService` | `ctor` | `public` | None |
| 106 | `BuildingManagementSummaryService` | `CaptureBuildings` | `BuildingManagementSummary` | `public` | None |
| 111 | `BuildingManagementSummaryService` | `CaptureShops` | `ShopManagementSummary` | `public` | None |
| 116 | `BuildingManagementSummaryService` | `CaptureWarehouses` | `WarehouseManagementSummary` | `public` | None |
| 126 | `BuildingManagementSummaryService` | `IsValidWarehouse` | `bool` | `private static` | None |
| 137 | `BuildingManagementSummaryQuery` | `FromBuildings` | `BuildingManagementSummary` | `public static` | None |
| 156 | `BuildingManagementSummaryQuery` | `FromShops` | `ShopManagementSummary` | `public static` | None |
| 167 | `BuildingManagementSummaryQuery` | `FromWarehouses` | `WarehouseManagementSummary` | `public static` | None |
| 189 | `BuildingManagementSummaryQuery` | `IsValidWarehouse` | `bool` | `private static` | None |

### `Assets/Scripts/Buildings/BuildingSummaryFormatter.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `BuildingSummaryPresentation` | `BuildingSummaryPresentation` | `ctor` | `public` | None |
| 17 | `IBuildingSummaryFormatter` | `Format` | `BuildingSummaryPresentation` | `-` | None |
| 23 | `BuildingSummaryFormatter` | `BuildingSummaryFormatter` | `ctor` | `public` | None |
| 29 | `BuildingSummaryFormatter` | `Format` | `BuildingSummaryPresentation` | `public` | None |
| 40 | `BuildingSummaryFormatter` | `FormatStockText` | `string` | `private static` | None |

### `Assets/Scripts/Buildings/Door.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `Door` | `Initialization` | `void` | `public override` | None |
| 11 | `Door` | `OnTriggerEnter2D` | `void` | `private` | None |
| 22 | `Door` | `OnTriggerExit2D` | `void` | `private` | None |

### `Assets/Scripts/Buildings/Facility.cs`

- Functions detected: 10
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 12 | `Facility` | `Initialization` | `void` | `public override` | None |
| 23 | `Facility` | `Interact` | `IEnumerator` | `public` | Coroutine/timing orchestration |
| 56 | `Facility` | `CanAssignWorker` | `bool` | `public` | None |
| 90 | `Facility` | `AllocateWorker` | `IEnumerator` | `public` | None |
| 109 | `Facility` | `DeallocateWorker` | `void` | `public` | None |
| 128 | `Facility` | `PruneInvalidWorker` | `void` | `private` | None |
| 150 | `Facility` | `GetRandomUsePosition` | `Vector2` | `private` | None |
| 164 | `Facility` | `GetWorkerPosition` | `Vector2` | `private` | None |
| 175 | `Facility` | `ApplyUseRecovery` | `void` | `private` | None |
| 259 | `Facility` | `objectNameOrDefault` | `string` | `private` | None |

### `Assets/Scripts/Buildings/FacilityCrimeRiskUtility.cs`

- Functions detected: 12
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 14 | `FacilityCrimeRiskContext` | `FacilityCrimeRiskContext` | `ctor` | `public` | None |
| 40 | `FacilityCrimeRiskUtility` | `CalculateShopliftingChance` | `float` | `public static` | None |
| 58 | `FacilityCrimeRiskUtility` | `CalculateOperationalRisk` | `float` | `public static` | None |
| 87 | `FacilityCrimeRiskUtility` | `GetSupervisionPressure` | `float` | `private static` | None |
| 98 | `FacilityCrimeRiskUtility` | `GetNeedPressure` | `float` | `private static` | None |
| 125 | `FacilityCrimeRiskUtility` | `GetCrowdPressure` | `float` | `private static` | None |
| 138 | `FacilityCrimeRiskUtility` | `GetCartValuePressure` | `float` | `private static` | None |
| 150 | `FacilityCrimeRiskUtility` | `GetFacilityStatePressure` | `float` | `private static` | None |
| 166 | `FacilityCrimeRiskUtility` | `GetActorIncidentMultiplier` | `float` | `private static` | None |
| 180 | `FacilityCrimeRiskUtility` | `GetStat` | `float` | `private static` | None |
| 185 | `FacilityCrimeRiskUtility` | `LowStatPressure` | `float` | `private static` | None |
| 191 | `FacilityCrimeRiskUtility` | `ShouldTriggerCrime` | `bool` | `public static` | None |

### `Assets/Scripts/Buildings/Hallway.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/Buildings/IInteractable.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `IInteractable` | `Interact` | `IEnumerator` | `public` | None |
| 10 | `IGridMovementHandler` | `Traverse` | `IEnumerator` | `public` | None |
| 27 | `IWorkableFacility` | `CanAssignWorker` | `bool` | `public` | None |
| 28 | `IWorkableFacility` | `AllocateWorker` | `IEnumerator` | `public` | None |
| 29 | `IWorkableFacility` | `DeallocateWorker` | `void` | `public` | None |

### `Assets/Scripts/Buildings/Shop.cs`

- Functions detected: 48
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 51 | `Shop` | `Initialization` | `void` | `public override` | None |
| 61 | `Shop` | `ConstructShop` | `void` | `public` | None |
| 81 | `Shop` | `Interact` | `IEnumerator` | `public` | Coroutine/timing orchestration |
| 199 | `Shop` | `CreatesRevenueFor` | `bool` | `public static` | None |
| 204 | `Shop` | `IsInternalStaffUse` | `bool` | `public static` | None |
| 209 | `Shop` | `CanServeCustomer` | `bool` | `public` | None |
| 231 | `Shop` | `GetCheckoutCrimeChance` | `float` | `public` | None |
| 236 | `Shop` | `GetCheckoutCrimeChance` | `float` | `public` | None |
| 250 | `Shop` | `TryResolveCheckoutCrime` | `bool` | `private` | None |
| 284 | `Shop` | `BuildCrimeDetail` | `string` | `private` | None |
| 293 | `Shop` | `GetCartValue` | `int` | `private static` | None |
| 312 | `Shop` | `RunCheckoutService` | `IEnumerator` | `private` | Coroutine/timing orchestration |
| 343 | `Shop` | `WaitForServingWorker` | `IEnumerator` | `private` | Coroutine/timing orchestration |
| 375 | `Shop` | `ShouldWaitForServingWorker` | `bool` | `private` | None |
| 384 | `Shop` | `GetStock` | `List<Stock>` | `public` | None |
| 389 | `Shop` | `GetStock` | `List<Stock>` | `private` | None |
| 402 | `Shop` | `CreatePricedStock` | `Stock` | `private` | None |
| 412 | `Shop` | `GetPriceMultiplier` | `float` | `private` | None |
| 418 | `Shop` | `GetRemainingStockAfterSelection` | `int` | `private static` | None |
| 432 | `Shop` | `GetStockCount` | `int` | `public` | None |
| 445 | `Shop` | `RestockFrom` | `int` | `public` | None |
| 507 | `Shop` | `TryFindRestockSource` | `bool` | `public` | None |
| 569 | `Shop` | `ReceiveRestock` | `int` | `public` | None |
| 592 | `Shop` | `HasRestockSupply` | `bool` | `public` | None |
| 632 | `Shop` | `DebugClearStock` | `void` | `public` | None |
| 638 | `Shop` | `GetRandomBuyPos` | `Vector2` | `public` | None |
| 645 | `Shop` | `GetCheckoutWorldPosition` | `Vector2` | `private` | None |
| 651 | `Shop` | `GetWorkUrgency` | `float` | `public override` | None |
| 665 | `Shop` | `isVisitable` | `bool` | `public override` | None |
| 670 | `Shop` | `CanAssignWorker` | `bool` | `public` | None |
| 704 | `Shop` | `AllocateWorker` | `IEnumerator` | `public` | None |
| 725 | `Shop` | `DeallocateWorker` | `void` | `public` | None |
| 745 | `Shop` | `PruneInvalidWorker` | `void` | `private` | None |
| 769 | `Shop` | `RequiresServingWorker` | `bool` | `private` | None |
| 776 | `Shop` | `FillStock` | `void` | `private` | None |
| 798 | `Shop` | `AddRemainStock` | `void` | `private` | None |
| 814 | `Shop` | `CreateRemainStock` | `RemainStock` | `private` | None |
| 824 | `Shop` | `GetStockCategory` | `StockCategory` | `private` | None |
| 829 | `Shop` | `GetConfiguredStockCapacity` | `int` | `private` | None |
| 848 | `Shop` | `objectNameOrDefault` | `string` | `private` | None |
| 855 | `Shop` | `TryInitializeStock` | `bool` | `private` | None |
| 889 | `Shop` | `EnsureStockInitialized` | `void` | `private` | None |
| 894 | `Shop` | `TryResolveGameData` | `bool` | `private` | None |
| 914 | `Shop` | `RequireStockCatalog` | `IShopStockCatalog` | `private` | None |
| 920 | `Shop` | `RequireFloatingNumberFeedbackService` | `IFloatingNumberFeedbackService` | `private` | None |
| 926 | `Shop` | `RequireWorkforceReplanService` | `IWorkforceReplanService` | `private` | None |
| 940 | `RemainStock` | `RemainStock` | `ctor` | `public` | None |
| 953 | `Stock` | `Stock` | `ctor` | `public` | None |

### `Assets/Scripts/Buildings/SO/BuildingSO.cs`

- Functions detected: 6
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 75 | `FacilityData` | `SupportsRole` | `bool` | `public` | None |
| 80 | `FacilityData` | `SupportsWork` | `bool` | `public` | None |
| 101 | `GridBuildingPlacement` | `GridBuildingPlacement` | `ctor` | `public` | None |
| 117 | `GridBuildingPlacement` | `GetGridPosList` | `List<Vector2Int>` | `public` | None |
| 184 | `BuildingSO` | `GetGridPosList` | `List<Vector2Int>` | `public` | None |
| 189 | `BuildingSO` | `GetDraggable` | `bool` | `public` | None |

### `Assets/Scripts/Buildings/SO/Conditions/ConditionNeedMoney.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `ConditionNeedMoney` | `OnBuild` | `void` | `public` | None |
| 15 | `ConditionNeedMoney` | `IsSatisfy` | `bool` | `public` | None |

### `Assets/Scripts/Buildings/SO/Conditions/ConditionNeedToConnect.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 12 | `ConditionNeedToConnect` | `OnBuild` | `void` | `public` | None |
| 14 | `ConditionNeedToConnect` | `IsSatisfy` | `bool` | `public` | None |

### `Assets/Scripts/Buildings/SO/Conditions/IBuildingCondition.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IBuildingCondition` | `IsSatisfy` | `bool` | `public` | None |
| 11 | `IBuildingCondition` | `OnBuild` | `void` | `public` | None |
| 18 | `BuildingConditionContext` | `BuildingConditionContext` | `ctor` | `public` | None |

### `Assets/Scripts/Buildings/SO/OnBuyItemSO.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `OnBuyItemSO` | `Onbuy` | `void` | `public virtual` | None |

### `Assets/Scripts/Buildings/SO/SaleItem.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/Buildings/SO/StatChange.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `StatChange` | `Onbuy` | `void` | `public override` | None |

### `Assets/Scripts/Buildings/SO/StockInfo.cs`

- Functions detected: 26
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 26 | `WarehouseInventory` | `WarehouseInventory` | `ctor` | `public` | None |
| 30 | `WarehouseInventory` | `WarehouseInventory` | `ctor` | `public` | None |
| 35 | `WarehouseInventory` | `CreateSeeded` | `WarehouseInventory` | `public static` | None |
| 51 | `WarehouseInventory` | `GetStock` | `int` | `public` | None |
| 58 | `WarehouseInventory` | `HasStock` | `bool` | `public` | None |
| 63 | `WarehouseInventory` | `CanStore` | `bool` | `public` | None |
| 68 | `WarehouseInventory` | `AddStock` | `int` | `public` | None |
| 77 | `WarehouseInventory` | `Deposit` | `int` | `public` | None |
| 87 | `WarehouseInventory` | `Withdraw` | `int` | `public` | None |
| 107 | `StockDeliveryOffer` | `StockDeliveryOffer` | `ctor` | `public` | None |
| 125 | `StockProductionRule` | `StockProductionRule` | `ctor` | `public` | None |
| 144 | `StockSupplyResult` | `StockSupplyResult` | `ctor` | `public` | None |
| 162 | `StockSupplyResult` | `ToSummaryText` | `string` | `public` | None |
| 179 | `StockSupplyEvent` | `StockSupplyEvent` | `ctor` | `public` | None |
| 186 | `StockSupplyEvent` | `Trigger` | `void` | `public static` | None |
| 196 | `StockSupplyService` | `CreateDailyDeliveryOffers` | `IReadOnlyList<StockDeliveryOffer>` | `public static` | None |
| 207 | `StockSupplyService` | `CreateDailyDeliveryOffers` | `IReadOnlyList<StockDeliveryOffer>` | `public static` | None |
| 233 | `StockSupplyService` | `TryPurchaseDelivery` | `bool` | `public static` | None |
| 282 | `StockSupplyService` | `GrantReward` | `bool` | `public static` | None |
| 318 | `StockSupplyService` | `RunInternalProduction` | `List<StockSupplyResult>` | `public static` | None |
| 334 | `StockSupplyService` | `GetRemainingCapacity` | `int` | `public static` | None |
| 349 | `StockSupplyService` | `CreateOffer` | `StockDeliveryOffer` | `private static` | None |
| 362 | `StockSupplyService` | `CanDepositAll` | `bool` | `private static` | None |
| 368 | `StockSupplyService` | `DepositToWarehouses` | `int` | `private static` | None |
| 385 | `StockSupplyService` | `GetValidWarehouses` | `IEnumerable<IWarehouseFacility>` | `private static` | None |
| 392 | `StockSupplyService` | `Fail` | `StockSupplyResult` | `private static` | None |

### `Assets/Scripts/Buildings/Stair.cs`

- Functions detected: 4
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 11 | `Stair` | `Traverse` | `IEnumerator` | `public` | Coroutine/timing orchestration |
| 44 | `Stair` | `Interact` | `IEnumerator` | `public` | None |
| 71 | `Stair` | `GetHiddenTravelDelay` | `float` | `private` | None |
| 78 | `Stair` | `GetFloorCenterAnchor` | `Vector3` | `private` | None |

### `Assets/Scripts/Buildings/UI/UIBuildingInfo.cs`

- Functions detected: 8
- Judgment: UI type usage fits this UI/factory context.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 29 | `UIBuildingInfo` | `ConstructUIBuildingInfo` | `void` | `public` | None |
| 40 | `UIBuildingInfo` | `Awake` | `void` | `private` | Lifecycle contains dependency/workflow logic, UI type dependency |
| 47 | `UIBuildingInfo` | `Start` | `void` | `-` | None |
| 52 | `UIBuildingInfo` | `DisplayBuildingInfo` | `void` | `public` | UI type dependency |
| 90 | `UIBuildingInfo` | `OpenDispaly` | `void` | `public` | None |
| 100 | `UIBuildingInfo` | `CloseDispaly` | `void` | `public` | None |
| 111 | `UIBuildingInfo` | `ResolveBuildingLookup` | `IBuildingDefinitionLookup` | `private` | None |
| 117 | `UIBuildingInfo` | `ResolveTouchGuard` | `IUiTouchGuardService` | `private` | None |

### `Assets/Scripts/Buildings/UI/UIBuildingSelectButton.cs`

- Functions detected: 10
- Judgment: UI type usage fits this UI/factory context.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 17 | `UIBuildingSelectButton` | `Construct` | `void` | `public` | None |
| 24 | `UIBuildingSelectButton` | `Initialization` | `void` | `public` | None |
| 33 | `UIBuildingSelectButton` | `Initialization` | `void` | `public` | None |
| 39 | `UIBuildingSelectButton` | `OnClick` | `void` | `public` | None |
| 46 | `UIBuildingSelectButton` | `ActiveDestroyMode` | `void` | `public` | None |
| 51 | `UIBuildingSelectButton` | `ApplyButtonSize` | `void` | `private` | UI type dependency |
| 59 | `UIBuildingSelectButton` | `SetIcon` | `void` | `private` | UI type dependency |
| 78 | `UIBuildingSelectButton` | `ResolveIconImage` | `Image` | `private` | UI type dependency |
| 87 | `UIBuildingSelectButton` | `GetFittedIconSize` | `Vector2` | `private` | None |
| 101 | `UIBuildingSelectButton` | `RequireBuildingController` | `DungeonStoryGridBuildingController` | `private` | None |

### `Assets/Scripts/Character/Ability/AbilityMove.cs`

- Functions detected: 36
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 16 | `AbilityMove` | `ConstructAbilityMove` | `void` | `public` | None |
| 28 | `AbilityMove` | `Awake` | `void` | `protected override` | None |
| 33 | `AbilityMove` | `Initializtion` | `void` | `public override` | None |
| 44 | `AbilityMove` | `MoveByPath` | `IEnumerator` | `public` | None |
| 63 | `AbilityMove` | `MoveByStep` | `IEnumerator` | `public` | None |
| 88 | `AbilityMove` | `Move2GridPosition` | `IEnumerator` | `public` | None |
| 97 | `AbilityMove` | `StartExitDungeon` | `void` | `public` | Coroutine/timing orchestration |
| 116 | `AbilityMove` | `StartEnterDungeon` | `void` | `public` | Coroutine/timing orchestration |
| 126 | `AbilityMove` | `StartMoveByCurrentActionPath` | `void` | `public` | None |
| 132 | `AbilityMove` | `StartWait` | `void` | `public` | None |
| 138 | `AbilityMove` | `StartIdleWander` | `bool` | `public` | None |
| 152 | `AbilityMove` | `CancelActiveMovement` | `void` | `public` | Coroutine/timing orchestration |
| 161 | `AbilityMove` | `StartTrackedActionMovement` | `void` | `private` | Coroutine/timing orchestration |
| 167 | `AbilityMove` | `TrackActionMovement` | `IEnumerator` | `private` | None |
| 173 | `AbilityMove` | `GetCurrentAction` | `AIAction` | `private` | None |
| 180 | `AbilityMove` | `MoveByCurrentActionPath` | `IEnumerator` | `private` | None |
| 200 | `AbilityMove` | `MoveByPathThenWait` | `IEnumerator` | `private` | None |
| 220 | `AbilityMove` | `TryFindIdleWanderPath` | `bool` | `public` | None |
| 254 | `AbilityMove` | `IsPlainIdleWalkable` | `bool` | `private` | None |
| 265 | `AbilityMove` | `IsSupportedIdleWanderPath` | `bool` | `private static` | None |
| 293 | `AbilityMove` | `IsSupportedVerticalMovementStep` | `bool` | `private static` | None |
| 300 | `AbilityMove` | `GetIdleSearchResult` | `GridPathSearchResult` | `private` | None |
| 317 | `AbilityMove` | `SnapToGridRowIfWalkable` | `void` | `private` | None |
| 331 | `AbilityMove` | `MoveByCurrentBestActionPath` | `IEnumerator` | `public` | None |
| 336 | `AbilityMove` | `MoveByActionPath` | `IEnumerator` | `private` | None |
| 349 | `AbilityMove` | `RefreshCurrentActionReservation` | `void` | `private` | None |
| 359 | `AbilityMove` | `IsActionMovementCancelled` | `bool` | `private` | None |
| 368 | `AbilityMove` | `WaitForAiAction` | `IEnumerator` | `private` | None |
| 383 | `AbilityMove` | `WaitForAiActionDelay` | `IEnumerator` | `private` | None |
| 398 | `AbilityMove` | `EnterDungeon` | `IEnumerator` | `private` | None |
| 423 | `AbilityMove` | `ExitDungeon` | `IEnumerator` | `private` | Coroutine/timing orchestration |
| 476 | `AbilityMove` | `TryResolveSpawner` | `bool` | `private` | None |
| 486 | `AbilityMove` | `RequireAiSchedulingService` | `ICharacterAiSchedulingService` | `private` | None |
| 492 | `AbilityMove` | `Move2PosByTime` | `IEnumerator` | `public` | None |
| 505 | `AbilityMove` | `Move2PosBySpeed` | `IEnumerator` | `public` | None |
| 554 | `AbilityMove` | `UpdateFacingForMovement` | `void` | `private` | None |

### `Assets/Scripts/Character/Ability/AbilitySchedule.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 10 | `AbilitySchedule` | `Awake` | `void` | `protected override` | None |
| 20 | `AbilitySchedule` | `CheckTime` | `void` | `private` | None |
| 24 | `AbilitySchedule` | `OnDisable` | `void` | `private` | None |

### `Assets/Scripts/Character/Ability/AbilityShopping.cs`

- Functions detected: 22
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 21 | `AbilityShopping` | `ConstructAbilityShopping` | `void` | `public` | None |
| 32 | `AbilityShopping` | `Initializtion` | `void` | `public override` | None |
| 43 | `AbilityShopping` | `DetermineBuyingItem` | `Stock` | `public` | None |
| 73 | `AbilityShopping` | `CanPay` | `bool` | `public` | None |
| 79 | `AbilityShopping` | `CanBuyFrom` | `bool` | `public` | None |
| 120 | `AbilityShopping` | `GetAffordabilityScore` | `float` | `public` | None |
| 147 | `AbilityShopping` | `StartSopping` | `void` | `public` | Coroutine/timing orchestration |
| 153 | `AbilityShopping` | `Shopping` | `IEnumerator` | `private` | None |
| 200 | `AbilityShopping` | `RegisterVisit` | `void` | `public` | None |
| 213 | `AbilityShopping` | `RegisterLookAround` | `void` | `public` | None |
| 218 | `AbilityShopping` | `BeginOffDutyVisitCycle` | `void` | `public` | None |
| 225 | `AbilityShopping` | `IsOffDutyStaffVisitor` | `bool` | `public` | None |
| 234 | `AbilityShopping` | `GetInterestRoles` | `FacilityRole` | `public` | None |
| 239 | `AbilityShopping` | `CanLookAround` | `bool` | `public` | None |
| 246 | `AbilityShopping` | `ShouldExitDungeon` | `bool` | `public` | None |
| 253 | `AbilityShopping` | `ShouldEndVisitCycle` | `bool` | `public` | None |
| 258 | `AbilityShopping` | `IsThereVisitableBuilding` | `bool` | `public` | None |
| 291 | `AbilityShopping` | `FindShop` | `BuildableObject` | `public` | None |
| 334 | `AbilityShopping` | `CanVisitBuilding` | `bool` | `private` | None |
| 348 | `AbilityShopping` | `GetShoppingCount` | `int` | `public` | None |
| 355 | `AbilityShopping` | `BuyItem` | `IEnumerator` | `public` | Coroutine/timing orchestration |
| 374 | `AbilityShopping` | `RequireFloatingIconFeedbackService` | `IFloatingIconFeedbackService` | `private` | None |

### `Assets/Scripts/Character/Ability/AbilityWork.cs`

- Functions detected: 61
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 127 | `AbilityWork` | `Awake` | `void` | `protected override` | None |
| 134 | `AbilityWork` | `ConstructAbilityWork` | `void` | `public` | None |
| 154 | `AbilityWork` | `Initializtion` | `void` | `public override` | None |
| 171 | `AbilityWork` | `EnsureWorkReferences` | `void` | `public` | None |
| 176 | `AbilityWork` | `TryAssignShop` | `bool` | `public` | None |
| 181 | `AbilityWork` | `TryAssignWork` | `bool` | `public` | None |
| 186 | `AbilityWork` | `TryGetBestWorkCandidate` | `bool` | `public` | None |
| 200 | `AbilityWork` | `GetWorkUtilityScore` | `float` | `public` | None |
| 205 | `AbilityWork` | `TryGetLastRejectedWorkCandidate` | `bool` | `public` | None |
| 213 | `AbilityWork` | `StartWorking` | `void` | `public` | Coroutine/timing orchestration |
| 247 | `AbilityWork` | `TryAssignWorkTarget` | `bool` | `public` | None |
| 262 | `AbilityWork` | `TrySetPriorityWorkTarget` | `bool` | `public` | None |
| 267 | `AbilityWork` | `TrySetPriorityWorkTarget` | `bool` | `public` | None |
| 276 | `AbilityWork` | `TrySetPrioritySuppressTarget` | `bool` | `public` | None |
| 284 | `AbilityWork` | `TryGetPrioritySuppressDestination` | `bool` | `public` | None |
| 289 | `AbilityWork` | `ClearPriorityWorkTarget` | `void` | `public` | None |
| 294 | `AbilityWork` | `SetWorkPriority` | `void` | `public` | None |
| 319 | `AbilityWork` | `ShouldUseRestProtection` | `bool` | `public` | None |
| 324 | `AbilityWork` | `CanStartWorkAction` | `bool` | `public` | None |
| 329 | `AbilityWork` | `CanStartWorkAction` | `bool` | `public` | None |
| 335 | `AbilityWork` | `CanContinueCurrentWork` | `bool` | `public` | None |
| 340 | `AbilityWork` | `ShouldInterruptCurrentWork` | `bool` | `public` | None |
| 345 | `AbilityWork` | `ShouldThrottleRoutineWork` | `bool` | `public` | None |
| 354 | `AbilityWork` | `BeginRoutineWorkCooldown` | `void` | `public` | None |
| 368 | `AbilityWork` | `ShouldTakeOffDuty` | `bool` | `public` | None |
| 373 | `AbilityWork` | `ShouldReturnToWork` | `bool` | `public` | None |
| 378 | `AbilityWork` | `BeginOffDuty` | `void` | `public` | None |
| 383 | `AbilityWork` | `PrepareForExpedition` | `void` | `public` | None |
| 388 | `AbilityWork` | `SetDutyState` | `void` | `public` | None |
| 393 | `AbilityWork` | `RecoverOffDuty` | `void` | `public` | None |
| 404 | `AbilityWork` | `ApplyWorkFatigueTick` | `void` | `public` | None |
| 409 | `AbilityWork` | `CheckActionWork` | `IEnumerator` | `public` | None |
| 414 | `AbilityWork` | `CheckSchedule` | `void` | `public` | None |
| 422 | `AbilityWork` | `AssignWork` | `void` | `internal` | None |
| 428 | `AbilityWork` | `ReleaseAssignedWorkTarget` | `void` | `internal` | None |
| 433 | `AbilityWork` | `StopAssignedWork` | `void` | `internal` | None |
| 438 | `AbilityWork` | `StopAssignedWorkFromAi` | `void` | `internal` | None |
| 443 | `AbilityWork` | `StopAssignedWork` | `void` | `private` | None |
| 477 | `AbilityWork` | `IsActiveWorkRun` | `bool` | `internal` | None |
| 482 | `AbilityWork` | `CanContinueWorkRun` | `bool` | `internal` | None |
| 487 | `AbilityWork` | `StartCheckActionWork` | `Coroutine` | `internal` | Coroutine/timing orchestration |
| 499 | `AbilityWork` | `ClearActiveWorkRoutine` | `void` | `internal` | None |
| 507 | `AbilityWork` | `ClearActiveWorkCheckRoutine` | `void` | `internal` | None |
| 515 | `AbilityWork` | `HasUrgentPriorityTarget` | `bool` | `internal` | None |
| 520 | `AbilityWork` | `MarkFacilityDynamicStateDirty` | `void` | `internal` | None |
| 525 | `AbilityWork` | `CanExecuteSuppressCommand` | `bool` | `private` | None |
| 531 | `AbilityWork` | `OnDisable` | `void` | `private` | None |
| 536 | `AbilityWork` | `OnEnable` | `void` | `private` | None |
| 541 | `AbilityWork` | `TryBindScheduleEvents` | `void` | `private` | None |
| 564 | `AbilityWork` | `UnbindScheduleEvents` | `void` | `private` | None |
| 574 | `AbilityWork` | `EnsureWorkModules` | `void` | `private` | None |
| 590 | `AbilityWork` | `BeginWorkRun` | `int` | `private` | None |
| 597 | `AbilityWork` | `InvalidateActiveWorkRun` | `void` | `private` | None |
| 603 | `AbilityWork` | `StopActiveWorkRoutines` | `void` | `private` | None |
| 609 | `AbilityWork` | `StopActiveWorkRoutine` | `void` | `private` | Coroutine/timing orchestration |
| 620 | `AbilityWork` | `StopActiveWorkCheckRoutine` | `void` | `private` | Coroutine/timing orchestration |
| 631 | `AbilityWork` | `ExecuteRestockWork` | `IEnumerator` | `private` | None |
| 636 | `AbilityWork` | `ExecuteRepairWork` | `IEnumerator` | `private` | None |
| 641 | `AbilityWork` | `ExecuteResearchWork` | `IEnumerator` | `private` | None |
| 646 | `AbilityWork` | `SuppressPriorityTarget` | `IEnumerator` | `private` | None |
| 651 | `AbilityWork` | `TryEvaluateWorkTarget` | `bool` | `private` | None |

### `Assets/Scripts/Character/Ability/CharacterAbility.cs`

- Functions detected: 7
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 19 | `CharacterAbility` | `ConstructCharacterAbility` | `void` | `public` | None |
| 27 | `CharacterAbility` | `Awake` | `void` | `protected virtual` | None |
| 33 | `CharacterAbility` | `Start` | `void` | `protected virtual` | None |
| 38 | `CharacterAbility` | `CacheCommonReferences` | `void` | `protected` | None |
| 59 | `CharacterAbility` | `TryGetGrid` | `bool` | `protected` | None |
| 70 | `CharacterAbility` | `CacheSplitComponents` | `void` | `private` | None |
| 79 | `CharacterAbility` | `Initializtion` | `void` | `public virtual` | None |

### `Assets/Scripts/Character/Ability/CharacterVisitPolicy.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 19 | `CharacterVisitPolicy` | `GetInterestRoles` | `FacilityRole` | `public static` | None |
| 26 | `CharacterVisitPolicy` | `IsStaffPurchaseShop` | `bool` | `public static` | None |
| 33 | `CharacterVisitPolicy` | `CanVisitBuilding` | `bool` | `public static` | None |

### `Assets/Scripts/Character/AI/Action/AIActionSet.cs`

- Functions detected: 16
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 19 | `AIActionSet` | `CanStart` | `bool` | `public virtual` | None |
| 24 | `AIActionSet` | `AdjustScore` | `float` | `public virtual` | None |
| 29 | `AIActionSet` | `CanStartWithContext` | `bool` | `public virtual` | None |
| 65 | `AIActionSet` | `CanStartWithFailure` | `bool` | `public virtual` | None |
| 82 | `AIActionSet` | `CanContinue` | `bool` | `public virtual` | None |
| 88 | `AIActionSet` | `CanInterrupt` | `bool` | `public virtual` | None |
| 94 | `AIActionSet` | `Execute` | `void` | `public virtual` | None |
| 98 | `AIActionSet` | `OnStop` | `void` | `public virtual` | None |
| 102 | `AIActionSet` | `GetDestinationCandidates` | `IReadOnlyList<BuildableObject>` | `public virtual` | None |
| 112 | `AIActionSet` | `SelectDestination` | `BuildableObject` | `public virtual` | None |
| 121 | `AIActionSet` | `TryResolveDestination` | `bool` | `public virtual` | None |
| 152 | `AIActionSet` | `TryResolveDestinationWithFailure` | `bool` | `public virtual` | None |
| 171 | `AIActionSet` | `TryReserveDestination` | `bool` | `public virtual` | None |
| 180 | `AIActionSet` | `RefreshDestinationReservation` | `void` | `public virtual` | None |
| 186 | `AIActionSet` | `ReleaseDestinationReservation` | `void` | `public virtual` | None |
| 192 | `AIActionSet` | `GetDestination` | `BuildableObject` | `public virtual` | None |

### `Assets/Scripts/Character/AI/Action/AIEat.cs`

- Functions detected: 9
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `AIEat` | `CanStart` | `bool` | `public override` | None |
| 12 | `AIEat` | `Execute` | `void` | `public override` | None |
| 17 | `AIEat` | `GetDestinationCandidates` | `IReadOnlyList<BuildableObject>` | `public override` | None |
| 29 | `AIEat` | `SelectDestination` | `BuildableObject` | `public override` | None |
| 44 | `AIEat` | `TryResolveDestination` | `bool` | `public override` | None |
| 65 | `AIEat` | `TryReserveDestination` | `bool` | `public override` | None |
| 88 | `AIEat` | `RefreshDestinationReservation` | `void` | `public override` | None |
| 93 | `AIEat` | `ReleaseDestinationReservation` | `void` | `public override` | None |
| 98 | `AIEat` | `CanUseVisitorAction` | `bool` | `private static` | None |

### `Assets/Scripts/Character/AI/Action/AIExitDungeon.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `AIExitDungeon` | `CanStart` | `bool` | `public override` | None |
| 16 | `AIExitDungeon` | `Execute` | `void` | `public override` | None |

### `Assets/Scripts/Character/AI/Action/AIFacilityRoleAction.cs`

- Functions detected: 9
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 15 | `AIFacilityRoleAction` | `CanStart` | `bool` | `public override` | None |
| 20 | `AIFacilityRoleAction` | `Execute` | `void` | `public override` | None |
| 25 | `AIFacilityRoleAction` | `GetDestinationCandidates` | `IReadOnlyList<BuildableObject>` | `public override` | None |
| 37 | `AIFacilityRoleAction` | `SelectDestination` | `BuildableObject` | `public override` | None |
| 52 | `AIFacilityRoleAction` | `TryResolveDestination` | `bool` | `public override` | None |
| 73 | `AIFacilityRoleAction` | `TryReserveDestination` | `bool` | `public override` | None |
| 96 | `AIFacilityRoleAction` | `RefreshDestinationReservation` | `void` | `public override` | None |
| 101 | `AIFacilityRoleAction` | `ReleaseDestinationReservation` | `void` | `public override` | None |
| 106 | `AIFacilityRoleAction` | `CanUseVisitorAction` | `bool` | `private static` | None |

### `Assets/Scripts/Character/AI/Action/AILookAround.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 13 | `AILookAround` | `CanStart` | `bool` | `public override` | None |
| 18 | `AILookAround` | `Execute` | `void` | `public override` | None |
| 47 | `AILookAround` | `GetDestinationCandidates` | `IReadOnlyList<BuildableObject>` | `public override` | None |
| 70 | `AILookAround` | `SelectDestination` | `BuildableObject` | `public override` | None |
| 82 | `AILookAround` | `CanUseVisitLookAround` | `bool` | `public static` | None |

### `Assets/Scripts/Character/AI/Action/AIRest.cs`

- Functions detected: 9
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `AIRest` | `CanStart` | `bool` | `public override` | None |
| 12 | `AIRest` | `Execute` | `void` | `public override` | None |
| 17 | `AIRest` | `GetDestinationCandidates` | `IReadOnlyList<BuildableObject>` | `public override` | None |
| 32 | `AIRest` | `SelectDestination` | `BuildableObject` | `public override` | None |
| 47 | `AIRest` | `TryResolveDestination` | `bool` | `public override` | None |
| 68 | `AIRest` | `TryReserveDestination` | `bool` | `public override` | None |
| 91 | `AIRest` | `RefreshDestinationReservation` | `void` | `public override` | None |
| 96 | `AIRest` | `ReleaseDestinationReservation` | `void` | `public override` | None |
| 101 | `AIRest` | `CanUseVisitorAction` | `bool` | `private static` | None |

### `Assets/Scripts/Character/AI/Action/AIShopping.cs`

- Functions detected: 9
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `AIShopping` | `CanStart` | `bool` | `public override` | None |
| 12 | `AIShopping` | `Execute` | `void` | `public override` | None |
| 17 | `AIShopping` | `GetDestinationCandidates` | `IReadOnlyList<BuildableObject>` | `public override` | None |
| 33 | `AIShopping` | `SelectDestination` | `BuildableObject` | `public override` | None |
| 57 | `AIShopping` | `TryResolveDestination` | `bool` | `public override` | None |
| 86 | `AIShopping` | `TryReserveDestination` | `bool` | `public override` | None |
| 109 | `AIShopping` | `RefreshDestinationReservation` | `void` | `public override` | None |
| 114 | `AIShopping` | `ReleaseDestinationReservation` | `void` | `public override` | None |
| 119 | `AIShopping` | `CanUseVisitorAction` | `bool` | `private static` | None |

### `Assets/Scripts/Character/AI/Action/AIWait.cs`

- Functions detected: 4
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 13 | `AIWait` | `AdjustScore` | `float` | `public override` | None |
| 47 | `AIWait` | `CanStart` | `bool` | `public override` | None |
| 52 | `AIWait` | `Execute` | `void` | `public override` | None |
| 95 | `AIWait` | `HasOffDutyVisitCandidate` | `bool` | `private static` | None |

### `Assets/Scripts/Character/AI/Action/AIWork.cs`

- Functions detected: 11
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 15 | `AIWork` | `AdjustScore` | `float` | `public override` | None |
| 35 | `AIWork` | `CanStart` | `bool` | `public override` | None |
| 48 | `AIWork` | `CanContinue` | `bool` | `public override` | None |
| 56 | `AIWork` | `CanInterrupt` | `bool` | `public override` | None |
| 64 | `AIWork` | `Execute` | `void` | `public override` | None |
| 81 | `AIWork` | `OnStop` | `void` | `public override` | None |
| 89 | `AIWork` | `TryReserveDestination` | `bool` | `public override` | None |
| 112 | `AIWork` | `RefreshDestinationReservation` | `void` | `public override` | None |
| 117 | `AIWork` | `ReleaseDestinationReservation` | `void` | `public override` | None |
| 122 | `AIWork` | `GetDestinationCandidates` | `IReadOnlyList<BuildableObject>` | `public override` | None |
| 142 | `AIWork` | `CanHandleSuppressCommand` | `bool` | `private` | None |

### `Assets/Scripts/Character/AI/AIActionFailure.cs`

- Functions detected: 8
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 27 | `AIActionFailure` | `AIActionFailure` | `ctor` | `public` | None |
| 40 | `AIActionFailure` | `Create` | `AIActionFailure` | `public static` | None |
| 48 | `AIActionFailure` | `FromReason` | `AIActionFailure` | `public static` | None |
| 56 | `AIActionFailure` | `ClassifyKind` | `AIActionFailureKind` | `public static` | None |
| 126 | `AIActionFailure` | `ToString` | `string` | `public override` | None |
| 131 | `AIActionFailure` | `ContainsAny` | `bool` | `private static` | None |
| 144 | `AIActionFailure` | `GetDefaultReason` | `string` | `private static` | None |
| 171 | `AIActionDebugCandidate` | `AIActionDebugCandidate` | `ctor` | `public` | None |

### `Assets/Scripts/Character/AI/AIBrain.cs`

- Functions detected: 60
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 49 | `AIBrain` | `ConstructAIBrain` | `void` | `public` | None |
| 77 | `AIBrain` | `Initializtion` | `void` | `public override` | None |
| 93 | `AIBrain` | `UseOwnerWorkActions` | `void` | `public` | None |
| 101 | `AIBrain` | `NormalizeConfiguredActions` | `void` | `private` | None |
| 124 | `AIBrain` | `EnsureVisitorActions` | `void` | `private` | None |
| 144 | `AIBrain` | `AddRequiredAction` | `void` | `private` | None |
| 151 | `AIBrain` | `AddRequiredFacilityRoleAction` | `void` | `private` | None |
| 169 | `AIBrain` | `RequireActionCatalog` | `ICharacterAiActionAssetCatalog` | `private` | None |
| 175 | `AIBrain` | `RequireAiSchedulingService` | `ICharacterAiSchedulingService` | `private` | None |
| 181 | `AIBrain` | `RequireFacilityScoringContext` | `FacilityScoringContext` | `public` | None |
| 192 | `AIBrain` | `RequireFacilityCandidateCache` | `IFacilityCandidateCache` | `public` | None |
| 198 | `AIBrain` | `RequireFacilityLookup` | `ICharacterAiFacilityLookup` | `public` | None |
| 204 | `AIBrain` | `RequireJobGiverCatalog` | `ICharacterAiJobGiverCatalog` | `public` | None |
| 210 | `AIBrain` | `RequireDecisionPipeline` | `ICharacterAiDecisionPipeline` | `public` | None |
| 216 | `AIBrain` | `DecideAction` | `bool` | `public` | None |
| 255 | `AIBrain` | `TryCommitActionCandidate` | `bool` | `public` | None |
| 314 | `AIBrain` | `TryFindBestScoredAction` | `bool` | `public` | None |
| 391 | `AIBrain` | `DecideActionByScoreThenDestination` | `bool` | `private` | None |
| 435 | `AIBrain` | `TryFindHighestScoredAction` | `bool` | `private` | None |
| 459 | `AIBrain` | `GetSelectionScore` | `float` | `private` | None |
| 484 | `AIBrain` | `GetPathSearch` | `GridPathSearchResult` | `public` | None |
| 506 | `AIBrain` | `TryGetRuntimeGrid` | `bool` | `public` | None |
| 511 | `AIBrain` | `ClearPathSearchCache` | `void` | `public` | None |
| 516 | `AIBrain` | `RequestImmediateReplan` | `void` | `public` | None |
| 545 | `AIBrain` | `ClearSelectedActionForIdle` | `void` | `public` | None |
| 559 | `AIBrain` | `NotifyActionStarted` | `void` | `public` | None |
| 571 | `AIBrain` | `ShouldStopCurrentAction` | `bool` | `public` | None |
| 592 | `AIBrain` | `ShouldStopCurrentActionForReplan` | `bool` | `public` | None |
| 637 | `AIBrain` | `CanContinueCurrentAction` | `bool` | `public` | None |
| 687 | `AIBrain` | `StopCurrentActionForReplan` | `bool` | `public` | None |
| 719 | `AIBrain` | `TryUseQueuedAction` | `bool` | `private` | None |
| 743 | `AIBrain` | `TryFindInterruptAction` | `bool` | `private` | None |
| 805 | `AIBrain` | `CanConsiderAction` | `bool` | `private` | None |
| 812 | `AIBrain` | `CanConsiderAction` | `bool` | `private` | None |
| 857 | `AIBrain` | `CanUseAction` | `bool` | `private` | None |
| 864 | `AIBrain` | `CanUseAction` | `bool` | `private` | None |
| 881 | `AIBrain` | `IsActionCoolingDown` | `bool` | `private` | None |
| 889 | `AIBrain` | `RecordActionFailure` | `void` | `private` | None |
| 894 | `AIBrain` | `RecordActionFailure` | `void` | `private` | None |
| 906 | `AIBrain` | `RecordNoActionFailure` | `void` | `private` | None |
| 918 | `AIBrain` | `GetActionLabel` | `string` | `private static` | None |
| 925 | `AIBrain` | `RefineActionFailure` | `AIActionFailure` | `private` | None |
| 944 | `AIBrain` | `RecordCandidateDebug` | `void` | `private` | None |
| 959 | `AIBrain` | `RememberCandidateFailure` | `void` | `private` | None |
| 979 | `AIBrain` | `ReleaseFinishedActionReservation` | `void` | `private` | None |
| 989 | `AIBrain` | `ShouldCooldownCandidateFailure` | `bool` | `private static` | None |
| 997 | `AIBrain` | `GetFailureDebugPriority` | `int` | `private static` | None |
| 1015 | `AIBrain` | `GetDebugSummary` | `string` | `public` | None |
| 1051 | `AIBrain` | `GetDestinationLabel` | `string` | `private static` | None |
| 1066 | `AIBrain` | `GetDebugHash` | `int` | `public` | None |
| 1084 | `AIBrain` | `MarkDebugDirty` | `void` | `private` | None |
| 1121 | `AIAction` | `MarkStarted` | `void` | `public` | None |
| 1126 | `AIAction` | `CalculateScore` | `float` | `public` | None |
| 1167 | `AIAction` | `SetDestinationWithFailure` | `bool` | `public` | None |
| 1209 | `AIAction` | `RefreshReservation` | `void` | `public` | None |
| 1219 | `AIAction` | `ReleaseReservation` | `void` | `public` | None |
| 1231 | `AIAction` | `TryReserveResolvedDestination` | `bool` | `private` | None |
| 1253 | `AIAction` | `ResolvePathPlan` | `bool` | `private` | None |
| 1260 | `AIAction` | `ResolvePathPlan` | `bool` | `private` | None |
| 1280 | `AIAction` | `IsCharacterAtDestination` | `bool` | `private static` | None |

### `Assets/Scripts/Character/AI/AiDirectorContextAggregator.cs`

- Functions detected: 8
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 17 | `AiDirectorContextSummary` | `ToPromptText` | `string` | `public` | None |
| 41 | `AiDirectorContextAggregator` | `Build` | `AiDirectorContextSummary` | `public static` | None |
| 60 | `AiDirectorContextAggregator` | `AverageCondition` | `float` | `private static` | None |
| 86 | `AiDirectorContextAggregator` | `CountStockShortages` | `int` | `private static` | None |
| 108 | `AiDirectorContextAggregator` | `GetTopQueuedFacilities` | `string[]` | `private static` | None |
| 119 | `AiDirectorContextAggregator` | `GetRepeatedFailureReasons` | `string[]` | `private static` | None |
| 146 | `AiDirectorContextAggregator` | `GetRecentEvents` | `string[]` | `private static` | None |
| 160 | `AiDirectorContextAggregator` | `GetBuildingLabel` | `string` | `private static` | None |

### `Assets/Scripts/Character/AI/AiDirectorContextSceneQuery.cs`

- Functions detected: 4
- Judgment: scene discovery should be isolated behind providers/build callbacks.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `AiDirectorContextSceneSnapshot` | `AiDirectorContextSceneSnapshot` | `ctor` | `public` | None |
| 18 | `IAiDirectorContextSceneQuery` | `Capture` | `AiDirectorContextSceneSnapshot` | `-` | None |
| 24 | `AiDirectorContextSceneQuery` | `AiDirectorContextSceneQuery` | `ctor` | `public` | None |
| 30 | `AiDirectorContextSceneQuery` | `Capture` | `AiDirectorContextSceneSnapshot` | `public` | Scene query coupling |

### `Assets/Scripts/Character/AI/AiDirectorRuntime.cs`

- Functions detected: 29
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 51 | `AiDirectorRuntime` | `ConstructAiDirectorRuntime` | `void` | `public` | None |
| 68 | `AiDirectorRuntime` | `SetWarningLogsSuppressedForDebug` | `void` | `public` | None |
| 73 | `AiDirectorRuntime` | `Update` | `void` | `private` | None |
| 84 | `AiDirectorRuntime` | `EvaluateOneActor` | `void` | `public` | None |
| 108 | `AiDirectorRuntime` | `ShouldRequestMoodImpulse` | `bool` | `public` | None |
| 131 | `AiDirectorRuntime` | `ShouldRequestMacroGoal` | `bool` | `public` | None |
| 161 | `AiDirectorRuntime` | `HasUrgentDirectorReason` | `bool` | `private` | None |
| 171 | `AiDirectorRuntime` | `HasRepeatedFailureReason` | `bool` | `private` | None |
| 180 | `AiDirectorRuntime` | `HasRoutineDirectorReason` | `bool` | `private` | None |
| 195 | `AiDirectorRuntime` | `GetNextRoutineMacroGoalTime` | `float` | `private` | None |
| 207 | `AiDirectorRuntime` | `ScheduleNextRoutineMacroGoal` | `void` | `private` | None |
| 217 | `AiDirectorRuntime` | `GetNextMoodImpulseTime` | `float` | `private` | None |
| 229 | `AiDirectorRuntime` | `ScheduleNextMoodImpulse` | `void` | `private` | None |
| 239 | `AiDirectorRuntime` | `RequestMoodImpulse` | `bool` | `public` | None |
| 265 | `AiDirectorRuntime` | `RequestMacroGoal` | `bool` | `public` | None |
| 291 | `AiDirectorRuntime` | `OnMacroGoalResult` | `void` | `private` | None |
| 322 | `AiDirectorRuntime` | `OnMoodImpulseResult` | `void` | `private` | None |
| 368 | `AiDirectorRuntime` | `TryGetLlmRuntime` | `bool` | `private` | None |
| 385 | `AiDirectorRuntime` | `RequireContextSceneQuery` | `IAiDirectorContextSceneQuery` | `private` | None |
| 395 | `AiDirectorRuntime` | `RequireAiSchedulingService` | `ICharacterAiSchedulingService` | `private` | None |
| 405 | `AiDirectorRuntime` | `RequireFacilityLookup` | `ICharacterAiFacilityLookup` | `private` | None |
| 415 | `AiDirectorRuntime` | `BuildMacroGoalPrompt` | `string` | `private` | None |
| 450 | `AiDirectorRuntime` | `BuildMoodImpulsePrompt` | `string` | `private` | None |
| 493 | `AiDirectorRuntime` | `ValidateMoodImpulseTarget` | `bool` | `private` | None |
| 520 | `AiDirectorRuntime` | `ApplyMoodImpulseSideEffects` | `void` | `private` | None |
| 547 | `AiDirectorRuntime` | `TryCreateMacroGoalFromMoodImpulse` | `bool` | `private` | None |
| 585 | `AiDirectorRuntime` | `GetMood` | `float` | `private static` | None |
| 590 | `AiDirectorRuntime` | `GetCondition` | `float` | `private static` | None |
| 600 | `AiDirectorRuntime` | `LogWarningIfAllowed` | `void` | `private` | None |

### `Assets/Scripts/Character/AI/CharacterAiBehaviorTasks.cs`

- Functions detected: 56
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `CharacterAiBehaviorTaskServices` | `TryGetDecisionPipeline` | `bool` | `public static` | None |
| 27 | `HasCriticalState` | `OnAwake` | `void` | `public override` | None |
| 32 | `HasCriticalState` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 47 | `HasMacroGoal` | `OnAwake` | `void` | `public override` | None |
| 52 | `HasMacroGoal` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 67 | `HasMacroGoalType` | `OnAwake` | `void` | `public override` | None |
| 72 | `HasMacroGoalType` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 88 | `ClearMacroGoal` | `OnAwake` | `void` | `public override` | None |
| 93 | `ClearMacroGoal` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 111 | `RunComplainMacroGoal` | `OnAwake` | `void` | `public override` | None |
| 116 | `RunComplainMacroGoal` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 137 | `RunAvoidFacilityMacroGoal` | `OnAwake` | `void` | `public override` | None |
| 142 | `RunAvoidFacilityMacroGoal` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 163 | `RunExitDungeonMacroGoal` | `OnAwake` | `void` | `public override` | None |
| 168 | `RunExitDungeonMacroGoal` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 189 | `RunVandalizeMacroGoal` | `OnAwake` | `void` | `public override` | None |
| 194 | `RunVandalizeMacroGoal` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 215 | `RunMacroGoalDecision` | `OnAwake` | `void` | `public override` | None |
| 220 | `RunMacroGoalDecision` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 238 | `RunCriticalState` | `OnAwake` | `void` | `public override` | None |
| 243 | `RunCriticalState` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 261 | `HasContinuableCurrentAction` | `OnAwake` | `void` | `public override` | None |
| 266 | `HasContinuableCurrentAction` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 280 | `ContinueCurrentAction` | `OnAwake` | `void` | `public override` | None |
| 285 | `ContinueCurrentAction` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 303 | `ShouldStopCurrentAction` | `OnAwake` | `void` | `public override` | None |
| 308 | `ShouldStopCurrentAction` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 322 | `StopCurrentActionForReplan` | `OnAwake` | `void` | `public override` | None |
| 327 | `StopCurrentActionForReplan` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 346 | `CharacterRoutineGroupBranchBase` | `OnAwake` | `void` | `public override` | None |
| 351 | `CharacterRoutineGroupBranchBase` | `GetPriority` | `float` | `public override` | None |
| 356 | `CharacterRoutineGroupBranchBase` | `GetUtility` | `float` | `public override` | None |
| 361 | `CharacterRoutineGroupBranchBase` | `EvaluatePriority` | `float` | `private` | None |
| 407 | `CharacterJobGiverBranchBase` | `OnAwake` | `void` | `public override` | None |
| 412 | `CharacterJobGiverBranchBase` | `GetUtility` | `float` | `public override` | None |
| 442 | `CharacterJobGiverBranchBase` | `ResolveJobGiver` | `CharacterAiJobGiver` | `private` | None |
| 513 | `AmbientIdleJobGiverBranch` | `OnAwake` | `void` | `public override` | None |
| 518 | `AmbientIdleJobGiverBranch` | `GetUtility` | `float` | `public override` | None |
| 544 | `SelectCharacterActionBase` | `OnAwake` | `void` | `public override` | None |
| 549 | `SelectCharacterActionBase` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 566 | `SelectCharacterActionBase` | `MatchesAction` | `bool` | `protected abstract` | None |
| 574 | `SelectExitDungeonAction` | `MatchesAction` | `bool` | `protected override` | None |
| 581 | `SelectEatAction` | `MatchesAction` | `bool` | `protected override` | None |
| 588 | `SelectRestAction` | `MatchesAction` | `bool` | `protected override` | None |
| 595 | `SelectToiletAction` | `MatchesAction` | `bool` | `protected override` | None |
| 606 | `SelectHygieneAction` | `MatchesAction` | `bool` | `protected override` | None |
| 617 | `SelectWorkAction` | `MatchesAction` | `bool` | `protected override` | None |
| 624 | `SelectShoppingAction` | `MatchesAction` | `bool` | `protected override` | None |
| 631 | `SelectLookAroundAction` | `MatchesAction` | `bool` | `protected override` | None |
| 638 | `SelectWaitAction` | `MatchesAction` | `bool` | `protected override` | None |
| 645 | `RunSelectedCharacterAction` | `OnAwake` | `void` | `public override` | None |
| 650 | `RunSelectedCharacterAction` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 668 | `RunIdleBehavior` | `OnAwake` | `void` | `public override` | None |
| 673 | `RunIdleBehavior` | `OnUpdate` | `TaskStatus` | `public override` | None |
| 691 | `EmitContextBubble` | `OnAwake` | `void` | `public override` | None |
| 696 | `EmitContextBubble` | `OnUpdate` | `TaskStatus` | `public override` | None |

### `Assets/Scripts/Character/AI/CharacterAiDecisionPipeline.cs`

- Functions detected: 49
- Judgment: scene discovery should be isolated behind providers/build callbacks.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `CharacterAiDecisionTickResult` | `CharacterAiDecisionTickResult` | `ctor` | `public` | None |
| 27 | `ICharacterAiDecisionPipeline` | `HasCriticalState` | `bool` | `-` | None |
| 28 | `ICharacterAiDecisionPipeline` | `RunCritical` | `CharacterAiDecisionTickResult` | `-` | None |
| 29 | `ICharacterAiDecisionPipeline` | `HasMacroGoal` | `bool` | `-` | None |
| 30 | `ICharacterAiDecisionPipeline` | `HasContinuableCurrentAction` | `bool` | `-` | None |
| 31 | `ICharacterAiDecisionPipeline` | `ShouldStopCurrentActionForReplan` | `bool` | `-` | None |
| 32 | `ICharacterAiDecisionPipeline` | `ContinueCurrentAction` | `CharacterAiDecisionTickResult` | `-` | None |
| 33 | `ICharacterAiDecisionPipeline` | `StopCurrentActionForReplan` | `CharacterAiDecisionTickResult` | `-` | None |
| 34 | `ICharacterAiDecisionPipeline` | `SelectJobGiverAction` | `CharacterAiDecisionTickResult` | `-` | None |
| 35 | `ICharacterAiDecisionPipeline` | `RunSelectedAction` | `CharacterAiDecisionTickResult` | `-` | None |
| 39 | `ICharacterAiDecisionPipeline` | `RunMacroGoalDecision` | `CharacterAiDecisionTickResult` | `-` | None |
| 40 | `ICharacterAiDecisionPipeline` | `RunIdleBehavior` | `CharacterAiDecisionTickResult` | `-` | None |
| 41 | `ICharacterAiDecisionPipeline` | `HasMacroGoalType` | `bool` | `-` | None |
| 42 | `ICharacterAiDecisionPipeline` | `ClearContinueMacro` | `CharacterAiDecisionTickResult` | `-` | None |
| 43 | `ICharacterAiDecisionPipeline` | `RunComplainMacro` | `CharacterAiDecisionTickResult` | `-` | None |
| 44 | `ICharacterAiDecisionPipeline` | `ApplyAvoidFacility` | `CharacterAiDecisionTickResult` | `-` | None |
| 45 | `ICharacterAiDecisionPipeline` | `RunExitDungeonMacro` | `CharacterAiDecisionTickResult` | `-` | None |
| 46 | `ICharacterAiDecisionPipeline` | `RunVandalizeMacro` | `CharacterAiDecisionTickResult` | `-` | None |
| 51 | `ICharacterAiFacilityLookup` | `FindFacility` | `BuildableObject` | `-` | None |
| 57 | `CharacterAiFacilityLookup` | `CharacterAiFacilityLookup` | `ctor` | `public` | None |
| 63 | `CharacterAiFacilityLookup` | `FindFacility` | `BuildableObject` | `public` | Scene query coupling |
| 81 | `CharacterAiDecisionPipeline` | `HasCriticalState` | `bool` | `public` | None |
| 89 | `CharacterAiDecisionPipeline` | `RunCritical` | `CharacterAiDecisionTickResult` | `public` | None |
| 105 | `CharacterAiDecisionPipeline` | `HasMacroGoal` | `bool` | `public` | None |
| 110 | `CharacterAiDecisionPipeline` | `HasContinuableCurrentAction` | `bool` | `public` | None |
| 117 | `CharacterAiDecisionPipeline` | `ShouldStopCurrentActionForReplan` | `bool` | `public` | None |
| 124 | `CharacterAiDecisionPipeline` | `ContinueCurrentAction` | `CharacterAiDecisionTickResult` | `public` | None |
| 153 | `CharacterAiDecisionPipeline` | `StopCurrentActionForReplan` | `CharacterAiDecisionTickResult` | `public` | None |
| 182 | `CharacterAiDecisionPipeline` | `SelectJobGiverAction` | `CharacterAiDecisionTickResult` | `public` | None |
| 226 | `CharacterAiDecisionPipeline` | `RunSelectedAction` | `CharacterAiDecisionTickResult` | `public` | None |
| 259 | `CharacterAiDecisionPipeline` | `RunMacroGoalDecision` | `CharacterAiDecisionTickResult` | `public` | None |
| 320 | `CharacterAiDecisionPipeline` | `RunIdleBehavior` | `CharacterAiDecisionTickResult` | `public` | None |
| 356 | `CharacterAiDecisionPipeline` | `HasMacroGoalType` | `bool` | `public` | None |
| 365 | `CharacterAiDecisionPipeline` | `RunMacroJobGiverDecision` | `CharacterAiDecisionTickResult` | `private` | None |
| 449 | `CharacterAiDecisionPipeline` | `ClearContinueMacro` | `CharacterAiDecisionTickResult` | `public` | None |
| 461 | `CharacterAiDecisionPipeline` | `RunComplainMacro` | `CharacterAiDecisionTickResult` | `public` | None |
| 482 | `CharacterAiDecisionPipeline` | `ApplyAvoidFacility` | `CharacterAiDecisionTickResult` | `public` | None |
| 509 | `CharacterAiDecisionPipeline` | `RunExitDungeonMacro` | `CharacterAiDecisionTickResult` | `public` | None |
| 541 | `CharacterAiDecisionPipeline` | `RunVandalizeMacro` | `CharacterAiDecisionTickResult` | `public` | None |
| 578 | `CharacterAiDecisionPipeline` | `FindFacility` | `BuildableObject` | `private static` | None |
| 583 | `CharacterAiDecisionPipeline` | `MatchesFacility` | `bool` | `public static` | None |
| 608 | `CharacterAiDecisionPipeline` | `RequireFacilityLookup` | `ICharacterAiFacilityLookup` | `private static` | None |
| 619 | `CharacterAiDecisionPipeline` | `RequireJobGiverCatalog` | `ICharacterAiJobGiverCatalog` | `private static` | None |
| 630 | `CharacterAiDecisionPipeline` | `CanVandalize` | `bool` | `private static` | None |
| 666 | `CharacterAiDecisionPipeline` | `GetBuildingLabel` | `string` | `private static` | None |
| 678 | `CharacterAiDecisionPipeline` | `GetBranchForActionSet` | `CharacterAiBranch` | `public static` | None |
| 695 | `CharacterAiDecisionPipeline` | `GetActionLabel` | `string` | `public static` | None |
| 707 | `CharacterAiDecisionPipeline` | `TryPrepare` | `bool` | `private static` | None |
| 737 | `CharacterAiDecisionPipeline` | `Result` | `CharacterAiDecisionTickResult` | `private static` | None |

### `Assets/Scripts/Character/AI/CharacterAiJobGiver.cs`

- Functions detected: 39
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `CharacterAiActionCandidate` | `CharacterAiActionCandidate` | `ctor` | `public` | None |
| 28 | `CharacterAiJobCandidate` | `CharacterAiJobCandidate` | `ctor` | `public` | None |
| 59 | `CharacterAiJobGiver` | `TryEvaluate` | `bool` | `public` | None |
| 102 | `CharacterAiJobGiver` | `MatchesAction` | `bool` | `public abstract` | None |
| 104 | `CharacterAiJobGiver` | `GetDomainScore` | `float` | `protected abstract` | None |
| 106 | `CharacterAiJobGiver` | `CombineUtility` | `float` | `protected virtual` | None |
| 111 | `CharacterAiJobGiver` | `Need` | `float` | `public static` | None |
| 124 | `CharacterAiJobGiver` | `StatRatio` | `float` | `public static` | None |
| 137 | `CharacterAiJobGiver` | `InterestMultiplier` | `float` | `protected static` | None |
| 142 | `CharacterAiJobGiver` | `CreateRejected` | `CharacterAiJobCandidate` | `private` | None |
| 159 | `CharacterAiRoutinePriority` | `GetPriority` | `float` | `public static` | None |
| 188 | `CharacterAiRoutinePriority` | `GetSurvivalPriority` | `float` | `private static` | None |
| 226 | `CharacterAiRoutinePriority` | `GetDutyPriority` | `float` | `private static` | None |
| 247 | `CharacterAiRoutinePriority` | `GetLeisurePriority` | `float` | `private static` | None |
| 270 | `CharacterAiRoutinePriority` | `GetIdlePriority` | `float` | `private static` | None |
| 276 | `CharacterAiRoutinePriority` | `ReturnNoPriority` | `float` | `private static` | None |
| 282 | `CharacterAiRoutinePriority` | `GetSurvivalPressure` | `float` | `private static` | None |
| 293 | `CharacterAiRoutinePriority` | `CanUseLeisure` | `bool` | `private static` | None |
| 308 | `CharacterAiRoutinePriority` | `ShouldExitDungeon` | `bool` | `private static` | None |
| 321 | `ExitDungeonJobGiver` | `MatchesAction` | `bool` | `public override` | None |
| 322 | `ExitDungeonJobGiver` | `GetDomainScore` | `float` | `protected override` | None |
| 334 | `GetFoodJobGiver` | `MatchesAction` | `bool` | `public override` | None |
| 335 | `GetFoodJobGiver` | `GetDomainScore` | `float` | `protected override` | None |
| 348 | `RestJobGiver` | `MatchesAction` | `bool` | `public override` | None |
| 349 | `RestJobGiver` | `GetDomainScore` | `float` | `protected override` | None |
| 364 | `ToiletJobGiver` | `MatchesAction` | `bool` | `public override` | None |
| 370 | `ToiletJobGiver` | `GetDomainScore` | `float` | `protected override` | None |
| 383 | `HygieneJobGiver` | `MatchesAction` | `bool` | `public override` | None |
| 389 | `HygieneJobGiver` | `GetDomainScore` | `float` | `protected override` | None |
| 402 | `WorkJobGiver` | `MatchesAction` | `bool` | `public override` | None |
| 403 | `WorkJobGiver` | `GetDomainScore` | `float` | `protected override` | None |
| 435 | `ShoppingJobGiver` | `MatchesAction` | `bool` | `public override` | None |
| 436 | `ShoppingJobGiver` | `GetDomainScore` | `float` | `protected override` | None |
| 449 | `LookAroundJobGiver` | `MatchesAction` | `bool` | `public override` | None |
| 450 | `LookAroundJobGiver` | `GetDomainScore` | `float` | `protected override` | None |
| 471 | `WaitJobGiver` | `MatchesAction` | `bool` | `public override` | None |
| 472 | `WaitJobGiver` | `GetDomainScore` | `float` | `protected override` | None |
| 507 | `ICharacterAiJobGiverCatalog` | `Get` | `CharacterAiJobGiver` | `-` | None |
| 521 | `CharacterAiJobGiverCatalog` | `Get` | `CharacterAiJobGiver` | `public` | None |

### `Assets/Scripts/Character/AI/CharacterAiPersonality.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 12 | `CharacterAiPersonality` | `GetActionMultiplier` | `float` | `public` | None |
| 40 | `CharacterAiPersonality` | `ClampMultiplier` | `float` | `private static` | None |
| 49 | `CharacterAiPersonalityUtility` | `GetActionScoreMultiplier` | `float` | `public static` | None |

### `Assets/Scripts/Character/AI/CharacterAiScheduler.cs`

- Functions detected: 37
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 60 | `CharacterAiScheduler` | `Awake` | `void` | `private` | None |
| 65 | `CharacterAiScheduler` | `OnEnable` | `void` | `private` | None |
| 71 | `CharacterAiScheduler` | `Construct` | `void` | `public` | None |
| 90 | `CharacterAiScheduler` | `Start` | `void` | `private` | None |
| 95 | `CharacterAiScheduler` | `Update` | `void` | `private` | None |
| 100 | `CharacterAiScheduler` | `RegisterActor` | `void` | `public` | None |
| 110 | `CharacterAiScheduler` | `UnregisterActor` | `void` | `public` | None |
| 120 | `CharacterAiScheduler` | `RequestImmediateDecisionFor` | `void` | `public` | None |
| 131 | `CharacterAiScheduler` | `TryConsumePathSearchBudget` | `bool` | `public` | None |
| 141 | `CharacterAiScheduler` | `ShouldShowCharacterFeedbackFor` | `bool` | `public` | None |
| 151 | `CharacterAiScheduler` | `GetMovementFrameStrideFor` | `int` | `public` | None |
| 164 | `CharacterAiScheduler` | `RunManualTick` | `void` | `public` | None |
| 170 | `CharacterAiScheduler` | `ClearRegistrationsForDebug` | `void` | `public` | None |
| 181 | `CharacterAiScheduler` | `ResetPathSearchBudgetForDebugInstance` | `void` | `public` | None |
| 189 | `CharacterAiScheduler` | `ProcessAiBudget` | `void` | `private` | None |
| 267 | `CharacterAiScheduler` | `RefreshBehaviorDesignerVisualsForEditor` | `void` | `private` | None |
| 287 | `CharacterAiScheduler` | `RegisterExistingCharacters` | `void` | `private` | None |
| 295 | `CharacterAiScheduler` | `RegisterExistingCharactersIfInjected` | `void` | `private` | None |
| 303 | `CharacterAiScheduler` | `RequireSceneQuery` | `IDungeonSceneComponentQuery` | `private` | None |
| 313 | `CharacterAiScheduler` | `RegisterInternal` | `void` | `private` | None |
| 326 | `CharacterAiScheduler` | `TryRunScheduledDecision` | `bool` | `private` | None |
| 372 | `CharacterAiScheduler` | `ConfigureBehaviorManagerForManualTick` | `void` | `private static` | None |
| 386 | `CharacterAiScheduler` | `ConfigureCharacterBehaviorTree` | `BehaviorTree` | `private` | None |
| 391 | `CharacterAiScheduler` | `UnregisterInternal` | `void` | `private` | None |
| 403 | `CharacterAiScheduler` | `RemoveAt` | `void` | `private` | None |
| 419 | `CharacterAiScheduler` | `GetDecisionInterval` | `float` | `private` | None |
| 431 | `CharacterAiScheduler` | `IsHighDetailCharacter` | `bool` | `private` | None |
| 457 | `CharacterAiScheduler` | `TryConsumePathSearchBudgetInternal` | `bool` | `private` | None |
| 469 | `CharacterAiScheduler` | `BeginPathBudgetWindow` | `void` | `private` | None |
| 477 | `CharacterAiScheduler` | `ResetPathBudgetIfNeeded` | `void` | `private` | None |
| 488 | `CharacterAiScheduler` | `GetDecisionBudgetForFrame` | `int` | `private` | None |
| 494 | `CharacterAiScheduler` | `GetPathSearchBudgetForFrame` | `int` | `private` | None |
| 500 | `CharacterAiScheduler` | `EnsureAdaptiveBudgetsInitialized` | `void` | `private` | None |
| 508 | `CharacterAiScheduler` | `ResetAdaptiveBudgets` | `void` | `private` | None |
| 514 | `CharacterAiScheduler` | `UpdateAdaptiveBudgets` | `void` | `private` | None |
| 549 | `CharacterAiScheduler` | `RequireMainCameraProvider` | `IMainCameraProvider` | `private` | None |
| 555 | `CharacterAiScheduler` | `RequireBehaviorTreeConfigurator` | `ICharacterBehaviorTreeRuntimeConfigurator` | `private` | None |

### `Assets/Scripts/Character/AI/CharacterBehaviorTreeRuntimeConfigurator.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `ICharacterBehaviorTreeRuntimeConfigurator` | `Configure` | `BehaviorTree` | `-` | None |
| 12 | `CharacterBehaviorTreeRuntimeConfigurator` | `Configure` | `BehaviorTree` | `public` | None |

### `Assets/Scripts/Character/AI/CharacterBlackboard.cs`

- Functions detected: 41
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 85 | `CharacterMacroGoal` | `IsActive` | `bool` | `public` | None |
| 91 | `CharacterMacroGoal` | `IsEquivalentTo` | `bool` | `public` | None |
| 115 | `CharacterMoodImpulse` | `IsActive` | `bool` | `public` | None |
| 122 | `CharacterMoodImpulse` | `IsEquivalentTo` | `bool` | `public` | None |
| 199 | `CharacterBlackboard` | `Awake` | `void` | `private` | None |
| 204 | `CharacterBlackboard` | `Bind` | `void` | `public` | None |
| 218 | `CharacterBlackboard` | `BeginDecisionTrace` | `void` | `public` | None |
| 225 | `CharacterBlackboard` | `RecordBtStatus` | `void` | `public` | None |
| 233 | `CharacterBlackboard` | `SetIntent` | `void` | `public` | None |
| 244 | `CharacterBlackboard` | `RecordJobGiverUtility` | `void` | `public` | None |
| 258 | `CharacterBlackboard` | `RecordSelectedJobGiverUtility` | `void` | `public` | None |
| 264 | `CharacterBlackboard` | `RecordSelectedUtilitySummary` | `void` | `public` | None |
| 270 | `CharacterBlackboard` | `RecordRoutineGroupPriority` | `void` | `public` | None |
| 284 | `CharacterBlackboard` | `ClearJobGiverCandidateCache` | `void` | `public` | None |
| 290 | `CharacterBlackboard` | `CacheJobGiverCandidate` | `void` | `public` | None |
| 301 | `CharacterBlackboard` | `RemoveJobGiverCandidateCache` | `void` | `public` | None |
| 312 | `CharacterBlackboard` | `TryGetCachedJobGiverCandidate` | `bool` | `public` | None |
| 321 | `CharacterBlackboard` | `Commit` | `void` | `public` | None |
| 334 | `CharacterBlackboard` | `RefreshCommitment` | `void` | `public` | None |
| 347 | `CharacterBlackboard` | `TryGetCommitmentBonus` | `bool` | `public` | None |
| 375 | `CharacterBlackboard` | `CanBreakCommitment` | `bool` | `public` | None |
| 388 | `CharacterBlackboard` | `ClearCommitment` | `void` | `public` | None |
| 404 | `CharacterBlackboard` | `IsFacilityCoolingDown` | `bool` | `public` | None |
| 422 | `CharacterBlackboard` | `PutFacilityOnCooldown` | `void` | `public` | None |
| 436 | `CharacterBlackboard` | `ReportActionFailure` | `void` | `public` | None |
| 478 | `CharacterBlackboard` | `HasActiveMacroGoal` | `bool` | `public` | None |
| 494 | `CharacterBlackboard` | `HasActiveMoodImpulse` | `bool` | `public` | None |
| 516 | `CharacterBlackboard` | `SetMacroGoal` | `void` | `public` | None |
| 539 | `CharacterBlackboard` | `ClearMacroGoal` | `void` | `public` | None |
| 551 | `CharacterBlackboard` | `SetMoodImpulse` | `void` | `public` | None |
| 581 | `CharacterBlackboard` | `ClearMoodImpulse` | `void` | `public` | None |
| 594 | `CharacterBlackboard` | `GetRecentFailureCount` | `int` | `public` | None |
| 599 | `CharacterBlackboard` | `GetDebugSummary` | `string` | `public` | None |
| 621 | `CharacterBlackboard` | `BuildDecisionRouteSummary` | `string` | `private` | None |
| 652 | `CharacterBlackboard` | `AppendDecisionTrace` | `void` | `private` | None |
| 667 | `CharacterBlackboard` | `TrimTrace` | `string` | `private static` | None |
| 680 | `CharacterBlackboard` | `PruneFacilityCooldowns` | `void` | `private` | None |
| 710 | `CharacterBlackboard` | `ShouldCooldownFacility` | `bool` | `private static` | None |
| 719 | `CharacterBlackboard` | `IsDestinationInvalid` | `bool` | `private static` | None |
| 724 | `CharacterBlackboard` | `GetActionLabel` | `string` | `private static` | None |
| 736 | `CharacterBlackboard` | `GetBuildingLabel` | `string` | `private static` | None |

### `Assets/Scripts/Character/AI/CharacterDialogueBubbleFactory.cs`

- Functions detected: 3
- Judgment: object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `ICharacterDialogueBubbleFactory` | `Create` | `TextMeshPro` | `-` | None |
| 14 | `CharacterDialogueBubbleFactory` | `CharacterDialogueBubbleFactory` | `ctor` | `public` | None |
| 21 | `CharacterDialogueBubbleFactory` | `Create` | `TextMeshPro` | `public` | Runtime object creation |

### `Assets/Scripts/Character/AI/CharacterDialogueRuntime.cs`

- Functions detected: 17
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 31 | `CharacterDialogueRuntime` | `ConstructCharacterDialogueRuntime` | `void` | `public` | None |
| 45 | `CharacterDialogueRuntime` | `Awake` | `void` | `private` | None |
| 52 | `CharacterDialogueRuntime` | `OnEnable` | `void` | `private` | None |
| 61 | `CharacterDialogueRuntime` | `OnDisable` | `void` | `private` | None |
| 74 | `CharacterDialogueRuntime` | `LateUpdate` | `void` | `private` | None |
| 90 | `CharacterDialogueRuntime` | `ShowLine` | `void` | `public` | None |
| 105 | `CharacterDialogueRuntime` | `OnLogAdded` | `void` | `private` | None |
| 131 | `CharacterDialogueRuntime` | `OnBubbleResult` | `void` | `private` | None |
| 158 | `CharacterDialogueRuntime` | `TryGetLlmRuntime` | `bool` | `private` | None |
| 175 | `CharacterDialogueRuntime` | `RequireAiSchedulingService` | `ICharacterAiSchedulingService` | `private` | None |
| 181 | `CharacterDialogueRuntime` | `HideLine` | `void` | `private` | None |
| 192 | `CharacterDialogueRuntime` | `BuildPrompt` | `string` | `private` | None |
| 207 | `CharacterDialogueRuntime` | `ShouldRequestBubble` | `bool` | `private static` | None |
| 236 | `CharacterDialogueRuntime` | `EnsureText` | `void` | `private` | None |
| 246 | `CharacterDialogueRuntime` | `GetLocalOffset` | `Vector3` | `private` | None |
| 258 | `CharacterDialogueRuntime` | `RequireBubbleFactory` | `ICharacterDialogueBubbleFactory` | `private` | None |
| 265 | `CharacterDialogueRuntime` | `ContainsAny` | `bool` | `private static` | None |

### `Assets/Scripts/Character/AI/CharacterMoodImpulseUtility.cs`

- Functions detected: 17
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `CharacterMoodImpulseUtility` | `GetMood01` | `float` | `public static` | None |
| 13 | `CharacterMoodImpulseUtility` | `GetGoodMoodAdherenceMultiplier` | `float` | `public static` | None |
| 35 | `CharacterMoodImpulseUtility` | `ApplyRoutineBias` | `float` | `public static` | None |
| 121 | `CharacterMoodImpulseUtility` | `ApplyJobGiverBias` | `float` | `public static` | None |
| 158 | `CharacterMoodImpulseUtility` | `ShouldInterruptCurrentAction` | `bool` | `public static` | None |
| 206 | `CharacterMoodImpulseUtility` | `GetBranchForActionSet` | `CharacterAiBranch` | `public static` | None |
| 223 | `CharacterMoodImpulseUtility` | `MatchesBranch` | `bool` | `public static` | None |
| 264 | `CharacterMoodImpulseUtility` | `AppendReason` | `string` | `public static` | None |
| 276 | `CharacterMoodImpulseUtility` | `TryGetActiveImpulse` | `bool` | `private static` | None |
| 291 | `CharacterMoodImpulseUtility` | `IsOnDuty` | `bool` | `private static` | None |
| 297 | `CharacterMoodImpulseUtility` | `IsSurvivalImpulse` | `bool` | `private static` | None |
| 306 | `CharacterMoodImpulseUtility` | `IsLeisureImpulse` | `bool` | `private static` | None |
| 316 | `CharacterMoodImpulseUtility` | `IsTemperamentalImpulse` | `bool` | `private static` | None |
| 328 | `CharacterMoodImpulseUtility` | `MatchesFacilityTarget` | `bool` | `private static` | None |
| 353 | `CharacterMoodImpulseUtility` | `AppendImpulseReason` | `string` | `private static` | None |
| 358 | `CharacterMoodImpulseUtility` | `GetFacilityLabel` | `string` | `private static` | None |
| 370 | `CharacterMoodImpulseUtility` | `GetCondition` | `float` | `private static` | None |

### `Assets/Scripts/Character/AI/CharacterSocialMemory.cs`

- Functions detected: 27
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 45 | `SocialRumor` | `Clone` | `SocialRumor` | `public` | None |
| 57 | `SocialMemoryFloat` | `SocialMemoryFloat` | `ctor` | `public` | None |
| 82 | `CharacterSocialMemory` | `Awake` | `void` | `private` | None |
| 87 | `CharacterSocialMemory` | `Bind` | `void` | `public` | None |
| 92 | `CharacterSocialMemory` | `HearRumor` | `void` | `public` | None |
| 130 | `CharacterSocialMemory` | `GetFacilitySentiment` | `float` | `public` | None |
| 154 | `CharacterSocialMemory` | `GetRelationshipSentiment` | `float` | `public` | None |
| 173 | `CharacterSocialMemory` | `GetSourceTrust` | `float` | `public` | None |
| 184 | `CharacterSocialMemory` | `GetSourceTrustScore` | `float` | `private` | None |
| 210 | `CharacterSocialMemory` | `RememberRumor` | `void` | `private` | None |
| 220 | `CharacterSocialMemory` | `PruneExpiredRumors` | `void` | `private` | None |
| 238 | `CharacterSocialMemory` | `RebuildSentimentMaps` | `void` | `private` | None |
| 268 | `CharacterSocialMemory` | `SyncDebugLists` | `void` | `private` | None |
| 275 | `CharacterSocialMemory` | `Blend` | `void` | `private static` | None |
| 286 | `CharacterSocialMemory` | `GetDictionaryValue` | `float` | `private static` | None |
| 293 | `CharacterSocialMemory` | `SyncDebugList` | `void` | `private static` | None |
| 306 | `SocialRumorUtility` | `GetFacilityKeys` | `IEnumerable<string>` | `public static` | None |
| 323 | `SocialRumorUtility` | `GetCharacterKeys` | `IEnumerable<string>` | `public static` | None |
| 341 | `SocialRumorUtility` | `GetActorKey` | `string` | `public static` | None |
| 352 | `SocialRumorUtility` | `GetActorNameKey` | `string` | `public static` | None |
| 362 | `SocialRumorUtility` | `MatchesFacilityKey` | `bool` | `public static` | None |
| 383 | `SocialRumorUtility` | `MatchesFacilityTag` | `bool` | `public static` | None |
| 399 | `SocialRumorUtility` | `GetActorLabel` | `string` | `public static` | None |
| 409 | `SocialRumorUtility` | `GetFacilityLabel` | `string` | `public static` | None |
| 424 | `SocialRumorUtility` | `GetFacilityTag` | `string` | `public static` | None |
| 439 | `SocialRumorUtility` | `ContainsNormalized` | `bool` | `private static` | None |
| 445 | `SocialRumorUtility` | `Normalize` | `string` | `private static` | None |

### `Assets/Scripts/Character/AI/CharacterSocialMemoryFactory.cs`

- Functions detected: 3
- Judgment: manual resolver use is acceptable only in composition roots/factories.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `ICharacterSocialMemoryFactory` | `GetOrAdd` | `CharacterSocialMemory` | `-` | None |
| 13 | `CharacterSocialMemoryFactory` | `CharacterSocialMemoryFactory` | `ctor` | `public` | None |
| 19 | `CharacterSocialMemoryFactory` | `GetOrAdd` | `CharacterSocialMemory` | `public` | Manual resolver/injection |

### `Assets/Scripts/Character/AI/CharacterSocialMemoryService.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `ICharacterSocialMemoryService` | `GetOrAdd` | `CharacterSocialMemory` | `-` | None |
| 11 | `CharacterSocialMemoryService` | `CharacterSocialMemoryService` | `ctor` | `public` | None |
| 17 | `CharacterSocialMemoryService` | `GetOrAdd` | `CharacterSocialMemory` | `public` | None |

### `Assets/Scripts/Character/AI/Consideration/Consideration.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 10 | `Consideration` | `ScoreConsideration` | `float` | `public abstract` | None |

### `Assets/Scripts/Character/AI/Consideration/ConsiderationCanLookAround.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `ConsiderationCanLookAround` | `ScoreConsideration` | `float` | `public override` | None |

### `Assets/Scripts/Character/AI/Consideration/ConsiderationFacilityNeed.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 14 | `ConsiderationFacilityNeed` | `ScoreConsideration` | `float` | `public override` | None |

### `Assets/Scripts/Character/AI/Consideration/ConsiderationIsVisitable.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `ConsiderationIsVisitable` | `ScoreConsideration` | `float` | `public override` | None |
| 33 | `ConsiderationIsVisitable` | `ConvertLegacyType` | `FacilityRole` | `private static` | None |

### `Assets/Scripts/Character/AI/Consideration/ConsiderationRandom.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 10 | `ConsiderationRandom` | `ScoreConsideration` | `float` | `public override` | None |

### `Assets/Scripts/Character/AI/Consideration/ConsiderationShoppingCount.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `ConsiderationShoppingCount` | `ScoreConsideration` | `float` | `public override` | None |

### `Assets/Scripts/Character/AI/Consideration/ConsiderationShouldExitDungeon.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `ConsiderationShouldExitDungeon` | `ScoreConsideration` | `float` | `public override` | None |

### `Assets/Scripts/Character/AI/Consideration/ConsiderationStat.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 10 | `ConsiderationStat` | `ScoreConsideration` | `float` | `public override` | None |

### `Assets/Scripts/Character/AI/Consideration/ConsiderationWorkNeed.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 14 | `ConsiderationWorkNeed` | `ScoreConsideration` | `float` | `public override` | None |

### `Assets/Scripts/Character/AI/CustomerPersonaRuntime.cs`

- Functions detected: 16
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 20 | `CustomerPersonaData` | `Clamp` | `void` | `public` | None |
| 32 | `CustomerPersonaData` | `ClampMultiplier` | `float` | `private static` | None |
| 56 | `CustomerPersonaRuntime` | `ConstructCustomerPersonaRuntime` | `void` | `public` | None |
| 63 | `CustomerPersonaRuntime` | `Awake` | `void` | `private` | None |
| 68 | `CustomerPersonaRuntime` | `Bind` | `void` | `public` | None |
| 74 | `CustomerPersonaRuntime` | `RequestPersonaIfNeeded` | `bool` | `public` | None |
| 115 | `CustomerPersonaRuntime` | `ApplyGeneratedPersona` | `void` | `public` | None |
| 130 | `CustomerPersonaRuntime` | `TryGetLlmRuntime` | `bool` | `private` | None |
| 151 | `CustomerPersonaRuntime` | `GetActionMultiplier` | `float` | `public` | None |
| 179 | `CustomerPersonaRuntime` | `GetConditionCurveMultiplier` | `float` | `public` | None |
| 191 | `CustomerPersonaRuntime` | `GetFacilityTagPreference` | `float` | `public` | None |
| 223 | `CustomerPersonaRuntime` | `ValidatePersona` | `bool` | `private static` | None |
| 259 | `CustomerPersonaRuntime` | `IsMultiplierValid` | `bool` | `private static` | None |
| 271 | `CustomerPersonaRuntime` | `OnPersonaResult` | `void` | `private` | None |
| 291 | `CustomerPersonaRuntime` | `BuildPersonaPrompt` | `string` | `private static` | None |
| 313 | `CustomerPersonaRuntime` | `GetCondition` | `float` | `private static` | None |

### `Assets/Scripts/Character/AI/FacilityCandidateCache.cs`

- Functions detected: 10
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IFacilityCandidateCache` | `GetCandidates` | `IReadOnlyList<BuildableObject>` | `-` | None |
| 7 | `IFacilityCandidateCache` | `MarkDynamicStateDirty` | `void` | `-` | None |
| 8 | `IFacilityCandidateCache` | `Clear` | `void` | `-` | None |
| 24 | `FacilityCandidateCacheStore` | `GetCandidates` | `IReadOnlyList<BuildableObject>` | `public` | None |
| 52 | `FacilityCandidateCacheStore` | `MarkDynamicStateDirty` | `void` | `public` | None |
| 60 | `FacilityCandidateCacheStore` | `Clear` | `void` | `public` | None |
| 66 | `FacilityCandidateCacheStore` | `GetCache` | `GridFacilityCache` | `private` | None |
| 84 | `FacilityCandidateCacheStore` | `GetSingleRoleCandidates` | `List<BuildableObject>` | `private` | None |
| 108 | `FacilityCandidateCacheStore` | `IsSingleRole` | `bool` | `private static` | None |
| 114 | `FacilityCandidateCacheStore` | `GetSingleRoles` | `IEnumerable<FacilityRole>` | `private static` | None |

### `Assets/Scripts/Character/AI/FacilityCandidateScorer.cs`

- Functions detected: 23
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 18 | `FacilityCandidateScorer` | `GetCandidates` | `List<BuildableObject>` | `public static` | None |
| 43 | `FacilityCandidateScorer` | `SelectBest` | `BuildableObject` | `public static` | None |
| 79 | `FacilityCandidateScorer` | `HasCandidate` | `bool` | `public static` | None |
| 101 | `FacilityCandidateScorer` | `TrySelectBest` | `bool` | `public static` | None |
| 137 | `FacilityCandidateScorer` | `IsCandidate` | `bool` | `public static` | None |
| 151 | `FacilityCandidateScorer` | `IsCandidate` | `bool` | `public static` | None |
| 200 | `FacilityCandidateScorer` | `ScoreCandidate` | `float` | `public static` | None |
| 237 | `FacilityCandidateScorer` | `GetNeedScore` | `float` | `public static` | None |
| 276 | `FacilityCandidateScorer` | `GetCandidateSource` | `IEnumerable<BuildableObject>` | `private static` | None |
| 294 | `FacilityCandidateScorer` | `RequireFacilityCandidateCache` | `IFacilityCandidateCache` | `private static` | None |
| 305 | `FacilityCandidateScorer` | `IsReachableCandidate` | `bool` | `private static` | None |
| 320 | `FacilityCandidateScorer` | `GetBestMatchedRole` | `FacilityRole` | `private static` | None |
| 350 | `FacilityCandidateScorer` | `HasMultipleRoles` | `bool` | `private static` | None |
| 356 | `FacilityCandidateScorer` | `GetLowStatNeed` | `float` | `private static` | None |
| 369 | `FacilityCandidateScorer` | `GetPreferenceScore` | `float` | `private static` | None |
| 382 | `FacilityCandidateScorer` | `GetSpeciesTagPreferenceScore` | `float` | `private static` | None |
| 406 | `FacilityCandidateScorer` | `GetCharacterModelPreferenceScore` | `float` | `private static` | None |
| 422 | `FacilityCandidateScorer` | `GetStockScore` | `float` | `private static` | None |
| 438 | `FacilityCandidateScorer` | `GetAffordabilityScore` | `float` | `private static` | None |
| 453 | `FacilityCandidateScorer` | `GetCrowdScore` | `float` | `private static` | None |
| 465 | `FacilityCandidateScorer` | `GetDistanceScore` | `float` | `private static` | None |
| 481 | `FacilityCandidateScorer` | `GetNoveltyScore` | `float` | `private static` | None |
| 491 | `FacilityCandidateScorer` | `GetReputationBias` | `float` | `private static` | None |

### `Assets/Scripts/Character/AI/FacilityScoringContext.cs`

- Functions detected: 8
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `FacilityScoringContext` | `FacilityScoringContext` | `ctor` | `public` | None |
| 19 | `FacilityScoringContext` | `FacilityScoringContext` | `ctor` | `private` | None |
| 32 | `FacilityScoringContext` | `WithoutReputationBiasForIsolatedTest` | `FacilityScoringContext` | `public static` | None |
| 38 | `FacilityScoringContext` | `RequireFromActor` | `FacilityScoringContext` | `public static` | None |
| 49 | `FacilityScoringContext` | `GetReputationBias` | `float` | `public` | None |
| 65 | `FacilityScoringContext` | `IsFacilityRoleAvailable` | `bool` | `public` | None |
| 74 | `FacilityScoringContext` | `GetRoomUtilityScore` | `float` | `public` | None |
| 79 | `FacilityScoringContext` | `RequireRoomFacilityPolicy` | `IRoomFacilityPolicy` | `private` | None |

### `Assets/Scripts/Character/AI/Idle/IdleBehavior.cs`

- Functions detected: 10
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 4 | `IIdleBehavior` | `CanRun` | `bool` | `-` | None |
| 5 | `IIdleBehavior` | `TryRun` | `bool` | `-` | None |
| 11 | `StaffWanderIdleBehavior` | `CanRun` | `bool` | `public` | None |
| 17 | `StaffWanderIdleBehavior` | `TryRun` | `bool` | `public` | None |
| 41 | `StaticWaitIdleBehavior` | `CanRun` | `bool` | `public` | None |
| 46 | `StaticWaitIdleBehavior` | `TryRun` | `bool` | `public` | None |
| 65 | `IdleBehaviorRunner` | `TryRunDefault` | `bool` | `public static` | None |
| 97 | `IdleBehaviorRunner` | `TryRunStatic` | `bool` | `public static` | None |
| 114 | `IdleBehaviorRunner` | `GetSelectedBehaviorTypeNameForDebug` | `string` | `public static` | None |
| 120 | `IdleBehaviorRunner` | `SelectBehavior` | `IIdleBehavior` | `private static` | None |

### `Assets/Scripts/Character/AI/LlmJsonResponseParser.cs`

- Functions detected: 25
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `ILlmJsonPayload` | `Validate` | `bool` | `-` | None |
| 15 | `LlmJsonResponseParser` | `TryParse` | `bool` | `public static` | None |
| 74 | `LlmJsonResponseParser` | `TryExtractJsonObject` | `bool` | `public static` | None |
| 99 | `LlmJsonResponseParser` | `StripMarkdownFence` | `string` | `private static` | None |
| 135 | `MoodImpulseJsonDto` | `Validate` | `bool` | `public` | None |
| 180 | `MoodImpulseJsonDto` | `ValidateRawJson` | `bool` | `public static` | None |
| 204 | `MoodImpulseJsonDto` | `ToRuntimeImpulse` | `CharacterMoodImpulse` | `public` | None |
| 219 | `MoodImpulseJsonDto` | `HasRawNumber` | `bool` | `private static` | None |
| 225 | `MoodImpulseJsonDto` | `HasRawInteger` | `bool` | `private static` | None |
| 248 | `CustomerPersonaJsonDto` | `Validate` | `bool` | `public` | None |
| 278 | `CustomerPersonaJsonDto` | `ToRuntimeData` | `CustomerPersonaData` | `public` | None |
| 295 | `CustomerPersonaJsonDto` | `ValidateRawJson` | `bool` | `public static` | None |
| 328 | `CustomerPersonaJsonDto` | `ValidateMultiplier` | `bool` | `private static` | None |
| 340 | `CustomerPersonaJsonDto` | `HasRawNumber` | `bool` | `private static` | None |
| 359 | `MacroGoalJsonDto` | `Validate` | `bool` | `public` | None |
| 399 | `MacroGoalJsonDto` | `ValidateRawJson` | `bool` | `public static` | None |
| 417 | `MacroGoalJsonDto` | `ToRuntimeGoal` | `CharacterMacroGoal` | `public` | None |
| 431 | `MacroGoalJsonDto` | `HasRawNumber` | `bool` | `private static` | None |
| 437 | `MacroGoalJsonDto` | `HasRawInteger` | `bool` | `private static` | None |
| 462 | `SocialRumorJsonDto` | `Validate` | `bool` | `public` | None |
| 573 | `SocialRumorJsonDto` | `ValidateRawJson` | `bool` | `public static` | None |
| 595 | `SocialRumorJsonDto` | `ToRuntimeRumor` | `SocialRumor` | `public` | None |
| 618 | `SocialRumorJsonDto` | `HasRawNumber` | `bool` | `private static` | None |
| 624 | `SocialRumorJsonDto` | `HasRawInteger` | `bool` | `private static` | None |
| 636 | `BubbleLineJsonDto` | `Validate` | `bool` | `public` | None |

### `Assets/Scripts/Character/AI/LocalLlmRequestQueue.cs`

- Functions detected: 37
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 29 | `ILocalLlmRuntime` | `GeneratePersonaAsync` | `bool` | `-` | None |
| 30 | `ILocalLlmRuntime` | `GenerateMacroGoalAsync` | `bool` | `-` | None |
| 31 | `ILocalLlmRuntime` | `GenerateMoodImpulseAsync` | `bool` | `-` | None |
| 32 | `ILocalLlmRuntime` | `GenerateSocialRumorAsync` | `bool` | `-` | None |
| 33 | `ILocalLlmRuntime` | `GenerateFacilityEvolutionAsync` | `bool` | `-` | None |
| 34 | `ILocalLlmRuntime` | `GenerateBubbleLineAsync` | `bool` | `-` | None |
| 39 | `LocalLlmResult` | `LocalLlmResult` | `ctor` | `public` | None |
| 102 | `LocalLlmRequestQueue` | `ConfigureBubblePolicyForDebug` | `void` | `public` | None |
| 108 | `LocalLlmRequestQueue` | `ConfigureTimeoutsForDebug` | `void` | `public` | None |
| 124 | `LocalLlmRequestQueue` | `ClearForDebug` | `void` | `public` | None |
| 133 | `LocalLlmRequestQueue` | `AbortAllForDebug` | `void` | `public` | None |
| 144 | `LocalLlmRequestQueue` | `SetWarningLogsSuppressedForDebug` | `void` | `public` | None |
| 149 | `LocalLlmRequestQueue` | `Awake` | `void` | `private` | None |
| 161 | `LocalLlmRequestQueue` | `OnDestroy` | `void` | `private` | None |
| 169 | `LocalLlmRequestQueue` | `Update` | `void` | `private` | Coroutine/timing orchestration, Lifecycle contains dependency/workflow logic |
| 181 | `LocalLlmRequestQueue` | `EnqueuePersona` | `bool` | `public` | None |
| 186 | `LocalLlmRequestQueue` | `EnqueueMacroGoal` | `bool` | `public` | None |
| 191 | `LocalLlmRequestQueue` | `EnqueueMoodImpulse` | `bool` | `public` | None |
| 196 | `LocalLlmRequestQueue` | `EnqueueSocialRumor` | `bool` | `public` | None |
| 201 | `LocalLlmRequestQueue` | `EnqueueFacilityEvolution` | `bool` | `public` | None |
| 206 | `LocalLlmRequestQueue` | `EnqueueBubbleLine` | `bool` | `public` | None |
| 211 | `LocalLlmRequestQueue` | `GeneratePersonaAsync` | `bool` | `public` | None |
| 216 | `LocalLlmRequestQueue` | `GenerateMacroGoalAsync` | `bool` | `public` | None |
| 221 | `LocalLlmRequestQueue` | `GenerateMoodImpulseAsync` | `bool` | `public` | None |
| 226 | `LocalLlmRequestQueue` | `GenerateSocialRumorAsync` | `bool` | `public` | None |
| 231 | `LocalLlmRequestQueue` | `GenerateFacilityEvolutionAsync` | `bool` | `public` | None |
| 236 | `LocalLlmRequestQueue` | `GenerateBubbleLineAsync` | `bool` | `public` | None |
| 241 | `LocalLlmRequestQueue` | `Enqueue` | `bool` | `private` | None |
| 293 | `LocalLlmRequestQueue` | `ProcessRequest` | `IEnumerator` | `private` | None |
| 366 | `LocalLlmRequestQueue` | `BuildRequest` | `UnityWebRequest` | `private` | None |
| 396 | `LocalLlmRequestQueue` | `Complete` | `void` | `private` | None |
| 412 | `LocalLlmRequestQueue` | `LogWarningIfAllowed` | `void` | `private` | None |
| 422 | `LocalLlmRequestQueue` | `TryDropLowestPriorityBubble` | `bool` | `private` | None |
| 454 | `LocalLlmRequestQueue` | `DropExpiredBubbleRequests` | `void` | `private` | None |
| 480 | `LocalLlmRequestQueue` | `FindNextRequestIndex` | `int` | `private` | None |
| 500 | `LocalLlmRequestQueue` | `GetPriority` | `int` | `private static` | None |
| 514 | `LocalLlmRequestQueue` | `TryExtractContent` | `bool` | `private static` | None |

### `Assets/Scripts/Character/AI/SocialReputationRuntime.cs`

- Functions detected: 53
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 55 | `SocialReputationRuntime` | `ConstructSocialReputationRuntime` | `void` | `public` | None |
| 74 | `SocialReputationRuntime` | `Awake` | `void` | `private` | None |
| 86 | `SocialReputationRuntime` | `OnEnable` | `void` | `private` | None |
| 91 | `SocialReputationRuntime` | `Start` | `void` | `private` | None |
| 96 | `SocialReputationRuntime` | `OnDisable` | `void` | `private` | None |
| 105 | `SocialReputationRuntime` | `Update` | `void` | `private` | None |
| 116 | `SocialReputationRuntime` | `RequestSocialInterpretation` | `bool` | `public` | None |
| 180 | `SocialReputationRuntime` | `ReportFacilityExperience` | `bool` | `public` | None |
| 217 | `SocialReputationRuntime` | `ApplyRumor` | `bool` | `public` | None |
| 241 | `SocialReputationRuntime` | `TryGetLlmRuntime` | `bool` | `private` | None |
| 258 | `SocialReputationRuntime` | `RequireSceneQuery` | `IDungeonSceneComponentQuery` | `private` | None |
| 268 | `SocialReputationRuntime` | `GetFacilityUtilityBias` | `float` | `public` | None |
| 279 | `SocialReputationRuntime` | `GetCombinedFacilitySentiment` | `float` | `public` | None |
| 292 | `SocialReputationRuntime` | `GetGlobalFacilitySentiment` | `float` | `public` | None |
| 316 | `SocialReputationRuntime` | `ClearForDebug` | `void` | `public` | None |
| 334 | `SocialReputationRuntime` | `SetActorLogRequestsSuppressedForDebug` | `void` | `public` | None |
| 339 | `SocialReputationRuntime` | `SetWarningLogsSuppressedForDebug` | `void` | `public` | None |
| 344 | `SocialReputationRuntime` | `RestrictActorLogRequestsForDebug` | `void` | `public` | None |
| 349 | `SocialReputationRuntime` | `RegisterActorForDebug` | `void` | `public` | None |
| 354 | `SocialReputationRuntime` | `RegisterExistingActors` | `void` | `private` | None |
| 363 | `SocialReputationRuntime` | `RegisterExistingActorsIfInjected` | `void` | `private` | None |
| 371 | `SocialReputationRuntime` | `RegisterActor` | `void` | `private` | None |
| 390 | `SocialReputationRuntime` | `OnActorLogAdded` | `void` | `private` | None |
| 397 | `SocialReputationRuntime` | `OnSocialRumorResult` | `void` | `private` | None |
| 402 | `SocialReputationRuntime` | `OnSocialRumorResult` | `void` | `private` | None |
| 461 | `SocialReputationRuntime` | `LogSocialWarningIfNeeded` | `void` | `private` | None |
| 469 | `SocialReputationRuntime` | `LogWarningIfAllowed` | `void` | `private` | None |
| 479 | `SocialReputationRuntime` | `BuildSocialRumorPrompt` | `string` | `private` | None |
| 534 | `SocialReputationRuntime` | `BuildFacilityExperienceRumorPrompt` | `string` | `private` | None |
| 580 | `SocialReputationRuntime` | `AppendCandidateFacilities` | `void` | `private` | None |
| 611 | `SocialReputationRuntime` | `FindNearbyFacilities` | `IEnumerable<BuildableObject>` | `private` | None |
| 625 | `SocialReputationRuntime` | `AppendFacilityLine` | `void` | `private static` | None |
| 632 | `SocialReputationRuntime` | `ResolveFacilityFromLog` | `BuildableObject` | `private` | None |
| 644 | `SocialReputationRuntime` | `TryExtractFacilityId` | `bool` | `private static` | None |
| 661 | `SocialReputationRuntime` | `InferSocialEventSentiment` | `float` | `private static` | None |
| 693 | `SocialReputationRuntime` | `ShouldInterpretSocialEvent` | `bool` | `private static` | None |
| 732 | `SocialReputationRuntime` | `SpreadRumor` | `int` | `private` | None |
| 761 | `SocialReputationRuntime` | `CanHearRumor` | `bool` | `private` | None |
| 777 | `SocialReputationRuntime` | `ApplyGlobalFacilityReputation` | `void` | `private` | None |
| 784 | `SocialReputationRuntime` | `RebuildGlobalFacilityReputation` | `void` | `private` | None |
| 800 | `SocialReputationRuntime` | `ApplyGlobalFacilityReputationEntry` | `void` | `private` | None |
| 817 | `SocialReputationRuntime` | `PruneExpiredGlobalRumors` | `void` | `private` | None |
| 835 | `SocialReputationRuntime` | `EnsureMemory` | `CharacterSocialMemory` | `private` | None |
| 845 | `SocialReputationRuntime` | `RequireSocialMemoryService` | `ICharacterSocialMemoryService` | `private` | None |
| 855 | `SocialReputationRuntime` | `FillSourceIfMissing` | `void` | `private static` | None |
| 873 | `SocialReputationRuntime` | `HasValidTarget` | `bool` | `private static` | None |
| 893 | `SocialReputationRuntime` | `RumorTargetsExpectedFacility` | `bool` | `private static` | None |
| 913 | `SocialReputationRuntime` | `RumorTargetsKnownFacility` | `bool` | `private` | None |
| 942 | `SocialReputationRuntime` | `SentimentMatchesExpected` | `bool` | `private static` | None |
| 952 | `SocialReputationRuntime` | `GetNextRequestTime` | `float` | `private` | None |
| 959 | `SocialReputationRuntime` | `UnsubscribeAllActors` | `void` | `private` | None |
| 979 | `SocialReputationRuntime` | `SyncDebugList` | `void` | `private` | None |
| 988 | `SocialReputationRuntime` | `ContainsAny` | `bool` | `private static` | None |

### `Assets/Scripts/Character/CharacterSpawner.cs`

- Functions detected: 26
- Judgment: manual resolver use should move to a composition root or factory; timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 27 | `CharacterSpawner` | `Construct` | `void` | `public` | None |
| 44 | `CharacterSpawner` | `Awake` | `void` | `private` | None |
| 49 | `CharacterSpawner` | `Start` | `void` | `public override` | Coroutine/timing orchestration, Lifecycle contains dependency/workflow logic |
| 62 | `CharacterSpawner` | `EnsureRuntimeState` | `void` | `private` | Coroutine/timing orchestration |
| 80 | `CharacterSpawner` | `StartSpawn` | `IEnumerator` | `public` | None |
| 98 | `CharacterSpawner` | `Update` | `void` | `-` | None |
| 117 | `CharacterSpawner` | `TrySpawnCharacter` | `bool` | `public` | Manual resolver/injection |
| 177 | `CharacterSpawner` | `GetOutsideSpawnWorldPosition` | `Vector3` | `public` | None |
| 187 | `CharacterSpawner` | `GetEntryDoorWorldPosition` | `Vector3` | `public` | None |
| 202 | `CharacterSpawner` | `GetEntryGridPosition` | `Vector2Int` | `public` | None |
| 209 | `CharacterSpawner` | `TryGetEntryGridPosition` | `bool` | `public` | None |
| 228 | `CharacterSpawner` | `GetRegularCustomerState` | `RegularCustomerState` | `private` | None |
| 235 | `CharacterSpawner` | `TryGetGrid` | `bool` | `private` | None |
| 240 | `CharacterSpawner` | `ResolveRegularCustomerRuntimeProvider` | `IRegularCustomerRuntimeProvider` | `private` | None |
| 246 | `CharacterSpawner` | `ResolveGridSystemProvider` | `IGridSystemProvider` | `private` | None |
| 252 | `CharacterSpawner` | `ResolveRunVariableReader` | `IRunVariableRuntimeReader` | `private` | None |
| 258 | `CharacterSpawner` | `RequireCharacterObjectFactory` | `ICharacterSpawnObjectFactory` | `private` | None |
| 264 | `CharacterSpawner` | `Respawned` | `void` | `public` | None |
| 269 | `CharacterSpawner` | `CreatePooledItem` | `GameObject` | `private` | None |
| 273 | `CharacterSpawner` | `OnTakeFromPool` | `void` | `private` | None |
| 277 | `CharacterSpawner` | `OnReturnedToPool` | `void` | `private` | None |
| 287 | `CharacterSpawner` | `OnDestroyPoolObject` | `void` | `private` | None |
| 291 | `CharacterSpawner` | `Interact` | `IEnumerator` | `public` | None |
| 321 | `CharacterRespawnData` | `CharacterRespawnData` | `ctor` | `public` | None |
| 327 | `CharacterRespawnData` | `StartCheckRespawn` | `void` | `public` | None |
| 332 | `CharacterRespawnData` | `CheckResapwn` | `bool` | `public` | None |

### `Assets/Scripts/Character/CharacterSpawnObjectFactory.cs`

- Functions detected: 7
- Judgment: manual resolver use is acceptable only in composition roots/factories; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `ICharacterSpawnObjectFactory` | `Create` | `GameObject` | `-` | None |
| 9 | `ICharacterSpawnObjectFactory` | `Inject` | `void` | `-` | None |
| 10 | `ICharacterSpawnObjectFactory` | `Destroy` | `void` | `-` | None |
| 16 | `CharacterSpawnObjectFactory` | `CharacterSpawnObjectFactory` | `ctor` | `public` | None |
| 22 | `CharacterSpawnObjectFactory` | `Create` | `GameObject` | `public` | Runtime object creation |
| 32 | `CharacterSpawnObjectFactory` | `Inject` | `void` | `public` | Manual resolver/injection |
| 48 | `CharacterSpawnObjectFactory` | `Destroy` | `void` | `public` | None |

### `Assets/Scripts/Character/Core/CharacterAbilityCache.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 20 | `CharacterAbilityCache` | `Awake` | `void` | `private` | None |
| 25 | `CharacterAbilityCache` | `CacheAbility` | `void` | `public` | None |
| 31 | `CharacterAbilityCache` | `RefreshAbilityCache` | `void` | `public` | None |
| 37 | `CharacterAbilityCache` | `GetAbility` | `T` | `public` | None |
| 52 | `CharacterAbilityCache` | `TryGetAbility` | `bool` | `public` | None |

### `Assets/Scripts/Character/Core/CharacterActor.cs`

- Functions detected: 63
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 116 | `CharacterActor` | `ConstructCharacterActor` | `void` | `public` | None |
| 221 | `CharacterActor` | `From` | `CharacterActor` | `public static` | None |
| 226 | `CharacterActor` | `Awake` | `void` | `private` | None |
| 232 | `CharacterActor` | `Start` | `void` | `private` | Coroutine/timing orchestration, Lifecycle contains dependency/workflow logic |
| 251 | `CharacterActor` | `Update` | `void` | `private` | None |
| 256 | `CharacterActor` | `OnEnable` | `void` | `private` | None |
| 261 | `CharacterActor` | `OnDisable` | `void` | `private` | None |
| 267 | `CharacterActor` | `OnMouseDown` | `void` | `private` | None |
| 272 | `CharacterActor` | `Initialize` | `void` | `public` | None |
| 296 | `CharacterActor` | `TryExecuteSelectedAiAction` | `bool` | `public` | None |
| 312 | `CharacterActor` | `GetReachableBuilding` | `List<BuildableObject>` | `public` | None |
| 332 | `CharacterActor` | `TryGetGrid` | `bool` | `private` | None |
| 343 | `CharacterActor` | `EnsureRuntimeState` | `void` | `public` | None |
| 385 | `CharacterActor` | `GetInfoType` | `InfoFeedEvent.Type` | `public` | None |
| 390 | `CharacterActor` | `GetAbility` | `T` | `public` | None |
| 396 | `CharacterActor` | `TryGetAbility` | `bool` | `public` | None |
| 408 | `CharacterActor` | `GetNowXY` | `Vector2Int` | `public` | None |
| 414 | `CharacterActor` | `AddLog` | `void` | `public` | None |
| 420 | `CharacterActor` | `GetMoveSpeed` | `float` | `public` | None |
| 426 | `CharacterActor` | `GetConsumptionMultiplier` | `float` | `public` | None |
| 432 | `CharacterActor` | `GetStayDurationMultiplier` | `float` | `public` | None |
| 438 | `CharacterActor` | `GetCrowdSensitivityMultiplier` | `float` | `public` | None |
| 444 | `CharacterActor` | `GetWorkSpeedMultiplier` | `float` | `public` | None |
| 450 | `CharacterActor` | `GetWorkPreferenceScore` | `float` | `public` | None |
| 456 | `CharacterActor` | `GetFacilityPreferenceScore` | `float` | `public` | None |
| 462 | `CharacterActor` | `GetAccidentChanceMultiplier` | `float` | `public` | None |
| 468 | `CharacterActor` | `GetIncidentType` | `CharacterSpeciesIncidentType` | `public` | None |
| 474 | `CharacterActor` | `GetCombatPowerMultiplier` | `float` | `public` | None |
| 480 | `CharacterActor` | `GetFatigueEfficiencyMultiplier` | `float` | `public` | None |
| 486 | `CharacterActor` | `GetInjuryEfficiencyMultiplier` | `float` | `public` | None |
| 510 | `CharacterActor` | `Initialization` | `void` | `public` | None |
| 515 | `CharacterActor` | `CacheAbility` | `void` | `public` | None |
| 521 | `CharacterActor` | `RefreshAbilityCache` | `void` | `public` | None |
| 527 | `CharacterActor` | `ChangeStatByTick` | `IEnumerator` | `public` | None |
| 533 | `CharacterActor` | `ChangesStat` | `void` | `public` | None |
| 539 | `CharacterActor` | `GetCharacterStat` | `int` | `public` | None |
| 545 | `CharacterActor` | `ApplyDamage` | `void` | `public` | None |
| 551 | `CharacterActor` | `Heal` | `void` | `public` | None |
| 557 | `CharacterActor` | `SetInjurySeverity` | `void` | `public` | None |
| 563 | `CharacterActor` | `Die` | `void` | `public` | None |
| 569 | `CharacterActor` | `InitializeStats` | `void` | `public` | None |
| 575 | `CharacterActor` | `SetLifecycleState` | `void` | `public` | None |
| 581 | `CharacterActor` | `BeginExpedition` | `bool` | `public` | None |
| 587 | `CharacterActor` | `EndExpedition` | `void` | `public` | None |
| 593 | `CharacterActor` | `ChangeLayer` | `void` | `public` | None |
| 599 | `CharacterActor` | `ApplyVisualFootAnchor` | `void` | `public` | None |
| 605 | `CharacterActor` | `GetVisualTopLocalY` | `float` | `public` | None |
| 611 | `CharacterActor` | `DoFade` | `void` | `public` | None |
| 617 | `CharacterActor` | `Flip` | `void` | `public` | None |
| 623 | `CharacterActor` | `HideForTraversal` | `void` | `public` | None |
| 629 | `CharacterActor` | `RestoreTraversalVisibility` | `void` | `public` | None |
| 635 | `CharacterActor` | `SetAiPaused` | `void` | `public` | None |
| 641 | `CharacterActor` | `IsAiPaused` | `bool` | `public` | None |
| 646 | `CharacterActor` | `GetSpeciesShortDescription` | `string` | `public` | None |
| 652 | `CharacterActor` | `RaiseDied` | `void` | `internal` | None |
| 668 | `CharacterActor` | `EnsureFeedbackBubbleIfInjected` | `void` | `private` | None |
| 678 | `CharacterActor` | `EnsureSocialMemory` | `void` | `private` | None |
| 685 | `CharacterActor` | `RequireAiSchedulingService` | `ICharacterAiSchedulingService` | `private` | None |
| 691 | `CharacterActor` | `RequireWorldInfoClickSelector` | `IWorldInfoClickSelector` | `private` | None |
| 697 | `CharacterActor` | `RegisterWithAiSchedulerRequired` | `void` | `private` | None |
| 703 | `CharacterActor` | `RegisterWithAiSchedulerIfReady` | `void` | `private` | None |
| 714 | `CharacterActor` | `UnregisterFromAiScheduler` | `void` | `private` | None |
| 725 | `CharacterActor` | `EmptyRoutine` | `IEnumerator` | `private static` | None |

### `Assets/Scripts/Character/Core/CharacterDeathEvent.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `CharacterDeathEvent` | `CharacterDeathEvent` | `ctor` | `public` | None |
| 13 | `CharacterDeathEvent` | `Trigger` | `void` | `public static` | None |

### `Assets/Scripts/Character/Core/CharacterEnums.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/Character/Core/CharacterIdentity.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 50 | `CharacterIdentity` | `Awake` | `void` | `private` | None |
| 59 | `CharacterIdentity` | `Bind` | `void` | `public` | None |
| 64 | `CharacterIdentity` | `SetData` | `void` | `public` | None |
| 72 | `CharacterIdentity` | `SetCharacterType` | `void` | `public` | None |
| 77 | `CharacterIdentity` | `GetSpeciesShortDescription` | `string` | `public` | None |

### `Assets/Scripts/Character/Core/CharacterLifecycle.cs`

- Functions detected: 10
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 33 | `CharacterLifecycle` | `ConstructCharacterLifecycle` | `void` | `public` | None |
| 40 | `CharacterLifecycle` | `Awake` | `void` | `private` | None |
| 45 | `CharacterLifecycle` | `Bind` | `void` | `public` | None |
| 55 | `CharacterLifecycle` | `SetAiPaused` | `void` | `public` | None |
| 60 | `CharacterLifecycle` | `BeginExpedition` | `bool` | `public` | None |
| 81 | `CharacterLifecycle` | `EndExpedition` | `void` | `public` | None |
| 100 | `CharacterLifecycle` | `SetLifecycleState` | `void` | `public` | None |
| 126 | `CharacterLifecycle` | `GetNowXY` | `Vector2Int` | `public` | None |
| 138 | `CharacterLifecycle` | `SnapToWalkableGridWhenReady` | `IEnumerator` | `public` | None |
| 171 | `CharacterLifecycle` | `TryGetGrid` | `bool` | `private` | None |

### `Assets/Scripts/Character/Core/CharacterLog.cs`

- Functions detected: 8
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 26 | `CharacterLog` | `Awake` | `void` | `private` | None |
| 31 | `CharacterLog` | `Bind` | `void` | `public` | None |
| 36 | `CharacterLog` | `AddLog` | `void` | `public` | None |
| 67 | `CharacterLog` | `EnsureLog` | `void` | `private` | None |
| 81 | `CharacterLogEntry` | `CharacterLogEntry` | `ctor` | `public` | None |
| 93 | `CharacterLogUtility` | `ToCauseTag` | `string` | `public static` | None |
| 154 | `CharacterLogUtility` | `ContainsAny` | `bool` | `private static` | None |
| 159 | `CharacterLogUtility` | `ExtractReasonAfterSeparator` | `string` | `private static` | None |

### `Assets/Scripts/Character/Core/CharacterStats.cs`

- Functions detected: 26
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 62 | `CharacterStats` | `Awake` | `void` | `private` | None |
| 67 | `CharacterStats` | `ConstructCharacterStats` | `void` | `public` | None |
| 81 | `CharacterStats` | `Bind` | `void` | `public` | None |
| 91 | `CharacterStats` | `ChangeStatByTick` | `IEnumerator` | `public` | Coroutine/timing orchestration |
| 111 | `CharacterStats` | `ChangesStat` | `void` | `public` | None |
| 118 | `CharacterStats` | `GetCharacterStat` | `int` | `public` | None |
| 123 | `CharacterStats` | `GetMoveSpeed` | `float` | `public` | None |
| 132 | `CharacterStats` | `GetConsumptionMultiplier` | `float` | `public` | None |
| 137 | `CharacterStats` | `GetStayDurationMultiplier` | `float` | `public` | None |
| 142 | `CharacterStats` | `GetCrowdSensitivityMultiplier` | `float` | `public` | None |
| 147 | `CharacterStats` | `GetWorkSpeedMultiplier` | `float` | `public` | None |
| 160 | `CharacterStats` | `GetWorkPreferenceScore` | `float` | `public` | None |
| 165 | `CharacterStats` | `GetFacilityPreferenceScore` | `float` | `public` | None |
| 170 | `CharacterStats` | `GetAccidentChanceMultiplier` | `float` | `public` | None |
| 175 | `CharacterStats` | `GetIncidentType` | `CharacterSpeciesIncidentType` | `public` | None |
| 180 | `CharacterStats` | `GetCombatPowerMultiplier` | `float` | `public` | None |
| 186 | `CharacterStats` | `GetFatigueEfficiencyMultiplier` | `float` | `public` | None |
| 197 | `CharacterStats` | `GetInjuryEfficiencyMultiplier` | `float` | `public` | None |
| 202 | `CharacterStats` | `ApplyDamage` | `void` | `public` | None |
| 218 | `CharacterStats` | `Heal` | `void` | `public` | None |
| 227 | `CharacterStats` | `SetInjurySeverity` | `void` | `public` | None |
| 234 | `CharacterStats` | `Die` | `void` | `public` | None |
| 254 | `CharacterStats` | `RecalculateVitals` | `void` | `public` | None |
| 276 | `CharacterStats` | `EnsureStats` | `void` | `private` | None |
| 287 | `CharacterStats` | `ResolveMetaProgressionRuntimeReader` | `IMetaProgressionRuntimeReader` | `private` | None |
| 293 | `CharacterStats` | `EnsureStat` | `void` | `private` | None |

### `Assets/Scripts/Character/Core/CharacterVisual.cs`

- Functions detected: 27
- Judgment: non-UI code depends on UI types; object creation is a factory boundary candidate; timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 29 | `CharacterVisual` | `Awake` | `void` | `private` | None |
| 34 | `CharacterVisual` | `Bind` | `void` | `public` | None |
| 41 | `CharacterVisual` | `ChangeLayer` | `void` | `public` | None |
| 50 | `CharacterVisual` | `EnsureVisualReferences` | `void` | `public` | None |
| 98 | `CharacterVisual` | `CreateVisualRoot` | `Transform` | `private` | Runtime object creation |
| 109 | `CharacterVisual` | `CopySpriteRenderer` | `void` | `private static` | None |
| 128 | `CharacterVisual` | `RemoveRootSpriteRenderer` | `void` | `private static` | None |
| 145 | `CharacterVisual` | `SetCharacterSprite` | `void` | `public` | None |
| 157 | `CharacterVisual` | `ApplyVisualFootAnchor` | `void` | `public` | None |
| 173 | `CharacterVisual` | `GetVisualTopLocalY` | `float` | `public` | None |
| 185 | `CharacterVisual` | `DoFade` | `void` | `public` | None |
| 195 | `CharacterVisual` | `Flip` | `void` | `public` | None |
| 214 | `CharacterVisual` | `HideForTraversal` | `void` | `public` | Coroutine/timing orchestration |
| 232 | `CharacterVisual` | `RestoreTraversalVisibility` | `void` | `public` | None |
| 238 | `CharacterVisual` | `RestoreTraversalVisibilityAfter` | `IEnumerator` | `private` | None |
| 245 | `CharacterVisual` | `StopTraversalVisibilityTimer` | `void` | `private` | Coroutine/timing orchestration |
| 256 | `CharacterVisual` | `RestoreTraversalVisibilityNow` | `void` | `private` | UI type dependency |
| 293 | `CharacterVisual` | `RecoverExpiredTraversalVisibility` | `void` | `public` | None |
| 318 | `CharacterVisual` | `CaptureRendererVisibility` | `List<RendererVisibilityState>` | `private` | None |
| 332 | `CharacterVisual` | `CaptureCanvasVisibility` | `List<CanvasVisibilityState>` | `private` | UI type dependency |
| 346 | `CharacterVisual` | `SetTraversalVisible` | `void` | `private` | UI type dependency |
| 371 | `CharacterVisual` | `SetRenderersVisible` | `void` | `public` | None |
| 377 | `CharacterVisual` | `EnsureVisibleForActiveLifecycle` | `void` | `public` | None |
| 391 | `CharacterVisual` | `CanRecoverActiveVisibility` | `bool` | `private` | None |
| 409 | `CharacterVisual` | `SetSpriteRenderersVisible` | `void` | `private` | None |
| 420 | `RendererVisibilityState` | `RendererVisibilityState` | `ctor` | `public` | None |
| 432 | `CanvasVisibilityState` | `CanvasVisibilityState` | `ctor` | `public` | UI type dependency |

### `Assets/Scripts/Character/Core/CharacterVisualRootFactory.cs`

- Functions detected: 2
- Judgment: object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `ICharacterVisualRootFactory` | `EnsureVisualRoot` | `SpriteRenderer` | `-` | None |
| 12 | `CharacterVisualRootFactory` | `EnsureVisualRoot` | `SpriteRenderer` | `public` | Runtime object creation |

### `Assets/Scripts/Character/Core/Customer.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/Character/Core/OwnerCharacterFactory.cs`

- Functions detected: 6
- Judgment: manual resolver use is acceptable only in composition roots/factories; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `IOwnerCharacterFactory` | `CreateOwner` | `CharacterActor` | `-` | None |
| 20 | `OwnerCharacterFactory` | `OwnerCharacterFactory` | `ctor` | `public` | None |
| 33 | `OwnerCharacterFactory` | `CreateOwner` | `CharacterActor` | `public` | Runtime object creation |
| 59 | `OwnerCharacterFactory` | `EnsureOwnerComponents` | `CharacterActor` | `private` | None |
| 96 | `OwnerCharacterFactory` | `InjectOwnerRuntime` | `void` | `private` | Manual resolver/injection |
| 104 | `OwnerCharacterFactory` | `ResolveOwnerSpawnPosition` | `Vector3` | `private` | None |

### `Assets/Scripts/Character/Core/OwnerRunManager.cs`

- Functions detected: 16
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 28 | `OwnerRunManager` | `Awake` | `void` | `private` | None |
| 34 | `OwnerRunManager` | `ConstructOwnerRunManager` | `void` | `public` | None |
| 46 | `OwnerRunManager` | `Start` | `void` | `private` | None |
| 58 | `OwnerRunManager` | `SelectOwnerByIndex` | `void` | `public` | None |
| 70 | `OwnerRunManager` | `SelectOwner` | `void` | `public` | None |
| 95 | `OwnerRunManager` | `HandleOwnerDeath` | `void` | `public` | None |
| 111 | `OwnerRunManager` | `GetDefaultOwner` | `CharacterSO` | `public` | None |
| 117 | `OwnerRunManager` | `SpawnOwner` | `CharacterActor` | `private` | None |
| 126 | `OwnerRunManager` | `EnsureOwnerCandidates` | `void` | `private` | None |
| 141 | `OwnerRunManager` | `NormalizeOwnerCandidates` | `void` | `private` | None |
| 149 | `OwnerRunManager` | `ResolveOwnerCharacterFactory` | `IOwnerCharacterFactory` | `private` | None |
| 155 | `OwnerRunManager` | `OnTriggerEvent` | `void` | `public` | None |
| 163 | `OwnerRunManager` | `OnEnable` | `void` | `private` | None |
| 168 | `OwnerRunManager` | `OnDisable` | `void` | `private` | None |
| 179 | `OwnerRunEndedEvent` | `OwnerRunEndedEvent` | `ctor` | `public` | None |
| 187 | `OwnerRunEndedEvent` | `Trigger` | `void` | `public static` | None |

### `Assets/Scripts/Character/Core/Shopkeeper.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/Character/Input/OwnerCommandController.cs`

- Functions detected: 10
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 20 | `OwnerCommandController` | `ConstructOwnerCommandController` | `void` | `public` | None |
| 37 | `OwnerCommandController` | `Update` | `void` | `private` | None |
| 50 | `OwnerCommandController` | `TryIssuePriorityWorkCommand` | `void` | `private` | None |
| 100 | `OwnerCommandController` | `TryIssueSuppressCommand` | `void` | `private` | None |
| 132 | `OwnerCommandController` | `OnTriggerEvent` | `void` | `public` | None |
| 141 | `OwnerCommandController` | `OnEnable` | `void` | `private` | None |
| 146 | `OwnerCommandController` | `OnDisable` | `void` | `private` | None |
| 151 | `OwnerCommandController` | `RequireMainCameraProvider` | `IMainCameraProvider` | `private` | None |
| 157 | `OwnerCommandController` | `RequireInputReader` | `IPlayerInputReader` | `private` | None |
| 163 | `OwnerCommandController` | `RequirePointerRaycaster` | `IWorldPointerRaycaster` | `private` | None |

### `Assets/Scripts/Character/SO/CharacterModelData.cs`

- Functions detected: 27
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 41 | `CharacterStatBlock` | `CreateDefault` | `CharacterStatBlock` | `public static` | None |
| 57 | `CharacterStatBlock` | `Get` | `int` | `public` | None |
| 74 | `CharacterStatBlock` | `Add` | `void` | `public` | None |
| 108 | `CharacterModelModifiers` | `Multiply` | `void` | `public` | None |
| 137 | `CharacterRuntimeProfile` | `CharacterRuntimeProfile` | `ctor` | `private` | None |
| 158 | `CharacterRuntimeProfile` | `From` | `CharacterRuntimeProfile` | `public static` | None |
| 163 | `CharacterRuntimeProfile` | `GetStat` | `int` | `public` | None |
| 168 | `CharacterRuntimeProfile` | `GetMoveSpeedMultiplier` | `float` | `public` | None |
| 174 | `CharacterRuntimeProfile` | `GetSpendingMultiplier` | `float` | `public` | None |
| 180 | `CharacterRuntimeProfile` | `GetConsumptionMultiplier` | `float` | `public` | None |
| 185 | `CharacterRuntimeProfile` | `GetStayDurationMultiplier` | `float` | `public` | None |
| 191 | `CharacterRuntimeProfile` | `GetCrowdSensitivityMultiplier` | `float` | `public` | None |
| 196 | `CharacterRuntimeProfile` | `GetAccidentChanceMultiplier` | `float` | `public` | None |
| 203 | `CharacterRuntimeProfile` | `GetIncidentType` | `CharacterSpeciesIncidentType` | `public` | None |
| 208 | `CharacterRuntimeProfile` | `GetIncidentName` | `string` | `public` | None |
| 213 | `CharacterRuntimeProfile` | `GetIncidentDescription` | `string` | `public` | None |
| 218 | `CharacterRuntimeProfile` | `GetShortDescription` | `string` | `public` | None |
| 223 | `CharacterRuntimeProfile` | `GetCombatPowerMultiplier` | `float` | `public` | None |
| 229 | `CharacterRuntimeProfile` | `GetWorkSpeedMultiplier` | `float` | `public` | None |
| 251 | `CharacterRuntimeProfile` | `GetWorkPreferenceScore` | `float` | `public` | None |
| 271 | `CharacterRuntimeProfile` | `GetFacilityPreferenceScore` | `float` | `public` | None |
| 291 | `CharacterRuntimeProfile` | `HasTrait` | `bool` | `public` | None |
| 298 | `CharacterRuntimeProfile` | `ClampStatMultiplier` | `float` | `private` | None |
| 307 | `CharacterRuntimeProfile` | `GetBestWorkStat` | `CharacterStatType` | `private static` | None |
| 319 | `CharacterRuntimeProfile` | `BuildFinalStats` | `CharacterStatBlock` | `private static` | None |
| 340 | `CharacterRuntimeProfile` | `BuildFinalModifiers` | `CharacterModelModifiers` | `private static` | None |
| 357 | `CharacterRuntimeProfile` | `CopyStats` | `CharacterStatBlock` | `private static` | None |

### `Assets/Scripts/Character/SO/CharacterSO.cs`

- Functions detected: 4
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 55 | `CharacterSO` | `CreateRuntimeProfile` | `CharacterRuntimeProfile` | `public` | None |
| 99 | `CharacterSO` | `GetFrequencyVisit` | `int` | `public` | None |
| 111 | `CharacterSO` | `GetHoldingMoney` | `int` | `public` | None |
| 115 | `CharacterSO` | `GetHoldingMoney` | `int` | `public` | None |

### `Assets/Scripts/Character/SO/CharacterSpeciesSO.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/Character/SO/CharacterTraitSO.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/Character/UI/CharacterFeedbackBubble.cs`

- Functions detected: 21
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 39 | `CharacterFeedbackBubble` | `ConstructCharacterFeedbackBubble` | `void` | `public` | None |
| 50 | `CharacterFeedbackBubble` | `Awake` | `void` | `private` | None |
| 59 | `CharacterFeedbackBubble` | `OnEnable` | `void` | `private` | None |
| 76 | `CharacterFeedbackBubble` | `OnDisable` | `void` | `private` | None |
| 94 | `CharacterFeedbackBubble` | `LateUpdate` | `void` | `private` | None |
| 112 | `CharacterFeedbackBubble` | `Show` | `void` | `public` | None |
| 123 | `CharacterFeedbackBubble` | `EvaluatePersistentState` | `CharacterFeedbackState` | `public` | None |
| 163 | `CharacterFeedbackBubble` | `ClassifyLogTag` | `CharacterFeedbackState` | `public static` | None |
| 198 | `CharacterFeedbackBubble` | `GetSymbol` | `string` | `public static` | None |
| 211 | `CharacterFeedbackBubble` | `OnLogAdded` | `void` | `private` | None |
| 227 | `CharacterFeedbackBubble` | `OnStatChanged` | `void` | `private` | None |
| 235 | `CharacterFeedbackBubble` | `ApplyState` | `void` | `private` | None |
| 254 | `CharacterFeedbackBubble` | `HideView` | `void` | `private` | None |
| 260 | `CharacterFeedbackBubble` | `EnsureView` | `void` | `private` | None |
| 270 | `CharacterFeedbackBubble` | `ReleaseView` | `void` | `private` | None |
| 281 | `CharacterFeedbackBubble` | `GetLocalOffset` | `Vector3` | `private` | None |
| 293 | `CharacterFeedbackBubble` | `GetStat` | `float` | `private` | None |
| 302 | `CharacterFeedbackBubble` | `RequireAiSchedulingService` | `ICharacterAiSchedulingService` | `private` | None |
| 308 | `CharacterFeedbackBubble` | `RequireBubbleViewFactory` | `ICharacterFeedbackBubbleViewFactory` | `private` | None |
| 315 | `CharacterFeedbackBubble` | `GetColor` | `Color` | `private static` | None |
| 328 | `CharacterFeedbackBubble` | `ContainsAny` | `bool` | `private static` | None |

### `Assets/Scripts/Character/UI/CharacterFeedbackBubbleFactory.cs`

- Functions detected: 3
- Judgment: manual resolver use is acceptable only in composition roots/factories.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `ICharacterFeedbackBubbleFactory` | `GetOrAdd` | `CharacterFeedbackBubble` | `-` | None |
| 13 | `CharacterFeedbackBubbleFactory` | `CharacterFeedbackBubbleFactory` | `ctor` | `public` | None |
| 19 | `CharacterFeedbackBubbleFactory` | `GetOrAdd` | `CharacterFeedbackBubble` | `public` | Manual resolver/injection |

### `Assets/Scripts/Character/UI/CharacterFeedbackBubbleService.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `ICharacterFeedbackBubbleService` | `GetOrAdd` | `CharacterFeedbackBubble` | `-` | None |
| 11 | `CharacterFeedbackBubbleService` | `CharacterFeedbackBubbleService` | `ctor` | `public` | None |
| 17 | `CharacterFeedbackBubbleService` | `GetOrAdd` | `CharacterFeedbackBubble` | `public` | None |

### `Assets/Scripts/Character/UI/CharacterFeedbackBubbleViewFactory.cs`

- Functions detected: 6
- Judgment: object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `ICharacterFeedbackBubbleViewFactory` | `Acquire` | `TextMeshPro` | `-` | None |
| 10 | `ICharacterFeedbackBubbleViewFactory` | `Release` | `void` | `-` | None |
| 17 | `CharacterFeedbackBubbleViewFactory` | `CharacterFeedbackBubbleViewFactory` | `ctor` | `public` | None |
| 24 | `CharacterFeedbackBubbleViewFactory` | `Acquire` | `TextMeshPro` | `public` | None |
| 46 | `CharacterFeedbackBubbleViewFactory` | `Release` | `void` | `public` | None |
| 59 | `CharacterFeedbackBubbleViewFactory` | `CreateTextView` | `TextMeshPro` | `private` | Runtime object creation |

### `Assets/Scripts/Character/UI/CharacterFloatingIcon.cs`

- Functions detected: 6
- Judgment: scene discovery should be isolated behind providers/build callbacks.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IFloatingIconFeedbackService` | `Show` | `bool` | `-` | None |
| 19 | `GameManagerFloatingIconFeedbackService` | `GameManagerFloatingIconFeedbackService` | `ctor` | `public` | None |
| 24 | `GameManagerFloatingIconFeedbackService` | `Show` | `bool` | `public` | None |
| 44 | `GameManagerFloatingIconFeedbackService` | `ResolveNumber` | `DamageNumber` | `private` | None |
| 60 | `GameManagerFloatingIconFeedbackService` | `ResolveGameManager` | `GameManager` | `private` | Scene query coupling |
| 67 | `GameManagerFloatingIconFeedbackService` | `FitIcon` | `void` | `private static` | None |

### `Assets/Scripts/Character/UI/OwnerSelectionOptionButtonFactory.cs`

- Functions detected: 5
- Judgment: UI type usage fits this UI/factory context; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `IOwnerSelectionOptionButtonFactory` | `Create` | `Button` | `-` | UI type dependency |
| 10 | `IOwnerSelectionOptionButtonFactory` | `Release` | `void` | `-` | None |
| 16 | `OwnerSelectionOptionButtonFactory` | `OwnerSelectionOptionButtonFactory` | `ctor` | `public` | None |
| 22 | `OwnerSelectionOptionButtonFactory` | `Create` | `Button` | `public` | Runtime object creation, UI type dependency |
| 52 | `OwnerSelectionOptionButtonFactory` | `Release` | `void` | `public` | None |

### `Assets/Scripts/Character/UI/OwnerSelectionPanel.cs`

- Functions detected: 9
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 18 | `OwnerSelectionPanel` | `ConstructOwnerSelectionPanel` | `void` | `public` | None |
| 32 | `OwnerSelectionPanel` | `Start` | `void` | `private` | None |
| 50 | `OwnerSelectionPanel` | `BuildOptions` | `void` | `public` | None |
| 77 | `OwnerSelectionPanel` | `ResolveOwnerRunManager` | `OwnerRunManager` | `private` | None |
| 98 | `OwnerSelectionPanel` | `RefreshSelectedOwner` | `void` | `private` | None |
| 108 | `OwnerSelectionPanel` | `RequireTmpKoreanFontService` | `ITmpKoreanFontService` | `private` | None |
| 115 | `OwnerSelectionPanel` | `RequireOptionButtonFactory` | `IOwnerSelectionOptionButtonFactory` | `private` | None |
| 122 | `OwnerSelectionPanel` | `MakeButtonLabel` | `string` | `private static` | None |
| 132 | `OwnerSelectionPanel` | `OnDestroy` | `void` | `private` | None |

### `Assets/Scripts/Character/UI/StaffWorkPriorityPanel.cs`

- Functions detected: 27
- Judgment: UI type usage fits this UI/factory context.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 34 | `StaffWorkPriorityPanel` | `ConstructStaffWorkPriorityPanel` | `void` | `public` | None |
| 45 | `StaffWorkPriorityPanel` | `Awake` | `void` | `private` | None |
| 50 | `StaffWorkPriorityPanel` | `Start` | `void` | `private` | None |
| 56 | `StaffWorkPriorityPanel` | `Update` | `void` | `private` | None |
| 71 | `StaffWorkPriorityPanel` | `OnTriggerEvent` | `void` | `public` | None |
| 88 | `StaffWorkPriorityPanel` | `Refresh` | `void` | `public` | None |
| 94 | `StaffWorkPriorityPanel` | `EnsureLayout` | `void` | `private` | UI type dependency |
| 155 | `StaffWorkPriorityPanel` | `ResolveHost` | `RectTransform` | `private` | UI type dependency |
| 184 | `StaffWorkPriorityPanel` | `ClearHost` | `void` | `private` | None |
| 205 | `StaffWorkPriorityPanel` | `BuildTable` | `void` | `private` | None |
| 248 | `StaffWorkPriorityPanel` | `BuildHeader` | `void` | `private` | UI type dependency |
| 272 | `StaffWorkPriorityPanel` | `BuildWorkerRow` | `void` | `private` | UI type dependency |
| 308 | `StaffWorkPriorityPanel` | `CreateRow` | `GameObject` | `private` | UI type dependency |
| 321 | `StaffWorkPriorityPanel` | `CreateLabelCell` | `TMP_Text` | `private` | UI type dependency |
| 346 | `StaffWorkPriorityPanel` | `CreatePriorityCell` | `void` | `private` | UI type dependency |
| 374 | `StaffWorkPriorityPanel` | `CreateCellObject` | `GameObject` | `private` | UI type dependency |
| 384 | `StaffWorkPriorityPanel` | `AddCellText` | `TMP_Text` | `private` | UI type dependency |
| 405 | `StaffWorkPriorityPanel` | `CalculateWorkerHash` | `int` | `private` | None |
| 410 | `StaffWorkPriorityPanel` | `GetWorkTypeLabel` | `string` | `private static` | None |
| 426 | `StaffWorkPriorityPanel` | `GetPriorityLabel` | `string` | `private static` | None |
| 437 | `StaffWorkPriorityPanel` | `GetPriorityColor` | `Color` | `private static` | None |
| 452 | `StaffWorkPriorityPanel` | `GetWorkerStatus` | `string` | `private static` | None |
| 480 | `StaffWorkPriorityPanel` | `ClearSpawnedObjects` | `void` | `private` | None |
| 504 | `StaffWorkPriorityPanel` | `OnEnable` | `void` | `private` | None |
| 510 | `StaffWorkPriorityPanel` | `OnDisable` | `void` | `private` | None |
| 515 | `StaffWorkPriorityPanel` | `RequireModelBuilder` | `IStaffWorkPriorityPanelModelBuilder` | `private` | None |
| 521 | `StaffWorkPriorityPanel` | `RequireUiFactory` | `IStaffWorkPriorityPanelUiFactory` | `private` | None |

### `Assets/Scripts/Character/UI/StaffWorkPriorityPanelModel.cs`

- Functions detected: 10
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `StaffWorkPriorityRowModel` | `StaffWorkPriorityRowModel` | `ctor` | `public` | None |
| 20 | `IStaffWorkPriorityPanelModelBuilder` | `BuildRows` | `IReadOnlyList<StaffWorkPriorityRowModel>` | `-` | None |
| 21 | `IStaffWorkPriorityPanelModelBuilder` | `CalculateWorkerHash` | `int` | `-` | None |
| 22 | `IStaffWorkPriorityPanelModelBuilder` | `CalculateWorkerHash` | `int` | `-` | None |
| 23 | `IStaffWorkPriorityPanelModelBuilder` | `GetDisplayName` | `string` | `-` | None |
| 29 | `StaffWorkPriorityPanelModelBuilder` | `StaffWorkPriorityPanelModelBuilder` | `ctor` | `public` | None |
| 35 | `StaffWorkPriorityPanelModelBuilder` | `BuildRows` | `IReadOnlyList<StaffWorkPriorityRowModel>` | `public` | None |
| 53 | `StaffWorkPriorityPanelModelBuilder` | `CalculateWorkerHash` | `int` | `public` | None |
| 58 | `StaffWorkPriorityPanelModelBuilder` | `CalculateWorkerHash` | `int` | `public` | None |
| 89 | `StaffWorkPriorityPanelModelBuilder` | `GetDisplayName` | `string` | `public` | None |

### `Assets/Scripts/Character/UI/StaffWorkPriorityPanelUiFactory.cs`

- Functions detected: 29
- Judgment: UI type usage fits this UI/factory context; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `IStaffWorkPriorityPanelUiFactory` | `EnsureRectTransform` | `RectTransform` | `-` | UI type dependency |
| 9 | `IStaffWorkPriorityPanelUiFactory` | `CreateUiObject` | `GameObject` | `-` | None |
| 10 | `IStaffWorkPriorityPanelUiFactory` | `AddImage` | `Image` | `-` | UI type dependency |
| 11 | `IStaffWorkPriorityPanelUiFactory` | `AddScrollRect` | `ScrollRect` | `-` | None |
| 12 | `IStaffWorkPriorityPanelUiFactory` | `AddMask` | `Mask` | `-` | None |
| 13 | `IStaffWorkPriorityPanelUiFactory` | `AddVerticalLayoutGroup` | `VerticalLayoutGroup` | `-` | None |
| 14 | `IStaffWorkPriorityPanelUiFactory` | `AddHorizontalLayoutGroup` | `HorizontalLayoutGroup` | `-` | None |
| 15 | `IStaffWorkPriorityPanelUiFactory` | `AddContentSizeFitter` | `ContentSizeFitter` | `-` | None |
| 16 | `IStaffWorkPriorityPanelUiFactory` | `AddLayoutElement` | `LayoutElement` | `-` | None |
| 17 | `IStaffWorkPriorityPanelUiFactory` | `AddButton` | `Button` | `-` | UI type dependency |
| 18 | `IStaffWorkPriorityPanelUiFactory` | `AddText` | `TMP_Text` | `-` | UI type dependency |
| 19 | `IStaffWorkPriorityPanelUiFactory` | `AddShadow` | `Shadow` | `-` | None |
| 20 | `IStaffWorkPriorityPanelUiFactory` | `ApplyFonts` | `void` | `-` | None |
| 21 | `IStaffWorkPriorityPanelUiFactory` | `Release` | `void` | `-` | None |
| 27 | `StaffWorkPriorityPanelUiFactory` | `StaffWorkPriorityPanelUiFactory` | `ctor` | `public` | None |
| 33 | `StaffWorkPriorityPanelUiFactory` | `EnsureRectTransform` | `RectTransform` | `public` | UI type dependency |
| 46 | `StaffWorkPriorityPanelUiFactory` | `CreateUiObject` | `GameObject` | `public` | Runtime object creation, UI type dependency |
| 53 | `StaffWorkPriorityPanelUiFactory` | `AddImage` | `Image` | `public` | UI type dependency |
| 60 | `StaffWorkPriorityPanelUiFactory` | `AddScrollRect` | `ScrollRect` | `public` | None |
| 70 | `StaffWorkPriorityPanelUiFactory` | `AddMask` | `Mask` | `public` | None |
| 77 | `StaffWorkPriorityPanelUiFactory` | `AddVerticalLayoutGroup` | `VerticalLayoutGroup` | `public` | None |
| 88 | `StaffWorkPriorityPanelUiFactory` | `AddHorizontalLayoutGroup` | `HorizontalLayoutGroup` | `public` | None |
| 99 | `StaffWorkPriorityPanelUiFactory` | `AddContentSizeFitter` | `ContentSizeFitter` | `public` | None |
| 107 | `StaffWorkPriorityPanelUiFactory` | `AddLayoutElement` | `LayoutElement` | `public` | None |
| 117 | `StaffWorkPriorityPanelUiFactory` | `AddButton` | `Button` | `public` | UI type dependency |
| 124 | `StaffWorkPriorityPanelUiFactory` | `AddText` | `TMP_Text` | `public` | UI type dependency |
| 131 | `StaffWorkPriorityPanelUiFactory` | `AddShadow` | `Shadow` | `public` | None |
| 139 | `StaffWorkPriorityPanelUiFactory` | `ApplyFonts` | `void` | `public` | None |
| 144 | `StaffWorkPriorityPanelUiFactory` | `Release` | `void` | `public` | None |

### `Assets/Scripts/Character/Work/CharacterWorkRoleUtility.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 3 | `CharacterWorkRoleUtility` | `TryGetWork` | `bool` | `public static` | None |
| 10 | `CharacterWorkRoleUtility` | `IsWorker` | `bool` | `public static` | None |
| 15 | `CharacterWorkRoleUtility` | `IsOnDutyWorker` | `bool` | `public static` | None |

### `Assets/Scripts/Character/Work/StaffDiscontentSystem.cs`

- Functions detected: 42
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 52 | `StaffDiscontentRules` | `CreateDefault` | `StaffDiscontentRules` | `public static` | None |
| 73 | `StaffDiscontentSnapshot` | `ToSummaryText` | `string` | `public` | None |
| 82 | `StaffDiscontentRecord` | `StaffDiscontentRecord` | `ctor` | `public` | None |
| 100 | `StaffDiscontentRecord` | `Update` | `StaffDiscontentOutcome` | `public` | None |
| 165 | `StaffDiscontentRecord` | `MarkIsolated` | `bool` | `public` | None |
| 177 | `StaffDiscontentRecord` | `MarkSuppressed` | `bool` | `public` | None |
| 191 | `StaffDiscontentRecord` | `TryCalm` | `bool` | `public` | None |
| 221 | `StaffDiscontentRecord` | `ToSnapshot` | `StaffDiscontentSnapshot` | `public` | None |
| 244 | `StaffRebellionResponseResult` | `StaffRebellionResponseResult` | `ctor` | `public` | None |
| 270 | `StaffDiscontentState` | `ProcessStaff` | `StaffDiscontentRecord` | `public` | None |
| 284 | `StaffDiscontentState` | `TryGetRecord` | `bool` | `public` | None |
| 295 | `StaffDiscontentState` | `IsPermanentLoss` | `bool` | `public` | None |
| 300 | `StaffDiscontentState` | `GetOrCreate` | `StaffDiscontentRecord` | `private` | None |
| 316 | `StaffDiscontentChangedEvent` | `StaffDiscontentChangedEvent` | `ctor` | `public` | None |
| 323 | `StaffDiscontentChangedEvent` | `Trigger` | `void` | `public static` | None |
| 334 | `StaffPermanentLossEvent` | `StaffPermanentLossEvent` | `ctor` | `public` | None |
| 341 | `StaffPermanentLossEvent` | `Trigger` | `void` | `public static` | None |
| 352 | `StaffRebellionResponseEvent` | `StaffRebellionResponseEvent` | `ctor` | `public` | None |
| 359 | `StaffRebellionResponseEvent` | `Trigger` | `void` | `public static` | None |
| 369 | `StaffDiscontentService` | `IsTrackableStaff` | `bool` | `public static` | None |
| 378 | `StaffDiscontentService` | `GetStaffId` | `int` | `public static` | None |
| 389 | `StaffDiscontentService` | `GetStaffDisplayName` | `string` | `public static` | None |
| 405 | `StaffDiscontentService` | `GetMood` | `float` | `public static` | None |
| 418 | `StaffDiscontentService` | `EvaluateStage` | `StaffDiscontentStage` | `public static` | None |
| 454 | `StaffDiscontentService` | `GetWorkEfficiencyMultiplier` | `float` | `public static` | None |
| 468 | `StaffDiscontentService` | `ShouldBlockWork` | `bool` | `public static` | None |
| 475 | `StaffDiscontentService` | `GetBlockReason` | `string` | `public static` | None |
| 497 | `StaffDiscontentRuntime` | `Construct` | `void` | `public` | None |
| 504 | `StaffDiscontentRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 509 | `StaffDiscontentRuntime` | `ProcessStaff` | `StaffDiscontentRecord` | `public` | None |
| 525 | `StaffDiscontentRuntime` | `ProcessAllStaff` | `void` | `public` | None |
| 534 | `StaffDiscontentRuntime` | `GetWorkEfficiencyMultiplier` | `float` | `public` | None |
| 547 | `StaffDiscontentRuntime` | `ShouldBlockWork` | `bool` | `public` | None |
| 567 | `StaffDiscontentRuntime` | `IsRebellionTarget` | `bool` | `public` | None |
| 575 | `StaffDiscontentRuntime` | `DispatchAutoSuppress` | `int` | `public` | None |
| 629 | `StaffDiscontentRuntime` | `TryIsolateRebel` | `bool` | `public` | None |
| 651 | `StaffDiscontentRuntime` | `TryCalmStaff` | `bool` | `public` | None |
| 672 | `StaffDiscontentRuntime` | `ResolveSuppressedRebel` | `bool` | `public` | None |
| 695 | `StaffDiscontentRuntime` | `ApplyOutcome` | `void` | `private` | None |
| 738 | `StaffDiscontentRuntime` | `RequireSceneQuery` | `IDungeonSceneComponentQuery` | `private` | None |
| 748 | `StaffDiscontentRuntime` | `OnEnable` | `void` | `private` | None |
| 753 | `StaffDiscontentRuntime` | `OnDisable` | `void` | `private` | None |

### `Assets/Scripts/Character/Work/StaffWorkforceQueryService.cs`

- Functions detected: 7
- Judgment: scene discovery should be isolated behind providers/build callbacks.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IStaffWorkforceQueryService` | `FindActiveWorkers` | `IReadOnlyList<CharacterActor>` | `-` | None |
| 7 | `IStaffWorkforceQueryService` | `IsActiveWorker` | `bool` | `-` | None |
| 8 | `IStaffWorkforceQueryService` | `GetDisplayName` | `string` | `-` | None |
| 14 | `StaffWorkforceRuntimeQueryService` | `StaffWorkforceRuntimeQueryService` | `ctor` | `public` | None |
| 19 | `StaffWorkforceRuntimeQueryService` | `FindActiveWorkers` | `IReadOnlyList<CharacterActor>` | `public` | Scene query coupling |
| 28 | `StaffWorkforceRuntimeQueryService` | `IsActiveWorker` | `bool` | `public` | None |
| 35 | `StaffWorkforceRuntimeQueryService` | `GetDisplayName` | `string` | `public` | None |

### `Assets/Scripts/Character/Work/WorkCommandHandler.cs`

- Functions detected: 9
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 14 | `WorkCommandHandler` | `WorkCommandHandler` | `ctor` | `public` | None |
| 25 | `WorkCommandHandler` | `TrySetPriorityWorkTarget` | `bool` | `public` | None |
| 30 | `WorkCommandHandler` | `TrySetPriorityWorkTarget` | `bool` | `public` | None |
| 60 | `WorkCommandHandler` | `TrySetPrioritySuppressTarget` | `bool` | `public` | None |
| 91 | `WorkCommandHandler` | `TryGetPrioritySuppressDestination` | `bool` | `public` | None |
| 118 | `WorkCommandHandler` | `ClearPriorityWorkTarget` | `void` | `public` | None |
| 126 | `WorkCommandHandler` | `HasUrgentPriorityTarget` | `bool` | `public` | None |
| 138 | `WorkCommandHandler` | `SuppressPriorityTarget` | `IEnumerator` | `public` | Coroutine/timing orchestration |
| 215 | `WorkCommandHandler` | `CanReachSuppressTarget` | `bool` | `private` | None |

### `Assets/Scripts/Character/Work/WorkDebugLog.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 3 | `WorkDebugLog` | `LogProgress` | `void` | `public static` | None |
| 7 | `WorkDebugLog` | `LogEnd` | `void` | `public static` | None |
| 18 | `WorkDebugLog` | `GetCharacterName` | `string` | `private static` | None |

### `Assets/Scripts/Character/Work/WorkDutyController.cs`

- Functions detected: 18
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 11 | `WorkDutyController` | `WorkDutyController` | `ctor` | `public` | None |
| 19 | `WorkDutyController` | `InitializeWorkerCondition` | `void` | `public` | None |
| 34 | `WorkDutyController` | `ShouldUseRestProtection` | `bool` | `public` | None |
| 83 | `WorkDutyController` | `CanStartWorkAction` | `bool` | `public` | None |
| 147 | `WorkDutyController` | `ShouldTakeOffDuty` | `bool` | `public` | None |
| 165 | `WorkDutyController` | `ShouldReturnToWork` | `bool` | `public` | None |
| 187 | `WorkDutyController` | `BeginOffDuty` | `void` | `public` | None |
| 211 | `WorkDutyController` | `PrepareForExpedition` | `void` | `public` | None |
| 219 | `WorkDutyController` | `SetDutyState` | `void` | `public` | None |
| 241 | `WorkDutyController` | `RecoverOffDuty` | `void` | `public` | None |
| 260 | `WorkDutyController` | `ApplyWorkFatigueTick` | `void` | `public` | None |
| 271 | `WorkDutyController` | `CheckActionWork` | `IEnumerator` | `public` | Coroutine/timing orchestration |
| 325 | `WorkDutyController` | `ShouldInterruptCurrentWork` | `bool` | `public` | None |
| 362 | `WorkDutyController` | `ShouldEndRoutineWorkShift` | `bool` | `private` | None |
| 384 | `WorkDutyController` | `CanContinueAssignedWork` | `bool` | `public` | None |
| 433 | `WorkDutyController` | `EnsureStatAtLeast` | `void` | `private` | None |
| 444 | `WorkDutyController` | `GetStat` | `float` | `private` | None |
| 457 | `WorkDutyController` | `GetWorkerStats` | `CharacterStats` | `private` | None |

### `Assets/Scripts/Character/Work/WorkforceReplanService.cs`

- Functions detected: 3
- Judgment: scene discovery should be isolated behind providers/build callbacks.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IWorkforceReplanService` | `RequestIdleWorkersToReplan` | `void` | `-` | None |
| 13 | `DungeonWorkforceReplanService` | `DungeonWorkforceReplanService` | `ctor` | `public` | None |
| 19 | `DungeonWorkforceReplanService` | `RequestIdleWorkersToReplan` | `void` | `public` | Scene query coupling |

### `Assets/Scripts/Character/Work/WorkGridUtility.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IWorkGridResolver` | `ResolveActiveGrid` | `Grid` | `-` | None |
| 10 | `IWorkGridResolver` | `GetGridPosition` | `Vector2Int` | `-` | None |
| 17 | `WorkGridResolver` | `WorkGridResolver` | `ctor` | `public` | None |
| 23 | `WorkGridResolver` | `ResolveActiveGrid` | `Grid` | `public` | None |
| 47 | `WorkGridResolver` | `GetGridPosition` | `Vector2Int` | `public` | None |

### `Assets/Scripts/Character/Work/WorkPriorityProfile.cs`

- Functions detected: 17
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 15 | `WorkPriorityLevelExtensions` | `Next` | `WorkPriorityLevel` | `public static` | None |
| 25 | `WorkPriorityLevelExtensions` | `GetBaseScore` | `float` | `public static` | None |
| 36 | `WorkPriorityLevelExtensions` | `ToDisplayText` | `string` | `public static` | None |
| 62 | `WorkTaskCatalog` | `GetDisplayName` | `string` | `public static` | None |
| 78 | `WorkTaskCatalog` | `GetSingleTypes` | `IEnumerable<FacilityWorkType>` | `public static` | None |
| 93 | `WorkCommandResolver` | `TryResolveFacilityCommand` | `bool` | `public static` | None |
| 167 | `WorkCommandResolver` | `IsSuppressTarget` | `bool` | `public static` | None |
| 175 | `WorkCommandResolver` | `TryResolveSuppressCommand` | `bool` | `public static` | None |
| 222 | `WorkCommandResolver` | `TryUse` | `bool` | `private static` | None |
| 243 | `WorkCommandResolver` | `NeedsRestock` | `bool` | `private static` | None |
| 272 | `WorkPriorityProfile` | `CreateDefault` | `WorkPriorityProfile` | `public static` | None |
| 277 | `WorkPriorityProfile` | `Clone` | `WorkPriorityProfile` | `public` | None |
| 292 | `WorkPriorityProfile` | `GetPriority` | `WorkPriorityLevel` | `public` | None |
| 308 | `WorkPriorityProfile` | `SetPriority` | `void` | `public` | None |
| 339 | `WorkPriorityProfile` | `IsEnabled` | `bool` | `public` | None |
| 344 | `WorkPriorityProfile` | `ApplyPreferredTypes` | `void` | `public` | None |
| 356 | `WorkPriorityProfile` | `GetBestPriority` | `WorkPriorityLevel` | `private` | None |

### `Assets/Scripts/Character/Work/WorkTargetCandidate.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 3 | `WorkTargetCandidate` | `WorkTargetCandidate` | `ctor` | `public` | None |
| 30 | `WorkTargetCandidate` | `Invalid` | `WorkTargetCandidate` | `public static` | None |

### `Assets/Scripts/Character/Work/WorkTargetSelector.cs`

- Functions detected: 14
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `WorkTargetSelector` | `WorkTargetSelector` | `ctor` | `public` | None |
| 14 | `WorkTargetSelector` | `TryAssignWork` | `bool` | `public` | None |
| 82 | `WorkTargetSelector` | `CanUseAsWorkTarget` | `bool` | `public` | None |
| 87 | `WorkTargetSelector` | `CanUseAsWorkTarget` | `bool` | `public` | None |
| 92 | `WorkTargetSelector` | `HasUrgentAvailableWork` | `bool` | `public` | None |
| 108 | `WorkTargetSelector` | `TryGetBestCandidate` | `bool` | `public` | None |
| 139 | `WorkTargetSelector` | `GetUtilityScore` | `float` | `public` | None |
| 149 | `WorkTargetSelector` | `GetReachableBuildings` | `IEnumerable<BuildableObject>` | `public` | None |
| 167 | `WorkTargetSelector` | `FindReachableWarehouses` | `IEnumerable<IWarehouseFacility>` | `public` | None |
| 174 | `WorkTargetSelector` | `TryEvaluateWorkTarget` | `bool` | `public` | None |
| 325 | `WorkTargetSelector` | `BuildCandidate` | `WorkTargetCandidate` | `private` | None |
| 351 | `WorkTargetSelector` | `GetFailureRelevance` | `int` | `private static` | None |
| 364 | `WorkTargetSelector` | `GetDistanceScore` | `float` | `private static` | None |
| 383 | `WorkTargetSelector` | `CanUseSuppressFor` | `bool` | `private static` | None |

### `Assets/Scripts/Character/Work/WorkTaskExecutor.cs`

- Functions detected: 15
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 12 | `WorkTaskExecutor` | `WorkTaskExecutor` | `ctor` | `public` | None |
| 18 | `WorkTaskExecutor` | `Work` | `IEnumerator` | `public` | None |
| 137 | `WorkTaskExecutor` | `HasReachedAssignedWorkTarget` | `bool` | `private` | None |
| 148 | `WorkTaskExecutor` | `ExecuteRestockHaulWork` | `IEnumerator` | `private` | Coroutine/timing orchestration |
| 255 | `WorkTaskExecutor` | `TryCreateRestockHaulPlan` | `bool` | `private` | None |
| 315 | `WorkTaskExecutor` | `GetWarehousePickupWorldPosition` | `Vector3` | `private static` | None |
| 350 | `WorkTaskExecutor` | `TryGetPathToBuilding` | `bool` | `private` | None |
| 367 | `WorkTaskExecutor` | `ReturnCarriedStock` | `void` | `private` | None |
| 385 | `WorkTaskExecutor` | `ExecuteRestockWork` | `IEnumerator` | `public` | Coroutine/timing orchestration |
| 403 | `WorkTaskExecutor` | `ExecuteRepairWork` | `IEnumerator` | `public` | Coroutine/timing orchestration |
| 420 | `WorkTaskExecutor` | `ExecuteResearchWork` | `IEnumerator` | `public` | Coroutine/timing orchestration |
| 445 | `WorkTaskExecutor` | `EndAiAction` | `void` | `private static` | None |
| 454 | `WorkTaskExecutor` | `FinishWorkRun` | `void` | `private` | None |
| 470 | `WorkTaskExecutor` | `ShouldAbortWorkRun` | `bool` | `private` | None |
| 478 | `WorkTaskExecutor` | `AbortWorkRun` | `void` | `private` | None |

### `Assets/Scripts/Codex/CodexEvolutionRecorder.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 3 | `CodexEvolutionRecorder` | `Record` | `void` | `public static` | None |
| 43 | `CodexEvolutionRecorder` | `AddLineIfNotBlank` | `void` | `private static` | None |

### `Assets/Scripts/Codex/CodexFacilityInfoWriter.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 3 | `CodexFacilityInfoWriter` | `Add` | `void` | `public static` | None |
| 17 | `CodexFacilityInfoWriter` | `GetFacilityEntryId` | `string` | `public static` | None |

### `Assets/Scripts/Codex/CodexInvasionObservationMapper.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `CodexInvasionObservationMapper` | `FromEffectTag` | `IEnumerable<string>` | `public static` | None |
| 38 | `CodexInvasionObservationMapper` | `NormalizeObservation` | `string` | `public static` | None |

### `Assets/Scripts/Codex/CodexInvasionRecorder.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `CodexInvasionRecorder` | `RecordDefenseObservation` | `void` | `public static` | None |
| 34 | `CodexInvasionRecorder` | `RecordCombatReport` | `void` | `public static` | None |
| 57 | `CodexInvasionRecorder` | `RecordFacilityDamage` | `void` | `public static` | None |
| 63 | `CodexInvasionRecorder` | `SeedBreakthroughIntruder` | `void` | `public static` | None |
| 80 | `CodexInvasionRecorder` | `AddInvasionInfo` | `void` | `private static` | None |

### `Assets/Scripts/Codex/CodexObservationRecorder.cs`

- Functions detected: 6
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `CodexObservationRecorder` | `ObserveCharacter` | `void` | `public static` | None |
| 27 | `CodexObservationRecorder` | `ObserveSpecies` | `void` | `public static` | None |
| 59 | `CodexObservationRecorder` | `ObserveFacility` | `void` | `public static` | None |
| 69 | `CodexObservationRecorder` | `ObserveFacility` | `void` | `public static` | None |
| 127 | `CodexObservationRecorder` | `AddIfNotBlank` | `void` | `private static` | None |
| 135 | `CodexObservationRecorder` | `GetMonsterEntryId` | `string` | `private static` | None |

### `Assets/Scripts/Codex/CodexRecipeRecorder.cs`

- Functions detected: 6
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `CodexRecipeRecorder` | `RecordResearch` | `void` | `public static` | None |
| 44 | `CodexRecipeRecorder` | `RecordSynthesis` | `void` | `public static` | None |
| 60 | `CodexRecipeRecorder` | `ImportSynthesisRecipes` | `void` | `public static` | None |
| 89 | `CodexRecipeRecorder` | `AddRecipeInfo` | `void` | `private static` | None |
| 113 | `CodexRecipeRecorder` | `AddSpecialRecipeHint` | `void` | `private static` | None |
| 128 | `CodexRecipeRecorder` | `BuildSpecialRecipeHint` | `string` | `private static` | None |

### `Assets/Scripts/Codex/CodexRecordSummaryQuery.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `CodexRecordSummary` | `CodexRecordSummary` | `ctor` | `public` | None |
| 31 | `ICodexRecordSummaryService` | `Capture` | `CodexRecordSummary` | `-` | None |
| 44 | `CodexRecordSummaryRuntimeSource` | `CodexRecordSummaryRuntimeSource` | `ctor` | `public` | None |
| 58 | `CodexRecordSummaryService` | `CodexRecordSummaryService` | `ctor` | `public` | None |
| 63 | `CodexRecordSummaryService` | `Capture` | `CodexRecordSummary` | `public` | None |

### `Assets/Scripts/Codex/CodexReferenceImporter.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `ICodexReferenceImporter` | `Import` | `void` | `-` | None |
| 12 | `CodexReferenceImporter` | `CodexReferenceImporter` | `ctor` | `public` | None |
| 22 | `CodexReferenceImporter` | `Import` | `void` | `public` | None |

### `Assets/Scripts/Codex/CodexSystem.cs`

- Functions detected: 45
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 25 | `CodexInfoLine` | `CodexInfoLine` | `ctor` | `public` | None |
| 42 | `CodexEntrySnapshot` | `ToDisplayText` | `string` | `public` | None |
| 67 | `CodexEntryRecord` | `CodexEntryRecord` | `ctor` | `public` | None |
| 81 | `CodexEntryRecord` | `Rename` | `void` | `public` | None |
| 89 | `CodexEntryRecord` | `AddInfo` | `bool` | `public` | None |
| 107 | `CodexEntryRecord` | `ToSnapshot` | `CodexEntrySnapshot` | `public` | None |
| 126 | `CodexState` | `GetOrCreate` | `CodexEntryRecord` | `public` | None |
| 142 | `CodexState` | `AddInfo` | `bool` | `public` | None |
| 147 | `CodexState` | `HasInfo` | `bool` | `public` | None |
| 154 | `CodexState` | `GetSnapshots` | `IReadOnlyList<CodexEntrySnapshot>` | `public` | None |
| 163 | `CodexState` | `GetSnapshot` | `CodexEntrySnapshot` | `public` | None |
| 170 | `CodexState` | `GetKey` | `string` | `private static` | None |
| 181 | `CodexUpdatedEvent` | `CodexUpdatedEvent` | `ctor` | `public` | None |
| 189 | `CodexUpdatedEvent` | `Trigger` | `void` | `public static` | None |
| 201 | `CodexService` | `ImportReferenceData` | `void` | `public static` | None |
| 214 | `CodexService` | `ObserveCharacter` | `void` | `public static` | None |
| 219 | `CodexService` | `ObserveSpecies` | `void` | `public static` | None |
| 224 | `CodexService` | `ObserveFacility` | `void` | `public static` | None |
| 229 | `CodexService` | `ObserveFacility` | `void` | `public static` | None |
| 234 | `CodexService` | `RecordDefenseObservation` | `void` | `public static` | None |
| 239 | `CodexService` | `RecordCombatReport` | `void` | `public static` | None |
| 244 | `CodexService` | `RecordFacilityDamage` | `void` | `public static` | None |
| 249 | `CodexService` | `RecordResearch` | `void` | `public static` | None |
| 264 | `CodexService` | `RecordSynthesis` | `void` | `public static` | None |
| 273 | `CodexService` | `RecordEvolution` | `void` | `public static` | None |
| 278 | `CodexService` | `ImportSynthesisRecipes` | `void` | `public static` | None |
| 286 | `CodexService` | `SeedBreakthroughIntruder` | `void` | `public static` | None |
| 314 | `CodexRuntime` | `ConstructCodexRuntime` | `void` | `public` | None |
| 336 | `CodexRuntime` | `Start` | `void` | `private` | None |
| 344 | `CodexRuntime` | `ImportReferenceData` | `void` | `public` | None |
| 352 | `CodexRuntime` | `ResolveResearchStateService` | `IBlueprintResearchStateService` | `private` | None |
| 358 | `CodexRuntime` | `ResolveReferenceImporter` | `ICodexReferenceImporter` | `private` | None |
| 364 | `CodexRuntime` | `ResolveSynthesisRecipeQuery` | `IFacilitySynthesisRecipeQuery` | `private` | None |
| 370 | `CodexRuntime` | `ResolveFacilityShopCatalog` | `IFacilityShopCatalog` | `private` | None |
| 376 | `CodexRuntime` | `GetEntries` | `IReadOnlyList<CodexEntrySnapshot>` | `public` | None |
| 381 | `CodexRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 389 | `CodexRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 396 | `CodexRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 402 | `CodexRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 408 | `CodexRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 415 | `CodexRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 426 | `CodexRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 432 | `CodexRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 438 | `CodexRuntime` | `OnEnable` | `void` | `private` | None |
| 450 | `CodexRuntime` | `OnDisable` | `void` | `private` | None |

### `Assets/Scripts/Codex/CodexTextFormatter.cs`

- Functions detected: 10
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `CodexTextFormatter` | `FormatEvolutionMutationTags` | `string` | `public static` | None |
| 18 | `CodexTextFormatter` | `FormatFacilityRoles` | `string` | `public static` | None |
| 26 | `CodexTextFormatter` | `FormatWorkTypes` | `string` | `public static` | None |
| 34 | `CodexTextFormatter` | `FormatDefenseConcept` | `string` | `public static` | None |
| 48 | `CodexTextFormatter` | `FormatTriggerTimings` | `string` | `public static` | None |
| 68 | `CodexTextFormatter` | `FormatTargetRule` | `string` | `public static` | None |
| 80 | `CodexTextFormatter` | `FormatDefenseEffects` | `IEnumerable<string>` | `public static` | None |
| 93 | `CodexTextFormatter` | `FormatFacilityRole` | `string` | `private static` | None |
| 110 | `CodexTextFormatter` | `FormatWorkType` | `string` | `private static` | None |
| 126 | `CodexTextFormatter` | `FormatEffect` | `string` | `private static` | None |

### `Assets/Scripts/Codex/UI/CodexPanel.cs`

- Functions detected: 11
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 14 | `CodexPanel` | `Construct` | `void` | `public` | None |
| 21 | `CodexPanel` | `Bind` | `void` | `public` | None |
| 27 | `CodexPanel` | `BindGeneratedView` | `void` | `internal` | None |
| 34 | `CodexPanel` | `Refresh` | `void` | `public` | None |
| 46 | `CodexPanel` | `OnTriggerEvent` | `void` | `public` | None |
| 51 | `CodexPanel` | `AppendCategory` | `void` | `private static` | None |
| 86 | `CodexPanel` | `GetSummaryLines` | `IEnumerable<CodexInfoLine>` | `private static` | None |
| 93 | `CodexPanel` | `ApplyText` | `void` | `private` | None |
| 101 | `CodexPanel` | `ResolveRuntime` | `CodexRuntime` | `private` | None |
| 110 | `CodexPanel` | `OnEnable` | `void` | `private` | None |
| 115 | `CodexPanel` | `OnDisable` | `void` | `private` | None |

### `Assets/Scripts/Data.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 22 | `Data` | `Initialize` | `void` | `public` | None |
| 39 | `DataList` | `Add` | `void` | `public` | None |
| 45 | `DataList` | `Remove` | `void` | `public` | None |

### `Assets/Scripts/Defense/DefenseEffectSO.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 39 | `DefenseEffectSO` | `Apply` | `void` | `public` | None |

### `Assets/Scripts/Defense/DefenseFacilitySystem.cs`

- Functions detected: 21
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 61 | `DefenseEffectData` | `Clone` | `DefenseEffectData` | `public` | None |
| 91 | `DefenseFacilityData` | `SupportsTrigger` | `bool` | `public` | None |
| 101 | `DefenseActivationReport` | `DefenseActivationReport` | `ctor` | `public` | None |
| 120 | `DefenseActivationReport` | `AddDamage` | `void` | `public` | None |
| 125 | `DefenseActivationReport` | `AddMovementDelay` | `void` | `public` | None |
| 130 | `DefenseActivationReport` | `AddEffectTag` | `void` | `public` | None |
| 138 | `DefenseActivationReport` | `FormatSummary` | `string` | `public` | None |
| 153 | `DefenseFacilityTriggeredEvent` | `DefenseFacilityTriggeredEvent` | `ctor` | `public` | None |
| 160 | `DefenseFacilityTriggeredEvent` | `Trigger` | `void` | `public static` | None |
| 173 | `DefenseFacility` | `CanTrigger` | `bool` | `public` | None |
| 210 | `DefenseFacility` | `Trigger` | `DefenseActivationReport` | `public` | None |
| 233 | `DefenseFacilityResolver` | `TriggerAt` | `List<DefenseActivationReport>` | `public static` | None |
| 277 | `DefenseEffectResolver` | `ApplyEffects` | `void` | `public static` | None |
| 323 | `DefenseEffectResolver` | `ApplyEffect` | `void` | `public static` | None |
| 370 | `DefenseEffectResolver` | `TickStatuses` | `float` | `public static` | None |
| 388 | `DefenseEffectResolver` | `ApplyDamage` | `void` | `private static` | None |
| 410 | `DefenseStatusRuntime` | `ApplyStatus` | `int` | `public` | None |
| 427 | `DefenseStatusRuntime` | `ClearStatus` | `void` | `public` | None |
| 432 | `DefenseStatusRuntime` | `GetIncomingDamageMultiplier` | `float` | `public` | None |
| 438 | `DefenseStatusRuntime` | `Tick` | `float` | `public` | None |
| 471 | `DefenseRuntimeStatus` | `DefenseRuntimeStatus` | `ctor` | `public` | None |

### `Assets/Scripts/Defense/DefenseStatusRuntimeFactory.cs`

- Functions detected: 4
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `IDefenseStatusRuntimeFactory` | `GetOrAdd` | `DefenseStatusRuntime` | `-` | None |
| 6 | `IDefenseStatusRuntimeFactory` | `Get` | `DefenseStatusRuntime` | `-` | None |
| 11 | `DefenseStatusRuntimeFactory` | `GetOrAdd` | `DefenseStatusRuntime` | `public` | None |
| 25 | `DefenseStatusRuntimeFactory` | `Get` | `DefenseStatusRuntime` | `public` | None |

### `Assets/Scripts/Defense/DefenseStatusRuntimeService.cs`

- Functions detected: 7
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `IDefenseStatusRuntimeService` | `GetOrAdd` | `DefenseStatusRuntime` | `-` | None |
| 6 | `IDefenseStatusRuntimeService` | `Get` | `DefenseStatusRuntime` | `-` | None |
| 7 | `IDefenseStatusRuntimeService` | `TickStatuses` | `float` | `-` | None |
| 13 | `DefenseStatusRuntimeService` | `DefenseStatusRuntimeService` | `ctor` | `public` | None |
| 19 | `DefenseStatusRuntimeService` | `GetOrAdd` | `DefenseStatusRuntime` | `public` | None |
| 24 | `DefenseStatusRuntimeService` | `Get` | `DefenseStatusRuntime` | `public` | None |
| 29 | `DefenseStatusRuntimeService` | `TickStatuses` | `float` | `public` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionBuildingReplacerFactory.cs`

- Functions detected: 4
- Judgment: manual resolver use is acceptable only in composition roots/factories.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IFacilityEvolutionBuildingReplacerFactory` | `Create` | `IFacilityEvolutionBuildingReplacer` | `-` | None |
| 14 | `GridFacilityEvolutionBuildingReplacerFactory` | `GridFacilityEvolutionBuildingReplacerFactory` | `ctor` | `public` | None |
| 27 | `GridFacilityEvolutionBuildingReplacerFactory` | `Create` | `IFacilityEvolutionBuildingReplacer` | `public` | None |
| 36 | `GridFacilityEvolutionBuildingReplacerFactory` | `InjectCreatedBuilding` | `void` | `private` | Manual resolver/injection |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionIdentity.cs`

- Functions detected: 26
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `IFacilityIdentityPressureProvider` | `Apply` | `void` | `-` | None |
| 13 | `FacilityEvolutionIdentityScore` | `FacilityEvolutionIdentityScore` | `ctor` | `public` | None |
| 30 | `FacilityEvolutionIdentityScore` | `ToMessage` | `string` | `public` | None |
| 47 | `FacilityIdentitySnapshot` | `FacilityIdentitySnapshot` | `ctor` | `public` | None |
| 92 | `FacilityIdentitySnapshot` | `BuildRoomSummary` | `string` | `private static` | None |
| 134 | `DefaultFacilityIdentityPressureProvider` | `Apply` | `void` | `public` | None |
| 142 | `FacilityIdentityPressureUtility` | `ApplyDefaultPressures` | `void` | `public static` | None |
| 161 | `FacilityIdentityPressureUtility` | `ScoreRecipe` | `FacilityEvolutionIdentityScore` | `public static` | None |
| 212 | `FacilityIdentityPressureUtility` | `ApplyPressure` | `void` | `private static` | None |
| 235 | `FacilityIdentityPressureUtility` | `CrowdSignals` | `IEnumerable<IdentitySignal>` | `private static` | None |
| 244 | `FacilityIdentityPressureUtility` | `LuxurySignals` | `IEnumerable<IdentitySignal>` | `private static` | None |
| 257 | `FacilityIdentityPressureUtility` | `CoreLuxuryStrength` | `float` | `private static` | None |
| 267 | `FacilityIdentityPressureUtility` | `CombatSignals` | `IEnumerable<IdentitySignal>` | `private static` | None |
| 279 | `FacilityIdentityPressureUtility` | `OutlawSignals` | `IEnumerable<IdentitySignal>` | `private static` | None |
| 289 | `FacilityIdentityPressureUtility` | `RestSignals` | `IEnumerable<IdentitySignal>` | `private static` | None |
| 299 | `FacilityIdentityPressureUtility` | `ServiceSignals` | `IEnumerable<IdentitySignal>` | `private static` | None |
| 309 | `FacilityIdentityPressureUtility` | `RitualSignals` | `IEnumerable<IdentitySignal>` | `private static` | None |
| 318 | `FacilityIdentityPressureUtility` | `SecuritySignals` | `IEnumerable<IdentitySignal>` | `private static` | None |
| 328 | `FacilityIdentityPressureUtility` | `LogisticsSignals` | `IEnumerable<IdentitySignal>` | `private static` | None |
| 337 | `FacilityIdentityPressureUtility` | `DetectConflicts` | `void` | `private static` | None |
| 344 | `FacilityIdentityPressureUtility` | `AddConflict` | `void` | `private static` | None |
| 354 | `FacilityIdentityPressureUtility` | `Signal` | `IdentitySignal` | `private static` | None |
| 359 | `FacilityIdentityPressureUtility` | `Token01` | `float` | `private static` | None |
| 364 | `FacilityIdentityPressureUtility` | `Normalize` | `float` | `private static` | None |
| 374 | `FacilityIdentityPressureUtility` | `InverseNormalize` | `float` | `private static` | None |
| 392 | `IdentitySignal` | `IdentitySignal` | `ctor` | `public` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionLlmProposalProvider.cs`

- Functions detected: 20
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 25 | `FacilityEvolutionProposalJsonDto` | `Validate` | `bool` | `public` | None |
| 71 | `FacilityEvolutionProposalJsonDto` | `ValidateReasonArray` | `bool` | `private static` | None |
| 103 | `FacilityEvolutionProposalJsonDto` | `ToRuntimeProposal` | `FacilityEvolutionProposal` | `public` | None |
| 192 | `CachedLocalLlmFacilityEvolutionProposalProvider` | `CachedLocalLlmFacilityEvolutionProposalProvider` | `ctor` | `public` | None |
| 206 | `CachedLocalLlmFacilityEvolutionProposalProvider` | `Propose` | `FacilityEvolutionProposal` | `public` | None |
| 236 | `CachedLocalLlmFacilityEvolutionProposalProvider` | `TryRequestProposal` | `void` | `private` | None |
| 275 | `CachedLocalLlmFacilityEvolutionProposalProvider` | `OnLlmResult` | `void` | `private` | None |
| 303 | `CachedLocalLlmFacilityEvolutionProposalProvider` | `SetStatus` | `void` | `private` | None |
| 310 | `CachedLocalLlmFacilityEvolutionProposalProvider` | `Rewrap` | `FacilityEvolutionProposal` | `private static` | None |
| 331 | `FacilityEvolutionPromptFormatter` | `BuildSignature` | `string` | `public static` | None |
| 349 | `FacilityEvolutionPromptFormatter` | `BuildPrompt` | `string` | `public static` | None |
| 404 | `FacilityEvolutionPromptFormatter` | `AppendPairs` | `void` | `private static` | None |
| 417 | `FacilityEvolutionPromptFormatter` | `AppendTokenPairs` | `void` | `private static` | None |
| 430 | `FacilityEvolutionPromptFormatter` | `AppendList` | `void` | `private static` | None |
| 441 | `FacilityEvolutionPromptFormatter` | `JoinValues` | `string` | `private static` | None |
| 451 | `FacilityEvolutionPromptFormatter` | `JoinPairs` | `string` | `private static` | None |
| 464 | `FacilityEvolutionPromptFormatter` | `JoinTokenPairs` | `string` | `private static` | None |
| 477 | `FacilityEvolutionPromptFormatter` | `JoinRequirements` | `string` | `private static` | None |
| 495 | `FacilityEvolutionPromptFormatter` | `JoinTokenRequirements` | `string` | `private static` | None |
| 507 | `FacilityEvolutionPromptFormatter` | `JoinIdentityWeights` | `string` | `private static` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionMutations.cs`

- Functions detected: 6
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `FacilityEvolutionMutationResult` | `FacilityEvolutionMutationResult` | `ctor` | `public` | None |
| 22 | `IFacilityEvolutionMutationResolver` | `Resolve` | `FacilityEvolutionMutationResult` | `-` | None |
| 31 | `DefaultFacilityEvolutionMutationResolver` | `Resolve` | `FacilityEvolutionMutationResult` | `public` | None |
| 84 | `DefaultFacilityEvolutionMutationResolver` | `TryAddMutation` | `void` | `private static` | None |
| 105 | `DefaultFacilityEvolutionMutationResolver` | `HasEvidence` | `bool` | `private static` | None |
| 110 | `DefaultFacilityEvolutionMutationResolver` | `GetTagEvidenceScore` | `float` | `private static` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionRecipeSO.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 50 | `FacilityEvolutionRecipeSO` | `MatchesSource` | `bool` | `public` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionRecord.cs`

- Functions detected: 22
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `IFacilityEvolutionRecordProvider` | `GetRecord` | `FacilityEvolutionRecord` | `-` | None |
| 13 | `IFacilityEvolutionRecordComponentService` | `GetOrAdd` | `FacilityEvolutionRecordComponent` | `-` | None |
| 14 | `IFacilityEvolutionRecordComponentService` | `ReplaceWith` | `void` | `-` | None |
| 26 | `FacilityEvolutionRecord` | `GetMetric` | `float` | `public` | None |
| 31 | `FacilityEvolutionRecord` | `GetToken` | `int` | `public` | None |
| 36 | `FacilityEvolutionRecord` | `AddMetric` | `void` | `public` | None |
| 46 | `FacilityEvolutionRecord` | `AddToken` | `void` | `public` | None |
| 57 | `FacilityEvolutionRecord` | `SetToken` | `void` | `public` | None |
| 67 | `FacilityEvolutionRecord` | `TryConsumeToken` | `bool` | `public` | None |
| 87 | `FacilityEvolutionRecord` | `AddEvent` | `void` | `public` | None |
| 95 | `FacilityEvolutionRecord` | `TryConsumeTokens` | `bool` | `public` | None |
| 129 | `FacilityEvolutionRecord` | `Clone` | `FacilityEvolutionRecord` | `public` | None |
| 157 | `FacilityEvolutionRecordComponent` | `GetRecord` | `FacilityEvolutionRecord` | `public` | None |
| 187 | `FacilityEvolutionRecordComponent` | `SetMetric` | `void` | `public` | None |
| 203 | `FacilityEvolutionRecordComponent` | `AddToken` | `void` | `public` | None |
| 219 | `FacilityEvolutionRecordComponent` | `AddRecentEvent` | `void` | `public` | None |
| 232 | `FacilityEvolutionRecordComponent` | `ReplaceWith` | `void` | `public` | None |
| 259 | `ComponentFacilityEvolutionRecordProvider` | `GetRecord` | `FacilityEvolutionRecord` | `public` | None |
| 274 | `FacilityEvolutionRecordComponentService` | `FacilityEvolutionRecordComponentService` | `ctor` | `public` | None |
| 281 | `FacilityEvolutionRecordComponentService` | `GetRecord` | `FacilityEvolutionRecord` | `public` | None |
| 292 | `FacilityEvolutionRecordComponentService` | `GetOrAdd` | `FacilityEvolutionRecordComponent` | `public` | None |
| 297 | `FacilityEvolutionRecordComponentService` | `ReplaceWith` | `void` | `public` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionRecordComponentFactory.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 3 | `IFacilityEvolutionRecordComponentFactory` | `GetOrAdd` | `FacilityEvolutionRecordComponent` | `-` | None |
| 8 | `FacilityEvolutionRecordComponentFactory` | `GetOrAdd` | `FacilityEvolutionRecordComponent` | `public` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionRecordEventRecorder.cs`

- Functions detected: 36
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IFacilityEvolutionRecordEventRecorder` | `RecordVisit` | `void` | `-` | None |
| 8 | `IFacilityEvolutionRecordEventRecorder` | `RecordRevenue` | `void` | `-` | None |
| 9 | `IFacilityEvolutionRecordEventRecorder` | `RecordStockConsumed` | `void` | `-` | None |
| 10 | `IFacilityEvolutionRecordEventRecorder` | `RecordCrime` | `void` | `-` | None |
| 11 | `IFacilityEvolutionRecordEventRecorder` | `RecordRestock` | `void` | `-` | None |
| 12 | `IFacilityEvolutionRecordEventRecorder` | `RecordDefenseTriggered` | `void` | `-` | None |
| 13 | `IFacilityEvolutionRecordEventRecorder` | `RecordInvasionDamage` | `void` | `-` | None |
| 14 | `IFacilityEvolutionRecordEventRecorder` | `CompleteOperatingDay` | `void` | `-` | None |
| 28 | `FacilityEvolutionRecordEventRecorder` | `FacilityEvolutionRecordEventRecorder` | `ctor` | `public` | None |
| 38 | `FacilityEvolutionRecordEventRecorder` | `RecordVisit` | `void` | `public` | None |
| 103 | `FacilityEvolutionRecordEventRecorder` | `RecordRevenue` | `void` | `public` | None |
| 128 | `FacilityEvolutionRecordEventRecorder` | `RecordStockConsumed` | `void` | `public` | None |
| 151 | `FacilityEvolutionRecordEventRecorder` | `RecordCrime` | `void` | `public` | None |
| 180 | `FacilityEvolutionRecordEventRecorder` | `RecordRestock` | `void` | `public` | None |
| 200 | `FacilityEvolutionRecordEventRecorder` | `RecordDefenseTriggered` | `void` | `public` | None |
| 226 | `FacilityEvolutionRecordEventRecorder` | `RecordInvasionDamage` | `void` | `public` | None |
| 242 | `FacilityEvolutionRecordEventRecorder` | `CompleteOperatingDay` | `void` | `public` | None |
| 264 | `FacilityEvolutionRecordEventRecorder` | `GetDayRecord` | `FacilityDayRecord` | `private` | None |
| 276 | `FacilityEvolutionRecordEventRecorder` | `MarkDynamicStateDirty` | `void` | `private` | None |
| 281 | `FacilityEvolutionRecordEventRecorder` | `GetUniqueVisitors` | `HashSet<int>` | `private` | None |
| 293 | `FacilityEvolutionRecordEventRecorder` | `GetOrAddRecord` | `FacilityEvolutionRecordComponent` | `private` | None |
| 298 | `FacilityEvolutionRecordEventRecorder` | `IncrementMetric` | `void` | `private static` | None |
| 308 | `FacilityEvolutionRecordEventRecorder` | `SetMetric` | `void` | `private static` | None |
| 313 | `FacilityEvolutionRecordEventRecorder` | `GetMetric` | `float` | `private static` | None |
| 318 | `FacilityEvolutionRecordEventRecorder` | `UpdateAverage` | `float` | `private static` | None |
| 328 | `FacilityEvolutionRecordEventRecorder` | `UpdateRatio` | `float` | `private static` | None |
| 339 | `FacilityEvolutionRecordEventRecorder` | `IsValidFacility` | `bool` | `private static` | None |
| 344 | `FacilityEvolutionRecordEventRecorder` | `GetFacilityKey` | `int` | `private static` | None |
| 349 | `FacilityEvolutionRecordEventRecorder` | `GetVisitorId` | `int` | `private static` | None |
| 356 | `FacilityEvolutionRecordEventRecorder` | `SupportsRole` | `bool` | `private static` | None |
| 361 | `FacilityEvolutionRecordEventRecorder` | `IsCombatVisitor` | `bool` | `private static` | None |
| 375 | `FacilityEvolutionRecordEventRecorder` | `IsNobleVisitor` | `bool` | `private static` | None |
| 388 | `FacilityEvolutionRecordEventRecorder` | `GetCondition` | `float` | `private static` | None |
| 398 | `FacilityEvolutionRecordEventRecorder` | `GetActorName` | `string` | `private static` | None |
| 403 | `FacilityEvolutionRecordEventRecorder` | `GetFacilityName` | `string` | `private static` | None |
| 411 | `FacilityDayRecord` | `FacilityDayRecord` | `ctor` | `public` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionRecordRuntime.cs`

- Functions detected: 12
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 21 | `FacilityEvolutionRecordRuntime` | `Construct` | `void` | `public` | None |
| 28 | `FacilityEvolutionRecordRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 33 | `FacilityEvolutionRecordRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 38 | `FacilityEvolutionRecordRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 43 | `FacilityEvolutionRecordRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 48 | `FacilityEvolutionRecordRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 53 | `FacilityEvolutionRecordRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 58 | `FacilityEvolutionRecordRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 63 | `FacilityEvolutionRecordRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 68 | `FacilityEvolutionRecordRuntime` | `OnEnable` | `void` | `private` | None |
| 80 | `FacilityEvolutionRecordRuntime` | `OnDisable` | `void` | `private` | None |
| 92 | `FacilityEvolutionRecordRuntime` | `ResolveRecordEventRecorder` | `IFacilityEvolutionRecordEventRecorder` | `private` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionRecordTokens.cs`

- Functions detected: 7
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 37 | `IFacilityEvolutionRecordTokenDefinitionProvider` | `GetDefinitions` | `IReadOnlyList<FacilityEvolutionRecordTokenDefinitionSO>` | `-` | None |
| 38 | `IFacilityEvolutionRecordTokenDefinitionProvider` | `GetDefinition` | `FacilityEvolutionRecordTokenDefinitionSO` | `-` | None |
| 44 | `EmptyFacilityEvolutionRecordTokenDefinitionProvider` | `GetDefinitions` | `IReadOnlyList<FacilityEvolutionRecordTokenDefinitionSO>` | `public` | None |
| 48 | `EmptyFacilityEvolutionRecordTokenDefinitionProvider` | `GetDefinition` | `FacilityEvolutionRecordTokenDefinitionSO` | `public` | None |
| 57 | `IFacilityEvolutionRecordTokenConsumer` | `TryConsume` | `bool` | `-` | None |
| 67 | `DefaultFacilityEvolutionRecordTokenConsumer` | `DefaultFacilityEvolutionRecordTokenConsumer` | `ctor` | `public` | None |
| 74 | `DefaultFacilityEvolutionRecordTokenConsumer` | `TryConsume` | `bool` | `public` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionRuntime.cs`

- Functions detected: 35
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `IFacilityEvolutionRecipeProvider` | `GetRecipes` | `IReadOnlyList<FacilityEvolutionRecipeSO>` | `-` | None |
| 14 | `IFacilityEvolutionBuildingReplacer` | `CanReplace` | `bool` | `-` | None |
| 15 | `IFacilityEvolutionBuildingReplacer` | `TryReplace` | `bool` | `-` | None |
| 21 | `GridFacilityEvolutionBuildingReplacer` | `GridFacilityEvolutionBuildingReplacer` | `ctor` | `public` | None |
| 27 | `GridFacilityEvolutionBuildingReplacer` | `CanReplace` | `bool` | `public` | None |
| 68 | `GridFacilityEvolutionBuildingReplacer` | `TryReplace` | `bool` | `public` | None |
| 118 | `FacilityEvolutionResult` | `FacilityEvolutionResult` | `ctor` | `public` | None |
| 151 | `FacilityEvolutionCompletedEvent` | `FacilityEvolutionCompletedEvent` | `ctor` | `public` | None |
| 158 | `FacilityEvolutionCompletedEvent` | `Trigger` | `void` | `public static` | None |
| 168 | `IFacilityEvolutionValidator` | `Validate` | `FacilityEvolutionValidationResult` | `-` | None |
| 180 | `DefaultFacilityEvolutionValidator` | `DefaultFacilityEvolutionValidator` | `ctor` | `public` | None |
| 190 | `DefaultFacilityEvolutionValidator` | `Validate` | `FacilityEvolutionValidationResult` | `public` | None |
| 227 | `IFacilityEvolutionCandidateBuilder` | `Build` | `FacilityEvolutionCandidate` | `-` | None |
| 240 | `DefaultFacilityEvolutionCandidateBuilder` | `DefaultFacilityEvolutionCandidateBuilder` | `ctor` | `public` | None |
| 246 | `DefaultFacilityEvolutionCandidateBuilder` | `Build` | `FacilityEvolutionCandidate` | `public` | None |
| 310 | `FacilityEvolutionEngine` | `FacilityEvolutionEngine` | `ctor` | `public` | None |
| 358 | `FacilityEvolutionEngine` | `BuildContext` | `FacilityEvolutionContext` | `public` | None |
| 367 | `FacilityEvolutionEngine` | `GetCandidates` | `IReadOnlyList<FacilityEvolutionCandidate>` | `public` | None |
| 395 | `FacilityEvolutionEngine` | `TryEvolve` | `bool` | `public` | None |
| 501 | `FacilityEvolutionEngine` | `ConsumeMaterials` | `bool` | `private` | None |
| 522 | `FacilityEvolutionEngine` | `Fail` | `FacilityEvolutionResult` | `private static` | None |
| 563 | `FacilityEvolutionRuntime` | `ConstructFacilityEvolutionRuntime` | `void` | `public` | None |
| 605 | `FacilityEvolutionRuntime` | `Configure` | `void` | `public` | None |
| 642 | `FacilityEvolutionRuntime` | `BuildContext` | `FacilityEvolutionContext` | `public` | None |
| 647 | `FacilityEvolutionRuntime` | `GetCandidates` | `IReadOnlyList<FacilityEvolutionCandidate>` | `public` | None |
| 654 | `FacilityEvolutionRuntime` | `TryEvolve` | `bool` | `public` | None |
| 678 | `FacilityEvolutionRuntime` | `CreateEngine` | `FacilityEvolutionEngine` | `private` | None |
| 707 | `FacilityEvolutionRuntime` | `CreateDefaultProposalProvider` | `IFacilityEvolutionProposalProvider` | `private` | None |
| 715 | `FacilityEvolutionRuntime` | `ResolveLocalLlmRuntime` | `ILocalLlmRuntime` | `private` | None |
| 727 | `FacilityEvolutionRuntime` | `ResolveResearchStateService` | `IBlueprintResearchStateService` | `private` | None |
| 733 | `FacilityEvolutionRuntime` | `ResolveRoomLayoutCache` | `IRoomLayoutCache` | `private` | None |
| 739 | `FacilityEvolutionRuntime` | `ResolveStateService` | `IFacilityEvolutionStateService` | `private` | None |
| 745 | `FacilityEvolutionRuntime` | `ResolveFacilityCandidateCache` | `IFacilityCandidateCache` | `private` | None |
| 751 | `FacilityEvolutionRuntime` | `ResolveRecordComponentService` | `IFacilityEvolutionRecordComponentService` | `private` | None |
| 758 | `FacilityEvolutionRuntime` | `ResolveBuildingReplacerFactory` | `IFacilityEvolutionBuildingReplacerFactory` | `private` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionService.cs`

- Functions detected: 33
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `IFacilityEvolutionResourceProvider` | `HasMaterial` | `bool` | `-` | None |
| 9 | `IFacilityEvolutionResourceProvider` | `ConsumeMaterial` | `bool` | `-` | None |
| 14 | `IFacilityEvolutionProposalProvider` | `Propose` | `FacilityEvolutionProposal` | `-` | None |
| 19 | `FacilityEvolutionProposal` | `FacilityEvolutionProposal` | `ctor` | `public` | None |
| 63 | `RuleBasedFacilityEvolutionProposalProvider` | `Propose` | `FacilityEvolutionProposal` | `public` | None |
| 103 | `RuleBasedFacilityEvolutionProposalProvider` | `BuildReason` | `string` | `private static` | None |
| 151 | `RuleBasedFacilityEvolutionProposalProvider` | `BuildRejectedHint` | `string` | `private static` | None |
| 182 | `RuleBasedFacilityEvolutionProposalProvider` | `BuildSummary` | `string` | `private static` | None |
| 216 | `RuleBasedFacilityEvolutionProposalProvider` | `GuessMutationTags` | `IReadOnlyList<string>` | `private static` | None |
| 242 | `EmptyFacilityEvolutionResourceProvider` | `HasMaterial` | `bool` | `public` | None |
| 246 | `EmptyFacilityEvolutionResourceProvider` | `ConsumeMaterial` | `bool` | `public` | None |
| 256 | `MemoryFacilityEvolutionResourceProvider` | `SetMaterial` | `void` | `public` | None |
| 264 | `MemoryFacilityEvolutionResourceProvider` | `HasMaterial` | `bool` | `public` | None |
| 274 | `MemoryFacilityEvolutionResourceProvider` | `ConsumeMaterial` | `bool` | `public` | None |
| 293 | `FacilityEvolutionContext` | `FacilityEvolutionContext` | `ctor` | `public` | None |
| 319 | `FacilityEvolutionValidationResult` | `Reject` | `void` | `public` | None |
| 328 | `FacilityEvolutionValidationResult` | `AddCheck` | `void` | `public` | None |
| 338 | `FacilityEvolutionValidationResult` | `ToMessage` | `string` | `public` | None |
| 347 | `FacilityEvolutionValidationCheck` | `FacilityEvolutionValidationCheck` | `ctor` | `public` | None |
| 367 | `FacilityEvolutionCandidate` | `FacilityEvolutionCandidate` | `ctor` | `public` | None |
| 374 | `FacilityEvolutionCandidate` | `FacilityEvolutionCandidate` | `ctor` | `public` | None |
| 386 | `FacilityEvolutionCandidate` | `FacilityEvolutionCandidate` | `ctor` | `public` | None |
| 423 | `FacilityEvolutionUtility` | `GetFacilityId` | `string` | `public static` | None |
| 432 | `FacilityEvolutionUtility` | `GetDefaultLineageTags` | `IEnumerable<string>` | `public static` | None |
| 463 | `FacilityEvolutionService` | `IsRecipeVisible` | `bool` | `public static` | None |
| 489 | `FacilityEvolutionService` | `GetSourceCandidates` | `IReadOnlyList<FacilityEvolutionRecipeSO>` | `public static` | None |
| 517 | `FacilityEvolutionService` | `Validate` | `FacilityEvolutionValidationResult` | `public static` | None |
| 581 | `FacilityEvolutionService` | `ValidateTags` | `void` | `private static` | None |
| 604 | `FacilityEvolutionService` | `ValidateMetricRequirements` | `void` | `private static` | None |
| 628 | `FacilityEvolutionService` | `ValidateTokenRequirements` | `void` | `private static` | None |
| 651 | `FacilityEvolutionService` | `ValidateIdentityPressure` | `void` | `private static` | None |
| 673 | `FacilityEvolutionService` | `ValidateUniqueFixtures` | `void` | `private static` | None |
| 702 | `FacilityEvolutionService` | `ValidateMaterials` | `void` | `private static` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionState.cs`

- Functions detected: 9
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 14 | `FacilityEvolutionHistoryEntry` | `Clone` | `FacilityEvolutionHistoryEntry` | `public` | None |
| 63 | `FacilityEvolutionStateComponent` | `CreateSnapshot` | `FacilityEvolutionStateSnapshot` | `public` | None |
| 82 | `FacilityEvolutionStateComponent` | `ApplySnapshot` | `void` | `public` | None |
| 119 | `FacilityEvolutionStateComponent` | `InitializeIfNeeded` | `void` | `public` | None |
| 144 | `FacilityEvolutionStateComponent` | `ApplyEvolution` | `void` | `public` | None |
| 200 | `FacilityEvolutionStateComponent` | `CaptureIdentity` | `void` | `private` | None |
| 226 | `IFacilityEvolutionStateService` | `GetOrAdd` | `FacilityEvolutionStateComponent` | `-` | None |
| 232 | `FacilityEvolutionStateService` | `FacilityEvolutionStateService` | `ctor` | `public` | None |
| 238 | `FacilityEvolutionStateService` | `GetOrAdd` | `FacilityEvolutionStateComponent` | `public` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionStateComponentFactory.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 3 | `IFacilityEvolutionStateComponentFactory` | `GetOrAdd` | `FacilityEvolutionStateComponent` | `-` | None |
| 8 | `FacilityEvolutionStateComponentFactory` | `GetOrAdd` | `FacilityEvolutionStateComponent` | `public` | None |

### `Assets/Scripts/FacilityEvolution/FacilityEvolutionTerms.cs`

- Functions detected: 4
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 119 | `FacilityEvolutionValue` | `FacilityEvolutionValue` | `ctor` | `public` | None |
| 132 | `FacilityEvolutionTokenValue` | `FacilityEvolutionTokenValue` | `ctor` | `public` | None |
| 162 | `FacilityEvolutionMetricRequirement` | `IsSatisfied` | `bool` | `public` | None |
| 193 | `FacilityEvolutionTokenRequirement` | `IsSatisfied` | `bool` | `public` | None |

### `Assets/Scripts/FacilityEvolution/RoomProfile.cs`

- Functions detected: 25
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `IRoomProfileProvider` | `Build` | `RoomProfile` | `-` | None |
| 22 | `RoomProfile` | `RoomProfile` | `ctor` | `public` | None |
| 45 | `RoomProfile` | `GetScore` | `float` | `public` | None |
| 50 | `RoomProfile` | `GetMetric` | `float` | `public` | None |
| 55 | `RoomProfile` | `GetIdentityPressure` | `float` | `public` | None |
| 60 | `RoomProfile` | `GetToken` | `int` | `public` | None |
| 65 | `RoomProfile` | `HasTag` | `bool` | `public` | None |
| 70 | `RoomProfile` | `AddTag` | `void` | `public` | None |
| 78 | `RoomProfile` | `AddScore` | `void` | `public` | None |
| 89 | `RoomProfile` | `AddMetric` | `void` | `public` | None |
| 99 | `RoomProfile` | `AddMetricDelta` | `void` | `public` | None |
| 110 | `RoomProfile` | `SetIdentityPressure` | `void` | `public` | None |
| 120 | `RoomProfile` | `AddRecordToken` | `void` | `public` | None |
| 131 | `RoomProfile` | `AddFixture` | `void` | `public` | None |
| 139 | `RoomProfile` | `AddRecentEvent` | `void` | `public` | None |
| 147 | `RoomProfile` | `AddDominantSignal` | `void` | `public` | None |
| 155 | `RoomProfile` | `AddConflictingSignal` | `void` | `public` | None |
| 169 | `RoomProfileBuilder` | `RoomProfileBuilder` | `ctor` | `public` | None |
| 179 | `RoomProfileBuilder` | `Build` | `RoomProfile` | `public` | None |
| 197 | `RoomProfileBuilder` | `CollectFixtures` | `IEnumerable<BuildableObject>` | `private static` | None |
| 225 | `RoomProfileBuilder` | `AddFixtureContribution` | `void` | `private static` | None |
| 266 | `RoomProfileBuilder` | `AddRoleContribution` | `void` | `private static` | None |
| 324 | `RoomProfileBuilder` | `AddDefenseContribution` | `void` | `private static` | None |
| 337 | `RoomProfileBuilder` | `AddRecordContribution` | `void` | `private static` | None |
| 360 | `RoomProfileBuilder` | `CalculateDerivedMetrics` | `void` | `private static` | None |

### `Assets/Scripts/FacilityEvolution/UI/FacilityEvolutionPanel.cs`

- Functions detected: 17
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 22 | `FacilityEvolutionPanel` | `Construct` | `void` | `public` | None |
| 29 | `FacilityEvolutionPanel` | `Bind` | `void` | `public` | None |
| 35 | `FacilityEvolutionPanel` | `BindGeneratedView` | `void` | `internal` | None |
| 42 | `FacilityEvolutionPanel` | `SelectFacility` | `void` | `public` | None |
| 48 | `FacilityEvolutionPanel` | `TryEvolve` | `bool` | `public` | None |
| 75 | `FacilityEvolutionPanel` | `TryEvolveFirstApproved` | `bool` | `public` | None |
| 90 | `FacilityEvolutionPanel` | `Refresh` | `void` | `public` | None |
| 180 | `FacilityEvolutionPanel` | `OnTriggerEvent` | `void` | `public` | None |
| 190 | `FacilityEvolutionPanel` | `OnTriggerEvent` | `void` | `public` | None |
| 195 | `FacilityEvolutionPanel` | `OnTriggerEvent` | `void` | `public` | None |
| 205 | `FacilityEvolutionPanel` | `ApplyText` | `void` | `private` | None |
| 213 | `FacilityEvolutionPanel` | `FormatList` | `string` | `private static` | None |
| 222 | `FacilityEvolutionPanel` | `FormatChecks` | `IEnumerable<string>` | `private static` | None |
| 242 | `FacilityEvolutionPanel` | `FormatPressures` | `string` | `private static` | None |
| 258 | `FacilityEvolutionPanel` | `ResolveRuntime` | `FacilityEvolutionRuntime` | `private` | None |
| 267 | `FacilityEvolutionPanel` | `OnEnable` | `void` | `private` | None |
| 274 | `FacilityEvolutionPanel` | `OnDisable` | `void` | `private` | None |

### `Assets/Scripts/FacilityShop/FacilityBlueprintSO.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/FacilityShop/FacilityShopSystem.cs`

- Functions detected: 43
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 29 | `FacilityShopOfferSnapshot` | `ToSummaryText` | `string` | `public` | None |
| 42 | `FacilityShopOffer` | `FacilityShopOffer` | `ctor` | `public` | None |
| 56 | `FacilityShopOffer` | `FacilityShopOffer` | `ctor` | `public` | None |
| 82 | `FacilityShopOffer` | `ToSnapshot` | `FacilityShopOfferSnapshot` | `public` | None |
| 107 | `FacilityShopPurchaseResult` | `FacilityShopPurchaseResult` | `ctor` | `public` | None |
| 130 | `FacilityShopRefreshedEvent` | `FacilityShopRefreshedEvent` | `ctor` | `public` | None |
| 142 | `FacilityShopRefreshedEvent` | `Trigger` | `void` | `public static` | None |
| 158 | `FacilityShopPurchasedEvent` | `FacilityShopPurchasedEvent` | `ctor` | `public` | None |
| 165 | `FacilityShopPurchasedEvent` | `Trigger` | `void` | `public static` | None |
| 180 | `FacilityShopUnlockState` | `UnlockBasicPurchase` | `bool` | `public` | None |
| 190 | `FacilityShopUnlockState` | `UnlockBasicPurchaseById` | `bool` | `public` | None |
| 200 | `FacilityShopUnlockState` | `IsBasicPurchaseUnlocked` | `bool` | `public` | None |
| 205 | `FacilityShopUnlockState` | `MarkBlueprintAcquired` | `bool` | `public` | None |
| 215 | `FacilityShopUnlockState` | `IsBlueprintAcquired` | `bool` | `public` | None |
| 227 | `FacilityShopService` | `CreateDailyOffers` | `IReadOnlyList<FacilityShopOffer>` | `public static` | None |
| 251 | `FacilityShopService` | `CreateDailyOffers` | `IReadOnlyList<FacilityShopOffer>` | `public static` | None |
| 309 | `FacilityShopService` | `CreateBasicPurchaseOffers` | `IReadOnlyList<FacilityShopOffer>` | `public static` | None |
| 337 | `FacilityShopService` | `CreateBasicPurchaseOffers` | `IReadOnlyList<FacilityShopOffer>` | `public static` | None |
| 370 | `FacilityShopService` | `TryPurchaseOffer` | `bool` | `public static` | None |
| 404 | `FacilityShopService` | `CanEnterBasicPurchase` | `bool` | `public static` | None |
| 409 | `FacilityShopService` | `GetBuildingStar` | `int` | `public static` | None |
| 430 | `FacilityShopService` | `GetBuildingName` | `string` | `public static` | None |
| 440 | `FacilityShopService` | `FindBuildingById` | `BuildingSO` | `public static` | None |
| 455 | `FacilityShopService` | `CreateBuildingOffer` | `FacilityShopOffer` | `private static` | None |
| 471 | `FacilityShopService` | `CreateBlueprintOffer` | `FacilityShopOffer` | `private static` | None |
| 488 | `FacilityShopService` | `IsDailyShopBuildingCandidate` | `bool` | `private static` | None |
| 496 | `FacilityShopService` | `ResolveBuildingRarity` | `FacilityShopRarity` | `private static` | None |
| 507 | `FacilityShopService` | `CalculateBuildingCost` | `int` | `private static` | None |
| 535 | `FacilityShopService` | `GetBuildingCostMultiplier` | `float` | `private static` | None |
| 540 | `FacilityShopService` | `GetBlueprintCostMultiplier` | `float` | `private static` | None |
| 547 | `FacilityShopService` | `ApplyPurchase` | `string` | `private static` | None |
| 588 | `DailyFacilityShopRuntime` | `ConstructDailyFacilityShopRuntime` | `void` | `public` | None |
| 602 | `DailyFacilityShopRuntime` | `Start` | `void` | `private` | None |
| 610 | `DailyFacilityShopRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 615 | `DailyFacilityShopRuntime` | `Refresh` | `void` | `public` | None |
| 637 | `DailyFacilityShopRuntime` | `TryPurchaseDailyOffer` | `bool` | `public` | None |
| 649 | `DailyFacilityShopRuntime` | `TryPurchaseBasicOffer` | `bool` | `public` | None |
| 662 | `DailyFacilityShopRuntime` | `OnEnable` | `void` | `private` | None |
| 667 | `DailyFacilityShopRuntime` | `OnDisable` | `void` | `private` | None |
| 672 | `DailyFacilityShopRuntime` | `FormatOfferList` | `string` | `private static` | None |
| 709 | `DailyFacilityShopRuntime` | `ResolveFacilityShopCatalog` | `IFacilityShopCatalog` | `private` | None |
| 715 | `DailyFacilityShopRuntime` | `ResolveRunVariableReader` | `IRunVariableRuntimeReader` | `private` | None |
| 721 | `DailyFacilityShopRuntime` | `ResolveMetaProgressionReader` | `IMetaProgressionRuntimeReader` | `private` | None |

### `Assets/Scripts/GameData.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/GameManager.cs`

- Functions detected: 7
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 25 | `GameManager` | `Awake` | `void` | `private` | None |
| 30 | `GameManager` | `Start` | `void` | `-` | Coroutine/timing orchestration, Lifecycle contains dependency/workflow logic |
| 38 | `GameManager` | `ConvertSecondsToGameTime` | `void` | `public` | None |
| 45 | `GameManager` | `ChangeGameSpeed` | `void` | `public` | None |
| 53 | `GameManager` | `TogglePause` | `void` | `public` | None |
| 65 | `GameManager` | `Update` | `void` | `-` | None |
| 90 | `GameManager` | `Timer` | `IEnumerator` | `public` | None |

### `Assets/Scripts/Grid/Building/GridBuildingObjectFactory.cs`

- Functions detected: 2
- Judgment: object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `IGridBuildingObjectFactory` | `Create` | `BuildableObject` | `-` | None |
| 11 | `GridBuildingObjectFactory` | `Create` | `BuildableObject` | `public` | Runtime object creation |

### `Assets/Scripts/Grid/Building/GridBuildingRuntime.cs`

- Functions detected: 47
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 20 | `GridBuildingPlacementService` | `GridBuildingPlacementService` | `ctor` | `public` | None |
| 25 | `GridBuildingPlacementService` | `GridBuildingPlacementService` | `ctor` | `public` | None |
| 35 | `GridBuildingPlacementService` | `GridBuildingPlacementService` | `ctor` | `public` | None |
| 50 | `GridBuildingPlacementService` | `SetGrid` | `void` | `public` | None |
| 55 | `GridBuildingPlacementService` | `TryPlaceBuilding` | `bool` | `public` | None |
| 73 | `GridBuildingPlacementService` | `CanPlaceBuilding` | `bool` | `public` | None |
| 78 | `GridBuildingPlacementService` | `CanPlaceBuilding` | `bool` | `public` | None |
| 83 | `GridBuildingPlacementService` | `TryDestroyBuilding` | `bool` | `public` | None |
| 117 | `GridBuildingPlacementService` | `PlaceInitialBuildings` | `void` | `public` | None |
| 141 | `GridBuildingPlacementService` | `EnsureHallwayUnderBuildingFootprint` | `void` | `private` | None |
| 157 | `GridBuildingPlacementService` | `IsDuplicateInitialHallway` | `bool` | `private` | None |
| 173 | `GridBuildingPlacementService` | `RequiresHallwayUnderFootprint` | `bool` | `private static` | None |
| 185 | `GridBuildingPlacementService` | `PlaceBuildingWithoutValidation` | `bool` | `private` | None |
| 221 | `GridBuildingPlacementService` | `CanRegisterBuilding` | `bool` | `private` | None |
| 244 | `IGridBuildingVisual` | `DrawBuilding` | `void` | `-` | None |
| 245 | `IGridBuildingVisual` | `DeleteBuilding` | `void` | `-` | None |
| 250 | `IGridBuildingFactory` | `Create` | `BuildableObject` | `-` | None |
| 251 | `IGridBuildingFactory` | `DeleteVisual` | `void` | `-` | None |
| 259 | `GridBuildingFactory` | `GridBuildingFactory` | `ctor` | `public` | None |
| 264 | `GridBuildingFactory` | `GridBuildingFactory` | `ctor` | `public` | None |
| 269 | `GridBuildingFactory` | `GridBuildingFactory` | `ctor` | `public` | None |
| 274 | `GridBuildingFactory` | `GridBuildingFactory` | `ctor` | `public` | None |
| 284 | `GridBuildingFactory` | `Create` | `BuildableObject` | `public` | None |
| 295 | `GridBuildingFactory` | `DeleteVisual` | `void` | `public` | None |
| 302 | `GridBuildingFactory` | `ValidateBuildingVisual` | `void` | `private static` | None |
| 312 | `GridBuildingFactory` | `HasTileVisual` | `bool` | `private static` | None |
| 324 | `BuildingPlacementValidator` | `BuildingPlacementValidator` | `ctor` | `public` | None |
| 329 | `BuildingPlacementValidator` | `BuildingPlacementValidator` | `ctor` | `public` | None |
| 334 | `BuildingPlacementValidator` | `BuildingPlacementValidator` | `ctor` | `public` | None |
| 342 | `BuildingPlacementValidator` | `CanBuild` | `bool` | `public` | None |
| 388 | `BuildingPlacementValidator` | `CanDestroy` | `bool` | `public` | None |
| 413 | `BuildingPlacementValidator` | `ApplyBuildSuccess` | `void` | `public` | None |
| 424 | `BuildingPlacementValidator` | `CreateConditionContext` | `BuildingConditionContext` | `private` | None |
| 435 | `GridBuildingExtensions` | `CanBuild` | `bool` | `public static` | None |
| 439 | `GridBuildingExtensions` | `HasBuildingInLayer` | `bool` | `public static` | None |
| 444 | `GridBuildingExtensions` | `HasBuilding` | `bool` | `public static` | None |
| 449 | `GridBuildingExtensions` | `GetBuildingInlayer` | `BuildableObject` | `public static` | None |
| 454 | `GridBuildingExtensions` | `GetBuilding` | `BuildableObject` | `public static` | None |
| 459 | `GridBuildingExtensions` | `GetAllBuilding` | `List<BuildableObject>` | `public static` | None |
| 468 | `GridBuildingExtensions` | `GetAllVisitableBuilding` | `List<BuildableObject>` | `public static` | None |
| 477 | `GridBuildingExtensions` | `GetAllReachableBuilding` | `List<BuildableObject>` | `public static` | None |
| 486 | `GridBuildingExtensions` | `GetAllVisitableBuilding` | `List<BuildableObject>` | `public static` | None |
| 495 | `GridBuildingExtensions` | `GetAllReachableBuilding` | `List<BuildableObject>` | `public static` | None |
| 504 | `GridBuildingExtensions` | `IsConneted` | `bool` | `public static` | None |
| 509 | `GridBuildingExtensions` | `IsConnected` | `bool` | `public static` | None |
| 520 | `GridBuildingExtensions` | `FindAllBuilding` | `List<BuildableObject>` | `public static` | None |
| 529 | `GridBuildingExtensions` | `CountBuilding` | `int` | `public static` | None |

### `Assets/Scripts/Grid/Core/Grid.cs`

- Functions detected: 60
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 40 | `GridTraversalLink` | `GridTraversalLink` | `ctor` | `public` | None |
| 58 | `GridMoveStep` | `GridMoveStep` | `ctor` | `public` | None |
| 72 | `GridMoveStep` | `WithDestination` | `GridMoveStep` | `public` | None |
| 93 | `GridPathSearchResult` | `GridPathSearchResult` | `ctor` | `public` | None |
| 110 | `GridPathSearchResult` | `GetAllVisitableOccupants` | `List<IGridOccupant>` | `public` | None |
| 115 | `GridPathSearchResult` | `ContainsVisitableOccupant` | `bool` | `public` | None |
| 120 | `GridPathSearchResult` | `GetMoveDistanceTo` | `int` | `public` | None |
| 135 | `GridPathSearchResult` | `GetAllReachableOccupants` | `List<IGridOccupant>` | `public` | None |
| 158 | `GridPathSearchResult` | `GetReachablePositions` | `List<Vector2Int>` | `public` | None |
| 163 | `GridPathSearchResult` | `TryGetMovePathToRandomReachablePosition` | `bool` | `public` | None |
| 216 | `GridPathSearchResult` | `GetOccupantPathTo` | `Queue<IGridOccupant>` | `public` | None |
| 232 | `GridPathSearchResult` | `GetOccupantPath` | `Queue<IGridOccupant>` | `public` | None |
| 247 | `GridPathSearchResult` | `GetMovePathTo` | `Queue<GridMoveStep>` | `public` | None |
| 263 | `GridPathSearchResult` | `GetMovePath` | `Queue<GridMoveStep>` | `public` | None |
| 278 | `GridPathSearchResult` | `BuildOccupantPath` | `Queue<IGridOccupant>` | `private` | None |
| 295 | `GridPathSearchResult` | `BuildMovePath` | `Queue<GridMoveStep>` | `private` | None |
| 318 | `GridPathSearchResult` | `TryGetVisitableOccupantPosition` | `bool` | `private` | None |
| 324 | `GridPathSearchResult` | `EnsureVisitableOccupantPositionCache` | `void` | `private` | None |
| 354 | `GridPathSearchResult` | `GetMoveDistance` | `int` | `private` | None |
| 384 | `GridPathSearchResult` | `IsDistanceInRange` | `bool` | `private static` | None |
| 410 | `Grid` | `Grid` | `ctor` | `public` | None |
| 415 | `Grid` | `Grid` | `ctor` | `public` | None |
| 435 | `Grid` | `SetUnityCoordinates` | `void` | `public` | None |
| 441 | `Grid` | `GetXY` | `Vector2Int` | `public` | None |
| 448 | `Grid` | `GetWorldPos` | `Vector3` | `public` | None |
| 453 | `Grid` | `GetWorldPos` | `Vector3` | `public` | None |
| 461 | `Grid` | `IsValidGridPos` | `bool` | `public` | None |
| 469 | `Grid` | `GetGridCell` | `GridCell` | `public` | None |
| 476 | `Grid` | `GetCells` | `IEnumerable<GridCell>` | `public` | None |
| 484 | `Grid` | `TryExpandGrid` | `Grid` | `public` | None |
| 504 | `Grid` | `RegisterOccupant` | `bool` | `public` | None |
| 532 | `Grid` | `RemoveOccupant` | `bool` | `public` | None |
| 565 | `Grid` | `SearchPath` | `GridPathSearchResult` | `public` | None |
| 570 | `Grid` | `SearchPath` | `GridPathSearchResult` | `private` | None |
| 655 | `Grid` | `GetOccupantPath` | `Queue<IGridOccupant>` | `public` | None |
| 660 | `Grid` | `GetMovePath` | `Queue<GridMoveStep>` | `public` | None |
| 665 | `Grid` | `TryGetMovePathToRandomReachablePosition` | `bool` | `public` | None |
| 681 | `Grid` | `GetAllVisitableOccupants` | `List<IGridOccupant>` | `public` | None |
| 686 | `Grid` | `GetAllReachableOccupants` | `List<IGridOccupant>` | `public` | None |
| 691 | `Grid` | `SmoothOccupantPath` | `Queue<IGridOccupant>` | `public` | None |
| 709 | `Grid` | `IsWalkable` | `bool` | `public` | None |
| 742 | `Grid` | `IsWalkableFacilityCell` | `bool` | `private static` | None |
| 748 | `Grid` | `TryFindNearestWalkablePosition` | `bool` | `public` | None |
| 775 | `Grid` | `IsConnectedWithAny` | `bool` | `public` | None |
| 782 | `Grid` | `FindAllOccupants` | `List<IGridOccupant>` | `public` | None |
| 805 | `Grid` | `NextSearchMark` | `int` | `private` | None |
| 817 | `Grid` | `RegisterTraversalLinks` | `void` | `private` | None |
| 837 | `Grid` | `CanConnectMovementCells` | `bool` | `private` | None |
| 854 | `Grid` | `ResolveMoveType` | `GridMoveType` | `private` | None |
| 864 | `Grid` | `AddMoveStep` | `void` | `private` | None |
| 890 | `GridSearchScratch` | `RentPositionQueue` | `Queue<Vector2Int>` | `public static` | None |
| 895 | `GridSearchScratch` | `RentMoveStepList` | `List<GridMoveStep>` | `public static` | None |
| 900 | `GridSearchScratch` | `RentPositionList` | `List<Vector2Int>` | `public static` | None |
| 905 | `GridSearchScratch` | `RentOccupantList` | `List<IGridOccupant>` | `public static` | None |
| 910 | `GridSearchScratch` | `RentOccupantSet` | `HashSet<IGridOccupant>` | `public static` | None |
| 915 | `GridSearchScratch` | `Return` | `void` | `public static` | None |
| 923 | `GridSearchScratch` | `Return` | `void` | `public static` | None |
| 931 | `GridSearchScratch` | `ReturnPositionList` | `void` | `public static` | None |
| 939 | `GridSearchScratch` | `ReturnOccupantList` | `void` | `public static` | None |
| 947 | `GridSearchScratch` | `Return` | `void` | `public static` | None |

### `Assets/Scripts/Grid/Core/GridCell.cs`

- Functions detected: 14
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 12 | `GridCell` | `GridCell` | `ctor` | `public` | None |
| 20 | `GridCell` | `GetOccupant` | `IGridOccupant` | `public` | None |
| 25 | `GridCell` | `GetTopOccupant` | `IGridOccupant` | `public` | None |
| 44 | `GridCell` | `ConnectFloor` | `void` | `public` | None |
| 61 | `GridCell` | `SetTraversalLinks` | `void` | `public` | None |
| 77 | `GridCell` | `RemoveOccupantByLayer` | `void` | `public` | None |
| 82 | `GridCell` | `GetAllOccupants` | `List<IGridOccupant>` | `public` | None |
| 88 | `GridCell` | `FillAllOccupants` | `void` | `public` | None |
| 100 | `GridCell` | `ContainsOccupant` | `bool` | `public` | None |
| 117 | `GridCell` | `CanOccupy` | `bool` | `public` | None |
| 121 | `GridCell` | `HasOccupantInLayer` | `bool` | `public` | None |
| 125 | `GridCell` | `HasOccupant` | `bool` | `public` | None |
| 129 | `GridCell` | `TrySetOccupant` | `bool` | `public` | None |
| 136 | `GridCell` | `SetOccupant` | `void` | `public` | None |

### `Assets/Scripts/Grid/DungeonStory/Building/DungeonStoryGridBuildingController.cs`

- Functions detected: 40
- Judgment: manual resolver use should move to a composition root or factory.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 30 | `DungeonStoryGridBuildingController` | `Construct` | `void` | `public` | None |
| 56 | `DungeonStoryGridBuildingController` | `Awake` | `void` | `private` | None |
| 66 | `DungeonStoryGridBuildingController` | `Start` | `void` | `private` | None |
| 76 | `DungeonStoryGridBuildingController` | `EnsureInitialized` | `void` | `private` | None |
| 107 | `DungeonStoryGridBuildingController` | `Update` | `void` | `private` | None |
| 118 | `DungeonStoryGridBuildingController` | `TriggerPlaceBuilding` | `void` | `public` | None |
| 145 | `DungeonStoryGridBuildingController` | `TriggerDestroyBuildableObject` | `void` | `public` | None |
| 164 | `DungeonStoryGridBuildingController` | `OnEnable` | `void` | `private` | None |
| 174 | `DungeonStoryGridBuildingController` | `OnDisable` | `void` | `private` | None |
| 184 | `DungeonStoryGridBuildingController` | `GetBuildingByMousePos` | `BuildableObject` | `public` | None |
| 195 | `DungeonStoryGridBuildingController` | `IsBuildable` | `bool` | `public` | None |
| 203 | `DungeonStoryGridBuildingController` | `IsBuildableAt` | `bool` | `public` | None |
| 211 | `DungeonStoryGridBuildingController` | `GetMouseWorldPosSnapped` | `Vector3` | `public` | None |
| 219 | `DungeonStoryGridBuildingController` | `SelectBuildingById` | `void` | `public` | None |
| 235 | `DungeonStoryGridBuildingController` | `ClearBuildingSO` | `void` | `public` | None |
| 240 | `DungeonStoryGridBuildingController` | `SetGridModeBuild` | `void` | `public` | None |
| 248 | `DungeonStoryGridBuildingController` | `SetGridModeNone` | `void` | `public` | None |
| 257 | `DungeonStoryGridBuildingController` | `SetDestroyMode` | `void` | `public` | None |
| 266 | `DungeonStoryGridBuildingController` | `PlaceBuilding` | `void` | `private` | None |
| 294 | `DungeonStoryGridBuildingController` | `DrawGridTextureWalls` | `void` | `private` | None |
| 299 | `DungeonStoryGridBuildingController` | `HasAnyGridOccupants` | `bool` | `private static` | None |
| 314 | `DungeonStoryGridBuildingController` | `OnGridExpand` | `void` | `private` | None |
| 322 | `DungeonStoryGridBuildingController` | `OnDestroy` | `void` | `private` | None |
| 332 | `DungeonStoryGridBuildingController` | `EnsureInputActions` | `void` | `private` | None |
| 348 | `DungeonStoryGridBuildingController` | `OnBuildingPlaceInput` | `void` | `private` | None |
| 354 | `DungeonStoryGridBuildingController` | `OnExpandGridInput` | `void` | `private` | None |
| 362 | `DungeonStoryGridBuildingController` | `FindBuildingDataById` | `BuildingSO` | `private` | None |
| 369 | `DungeonStoryGridBuildingController` | `CreateBuildingConditionContext` | `BuildingConditionContext` | `private` | None |
| 375 | `DungeonStoryGridBuildingController` | `ConfigurePlacedBuilding` | `void` | `private` | Manual resolver/injection |
| 384 | `DungeonStoryGridBuildingController` | `OnPlacedBuildingClicked` | `void` | `private` | None |
| 392 | `DungeonStoryGridBuildingController` | `ResolveGridSystem` | `GridSystemManager` | `private` | None |
| 403 | `DungeonStoryGridBuildingController` | `ResolveGridSystemProvider` | `IGridSystemProvider` | `private` | None |
| 409 | `DungeonStoryGridBuildingController` | `ResolveDataCatalog` | `IDataCatalog` | `private` | None |
| 415 | `DungeonStoryGridBuildingController` | `ResolveMouseWorldPosition` | `Vector3` | `private` | None |
| 422 | `DungeonStoryGridBuildingController` | `ResolveGridTexture` | `GridTexture` | `private` | None |
| 429 | `DungeonStoryGridBuildingController` | `ResolveObjectResolver` | `IObjectResolver` | `private` | None |
| 435 | `DungeonStoryGridBuildingController` | `ResolveGameDataProvider` | `IGameDataProvider` | `private` | None |
| 441 | `DungeonStoryGridBuildingController` | `ResolveGridBuildingObjectFactory` | `IGridBuildingObjectFactory` | `private` | None |
| 447 | `DungeonStoryGridBuildingController` | `ResolveBuildingDataById` | `BuildingSO` | `private` | None |
| 457 | `DungeonStoryGridBuildingController` | `TryResolveBuildingDataById` | `bool` | `private` | None |

### `Assets/Scripts/Grid/DungeonStory/UI/DungeonStoryGridGhostPresenter.cs`

- Functions detected: 16
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 26 | `DungeonStoryGridGhostPresenter` | `Construct` | `void` | `public` | None |
| 43 | `DungeonStoryGridGhostPresenter` | `Awake` | `void` | `protected override` | None |
| 48 | `DungeonStoryGridGhostPresenter` | `CanPlaceAt` | `bool` | `protected override` | None |
| 53 | `DungeonStoryGridGhostPresenter` | `OnGridModeChanged` | `void` | `private` | None |
| 64 | `DungeonStoryGridGhostPresenter` | `OnSelectedChanged` | `void` | `public` | None |
| 69 | `DungeonStoryGridGhostPresenter` | `OnEnable` | `void` | `private` | None |
| 74 | `DungeonStoryGridGhostPresenter` | `Start` | `void` | `private` | None |
| 79 | `DungeonStoryGridGhostPresenter` | `OnDisable` | `void` | `private` | None |
| 84 | `DungeonStoryGridGhostPresenter` | `SubscribeToRuntimeEventsIfInjected` | `void` | `private` | None |
| 92 | `DungeonStoryGridGhostPresenter` | `SubscribeToRuntimeEvents` | `void` | `private` | None |
| 109 | `DungeonStoryGridGhostPresenter` | `UnsubscribeFromRuntimeEvents` | `void` | `private` | None |
| 123 | `DungeonStoryGridGhostPresenter` | `RequireGridSystemProvider` | `IGridSystemProvider` | `private` | None |
| 129 | `DungeonStoryGridGhostPresenter` | `RequireGridSystem` | `GridSystemManager` | `private` | None |
| 134 | `DungeonStoryGridGhostPresenter` | `RequireBuildingControllerProvider` | `IDungeonGridBuildingControllerProvider` | `private` | None |
| 140 | `DungeonStoryGridGhostPresenter` | `RequireBuildingController` | `DungeonStoryGridBuildingController` | `private` | None |
| 145 | `DungeonStoryGridGhostPresenter` | `RequireWorldPointerPositionProvider` | `IWorldPointerPositionProvider` | `private` | None |

### `Assets/Scripts/Grid/DungeonStory/UI/GridConstructButtonFactory.cs`

- Functions detected: 8
- Judgment: manual resolver use is acceptable only in composition roots/factories; UI type usage fits this UI/factory context; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 10 | `IGridConstructButtonFactory` | `CreateCategoryPanel` | `UITab` | `-` | None |
| 11 | `IGridConstructButtonFactory` | `CreateCategoryButton` | `Button` | `-` | UI type dependency |
| 12 | `IGridConstructButtonFactory` | `CreateBuildingSelectButton` | `UIBuildingSelectButton` | `-` | None |
| 19 | `GridConstructButtonFactory` | `GridConstructButtonFactory` | `ctor` | `public` | None |
| 25 | `GridConstructButtonFactory` | `GridConstructButtonFactory` | `ctor` | `public` | None |
| 36 | `GridConstructButtonFactory` | `CreateCategoryPanel` | `UITab` | `public` | Manual resolver/injection, Runtime object creation |
| 55 | `GridConstructButtonFactory` | `CreateCategoryButton` | `Button` | `public` | Runtime object creation, UI type dependency |
| 87 | `GridConstructButtonFactory` | `CreateBuildingSelectButton` | `UIBuildingSelectButton` | `public` | Manual resolver/injection, Runtime object creation |

### `Assets/Scripts/Grid/DungeonStory/UI/GridConstructTab.cs`

- Functions detected: 19
- Judgment: UI type usage fits this UI/factory context.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 23 | `GridConstructTab` | `ConstructGridConstructTab` | `void` | `public` | None |
| 40 | `GridConstructTab` | `Start` | `void` | `private` | None |
| 45 | `GridConstructTab` | `OnClose` | `void` | `public override` | None |
| 51 | `GridConstructTab` | `OnOpen` | `void` | `public override` | None |
| 58 | `GridConstructTab` | `MakeSelectButton` | `void` | `private` | None |
| 88 | `GridConstructTab` | `AddCategoryPanel` | `void` | `private` | None |
| 103 | `GridConstructTab` | `HasCategoryPanel` | `bool` | `private` | None |
| 109 | `GridConstructTab` | `GetCategoryPanel` | `UITab` | `private` | None |
| 121 | `GridConstructTab` | `ResolveCategoryButtonRoot` | `Transform` | `private` | None |
| 126 | `GridConstructTab` | `ToggleSelectButton` | `void` | `public` | None |
| 146 | `GridConstructTab` | `RefreshCategoryLabels` | `void` | `public` | UI type dependency |
| 160 | `GridConstructTab` | `GetCategoryDisplayName` | `string` | `private static` | None |
| 175 | `GridConstructTab` | `TryGetCategoryDisplayName` | `bool` | `private static` | None |
| 193 | `GridConstructTab` | `RequireDataCatalog` | `IDataCatalog` | `private` | None |
| 199 | `GridConstructTab` | `RequirePopupService` | `IUiPopupService` | `private` | None |
| 205 | `GridConstructTab` | `RequireBuildingControllerProvider` | `IDungeonGridBuildingControllerProvider` | `private` | None |
| 211 | `GridConstructTab` | `RequireBuildingController` | `DungeonStoryGridBuildingController` | `private` | None |
| 216 | `GridConstructTab` | `RequireTmpKoreanFontService` | `ITmpKoreanFontService` | `private` | None |
| 223 | `GridConstructTab` | `RequireButtonFactory` | `IGridConstructButtonFactory` | `private` | None |

### `Assets/Scripts/Grid/DungeonStory/UI/GridUIManager.cs`

- Functions detected: 15
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 17 | `GridUIManager` | `Construct` | `void` | `public` | None |
| 31 | `GridUIManager` | `Start` | `void` | `private` | None |
| 38 | `GridUIManager` | `Update` | `void` | `private` | None |
| 51 | `GridUIManager` | `ToggleConstructTab` | `void` | `public` | None |
| 63 | `GridUIManager` | `ShowConstructTab` | `void` | `public` | None |
| 68 | `GridUIManager` | `CloseConstructTab` | `void` | `public` | None |
| 74 | `GridUIManager` | `HideGrid` | `void` | `public` | None |
| 80 | `GridUIManager` | `ShowGrid` | `void` | `public` | None |
| 86 | `GridUIManager` | `DrawGrid` | `void` | `public` | None |
| 91 | `GridUIManager` | `OnEnable` | `void` | `private` | None |
| 95 | `GridUIManager` | `OnDisable` | `void` | `private` | None |
| 99 | `GridUIManager` | `ResolveRuntimeDependencies` | `void` | `private` | None |
| 109 | `GridUIManager` | `RequireGridSystemProvider` | `IGridSystemProvider` | `private` | None |
| 115 | `GridUIManager` | `RequireBuildingControllerProvider` | `IDungeonGridBuildingControllerProvider` | `private` | None |
| 121 | `GridUIManager` | `RequireInputReader` | `IPlayerInputReader` | `private` | None |

### `Assets/Scripts/Grid/Placement/GridPlacementValidator.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `GridPlacementValidator` | `AreInsideGrid` | `bool` | `public` | None |
| 14 | `GridPlacementValidator` | `AreInsideHorizontalBounds` | `bool` | `public` | None |
| 20 | `GridPlacementValidator` | `CanOccupy` | `bool` | `public` | None |
| 26 | `GridPlacementValidator` | `HasSupportBelow` | `bool` | `public` | None |
| 45 | `GridPlacementValidator` | `CanRemoveOccupantWithoutUnsupportedAbove` | `bool` | `public` | None |

### `Assets/Scripts/Grid/Rendering/GridTexture.cs`

- Functions detected: 32
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 26 | `GridTexture` | `Awake` | `void` | `private` | None |
| 31 | `GridTexture` | `OnValidate` | `void` | `private` | None |
| 36 | `GridTexture` | `DrawBuilding` | `void` | `public` | None |
| 48 | `GridTexture` | `DrawBuilding` | `void` | `public` | None |
| 67 | `GridTexture` | `DeleteBuilding` | `void` | `public` | None |
| 79 | `GridTexture` | `DeleteBuilding` | `void` | `public` | None |
| 98 | `GridTexture` | `DrawSpriteBuilding` | `void` | `private` | None |
| 106 | `GridTexture` | `DeleteSpriteBuilding` | `void` | `private` | None |
| 114 | `GridTexture` | `GetSpriteTile` | `Tile` | `private` | None |
| 144 | `GridTexture` | `TryGetTilemap` | `bool` | `private` | None |
| 153 | `GridTexture` | `HasTileVisual` | `bool` | `private static` | None |
| 158 | `GridTexture` | `GetSpriteTilemapLayer` | `TilemapLayer` | `private static` | None |
| 166 | `GridTexture` | `GetTilePosition` | `Vector3Int` | `private static` | None |
| 171 | `GridTexture` | `SetWallCell` | `void` | `private` | None |
| 191 | `SpriteTileKey` | `SpriteTileKey` | `ctor` | `public` | None |
| 201 | `SpriteTileKey` | `Equals` | `bool` | `public` | None |
| 211 | `SpriteTileKey` | `Equals` | `bool` | `public override` | None |
| 216 | `SpriteTileKey` | `GetHashCode` | `int` | `public override` | None |
| 231 | `GridTexture` | `DrawWall` | `void` | `public` | None |
| 300 | `GridTexture` | `SynchronizeHallwayVisuals` | `void` | `private` | None |
| 325 | `GridTexture` | `ClearHallwayTile` | `void` | `private` | None |
| 332 | `GridTexture` | `CanShareHallwayVisual` | `bool` | `private static` | None |
| 338 | `GridTexture` | `ClearTile` | `void` | `private` | None |
| 346 | `GridTexture` | `ApplyHallwaySorting` | `void` | `private` | None |
| 353 | `GridTexture` | `ConfigureHallwayTilemap` | `void` | `private` | None |
| 385 | `GridBuildingTileTransformCalculator` | `Calculate` | `Matrix4x4` | `public static` | None |
| 390 | `GridBuildingTileTransformCalculator` | `Calculate` | `Matrix4x4` | `public static` | None |
| 395 | `GridBuildingTileTransformCalculator` | `Calculate` | `Matrix4x4` | `public static` | None |
| 436 | `GridBuildingTileTransformCalculator` | `GetVisualFootprintSize` | `Vector2` | `public static` | None |
| 444 | `GridBuildingTileTransformCalculator` | `GetVerticalVisualInset` | `float` | `private static` | None |
| 459 | `GridWallTileCalculator` | `GridWallTileCalculator` | `ctor` | `public` | None |
| 464 | `GridWallTileCalculator` | `GetWallTilePositions` | `HashSet<Vector2Int>` | `public` | None |

### `Assets/Scripts/Grid/System/GridSystemManager.cs`

- Functions detected: 16
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 36 | `GridSystemManager` | `Awake` | `void` | `protected virtual` | None |
| 41 | `GridSystemManager` | `OnEnable` | `void` | `protected virtual` | None |
| 46 | `GridSystemManager` | `EnsureGridInitialized` | `void` | `public` | None |
| 53 | `GridSystemManager` | `Start` | `void` | `protected virtual` | None |
| 59 | `GridSystemManager` | `GridExpand` | `void` | `public` | None |
| 68 | `GridSystemManager` | `SetGridMode` | `void` | `public` | None |
| 81 | `GridSystemManager` | `SetGridModeBuild` | `void` | `public` | None |
| 88 | `GridSystemManager` | `SetGridModeNone` | `void` | `public` | None |
| 93 | `GridSystemManager` | `ToggleBuildMode` | `void` | `public` | None |
| 98 | `GridSystemManager` | `TryBeginDragSelection` | `bool` | `public` | None |
| 108 | `GridSystemManager` | `UpdateDragSelection` | `void` | `public` | None |
| 117 | `GridSystemManager` | `CompleteDragSelection` | `List<Vector2Int>` | `public` | None |
| 123 | `GridSystemManager` | `CancelDragSelection` | `void` | `public` | None |
| 131 | `GridSystemManager` | `GetWorldPosSnapped` | `Vector3` | `public` | None |
| 141 | `GridSystemManager` | `NotifyGridObjectChanged` | `void` | `public` | None |
| 146 | `GridSystemManager` | `RecalculateSelectedPositions` | `void` | `private` | None |

### `Assets/Scripts/Grid/UI/GridGhostObject.cs`

- Functions detected: 18
- Judgment: object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 21 | `GridGhostObject` | `Awake` | `void` | `private` | None |
| 26 | `GridGhostObject` | `OnValidate` | `void` | `private` | None |
| 31 | `GridGhostObject` | `Initialize` | `void` | `public` | None |
| 57 | `GridGhostObject` | `Show` | `void` | `public` | None |
| 71 | `GridGhostObject` | `ShowRepeated` | `void` | `public` | None |
| 111 | `GridGhostObject` | `Hide` | `void` | `public` | None |
| 124 | `GridGhostObject` | `SetBuildable` | `void` | `public` | None |
| 133 | `GridGhostObject` | `SetWorldPosition` | `void` | `public` | None |
| 144 | `GridGhostObject` | `SetSize` | `void` | `public` | None |
| 151 | `GridGhostObject` | `ApplyFootprintSize` | `void` | `private` | None |
| 156 | `GridGhostObject` | `ApplyFootprintSize` | `Vector3` | `private` | None |
| 188 | `GridGhostObject` | `GetPreviewColor` | `Color` | `private` | None |
| 193 | `GridGhostObject` | `WithPreviewAlpha` | `Color` | `private` | None |
| 199 | `GridGhostObject` | `EnsureRepeatedRendererCount` | `void` | `private` | Runtime object creation |
| 220 | `GridGhostObject` | `ConfigurePreviewRenderers` | `void` | `private` | None |
| 228 | `GridGhostObject` | `ConfigurePreviewRenderer` | `void` | `private` | None |
| 243 | `GridGhostObject` | `HideRepeatedRenderers` | `void` | `private` | None |
| 255 | `GridGhostObject` | `EnsureInitialized` | `void` | `private` | None |

### `Assets/Scripts/Grid/UI/GridGhostObjectResolver.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IGridGhostObjectResolver` | `Resolve` | `GridGhostObject` | `-` | None |
| 11 | `GridGhostObjectResolver` | `Resolve` | `GridGhostObject` | `public` | None |

### `Assets/Scripts/Grid/UI/GridPlacementGhostPresenter.cs`

- Functions detected: 13
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 27 | `GridPlacementGhostPresenter` | `CanPlaceAt` | `bool` | `protected abstract` | None |
| 29 | `GridPlacementGhostPresenter` | `Awake` | `void` | `protected virtual` | None |
| 37 | `GridPlacementGhostPresenter` | `Update` | `void` | `protected virtual` | None |
| 42 | `GridPlacementGhostPresenter` | `RefreshGhostSelection` | `void` | `protected` | None |
| 56 | `GridPlacementGhostPresenter` | `HideGhost` | `void` | `protected` | None |
| 62 | `GridPlacementGhostPresenter` | `ConstructGridPlacementGhostPresenter` | `void` | `protected` | None |
| 70 | `GridPlacementGhostPresenter` | `UpdateGhost` | `void` | `private` | None |
| 91 | `GridPlacementGhostPresenter` | `UpdateSingleCellGhost` | `void` | `private` | None |
| 103 | `GridPlacementGhostPresenter` | `UpdateDraggedGhost` | `void` | `private` | None |
| 128 | `GridPlacementGhostPresenter` | `GetGhostFootprintSize` | `Vector2` | `private` | None |
| 135 | `GridPlacementGhostPresenter` | `EnsureGhostObject` | `void` | `private` | None |
| 153 | `GridPlacementGhostPresenter` | `TryInitializeLocalGhostObject` | `bool` | `private` | None |
| 165 | `GridPlacementGhostPresenter` | `InitializeGhostObject` | `void` | `private` | None |

### `Assets/Scripts/Grid/UI/UIGridTab.cs`

- Functions detected: 4
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `UIGridTab` | `Start` | `void` | `private` | None |
| 13 | `UIGridTab` | `ToggleTab` | `bool` | `public` | None |
| 31 | `UIGridTab` | `OpenTap` | `void` | `public` | None |
| 35 | `UIGridTab` | `CloseTab` | `void` | `public` | None |

### `Assets/Scripts/Infrastructure/BlueprintResearchRuntimeProvider.cs`

- Functions detected: 11
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `IBlueprintResearchRuntimeProvider` | `TryGetRuntime` | `bool` | `-` | None |
| 10 | `IBlueprintResearchWorkService` | `HasResearchWorkFor` | `bool` | `-` | None |
| 11 | `IBlueprintResearchWorkService` | `ApplyResearchWork` | `BlueprintResearchWorkResult` | `-` | None |
| 19 | `IBlueprintResearchStateService` | `GetState` | `BlueprintResearchState` | `-` | None |
| 25 | `BlueprintResearchRuntimeProvider` | `BlueprintResearchRuntimeProvider` | `ctor` | `public` | None |
| 31 | `BlueprintResearchRuntimeProvider` | `TryGetRuntime` | `bool` | `public` | Scene query coupling |
| 42 | `BlueprintResearchWorkService` | `BlueprintResearchWorkService` | `ctor` | `public` | None |
| 48 | `BlueprintResearchWorkService` | `HasResearchWorkFor` | `bool` | `public` | None |
| 56 | `BlueprintResearchWorkService` | `ApplyResearchWork` | `BlueprintResearchWorkResult` | `public` | None |
| 81 | `BlueprintResearchStateService` | `BlueprintResearchStateService` | `ctor` | `public` | None |
| 87 | `BlueprintResearchStateService` | `GetState` | `BlueprintResearchState` | `public` | None |

### `Assets/Scripts/Infrastructure/CharacterAiActionAssetCatalog.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `ICharacterAiActionAssetCatalog` | `GetRequiredAction` | `T` | `-` | None |
| 6 | `ICharacterAiActionAssetCatalog` | `GetRequiredFacilityRoleAction` | `AIFacilityRoleAction` | `-` | None |
| 12 | `ResourceCharacterAiActionAssetCatalog` | `ResourceCharacterAiActionAssetCatalog` | `ctor` | `public` | None |
| 18 | `ResourceCharacterAiActionAssetCatalog` | `GetRequiredAction` | `T` | `public` | None |
| 28 | `ResourceCharacterAiActionAssetCatalog` | `GetRequiredFacilityRoleAction` | `AIFacilityRoleAction` | `public` | None |

### `Assets/Scripts/Infrastructure/CharacterAiSchedulingService.cs`

- Functions detected: 17
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `ICharacterAiSchedulingService` | `Register` | `void` | `-` | None |
| 7 | `ICharacterAiSchedulingService` | `Unregister` | `void` | `-` | None |
| 8 | `ICharacterAiSchedulingService` | `RequestImmediateDecision` | `void` | `-` | None |
| 9 | `ICharacterAiSchedulingService` | `TryConsumePathSearchBudget` | `bool` | `-` | None |
| 10 | `ICharacterAiSchedulingService` | `ShouldShowCharacterFeedback` | `bool` | `-` | None |
| 11 | `ICharacterAiSchedulingService` | `GetMovementFrameStride` | `int` | `-` | None |
| 12 | `ICharacterAiSchedulingService` | `ResetPathSearchBudgetForDebug` | `void` | `-` | None |
| 19 | `CharacterAiSchedulingService` | `CharacterAiSchedulingService` | `ctor` | `public` | None |
| 27 | `CharacterAiSchedulingService` | `Register` | `void` | `public` | None |
| 32 | `CharacterAiSchedulingService` | `Unregister` | `void` | `public` | None |
| 40 | `CharacterAiSchedulingService` | `RequestImmediateDecision` | `void` | `public` | None |
| 45 | `CharacterAiSchedulingService` | `TryConsumePathSearchBudget` | `bool` | `public` | None |
| 50 | `CharacterAiSchedulingService` | `ShouldShowCharacterFeedback` | `bool` | `public` | None |
| 55 | `CharacterAiSchedulingService` | `GetMovementFrameStride` | `int` | `public` | None |
| 60 | `CharacterAiSchedulingService` | `ResetPathSearchBudgetForDebug` | `void` | `public` | None |
| 65 | `CharacterAiSchedulingService` | `ResolveScheduler` | `CharacterAiScheduler` | `private` | None |
| 72 | `CharacterAiSchedulingService` | `TryResolveScheduler` | `bool` | `private` | Scene query coupling |

### `Assets/Scripts/Infrastructure/CharacterSpawnerProvider.cs`

- Functions detected: 3
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `ICharacterSpawnerProvider` | `TryGetSpawner` | `bool` | `-` | None |
| 12 | `CharacterSpawnerProvider` | `CharacterSpawnerProvider` | `ctor` | `public` | None |
| 18 | `CharacterSpawnerProvider` | `TryGetSpawner` | `bool` | `public` | Scene query coupling |

### `Assets/Scripts/Infrastructure/CodexReferenceCatalogServices.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 14 | `DataCatalogCodexReferenceCatalog` | `DataCatalogCodexReferenceCatalog` | `ctor` | `public` | None |

### `Assets/Scripts/Infrastructure/DataCatalogService.cs`

- Functions detected: 4
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 12 | `DataManagerCatalog` | `DataManagerCatalog` | `ctor` | `public` | None |
| 29 | `IBuildingDefinitionLookup` | `GetBuilding` | `BuildingSO` | `-` | None |
| 35 | `BuildingDefinitionLookup` | `BuildingDefinitionLookup` | `ctor` | `public` | None |
| 40 | `BuildingDefinitionLookup` | `GetBuilding` | `BuildingSO` | `public` | None |

### `Assets/Scripts/Infrastructure/DataScriptableObjectSource.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IDataScriptableObjectSource` | `LoadAll` | `IReadOnlyCollection<DataScriptableObject>` | `-` | None |
| 14 | `ResourceDataScriptableObjectSource` | `ResourceDataScriptableObjectSource` | `ctor` | `public` | None |
| 20 | `ResourceDataScriptableObjectSource` | `LoadAll` | `IReadOnlyCollection<DataScriptableObject>` | `public` | None |

### `Assets/Scripts/Infrastructure/DungeonBackdropReferenceProvider.cs`

- Functions detected: 3
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 16 | `DungeonBackdropReferenceProvider` | `DungeonBackdropReferenceProvider` | `ctor` | `public` | None |
| 39 | `DungeonBackdropReferenceProvider` | `FindTransform` | `Transform` | `private` | Scene query coupling |
| 52 | `DungeonBackdropReferenceProvider` | `FindTilemap` | `Tilemap` | `private` | Scene query coupling |

### `Assets/Scripts/Infrastructure/DungeonGridBuildingRuntimeProviders.cs`

- Functions detected: 4
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 28 | `DungeonGridBuildingControllerProvider` | `DungeonGridBuildingControllerProvider` | `ctor` | `public` | None |
| 49 | `GridTextureProvider` | `GridTextureProvider` | `ctor` | `public` | None |
| 70 | `SceneCameraWorldPointerPositionProvider` | `SceneCameraWorldPointerPositionProvider` | `ctor` | `public` | None |
| 97 | `SceneMainCameraProvider` | `SceneMainCameraProvider` | `ctor` | `public` | None |

### `Assets/Scripts/Infrastructure/DungeonRuntimeLifetimeScope.cs`

- Functions detected: 4
- Judgment: manual resolver use is acceptable only in composition roots/factories; scene query is acceptable as a provider/composition boundary; non-UI code depends on UI types.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `DungeonRuntimeLifetimeScope` | `OnDestroy` | `void` | `protected override` | None |
| 11 | `DungeonRuntimeLifetimeScope` | `Configure` | `void` | `protected override` | None |
| 298 | `DungeonRuntimeLifetimeScope` | `InjectSceneComponents` | `void` | `private static` | Manual resolver/injection, Scene query coupling |
| 543 | `DungeonRuntimeLifetimeScope` | `CaptureSceneRuntimeReferences` | `DungeonSceneRuntimeReferences` | `private static` | Scene query coupling, UI type dependency |

### `Assets/Scripts/Infrastructure/DungeonSceneComponentQuery.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IDungeonSceneComponentQuery` | `First` | `T` | `-` | None |
| 8 | `IDungeonSceneComponentQuery` | `All` | `IReadOnlyList<T>` | `-` | None |
| 13 | `DungeonSceneComponentQuery` | `First` | `T` | `public` | None |
| 22 | `DungeonSceneComponentQuery` | `All` | `IReadOnlyList<T>` | `public` | None |
| 39 | `DungeonSceneComponentQuery` | `EnumerateLoadedSceneComponents` | `IEnumerable<T>` | `private static` | None |

### `Assets/Scripts/Infrastructure/DungeonSceneRuntimeReferences.cs`

- Functions detected: 1
- Judgment: non-UI code depends on UI types.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `DungeonSceneRuntimeReferences` | `DungeonSceneRuntimeReferences` | `ctor` | `public` | UI type dependency |

### `Assets/Scripts/Infrastructure/FacilityRecipeCatalogServices.cs`

- Functions detected: 25
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IFacilitySynthesisRecipeCatalog` | `GetRecipes` | `IReadOnlyList<FacilitySynthesisRecipeSO>` | `-` | None |
| 12 | `IFacilitySynthesisRecipeQuery` | `GetAllRecipes` | `IReadOnlyList<FacilitySynthesisRecipeSO>` | `-` | None |
| 13 | `IFacilitySynthesisRecipeQuery` | `IsVisible` | `bool` | `-` | None |
| 14 | `IFacilitySynthesisRecipeQuery` | `GetVisibleRecipes` | `IReadOnlyList<FacilitySynthesisRecipeSO>` | `-` | None |
| 15 | `IFacilitySynthesisRecipeQuery` | `ToSnapshot` | `FacilitySynthesisRecipeSnapshot` | `-` | None |
| 22 | `IFacilityEvolutionRecipeQuery` | `IsVisible` | `bool` | `-` | None |
| 23 | `IFacilityEvolutionRecipeQuery` | `GetVisibleRecipes` | `IReadOnlyList<FacilityEvolutionRecipeSO>` | `-` | None |
| 24 | `IFacilityEvolutionRecipeQuery` | `GetSourceCandidates` | `IReadOnlyList<FacilityEvolutionRecipeSO>` | `-` | None |
| 32 | `DataCatalogFacilitySynthesisRecipeCatalog` | `DataCatalogFacilitySynthesisRecipeCatalog` | `ctor` | `public` | None |
| 38 | `DataCatalogFacilitySynthesisRecipeCatalog` | `GetRecipes` | `IReadOnlyList<FacilitySynthesisRecipeSO>` | `public` | None |
| 54 | `FacilitySynthesisRecipeQuery` | `FacilitySynthesisRecipeQuery` | `ctor` | `public` | None |
| 64 | `FacilitySynthesisRecipeQuery` | `GetAllRecipes` | `IReadOnlyList<FacilitySynthesisRecipeSO>` | `public` | None |
| 69 | `FacilitySynthesisRecipeQuery` | `IsVisible` | `bool` | `public` | None |
| 74 | `FacilitySynthesisRecipeQuery` | `GetVisibleRecipes` | `IReadOnlyList<FacilitySynthesisRecipeSO>` | `public` | None |
| 81 | `FacilitySynthesisRecipeQuery` | `ToSnapshot` | `FacilitySynthesisRecipeSnapshot` | `public` | None |
| 93 | `DataCatalogFacilityEvolutionRecipeProvider` | `DataCatalogFacilityEvolutionRecipeProvider` | `ctor` | `public` | None |
| 99 | `DataCatalogFacilityEvolutionRecipeProvider` | `GetRecipes` | `IReadOnlyList<FacilityEvolutionRecipeSO>` | `public` | None |
| 116 | `FacilityEvolutionRecipeQuery` | `FacilityEvolutionRecipeQuery` | `ctor` | `public` | None |
| 129 | `FacilityEvolutionRecipeQuery` | `GetRecipes` | `IReadOnlyList<FacilityEvolutionRecipeSO>` | `public` | None |
| 134 | `FacilityEvolutionRecipeQuery` | `IsVisible` | `bool` | `public` | None |
| 139 | `FacilityEvolutionRecipeQuery` | `GetVisibleRecipes` | `IReadOnlyList<FacilityEvolutionRecipeSO>` | `public` | None |
| 146 | `FacilityEvolutionRecipeQuery` | `GetSourceCandidates` | `IReadOnlyList<FacilityEvolutionRecipeSO>` | `public` | None |
| 164 | `DataCatalogFacilityEvolutionRecordTokenDefinitionProvider` | `DataCatalogFacilityEvolutionRecordTokenDefinitionProvider` | `ctor` | `public` | None |
| 170 | `DataCatalogFacilityEvolutionRecordTokenDefinitionProvider` | `GetDefinitions` | `IReadOnlyList<FacilityEvolutionRecordTokenDefinitionSO>` | `public` | None |
| 181 | `DataCatalogFacilityEvolutionRecordTokenDefinitionProvider` | `GetDefinition` | `FacilityEvolutionRecordTokenDefinitionSO` | `public` | None |

### `Assets/Scripts/Infrastructure/FacilityShopRuntimeProvider.cs`

- Functions detected: 9
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IDailyFacilityShopRuntimeProvider` | `TryGetRuntime` | `bool` | `-` | None |
| 14 | `IFacilityShopCatalog` | `FindBuildingById` | `BuildingSO` | `-` | None |
| 19 | `IFacilityShopUnlockStateService` | `GetUnlockState` | `FacilityShopUnlockState` | `-` | None |
| 25 | `DailyFacilityShopRuntimeProvider` | `DailyFacilityShopRuntimeProvider` | `ctor` | `public` | None |
| 31 | `DailyFacilityShopRuntimeProvider` | `TryGetRuntime` | `bool` | `public` | Scene query coupling |
| 42 | `FacilityShopUnlockStateService` | `FacilityShopUnlockStateService` | `ctor` | `public` | None |
| 48 | `FacilityShopUnlockStateService` | `GetUnlockState` | `FacilityShopUnlockState` | `public` | None |
| 63 | `DataCatalogFacilityShopCatalog` | `DataCatalogFacilityShopCatalog` | `ctor` | `public` | None |
| 81 | `DataCatalogFacilityShopCatalog` | `FindBuildingById` | `BuildingSO` | `public` | None |

### `Assets/Scripts/Infrastructure/GameRuntimeServices.cs`

- Functions detected: 7
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IGameDataProvider` | `TryGetGameData` | `bool` | `-` | None |
| 12 | `IFloatingNumberFeedbackService` | `TryShow` | `bool` | `-` | None |
| 19 | `GameManagerGameDataProvider` | `GameManagerGameDataProvider` | `ctor` | `public` | None |
| 25 | `GameManagerGameDataProvider` | `TryGetGameData` | `bool` | `public` | Scene query coupling |
| 38 | `GameManagerFloatingNumberFeedbackService` | `GameManagerFloatingNumberFeedbackService` | `ctor` | `public` | None |
| 44 | `GameManagerFloatingNumberFeedbackService` | `TryShow` | `bool` | `public` | None |
| 58 | `GameManagerFloatingNumberFeedbackService` | `ResolveGameManager` | `GameManager` | `private` | Scene query coupling |

### `Assets/Scripts/Infrastructure/GridSystemProvider.cs`

- Functions detected: 5
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IGridSystemProvider` | `TryGetManager` | `bool` | `-` | None |
| 8 | `IGridSystemProvider` | `TryGetGrid` | `bool` | `-` | None |
| 15 | `GridSystemProvider` | `GridSystemProvider` | `ctor` | `public` | None |
| 46 | `GridSystemProvider` | `TryGetManager` | `bool` | `public` | Scene query coupling |
| 60 | `GridSystemProvider` | `TryGetGrid` | `bool` | `public` | None |

### `Assets/Scripts/Infrastructure/InvasionIntruderDataProvider.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `ResourceInvasionIntruderDataProvider` | `ResourceInvasionIntruderDataProvider` | `ctor` | `public` | None |
| 14 | `ResourceInvasionIntruderDataProvider` | `GetRequiredIntruderData` | `CharacterSO` | `public` | None |

### `Assets/Scripts/Infrastructure/InvasionThreatRuntimeProvider.cs`

- Functions detected: 3
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 3 | `IInvasionThreatRuntimeProvider` | `TryGetRuntime` | `bool` | `-` | None |
| 10 | `InvasionThreatRuntimeProvider` | `InvasionThreatRuntimeProvider` | `ctor` | `public` | None |
| 16 | `InvasionThreatRuntimeProvider` | `TryGetRuntime` | `bool` | `public` | Scene query coupling |

### `Assets/Scripts/Infrastructure/LocalLlmRuntimeProvider.cs`

- Functions detected: 5
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `ILocalLlmRuntimeProvider` | `TryGetRuntime` | `bool` | `-` | None |
| 6 | `ILocalLlmRuntimeProvider` | `GetRequiredRuntime` | `ILocalLlmRuntime` | `-` | None |
| 13 | `LocalLlmRuntimeProvider` | `LocalLlmRuntimeProvider` | `ctor` | `public` | None |
| 18 | `LocalLlmRuntimeProvider` | `TryGetRuntime` | `bool` | `public` | Scene query coupling |
| 25 | `LocalLlmRuntimeProvider` | `GetRequiredRuntime` | `ILocalLlmRuntime` | `public` | None |

### `Assets/Scripts/Infrastructure/MetaProgressionRuntimeProvider.cs`

- Functions detected: 16
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IMetaProgressionRuntimeProvider` | `TryGetRuntime` | `bool` | `-` | None |
| 11 | `IMetaProgressionRuntimeReader` | `GetStartingFacilityCandidateBonus` | `int` | `-` | None |
| 12 | `IMetaProgressionRuntimeReader` | `GetStartingOwnerTraitCandidateBonus` | `int` | `-` | None |
| 13 | `IMetaProgressionRuntimeReader` | `GetOwnerMaxHealthMultiplier` | `float` | `-` | None |
| 14 | `IMetaProgressionRuntimeReader` | `GetInvasionWarningThresholdMultiplier` | `float` | `-` | None |
| 15 | `IMetaProgressionRuntimeReader` | `IsRecipePreserved` | `bool` | `-` | None |
| 16 | `IMetaProgressionRuntimeReader` | `GetExpandedBasicPurchaseBuildingIds` | `IReadOnlyCollection<int>` | `-` | None |
| 22 | `MetaProgressionRuntimeProvider` | `MetaProgressionRuntimeProvider` | `ctor` | `public` | None |
| 28 | `MetaProgressionRuntimeProvider` | `TryGetRuntime` | `bool` | `public` | Scene query coupling |
| 39 | `MetaProgressionRuntimeReader` | `MetaProgressionRuntimeReader` | `ctor` | `public` | None |
| 45 | `MetaProgressionRuntimeReader` | `GetStartingFacilityCandidateBonus` | `int` | `public` | None |
| 52 | `MetaProgressionRuntimeReader` | `GetStartingOwnerTraitCandidateBonus` | `int` | `public` | None |
| 59 | `MetaProgressionRuntimeReader` | `GetOwnerMaxHealthMultiplier` | `float` | `public` | None |
| 66 | `MetaProgressionRuntimeReader` | `GetInvasionWarningThresholdMultiplier` | `float` | `public` | None |
| 73 | `MetaProgressionRuntimeReader` | `IsRecipePreserved` | `bool` | `public` | None |
| 79 | `MetaProgressionRuntimeReader` | `GetExpandedBasicPurchaseBuildingIds` | `IReadOnlyCollection<int>` | `public` | None |

### `Assets/Scripts/Infrastructure/PlayerInputServices.cs`

- Functions detected: 9
- Judgment: Direct engine input/camera access is intentionally isolated here as an adapter; gameplay/UI callers should depend on `IPlayerInputReader` or `IWorldPointerRaycaster`.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `IPlayerInputReader` | `GetKey` | `bool` | `-` | None |
| 10 | `IPlayerInputReader` | `GetKeyDown` | `bool` | `-` | None |
| 11 | `IPlayerInputReader` | `GetMouseButtonDown` | `bool` | `-` | None |
| 16 | `IWorldPointerRaycaster` | `TryRaycast` | `bool` | `-` | None |
| 24 | `UnityPlayerInputReader` | `GetKey` | `bool` | `public` | Direct input/camera dependency |
| 29 | `UnityPlayerInputReader` | `GetKeyDown` | `bool` | `public` | Direct input/camera dependency |
| 34 | `UnityPlayerInputReader` | `GetMouseButtonDown` | `bool` | `public` | Direct input/camera dependency |
| 44 | `PhysicsWorldPointerRaycaster` | `PhysicsWorldPointerRaycaster` | `ctor` | `public` | None |
| 49 | `PhysicsWorldPointerRaycaster` | `TryRaycast` | `bool` | `public` | Direct input/camera dependency |

### `Assets/Scripts/Infrastructure/RegularCustomerRuntimeProvider.cs`

- Functions detected: 3
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `IRegularCustomerRuntimeProvider` | `TryGetRuntime` | `bool` | `-` | None |
| 12 | `RegularCustomerRuntimeProvider` | `RegularCustomerRuntimeProvider` | `ctor` | `public` | None |
| 18 | `RegularCustomerRuntimeProvider` | `TryGetRuntime` | `bool` | `public` | Scene query coupling |

### `Assets/Scripts/Infrastructure/ResourcesAssetLoader.cs`

- Functions detected: 5
- Judgment: `Resources` access is intentionally isolated here behind `IResourcesAssetLoader`; callers should not call `Resources` directly.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `IResourcesAssetLoader` | `LoadRequired` | `T` | `-` | None |
| 9 | `IResourcesAssetLoader` | `LoadAllRequired` | `IReadOnlyCollection<T>` | `-` | None |
| 14 | `UnityResourcesAssetLoader` | `LoadRequired` | `T` | `public` | Resource loading in caller |
| 27 | `UnityResourcesAssetLoader` | `LoadAllRequired` | `IReadOnlyCollection<T>` | `public` | None |
| 45 | `UnityResourcesAssetLoader` | `ValidateResourcePath` | `void` | `private static` | None |

### `Assets/Scripts/Infrastructure/RuntimePanelProviders.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 22 | `FacilityEvolutionRuntimeProvider` | `FacilityEvolutionRuntimeProvider` | `ctor` | `public` | None |
| 43 | `FacilitySynthesisRuntimeProvider` | `FacilitySynthesisRuntimeProvider` | `ctor` | `public` | None |
| 64 | `CodexRuntimeProvider` | `CodexRuntimeProvider` | `ctor` | `public` | None |

### `Assets/Scripts/Infrastructure/RunVariableCatalogServices.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 28 | `ResourceRunCharacterCatalog` | `ResourceRunCharacterCatalog` | `ctor` | `public` | None |
| 51 | `ResourceOwnerCandidateCatalog` | `ResourceOwnerCandidateCatalog` | `ctor` | `public` | None |
| 69 | `RunStartVariableCatalog` | `RunStartVariableCatalog` | `ctor` | `public` | None |

### `Assets/Scripts/Infrastructure/RunVariableRuntimeProvider.cs`

- Functions detected: 26
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IRunVariableRuntimeProvider` | `TryGetRuntime` | `bool` | `-` | None |
| 11 | `IRunVariableRuntimeReader` | `GetInitialShopSeed` | `int` | `-` | None |
| 12 | `IRunVariableRuntimeReader` | `GetGuestDemandMultiplier` | `float` | `-` | None |
| 13 | `IRunVariableRuntimeReader` | `GetStockCostMultiplier` | `float` | `-` | None |
| 14 | `IRunVariableRuntimeReader` | `GetFacilityShopCostMultiplier` | `float` | `-` | None |
| 15 | `IRunVariableRuntimeReader` | `GetBlueprintCostMultiplier` | `float` | `-` | None |
| 16 | `IRunVariableRuntimeReader` | `GetThreatRiseMultiplier` | `float` | `-` | None |
| 17 | `IRunVariableRuntimeReader` | `GetWarningThresholdMultiplier` | `float` | `-` | None |
| 18 | `IRunVariableRuntimeReader` | `ApplyInvasionSettings` | `InvasionIntruderSettings` | `-` | None |
| 28 | `IOwnerRunManagerProvider` | `TryGetManager` | `bool` | `-` | None |
| 33 | `IOwnerRunLifecycleService` | `HandleOwnerDeath` | `void` | `-` | None |
| 40 | `RunVariableRuntimeProvider` | `RunVariableRuntimeProvider` | `ctor` | `public` | None |
| 46 | `RunVariableRuntimeProvider` | `TryGetRuntime` | `bool` | `public` | Scene query coupling |
| 58 | `RunVariableRuntimeReader` | `RunVariableRuntimeReader` | `ctor` | `public` | None |
| 64 | `RunVariableRuntimeReader` | `GetInitialShopSeed` | `int` | `public` | None |
| 72 | `RunVariableRuntimeReader` | `GetGuestDemandMultiplier` | `float` | `public` | None |
| 79 | `RunVariableRuntimeReader` | `GetStockCostMultiplier` | `float` | `public` | None |
| 86 | `RunVariableRuntimeReader` | `GetFacilityShopCostMultiplier` | `float` | `public` | None |
| 93 | `RunVariableRuntimeReader` | `GetBlueprintCostMultiplier` | `float` | `public` | None |
| 100 | `RunVariableRuntimeReader` | `GetThreatRiseMultiplier` | `float` | `public` | None |
| 107 | `RunVariableRuntimeReader` | `GetWarningThresholdMultiplier` | `float` | `public` | None |
| 114 | `RunVariableRuntimeReader` | `ApplyInvasionSettings` | `InvasionIntruderSettings` | `public` | None |
| 127 | `OwnerRunDataProvider` | `OwnerRunDataProvider` | `ctor` | `public` | None |
| 143 | `OwnerRunDataProvider` | `TryGetManager` | `bool` | `public` | Scene query coupling |
| 155 | `OwnerRunLifecycleService` | `OwnerRunLifecycleService` | `ctor` | `public` | None |
| 161 | `OwnerRunLifecycleService` | `HandleOwnerDeath` | `void` | `public` | None |

### `Assets/Scripts/Infrastructure/SceneBuildableLeakValidator.cs`

- Functions detected: 9
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 10 | `SceneBuildableLeakValidator` | `SceneBuildableLeakValidator` | `ctor` | `public` | None |
| 15 | `SceneBuildableLeakValidator` | `Initialize` | `void` | `public` | None |
| 34 | `SceneBuildableLeakValidator` | `CollectLeakedFacilities` | `void` | `private` | Scene query coupling |
| 50 | `SceneBuildableLeakValidator` | `CollectMissingScriptObjects` | `void` | `private static` | None |
| 65 | `SceneBuildableLeakValidator` | `CollectMissingScriptObjects` | `void` | `private static` | None |
| 89 | `SceneBuildableLeakValidator` | `IsLeakedFacilityRoot` | `bool` | `private static` | None |
| 107 | `SceneBuildableLeakValidator` | `DescribeLeakedBuildable` | `string` | `private static` | None |
| 113 | `SceneBuildableLeakValidator` | `DescribeMissingScript` | `string` | `private static` | None |
| 118 | `SceneBuildableLeakValidator` | `GetHierarchyPath` | `string` | `private static` | None |

### `Assets/Scripts/Infrastructure/ShopStockCatalogService.cs`

- Functions detected: 7
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IShopStockCatalog` | `TryGetStockInfoForShop` | `bool` | `-` | None |
| 8 | `IShopStockCatalog` | `TryGetSaleItem` | `bool` | `-` | None |
| 9 | `IShopStockCatalog` | `GetStockCategory` | `StockCategory` | `-` | None |
| 15 | `ShopStockCatalog` | `ShopStockCatalog` | `ctor` | `public` | None |
| 21 | `ShopStockCatalog` | `TryGetStockInfoForShop` | `bool` | `public` | None |
| 28 | `ShopStockCatalog` | `TryGetSaleItem` | `bool` | `public` | None |
| 34 | `ShopStockCatalog` | `GetStockCategory` | `StockCategory` | `public` | None |

### `Assets/Scripts/Infrastructure/SocialReputationRuntimeProvider.cs`

- Functions detected: 6
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `ISocialReputationRuntimeProvider` | `TryGetRuntime` | `bool` | `-` | None |
| 10 | `ISocialReputationBiasService` | `GetFacilityUtilityBias` | `float` | `-` | None |
| 17 | `SocialReputationRuntimeProvider` | `SocialReputationRuntimeProvider` | `ctor` | `public` | None |
| 22 | `SocialReputationRuntimeProvider` | `TryGetRuntime` | `bool` | `public` | Scene query coupling |
| 34 | `SocialReputationBiasService` | `SocialReputationBiasService` | `ctor` | `public` | None |
| 39 | `SocialReputationBiasService` | `GetFacilityUtilityBias` | `float` | `public` | None |

### `Assets/Scripts/Infrastructure/StaffDiscontentRuntimeProvider.cs`

- Functions detected: 12
- Judgment: scene query is acceptable as a provider/composition boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `IStaffDiscontentRuntimeProvider` | `TryGetRuntime` | `bool` | `-` | None |
| 10 | `IStaffDiscontentRuntimeService` | `GetWorkEfficiencyMultiplier` | `float` | `-` | None |
| 11 | `IStaffDiscontentRuntimeService` | `ShouldBlockWork` | `bool` | `-` | None |
| 12 | `IStaffDiscontentRuntimeService` | `IsRebellionTarget` | `bool` | `-` | None |
| 13 | `IStaffDiscontentRuntimeService` | `ResolveSuppressedRebel` | `bool` | `-` | None |
| 20 | `StaffDiscontentRuntimeProvider` | `StaffDiscontentRuntimeProvider` | `ctor` | `public` | None |
| 26 | `StaffDiscontentRuntimeProvider` | `TryGetRuntime` | `bool` | `public` | Scene query coupling |
| 38 | `StaffDiscontentRuntimeService` | `StaffDiscontentRuntimeService` | `ctor` | `public` | None |
| 44 | `StaffDiscontentRuntimeService` | `GetWorkEfficiencyMultiplier` | `float` | `public` | None |
| 51 | `StaffDiscontentRuntimeService` | `ShouldBlockWork` | `bool` | `public` | None |
| 59 | `StaffDiscontentRuntimeService` | `IsRebellionTarget` | `bool` | `public` | None |
| 66 | `StaffDiscontentRuntimeService` | `ResolveSuppressedRebel` | `bool` | `public` | None |

### `Assets/Scripts/Infrastructure/TmpKoreanFontProvider.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 11 | `ResourceTmpKoreanFontProvider` | `ResourceTmpKoreanFontProvider` | `ctor` | `public` | None |
| 17 | `ResourceTmpKoreanFontProvider` | `GetRequiredFont` | `TMP_FontAsset` | `public` | None |

### `Assets/Scripts/Invasion/InvasionCombatReportRuntime.cs`

- Functions detected: 10
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 18 | `InvasionCombatReportRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 24 | `InvasionCombatReportRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 30 | `InvasionCombatReportRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 47 | `InvasionCombatReportRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 57 | `InvasionCombatReportRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 67 | `InvasionCombatReportRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 82 | `InvasionCombatReportRuntime` | `OnEnable` | `void` | `private` | None |
| 92 | `InvasionCombatReportRuntime` | `OnDisable` | `void` | `private` | None |
| 102 | `InvasionCombatReportRuntime` | `EnsureReport` | `InvasionCombatReport` | `private` | None |
| 108 | `InvasionCombatReportRuntime` | `ClampLine` | `string` | `private` | None |

### `Assets/Scripts/Invasion/InvasionCombatReportSystem.cs`

- Functions detected: 23
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 10 | `InvasionCombatFeedbackEvent` | `InvasionCombatFeedbackEvent` | `ctor` | `public` | None |
| 18 | `InvasionCombatFeedbackEvent` | `Trigger` | `void` | `public static` | None |
| 30 | `InvasionCombatReportReadyEvent` | `InvasionCombatReportReadyEvent` | `ctor` | `public` | None |
| 37 | `InvasionCombatReportReadyEvent` | `Trigger` | `void` | `public static` | None |
| 52 | `InvasionCombatReport` | `InvasionCombatReport` | `ctor` | `public` | None |
| 87 | `InvasionCombatReport` | `SetIntruder` | `void` | `public` | None |
| 92 | `InvasionCombatReport` | `RecordDefenseActivation` | `void` | `public` | None |
| 120 | `InvasionCombatReport` | `RecordFacilityDamage` | `void` | `public` | None |
| 130 | `InvasionCombatReport` | `RecordFinalCombat` | `void` | `public` | None |
| 136 | `InvasionCombatReport` | `Resolve` | `void` | `public` | None |
| 143 | `InvasionCombatReport` | `ToDetailText` | `string` | `public` | None |
| 182 | `InvasionCombatReport` | `FindOrCreateContribution` | `DefenseContribution` | `private` | None |
| 195 | `InvasionCombatReport` | `AddObservation` | `void` | `private` | None |
| 203 | `InvasionCombatReport` | `GetDecisiveDefenseText` | `string` | `private` | None |
| 226 | `InvasionCombatReport` | `FormatFacilities` | `string` | `private` | None |
| 238 | `InvasionCombatReport` | `FormatObservations` | `string` | `private` | None |
| 243 | `InvasionCombatReport` | `AddContributionLine` | `void` | `private static` | None |
| 266 | `DefenseContribution` | `DefenseContribution` | `ctor` | `public` | None |
| 278 | `DefenseContribution` | `Add` | `void` | `public` | None |
| 302 | `InvasionCombatReportFormatter` | `FormatActivation` | `string` | `public static` | None |
| 331 | `InvasionCombatReportFormatter` | `FormatObservation` | `string` | `public static` | None |
| 377 | `InvasionCombatReportFormatter` | `FormatSynergy` | `string` | `public static` | None |
| 419 | `InvasionCombatReportFormatter` | `GetBuildingName` | `string` | `public static` | None |

### `Assets/Scripts/Invasion/InvasionDefenseSummaryQuery.cs`

- Functions detected: 6
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `InvasionDefenseSummary` | `InvasionDefenseSummary` | `ctor` | `public` | None |
| 41 | `IInvasionDefenseSummaryService` | `Capture` | `InvasionDefenseSummary` | `-` | None |
| 55 | `InvasionDefenseSummaryRuntimeSource` | `InvasionDefenseSummaryRuntimeSource` | `ctor` | `public` | None |
| 70 | `InvasionDefenseSummaryService` | `InvasionDefenseSummaryService` | `ctor` | `public` | None |
| 75 | `InvasionDefenseSummaryService` | `Capture` | `InvasionDefenseSummary` | `public` | None |
| 95 | `InvasionDefenseSummaryService` | `CountFacilities` | `void` | `private static` | None |

### `Assets/Scripts/Invasion/InvasionFacilityDamageResolver.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `InvasionFacilityDamageResolver` | `TryFindDamageTarget` | `bool` | `public static` | None |
| 40 | `InvasionFacilityDamageResolver` | `IsDamageableFacility` | `bool` | `private static` | None |

### `Assets/Scripts/Invasion/InvasionIntruderContext.cs`

- Functions detected: 10
- Judgment: scene discovery should be isolated behind providers/build callbacks.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `InvasionIntruderEntry` | `InvasionIntruderEntry` | `ctor` | `public` | None |
| 23 | `IInvasionIntruderContext` | `TryGetGrid` | `bool` | `-` | None |
| 24 | `IInvasionIntruderContext` | `TryGetOwner` | `bool` | `-` | None |
| 25 | `IInvasionIntruderContext` | `TryResolveEntry` | `bool` | `-` | None |
| 26 | `IInvasionIntruderContext` | `ApplyRunVariables` | `InvasionIntruderSettings` | `-` | None |
| 36 | `InvasionIntruderContext` | `InvasionIntruderContext` | `ctor` | `public` | None |
| 49 | `InvasionIntruderContext` | `TryGetGrid` | `bool` | `public` | None |
| 63 | `InvasionIntruderContext` | `TryGetOwner` | `bool` | `public` | Scene query coupling |
| 70 | `InvasionIntruderContext` | `TryResolveEntry` | `bool` | `public` | Scene query coupling |
| 77 | `InvasionIntruderContext` | `ApplyRunVariables` | `InvasionIntruderSettings` | `public` | None |

### `Assets/Scripts/Invasion/InvasionIntruderDataResolver.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 3 | `IInvasionIntruderDataProvider` | `GetRequiredIntruderData` | `CharacterSO` | `-` | None |

### `Assets/Scripts/Invasion/InvasionIntruderEntryResolver.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `InvasionIntruderEntryResolver` | `TryResolve` | `bool` | `public static` | None |

### `Assets/Scripts/Invasion/InvasionIntruderEvents.cs`

- Functions detected: 6
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `InvasionSpawnedEvent` | `InvasionSpawnedEvent` | `ctor` | `public` | None |
| 13 | `InvasionSpawnedEvent` | `Trigger` | `void` | `public static` | None |
| 26 | `InvasionFacilityDamagedEvent` | `InvasionFacilityDamagedEvent` | `ctor` | `public` | None |
| 34 | `InvasionFacilityDamagedEvent` | `Trigger` | `void` | `public static` | None |
| 47 | `InvasionFinalCombatStartedEvent` | `InvasionFinalCombatStartedEvent` | `ctor` | `public` | None |
| 55 | `InvasionFinalCombatStartedEvent` | `Trigger` | `void` | `public static` | None |

### `Assets/Scripts/Invasion/InvasionIntruderFactory.cs`

- Functions detected: 5
- Judgment: object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IInvasionIntruderFactory` | `Create` | `InvasionIntruderRuntime` | `-` | None |
| 7 | `IInvasionIntruderFactory` | `EnsureRuntime` | `InvasionIntruderRuntime` | `-` | None |
| 14 | `InvasionIntruderRuntimeFactory` | `InvasionIntruderRuntimeFactory` | `ctor` | `public` | None |
| 20 | `InvasionIntruderRuntimeFactory` | `Create` | `InvasionIntruderRuntime` | `public` | Runtime object creation |
| 30 | `InvasionIntruderRuntimeFactory` | `EnsureRuntime` | `InvasionIntruderRuntime` | `public` | None |

### `Assets/Scripts/Invasion/InvasionIntruderModel.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/Invasion/InvasionIntruderPlanner.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `InvasionIntruderPlanner` | `CalculateFocus` | `float` | `public static` | None |
| 11 | `InvasionIntruderPlanner` | `GetNextPath` | `Queue<GridMoveStep>` | `public static` | None |
| 47 | `InvasionIntruderPlanner` | `SelectExploreTarget` | `Vector2Int` | `public static` | None |
| 90 | `InvasionIntruderPlanner` | `IsAtOwner` | `bool` | `public static` | None |
| 100 | `InvasionIntruderPlanner` | `Manhattan` | `int` | `private static` | None |

### `Assets/Scripts/Invasion/InvasionIntruderSystem.cs`

- Functions detected: 27
- Judgment: timed workflow may deserve a service if it coordinates domain state.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 21 | `InvasionDirectorRuntime` | `Construct` | `void` | `public` | None |
| 38 | `InvasionDirectorRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 43 | `InvasionDirectorRuntime` | `TrySpawnIntruder` | `bool` | `public` | None |
| 78 | `InvasionDirectorRuntime` | `OnEnable` | `void` | `private` | None |
| 83 | `InvasionDirectorRuntime` | `OnDisable` | `void` | `private` | None |
| 88 | `InvasionDirectorRuntime` | `ResolveIntruderData` | `CharacterSO` | `private` | None |
| 94 | `InvasionDirectorRuntime` | `ResolveIntruderDataProvider` | `IInvasionIntruderDataProvider` | `private` | None |
| 100 | `InvasionDirectorRuntime` | `ResolveInvasionContext` | `IInvasionIntruderContext` | `private` | None |
| 106 | `InvasionDirectorRuntime` | `ResolveIntruderFactory` | `IInvasionIntruderFactory` | `private` | None |
| 112 | `InvasionDirectorRuntime` | `ResolveDefenseStatusRuntimeService` | `IDefenseStatusRuntimeService` | `private` | None |
| 118 | `InvasionDirectorRuntime` | `OnIntruderFinished` | `void` | `private` | None |
| 148 | `InvasionIntruderRuntime` | `Awake` | `void` | `private` | None |
| 154 | `InvasionIntruderRuntime` | `Initialize` | `void` | `public` | None |
| 164 | `InvasionIntruderRuntime` | `Begin` | `void` | `public` | Coroutine/timing orchestration |
| 190 | `InvasionIntruderRuntime` | `CreateNextPath` | `Queue<GridMoveStep>` | `public` | None |
| 196 | `InvasionIntruderRuntime` | `TryDamageNearbyFacility` | `bool` | `public` | None |
| 215 | `InvasionIntruderRuntime` | `ApplyFinalCombat` | `void` | `public` | None |
| 229 | `InvasionIntruderRuntime` | `ResolveSuppressedBy` | `void` | `public` | None |
| 246 | `InvasionIntruderRuntime` | `Run` | `IEnumerator` | `private` | Coroutine/timing orchestration |
| 309 | `InvasionIntruderRuntime` | `MovePathWithDefense` | `IEnumerator` | `private` | Coroutine/timing orchestration |
| 345 | `InvasionIntruderRuntime` | `TickDefenseStatuses` | `void` | `private` | None |
| 358 | `InvasionIntruderRuntime` | `FinalCombat` | `IEnumerator` | `private` | Coroutine/timing orchestration |
| 370 | `InvasionIntruderRuntime` | `ResolveIntruderDefeated` | `void` | `private` | None |
| 381 | `InvasionIntruderRuntime` | `Finish` | `void` | `private` | None |
| 393 | `InvasionIntruderRuntime` | `RequireRuntimeComponents` | `void` | `private` | None |
| 413 | `InvasionIntruderRuntime` | `ResolveInvasionContext` | `IInvasionIntruderContext` | `private` | None |
| 419 | `InvasionIntruderRuntime` | `ResolveDefenseStatusRuntimeService` | `IDefenseStatusRuntimeService` | `private` | None |

### `Assets/Scripts/Invasion/InvasionThreatRuntime.cs`

- Functions detected: 18
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 31 | `InvasionThreatRuntime` | `Construct` | `void` | `public` | None |
| 45 | `InvasionThreatRuntime` | `Update` | `void` | `private` | None |
| 50 | `InvasionThreatRuntime` | `Tick` | `void` | `public` | None |
| 91 | `InvasionThreatRuntime` | `AddThreat` | `void` | `public` | None |
| 99 | `InvasionThreatRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 104 | `InvasionThreatRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 113 | `InvasionThreatRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 123 | `InvasionThreatRuntime` | `OnEnable` | `void` | `private` | None |
| 130 | `InvasionThreatRuntime` | `OnDisable` | `void` | `private` | None |
| 137 | `InvasionThreatRuntime` | `TryRaiseWarning` | `void` | `private` | None |
| 163 | `InvasionThreatRuntime` | `TickCandidateDelay` | `void` | `private` | None |
| 199 | `InvasionThreatRuntime` | `ResetAfterInvasion` | `void` | `private` | None |
| 211 | `InvasionThreatRuntime` | `BuildSnapshot` | `InvasionThreatSnapshot` | `private` | None |
| 221 | `InvasionThreatRuntime` | `ResolveStage` | `InvasionThreatStage` | `private` | None |
| 245 | `InvasionThreatRuntime` | `SampleWorldFactors` | `InvasionThreatFactors` | `private` | None |
| 255 | `InvasionThreatRuntime` | `GetWarningThresholdMultiplier` | `float` | `private` | None |
| 261 | `InvasionThreatRuntime` | `RequireRunVariableReader` | `IRunVariableRuntimeReader` | `private` | None |
| 271 | `InvasionThreatRuntime` | `RequireMetaProgressionReader` | `IMetaProgressionRuntimeReader` | `private` | None |

### `Assets/Scripts/Invasion/InvasionThreatSystem.cs`

- Functions detected: 16
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 43 | `InvasionThreatSettings` | `GetDifficultyMultiplier` | `float` | `public` | None |
| 53 | `InvasionThreatSettings` | `GetCandidateDelay` | `float` | `public` | None |
| 68 | `InvasionThreatFactors` | `InvasionThreatFactors` | `ctor` | `public` | None |
| 85 | `InvasionThreatSnapshot` | `InvasionThreatSnapshot` | `ctor` | `public` | None |
| 104 | `InvasionThreatWarningEvent` | `InvasionThreatWarningEvent` | `ctor` | `public` | None |
| 111 | `InvasionThreatWarningEvent` | `Trigger` | `void` | `public static` | None |
| 122 | `InvasionCandidateEvent` | `InvasionCandidateEvent` | `ctor` | `public` | None |
| 129 | `InvasionCandidateEvent` | `Trigger` | `void` | `public static` | None |
| 140 | `InvasionStartedEvent` | `InvasionStartedEvent` | `ctor` | `public` | None |
| 147 | `InvasionStartedEvent` | `Trigger` | `void` | `public static` | None |
| 159 | `InvasionResolvedEvent` | `InvasionResolvedEvent` | `ctor` | `public` | None |
| 167 | `InvasionResolvedEvent` | `Trigger` | `void` | `public static` | None |
| 178 | `InvasionThreatCalculator` | `CalculateRisePerSecond` | `float` | `public static` | None |
| 182 | `InvasionThreatCalculator` | `CalculateRisePerSecond` | `float` | `public static` | None |
| 198 | `InvasionThreatCalculator` | `BuildWarningDetail` | `string` | `public static` | None |
| 230 | `InvasionThreatCalculator` | `BuildCandidateDetail` | `string` | `public static` | None |

### `Assets/Scripts/Invasion/InvasionThreatWorldSampler.cs`

- Functions detected: 6
- Judgment: scene discovery should be isolated behind providers/build callbacks.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IInvasionThreatWorldSampler` | `Sample` | `InvasionThreatFactors` | `-` | None |
| 12 | `InvasionThreatWorldSampler` | `InvasionThreatWorldSampler` | `ctor` | `public` | None |
| 18 | `InvasionThreatWorldSampler` | `Sample` | `InvasionThreatFactors` | `public` | Scene query coupling |
| 30 | `InvasionThreatWorldSampler` | `CalculateDungeonValue` | `float` | `private static` | None |
| 59 | `InvasionThreatWorldSampler` | `CalculateReputation` | `float` | `private static` | None |
| 92 | `InvasionThreatWorldSampler` | `CalculateRisk` | `float` | `private static` | None |

### `Assets/Scripts/Meta/MetaProgressionCalculator.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `MetaProgressionCalculator` | `CalculateLegacyCurrency` | `int` | `public static` | None |
| 24 | `MetaProgressionCalculator` | `GetThreatStageScore` | `int` | `private static` | None |

### `Assets/Scripts/Meta/MetaProgressionCatalog.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `MetaProgressionCatalog` | `Get` | `MetaUpgradeDefinition` | `public static` | None |

### `Assets/Scripts/Meta/MetaProgressionEvents.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 4 | `RunResultReadyEvent` | `RunResultReadyEvent` | `ctor` | `public` | None |
| 11 | `RunResultReadyEvent` | `Trigger` | `void` | `public static` | None |

### `Assets/Scripts/Meta/MetaProgressionModel.cs`

- Functions detected: 9
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 43 | `MetaProgressionState` | `AddCurrency` | `void` | `public` | None |
| 48 | `MetaProgressionState` | `GetUpgradeLevel` | `int` | `public` | None |
| 53 | `MetaProgressionState` | `TryPurchaseUpgrade` | `bool` | `public` | None |
| 82 | `MetaProgressionState` | `SetUpgradeLevelForDebug` | `void` | `public` | None |
| 90 | `MetaProgressionState` | `PreserveRecipes` | `void` | `public` | None |
| 120 | `RunResultSnapshot` | `ToDetailText` | `string` | `public` | None |
| 145 | `RunResultSnapshot` | `FormatTime` | `string` | `private static` | None |
| 153 | `RunResultSnapshot` | `TextOrDefault` | `string` | `private static` | None |
| 158 | `RunResultSnapshot` | `FormatThreatStage` | `string` | `private static` | None |

### `Assets/Scripts/Meta/MetaProgressionRunResultServices.cs`

- Functions detected: 7
- Judgment: scene discovery should be isolated behind providers/build callbacks.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `MetaRunResultBuildContext` | `MetaRunResultBuildContext` | `ctor` | `public` | None |
| 46 | `IMetaRunResultBuilder` | `Build` | `RunResultSnapshot` | `-` | None |
| 52 | `MetaRunResultBuilder` | `MetaRunResultBuilder` | `ctor` | `public` | None |
| 58 | `MetaRunResultBuilder` | `Build` | `RunResultSnapshot` | `public` | None |
| 93 | `IRunResultPanelService` | `Show` | `RunResultPanel` | `-` | None |
| 100 | `RunResultPanelService` | `RunResultPanelService` | `ctor` | `public` | None |
| 110 | `RunResultPanelService` | `Show` | `RunResultPanel` | `public` | Scene query coupling |

### `Assets/Scripts/Meta/MetaProgressionSystem.cs`

- Functions detected: 28
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 31 | `MetaProgressionRuntime` | `Construct` | `void` | `public` | None |
| 42 | `MetaProgressionRuntime` | `SetShowRunResultPanel` | `void` | `public` | None |
| 47 | `MetaProgressionRuntime` | `Awake` | `void` | `private` | None |
| 52 | `MetaProgressionRuntime` | `StartNewRun` | `void` | `public` | None |
| 59 | `MetaProgressionRuntime` | `TryPurchaseUpgrade` | `bool` | `public` | None |
| 70 | `MetaProgressionRuntime` | `GetStartingFacilityCandidateBonus` | `int` | `public` | None |
| 75 | `MetaProgressionRuntime` | `GetStartingOwnerTraitCandidateBonus` | `int` | `public` | None |
| 80 | `MetaProgressionRuntime` | `GetOwnerMaxHealthMultiplier` | `float` | `public` | None |
| 85 | `MetaProgressionRuntime` | `GetInvasionWarningThresholdMultiplier` | `float` | `public` | None |
| 90 | `MetaProgressionRuntime` | `IsRecipePreserved` | `bool` | `public` | None |
| 96 | `MetaProgressionRuntime` | `GetExpandedBasicPurchaseBuildingIds` | `IReadOnlyCollection<int>` | `public` | None |
| 116 | `MetaProgressionRuntime` | `RecordOffenseSuccess` | `void` | `public` | None |
| 121 | `MetaProgressionRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 131 | `MetaProgressionRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 136 | `MetaProgressionRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 141 | `MetaProgressionRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 146 | `MetaProgressionRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 151 | `MetaProgressionRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 156 | `MetaProgressionRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 161 | `MetaProgressionRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 166 | `MetaProgressionRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 171 | `MetaProgressionRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 176 | `MetaProgressionRuntime` | `EndRun` | `RunResultSnapshot` | `public` | None |
| 200 | `MetaProgressionRuntime` | `ResolveRunResultBuilder` | `IMetaRunResultBuilder` | `private` | None |
| 206 | `MetaProgressionRuntime` | `ResolveRunResultPanelService` | `IRunResultPanelService` | `private` | None |
| 212 | `MetaProgressionRuntime` | `PreserveRunRecipes` | `void` | `private` | None |
| 218 | `MetaProgressionRuntime` | `OnEnable` | `void` | `private` | None |
| 232 | `MetaProgressionRuntime` | `OnDisable` | `void` | `private` | None |

### `Assets/Scripts/Meta/MetaRunProgressTracker.cs`

- Functions detected: 13
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 19 | `MetaRunProgressTracker` | `StartNewRun` | `void` | `public` | None |
| 32 | `MetaRunProgressTracker` | `RecordOperatingDayStarted` | `void` | `public` | None |
| 37 | `MetaRunProgressTracker` | `RecordOperatingDayReport` | `void` | `public` | None |
| 48 | `MetaRunProgressTracker` | `RecordThreat` | `void` | `public` | None |
| 58 | `MetaRunProgressTracker` | `RecordInvasionStarted` | `void` | `public` | None |
| 64 | `MetaRunProgressTracker` | `RecordInvasionResolved` | `void` | `public` | None |
| 72 | `MetaRunProgressTracker` | `RecordFacilityVisit` | `void` | `public` | None |
| 81 | `MetaRunProgressTracker` | `RecordBlueprintResearchCompleted` | `void` | `public` | None |
| 89 | `MetaRunProgressTracker` | `RecordFacilitySynthesisCompleted` | `void` | `public` | None |
| 104 | `MetaRunProgressTracker` | `RecordOffenseSuccess` | `void` | `public` | None |
| 109 | `MetaRunProgressTracker` | `CreateResultContext` | `MetaRunResultBuildContext` | `public` | None |
| 125 | `MetaRunProgressTracker` | `RecordRecipe` | `void` | `private` | None |
| 133 | `MetaRunProgressTracker` | `GetThreatStageScore` | `int` | `private static` | None |

### `Assets/Scripts/Meta/RunResultPanel.cs`

- Functions detected: 3
- Judgment: UI type usage fits this UI/factory context.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `RunResultPanel` | `Render` | `void` | `public` | None |
| 17 | `RunResultPanel` | `Hide` | `void` | `public` | None |
| 22 | `RunResultPanel` | `EnsureView` | `void` | `private` | UI type dependency |

### `Assets/Scripts/Meta/RunResultPanelFactory.cs`

- Functions detected: 3
- Judgment: manual resolver use is acceptable only in composition roots/factories; UI type usage fits this UI/factory context; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `IRunResultPanelFactory` | `CreateDefaultPanel` | `RunResultPanel` | `-` | None |
| 15 | `RunResultPanelFactory` | `RunResultPanelFactory` | `ctor` | `public` | None |
| 25 | `RunResultPanelFactory` | `CreateDefaultPanel` | `RunResultPanel` | `public` | Manual resolver/injection, Runtime object creation, UI type dependency |

### `Assets/Scripts/Offense/OffenseExpeditionEvents.cs`

- Functions detected: 4
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 4 | `OffenseExpeditionStartedEvent` | `OffenseExpeditionStartedEvent` | `ctor` | `public` | None |
| 11 | `OffenseExpeditionStartedEvent` | `Trigger` | `void` | `public static` | None |
| 22 | `OffenseExpeditionCompletedEvent` | `OffenseExpeditionCompletedEvent` | `ctor` | `public` | None |
| 29 | `OffenseExpeditionCompletedEvent` | `Trigger` | `void` | `public static` | None |

### `Assets/Scripts/Offense/OffenseExpeditionModel.cs`

- Functions detected: 4
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 13 | `OffenseExpeditionMemberSnapshot` | `ToSummaryText` | `string` | `public` | None |
| 35 | `OffenseExpeditionResult` | `ToDetailText` | `string` | `public` | None |
| 92 | `OffenseExpeditionRun` | `OffenseExpeditionRun` | `ctor` | `public` | None |
| 117 | `OffenseExpeditionRun` | `Tick` | `void` | `public` | None |

### `Assets/Scripts/Offense/OffenseExpeditionService.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `OffenseExpeditionService` | `CanJoinExpedition` | `bool` | `public static` | None |
| 61 | `OffenseExpeditionService` | `CalculateMemberPower` | `float` | `public static` | None |
| 79 | `OffenseExpeditionService` | `CalculatePartyPower` | `float` | `public static` | None |
| 84 | `OffenseExpeditionService` | `ShouldSucceed` | `bool` | `public static` | None |
| 95 | `OffenseExpeditionService` | `Resolve` | `OffenseExpeditionResult` | `public static` | None |

### `Assets/Scripts/Offense/OffenseExpeditionSystem.cs`

- Functions detected: 21
- Judgment: non-UI code depends on UI types.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 18 | `OffenseExpeditionRuntime` | `Construct` | `void` | `public` | None |
| 38 | `OffenseExpeditionRuntime` | `Update` | `void` | `private` | None |
| 43 | `OffenseExpeditionRuntime` | `GetAvailableMemberActors` | `IReadOnlyList<CharacterActor>` | `public` | None |
| 48 | `OffenseExpeditionRuntime` | `TryStartExpedition` | `bool` | `public` | None |
| 105 | `OffenseExpeditionRuntime` | `Tick` | `void` | `public` | None |
| 118 | `OffenseExpeditionRuntime` | `CompleteExpeditionForDebug` | `bool` | `public` | None |
| 134 | `OffenseExpeditionRuntime` | `CompleteExpeditionAt` | `OffenseExpeditionResult` | `private` | None |
| 175 | `OffenseExpeditionRuntime` | `ShowExpeditionPanel` | `OffenseExpeditionPanel` | `public` | None |
| 180 | `OffenseExpeditionRuntime` | `ResolveMemberQuery` | `IOffenseExpeditionMemberQuery` | `private` | None |
| 186 | `OffenseExpeditionRuntime` | `ResolveWorldMapProvider` | `IOffenseWorldMapRuntimeProvider` | `private` | None |
| 192 | `OffenseExpeditionRuntime` | `ResolveRewardProvider` | `IOffenseRewardRuntimeProvider` | `private` | None |
| 198 | `OffenseExpeditionRuntime` | `ResolveMetaProgressionProvider` | `IMetaProgressionRuntimeProvider` | `private` | None |
| 204 | `OffenseExpeditionRuntime` | `ResolvePanelService` | `IOffensePanelService` | `private` | None |
| 223 | `OffenseExpeditionPanel` | `Bind` | `void` | `public` | None |
| 238 | `OffenseExpeditionPanel` | `Render` | `void` | `public` | None |
| 314 | `OffenseExpeditionPanel` | `Hide` | `void` | `public` | None |
| 319 | `OffenseExpeditionPanel` | `BuildMemberLabel` | `string` | `private static` | None |
| 333 | `OffenseExpeditionPanel` | `EnsureView` | `void` | `private` | UI type dependency |
| 344 | `OffenseExpeditionPanel` | `ClearButtons` | `void` | `private` | None |
| 357 | `OffenseExpeditionPanel` | `BindGeneratedView` | `void` | `internal` | None |
| 373 | `OffenseExpeditionPanel` | `RequireButtonFactory` | `IOffensePanelButtonFactory` | `private` | None |

### `Assets/Scripts/Offense/OffensePanelFactory.cs`

- Functions detected: 5
- Judgment: manual resolver use is acceptable only in composition roots/factories; UI type usage fits this UI/factory context.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IOffensePanelFactory` | `CreateWorldMapPanel` | `OffenseWorldMapPanel` | `-` | None |
| 8 | `IOffensePanelFactory` | `CreateExpeditionPanel` | `OffenseExpeditionPanel` | `-` | None |
| 15 | `OffensePanelFactory` | `OffensePanelFactory` | `ctor` | `public` | None |
| 25 | `OffensePanelFactory` | `CreateWorldMapPanel` | `OffenseWorldMapPanel` | `public` | Manual resolver/injection, UI type dependency |
| 71 | `OffensePanelFactory` | `CreateExpeditionPanel` | `OffenseExpeditionPanel` | `public` | Manual resolver/injection, UI type dependency |

### `Assets/Scripts/Offense/OffensePanelUiFactory.cs`

- Functions detected: 10
- Judgment: UI type usage fits this UI/factory context; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `OffensePanelUiFactory` | `CreateOverlayCanvas` | `GameObject` | `public static` | Runtime object creation, UI type dependency |
| 23 | `OffensePanelUiFactory` | `CreatePanel` | `GameObject` | `public static` | Runtime object creation, UI type dependency |
| 41 | `OffensePanelUiFactory` | `CreateVerticalRoot` | `GameObject` | `public static` | Runtime object creation, UI type dependency |
| 66 | `OffensePanelUiFactory` | `CreateText` | `GameObject` | `public static` | Runtime object creation, UI type dependency |
| 89 | `OffensePanelUiFactory` | `CreateButton` | `GameObject` | `public static` | Runtime object creation, UI type dependency |
| 118 | `IOffensePanelButtonFactory` | `CreateButton` | `GameObject` | `-` | UI type dependency |
| 119 | `IOffensePanelButtonFactory` | `Release` | `void` | `-` | None |
| 125 | `OffensePanelButtonFactory` | `OffensePanelButtonFactory` | `ctor` | `public` | None |
| 131 | `OffensePanelButtonFactory` | `CreateButton` | `GameObject` | `public` | None |
| 141 | `OffensePanelButtonFactory` | `Release` | `void` | `public` | None |

### `Assets/Scripts/Offense/OffenseRewardContextResolver.cs`

- Functions detected: 5
- Judgment: scene discovery should be isolated behind providers/build callbacks.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 11 | `OffenseRewardDebugContext` | `Clear` | `void` | `public` | None |
| 23 | `IOffenseRewardContextBuilder` | `Create` | `OffenseRewardContext` | `-` | None |
| 32 | `OffenseRewardContextBuilder` | `OffenseRewardContextBuilder` | `ctor` | `public` | None |
| 38 | `OffenseRewardContextBuilder` | `Create` | `OffenseRewardContext` | `public` | Scene query coupling |
| 71 | `OffenseRewardContextBuilder` | `ResolveWarehouses` | `IEnumerable<IWarehouseFacility>` | `private` | Scene query coupling |

### `Assets/Scripts/Offense/OffenseRewardEvents.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `OffenseRewardGrantedEvent` | `OffenseRewardGrantedEvent` | `ctor` | `public` | None |
| 18 | `OffenseRewardGrantedEvent` | `Trigger` | `void` | `public static` | None |

### `Assets/Scripts/Offense/OffenseRewardModel.cs`

- Functions detected: 10
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 13 | `OffenseRewardGrantResult` | `ToSummaryText` | `string` | `public` | None |
| 44 | `OffenseRewardState` | `Reset` | `void` | `public` | None |
| 57 | `OffenseRewardState` | `RecordMoney` | `void` | `public` | None |
| 62 | `OffenseRewardState` | `RecordStock` | `void` | `public` | None |
| 72 | `OffenseRewardState` | `RecordRareFacility` | `bool` | `public` | None |
| 77 | `OffenseRewardState` | `RecordBlueprint` | `bool` | `public` | None |
| 82 | `OffenseRewardState` | `RecordFactionWeakening` | `void` | `public` | None |
| 95 | `OffenseRewardState` | `RecordRecruitCandidates` | `void` | `public` | None |
| 100 | `OffenseRewardState` | `RecordPrisoners` | `void` | `public` | None |
| 105 | `OffenseRewardState` | `RecordSpecialMonsters` | `void` | `public` | None |

### `Assets/Scripts/Offense/OffenseRewardSelector.cs`

- Functions detected: 6
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `OffenseRewardSelector` | `OffenseRewardSelector` | `ctor` | `public` | None |
| 14 | `OffenseRewardSelector` | `ResolveStockCategory` | `StockCategory` | `public` | None |
| 35 | `OffenseRewardSelector` | `SelectRareFacility` | `BuildingSO` | `public` | None |
| 61 | `OffenseRewardSelector` | `SelectBlueprint` | `FacilityBlueprintSO` | `public` | None |
| 91 | `OffenseRewardSelector` | `IsHumanFactionWeakening` | `bool` | `public` | None |
| 106 | `OffenseRewardSelector` | `ContainsAny` | `bool` | `public` | None |

### `Assets/Scripts/Offense/OffenseRewardSystem.cs`

- Functions detected: 20
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 10 | `OffenseRewardGrantService` | `OffenseRewardGrantService` | `ctor` | `public` | None |
| 16 | `OffenseRewardGrantService` | `GrantRewards` | `IReadOnlyList<OffenseRewardGrantResult>` | `public` | None |
| 32 | `OffenseRewardGrantService` | `GrantReward` | `OffenseRewardGrantResult` | `private` | None |
| 49 | `OffenseRewardGrantService` | `GrantMoney` | `OffenseRewardGrantResult` | `private static` | None |
| 69 | `OffenseRewardGrantService` | `GrantStock` | `OffenseRewardGrantResult` | `private` | None |
| 96 | `OffenseRewardGrantService` | `GrantRareFacility` | `OffenseRewardGrantResult` | `private` | None |
| 127 | `OffenseRewardGrantService` | `GrantBlueprint` | `OffenseRewardGrantResult` | `private` | None |
| 154 | `OffenseRewardGrantService` | `GrantFactionWeakening` | `OffenseRewardGrantResult` | `private` | None |
| 164 | `OffenseRewardGrantService` | `GrantRecruitCandidate` | `OffenseRewardGrantResult` | `private static` | None |
| 173 | `OffenseRewardGrantService` | `GrantPrisoner` | `OffenseRewardGrantResult` | `private` | None |
| 188 | `OffenseRewardGrantService` | `Success` | `OffenseRewardGrantResult` | `private static` | None |
| 204 | `OffenseRewardGrantService` | `Fail` | `OffenseRewardGrantResult` | `private static` | None |
| 227 | `OffenseRewardRuntime` | `Construct` | `void` | `public` | None |
| 238 | `OffenseRewardRuntime` | `ApplyExpeditionRewards` | `IReadOnlyList<OffenseRewardGrantResult>` | `public` | None |
| 255 | `OffenseRewardRuntime` | `SetDebugContext` | `void` | `public` | None |
| 267 | `OffenseRewardRuntime` | `ClearDebugContext` | `void` | `public` | None |
| 272 | `OffenseRewardRuntime` | `ResetState` | `void` | `public` | None |
| 277 | `OffenseRewardRuntime` | `CreateContext` | `OffenseRewardContext` | `private` | None |
| 282 | `OffenseRewardRuntime` | `ResolveContextBuilder` | `IOffenseRewardContextBuilder` | `private` | None |
| 288 | `OffenseRewardRuntime` | `ResolveGrantService` | `IOffenseRewardGrantService` | `private` | None |

### `Assets/Scripts/Offense/OffenseRuntimeServices.cs`

- Functions detected: 21
- Judgment: scene discovery should be isolated behind providers/build callbacks.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IOffenseWorldMapRuntimeProvider` | `TryGetRuntime` | `bool` | `-` | None |
| 12 | `IOffenseRewardRuntimeProvider` | `TryGetRuntime` | `bool` | `-` | None |
| 17 | `IOffenseExpeditionMemberQuery` | `GetAvailableMemberActors` | `IReadOnlyList<CharacterActor>` | `-` | None |
| 28 | `IOffenseRewardSelector` | `ResolveStockCategory` | `StockCategory` | `-` | None |
| 29 | `IOffenseRewardSelector` | `SelectRareFacility` | `BuildingSO` | `-` | None |
| 32 | `IOffenseRewardSelector` | `SelectBlueprint` | `FacilityBlueprintSO` | `-` | None |
| 35 | `IOffenseRewardSelector` | `IsHumanFactionWeakening` | `bool` | `-` | None |
| 36 | `IOffenseRewardSelector` | `ContainsAny` | `bool` | `-` | None |
| 41 | `IOffenseRewardGrantService` | `GrantRewards` | `IReadOnlyList<OffenseRewardGrantResult>` | `-` | None |
| 48 | `IOffensePanelService` | `ShowWorldMap` | `OffenseWorldMapPanel` | `-` | None |
| 49 | `IOffensePanelService` | `ShowExpedition` | `OffenseExpeditionPanel` | `-` | None |
| 55 | `OffenseWorldMapRuntimeProvider` | `OffenseWorldMapRuntimeProvider` | `ctor` | `public` | None |
| 61 | `OffenseWorldMapRuntimeProvider` | `TryGetRuntime` | `bool` | `public` | Scene query coupling |
| 72 | `OffenseRewardRuntimeProvider` | `OffenseRewardRuntimeProvider` | `ctor` | `public` | None |
| 78 | `OffenseRewardRuntimeProvider` | `TryGetRuntime` | `bool` | `public` | Scene query coupling |
| 89 | `OffenseExpeditionMemberQuery` | `OffenseExpeditionMemberQuery` | `ctor` | `public` | None |
| 95 | `OffenseExpeditionMemberQuery` | `GetAvailableMemberActors` | `IReadOnlyList<CharacterActor>` | `public` | Scene query coupling |
| 108 | `DataCatalogOffenseRewardCatalog` | `DataCatalogOffenseRewardCatalog` | `ctor` | `public` | None |
| 134 | `OffensePanelService` | `OffensePanelService` | `ctor` | `public` | None |
| 150 | `OffensePanelService` | `ShowWorldMap` | `OffenseWorldMapPanel` | `public` | Scene query coupling |
| 163 | `OffensePanelService` | `ShowExpedition` | `OffenseExpeditionPanel` | `public` | Scene query coupling |

### `Assets/Scripts/Offense/OffenseTabSummaryQuery.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `OffenseTabSummary` | `OffenseTabSummary` | `ctor` | `public` | None |
| 41 | `IOffenseTabSummaryService` | `Capture` | `OffenseTabSummary` | `-` | None |
| 54 | `OffenseTabSummaryRuntimeSource` | `OffenseTabSummaryRuntimeSource` | `ctor` | `public` | None |
| 68 | `OffenseTabSummaryService` | `OffenseTabSummaryService` | `ctor` | `public` | None |
| 73 | `OffenseTabSummaryService` | `Capture` | `OffenseTabSummary` | `public` | None |

### `Assets/Scripts/Offense/OffenseWorldMapEvents.cs`

- Functions detected: 6
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `OffenseWorldMapChangedEvent` | `OffenseWorldMapChangedEvent` | `ctor` | `public` | None |
| 18 | `OffenseWorldMapChangedEvent` | `Trigger` | `void` | `public static` | None |
| 32 | `OffenseTargetSelectedEvent` | `OffenseTargetSelectedEvent` | `ctor` | `public` | None |
| 39 | `OffenseTargetSelectedEvent` | `Trigger` | `void` | `public static` | None |
| 52 | `OffenseReconUpgradedEvent` | `OffenseReconUpgradedEvent` | `ctor` | `public` | None |
| 61 | `OffenseReconUpgradedEvent` | `Trigger` | `void` | `public static` | None |

### `Assets/Scripts/Offense/OffenseWorldMapModel.cs`

- Functions detected: 10
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 31 | `OffenseRewardPreview` | `ToSummaryText` | `string` | `public` | None |
| 57 | `OffenseTargetDefinition` | `ToSnapshot` | `OffenseTargetSnapshot` | `public` | None |
| 93 | `OffenseTargetSnapshot` | `ToDetailText` | `string` | `public` | None |
| 128 | `OffenseTargetSnapshot` | `FormatDuration` | `string` | `private static` | None |
| 136 | `OffenseTargetSnapshot` | `GetKindName` | `string` | `private static` | None |
| 157 | `OffenseWorldMapState` | `Reset` | `void` | `public` | None |
| 164 | `OffenseWorldMapState` | `KnowTarget` | `bool` | `public` | None |
| 169 | `OffenseWorldMapState` | `AddKnownTarget` | `bool` | `public` | None |
| 174 | `OffenseWorldMapState` | `SetSelectedTarget` | `void` | `public` | None |
| 179 | `OffenseWorldMapState` | `TryUpgradeRecon` | `bool` | `public` | None |

### `Assets/Scripts/Offense/OffenseWorldMapService.cs`

- Functions detected: 8
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `OffenseWorldMapService` | `GetScanRange` | `float` | `public static` | None |
| 20 | `OffenseWorldMapService` | `CreateDefaultTargets` | `IReadOnlyList<OffenseTargetDefinition>` | `public static` | None |
| 89 | `OffenseWorldMapService` | `NormalizeTargets` | `IReadOnlyList<OffenseTargetDefinition>` | `public static` | None |
| 101 | `OffenseWorldMapService` | `RevealTargetsInRange` | `int` | `public static` | None |
| 123 | `OffenseWorldMapService` | `GetVisibleTargetSnapshots` | `IReadOnlyList<OffenseTargetSnapshot>` | `public static` | None |
| 139 | `OffenseWorldMapService` | `FindKnownTarget` | `OffenseTargetDefinition` | `public static` | None |
| 152 | `OffenseWorldMapService` | `CreateTarget` | `OffenseTargetDefinition` | `private static` | None |
| 179 | `OffenseWorldMapService` | `Reward` | `OffenseRewardPreview` | `private static` | None |

### `Assets/Scripts/Offense/OffenseWorldMapSystem.cs`

- Functions detected: 19
- Judgment: non-UI code depends on UI types.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 37 | `OffenseWorldMapRuntime` | `Construct` | `void` | `public` | None |
| 44 | `OffenseWorldMapRuntime` | `Awake` | `void` | `private` | None |
| 49 | `OffenseWorldMapRuntime` | `StartWorldMap` | `void` | `public` | None |
| 57 | `OffenseWorldMapRuntime` | `TryUpgradeRecon` | `bool` | `public` | None |
| 74 | `OffenseWorldMapRuntime` | `TrySelectTarget` | `bool` | `public` | None |
| 93 | `OffenseWorldMapRuntime` | `TryGetKnownTargetSnapshot` | `bool` | `public` | None |
| 107 | `OffenseWorldMapRuntime` | `ShowWorldMap` | `OffenseWorldMapPanel` | `public` | None |
| 113 | `OffenseWorldMapRuntime` | `SetPreciseIntelForDebug` | `void` | `public` | None |
| 119 | `OffenseWorldMapRuntime` | `SetTargetsForDebug` | `void` | `public` | None |
| 126 | `OffenseWorldMapRuntime` | `EnsureInitialized` | `void` | `private` | None |
| 137 | `OffenseWorldMapRuntime` | `RaiseChanged` | `void` | `private` | None |
| 147 | `OffenseWorldMapRuntime` | `ResolvePanelService` | `IOffensePanelService` | `private` | None |
| 163 | `OffenseWorldMapPanel` | `Bind` | `void` | `public` | None |
| 174 | `OffenseWorldMapPanel` | `Render` | `void` | `public` | None |
| 230 | `OffenseWorldMapPanel` | `Hide` | `void` | `public` | None |
| 235 | `OffenseWorldMapPanel` | `EnsureView` | `void` | `private` | UI type dependency |
| 246 | `OffenseWorldMapPanel` | `ClearButtons` | `void` | `private` | None |
| 256 | `OffenseWorldMapPanel` | `BindGeneratedView` | `void` | `internal` | None |
| 272 | `OffenseWorldMapPanel` | `RequireButtonFactory` | `IOffensePanelButtonFactory` | `private` | None |

### `Assets/Scripts/Operation/EventAlertCanvasProvider.cs`

- Functions detected: 3
- Judgment: non-UI code depends on UI types; object creation is a factory boundary candidate.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IEventAlertCanvasProvider` | `GetOrCreateCanvas` | `Canvas` | `-` | UI type dependency |
| 13 | `EventAlertCanvasProvider` | `EventAlertCanvasProvider` | `ctor` | `public` | None |
| 19 | `EventAlertCanvasProvider` | `GetOrCreateCanvas` | `Canvas` | `public` | Runtime object creation, UI type dependency |

### `Assets/Scripts/Operation/EventAlertChoicePresenter.cs`

- Functions detected: 3
- Judgment: non-UI code depends on UI types.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 10 | `EventAlertChoicePresenter` | `EventAlertChoicePresenter` | `ctor` | `public` | None |
| 16 | `EventAlertChoicePresenter` | `Rebuild` | `void` | `public` | UI type dependency |
| 40 | `EventAlertChoicePresenter` | `Clear` | `void` | `public` | UI type dependency |

### `Assets/Scripts/Operation/EventAlertEvents.cs`

- Functions detected: 4
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 4 | `EventAlertRequestedEvent` | `EventAlertRequestedEvent` | `ctor` | `public` | None |
| 11 | `EventAlertRequestedEvent` | `Trigger` | `void` | `public static` | None |
| 22 | `EventAlertLoggedEvent` | `EventAlertLoggedEvent` | `ctor` | `public` | None |
| 29 | `EventAlertLoggedEvent` | `Trigger` | `void` | `public static` | None |

### `Assets/Scripts/Operation/EventAlertLayout.cs`

- Functions detected: 7
- Judgment: non-UI code depends on UI types.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 18 | `EventAlertLayout` | `LayoutButtons` | `void` | `public static` | UI type dependency |
| 65 | `EventAlertLayout` | `ConfigureButtonViewport` | `void` | `public static` | None |
| 91 | `EventAlertLayout` | `GetAlertListHeight` | `float` | `private static` | UI type dependency |
| 104 | `EventAlertLayout` | `GetButtonViewportHeight` | `float` | `private static` | None |
| 114 | `EventAlertLayout` | `GetVisibleRowsForHeight` | `int` | `private static` | None |
| 129 | `EventAlertLayout` | `GetMaxVisibleRows` | `int` | `private static` | None |
| 136 | `EventAlertLayout` | `GetContentHeightForRows` | `float` | `private static` | None |

### `Assets/Scripts/Operation/EventAlertMergePolicy.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `EventAlertMergePolicy` | `FindMergeTarget` | `EventAlertRecord` | `public static` | None |
| 15 | `EventAlertMergePolicy` | `CanMerge` | `bool` | `private static` | None |

### `Assets/Scripts/Operation/EventAlertModel.cs`

- Functions detected: 7
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 17 | `EventAlertChoice` | `EventAlertChoice` | `ctor` | `public` | None |
| 33 | `EventAlertRequest` | `EventAlertRequest` | `ctor` | `public` | None |
| 47 | `EventAlertRequest` | `NormalizeChoices` | `IReadOnlyList<EventAlertChoice>` | `private static` | None |
| 67 | `EventAlertRecord` | `EventAlertRecord` | `ctor` | `public` | None |
| 78 | `EventAlertRecord` | `Increment` | `void` | `public` | None |
| 85 | `EventAlertRecord` | `ToDetailText` | `string` | `public` | None |
| 124 | `EventAlertRecord` | `GetImportanceName` | `string` | `private static` | None |

### `Assets/Scripts/Operation/EventAlertSelectionState.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 4 | `EventAlertSelectionState` | `Select` | `void` | `public` | None |
| 12 | `EventAlertSelectionState` | `ExecuteChoice` | `bool` | `public` | None |

### `Assets/Scripts/Operation/EventAlertService.cs`

- Functions detected: 4
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `EventAlertService` | `Raise` | `void` | `public static` | None |
| 14 | `EventAlertService` | `RaiseInvasionResult` | `void` | `public static` | None |
| 19 | `EventAlertService` | `RaiseStaffComplaint` | `void` | `public static` | None |
| 24 | `EventAlertService` | `RaiseBlueprintAcquired` | `void` | `public static` | None |

### `Assets/Scripts/Operation/EventAlertSystem.cs`

- Functions detected: 11
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 23 | `EventAlertRuntime` | `Construct` | `void` | `public` | None |
| 31 | `EventAlertRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 54 | `EventAlertRuntime` | `Open` | `void` | `public` | None |
| 65 | `EventAlertRuntime` | `CloseDetail` | `void` | `public` | None |
| 70 | `EventAlertRuntime` | `ExecuteChoice` | `bool` | `public` | None |
| 81 | `EventAlertRuntime` | `OnEnable` | `void` | `private` | None |
| 86 | `EventAlertRuntime` | `OnDisable` | `void` | `private` | None |
| 91 | `EventAlertRuntime` | `OnDestroy` | `void` | `private` | None |
| 96 | `EventAlertRuntime` | `CreateButton` | `void` | `private` | None |
| 101 | `EventAlertRuntime` | `UpdateButton` | `void` | `private` | None |
| 106 | `EventAlertRuntime` | `ResolveViewPresenter` | `IEventAlertViewPresenter` | `private` | None |

### `Assets/Scripts/Operation/EventAlertUiFactory.cs`

- Functions detected: 14
- Judgment: UI type usage fits this UI/factory context; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `EventAlertUiFactory` | `CreateAlertButton` | `Button` | `public static` | Runtime object creation, UI type dependency |
| 45 | `EventAlertUiFactory` | `UpdateAlertButton` | `void` | `public static` | UI type dependency |
| 59 | `EventAlertUiFactory` | `CreateChoiceButton` | `Button` | `public static` | Runtime object creation, UI type dependency |
| 100 | `EventAlertUiFactory` | `CreateText` | `TMP_Text` | `private static` | Runtime object creation, UI type dependency |
| 124 | `EventAlertUiFactory` | `GetImportanceColor` | `Color` | `private static` | None |
| 139 | `IEventAlertButtonFactory` | `CreateAlertButton` | `Button` | `-` | UI type dependency |
| 140 | `IEventAlertButtonFactory` | `UpdateAlertButton` | `void` | `-` | UI type dependency |
| 141 | `IEventAlertButtonFactory` | `CreateChoiceButton` | `Button` | `-` | UI type dependency |
| 142 | `IEventAlertButtonFactory` | `Release` | `void` | `-` | UI type dependency |
| 148 | `EventAlertButtonFactory` | `EventAlertButtonFactory` | `ctor` | `public` | None |
| 154 | `EventAlertButtonFactory` | `CreateAlertButton` | `Button` | `public` | None |
| 163 | `EventAlertButtonFactory` | `UpdateAlertButton` | `void` | `public` | None |
| 168 | `EventAlertButtonFactory` | `CreateChoiceButton` | `Button` | `public` | None |
| 182 | `EventAlertButtonFactory` | `Release` | `void` | `public` | None |

### `Assets/Scripts/Operation/EventAlertViewPresenter.cs`

- Functions detected: 19
- Judgment: non-UI code depends on UI types.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `EventAlertViewPresenterContext` | `EventAlertViewPresenterContext` | `ctor` | `public` | None |
| 36 | `IEventAlertViewPresenter` | `EnsureRuntimeUI` | `void` | `-` | None |
| 37 | `IEventAlertViewPresenter` | `DestroyRuntimeUI` | `void` | `-` | None |
| 38 | `IEventAlertViewPresenter` | `CreateButton` | `void` | `-` | None |
| 39 | `IEventAlertViewPresenter` | `UpdateButton` | `void` | `-` | None |
| 40 | `IEventAlertViewPresenter` | `OpenDetail` | `void` | `-` | None |
| 41 | `IEventAlertViewPresenter` | `CloseDetail` | `void` | `-` | None |
| 46 | `IEventAlertViewPresenterFactory` | `Create` | `IEventAlertViewPresenter` | `-` | None |
| 54 | `EventAlertViewPresenterFactory` | `EventAlertViewPresenterFactory` | `ctor` | `public` | None |
| 67 | `EventAlertViewPresenterFactory` | `Create` | `IEventAlertViewPresenter` | `public` | None |
| 92 | `EventAlertViewPresenter` | `EventAlertViewPresenter` | `ctor` | `public` | None |
| 118 | `EventAlertViewPresenter` | `EnsureRuntimeUI` | `void` | `public` | UI type dependency |
| 164 | `EventAlertViewPresenter` | `DestroyRuntimeUI` | `void` | `public` | None |
| 183 | `EventAlertViewPresenter` | `CreateButton` | `void` | `public` | UI type dependency |
| 204 | `EventAlertViewPresenter` | `UpdateButton` | `void` | `public` | UI type dependency |
| 214 | `EventAlertViewPresenter` | `OpenDetail` | `void` | `public` | None |
| 227 | `EventAlertViewPresenter` | `CloseDetail` | `void` | `public` | None |
| 235 | `EventAlertViewPresenter` | `LayoutButtons` | `void` | `private` | None |
| 240 | `EventAlertViewPresenter` | `ClearButtons` | `void` | `private` | UI type dependency |

### `Assets/Scripts/Operation/EventAlertViewUiFactory.cs`

- Functions detected: 12
- Judgment: UI type usage fits this UI/factory context; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 10 | `IEventAlertViewUiFactory` | `CreateRuntimeRoot` | `GameObject` | `-` | UI type dependency |
| 11 | `IEventAlertViewUiFactory` | `CreateButtonRoot` | `void` | `-` | UI type dependency |
| 20 | `IEventAlertViewUiFactory` | `BindExistingButtonRootReferences` | `void` | `-` | UI type dependency |
| 26 | `IEventAlertViewUiFactory` | `IsButtonRootReady` | `bool` | `-` | UI type dependency |
| 32 | `IEventAlertViewUiFactory` | `CreateDetailPanel` | `GameObject` | `-` | UI type dependency |
| 39 | `EventAlertViewUiFactory` | `EventAlertViewUiFactory` | `ctor` | `public` | None |
| 45 | `EventAlertViewUiFactory` | `CreateRuntimeRoot` | `GameObject` | `public` | Runtime object creation, UI type dependency |
| 58 | `EventAlertViewUiFactory` | `CreateButtonRoot` | `void` | `public` | Runtime object creation, UI type dependency |
| 114 | `EventAlertViewUiFactory` | `BindExistingButtonRootReferences` | `void` | `public` | UI type dependency |
| 135 | `EventAlertViewUiFactory` | `IsButtonRootReady` | `bool` | `public` | None |
| 148 | `EventAlertViewUiFactory` | `CreateDetailPanel` | `GameObject` | `public` | Runtime object creation, UI type dependency |
| 198 | `EventAlertViewUiFactory` | `CreateText` | `TMP_Text` | `private` | Runtime object creation, UI type dependency |

### `Assets/Scripts/Operation/OperatingDayReportAlertBridge.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `OperatingDayReportAlertBridge` | `OnTriggerEvent` | `void` | `public` | None |
| 26 | `OperatingDayReportAlertBridge` | `OnEnable` | `void` | `private` | None |
| 31 | `OperatingDayReportAlertBridge` | `OnDisable` | `void` | `private` | None |

### `Assets/Scripts/Operation/OperatingDaySettlement.cs`

- Functions detected: 42
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 73 | `OperatingDayReport` | `ToDetailText` | `string` | `public` | None |
| 104 | `OperatingDayReport` | `FormatList` | `string` | `private static` | None |
| 119 | `OperatingDayReport` | `TextOrDefault` | `string` | `private static` | None |
| 124 | `OperatingDayReport` | `GetStockCategoryName` | `string` | `private static` | None |
| 141 | `OperatingDayStartedEvent` | `OperatingDayStartedEvent` | `ctor` | `public` | None |
| 148 | `OperatingDayStartedEvent` | `Trigger` | `void` | `public static` | None |
| 159 | `OperatingDayEndedEvent` | `OperatingDayEndedEvent` | `ctor` | `public` | None |
| 166 | `OperatingDayEndedEvent` | `Trigger` | `void` | `public static` | None |
| 177 | `OperatingDayReportEvent` | `OperatingDayReportEvent` | `ctor` | `public` | None |
| 184 | `OperatingDayReportEvent` | `Trigger` | `void` | `public static` | None |
| 196 | `FacilityVisitEvent` | `FacilityVisitEvent` | `ctor` | `public` | None |
| 204 | `FacilityVisitEvent` | `Trigger` | `void` | `public static` | None |
| 218 | `FacilityRevenueEvent` | `FacilityRevenueEvent` | `ctor` | `public` | None |
| 227 | `FacilityRevenueEvent` | `Trigger` | `void` | `public static` | None |
| 243 | `FacilityStockConsumedEvent` | `FacilityStockConsumedEvent` | `ctor` | `public` | None |
| 253 | `FacilityStockConsumedEvent` | `Trigger` | `void` | `public static` | None |
| 276 | `FacilityCrimeEvent` | `FacilityCrimeEvent` | `ctor` | `public` | None |
| 292 | `FacilityCrimeEvent` | `Trigger` | `void` | `public static` | None |
| 315 | `FacilityRestockEvent` | `FacilityRestockEvent` | `ctor` | `public` | None |
| 325 | `FacilityRestockEvent` | `Trigger` | `void` | `public static` | None |
| 364 | `OperatingDaySettlementRuntime` | `Construct` | `void` | `public` | None |
| 378 | `OperatingDaySettlementRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 384 | `OperatingDaySettlementRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 391 | `OperatingDaySettlementRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 411 | `OperatingDaySettlementRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 423 | `OperatingDaySettlementRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 433 | `OperatingDaySettlementRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 441 | `OperatingDaySettlementRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 449 | `OperatingDaySettlementRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 454 | `OperatingDaySettlementRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 464 | `OperatingDaySettlementRuntime` | `OnEnable` | `void` | `private` | None |
| 477 | `OperatingDaySettlementRuntime` | `OnDisable` | `void` | `private` | None |
| 490 | `OperatingDaySettlementRuntime` | `BuildReport` | `OperatingDayReport` | `private` | None |
| 531 | `OperatingDaySettlementRuntime` | `FillBuildingSnapshot` | `void` | `private` | None |
| 564 | `OperatingDaySettlementRuntime` | `FillStaffSnapshot` | `void` | `private static` | None |
| 588 | `OperatingDaySettlementRuntime` | `ResetLedger` | `void` | `private` | None |
| 602 | `OperatingDaySettlementRuntime` | `GetFacilityName` | `string` | `private static` | None |
| 613 | `OperatingDaySettlementRuntime` | `GetStat` | `float` | `private static` | None |
| 624 | `OperatingDaySettlementRuntime` | `IsStaffCharacter` | `bool` | `private static` | None |
| 632 | `OperatingDaySettlementRuntime` | `RequireSceneQuery` | `IDungeonSceneComponentQuery` | `private` | None |
| 642 | `OperatingDaySettlementRuntime` | `RequireFacilityShopCatalog` | `IFacilityShopCatalog` | `private` | None |
| 652 | `OperatingDaySettlementRuntime` | `RequireRunVariableReader` | `IRunVariableRuntimeReader` | `private` | None |

### `Assets/Scripts/Operation/OperationTabSummaryQuery.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `OperationTabSummary` | `OperationTabSummary` | `ctor` | `public` | None |
| 41 | `IOperationTabSummaryService` | `Capture` | `OperationTabSummary` | `-` | None |
| 55 | `OperationTabSummaryRuntimeSource` | `OperationTabSummaryRuntimeSource` | `ctor` | `public` | None |
| 70 | `OperationTabSummaryService` | `OperationTabSummaryService` | `ctor` | `public` | None |
| 75 | `OperationTabSummaryService` | `Capture` | `OperationTabSummary` | `public` | None |

### `Assets/Scripts/Recruitment/RegularCustomerSystem.cs`

- Functions detected: 36
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 32 | `RegularCustomerRules` | `CreateDefault` | `RegularCustomerRules` | `public static` | None |
| 48 | `RegularCustomerSnapshot` | `ToSummaryText` | `string` | `public` | None |
| 58 | `RegularCustomerRecord` | `RegularCustomerRecord` | `ctor` | `public` | None |
| 90 | `RegularCustomerRecord` | `RecordVisit` | `void` | `public` | None |
| 114 | `RegularCustomerRecord` | `MarkRecruited` | `bool` | `public` | None |
| 126 | `RegularCustomerRecord` | `ToSnapshot` | `RegularCustomerSnapshot` | `public` | None |
| 144 | `RegularCustomerVisitResult` | `RegularCustomerVisitResult` | `ctor` | `public` | None |
| 167 | `RegularCustomerRecruitResult` | `RegularCustomerRecruitResult` | `ctor` | `public` | None |
| 189 | `RegularCustomerState` | `RecordVisit` | `RegularCustomerVisitResult` | `public` | None |
| 213 | `RegularCustomerState` | `TryGetRecord` | `bool` | `public` | None |
| 218 | `RegularCustomerState` | `IsRecruited` | `bool` | `public` | None |
| 223 | `RegularCustomerState` | `TryRecruit` | `bool` | `public` | None |
| 254 | `RegularCustomerState` | `GetOrCreate` | `RegularCustomerRecord` | `private` | None |
| 270 | `RegularCustomerUpdatedEvent` | `RegularCustomerUpdatedEvent` | `ctor` | `public` | None |
| 277 | `RegularCustomerUpdatedEvent` | `Trigger` | `void` | `public static` | None |
| 288 | `RegularCustomerBecameRegularEvent` | `RegularCustomerBecameRegularEvent` | `ctor` | `public` | None |
| 295 | `RegularCustomerBecameRegularEvent` | `Trigger` | `void` | `public static` | None |
| 306 | `RecruitCandidateDiscoveredEvent` | `RecruitCandidateDiscoveredEvent` | `ctor` | `public` | None |
| 313 | `RecruitCandidateDiscoveredEvent` | `Trigger` | `void` | `public static` | None |
| 324 | `CustomerRecruitedEvent` | `CustomerRecruitedEvent` | `ctor` | `public` | None |
| 331 | `CustomerRecruitedEvent` | `Trigger` | `void` | `public static` | None |
| 341 | `RegularCustomerService` | `IsTrackableCustomer` | `bool` | `public static` | None |
| 349 | `RegularCustomerService` | `GetCustomerId` | `int` | `public static` | None |
| 360 | `RegularCustomerService` | `GetCustomerDisplayName` | `string` | `public static` | None |
| 376 | `RegularCustomerService` | `GetCustomerSpeciesTag` | `string` | `public static` | None |
| 387 | `RegularCustomerService` | `GetSatisfaction` | `float` | `public static` | None |
| 400 | `RegularCustomerService` | `GetCustomerData` | `CharacterSO` | `public static` | None |
| 405 | `RegularCustomerService` | `GetIdentity` | `CharacterIdentity` | `private static` | None |
| 410 | `RegularCustomerService` | `MeetsRegularCondition` | `bool` | `public static` | None |
| 418 | `RegularCustomerService` | `MeetsRecruitCandidateCondition` | `bool` | `public static` | None |
| 427 | `RegularCustomerService` | `CanSpawnAsCustomer` | `bool` | `public static` | None |
| 442 | `RegularCustomerService` | `FormatCapabilities` | `string` | `public static` | None |
| 461 | `RegularCustomerRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 494 | `RegularCustomerRuntime` | `TryRecruit` | `bool` | `public` | None |
| 511 | `RegularCustomerRuntime` | `OnEnable` | `void` | `private` | None |
| 516 | `RegularCustomerRuntime` | `OnDisable` | `void` | `private` | None |

### `Assets/Scripts/Research/BlueprintResearchSystem.cs`

- Functions detected: 29
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 12 | `BlueprintResearchTask` | `BlueprintResearchTask` | `ctor` | `public` | None |
| 24 | `BlueprintResearchTask` | `AddProgress` | `float` | `public` | None |
| 49 | `BlueprintResearchState` | `EnqueueBlueprint` | `bool` | `public` | None |
| 65 | `BlueprintResearchState` | `TryGetActiveTask` | `bool` | `public` | None |
| 71 | `BlueprintResearchState` | `IsCompleted` | `bool` | `public` | None |
| 76 | `BlueprintResearchState` | `MarkCompleted` | `void` | `public` | None |
| 86 | `BlueprintResearchState` | `UnlockRecipe` | `bool` | `public` | None |
| 95 | `BlueprintResearchUnlockResult` | `BlueprintResearchUnlockResult` | `ctor` | `public` | None |
| 115 | `BlueprintResearchWorkResult` | `BlueprintResearchWorkResult` | `ctor` | `public` | None |
| 146 | `BlueprintResearchQueuedEvent` | `BlueprintResearchQueuedEvent` | `ctor` | `public` | None |
| 153 | `BlueprintResearchQueuedEvent` | `Trigger` | `void` | `public static` | None |
| 164 | `BlueprintResearchProgressEvent` | `BlueprintResearchProgressEvent` | `ctor` | `public` | None |
| 171 | `BlueprintResearchProgressEvent` | `Trigger` | `void` | `public static` | None |
| 183 | `BlueprintResearchCompletedEvent` | `BlueprintResearchCompletedEvent` | `ctor` | `public` | None |
| 191 | `BlueprintResearchCompletedEvent` | `Trigger` | `void` | `public static` | None |
| 203 | `BlueprintResearchService` | `CalculateResearchWork` | `float` | `public static` | None |
| 212 | `BlueprintResearchService` | `GetFacilityResearchMultiplier` | `float` | `public static` | None |
| 238 | `BlueprintResearchService` | `ApplyCompletion` | `BlueprintResearchUnlockResult` | `public static` | None |
| 313 | `BlueprintResearchRuntime` | `Construct` | `void` | `public` | None |
| 324 | `BlueprintResearchRuntime` | `EnqueueBlueprint` | `bool` | `public` | None |
| 340 | `BlueprintResearchRuntime` | `ApplyResearchWork` | `BlueprintResearchWorkResult` | `public` | None |
| 373 | `BlueprintResearchRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 383 | `BlueprintResearchRuntime` | `CompleteTask` | `void` | `private` | None |
| 402 | `BlueprintResearchRuntime` | `FormatUnlockResult` | `string` | `private static` | None |
| 416 | `BlueprintResearchRuntime` | `AddLines` | `void` | `private static` | None |
| 426 | `BlueprintResearchRuntime` | `ResolveShopUnlockStateService` | `IFacilityShopUnlockStateService` | `private` | None |
| 432 | `BlueprintResearchRuntime` | `ResolveFacilityShopCatalog` | `IFacilityShopCatalog` | `private` | None |
| 438 | `BlueprintResearchRuntime` | `OnEnable` | `void` | `private` | None |
| 443 | `BlueprintResearchRuntime` | `OnDisable` | `void` | `private` | None |

### `Assets/Scripts/Research/ResearchCraftingSummaryQuery.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `ResearchCraftingSummary` | `ResearchCraftingSummary` | `ctor` | `public` | None |
| 34 | `IResearchCraftingSummaryService` | `Capture` | `ResearchCraftingSummary` | `-` | None |
| 46 | `ResearchCraftingSummaryRuntimeSource` | `ResearchCraftingSummaryRuntimeSource` | `ctor` | `public` | None |
| 59 | `ResearchCraftingSummaryService` | `ResearchCraftingSummaryService` | `ctor` | `public` | None |
| 64 | `ResearchCraftingSummaryService` | `Capture` | `ResearchCraftingSummary` | `public` | None |

### `Assets/Scripts/Rooms/RoomDetector.cs`

- Functions detected: 18
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 18 | `RoomDetector` | `Build` | `RoomLayout` | `public static` | None |
| 62 | `RoomDetector` | `IsDoor` | `bool` | `internal static` | None |
| 79 | `RoomDetector` | `IsWall` | `bool` | `internal static` | None |
| 86 | `RoomDetector` | `IsInteriorCell` | `bool` | `private static` | None |
| 94 | `RoomDetector` | `CollectConnectedInteriorCells` | `List<Vector2Int>` | `private static` | None |
| 126 | `RoomDetector` | `AnalyzeBoundary` | `RoomBoundaryInfo` | `private static` | None |
| 184 | `RoomDetector` | `CollectFurniture` | `List<BuildableObject>` | `private static` | None |
| 210 | `RoomDetector` | `AddSelfContainedFacilityRooms` | `void` | `private static` | None |
| 242 | `RoomDetector` | `IsSelfContainedFacilityRoom` | `bool` | `private static` | None |
| 253 | `RoomDetector` | `GetValidBuildCells` | `List<Vector2Int>` | `private static` | None |
| 283 | `RoomBoundaryInfo` | `AddDoor` | `void` | `public` | None |
| 291 | `RoomBoundaryInfo` | `AddWall` | `void` | `public` | None |
| 309 | `RoomOccupancySnapshot` | `Build` | `RoomOccupancySnapshot` | `public static` | None |
| 352 | `RoomOccupancySnapshot` | `HasDoor` | `bool` | `public` | None |
| 357 | `RoomOccupancySnapshot` | `HasWall` | `bool` | `public` | None |
| 362 | `RoomOccupancySnapshot` | `TryGetDoor` | `bool` | `public` | None |
| 367 | `RoomOccupancySnapshot` | `TryGetWall` | `bool` | `public` | None |
| 372 | `RoomOccupancySnapshot` | `GetBuildings` | `IReadOnlyList<BuildableObject>` | `public` | None |

### `Assets/Scripts/Rooms/RoomFacilityPolicy.cs`

- Functions detected: 5
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IRoomFacilityPolicy` | `IsFacilityRoleAvailable` | `bool` | `-` | None |
| 10 | `IRoomFacilityPolicy` | `GetRoomUtilityScore` | `float` | `-` | None |
| 17 | `RoomFacilityPolicyService` | `RoomFacilityPolicyService` | `ctor` | `public` | None |
| 23 | `RoomFacilityPolicyService` | `IsFacilityRoleAvailable` | `bool` | `public` | None |
| 66 | `RoomFacilityPolicyService` | `GetRoomUtilityScore` | `float` | `public` | None |

### `Assets/Scripts/Rooms/RoomInstance.cs`

- Functions detected: 7
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `RoomInstance` | `RoomInstance` | `ctor` | `public` | None |
| 58 | `RoomInstance` | `ContainsCell` | `bool` | `public` | None |
| 63 | `RoomInstance` | `ContainsPart` | `bool` | `public` | None |
| 68 | `RoomInstance` | `SupportsFacilityRole` | `bool` | `public` | None |
| 75 | `RoomInstance` | `GetQualityScore` | `float` | `public` | None |
| 88 | `RoomInstance` | `BuildFacilityRoles` | `FacilityRole` | `private static` | None |
| 107 | `RoomInstance` | `CalculateBounds` | `RectInt` | `private static` | None |

### `Assets/Scripts/Rooms/RoomLayout.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `RoomLayout` | `RoomLayout` | `ctor` | `public` | None |
| 38 | `RoomLayout` | `TryGetRoom` | `bool` | `public` | None |
| 43 | `RoomLayout` | `TryGetRoom` | `bool` | `public` | None |

### `Assets/Scripts/Rooms/RoomRegistry.cs`

- Functions detected: 6
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IRoomLayoutCache` | `GetLayout` | `RoomLayout` | `-` | None |
| 7 | `IRoomLayoutCache` | `TryGetRoom` | `bool` | `-` | None |
| 8 | `IRoomLayoutCache` | `Clear` | `void` | `-` | None |
| 21 | `RoomLayoutCache` | `GetLayout` | `RoomLayout` | `public` | None |
| 43 | `RoomLayoutCache` | `TryGetRoom` | `bool` | `public` | None |
| 54 | `RoomLayoutCache` | `Clear` | `void` | `public` | None |

### `Assets/Scripts/Rooms/RoomRole.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 20 | `RoomRoleUtility` | `FromFacilityRoles` | `RoomRole` | `public static` | None |
| 34 | `RoomRoleUtility` | `ToFacilityRoles` | `FacilityRole` | `public static` | None |

### `Assets/Scripts/Run/RunStartVariableSelector.cs`

- Functions detected: 4
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IRunStartVariableSelector` | `Create` | `RunStartVariableSnapshot` | `-` | None |
| 17 | `RunStartVariableSelector` | `RunStartVariableSelector` | `ctor` | `public` | None |
| 27 | `RunStartVariableSelector` | `Create` | `RunStartVariableSnapshot` | `public` | None |
| 75 | `RunStartVariableSelector` | `ResolveLayoutId` | `string` | `private static` | None |

### `Assets/Scripts/Run/RunVariableCatalog.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `RunVariableCatalog` | `Get` | `RunVariableDefinition` | `public static` | None |
| 14 | `RunVariableCatalog` | `GetByCategory` | `IReadOnlyList<RunVariableDefinition>` | `public static` | None |

### `Assets/Scripts/Run/RunVariableEffects.cs`

- Functions detected: 7
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `RunVariableEffects` | `GetGuestDemandMultiplier` | `float` | `public static` | None |
| 20 | `RunVariableEffects` | `GetStockCostMultiplier` | `float` | `public static` | None |
| 34 | `RunVariableEffects` | `GetFacilityShopCostMultiplier` | `float` | `public static` | None |
| 55 | `RunVariableEffects` | `GetBlueprintCostMultiplier` | `float` | `public static` | None |
| 67 | `RunVariableEffects` | `GetThreatRiseMultiplier` | `float` | `public static` | None |
| 83 | `RunVariableEffects` | `GetWarningThresholdMultiplier` | `float` | `public static` | None |
| 95 | `RunVariableEffects` | `ApplyInvasionSettings` | `InvasionIntruderSettings` | `public static` | None |

### `Assets/Scripts/Run/RunVariableEvents.cs`

- Functions detected: 8
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 4 | `RunStartVariablesSelectedEvent` | `RunStartVariablesSelectedEvent` | `ctor` | `public` | None |
| 11 | `RunStartVariablesSelectedEvent` | `Trigger` | `void` | `public static` | None |
| 22 | `RunVariableActivatedEvent` | `RunVariableActivatedEvent` | `ctor` | `public` | None |
| 29 | `RunVariableActivatedEvent` | `Trigger` | `void` | `public static` | None |
| 40 | `RunVariableExpiredEvent` | `RunVariableExpiredEvent` | `ctor` | `public` | None |
| 47 | `RunVariableExpiredEvent` | `Trigger` | `void` | `public static` | None |
| 58 | `InvasionVariableSelectedEvent` | `InvasionVariableSelectedEvent` | `ctor` | `public` | None |
| 65 | `InvasionVariableSelectedEvent` | `Trigger` | `void` | `public static` | None |

### `Assets/Scripts/Run/RunVariableModel.cs`

- Functions detected: 12
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 43 | `RunStartVariableSnapshot` | `ToSummaryText` | `string` | `public` | None |
| 57 | `RunStartVariableSnapshot` | `FormatIds` | `string` | `private static` | None |
| 63 | `RunStartVariableSnapshot` | `FormatStrings` | `string` | `private static` | None |
| 71 | `RunStartVariableSnapshot` | `TextOrDefault` | `string` | `private static` | None |
| 100 | `RunVariableDefinition` | `ToDetailText` | `string` | `public` | None |
| 109 | `ActiveRunVariable` | `ActiveRunVariable` | `ctor` | `public` | None |
| 120 | `ActiveRunVariable` | `AdvanceDay` | `void` | `public` | None |
| 135 | `RunVariableState` | `SetStartVariables` | `void` | `public` | None |
| 140 | `RunVariableState` | `ActivateOperationVariable` | `ActiveRunVariable` | `public` | None |
| 153 | `RunVariableState` | `AdvanceOperationVariables` | `IReadOnlyList<ActiveRunVariable>` | `public` | None |
| 169 | `RunVariableState` | `SetInvasionVariable` | `void` | `public` | None |
| 176 | `RunVariableState` | `ClearInvasionVariable` | `void` | `public` | None |

### `Assets/Scripts/Run/RunVariableSystem.cs`

- Functions detected: 27
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 24 | `RunVariableRuntime` | `Construct` | `void` | `public` | None |
| 38 | `RunVariableRuntime` | `Awake` | `void` | `private` | None |
| 48 | `RunVariableRuntime` | `StartRun` | `void` | `public` | None |
| 66 | `RunVariableRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 77 | `RunVariableRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 86 | `RunVariableRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 92 | `RunVariableRuntime` | `OnTriggerEvent` | `void` | `public` | None |
| 97 | `RunVariableRuntime` | `ActivateOperationVariable` | `ActiveRunVariable` | `public` | None |
| 119 | `RunVariableRuntime` | `SelectInvasionVariable` | `RunVariableDefinition` | `public` | None |
| 141 | `RunVariableRuntime` | `GetGuestDemandMultiplier` | `float` | `public` | None |
| 146 | `RunVariableRuntime` | `GetStockCostMultiplier` | `float` | `public` | None |
| 151 | `RunVariableRuntime` | `GetFacilityShopCostMultiplier` | `float` | `public` | None |
| 156 | `RunVariableRuntime` | `GetBlueprintCostMultiplier` | `float` | `public` | None |
| 161 | `RunVariableRuntime` | `GetThreatRiseMultiplier` | `float` | `public` | None |
| 166 | `RunVariableRuntime` | `GetWarningThresholdMultiplier` | `float` | `public` | None |
| 171 | `RunVariableRuntime` | `ApplyInvasionSettings` | `InvasionIntruderSettings` | `public` | None |
| 176 | `RunVariableRuntime` | `RollOperationVariable` | `void` | `private` | None |
| 188 | `RunVariableRuntime` | `SelectRandomInvasionVariable` | `void` | `private` | None |
| 201 | `RunVariableRuntime` | `EnsureRunStarted` | `void` | `private` | None |
| 213 | `RunVariableRuntime` | `EnsureRandom` | `void` | `private` | None |
| 221 | `RunVariableRuntime` | `ResolveDifficulty` | `InvasionThreatDifficulty` | `private` | None |
| 232 | `RunVariableRuntime` | `ResolveSelectedOwnerData` | `CharacterSO` | `private` | None |
| 237 | `RunVariableRuntime` | `ResolveOwnerRunDataProvider` | `IOwnerRunDataProvider` | `private` | None |
| 243 | `RunVariableRuntime` | `ResolveInvasionThreatRuntimeProvider` | `IInvasionThreatRuntimeProvider` | `private` | None |
| 249 | `RunVariableRuntime` | `ResolveRunStartVariableSelector` | `IRunStartVariableSelector` | `private` | None |
| 255 | `RunVariableRuntime` | `OnEnable` | `void` | `private` | None |
| 263 | `RunVariableRuntime` | `OnDisable` | `void` | `private` | None |

### `Assets/Scripts/Synthesis/FacilitySynthesisRecipeSO.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/Synthesis/FacilitySynthesisSystem.cs`

- Functions detected: 26
- Judgment: manual resolver use should move to a composition root or factory.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 16 | `FacilitySynthesisRecipeSnapshot` | `ToSummaryText` | `string` | `public` | None |
| 29 | `FacilitySynthesisResult` | `FacilitySynthesisResult` | `ctor` | `public` | None |
| 53 | `FacilitySynthesisCompletedEvent` | `FacilitySynthesisCompletedEvent` | `ctor` | `public` | None |
| 60 | `FacilitySynthesisCompletedEvent` | `Trigger` | `void` | `public static` | None |
| 71 | `FacilitySynthesisSelectionChangedEvent` | `FacilitySynthesisSelectionChangedEvent` | `ctor` | `public` | None |
| 78 | `FacilitySynthesisSelectionChangedEvent` | `Trigger` | `void` | `public static` | None |
| 88 | `FacilitySynthesisService` | `IsRecipeVisible` | `bool` | `public static` | None |
| 114 | `FacilitySynthesisService` | `ToSnapshot` | `FacilitySynthesisRecipeSnapshot` | `public static` | None |
| 139 | `FacilitySynthesisService` | `MatchesMaterials` | `bool` | `public static` | None |
| 156 | `FacilitySynthesisService` | `CalculateInheritedLevel` | `int` | `public static` | None |
| 183 | `FacilitySynthesisRuntime` | `ConstructFacilitySynthesisRuntime` | `void` | `public` | None |
| 216 | `FacilitySynthesisRuntime` | `ToggleMaterialSelection` | `void` | `public` | None |
| 235 | `FacilitySynthesisRuntime` | `ClearSelection` | `void` | `public` | None |
| 241 | `FacilitySynthesisRuntime` | `TrySynthesizeSelected` | `bool` | `public` | None |
| 252 | `FacilitySynthesisRuntime` | `TrySynthesizeSelected` | `bool` | `public` | None |
| 258 | `FacilitySynthesisRuntime` | `ToSnapshot` | `FacilitySynthesisRecipeSnapshot` | `public` | None |
| 263 | `FacilitySynthesisRuntime` | `TrySynthesize` | `bool` | `public` | None |
| 323 | `FacilitySynthesisRuntime` | `Validate` | `bool` | `private` | None |
| 387 | `FacilitySynthesisRuntime` | `CanPlaceResultOverMaterials` | `bool` | `private static` | None |
| 411 | `FacilitySynthesisRuntime` | `RemoveMaterialFromGrid` | `void` | `private` | None |
| 426 | `FacilitySynthesisRuntime` | `ResolveResearchStateService` | `IBlueprintResearchStateService` | `private` | None |
| 432 | `FacilitySynthesisRuntime` | `ResolveGridTextureProvider` | `IGridTextureProvider` | `private` | None |
| 438 | `FacilitySynthesisRuntime` | `InjectCreatedBuilding` | `void` | `private` | Manual resolver/injection |
| 448 | `FacilitySynthesisRuntime` | `ResolveObjectResolver` | `IObjectResolver` | `private` | None |
| 454 | `FacilitySynthesisRuntime` | `ResolveGridBuildingObjectFactory` | `IGridBuildingObjectFactory` | `private` | None |
| 460 | `FacilitySynthesisRuntime` | `ResolveRecipeQuery` | `IFacilitySynthesisRecipeQuery` | `private` | None |

### `Assets/Scripts/Synthesis/UI/FacilitySynthesisPanel.cs`

- Functions detected: 11
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 18 | `FacilitySynthesisPanel` | `Construct` | `void` | `public` | None |
| 25 | `FacilitySynthesisPanel` | `Bind` | `void` | `public` | None |
| 31 | `FacilitySynthesisPanel` | `BindGeneratedView` | `void` | `internal` | None |
| 38 | `FacilitySynthesisPanel` | `Refresh` | `void` | `public` | None |
| 80 | `FacilitySynthesisPanel` | `OnTriggerEvent` | `void` | `public` | None |
| 85 | `FacilitySynthesisPanel` | `OnTriggerEvent` | `void` | `public` | None |
| 90 | `FacilitySynthesisPanel` | `OnTriggerEvent` | `void` | `public` | None |
| 95 | `FacilitySynthesisPanel` | `ApplyText` | `void` | `private` | None |
| 103 | `FacilitySynthesisPanel` | `ResolveRuntime` | `FacilitySynthesisRuntime` | `private` | None |
| 112 | `FacilitySynthesisPanel` | `OnEnable` | `void` | `private` | None |
| 119 | `FacilitySynthesisPanel` | `OnDisable` | `void` | `private` | None |

### `Assets/Scripts/UI/BackgroundManager.cs`

- Functions detected: 2
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 12 | `BackgroundManager` | `Awake` | `void` | `private` | None |
| 41 | `BackgroundManager` | `Start` | `void` | `-` | None |

### `Assets/Scripts/UI/BuildingSummaryInfo.cs`

- Functions detected: 16
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 15 | `BuildingSummaryInfo` | `Construct` | `void` | `public` | None |
| 29 | `BuildingSummaryInfo` | `Start` | `void` | `private` | None |
| 36 | `BuildingSummaryInfo` | `OnTriggerEvent` | `void` | `public` | None |
| 54 | `BuildingSummaryInfo` | `OnClose` | `void` | `public override` | None |
| 59 | `BuildingSummaryInfo` | `OnEnable` | `void` | `public` | None |
| 64 | `BuildingSummaryInfo` | `OnDisable` | `void` | `private` | None |
| 69 | `BuildingSummaryInfo` | `ResolvePopupService` | `IUiPopupService` | `private` | None |
| 76 | `BuildingSummaryInfo` | `ResolveSummaryFormatter` | `IBuildingSummaryFormatter` | `private` | None |
| 83 | `BuildingSummaryInfo` | `RequireTmpKoreanFontService` | `ITmpKoreanFontService` | `private` | None |
| 90 | `BuildingSummaryInfo` | `RequireUiRoot` | `GameObject` | `private` | None |
| 96 | `BuildingSummaryInfo` | `RequireObjectNameText` | `TMP_Text` | `private` | None |
| 102 | `BuildingSummaryInfo` | `RequireStockText` | `TMP_Text` | `private` | None |
| 113 | `BuildingInfoTarget` | `BuildingInfoTarget` | `ctor` | `public` | None |
| 118 | `BuildingInfoTarget` | `GetInfoType` | `InfoFeedEvent.Type` | `public` | None |
| 133 | `InfoFeedEvent` | `InfoFeedEvent` | `ctor` | `public` | None |
| 138 | `InfoFeedEvent` | `Trigger` | `void` | `public static` | None |

### `Assets/Scripts/UI/CameraManager.cs`

- Functions detected: 18
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 20 | `CameraManager` | `Construct` | `void` | `public` | None |
| 32 | `CameraManager` | `Awake` | `void` | `private` | None |
| 37 | `CameraManager` | `OnEnable` | `void` | `private` | None |
| 42 | `CameraManager` | `Start` | `void` | `private` | None |
| 49 | `CameraManager` | `OnDisable` | `void` | `private` | None |
| 54 | `CameraManager` | `OnDestroy` | `void` | `private` | None |
| 59 | `CameraManager` | `Update` | `void` | `private` | None |
| 90 | `CameraManager` | `ClampToCurrentBounds` | `void` | `public` | None |
| 100 | `CameraManager` | `OnGridExpand` | `void` | `public` | None |
| 105 | `CameraManager` | `SubscribeToGridExpansionIfInjected` | `void` | `private` | None |
| 113 | `CameraManager` | `SubscribeToGridExpansion` | `void` | `private` | None |
| 126 | `CameraManager` | `UnsubscribeFromGridExpansion` | `void` | `private` | None |
| 137 | `CameraManager` | `RequireGridSystemProvider` | `IGridSystemProvider` | `private` | None |
| 143 | `CameraManager` | `UpdateViewportSize` | `void` | `private` | None |
| 157 | `CameraManager` | `GetGridBounds` | `void` | `private` | None |
| 172 | `CameraManager` | `ClampAxis` | `float` | `private static` | None |
| 184 | `CameraManager` | `RequireMainCameraProvider` | `IMainCameraProvider` | `private` | None |
| 190 | `CameraManager` | `RequireInputReader` | `IPlayerInputReader` | `private` | None |

### `Assets/Scripts/UI/CharacterSummaryRuntimeLogFactory.cs`

- Functions detected: 6
- Judgment: UI type usage fits this UI/factory context; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `ICharacterSummaryRuntimeLogFactory` | `Ensure` | `TMP_Text` | `-` | UI type dependency |
| 9 | `ICharacterSummaryRuntimeLogFactory` | `ApplyFonts` | `void` | `-` | None |
| 17 | `CharacterSummaryRuntimeLogFactory` | `CharacterSummaryRuntimeLogFactory` | `ctor` | `public` | None |
| 24 | `CharacterSummaryRuntimeLogFactory` | `Ensure` | `TMP_Text` | `public` | Runtime object creation, UI type dependency |
| 50 | `CharacterSummaryRuntimeLogFactory` | `ApplyFonts` | `void` | `public` | None |
| 60 | `CharacterSummaryRuntimeLogFactory` | `ApplyStyle` | `void` | `private` | None |

### `Assets/Scripts/UI/CharacterSummeryInfo.cs`

- Functions detected: 17
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 25 | `CharacterSummeryInfo` | `Construct` | `void` | `public` | None |
| 36 | `CharacterSummeryInfo` | `Start` | `void` | `private` | None |
| 43 | `CharacterSummeryInfo` | `OnTriggerEvent` | `void` | `public` | None |
| 83 | `CharacterSummeryInfo` | `OnClose` | `void` | `public override` | None |
| 89 | `CharacterSummeryInfo` | `OnStatChange` | `void` | `public` | None |
| 99 | `CharacterSummeryInfo` | `SetSlider` | `void` | `private static` | None |
| 114 | `CharacterSummeryInfo` | `OnLogAdded` | `void` | `public` | None |
| 119 | `CharacterSummeryInfo` | `RefreshLogText` | `void` | `public` | None |
| 129 | `CharacterSummeryInfo` | `FormatLogText` | `string` | `public static` | None |
| 134 | `CharacterSummeryInfo` | `FormatLogText` | `string` | `public static` | None |
| 152 | `CharacterSummeryInfo` | `OnEnable` | `void` | `public` | None |
| 157 | `CharacterSummeryInfo` | `OnDisable` | `void` | `private` | None |
| 163 | `CharacterSummeryInfo` | `UnbindCharacter` | `void` | `private` | None |
| 185 | `CharacterSummeryInfo` | `GetDisplayName` | `string` | `private static` | None |
| 196 | `CharacterSummeryInfo` | `ResolvePopupService` | `IUiPopupService` | `private` | None |
| 207 | `CharacterSummeryInfo` | `ResolveRuntimeLogFactory` | `ICharacterSummaryRuntimeLogFactory` | `private` | None |
| 218 | `CharacterSummeryInfo` | `RequireUiRoot` | `GameObject` | `private` | None |

### `Assets/Scripts/UI/DungeonBackdropSpriteTilingFactory.cs`

- Functions detected: 2
- Judgment: object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IDungeonBackdropSpriteTilingFactory` | `Duplicate` | `SpriteRenderer` | `-` | None |
| 11 | `DungeonBackdropSpriteTilingFactory` | `Duplicate` | `SpriteRenderer` | `public` | Runtime object creation |

### `Assets/Scripts/UI/DungeonSceneBackdropFitter.cs`

- Functions detected: 20
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 17 | `DungeonSceneBackdropFitter` | `Construct` | `void` | `public` | None |
| 29 | `DungeonSceneBackdropFitter` | `OnEnable` | `void` | `private` | None |
| 34 | `DungeonSceneBackdropFitter` | `Start` | `void` | `private` | None |
| 40 | `DungeonSceneBackdropFitter` | `OnDisable` | `void` | `private` | None |
| 45 | `DungeonSceneBackdropFitter` | `FitToGrid` | `void` | `public` | None |
| 61 | `DungeonSceneBackdropFitter` | `SubscribeToGridExpansionIfInjected` | `void` | `private` | None |
| 69 | `DungeonSceneBackdropFitter` | `SubscribeToGridExpansion` | `void` | `private` | None |
| 82 | `DungeonSceneBackdropFitter` | `UnsubscribeFromGridExpansion` | `void` | `private` | None |
| 93 | `DungeonSceneBackdropFitter` | `RequireGridSystemProvider` | `IGridSystemProvider` | `private` | None |
| 99 | `DungeonSceneBackdropFitter` | `RequireBackdropReferenceProvider` | `IDungeonBackdropReferenceProvider` | `private` | None |
| 105 | `DungeonSceneBackdropFitter` | `RequireSpriteTilingFactory` | `IDungeonBackdropSpriteTilingFactory` | `private` | None |
| 111 | `DungeonSceneBackdropFitter` | `ResolveBackgroundRoot` | `Transform` | `private` | None |
| 118 | `DungeonSceneBackdropFitter` | `ResolveGroundTilemap` | `Tilemap` | `private` | None |
| 125 | `DungeonSceneBackdropFitter` | `ExtendGround` | `void` | `private` | None |
| 156 | `DungeonSceneBackdropFitter` | `ExtendBackgroundSprites` | `void` | `private` | None |
| 181 | `DungeonSceneBackdropFitter` | `IsTiledBackgroundRenderer` | `bool` | `private static` | None |
| 189 | `DungeonSceneBackdropFitter` | `ExtendBackgroundSpriteGroup` | `void` | `private` | None |
| 215 | `DungeonSceneBackdropFitter` | `FitSolidBackground` | `void` | `private` | None |
| 233 | `DungeonSceneBackdropFitter` | `GetMinX` | `float` | `private static` | None |
| 244 | `DungeonSceneBackdropFitter` | `GetMaxX` | `float` | `private static` | None |

### `Assets/Scripts/UI/IInfoable.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IInfoable` | `GetInfoType` | `InfoFeedEvent.Type` | `public` | None |

### `Assets/Scripts/UI/NoticeFeed.cs`

- Functions detected: 7
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `NoticeFeed` | `ConstructNoticeFeed` | `void` | `public` | None |
| 15 | `NoticeFeed` | `OnTriggerEvent` | `void` | `public virtual` | None |
| 20 | `NoticeFeed` | `OnEnable` | `void` | `private` | None |
| 25 | `NoticeFeed` | `OnDisable` | `void` | `private` | None |
| 29 | `NoticeFeed` | `RequirePresenter` | `INoticeFeedPresenter` | `private` | None |
| 47 | `NoticeFeedEvent` | `NoticeFeedEvent` | `ctor` | `public` | None |
| 53 | `NoticeFeedEvent` | `Trigger` | `void` | `public static` | None |

### `Assets/Scripts/UI/NoticeFeedItemFactory.cs`

- Functions detected: 12
- Judgment: UI type usage fits this UI/factory context; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `INoticeFeedItemFactory` | `Create` | `GameObject` | `-` | None |
| 9 | `INoticeFeedItemFactory` | `TryPrepare` | `bool` | `-` | UI type dependency |
| 10 | `INoticeFeedItemFactory` | `OnTake` | `void` | `-` | None |
| 11 | `INoticeFeedItemFactory` | `OnReturn` | `void` | `-` | None |
| 12 | `INoticeFeedItemFactory` | `DestroyItem` | `void` | `-` | None |
| 18 | `NoticeFeedItemFactory` | `NoticeFeedItemFactory` | `ctor` | `public` | None |
| 25 | `NoticeFeedItemFactory` | `Create` | `GameObject` | `public` | Runtime object creation |
| 36 | `NoticeFeedItemFactory` | `TryPrepare` | `bool` | `public` | UI type dependency |
| 60 | `NoticeFeedItemFactory` | `OnTake` | `void` | `public` | None |
| 68 | `NoticeFeedItemFactory` | `OnReturn` | `void` | `public` | None |
| 76 | `NoticeFeedItemFactory` | `DestroyItem` | `void` | `public` | None |
| 84 | `NoticeFeedItemFactory` | `GetColor` | `Color` | `private static` | None |

### `Assets/Scripts/UI/NoticeFeedPresenter.cs`

- Functions detected: 5
- Judgment: UI type usage fits this UI/factory context.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 10 | `INoticeFeedPresenter` | `Present` | `void` | `-` | None |
| 23 | `NoticeFeedPresenter` | `NoticeFeedPresenter` | `ctor` | `public` | None |
| 29 | `NoticeFeedPresenter` | `Present` | `void` | `public` | UI type dependency |
| 57 | `NoticeFeedPresenter` | `GetPool` | `IObjectPool<GameObject>` | `private` | None |
| 68 | `NoticeFeedPresenter` | `CreatePool` | `IObjectPool<GameObject>` | `private` | None |

### `Assets/Scripts/UI/RuntimePanelFactories.cs`

- Functions detected: 16
- Judgment: manual resolver use should move to a composition root or factory; UI type usage fits this UI/factory context; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `ICodexPanelFactory` | `CreateDefaultPanel` | `CodexPanel` | `-` | None |
| 14 | `IFacilitySynthesisPanelFactory` | `CreateDefaultPanel` | `FacilitySynthesisPanel` | `-` | None |
| 19 | `IFacilityEvolutionPanelFactory` | `CreateDefaultPanel` | `FacilityEvolutionPanel` | `-` | None |
| 26 | `CodexPanelFactory` | `CodexPanelFactory` | `ctor` | `public` | None |
| 32 | `CodexPanelFactory` | `CodexPanelFactory` | `ctor` | `public` | None |
| 43 | `CodexPanelFactory` | `CreateDefaultPanel` | `CodexPanel` | `public` | Manual resolver/injection, UI type dependency |
| 65 | `CodexPanelFactory` | `CreateSummaryText` | `TMP_Text` | `private` | None |
| 76 | `FacilitySynthesisPanelFactory` | `FacilitySynthesisPanelFactory` | `ctor` | `public` | None |
| 82 | `FacilitySynthesisPanelFactory` | `FacilitySynthesisPanelFactory` | `ctor` | `public` | None |
| 93 | `FacilitySynthesisPanelFactory` | `CreateDefaultPanel` | `FacilitySynthesisPanel` | `public` | Manual resolver/injection, UI type dependency |
| 126 | `FacilityEvolutionPanelFactory` | `FacilityEvolutionPanelFactory` | `ctor` | `public` | None |
| 132 | `FacilityEvolutionPanelFactory` | `FacilityEvolutionPanelFactory` | `ctor` | `public` | None |
| 143 | `FacilityEvolutionPanelFactory` | `CreateDefaultPanel` | `FacilityEvolutionPanel` | `public` | Manual resolver/injection, UI type dependency |
| 174 | `RuntimePanelFactoryUtility` | `CreateOverlayCanvas` | `GameObject` | `public static` | Runtime object creation, UI type dependency |
| 184 | `RuntimePanelFactoryUtility` | `CreatePanel` | `GameObject` | `public static` | Runtime object creation, UI type dependency |
| 206 | `RuntimePanelFactoryUtility` | `CreateSummaryText` | `TMP_Text` | `public static` | Runtime object creation, UI type dependency |

### `Assets/Scripts/UI/SummeryInfo.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `SummeryInfo` | `ShowInfo` | `void` | `public abstract` | None |

### `Assets/Scripts/UI/TMPKoreanFont.cs`

- Functions detected: 10
- Judgment: UI type usage fits this UI/factory context.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `ITmpKoreanFontProvider` | `GetRequiredFont` | `TMP_FontAsset` | `-` | None |
| 12 | `ITmpKoreanFontService` | `Resolve` | `TMP_FontAsset` | `-` | None |
| 13 | `ITmpKoreanFontService` | `Apply` | `void` | `-` | UI type dependency |
| 14 | `ITmpKoreanFontService` | `ApplyToChildren` | `void` | `-` | None |
| 20 | `TmpKoreanFontAssetProvider` | `TmpKoreanFontAssetProvider` | `ctor` | `public` | None |
| 26 | `TmpKoreanFontAssetProvider` | `GetRequiredFont` | `TMP_FontAsset` | `public` | None |
| 36 | `TmpKoreanFontService` | `TmpKoreanFontService` | `ctor` | `public` | None |
| 42 | `TmpKoreanFontService` | `Resolve` | `TMP_FontAsset` | `public` | None |
| 47 | `TmpKoreanFontService` | `Apply` | `void` | `public` | None |
| 58 | `TmpKoreanFontService` | `ApplyToChildren` | `void` | `public` | UI type dependency |

### `Assets/Scripts/UI/TmpKoreanFontSettingsSO.cs`

- Functions detected: 1
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 11 | `TmpKoreanFontSettingsSO` | `GetRequiredFont` | `TMP_FontAsset` | `public` | None |

### `Assets/Scripts/UI/UIBase.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/UI/UIManager.cs`

- Functions detected: 16
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 20 | `UIManager` | `Construct` | `void` | `public` | None |
| 26 | `UIManager` | `Update` | `void` | `public` | None |
| 34 | `UIManager` | `CloseAllPopup` | `void` | `public` | None |
| 41 | `UIManager` | `OpenPopup` | `void` | `public` | None |
| 47 | `UIManager` | `ClosePopupPeek` | `void` | `public` | None |
| 52 | `UIManager` | `ClosePopupPeek` | `void` | `public` | None |
| 58 | `UIManager` | `UpdateTime` | `void` | `public` | None |
| 62 | `UIManager` | `UpdateHoldingMoneyText` | `void` | `private` | None |
| 66 | `UIManager` | `UpdateGameSpeedText` | `void` | `private` | None |
| 70 | `UIManager` | `MakeTouchFalse` | `void` | `public` | None |
| 75 | `UIManager` | `MakeTouchTrue` | `void` | `public` | None |
| 80 | `UIManager` | `OnEnable` | `void` | `private` | None |
| 87 | `UIManager` | `OnDisable` | `void` | `private` | None |
| 94 | `UIManager` | `OnHourChanged` | `void` | `private` | None |
| 99 | `UIManager` | `OnDayChanged` | `void` | `private` | None |
| 104 | `UIManager` | `RequireInputReader` | `IPlayerInputReader` | `private` | None |

### `Assets/Scripts/UI/UIPopUp.cs`

- Functions detected: 3
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `UIPopUp` | `OnOpen` | `void` | `public virtual` | None |
| 11 | `UIPopUp` | `OnClose` | `void` | `public virtual` | None |
| 15 | `UIPopUp` | `ClosePopup` | `void` | `public virtual` | None |

### `Assets/Scripts/UI/UiPopupService.cs`

- Functions detected: 14
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 5 | `IUiPopupService` | `CloseAll` | `void` | `-` | None |
| 6 | `IUiPopupService` | `Open` | `void` | `-` | None |
| 7 | `IUiPopupService` | `ClosePeek` | `void` | `-` | None |
| 12 | `IUiTouchGuardService` | `BlockTouch` | `void` | `-` | None |
| 13 | `IUiTouchGuardService` | `ReleaseTouch` | `void` | `-` | None |
| 19 | `UiPopupService` | `UiPopupService` | `ctor` | `public` | None |
| 24 | `UiPopupService` | `CloseAll` | `void` | `public` | None |
| 29 | `UiPopupService` | `Open` | `void` | `public` | None |
| 34 | `UiPopupService` | `ClosePeek` | `void` | `public` | None |
| 39 | `UiPopupService` | `ResolveManager` | `UIManager` | `private` | None |
| 50 | `UiTouchGuardService` | `UiTouchGuardService` | `ctor` | `public` | None |
| 55 | `UiTouchGuardService` | `BlockTouch` | `void` | `public` | None |
| 60 | `UiTouchGuardService` | `ReleaseTouch` | `void` | `public` | None |
| 65 | `UiTouchGuardService` | `ResolveManager` | `UIManager` | `private` | None |

### `Assets/Scripts/UI/UITab.cs`

- Functions detected: 6
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `UITab` | `Construct` | `void` | `public` | None |
| 16 | `UITab` | `ToggleTab` | `bool` | `public` | None |
| 29 | `UITab` | `Toggle` | `bool` | `public` | None |
| 43 | `UITab` | `OnClose` | `void` | `public override` | None |
| 47 | `UITab` | `CloseTab` | `void` | `public` | None |
| 52 | `UITab` | `ResolvePopupService` | `IUiPopupService` | `private` | None |

### `Assets/Scripts/UI/UITabContentTextProvider.cs`

- Functions detected: 12
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 7 | `IUITabContentTextProvider` | `Build` | `string` | `-` | None |
| 19 | `UITabContentTextProvider` | `UITabContentTextProvider` | `ctor` | `public` | None |
| 44 | `UITabContentTextProvider` | `Build` | `string` | `public` | None |
| 61 | `UITabContentTextProvider` | `BuildBuildingManagementText` | `string` | `private` | None |
| 79 | `UITabContentTextProvider` | `BuildStaffManagementText` | `string` | `private` | None |
| 114 | `UITabContentTextProvider` | `BuildShopText` | `string` | `private` | None |
| 132 | `UITabContentTextProvider` | `BuildWarehouseText` | `string` | `private` | None |
| 155 | `UITabContentTextProvider` | `BuildOperationText` | `string` | `private` | None |
| 182 | `UITabContentTextProvider` | `BuildInvasionDefenseText` | `string` | `private` | None |
| 211 | `UITabContentTextProvider` | `BuildOffenseText` | `string` | `private` | None |
| 239 | `UITabContentTextProvider` | `BuildResearchCraftingText` | `string` | `private` | None |
| 259 | `UITabContentTextProvider` | `BuildCodexRecordText` | `string` | `private` | None |

### `Assets/Scripts/UI/UITabGeneratedPanelFactory.cs`

- Functions detected: 9
- Judgment: manual resolver use is acceptable only in composition roots/factories; UI type usage fits this UI/factory context; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 9 | `GeneratedUITabPanel` | `GeneratedUITabPanel` | `ctor` | `public` | None |
| 21 | `IUITabGeneratedPanelFactory` | `Create` | `GeneratedUITabPanel` | `-` | None |
| 26 | `IStaffWorkPriorityPanelFactory` | `Ensure` | `StaffWorkPriorityPanel` | `-` | None |
| 37 | `UITabGeneratedPanelFactory` | `UITabGeneratedPanelFactory` | `ctor` | `public` | None |
| 50 | `UITabGeneratedPanelFactory` | `Create` | `GeneratedUITabPanel` | `public` | Manual resolver/injection, Runtime object creation, UI type dependency |
| 106 | `UITabGeneratedPanelFactory` | `CreateText` | `TMP_Text` | `private` | Runtime object creation, UI type dependency |
| 120 | `StaffWorkPriorityPanelFactory` | `StaffWorkPriorityPanelFactory` | `ctor` | `public` | None |
| 126 | `StaffWorkPriorityPanelFactory` | `Ensure` | `StaffWorkPriorityPanel` | `public` | Manual resolver/injection |
| 144 | `StaffWorkPriorityPanelFactory` | `DisablePlaceholderBody` | `void` | `private static` | UI type dependency |

### `Assets/Scripts/UI/UITabManager.cs`

- Functions detected: 36
- Judgment: UI type usage fits this UI/factory context.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 66 | `UITabManager` | `Start` | `void` | `private` | None |
| 71 | `UITabManager` | `Construct` | `void` | `public` | None |
| 94 | `UITabManager` | `ToggleSelectButton` | `void` | `public` | None |
| 118 | `UITabManager` | `ResgisterTab` | `void` | `public` | None |
| 123 | `UITabManager` | `UnRegisterTab` | `void` | `public` | None |
| 128 | `UITabManager` | `ConfigureTopTabs` | `void` | `private` | UI type dependency |
| 163 | `UITabManager` | `EnsureHudCanvasRendersAboveWorld` | `void` | `private` | UI type dependency |
| 176 | `UITabManager` | `RegisterExistingTabs` | `void` | `private` | None |
| 192 | `UITabManager` | `EnsureSpecializedTabContent` | `void` | `private` | None |
| 200 | `UITabManager` | `EnsureSpecializedTabContent` | `void` | `private` | None |
| 213 | `UITabManager` | `EnsureSpecializedTabContent` | `void` | `private` | None |
| 227 | `UITabManager` | `GetAllTabs` | `IEnumerable<UITab>` | `private` | None |
| 236 | `UITabManager` | `CloseAllTabsImmediate` | `void` | `private` | None |
| 249 | `UITabManager` | `CloseAllTabsForInitialState` | `void` | `private` | None |
| 257 | `UITabManager` | `GetPanelTitle` | `string` | `private static` | None |
| 270 | `UITabManager` | `OpenTabImmediate` | `void` | `private` | None |
| 278 | `UITabManager` | `BindTopButtons` | `void` | `private` | UI type dependency |
| 304 | `UITabManager` | `ResolveButtonPanel` | `Transform` | `private` | None |
| 323 | `UITabManager` | `GetButtonLabel` | `string` | `private static` | UI type dependency |
| 329 | `UITabManager` | `TryGetTopTabId` | `bool` | `private static` | None |
| 335 | `UITabManager` | `NormalizeTopTabLabel` | `string` | `private static` | None |
| 342 | `UITabManager` | `EnsureTopButtons` | `void` | `private` | UI type dependency |
| 383 | `UITabManager` | `ArrangeTopButtonsInSingleRow` | `void` | `private` | None |
| 393 | `UITabManager` | `OrderTopButtons` | `void` | `private static` | UI type dependency |
| 410 | `UITabManager` | `FindTopButtonForId` | `Button` | `private static` | UI type dependency |
| 423 | `UITabManager` | `NormalizeTopButtons` | `void` | `private` | UI type dependency |
| 455 | `UITabManager` | `SetButtonLabel` | `void` | `private` | UI type dependency |
| 464 | `UITabManager` | `PolishTopButton` | `void` | `private` | UI type dependency |
| 477 | `UITabManager` | `EnsureGeneratedTab` | `void` | `private` | None |
| 491 | `UITabManager` | `RefreshGeneratedTab` | `void` | `private` | UI type dependency |
| 507 | `UITabManager` | `BuildTabContent` | `string` | `private` | None |
| 518 | `UITabManager` | `ResolvePopupService` | `IUiPopupService` | `private` | None |
| 529 | `UITabManager` | `RequireTmpKoreanFontService` | `ITmpKoreanFontService` | `private` | None |
| 536 | `UITabManager` | `RequireGeneratedPanelFactory` | `IUITabGeneratedPanelFactory` | `private` | None |
| 543 | `UITabManager` | `RequireStaffWorkPriorityPanelFactory` | `IStaffWorkPriorityPanelFactory` | `private` | None |
| 550 | `UITabManager` | `RequireTopButtonFactory` | `IUITabTopButtonFactory` | `private` | None |

### `Assets/Scripts/UI/UITabTopButtonFactory.cs`

- Functions detected: 8
- Judgment: UI type usage fits this UI/factory context; object creation fits this factory/context boundary.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 8 | `IUITabTopButtonFactory` | `CreateButton` | `Button` | `-` | UI type dependency |
| 9 | `IUITabTopButtonFactory` | `EnsureSingleRowLayout` | `HorizontalLayoutGroup` | `-` | None |
| 15 | `UITabTopButtonFactory` | `UITabTopButtonFactory` | `ctor` | `public` | None |
| 21 | `UITabTopButtonFactory` | `CreateButton` | `Button` | `public` | Runtime object creation, UI type dependency |
| 40 | `UITabTopButtonFactory` | `EnsureSingleRowLayout` | `HorizontalLayoutGroup` | `public` | UI type dependency |
| 84 | `UITabTopButtonFactory` | `SetLabel` | `void` | `private` | UI type dependency |
| 96 | `UITabTopButtonFactory` | `MoveRowChildrenBackToRoot` | `void` | `private static` | None |
| 117 | `UITabTopButtonFactory` | `SanitizeName` | `string` | `private static` | None |

### `Assets/Scripts/UI/WorldInfoClickSelector.cs`

- Functions detected: 12
- Judgment: input/camera access should be wrapped for testability and UI blocking.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 6 | `IWorldInfoClickSelector` | `TryTriggerCharacterUnderPointer` | `bool` | `-` | None |
| 7 | `IWorldInfoClickSelector` | `TryGetPreferredCharacterUnderPointer` | `bool` | `-` | None |
| 8 | `IWorldInfoClickSelector` | `TryGetPreferredCharacterAtScreenPosition` | `bool` | `-` | None |
| 9 | `IWorldInfoClickSelector` | `TryGetPreferredCharacter` | `bool` | `-` | None |
| 19 | `WorldInfoClickSelectionService` | `WorldInfoClickSelectionService` | `ctor` | `public` | None |
| 29 | `WorldInfoClickSelectionService` | `TryTriggerCharacterUnderPointer` | `bool` | `public` | None |
| 45 | `WorldInfoClickSelectionService` | `TryGetPreferredCharacterUnderPointer` | `bool` | `public` | None |
| 50 | `WorldInfoClickSelectionService` | `TryGetPreferredCharacterAtScreenPosition` | `bool` | `public` | Direct input/camera dependency |
| 65 | `WorldInfoClickSelectionService` | `TryGetPreferredCharacter` | `bool` | `public` | None |
| 104 | `WorldInfoClickSelectionService` | `CanSelectWorldInfo` | `bool` | `private` | None |
| 109 | `WorldInfoClickSelectionService` | `GetCharacterClickPriority` | `int` | `private static` | None |
| 136 | `WorldInfoClickSelectionService` | `TriggerCharacter` | `void` | `private` | None |

### `Assets/Scripts/Utils/DataScriptableObject.cs`

- Functions detected: 0
- Judgment: No obvious static coupling signals found.
- No methods/functions detected by the lightweight parser.

### `Assets/Scripts/Utils/EventObserver.cs`

- Functions detected: 15
- Judgment: No obvious static coupling signals found.

| Line | Type | Function | Return | Modifiers | Coupling / placement signals |
|------|------|----------|--------|-----------|------------------------------|
| 10 | `EventObserver` | `EventObserver` | `ctor` | `static` | None |
| 15 | `EventObserver` | `AddListener` | `void` | `public static` | None |
| 29 | `EventObserver` | `RemoveListener` | `void` | `public static` | None |
| 52 | `EventObserver` | `TriggerEvent` | `void` | `public static` | None |
| 62 | `EventObserver` | `SubscriptionExists` | `bool` | `private static` | None |
| 85 | `UtilEventListener` | `OnTriggerEvent` | `void` | `-` | None |
| 90 | `EventRegister` | `EventStartListening` | `void` | `public static` | None |
| 95 | `EventRegister` | `EventStopListening` | `void` | `public static` | None |
| 107 | `EventListenerWrapper` | `EventListenerWrapper` | `ctor` | `public` | None |
| 113 | `EventListenerWrapper` | `Dispose` | `void` | `public` | None |
| 119 | `EventListenerWrapper` | `OnEvent` | `TTarget` | `protected virtual` | None |
| 121 | `EventListenerWrapper` | `OnTriggerEvent` | `void` | `public` | None |
| 126 | `EventListenerWrapper` | `RegisterCallbacks` | `void` | `private` | None |
| 142 | `GameEvent` | `GameEvent` | `ctor` | `public` | None |
| 147 | `GameEvent` | `Trigger` | `void` | `public static` | None |

## Vendor Appendix
Vendor/plugin function lists are in `SCRIPT_FUNCTION_AUDIT_VENDOR.md`.
