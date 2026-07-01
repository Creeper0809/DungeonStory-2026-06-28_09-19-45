using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class StaffDiscontentDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Character/Run P2 Staff Discontent Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P2 staff discontent scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("만족도 낮음 상태", VerifyLowSatisfactionStage, errors);
        RunScenario("1단계 효율 저하", VerifyEfficiencyDropMultiplier, errors);
        RunScenario("2단계 태업 작업 차단", VerifyWorkDisruptionBlocksWork, errors);
        RunScenario("3단계 이탈 영구 손실", VerifyDeparturePermanentLoss, errors);
        RunScenario("4단계 국지 반란", VerifyLocalRebellionPermanentLoss, errors);
        RunScenario("반란 장기 방치 사장 위협", VerifyOwnerThreatEscalation, errors);

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
            Debug.Log("P2 staff discontent scenarios passed.");
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

    private static bool VerifyLowSatisfactionStage()
    {
        using ScenarioRuntime runtime = new ScenarioRuntime();
        Character staff = CreateStaff(201, "Low Satisfaction Staff", 45f);

        StaffDiscontentRecord record = runtime.Runtime.ProcessStaff(staff, out StaffDiscontentOutcome outcome);
        bool valid = record != null
            && outcome == StaffDiscontentOutcome.Warning
            && record.Stage == StaffDiscontentStage.LowSatisfaction
            && !record.IsPermanentLoss;

        DestroyStaff(staff);
        return valid;
    }

    private static bool VerifyEfficiencyDropMultiplier()
    {
        using ScenarioRuntime runtime = new ScenarioRuntime();
        Character staff = CreateStaff(202, "Efficiency Drop Staff", 34f);

        StaffDiscontentRecord record = runtime.Runtime.ProcessStaff(staff, out StaffDiscontentOutcome outcome);
        float multiplier = runtime.Runtime.GetWorkEfficiencyMultiplier(staff);
        bool valid = record != null
            && outcome == StaffDiscontentOutcome.EfficiencyPenalty
            && record.Stage == StaffDiscontentStage.EfficiencyDrop
            && multiplier < 1f
            && multiplier > 0f;

        DestroyStaff(staff);
        return valid;
    }

    private static bool VerifyWorkDisruptionBlocksWork()
    {
        using ScenarioRuntime runtime = new ScenarioRuntime();
        Character staff = CreateStaff(203, "Work Disruption Staff", 20f);
        AbilityWork work = staff.GetAbility<AbilityWork>();

        StaffDiscontentRecord record = runtime.Runtime.ProcessStaff(staff, out StaffDiscontentOutcome outcome);
        bool canWork = work.CanStartWorkAction();
        bool valid = record != null
            && outcome == StaffDiscontentOutcome.WorkDisruption
            && record.Stage == StaffDiscontentStage.WorkDisruption
            && !canWork
            && work.IsOffDuty;

        DestroyStaff(staff);
        return valid;
    }

    private static bool VerifyDeparturePermanentLoss()
    {
        using ScenarioRuntime runtime = new ScenarioRuntime();
        Character staff = CreateStaff(204, "Departing Staff", 10f);

        StaffDiscontentRecord record = runtime.Runtime.ProcessStaff(staff, out StaffDiscontentOutcome outcome);
        bool valid = record != null
            && outcome == StaffDiscontentOutcome.PermanentDeparture
            && record.Stage == StaffDiscontentStage.Departure
            && record.IsPermanentLoss
            && record.IsDeparted
            && staff.CurrentLifecycleState == Character.LifecycleState.Despawned;

        DestroyStaff(staff);
        return valid;
    }

    private static bool VerifyLocalRebellionPermanentLoss()
    {
        using ScenarioRuntime runtime = new ScenarioRuntime();
        Character staff = CreateStaff(205, "Rebel Staff", 5f);

        StaffDiscontentRecord record = runtime.Runtime.ProcessStaff(staff, out StaffDiscontentOutcome outcome);
        bool valid = record != null
            && outcome == StaffDiscontentOutcome.LocalRebellion
            && record.Stage == StaffDiscontentStage.LocalRebellion
            && record.IsPermanentLoss
            && record.IsInLocalRebellion
            && !record.IsDeparted;

        DestroyStaff(staff);
        return valid;
    }

    private static bool VerifyOwnerThreatEscalation()
    {
        using ScenarioRuntime runtime = new ScenarioRuntime();
        Character staff = CreateStaff(206, "Escalating Rebel Staff", 5f);

        StaffDiscontentRecord record = runtime.Runtime.ProcessStaff(staff, out StaffDiscontentOutcome firstOutcome);
        record = runtime.Runtime.ProcessStaff(staff, out StaffDiscontentOutcome secondOutcome);
        bool valid = firstOutcome == StaffDiscontentOutcome.LocalRebellion
            && secondOutcome == StaffDiscontentOutcome.OwnerThreat
            && record != null
            && record.IsOwnerThreat;

        DestroyStaff(staff);
        return valid;
    }

    private static Character CreateStaff(int id, string name, float mood)
    {
        CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
        data.id = id;
        data.characterType = CharacterType.NPC;
        data.role = CharacterRole.Regular;
        data.characterName = name;
        data.speciesTag = "Orc";
        data.defaultWorkPriorities = WorkPriorityProfile.CreateDefault();

        GameObject obj = new GameObject(name);
        obj.AddComponent<SpriteRenderer>();
        obj.AddComponent<Character>();
        obj.AddComponent<AbilityMove>();
        obj.AddComponent<AbilityShopping>();
        obj.AddComponent<AbilityWork>();
        obj.AddComponent<AIBrain>();

        Character character = obj.GetComponent<Character>();
        typeof(Character)
            .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(character, null);
        character.RefreshAbilityCache();
        character.Initialization(data);
        character.SetLifecycleState(Character.LifecycleState.Active);
        character.stats[Character.Condition.MOOD] = mood;
        character.stats[Character.Condition.SLEEP] = 80f;
        character.stats[Character.Condition.HUNGER] = 80f;
        character.stats[Character.Condition.FUN] = 80f;
        return character;
    }

    private static void DestroyStaff(Character staff)
    {
        if (staff == null) return;

        if (staff.data != null)
        {
            Object.DestroyImmediate(staff.data);
        }

        Object.DestroyImmediate(staff.gameObject);
    }

    private sealed class ScenarioRuntime : IDisposable
    {
        private readonly GameObject runtimeObject;

        public ScenarioRuntime()
        {
            runtimeObject = new GameObject("StaffDiscontentRuntime_Test");
            Runtime = runtimeObject.AddComponent<StaffDiscontentRuntime>();
        }

        public StaffDiscontentRuntime Runtime { get; }

        public void Dispose()
        {
            Object.DestroyImmediate(runtimeObject);
        }
    }
}
