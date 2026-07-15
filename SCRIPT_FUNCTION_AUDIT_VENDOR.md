# Script Function Audit - Vendor Appendix

- Generated: 2026-07-12
- Vendor/plugin non-Editor scripts: 613
- These files are list-only and not refactor targets.

## `Assets/Behavior Designer/Runtime/BehaviorTree.cs`

- Functions detected: 17

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 28 | `-` | `DungeonStoryReloadExternalBehaviorForRuntime` | `void` | `public` |
| 47 | `-` | `DungeonStoryRefreshVisualStatus` | `void` | `public` |
| 63 | `-` | `DungeonStoryManualTick` | `bool` | `public` |
| 118 | `-` | `RefreshDungeonStoryVisualPath` | `void` | `private` |
| 171 | `-` | `MarkDungeonStoryMacroPath` | `void` | `private` |
| 207 | `-` | `MarkDungeonStoryActionPath` | `void` | `private static` |
| 229 | `-` | `ClearDungeonStoryExecutionStatus` | `void` | `private static` |
| 258 | `-` | `MarkDungeonStoryPath` | `bool` | `private static` |
| 263 | `-` | `MarkDungeonStoryPathWithDescendants` | `bool` | `private static` |
| 274 | `-` | `MarkDungeonStoryPathAndGetLast` | `bool` | `private static` |
| 295 | `-` | `FindDungeonStoryChild` | `Task` | `private static` |
| 313 | `-` | `MarkDungeonStoryNodeRunning` | `void` | `private static` |
| 324 | `-` | `MarkDungeonStorySubtreeRunning` | `void` | `private static` |
| 343 | `-` | `TryPrepareDungeonStoryRuntimeTree` | `bool` | `private` |
| 375 | `-` | `BindDungeonStoryTaskTree` | `void` | `private` |
| 406 | `-` | `EvaluateDungeonStoryTask` | `TaskStatus` | `private` |
| 425 | `-` | `EvaluateDungeonStoryParentTask` | `TaskStatus` | `private` |

## `Assets/Behavior Designer/Runtime/ExternalBehaviorTree.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Object Drawers/FloatSliderAttribute.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `FloatSliderAttribute` | `-` | `public` |

## `Assets/Behavior Designer/Runtime/Object Drawers/IntSliderAttribute.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `IntSliderAttribute` | `-` | `public` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/BehaviorTreeReference.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Tasks/Actions/Idle.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `OnUpdate` | `TaskStatus` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/Log.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 26 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/PerformInterruption.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/Reflection/GetFieldValue.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 21 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 48 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/Reflection/GetPropertyValue.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 21 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 48 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/Reflection/InvokeMethod.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 71 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/Reflection/SetFieldValue.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 47 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/Reflection/SetPropertyValue.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 47 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/RestartBehaviorTree.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnAwake` | `void` | `public override` |
| 34 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 48 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/SendEvent.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 27 | `-` | `OnStart` | `void` | `public override` |
| 46 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 65 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/StackedAction.cs`

- Functions detected: 16

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnAwake` | `void` | `public override` |
| 41 | `-` | `OnStart` | `void` | `public override` |
| 55 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 81 | `-` | `OnFixedUpdate` | `void` | `public override` |
| 95 | `-` | `OnLateUpdate` | `void` | `public override` |
| 109 | `-` | `OnEnd` | `void` | `public override` |
| 123 | `-` | `OnTriggerEnter` | `void` | `public override` |
| 137 | `-` | `OnTriggerEnter2D` | `void` | `public override` |
| 151 | `-` | `OnTriggerExit` | `void` | `public override` |
| 165 | `-` | `OnTriggerExit2D` | `void` | `public override` |
| 179 | `-` | `OnCollisionEnter` | `void` | `public override` |
| 193 | `-` | `OnCollisionEnter2D` | `void` | `public override` |
| 207 | `-` | `OnCollisionExit` | `void` | `public override` |
| 221 | `-` | `OnCollisionExit2D` | `void` | `public override` |
| 235 | `-` | `OnDrawNodeText` | `string` | `public override` |
| 255 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/StartBehaviorTree.cs`

- Functions detected: 5

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnStart` | `void` | `public override` |
| 55 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 69 | `-` | `BehaviorEnded` | `void` | `private` |
| 74 | `-` | `OnEnd` | `void` | `public override` |
| 81 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/StopBehaviorTree.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 36 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 47 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Actions/Wait.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 24 | `-` | `OnStart` | `void` | `public override` |
| 35 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 45 | `-` | `OnPause` | `void` | `public override` |
| 56 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Composites/Parallel.cs`

- Functions detected: 9

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnAwake` | `void` | `public override` |
| 20 | `-` | `OnChildStarted` | `void` | `public override` |
| 27 | `-` | `CanRunParallelChildren` | `bool` | `public override` |
| 33 | `-` | `CurrentChildIndex` | `int` | `public override` |
| 38 | `-` | `CanExecute` | `bool` | `public override` |
| 44 | `-` | `OnChildExecuted` | `void` | `public override` |
| 50 | `-` | `OverrideStatus` | `TaskStatus` | `public override` |
| 67 | `-` | `OnConditionalAbort` | `void` | `public override` |
| 76 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Composites/ParallelComplete.cs`

- Functions detected: 9

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnAwake` | `void` | `public override` |
| 18 | `-` | `OnChildStarted` | `void` | `public override` |
| 25 | `-` | `CanRunParallelChildren` | `bool` | `public override` |
| 31 | `-` | `CurrentChildIndex` | `int` | `public override` |
| 36 | `-` | `CanExecute` | `bool` | `public override` |
| 42 | `-` | `OnChildExecuted` | `void` | `public override` |
| 48 | `-` | `OnConditionalAbort` | `void` | `public override` |
| 57 | `-` | `OverrideStatus` | `TaskStatus` | `public override` |
| 71 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Composites/ParallelSelector.cs`

- Functions detected: 9

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnAwake` | `void` | `public override` |
| 20 | `-` | `OnChildStarted` | `void` | `public override` |
| 27 | `-` | `CanRunParallelChildren` | `bool` | `public override` |
| 33 | `-` | `CurrentChildIndex` | `int` | `public override` |
| 38 | `-` | `CanExecute` | `bool` | `public override` |
| 44 | `-` | `OnChildExecuted` | `void` | `public override` |
| 50 | `-` | `OnConditionalAbort` | `void` | `public override` |
| 59 | `-` | `OverrideStatus` | `TaskStatus` | `public override` |
| 76 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Composites/PrioritySelector.cs`

- Functions detected: 6

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 37 | `-` | `CurrentChildIndex` | `int` | `public override` |
| 43 | `-` | `CanExecute` | `bool` | `public override` |
| 49 | `-` | `OnChildExecuted` | `void` | `public override` |
| 56 | `-` | `OnConditionalAbort` | `void` | `public override` |
| 63 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Composites/RandomSelector.cs`

- Functions detected: 9

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 25 | `-` | `OnAwake` | `void` | `public override` |
| 39 | `-` | `OnStart` | `void` | `public override` |
| 45 | `-` | `CurrentChildIndex` | `int` | `public override` |
| 51 | `-` | `CanExecute` | `bool` | `public override` |
| 57 | `-` | `OnChildExecuted` | `void` | `public override` |
| 66 | `-` | `OnConditionalAbort` | `void` | `public override` |
| 74 | `-` | `OnEnd` | `void` | `public override` |
| 81 | `-` | `OnReset` | `void` | `public override` |
| 88 | `-` | `ShuffleChilden` | `void` | `private` |

## `Assets/Behavior Designer/Runtime/Tasks/Composites/RandomSequence.cs`

- Functions detected: 9

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 26 | `-` | `OnAwake` | `void` | `public override` |
| 40 | `-` | `OnStart` | `void` | `public override` |
| 46 | `-` | `CurrentChildIndex` | `int` | `public override` |
| 52 | `-` | `CanExecute` | `bool` | `public override` |
| 58 | `-` | `OnChildExecuted` | `void` | `public override` |
| 67 | `-` | `OnConditionalAbort` | `void` | `public override` |
| 75 | `-` | `OnEnd` | `void` | `public override` |
| 82 | `-` | `OnReset` | `void` | `public override` |
| 89 | `-` | `ShuffleChilden` | `void` | `private` |

## `Assets/Behavior Designer/Runtime/Tasks/Composites/Selector.cs`

- Functions detected: 5

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `CurrentChildIndex` | `int` | `public override` |
| 17 | `-` | `CanExecute` | `bool` | `public override` |
| 23 | `-` | `OnChildExecuted` | `void` | `public override` |
| 30 | `-` | `OnConditionalAbort` | `void` | `public override` |
| 37 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Composites/SelectorEvaluator.cs`

- Functions detected: 11

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `CurrentChildIndex` | `int` | `public override` |
| 23 | `-` | `OnChildStarted` | `void` | `public override` |
| 30 | `-` | `CanExecute` | `bool` | `public override` |
| 44 | `-` | `OnChildExecuted` | `void` | `public override` |
| 57 | `-` | `OnConditionalAbort` | `void` | `public override` |
| 64 | `-` | `OnEnd` | `void` | `public override` |
| 71 | `-` | `OverrideStatus` | `TaskStatus` | `public override` |
| 76 | `-` | `CanRunParallelChildren` | `bool` | `public override` |
| 83 | `-` | `CanReevaluate` | `bool` | `public override` |
| 89 | `-` | `OnReevaluationStarted` | `bool` | `public override` |
| 105 | `-` | `OnReevaluationEnded` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Composites/Sequence.cs`

- Functions detected: 5

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `CurrentChildIndex` | `int` | `public override` |
| 17 | `-` | `CanExecute` | `bool` | `public override` |
| 23 | `-` | `OnChildExecuted` | `void` | `public override` |
| 30 | `-` | `OnConditionalAbort` | `void` | `public override` |
| 37 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Composites/UtilitySelector.cs`

- Functions detected: 12

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 21 | `-` | `OnStart` | `void` | `public override` |
| 37 | `-` | `CurrentChildIndex` | `int` | `public override` |
| 43 | `-` | `OnChildStarted` | `void` | `public override` |
| 49 | `-` | `CanExecute` | `bool` | `public override` |
| 59 | `-` | `OnChildExecuted` | `void` | `public override` |
| 82 | `-` | `OnConditionalAbort` | `void` | `public override` |
| 89 | `-` | `OnEnd` | `void` | `public override` |
| 96 | `-` | `OverrideStatus` | `TaskStatus` | `public override` |
| 101 | `-` | `CanRunParallelChildren` | `bool` | `public override` |
| 107 | `-` | `CanReevaluate` | `bool` | `public override` |
| 113 | `-` | `OnReevaluationStarted` | `bool` | `public override` |
| 125 | `-` | `OnReevaluationEnded` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/HasReceivedEvent.cs`

- Functions detected: 9

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 21 | `-` | `OnStart` | `void` | `public override` |
| 33 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnEnd` | `void` | `public override` |
| 50 | `-` | `ReceivedEvent` | `void` | `private` |
| 55 | `-` | `ReceivedEvent` | `void` | `private` |
| 64 | `-` | `ReceivedEvent` | `void` | `private` |
| 77 | `-` | `ReceivedEvent` | `void` | `private` |
| 94 | `-` | `OnBehaviorComplete` | `void` | `public override` |
| 106 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/Physics/HasEnteredCollision.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnEnd` | `void` | `public override` |
| 25 | `-` | `OnCollisionEnter` | `void` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/Physics/HasEnteredCollision2D.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnEnd` | `void` | `public override` |
| 25 | `-` | `OnCollisionEnter2D` | `void` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/Physics/HasEnteredTrigger.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnEnd` | `void` | `public override` |
| 25 | `-` | `OnTriggerEnter` | `void` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/Physics/HasEnteredTrigger2D.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnEnd` | `void` | `public override` |
| 25 | `-` | `OnTriggerEnter2D` | `void` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/Physics/HasExitedCollision.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnEnd` | `void` | `public override` |
| 25 | `-` | `OnCollisionExit` | `void` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/Physics/HasExitedCollision2D.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnEnd` | `void` | `public override` |
| 25 | `-` | `OnCollisionExit2D` | `void` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/Physics/HasExitedTrigger.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnEnd` | `void` | `public override` |
| 25 | `-` | `OnTriggerExit` | `void` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/Physics/HasExitedTrigger2D.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnEnd` | `void` | `public override` |
| 25 | `-` | `OnTriggerExit2D` | `void` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/RandomProbability.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnAwake` | `void` | `public override` |
| 22 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 32 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/Reflection/CompareFieldValue.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 51 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/Reflection/ComparePropertyValue.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 49 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Conditionals/StackedConditional.cs`

- Functions detected: 16

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnAwake` | `void` | `public override` |
| 41 | `-` | `OnStart` | `void` | `public override` |
| 55 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 81 | `-` | `OnFixedUpdate` | `void` | `public override` |
| 95 | `-` | `OnLateUpdate` | `void` | `public override` |
| 109 | `-` | `OnEnd` | `void` | `public override` |
| 123 | `-` | `OnTriggerEnter` | `void` | `public override` |
| 137 | `-` | `OnTriggerEnter2D` | `void` | `public override` |
| 151 | `-` | `OnTriggerExit` | `void` | `public override` |
| 165 | `-` | `OnTriggerExit2D` | `void` | `public override` |
| 179 | `-` | `OnCollisionEnter` | `void` | `public override` |
| 193 | `-` | `OnCollisionEnter2D` | `void` | `public override` |
| 207 | `-` | `OnCollisionExit` | `void` | `public override` |
| 221 | `-` | `OnCollisionExit2D` | `void` | `public override` |
| 235 | `-` | `OnDrawNodeText` | `string` | `public override` |
| 255 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Decorators/ConditionalEvaluator.cs`

- Functions detected: 11

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnAwake` | `void` | `public override` |
| 30 | `-` | `OnStart` | `void` | `public override` |
| 37 | `-` | `CanExecute` | `bool` | `public override` |
| 51 | `-` | `CanReevaluate` | `bool` | `public override` |
| 56 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 63 | `-` | `OnChildExecuted` | `void` | `public override` |
| 69 | `-` | `OverrideStatus` | `TaskStatus` | `public override` |
| 76 | `-` | `OverrideStatus` | `TaskStatus` | `public override` |
| 83 | `-` | `OnEnd` | `void` | `public override` |
| 94 | `-` | `OnDrawNodeText` | `string` | `public override` |
| 103 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Decorators/Cooldown.cs`

- Functions detected: 6

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `CanExecute` | `bool` | `public override` |
| 23 | `-` | `CurrentChildIndex` | `int` | `public override` |
| 31 | `-` | `OnChildExecuted` | `void` | `public override` |
| 39 | `-` | `OverrideStatus` | `TaskStatus` | `public override` |
| 47 | `-` | `OverrideStatus` | `TaskStatus` | `public override` |
| 55 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Decorators/Interrupt.cs`

- Functions detected: 5

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `CanExecute` | `bool` | `public override` |
| 19 | `-` | `OnChildExecuted` | `void` | `public override` |
| 25 | `-` | `DoInterrupt` | `void` | `public` |
| 34 | `-` | `OverrideStatus` | `TaskStatus` | `public override` |
| 40 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Decorators/Inverter.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `CanExecute` | `bool` | `public override` |
| 16 | `-` | `OnChildExecuted` | `void` | `public override` |
| 22 | `-` | `Decorate` | `TaskStatus` | `public override` |
| 33 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Decorators/Repeater.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `CanExecute` | `bool` | `public override` |
| 25 | `-` | `OnChildExecuted` | `void` | `public override` |
| 32 | `-` | `OnEnd` | `void` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Decorators/ReturnFailure.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `CanExecute` | `bool` | `public override` |
| 15 | `-` | `OnChildExecuted` | `void` | `public override` |
| 21 | `-` | `Decorate` | `TaskStatus` | `public override` |
| 30 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Decorators/ReturnSuccess.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `CanExecute` | `bool` | `public override` |
| 15 | `-` | `OnChildExecuted` | `void` | `public override` |
| 21 | `-` | `Decorate` | `TaskStatus` | `public override` |
| 30 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Decorators/TaskGuard.cs`

- Functions detected: 6

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 25 | `-` | `CanExecute` | `bool` | `public override` |
| 31 | `-` | `OnChildStarted` | `void` | `public override` |
| 41 | `-` | `OverrideStatus` | `TaskStatus` | `public override` |
| 47 | `-` | `taskExecuting` | `void` | `public` |
| 54 | `-` | `OnEnd` | `void` | `public override` |
| 67 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Decorators/UntilFailure.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `CanExecute` | `bool` | `public override` |
| 15 | `-` | `OnChildExecuted` | `void` | `public override` |
| 21 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Decorators/UntilSuccess.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `CanExecute` | `bool` | `public override` |
| 15 | `-` | `OnChildExecuted` | `void` | `public override` |
| 21 | `-` | `OnEnd` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/EntryTask.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 8 | `-` | `MaxChildren` | `int` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animation/Blend.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 21 | `-` | `OnStart` | `void` | `public override` |
| 30 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 42 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animation/CrossFade.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 23 | `-` | `OnStart` | `void` | `public override` |
| 32 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 48 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animation/CrossFadeQueued.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 23 | `-` | `OnStart` | `void` | `public override` |
| 32 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 44 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animation/GetAnimatePhysics.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animation/IsPlaying.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animation/Play.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 44 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animation/PlayQueued.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 21 | `-` | `OnStart` | `void` | `public override` |
| 30 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 42 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animation/Rewind.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 42 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animation/Sample.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animation/SetAnimatePhysics.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animation/SetWrapMode.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animation/Stop.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 42 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/CrossFade.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 22 | `-` | `OnStart` | `void` | `public override` |
| 31 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 43 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/GetApplyRootMotion.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/GetBoolParameter.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/GetDeltaPosition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/GetDeltaRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/GetFloatParameter.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/GetGravityWeight.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/GetIntegerParameter.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/GetLayerWeight.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/GetSpeed.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/GetStringToHash.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/InterruptMatchTarget.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/IsInTransition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/IsName.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/IsParameterControlledByCurve.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/MatchTarget.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 28 | `-` | `OnStart` | `void` | `public override` |
| 37 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 49 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/Play.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnStart` | `void` | `public override` |
| 29 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 41 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/SetApplyRootMotion.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/SetBoolParameter.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 22 | `-` | `OnStart` | `void` | `public override` |
| 31 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 48 | `-` | `ResetValue` | `IEnumerator` | `public` |
| 54 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/SetFloatParameter.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 22 | `-` | `OnStart` | `void` | `public override` |
| 31 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 49 | `-` | `ResetValue` | `IEnumerator` | `public` |
| 55 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/SetIntegerParameter.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 22 | `-` | `OnStart` | `void` | `public override` |
| 31 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 49 | `-` | `ResetValue` | `IEnumerator` | `public` |
| 55 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/SetLayerWeight.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/SetLookAtPosition.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 20 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 30 | `-` | `OnAnimatorIK` | `void` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/SetLookAtWeight.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 23 | `-` | `OnStart` | `void` | `public override` |
| 29 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnAnimatorIK` | `void` | `public override` |
| 48 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/SetSpeed.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/SetTrigger.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/StartPlayback.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/StartRecording.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/StopPlayback.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/StopRecording.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Animator/WaitForState.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnAwake` | `void` | `public override` |
| 24 | `-` | `OnStart` | `void` | `public override` |
| 37 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 52 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/GetIgnoreListenerPause.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/GetIgnoreListenerVolume.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/GetLoop.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/GetMaxDistance.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/GetMinDistance.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/GetMute.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/GetPitch.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/GetPriority.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/GetSpread.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/GetTime.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/GetTimeSamples.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/GetVolume.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/IsPlaying.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/Pause.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/Play.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/PlayDelayed.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/PlayOneShot.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/PlayScheduled.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetAudioClip.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetIgnoreListenerPause.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetIgnoreListenerVolume.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetLoop.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetMaxDistance.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetMinDistance.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetMute.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetPitch.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetPriority.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetRolloffMode.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetScheduledEndTime.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetScheduledStartTime.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetSpread.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetTime.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetVelocityUpdateMode.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/SetVolume.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/AudioSource/Stop.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Behaviour/GetEnabled.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 26 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Behaviour/IsEnabled.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Behaviour/SetEnabled.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 25 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/BoxCollider/GetCenter.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/BoxCollider/GetSize.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/BoxCollider/SetCenter.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/BoxCollider/SetSize.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/BoxCollider2D/GetSize.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/BoxCollider2D/SetSize.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CapsuleCollider/GetCenter.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CapsuleCollider/GetDirection.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CapsuleCollider/GetHeight.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CapsuleCollider/GetRadius.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CapsuleCollider/SetCenter.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CapsuleCollider/SetDirection.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CapsuleCollider/SetHeight.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CapsuleCollider/SetRadius.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/GetCenter.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/GetHeight.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/GetRadius.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/GetSlopeLimit.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/GetStepOffset.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/GetVelocity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/HasColliderHit.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnEnd` | `void` | `public override` |
| 27 | `-` | `OnControllerColliderHit` | `void` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/IsGrounded.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/Move.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/SetCenter.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/SetHeight.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/SetRadius.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/SetSlopeLimit.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/SetStepOffset.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CharacterController/SimpleMove.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CircleCollider2D/GetOffset.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CircleCollider2D/GetRadius.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CircleCollider2D/SetOffset.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/CircleCollider2D/SetRadius.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Collider/GetEnabled.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 26 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Collider/SetEnabled.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 25 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Debug/DrawLine.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 26 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Debug/DrawRay.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Debug/LogFormat.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 22 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 34 | `-` | `buildParamsArray` | `object[]` | `private` |
| 60 | `-` | `isValid` | `bool` | `private` |
| 64 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Debug/LogValue.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/ActiveInHierarchy.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/ActiveSelf.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/CompareLayer.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/CompareTag.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/Destroy.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 25 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/DestroyImmediate.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/Find.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/FindGameObjectsWithTag.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 24 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/FindWithTag.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 29 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/GetComponent.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 23 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/GetTag.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/Instantiate.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 25 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/SendMessage.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 26 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/SetActive.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/GameObject/SetTag.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/GetAcceleration.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/GetAxis.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 29 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/GetAxisRaw.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 29 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/GetButton.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/GetKey.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/GetMouseButton.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/GetMousePosition.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/IsButtonDown.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/IsButtonUp.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/IsKeyDown.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/IsKeyUp.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/IsMouseDown.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Input/IsMouseUp.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/LayerMask/GetLayer.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/LayerMask/SetLayer.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/GetColor.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/GetCookieSize.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/GetIntensity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/GetRange.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/GetShadowBias.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/GetShadowStrength.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/GetSpotAngle.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/SetColor.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/SetCookie.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/SetCookieSize.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/SetCullingMask.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/SetIntensity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/SetRange.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/SetShadowBias.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/SetShadows.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/SetShadowStrength.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/SetSpotAngle.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Light/SetType.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/BoolComparison.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/BoolFlip.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 15 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/BoolOperator.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 42 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/FloatAbs.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 17 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/FloatClamp.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/FloatComparison.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 42 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/FloatOperator.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 56 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/IntAbs.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 17 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/IntClamp.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/IntComparison.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 42 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/IntOperator.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 29 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 57 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/IsFloatPositive.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 14 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/IsIntPositive.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 14 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/Lerp.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 24 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/LerpAngle.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 24 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/RandomBool.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/RandomFloat.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 27 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/RandomInt.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 27 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/SetBool.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 17 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/SetFloat.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 17 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Math/SetInt.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 17 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/GetAcceleration.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/GetAngularSpeed.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/GetDestination.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/GetIsStopped.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/GetRemainingDistance.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/GetSpeed.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/IsStopped.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/Move.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/ResetPath.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/Resume.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/SetAcceleration.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/SetAngularSpeed.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/SetDestination.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/SetIsStopped.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/SetSpeed.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/Stop.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/NavMeshAgent/Warp.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/Clear.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/GetDuration.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/GetEnableEmission.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/GetLoop.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/GetMaxParticles.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/GetParticleCount.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/GetPlaybackSpeed.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/GetTime.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/IsAlive.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/IsPaused.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/IsPlaying.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/IsStopped.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/Pause.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/Play.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/SetEnableEmission.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/SetLoop.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/SetMaxParticles.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/SetPlaybackSpeed.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/SetStartColor.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/SetStartDelay.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/SetStartLifetime.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/SetStartRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/SetStartSize.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/SetStartSpeed.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/SetTime.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/Simulate.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/ParticleSystem/Stop.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Physics/Linecast.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Physics/Raycast.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 34 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 59 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Physics/Spherecast.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 36 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 61 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Physics2D/Circlecast.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 36 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 60 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Physics2D/Linecast.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Physics2D/Raycast.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 34 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 58 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/PlayerPrefs/DeleteAll.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `OnUpdate` | `TaskStatus` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/PlayerPrefs/DeleteKey.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/PlayerPrefs/GetFloat.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 23 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/PlayerPrefs/GetInt.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 23 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/PlayerPrefs/GetString.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 23 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/PlayerPrefs/HasKey.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/PlayerPrefs/Save.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `OnUpdate` | `TaskStatus` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/PlayerPrefs/SetFloat.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/PlayerPrefs/SetInt.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/PlayerPrefs/SetString.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Quaternion/Angle.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Quaternion/AngleAxis.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Quaternion/Dot.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Quaternion/Euler.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Quaternion/FromToRotation.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Quaternion/Identity.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Quaternion/Inverse.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Quaternion/Lerp.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 24 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Quaternion/LookRotation.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Quaternion/RotateTowards.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 24 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Quaternion/Slerp.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 24 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Renderer/IsVisible.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnStart` | `void` | `public override` |
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 34 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Renderer/SetMaterial.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/AddExplosionForce.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 25 | `-` | `OnStart` | `void` | `public override` |
| 34 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 46 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/AddForce.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnStart` | `void` | `public override` |
| 29 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 41 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/AddForceAtPosition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 21 | `-` | `OnStart` | `void` | `public override` |
| 30 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 42 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/AddRelativeForce.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/AddRelativeTorque.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/AddTorque.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/GetAngularDrag.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/GetAngularVelocity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/GetCenterOfMass.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/GetDrag.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/GetFreezeRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/GetIsKinematic.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/GetMass.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/GetPosition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/GetRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/GetUseGravity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/GetVelocity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/IsKinematic.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnStart` | `void` | `public override` |
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 34 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/IsSleeping.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnStart` | `void` | `public override` |
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 34 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/MovePosition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/MoveRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/SetAngularDrag.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/SetAngularVelocity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/SetCenterOfMass.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/SetConstraints.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/SetDrag.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/SetFreezeRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/SetIsKinematic.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/SetMass.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/SetPosition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/SetRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/SetUseGravity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/SetVelocity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/Sleep.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnStart` | `void` | `public override` |
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 36 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/UseGravity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnStart` | `void` | `public override` |
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 34 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody/WakeUp.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnStart` | `void` | `public override` |
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 36 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/AddForce.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/AddForceAtPosition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/AddTorque.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/GetAngularDrag.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/GetAngularVelocity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/GetDrag.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/GetGravtyScale.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/GetIsKinematic.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/GetMass.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/GetPosition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/GetRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/GetVelocity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/IsKinematic.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/IsSleeping.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/MovePosition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/MoveRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/SetAngularDrag.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/SetAngularVelocity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/SetDrag.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/SetGravityScale.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/SetIsKinematic.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/SetMass.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/SetVelocity.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/Sleep.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Rigidbody2D/WakeUp.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnStart` | `void` | `public override` |
| 23 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedBool.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedColor.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedFloat.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedGameObject.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 23 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedGameObjectList.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 28 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedInt.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedObject.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 23 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedObjectList.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 28 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedQuaternion.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedRect.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedString.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 16 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedTransform.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 23 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedTransformList.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 28 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedVector2.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedVector3.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/CompareSharedVector4.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedBool.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedColor.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedFloat.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedGameObject.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedGameObjectList.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedInt.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedObject.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedObjectList.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedQuaternion.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedRect.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedString.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedTransform.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedTransformList.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedVector2.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedVector3.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SetSharedVector4.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SharedGameObjectsToGameObjectList.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnAwake` | `void` | `public override` |
| 20 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 34 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SharedGameObjectToTransform.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 25 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SharedTransformsToTransformList.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnAwake` | `void` | `public override` |
| 20 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 34 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SharedVariables/SharedTransformToGameObject.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 25 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SphereCollider/GetCenter.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SphereCollider/GetRadius.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SphereCollider/SetCenter.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/SphereCollider/SetRadius.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/String/BuildString.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/String/CompareTo.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/String/Format.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnAwake` | `void` | `public override` |
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/String/GetLength.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/String/GetRandomString.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 21 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/String/GetSubstring.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 26 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/String/IsNullOrEmpty.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 14 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/String/Replace.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 23 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/String/SetString.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Time/GetDeltaTime.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 17 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Time/GetRealtimeSinceStartup.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 17 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Time/GetTime.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 17 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Time/GetTimeScale.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 17 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Time/SetTimeScale.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 17 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Timeline/IsPaused.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnStart` | `void` | `public override` |
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 34 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Timeline/IsPlaying.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnStart` | `void` | `public override` |
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 34 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Timeline/Pause.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnStart` | `void` | `public override` |
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 36 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Timeline/Play.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnStart` | `void` | `public override` |
| 30 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 54 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Timeline/Resume.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 48 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Timeline/Stop.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnStart` | `void` | `public override` |
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 36 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/Find.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetAngleToTarget.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 23 | `-` | `OnStart` | `void` | `public override` |
| 32 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 56 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetChild.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `OnStart` | `void` | `public override` |
| 28 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetChildCount.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetEulerAngles.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetForwardVector.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetLocalEulerAngles.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetLocalPosition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetLocalRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetLocalScale.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetParent.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetPosition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetRightVector.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/GetUpVector.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnStart` | `void` | `public override` |
| 26 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 38 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/IsChildOf.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 35 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/LookAt.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnStart` | `void` | `public override` |
| 29 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 45 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/Rotate.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/RotateAround.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnStart` | `void` | `public override` |
| 29 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 41 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/SetEulerAngles.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/SetForwardVector.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/SetLocalEulerAngles.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/SetLocalPosition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/SetLocalRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/SetLocalScale.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/SetParent.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/SetPosition.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/SetRightVector.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/SetRotation.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/SetUpVector.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnStart` | `void` | `public override` |
| 25 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 37 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Transform/Translate.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnStart` | `void` | `public override` |
| 27 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 39 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/ClampMagnitude.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/Distance.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/Dot.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/GetMagnitude.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/GetRightVector.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/GetSqrMagnitude.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/GetUpVector.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/GetVector3.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/GetXY.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 24 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/Lerp.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 24 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/MoveTowards.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 24 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/Multiply.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/Normalize.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/Operator.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/SetValue.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector2/SetXY.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 28 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/Angle.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/ClampMagnitude.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/Distance.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/Dot.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/GetForwardVector.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/GetMagnitude.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/GetRightVector.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/GetSqrMagnitude.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/GetUpVector.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 18 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/GetVector2.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/GetXYZ.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 28 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/Lerp.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 24 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/MoveTowards.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 24 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/Multiply.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 22 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/Normalize.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 20 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/Operator.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 24 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 40 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/RotateTowards.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 26 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/SetValue.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 19 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Tasks/Unity/Vector3/SetXYZ.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `OnUpdate` | `TaskStatus` | `public override` |
| 33 | `-` | `OnReset` | `void` | `public override` |

## `Assets/Behavior Designer/Runtime/Variables/SharedAnimationCurve.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedBehaviour.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedBool.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedCollider.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedColor.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedFloat.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedGameObject.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedGameObjectList.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `SharedGameObjectList` | `-` | `public` |

## `Assets/Behavior Designer/Runtime/Variables/SharedHumanBodyBones.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedInt.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedLayerMask.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedMaterial.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedObject.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedObjectList.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedQuaternion.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedRect.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedString.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedTransform.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedTransformList.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `SharedTransformList` | `-` | `public` |

## `Assets/Behavior Designer/Runtime/Variables/SharedUInt.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedVector2.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedVector2Int.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedVector3.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedVector3Int.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Behavior Designer/Runtime/Variables/SharedVector4.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Plugins/DamageNumbersPro/Demo C#/DNP_ExampleGUI.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 27 | `-` | `Update` | `void` | `-` |
| 35 | `-` | `SpawnPopup` | `void` | `public` |

## `Assets/Plugins/DamageNumbersPro/Demo C#/DNP_ExampleMesh.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 27 | `-` | `Update` | `void` | `-` |
| 34 | `-` | `SpawnPopup` | `void` | `public` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_2DDemo.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `Start` | `void` | `-` |
| 22 | `-` | `Update` | `void` | `-` |
| 27 | `-` | `HandleShooting` | `void` | `-` |
| 41 | `-` | `Shoot` | `void` | `-` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_Camera.cs`

- Functions detected: 9

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `Awake` | `void` | `-` |
| 27 | `-` | `Update` | `void` | `-` |
| 44 | `-` | `ShowMouse` | `void` | `-` |
| 50 | `-` | `HideMouse` | `void` | `-` |
| 63 | `-` | `LateUpdate` | `void` | `-` |
| 68 | `-` | `HandleShooting` | `void` | `-` |
| 94 | `-` | `Shoot` | `void` | `-` |
| 163 | `-` | `HandleLooking` | `void` | `-` |
| 181 | `-` | `HandleMovement` | `void` | `-` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_Crosshair.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `Awake` | `void` | `-` |
| 26 | `-` | `FixedUpdate` | `void` | `-` |
| 49 | `-` | `HitEnemy` | `void` | `public` |
| 55 | `-` | `HitWall` | `void` | `public` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_CubeHighlight.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `Start` | `void` | `-` |
| 28 | `-` | `FixedUpdate` | `void` | `-` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_CubeSpawner.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `Start` | `void` | `-` |
| 16 | `-` | `SpawnCube` | `void` | `-` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_CursorHint.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `Start` | `void` | `-` |
| 15 | `-` | `FixedUpdate` | `void` | `-` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_DemoManager.cs`

- Functions detected: 7

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 23 | `-` | `Awake` | `void` | `-` |
| 57 | `-` | `Start` | `void` | `-` |
| 65 | `-` | `Update` | `void` | `-` |
| 118 | `-` | `SwitchScene` | `void` | `public` |
| 145 | `-` | `UpdateCurrent` | `void` | `-` |
| 153 | `-` | `GetCurrent` | `DamageNumber` | `public` |
| 158 | `-` | `GetSettings` | `DNP_PrefabSettings` | `public` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_Enemy.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 31 | `-` | `Start` | `void` | `-` |
| 64 | `-` | `FixedUpdate` | `void` | `-` |
| 128 | `-` | `Hurt` | `void` | `public` |
| 151 | `-` | `GetPelvis` | `Transform` | `public` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_FallingCube.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `Start` | `void` | `-` |
| 31 | `-` | `FixedUpdate` | `void` | `-` |
| 58 | `-` | `Break` | `void` | `public` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_GUI.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `Awake` | `void` | `-` |
| 22 | `-` | `Update` | `void` | `-` |
| 27 | `-` | `HandleShooting` | `void` | `-` |
| 43 | `-` | `Shoot` | `void` | `-` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_InputHandler.cs`

- Functions detected: 28

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `GetRight` | `bool` | `public static` |
| 24 | `-` | `GetLeft` | `bool` | `public static` |
| 35 | `-` | `GetBack` | `bool` | `public static` |
| 46 | `-` | `GetForward` | `bool` | `public static` |
| 57 | `-` | `GetJump` | `bool` | `public static` |
| 70 | `-` | `GetUp` | `bool` | `public static` |
| 81 | `-` | `GetDown` | `bool` | `public static` |
| 92 | `-` | `GetLeftClick` | `bool` | `public static` |
| 105 | `-` | `GetLeftHeld` | `bool` | `public static` |
| 116 | `-` | `GetRightClick` | `bool` | `public static` |
| 127 | `-` | `GetRightHeld` | `bool` | `public static` |
| 138 | `-` | `GetMouseDelta` | `Vector2` | `public static` |
| 149 | `-` | `GetMouseScroll` | `float` | `public static` |
| 160 | `-` | `GetEscape` | `bool` | `public static` |
| 180 | `-` | `GetRight` | `bool` | `public static` |
| 185 | `-` | `GetLeft` | `bool` | `public static` |
| 189 | `-` | `GetBack` | `bool` | `public static` |
| 193 | `-` | `GetForward` | `bool` | `public static` |
| 197 | `-` | `GetJump` | `bool` | `public static` |
| 203 | `-` | `GetUp` | `bool` | `public static` |
| 207 | `-` | `GetDown` | `bool` | `public static` |
| 211 | `-` | `GetLeftClick` | `bool` | `public static` |
| 217 | `-` | `GetLeftHeld` | `bool` | `public static` |
| 221 | `-` | `GetRightClick` | `bool` | `public static` |
| 225 | `-` | `GetRightHeld` | `bool` | `public static` |
| 229 | `-` | `GetMouseDelta` | `Vector2` | `public static` |
| 233 | `-` | `GetMouseScroll` | `float` | `public static` |
| 237 | `-` | `GetEscape` | `bool` | `public static` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_Player.cs`

- Functions detected: 9

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 34 | `-` | `Awake` | `void` | `-` |
| 45 | `-` | `GetCollider` | `CapsuleCollider2D` | `public` |
| 50 | `-` | `IncreaseIndex` | `void` | `-` |
| 92 | `-` | `Update` | `void` | `-` |
| 97 | `-` | `FixedUpdate` | `void` | `-` |
| 102 | `-` | `LateUpdate` | `void` | `-` |
| 107 | `-` | `HandleMovement` | `void` | `-` |
| 148 | `-` | `CheckGrounded` | `void` | `-` |
| 166 | `-` | `HandleAnimations` | `void` | `-` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_PrefabSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 15 | `-` | `Apply` | `void` | `public` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_SineFadeText.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `Awake` | `void` | `-` |
| 21 | `-` | `FixedUpdate` | `void` | `-` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_UIArea.cs`

- Functions detected: 6

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `Start` | `void` | `-` |
| 22 | `-` | `OnPointerEnter` | `void` | `public` |
| 27 | `-` | `OnPointerExit` | `void` | `public` |
| 35 | `-` | `GetRect` | `RectTransform` | `public static` |
| 39 | `-` | `CanSpawn` | `bool` | `public static` |
| 44 | `-` | `OnSpawn` | `void` | `public static` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_UIMove.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `Start` | `void` | `-` |
| 17 | `-` | `FixedUpdate` | `void` | `-` |

## `Assets/Plugins/DamageNumbersPro/Demo/Scripts/DNP_VillagerSpawner.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `Start` | `void` | `-` |
| 20 | `-` | `FixedUpdate` | `void` | `-` |
| 28 | `-` | `SpawnVillager` | `void` | `-` |

## `Assets/Plugins/DamageNumbersPro/Scripts/DamageNumberGUI.cs`

- Functions detected: 31

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnPreSpawn` | `void` | `protected override` |
| 38 | `-` | `OnStart` | `void` | `protected override` |
| 51 | `-` | `OnLateUpdate` | `void` | `protected override` |
| 67 | `-` | `OnStop` | `void` | `protected override` |
| 71 | `-` | `OnUpdate` | `void` | `protected override` |
| 76 | `-` | `OnAbsorb` | `void` | `protected override` |
| 80 | `-` | `OnTextUpdate` | `void` | `protected override` |
| 103 | `-` | `GetReferencesIfNecessary` | `void` | `public override` |
| 112 | `-` | `GetReferences` | `void` | `public override` |
| 124 | `-` | `GetTextMeshs` | `TMP_Text[]` | `public override` |
| 128 | `-` | `GetTextMesh` | `TMP_Text` | `public override` |
| 132 | `-` | `GetSharedMaterials` | `Material[]` | `public override` |
| 138 | `-` | `GetMaterials` | `Material[]` | `public override` |
| 142 | `-` | `GetSharedMaterial` | `Material` | `public override` |
| 146 | `-` | `GetMaterial` | `Material` | `public override` |
| 150 | `-` | `SetTextString` | `void` | `protected override` |
| 183 | `-` | `GetPosition` | `Vector3` | `public override` |
| 189 | `-` | `SetPosition` | `void` | `public override` |
| 194 | `-` | `SetAnchoredPosition` | `void` | `public override` |
| 208 | `-` | `SetAnchoredPosition` | `void` | `public override` |
| 223 | `-` | `SetLocalPositionA` | `void` | `protected override` |
| 228 | `-` | `SetLocalPositionB` | `void` | `protected override` |
| 232 | `-` | `GetPositionFactor` | `float` | `protected override` |
| 236 | `-` | `OnFade` | `void` | `protected override` |
| 248 | `-` | `UpdateRotationZ` | `void` | `protected override` |
| 253 | `-` | `CheckAndEnable3D` | `void` | `public override` |
| 257 | `-` | `IsMesh` | `bool` | `public override` |
| 261 | `-` | `GetUpVector` | `Vector3` | `public override` |
| 265 | `-` | `GetRightVector` | `Vector3` | `public override` |
| 269 | `-` | `GetFreshUpVector` | `Vector3` | `public override` |
| 273 | `-` | `GetFreshRightVector` | `Vector3` | `public override` |

## `Assets/Plugins/DamageNumbersPro/Scripts/DamageNumberMesh.cs`

- Functions detected: 6

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnStart` | `void` | `protected override` |
| 34 | `-` | `OnStop` | `void` | `protected override` |
| 38 | `-` | `OnUpdate` | `void` | `protected override` |
| 42 | `-` | `OnFade` | `void` | `protected override` |
| 46 | `-` | `OnTextUpdate` | `void` | `protected override` |
| 50 | `-` | `OnAbsorb` | `void` | `protected override` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Internal/DamageNumber.cs`

- Functions detected: 98

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 293 | `-` | `Start` | `void` | `-` |
| 307 | `-` | `Update` | `void` | `-` |
| 317 | `-` | `LateUpdate` | `void` | `-` |
| 327 | `-` | `UpdateDamageNumber` | `void` | `public` |
| 408 | `-` | `Spawn` | `DamageNumber` | `public` |
| 451 | `-` | `Spawn` | `DamageNumber` | `public` |
| 466 | `-` | `Spawn` | `DamageNumber` | `public` |
| 484 | `-` | `Spawn` | `DamageNumber` | `public` |
| 502 | `-` | `Spawn` | `DamageNumber` | `public` |
| 519 | `-` | `Spawn` | `DamageNumber` | `public` |
| 540 | `-` | `Spawn` | `DamageNumber` | `public` |
| 562 | `-` | `SpawnGUI` | `DamageNumber` | `public` |
| 578 | `-` | `SpawnGUI` | `DamageNumber` | `public` |
| 596 | `-` | `SpawnGUI` | `DamageNumber` | `public` |
| 615 | `-` | `SpawnGUI` | `DamageNumber` | `public` |
| 760 | `-` | `SetFollowedTarget` | `void` | `public` |
| 773 | `-` | `SetColor` | `void` | `public` |
| 784 | `-` | `SetGradientColor` | `void` | `public` |
| 796 | `-` | `SetRandomColor` | `void` | `public` |
| 800 | `-` | `SetRandomColor` | `void` | `public` |
| 804 | `-` | `SetGradientColor` | `void` | `public` |
| 814 | `-` | `SetFontMaterial` | `void` | `public` |
| 825 | `-` | `GetFontMaterial` | `TMP_FontAsset` | `public` |
| 841 | `-` | `SetScale` | `void` | `public` |
| 845 | `-` | `GetUpVector` | `Vector3` | `public virtual` |
| 850 | `-` | `GetRightVector` | `Vector3` | `public virtual` |
| 854 | `-` | `GetFreshUpVector` | `Vector3` | `public virtual` |
| 858 | `-` | `GetFreshRightVector` | `Vector3` | `public virtual` |
| 866 | `-` | `GetReferences` | `void` | `public virtual` |
| 892 | `-` | `GetReferencesIfNecessary` | `void` | `public virtual` |
| 900 | `-` | `FadeOut` | `void` | `public` |
| 910 | `-` | `FadeIn` | `void` | `public` |
| 919 | `-` | `GetTextMeshs` | `TMP_Text[]` | `public virtual` |
| 929 | `-` | `GetTextMesh` | `TMP_Text` | `public virtual` |
| 939 | `-` | `GetSharedMaterials` | `Material[]` | `public virtual` |
| 944 | `-` | `GetMaterials` | `Material[]` | `public virtual` |
| 948 | `-` | `GetSharedMaterial` | `Material` | `public virtual` |
| 952 | `-` | `GetMaterial` | `Material` | `public virtual` |
| 956 | `-` | `IsAlive` | `bool` | `public virtual` |
| 972 | `-` | `DestroyDNP` | `void` | `public` |
| 1011 | `-` | `CheckAndEnable3D` | `void` | `public virtual` |
| 1029 | `-` | `IsMesh` | `bool` | `public virtual` |
| 1038 | `-` | `NewMesh` | `GameObject` | `public static` |
| 1066 | `-` | `PrewarmPool` | `void` | `public` |
| 1100 | `-` | `Restart` | `void` | `protected` |
| 1139 | `-` | `Initialize` | `void` | `-` |
| 1249 | `-` | `PreparePooling` | `void` | `-` |
| 1276 | `-` | `PoolAvailable` | `bool` | `-` |
| 1289 | `-` | `SetPoolingID` | `void` | `-` |
| 1317 | `-` | `UpdateText` | `void` | `public` |
| 1483 | `-` | `SetTextString` | `void` | `protected virtual` |
| 1543 | `-` | `ProcessIntegers` | `string` | `-` |
| 1585 | `-` | `ApplyTextSettings` | `string` | `-` |
| 1659 | `-` | `ClearMeshs` | `void` | `private` |
| 1683 | `-` | `HandleFadeIn` | `void` | `-` |
| 1691 | `-` | `HandleFadeOut` | `void` | `-` |
| 1710 | `-` | `UpdateFade` | `void` | `-` |
| 1767 | `-` | `UpdateAlpha` | `void` | `public` |
| 1795 | `-` | `HandleFollowing` | `void` | `-` |
| 1827 | `-` | `HandleLerp` | `void` | `-` |
| 1835 | `-` | `HandleVelocity` | `void` | `-` |
| 1850 | `-` | `ApplyShake` | `Vector3` | `-` |
| 1868 | `-` | `GetTargetPosition` | `Vector3` | `public` |
| 1873 | `-` | `GetPosition` | `Vector3` | `public virtual` |
| 1878 | `-` | `SetLocalPositionA` | `void` | `protected virtual` |
| 1883 | `-` | `SetLocalPositionB` | `void` | `protected virtual` |
| 1888 | `-` | `SetPosition` | `void` | `public virtual` |
| 1893 | `-` | `SetToMousePosition` | `void` | `public virtual` |
| 1914 | `-` | `SetAnchoredPosition` | `void` | `public virtual` |
| 1932 | `-` | `SetAnchoredPosition` | `void` | `public virtual` |
| 1950 | `-` | `GetPositionFactor` | `float` | `protected virtual` |
| 1958 | `-` | `AddToDictionary` | `void` | `-` |
| 1987 | `-` | `RemoveFromDictionary` | `void` | `-` |
| 2004 | `-` | `HandleCombination` | `void` | `-` |
| 2051 | `-` | `TryCombination` | `void` | `-` |
| 2132 | `-` | `GetAbsorbed` | `void` | `-` |
| 2160 | `-` | `GiveNumber` | `void` | `-` |
| 2180 | `-` | `OnAbsorb` | `void` | `protected virtual` |
| 2192 | `-` | `HandleDestruction` | `void` | `-` |
| 2217 | `-` | `TryDestruction` | `void` | `-` |
| 2240 | `-` | `TryCollision` | `void` | `-` |
| 2249 | `-` | `TryCollision` | `void` | `-` |
| 2289 | `-` | `TryPush` | `void` | `-` |
| 2298 | `-` | `TryPush` | `void` | `-` |
| 2336 | `-` | `UpdateRotationZ` | `void` | `protected virtual` |
| 2341 | `-` | `SetRotationZ` | `void` | `protected` |
| 2348 | `-` | `HandleRotateOverTime` | `void` | `-` |
| 2353 | `-` | `UpdateScaleAnd3D` | `void` | `-` |
| 2479 | `-` | `OnFade` | `void` | `protected virtual` |
| 2488 | `-` | `OnTextUpdate` | `void` | `protected virtual` |
| 2497 | `-` | `OnUpdate` | `void` | `protected virtual` |
| 2507 | `-` | `OnStart` | `void` | `protected virtual` |
| 2515 | `-` | `OnStop` | `void` | `protected virtual` |
| 2524 | `-` | `OnLateUpdate` | `void` | `protected virtual` |
| 2529 | `-` | `OnPreSpawn` | `void` | `protected virtual` |
| 2543 | `-` | `OnDestroy` | `void` | `-` |
| 2567 | `-` | `OnSceneLoaded` | `void` | `-` |
| 2575 | `-` | `Reset` | `void` | `-` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Internal/DNPPreset.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 125 | `-` | `IsApplied` | `bool` | `public` |
| 268 | `-` | `Apply` | `void` | `public` |
| 405 | `-` | `Get` | `void` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Internal/DNPUpdater.cs`

- Functions detected: 5

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 30 | `-` | `Start` | `void` | `-` |
| 35 | `-` | `UpdatePopups` | `IEnumerator` | `-` |
| 86 | `-` | `RegisterPopup` | `void` | `public static` |
| 123 | `-` | `UnregisterPopup` | `void` | `public static` |
| 133 | `-` | `UpdateVectors` | `void` | `public static` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/CollisionSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `CollisionSettings` | `-` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/ColorByNumberSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `ColorByNumberSettings` | `-` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/CombinationSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `CombinationSettings` | `-` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/DestructionSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `DestructionSettings` | `-` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/DigitSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `DigitSettings` | `-` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/DistanceScalingSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `DistanceScalingSettings` | `-` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/FollowSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `FollowSettings` | `-` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/LerpSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `LerpSettings` | `-` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/PushSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `PushSettings` | `-` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/ScaleByNumberSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `ScaleByNumberSettings` | `-` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/ShakeSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `ShakeSettings` | `-` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/TextSettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 9 | `-` | `TextSettings` | `-` | `public` |

## `Assets/Plugins/DamageNumbersPro/Scripts/Settings/VelocitySettings.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 10 | `-` | `VelocitySettings` | `-` | `public` |

## `Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModuleAudio.cs`

- Functions detected: 12

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 63 | `-` | `DOComplete` | `int` | `public static` |
| 76 | `-` | `DOKill` | `int` | `public static` |
| 87 | `-` | `DOFlip` | `int` | `public static` |
| 97 | `-` | `DOGoto` | `int` | `public static` |
| 110 | `-` | `DOPause` | `int` | `public static` |
| 120 | `-` | `DOPlay` | `int` | `public static` |
| 130 | `-` | `DOPlayBackwards` | `int` | `public static` |
| 140 | `-` | `DOPlayForward` | `int` | `public static` |
| 150 | `-` | `DORestart` | `int` | `public static` |
| 160 | `-` | `DORewind` | `int` | `public static` |
| 170 | `-` | `DOSmoothRewind` | `int` | `public static` |
| 180 | `-` | `DOTogglePause` | `int` | `public static` |

## `Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModuleEPOOutline.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `DOKill` | `int` | `public static` |
| 74 | `-` | `DOKill` | `int` | `public static` |
| 79 | `-` | `DOKill` | `int` | `public static` |

## `Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModulePhysics.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 93 | `-` | `DOJump` | `Sequence` | `public static` |

## `Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModulePhysics2D.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 65 | `-` | `DOJump` | `Sequence` | `public static` |

## `Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModuleSprite.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 38 | `-` | `DOGradientColor` | `Sequence` | `public static` |
| 68 | `-` | `DOBlendableColor` | `Tweener` | `public static` |

## `Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModuleUI.cs`

- Functions detected: 12

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 95 | `-` | `DOGradientColor` | `Sequence` | `public static` |
| 336 | `-` | `DOPunchAnchorPos` | `Tweener` | `public static` |
| 352 | `-` | `DOShakeAnchorPos` | `Tweener` | `public static` |
| 368 | `-` | `DOShakeAnchorPos` | `Tweener` | `public static` |
| 385 | `-` | `DOJumpAnchorPos` | `Sequence` | `public static` |
| 429 | `-` | `DONormalizedPos` | `Tweener` | `public static` |
| 443 | `-` | `DOHorizontalNormalizedPos` | `Tweener` | `public static` |
| 452 | `-` | `DOVerticalNormalizedPos` | `Tweener` | `public static` |
| 550 | `-` | `DOBlendableColor` | `Tweener` | `public static` |
| 571 | `-` | `DOBlendableColor` | `Tweener` | `public static` |
| 592 | `-` | `DOBlendableColor` | `Tweener` | `public static` |
| 645 | `-` | `SwitchToRectTransform` | `Vector2` | `public static` |

## `Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModuleUnityVersion.cs`

- Functions detected: 20

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 22 | `-` | `DOGradientColor` | `Sequence` | `public static` |
| 46 | `-` | `DOGradientColor` | `Sequence` | `public static` |
| 75 | `-` | `WaitForCompletion` | `CustomYieldInstruction` | `public static` |
| 89 | `-` | `WaitForRewind` | `CustomYieldInstruction` | `public static` |
| 103 | `-` | `WaitForKill` | `CustomYieldInstruction` | `public static` |
| 117 | `-` | `WaitForElapsedLoops` | `CustomYieldInstruction` | `public static` |
| 132 | `-` | `WaitForPosition` | `CustomYieldInstruction` | `public static` |
| 148 | `-` | `WaitForStart` | `CustomYieldInstruction` | `public static` |
| 210 | `-` | `AsyncWaitForCompletion` | `System.Threading.Tasks.Task` | `public static async` |
| 224 | `-` | `AsyncWaitForRewind` | `System.Threading.Tasks.Task` | `public static async` |
| 238 | `-` | `AsyncWaitForKill` | `System.Threading.Tasks.Task` | `public static async` |
| 252 | `-` | `AsyncWaitForElapsedLoops` | `System.Threading.Tasks.Task` | `public static async` |
| 267 | `-` | `AsyncWaitForPosition` | `System.Threading.Tasks.Task` | `public static async` |
| 283 | `-` | `AsyncWaitForStart` | `System.Threading.Tasks.Task` | `public static async` |
| 319 | `-` | `WaitForCompletion` | `-` | `public` |
| 331 | `-` | `WaitForRewind` | `-` | `public` |
| 343 | `-` | `WaitForKill` | `-` | `public` |
| 356 | `-` | `WaitForElapsedLoops` | `-` | `public` |
| 370 | `-` | `WaitForPosition` | `-` | `public` |
| 383 | `-` | `WaitForStart` | `-` | `public` |

## `Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModuleUtils.cs`

- Functions detected: 5

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 38 | `-` | `Init` | `void` | `public static` |
| 56 | `-` | `Preserver` | `void` | `static` |
| 87 | `-` | `SetOrientationOnPath` | `void` | `public static` |
| 97 | `-` | `HasRigidbody2D` | `bool` | `public static` |
| 116 | `-` | `HasRigidbody` | `bool` | `public static` |

## `Assets/Plugins/Demigiant/DOTweenPro/DOTweenAnimation.cs`

- Functions detected: 43

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 64 | `-` | `Dispatch_OnReset` | `void` | `static` |
| 116 | `-` | `Awake` | `void` | `-` |
| 128 | `-` | `Start` | `void` | `-` |
| 136 | `-` | `Reset` | `void` | `-` |
| 141 | `-` | `OnDestroy` | `void` | `-` |
| 147 | `-` | `RewindThenRecreateTween` | `void` | `public` |
| 156 | `-` | `RewindThenRecreateTweenAndPlay` | `void` | `public` |
| 164 | `-` | `RecreateTween` | `void` | `public` |
| 169 | `-` | `RecreateTweenAndPlay` | `void` | `public` |
| 174 | `-` | `CreateTween` | `void` | `public` |
| 541 | `-` | `GetTweens` | `List<Tween>` | `public` |
| 557 | `-` | `SetAnimationTarget` | `void` | `public` |
| 584 | `-` | `DOPlay` | `void` | `public override` |
| 592 | `-` | `DOPlayBackwards` | `void` | `public override` |
| 600 | `-` | `DOPlayForward` | `void` | `public override` |
| 608 | `-` | `DOPause` | `void` | `public override` |
| 616 | `-` | `DOTogglePause` | `void` | `public override` |
| 624 | `-` | `DORewind` | `void` | `public override` |
| 639 | `-` | `DORestart` | `void` | `public override` |
| 645 | `-` | `DORestart` | `void` | `public override` |
| 659 | `-` | `DOComplete` | `void` | `public override` |
| 667 | `-` | `DOGotoAndPause` | `void` | `public override` |
| 674 | `-` | `DOGotoAndPlay` | `void` | `public override` |
| 680 | `-` | `DOGoto` | `void` | `-` |
| 694 | `-` | `DOKill` | `void` | `public override` |
| 705 | `-` | `DOPlayById` | `void` | `public` |
| 713 | `-` | `DOPlayAllById` | `void` | `public` |
| 720 | `-` | `DOPauseAllById` | `void` | `public` |
| 728 | `-` | `DOPlayBackwardsById` | `void` | `public` |
| 736 | `-` | `DOPlayBackwardsAllById` | `void` | `public` |
| 743 | `-` | `DOPlayForwardById` | `void` | `public` |
| 751 | `-` | `DOPlayForwardAllById` | `void` | `public` |
| 758 | `-` | `DOPlayNext` | `void` | `public` |
| 774 | `-` | `DORewindAndPlayNext` | `void` | `public` |
| 785 | `-` | `DORewindAllById` | `void` | `public` |
| 794 | `-` | `DORestartById` | `void` | `public` |
| 803 | `-` | `DORestartAllById` | `void` | `public` |
| 811 | `-` | `DOKillById` | `void` | `public` |
| 819 | `-` | `DOKillAllById` | `void` | `public` |
| 830 | `-` | `TypeToDOTargetType` | `TargetType` | `public static` |
| 849 | `-` | `CreateEditorPreview` | `Tween` | `public` |
| 885 | `-` | `ReEvaluateRelativeTween` | `void` | `-` |
| 906 | `-` | `IsSameOrSubclassOf` | `bool` | `public static` |

## `Assets/Plugins/Demigiant/DOTweenPro/DOTweenDeAudio.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Plugins/Demigiant/DOTweenPro/DOTweenDeUnityExtended.cs`

- Functions detected: 0
- No methods/functions detected by the lightweight parser.

## `Assets/Plugins/Demigiant/DOTweenPro/DOTweenProShortcuts.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `DOTweenProShortcuts` | `-` | `static` |
| 25 | `-` | `DOSpiral` | `Tweener` | `public static` |
| 57 | `-` | `DOSpiral` | `Tweener` | `public static` |

## `Assets/Plugins/Demigiant/DOTweenPro/DOTweenTextMeshPro.cs`

- Functions detected: 45

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 202 | `-` | `DOTweenTMPAnimator` | `-` | `public` |
| 240 | `-` | `DisposeInstanceFor` | `void` | `public static` |
| 250 | `-` | `Dispose` | `void` | `public` |
| 262 | `-` | `Refresh` | `void` | `public` |
| 287 | `-` | `Reset` | `void` | `public` |
| 296 | `-` | `OnTextChanged` | `void` | `-` |
| 302 | `-` | `ValidateChar` | `bool` | `-` |
| 324 | `-` | `ValidateSpan` | `bool` | `-` |
| 348 | `-` | `SkewSpanX` | `void` | `public` |
| 366 | `-` | `SkewSpanY` | `void` | `public` |
| 426 | `-` | `GetCharColor` | `Color` | `public` |
| 436 | `-` | `GetCharOffset` | `Vector3` | `public` |
| 446 | `-` | `GetCharRotation` | `Vector3` | `public` |
| 456 | `-` | `GetCharScale` | `Vector3` | `public` |
| 470 | `-` | `SetCharColor` | `void` | `public` |
| 484 | `-` | `SetCharOffset` | `void` | `public` |
| 498 | `-` | `SetCharRotation` | `void` | `public` |
| 512 | `-` | `SetCharScale` | `void` | `public` |
| 526 | `-` | `ShiftCharVertices` | `void` | `public` |
| 543 | `-` | `SkewCharX` | `float` | `public` |
| 560 | `-` | `SkewCharY` | `float` | `public` |
| 580 | `-` | `ResetVerticesShift` | `void` | `public` |
| 678 | `-` | `DOPunchCharOffset` | `Tweener` | `public` |
| 701 | `-` | `DOPunchCharRotation` | `Tweener` | `public` |
| 724 | `-` | `DOPunchCharScale` | `Tweener` | `public` |
| 738 | `-` | `DOPunchCharScale` | `Tweener` | `public` |
| 760 | `-` | `DOShakeCharOffset` | `Tweener` | `public` |
| 773 | `-` | `DOShakeCharOffset` | `Tweener` | `public` |
| 794 | `-` | `DOShakeCharRotation` | `Tweener` | `public` |
| 816 | `-` | `DOShakeCharScale` | `Tweener` | `public` |
| 829 | `-` | `DOShakeCharScale` | `Tweener` | `public` |
| 858 | `-` | `CharVertices` | `-` | `public` |
| 883 | `-` | `CharTransform` | `-` | `public` |
| 892 | `-` | `Refresh` | `void` | `public` |
| 906 | `-` | `ResetAll` | `void` | `public` |
| 912 | `-` | `ResetTransformationData` | `void` | `public` |
| 920 | `-` | `ResetGeometry` | `void` | `public` |
| 933 | `-` | `ResetColors` | `void` | `public` |
| 944 | `-` | `GetColor` | `Color32` | `public` |
| 949 | `-` | `GetVertices` | `CharVertices` | `public` |
| 957 | `-` | `UpdateAlpha` | `void` | `public` |
| 968 | `-` | `UpdateColor` | `void` | `public` |
| 978 | `-` | `UpdateGeometry` | `void` | `public` |
| 1005 | `-` | `ShiftVertices` | `void` | `public` |
| 1020 | `-` | `ResetVerticesShift` | `void` | `public` |

## `Assets/Plugins/Demigiant/DOTweenPro/DOTweenTk2d.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 80 | `-` | `DOGradientColor` | `Sequence` | `public static` |
| 202 | `-` | `DOGradientColor` | `Sequence` | `public static` |

## `Assets/Plugins/Sirenix/Odin Inspector/Modules/Unity.Mathematics/MathematicsDrawers.cs`

- Functions detected: 51

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 74 | `-` | `ProcessSelfAttributes` | `void` | `public override` |
| 79 | `-` | `ProcessChildMemberAttributes` | `void` | `public override` |
| 89 | `-` | `Initialize` | `void` | `protected override` |
| 112 | `-` | `Initialize` | `void` | `protected override` |
| 117 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 148 | `-` | `Initialize` | `void` | `protected override` |
| 153 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 187 | `-` | `Initialize` | `void` | `protected override` |
| 192 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 229 | `-` | `Initialize` | `void` | `protected override` |
| 234 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 261 | `-` | `PopulateGenericMenu` | `void` | `public` |
| 283 | `-` | `SetVector` | `void` | `private` |
| 294 | `-` | `NormalizeEntries` | `void` | `private` |
| 310 | `-` | `Initialize` | `void` | `protected override` |
| 315 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 343 | `-` | `PopulateGenericMenu` | `void` | `public` |
| 367 | `-` | `SetVector` | `void` | `private` |
| 378 | `-` | `NormalizeEntries` | `void` | `private` |
| 394 | `-` | `Initialize` | `void` | `protected override` |
| 399 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 428 | `-` | `PopulateGenericMenu` | `void` | `public` |
| 452 | `-` | `SetVector` | `void` | `private` |
| 463 | `-` | `NormalizeEntries` | `void` | `private` |
| 480 | `-` | `Initialize` | `void` | `protected override` |
| 485 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 512 | `-` | `PopulateGenericMenu` | `void` | `public` |
| 534 | `-` | `SetVector` | `void` | `private` |
| 545 | `-` | `NormalizeEntries` | `void` | `private` |
| 561 | `-` | `Initialize` | `void` | `protected override` |
| 566 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 594 | `-` | `PopulateGenericMenu` | `void` | `public` |
| 618 | `-` | `SetVector` | `void` | `private` |
| 629 | `-` | `NormalizeEntries` | `void` | `private` |
| 645 | `-` | `Initialize` | `void` | `protected override` |
| 650 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 679 | `-` | `PopulateGenericMenu` | `void` | `public` |
| 703 | `-` | `SetVector` | `void` | `private` |
| 714 | `-` | `NormalizeEntries` | `void` | `private` |
| 730 | `-` | `Initialize` | `void` | `protected override` |
| 735 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 755 | `-` | `Initialize` | `void` | `protected override` |
| 760 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 781 | `-` | `Initialize` | `void` | `protected override` |
| 786 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 808 | `-` | `Initialize` | `void` | `protected override` |
| 813 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 833 | `-` | `Initialize` | `void` | `protected override` |
| 838 | `-` | `DrawPropertyLayout` | `void` | `protected override` |
| 859 | `-` | `Initialize` | `void` | `protected override` |
| 864 | `-` | `DrawPropertyLayout` | `void` | `protected override` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark01.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 28 | `-` | `Start` | `IEnumerator` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark01_UGUI.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 33 | `-` | `Start` | `IEnumerator` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark02.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `Start` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark03.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `Awake` | `void` | `-` |
| 23 | `-` | `Start` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark04.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `Start` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/CameraController.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 51 | `-` | `Awake` | `void` | `-` |
| 66 | `-` | `Start` | `void` | `-` |
| 78 | `-` | `LateUpdate` | `void` | `-` |
| 123 | `-` | `GetPlayerInput` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/ChatController.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 13 | `-` | `OnEnable` | `void` | `-` |
| 18 | `-` | `OnDisable` | `void` | `-` |
| 23 | `-` | `AddToChatOutput` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/DropdownSample.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `OnButtonClick` | `void` | `public` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/EnvMapAnimator.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `Awake` | `void` | `-` |
| 19 | `-` | `Start` | `IEnumerator` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/ObjectSpin.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 25 | `-` | `Awake` | `void` | `-` |
| 35 | `-` | `Update` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/ShaderPropAnimator.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `Awake` | `void` | `-` |
| 26 | `-` | `Start` | `void` | `-` |
| 31 | `-` | `AnimateProperties` | `IEnumerator` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/SimpleScript.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 16 | `-` | `Start` | `void` | `-` |
| 49 | `-` | `Update` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/SkewTextExample.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `Awake` | `void` | `-` |
| 23 | `-` | `Start` | `void` | `-` |
| 29 | `-` | `CopyAnimationCurve` | `AnimationCurve` | `private` |
| 39 | `-` | `WarpText` | `IEnumerator` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TeleType.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 20 | `-` | `Awake` | `void` | `-` |
| 45 | `-` | `Start` | `IEnumerator` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TextConsoleSimulator.cs`

- Functions detected: 7

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 11 | `-` | `Awake` | `void` | `-` |
| 16 | `-` | `Start` | `void` | `-` |
| 23 | `-` | `OnEnable` | `void` | `-` |
| 30 | `-` | `OnDisable` | `void` | `-` |
| 35 | `-` | `ON_TEXT_CHANGED` | `void` | `-` |
| 42 | `-` | `RevealCharacters` | `IEnumerator` | `-` |
| 78 | `-` | `RevealWords` | `IEnumerator` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TextMeshProFloatingText.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 36 | `-` | `Awake` | `void` | `-` |
| 48 | `-` | `Start` | `void` | `-` |
| 96 | `-` | `DisplayTextMeshProFloatingText` | `IEnumerator` | `public` |
| 167 | `-` | `DisplayTextMeshFloatingText` | `IEnumerator` | `public` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TextMeshSpawner.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `Awake` | `void` | `-` |
| 22 | `-` | `Start` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_DigitValidator.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `Validate` | `char` | `public override` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_ExampleScript_01.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 24 | `-` | `Awake` | `void` | `-` |
| 52 | `-` | `Update` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_FrameRateCounter.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 26 | `-` | `Awake` | `void` | `-` |
| 63 | `-` | `Start` | `void` | `-` |
| 69 | `-` | `Update` | `void` | `-` |
| 102 | `-` | `Set_FrameCounter_Position` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_PhoneNumberValidator.cs`

- Functions detected: 1

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 14 | `-` | `Validate` | `char` | `public override` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_TextEventCheck.cs`

- Functions detected: 7

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `OnEnable` | `void` | `-` |
| 27 | `-` | `OnDisable` | `void` | `-` |
| 40 | `-` | `OnCharacterSelection` | `void` | `-` |
| 46 | `-` | `OnSpriteSelection` | `void` | `-` |
| 51 | `-` | `OnWordSelection` | `void` | `-` |
| 56 | `-` | `OnLineSelection` | `void` | `-` |
| 61 | `-` | `OnLinkSelection` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_TextEventHandler.cs`

- Functions detected: 9

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 98 | `-` | `Awake` | `void` | `-` |
| 121 | `-` | `LateUpdate` | `void` | `-` |
| 218 | `-` | `OnPointerEnter` | `void` | `public` |
| 224 | `-` | `OnPointerExit` | `void` | `public` |
| 230 | `-` | `SendOnCharacterSelection` | `void` | `private` |
| 237 | `-` | `SendOnSpriteSelection` | `void` | `private` |
| 243 | `-` | `SendOnWordSelection` | `void` | `private` |
| 249 | `-` | `SendOnLineSelection` | `void` | `private` |
| 255 | `-` | `SendOnLinkSelection` | `void` | `private` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_TextInfoDebugTool.cs`

- Functions detected: 14

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 33 | `-` | `OnDrawGizmos` | `void` | `-` |
| 94 | `-` | `DrawCharactersBounds` | `void` | `-` |
| 258 | `-` | `DrawWordBounds` | `void` | `-` |
| 362 | `-` | `DrawLinkBounds` | `void` | `-` |
| 466 | `-` | `DrawLineBounds` | `void` | `-` |
| 547 | `-` | `DrawBounds` | `void` | `-` |
| 562 | `-` | `DrawTextBounds` | `void` | `-` |
| 573 | `-` | `DrawRectangle` | `void` | `-` |
| 585 | `-` | `DrawDottedRectangle` | `void` | `-` |
| 594 | `-` | `DrawSolidRectangle` | `void` | `-` |
| 601 | `-` | `DrawSquare` | `void` | `-` |
| 615 | `-` | `DrawCrosshair` | `void` | `-` |
| 623 | `-` | `DrawRectangle` | `void` | `-` |
| 635 | `-` | `DrawDottedRectangle` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_TextSelector_A.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 19 | `-` | `Awake` | `void` | `-` |
| 28 | `-` | `LateUpdate` | `void` | `-` |
| 141 | `-` | `OnPointerEnter` | `void` | `public` |
| 148 | `-` | `OnPointerExit` | `void` | `public` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_TextSelector_B.cs`

- Functions detected: 10

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 36 | `-` | `Awake` | `void` | `-` |
| 56 | `-` | `OnEnable` | `void` | `-` |
| 63 | `-` | `OnDisable` | `void` | `-` |
| 69 | `-` | `ON_TEXT_CHANGED` | `void` | `-` |
| 79 | `-` | `LateUpdate` | `void` | `-` |
| 291 | `-` | `OnPointerEnter` | `void` | `public` |
| 298 | `-` | `OnPointerExit` | `void` | `public` |
| 305 | `-` | `OnPointerClick` | `void` | `public` |
| 448 | `-` | `OnPointerUp` | `void` | `public` |
| 454 | `-` | `RestoreCachedVertexAttributes` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_UiFrameRateCounter.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 25 | `-` | `Awake` | `void` | `-` |
| 50 | `-` | `Start` | `void` | `-` |
| 57 | `-` | `Update` | `void` | `-` |
| 88 | `-` | `Set_FrameCounter_Position` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/TMPro_InstructionOverlay.cs`

- Functions detected: 2

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 21 | `-` | `Awake` | `void` | `-` |
| 52 | `-` | `Set_FrameCounter_Position` | `void` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/VertexColorCycler.cs`

- Functions detected: 3

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 12 | `-` | `Awake` | `void` | `-` |
| 17 | `-` | `Start` | `void` | `-` |
| 23 | `-` | `AnimateVertexColors` | `IEnumerator` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/VertexJitter.cs`

- Functions detected: 6

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 27 | `-` | `Awake` | `void` | `-` |
| 32 | `-` | `OnEnable` | `void` | `-` |
| 38 | `-` | `OnDisable` | `void` | `-` |
| 43 | `-` | `Start` | `void` | `-` |
| 49 | `-` | `ON_TEXT_CHANGED` | `void` | `-` |
| 56 | `-` | `AnimateVertexColors` | `IEnumerator` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/VertexShakeA.cs`

- Functions detected: 6

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `Awake` | `void` | `-` |
| 24 | `-` | `OnEnable` | `void` | `-` |
| 30 | `-` | `OnDisable` | `void` | `-` |
| 35 | `-` | `Start` | `void` | `-` |
| 41 | `-` | `ON_TEXT_CHANGED` | `void` | `-` |
| 48 | `-` | `AnimateVertexColors` | `IEnumerator` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/VertexShakeB.cs`

- Functions detected: 6

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `Awake` | `void` | `-` |
| 23 | `-` | `OnEnable` | `void` | `-` |
| 29 | `-` | `OnDisable` | `void` | `-` |
| 34 | `-` | `Start` | `void` | `-` |
| 40 | `-` | `ON_TEXT_CHANGED` | `void` | `-` |
| 47 | `-` | `AnimateVertexColors` | `IEnumerator` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/VertexZoom.cs`

- Functions detected: 6

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 18 | `-` | `Awake` | `void` | `-` |
| 24 | `-` | `OnEnable` | `void` | `-` |
| 30 | `-` | `OnDisable` | `void` | `-` |
| 36 | `-` | `Start` | `void` | `-` |
| 42 | `-` | `ON_TEXT_CHANGED` | `void` | `-` |
| 49 | `-` | `AnimateVertexColors` | `IEnumerator` | `-` |

## `Assets/TextMesh Pro/Examples & Extras/Scripts/WarpTextExample.cs`

- Functions detected: 4

| Line | Type | Function | Return | Modifiers |
|------|------|----------|--------|-----------|
| 17 | `-` | `Awake` | `void` | `-` |
| 22 | `-` | `Start` | `void` | `-` |
| 28 | `-` | `CopyAnimationCurve` | `AnimationCurve` | `private` |
| 38 | `-` | `WarpText` | `IEnumerator` | `-` |

