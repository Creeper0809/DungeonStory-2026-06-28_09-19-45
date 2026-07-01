using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class OffenseWorldMapDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Offense/Run P3 World Map Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P3 offense world map scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("초기 가까운 원정 대상 공개", VerifyInitialNearbyTargets, errors);
        RunScenario("정찰 강화로 원정 대상 추가 공개", VerifyReconUpgradeRevealsMoreTargets, errors);
        RunScenario("원정 대상 정보 필드", VerifyTargetRequirementFields, errors);
        RunScenario("원정 대상 선택 이벤트", VerifyTargetSelectionEvent, errors);
        RunScenario("월드맵 패널 생성", VerifyPanelCreation, errors);

        if (errors.Count > 0)
        {
            foreach (string error in errors)
            {
                Debug.LogError(error);
            }

            return false;
        }

        if (logSuccess)
        {
            Debug.Log("P3 offense world map scenarios passed.");
        }

        return true;
    }

    private static void RunScenario(string name, Func<bool> scenario, List<string> errors)
    {
        try
        {
            if (scenario()) return;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        errors.Add(name);
    }

    private static bool VerifyInitialNearbyTargets()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        IReadOnlyList<OffenseTargetSnapshot> visible = scenario.Runtime.VisibleTargets;

        return scenario.Runtime.State.ReconLevel == 0
            && visible.Count >= 2
            && visible.All((target) => target.distance <= OffenseWorldMapService.GetScanRange(0))
            && visible.Any((target) => target.id == "food_farm")
            && visible.Any((target) => target.id == "merchant_road")
            && visible.All((target) => target.id != "mana_ruins");
    }

    private static bool VerifyReconUpgradeRevealsMoreTargets()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        int initialCount = scenario.Runtime.VisibleTargets.Count;
        bool upgraded = scenario.Runtime.TryUpgradeRecon(out string message);
        int afterFirstUpgrade = scenario.Runtime.VisibleTargets.Count;

        scenario.Runtime.TryUpgradeRecon(out _);
        int afterSecondUpgrade = scenario.Runtime.VisibleTargets.Count;

        return upgraded
            && message.Contains("정찰 Lv.1")
            && afterFirstUpgrade > initialCount
            && afterSecondUpgrade > afterFirstUpgrade
            && scenario.Runtime.VisibleTargets.Any((target) => target.id == "mana_ruins");
    }

    private static bool VerifyTargetRequirementFields()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        scenario.Runtime.TryUpgradeRecon(out _);

        OffenseTargetSnapshot target = scenario.Runtime.VisibleTargets.FirstOrDefault((candidate) => candidate.id == "old_armory");
        return target != null
            && target.danger > 0f
            && target.durationSeconds > 0f
            && target.requiredMembers >= 1
            && target.requiredPower > 0f
            && target.rewards.Length > 0
            && target.ToDetailText().Contains("필요 인력");
    }

    private static bool VerifyTargetSelectionEvent()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        using CountingSelectionListener selections = new CountingSelectionListener();

        bool selected = scenario.Runtime.TrySelectTarget("food_farm", out OffenseTargetSnapshot snapshot, out string message);
        return selected
            && snapshot != null
            && message.Contains(snapshot.title)
            && scenario.Runtime.State.SelectedTargetId == "food_farm"
            && selections.Count == 1
            && selections.LastTargetId == "food_farm";
    }

    private static bool VerifyPanelCreation()
    {
        using ScenarioRuntime scenario = new ScenarioRuntime();
        OffenseWorldMapPanel panel = scenario.Runtime.ShowWorldMap();

        bool valid = panel != null
            && panel.gameObject.activeSelf
            && Object.FindFirstObjectByType<Canvas>() != null;

        if (panel != null)
        {
            Object.DestroyImmediate(panel.transform.root.gameObject);
        }

        return valid;
    }

    private sealed class ScenarioRuntime : IDisposable
    {
        private readonly GameObject runtimeObject;

        public OffenseWorldMapRuntime Runtime { get; }

        public ScenarioRuntime()
        {
            runtimeObject = new GameObject("Offense World Map Scenario Runtime");
            Runtime = runtimeObject.AddComponent<OffenseWorldMapRuntime>();
            Runtime.StartWorldMap();
        }

        public void Dispose()
        {
            Object.DestroyImmediate(runtimeObject);
        }
    }

    private sealed class CountingSelectionListener :
        UtilEventListener<OffenseTargetSelectedEvent>,
        IDisposable
    {
        public int Count { get; private set; }
        public string LastTargetId { get; private set; }

        public CountingSelectionListener()
        {
            this.EventStartListening<OffenseTargetSelectedEvent>();
        }

        public void OnTriggerEvent(OffenseTargetSelectedEvent eventType)
        {
            if (eventType.target == null) return;

            Count++;
            LastTargetId = eventType.target.id;
        }

        public void Dispose()
        {
            this.EventStopListening<OffenseTargetSelectedEvent>();
        }
    }
}
