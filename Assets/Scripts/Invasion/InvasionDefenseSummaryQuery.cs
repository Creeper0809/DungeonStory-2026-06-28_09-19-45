using System;
using System.Collections.Generic;

public readonly struct InvasionDefenseSummary
{
    public InvasionDefenseSummary(
        bool hasThreatRuntime,
        float currentThreat,
        InvasionThreatStage currentStage,
        float safetyRemaining,
        bool isCandidatePending,
        int activeIntruders,
        int defenseFacilities,
        int damagedFacilities,
        bool hasCurrentCombatReport)
    {
        HasThreatRuntime = hasThreatRuntime;
        CurrentThreat = currentThreat;
        CurrentStage = currentStage;
        SafetyRemaining = safetyRemaining;
        IsCandidatePending = isCandidatePending;
        ActiveIntruders = activeIntruders;
        DefenseFacilities = defenseFacilities;
        DamagedFacilities = damagedFacilities;
        HasCurrentCombatReport = hasCurrentCombatReport;
    }

    public bool HasThreatRuntime { get; }
    public float CurrentThreat { get; }
    public InvasionThreatStage CurrentStage { get; }
    public float SafetyRemaining { get; }
    public bool IsCandidatePending { get; }
    public int ActiveIntruders { get; }
    public int DefenseFacilities { get; }
    public int DamagedFacilities { get; }
    public bool HasCurrentCombatReport { get; }
}

public interface IInvasionDefenseSummaryService
{
    InvasionDefenseSummary Capture();
}

public sealed class InvasionDefenseSummaryService : IInvasionDefenseSummaryService
{
    private readonly IDungeonSceneComponentQuery sceneQuery;

    public InvasionDefenseSummaryService(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public InvasionDefenseSummary Capture()
    {
        InvasionThreatRuntime threat = sceneQuery.First<InvasionThreatRuntime>(includeInactive: true);
        InvasionDirectorRuntime director = sceneQuery.First<InvasionDirectorRuntime>(includeInactive: true);
        InvasionCombatReportRuntime reportRuntime = sceneQuery.First<InvasionCombatReportRuntime>(includeInactive: true);

        CountFacilities(sceneQuery.All<BuildableObject>(), out int defenseFacilities, out int damagedFacilities);

        return new InvasionDefenseSummary(
            threat != null,
            threat != null ? threat.CurrentThreat : 0f,
            threat != null ? threat.CurrentStage : default,
            threat != null ? threat.SafetyRemaining : 0f,
            threat != null && threat.IsCandidatePending,
            director != null ? director.ActiveIntruders.Count : 0,
            defenseFacilities,
            damagedFacilities,
            reportRuntime != null && reportRuntime.CurrentReport != null);
    }

    private static void CountFacilities(
        IReadOnlyList<BuildableObject> buildings,
        out int defenseFacilities,
        out int damagedFacilities)
    {
        defenseFacilities = 0;
        damagedFacilities = 0;

        if (buildings == null)
        {
            return;
        }

        for (int i = 0; i < buildings.Count; i++)
        {
            BuildableObject building = buildings[i];
            if (building == null)
            {
                continue;
            }

            if (building.BuildingData != null && building.BuildingData.Defense.IsDefenseFacility)
            {
                defenseFacilities++;
            }

            if (building.IsDamaged)
            {
                damagedFacilities++;
            }
        }
    }
}
