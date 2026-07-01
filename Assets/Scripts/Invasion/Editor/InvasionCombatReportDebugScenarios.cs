using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class InvasionCombatReportDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Invasion/Run P1 Combat Report Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P1 invasion combat report scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();
        RunScenario("전투 발동 피드와 결과 요약", VerifyCombatFeedbackAndSummary, errors);
        RunScenario("추천 대응 미표시", VerifySummaryHasNoRecommendation, errors);

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
            Debug.Log("P1 invasion combat report scenarios passed.");
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

    private static bool VerifyCombatFeedbackAndSummary()
    {
        using CombatReportScenarioWorld world = new CombatReportScenarioWorld();
        CountingCombatReportListener reports = new CountingCombatReportListener();
        CountingCombatFeedbackListener feedback = new CountingCombatFeedbackListener();
        CountingEventAlertRequestListener alerts = new CountingEventAlertRequestListener();

        world.StartInvasion();
        DefenseActivationReport spikeReport = world.CreateDefenseReport(
            "가시 함정",
            DefenseAttackConcept.Physical,
            18f,
            0f,
            "가시 피해");
        world.TriggerDefense(spikeReport);

        DefenseActivationReport iceReport = world.CreateDefenseReport(
            "냉기 분사구",
            DefenseAttackConcept.Ice,
            8f,
            1.5f,
            "감속");
        world.TriggerDefense(iceReport);

        BuildableObject damaged = world.CreateFacility("저가 음식점", DefenseAttackConcept.None);
        damaged.SetDamaged(true);
        world.TriggerFacilityDamaged(damaged);
        world.Resolve(true, 1f);

        InvasionCombatReport report = reports.LastReport;
        EventAlertRequest alert = alerts.LastRequest;
        string detail = report != null ? report.ToDetailText() : string.Empty;

        bool valid = reports.Count == 1
            && feedback.Count == 2
            && alert != null
            && alert.Title == "침입 결과"
            && alert.Importance == EventAlertImportance.Medium
            && detail.Contains("가장 많은 피해를 준 시설: 가시 함정")
            && detail.Contains("가장 오래 지연시킨 시설: 냉기 분사구")
            && detail.Contains("피해를 입은 시설: 저가 음식점")
            && detail.Contains("파손 시설: 저가 음식점")
            && detail.Contains("획득한 관찰 정보")
            && detail.Contains("전투 중 발동");

        reports.Dispose();
        feedback.Dispose();
        alerts.Dispose();
        return valid;
    }

    private static bool VerifySummaryHasNoRecommendation()
    {
        using CombatReportScenarioWorld world = new CombatReportScenarioWorld();
        CountingCombatReportListener reports = new CountingCombatReportListener();

        world.StartInvasion();
        DefenseActivationReport guardReport = world.CreateDefenseReport(
            "경비실",
            DefenseAttackConcept.Guard,
            12f,
            0f,
            "경비 교전");
        world.TriggerDefense(guardReport);
        world.TriggerFinalCombat();
        world.Resolve(false, 5f);

        string detail = reports.LastReport != null ? reports.LastReport.ToDetailText() : string.Empty;
        bool valid = detail.Contains("방어 결과: 방어 실패")
            && detail.Contains("결정적 방어")
            && !detail.Contains("추천")
            && !detail.Contains("건설하세요")
            && !detail.Contains("연구하세요");

        reports.Dispose();
        return valid;
    }

    private sealed class CombatReportScenarioWorld : IDisposable
    {
        private readonly List<Object> objects = new List<Object>();

        public CombatReportScenarioWorld()
        {
            GameObject runtimeObject = new GameObject("CombatReportRuntime_Test");
            Runtime = runtimeObject.AddComponent<InvasionCombatReportRuntime>();
            objects.Add(runtimeObject);

            Intruder = CreateCharacter("Test Intruder");
            Owner = CreateCharacter("Test Owner");
        }

        public InvasionCombatReportRuntime Runtime { get; }
        public Character Intruder { get; }
        public Character Owner { get; }

        public void StartInvasion()
        {
            InvasionThreatSnapshot snapshot = new InvasionThreatSnapshot(
                100f,
                InvasionThreatStage.Candidate,
                new InvasionThreatFactors(3f, 2f, 1f, 0f),
                0f,
                0f);
            Runtime.OnTriggerEvent(new InvasionStartedEvent(snapshot));
            Runtime.OnTriggerEvent(new InvasionSpawnedEvent(Intruder, snapshot));
        }

        public void TriggerDefense(DefenseActivationReport report)
        {
            Runtime.OnTriggerEvent(new DefenseFacilityTriggeredEvent(report));
        }

        public void TriggerFacilityDamaged(BuildableObject facility)
        {
            Runtime.OnTriggerEvent(new InvasionFacilityDamagedEvent(Intruder, facility));
        }

        public void TriggerFinalCombat()
        {
            Runtime.OnTriggerEvent(new InvasionFinalCombatStartedEvent(Intruder, Owner));
        }

        public void Resolve(bool defended, float residualRisk)
        {
            Runtime.OnTriggerEvent(new InvasionResolvedEvent(defended, residualRisk));
        }

        public DefenseActivationReport CreateDefenseReport(
            string buildingName,
            DefenseAttackConcept concept,
            float damage,
            float delay,
            string effectTag)
        {
            DefenseFacility facility = CreateFacility(buildingName, concept) as DefenseFacility;
            DefenseActivationReport report = new DefenseActivationReport(
                facility,
                Intruder,
                DefenseTriggerTiming.OnEnter);
            report.AddDamage(damage);
            report.AddMovementDelay(delay);
            report.AddEffectTag(effectTag);
            return report;
        }

        public BuildableObject CreateFacility(string buildingName, DefenseAttackConcept concept)
        {
            BuildingSO data = ScriptableObject.CreateInstance<BuildingSO>();
            data.objectName = buildingName;
            data.width = 1;
            data.height = 1;
            data.layer = GridLayer.Building;
            data.category = BuildingCategory.Special;
            data.type = typeof(DefenseFacility);
            data.facility = new FacilityData
            {
                supportedWorkTypes = FacilityWorkType.Repair,
                disabledWhenDamaged = true
            };
            data.defense = new DefenseFacilityData
            {
                enabled = concept != DefenseAttackConcept.None,
                concept = concept,
                triggerTimings = DefenseTriggerTiming.OnEnter,
                targetRule = DefenseTargetRule.EnteringIntruder,
                combatLogText = buildingName
            };
            objects.Add(data);

            GameObject buildingObject = new GameObject(buildingName);
            DefenseFacility facility = buildingObject.AddComponent<DefenseFacility>();
            facility.Initialization(data, Vector2Int.zero);
            objects.Add(buildingObject);
            return facility;
        }

        public void Dispose()
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i] != null)
                {
                    Object.DestroyImmediate(objects[i]);
                }
            }
        }

        private Character CreateCharacter(string name)
        {
            GameObject characterObject = new GameObject(name);
            Character character = characterObject.AddComponent<Character>();
            character.SetLifecycleState(Character.LifecycleState.Active);
            objects.Add(characterObject);
            return character;
        }
    }

    private sealed class CountingCombatReportListener : UtilEventListener<InvasionCombatReportReadyEvent>, IDisposable
    {
        public int Count { get; private set; }
        public InvasionCombatReport LastReport { get; private set; }

        public CountingCombatReportListener()
        {
            this.EventStartListening<InvasionCombatReportReadyEvent>();
        }

        public void OnTriggerEvent(InvasionCombatReportReadyEvent eventType)
        {
            Count++;
            LastReport = eventType.report;
        }

        public void Dispose()
        {
            this.EventStopListening<InvasionCombatReportReadyEvent>();
        }
    }

    private sealed class CountingCombatFeedbackListener : UtilEventListener<InvasionCombatFeedbackEvent>, IDisposable
    {
        public int Count { get; private set; }

        public CountingCombatFeedbackListener()
        {
            this.EventStartListening<InvasionCombatFeedbackEvent>();
        }

        public void OnTriggerEvent(InvasionCombatFeedbackEvent eventType)
        {
            if (!string.IsNullOrWhiteSpace(eventType.message))
            {
                Count++;
            }
        }

        public void Dispose()
        {
            this.EventStopListening<InvasionCombatFeedbackEvent>();
        }
    }

    private sealed class CountingEventAlertRequestListener : UtilEventListener<EventAlertRequestedEvent>, IDisposable
    {
        public EventAlertRequest LastRequest { get; private set; }

        public CountingEventAlertRequestListener()
        {
            this.EventStartListening<EventAlertRequestedEvent>();
        }

        public void OnTriggerEvent(EventAlertRequestedEvent eventType)
        {
            LastRequest = eventType.request;
        }

        public void Dispose()
        {
            this.EventStopListening<EventAlertRequestedEvent>();
        }
    }
}
