using UnityEngine;

public sealed class DungeonSceneRuntimeReferences
{
    public DungeonSceneRuntimeReferences(
        UIManager uiManager,
        OperatingDaySettlementRuntime settlement,
        EventAlertRuntime alerts,
        RunVariableRuntime runVariables,
        Canvas canvas)
    {
        UIManager = uiManager;
        Settlement = settlement;
        Alerts = alerts;
        RunVariables = runVariables;
        Canvas = canvas;
    }

    public UIManager UIManager { get; }
    public OperatingDaySettlementRuntime Settlement { get; }
    public EventAlertRuntime Alerts { get; }
    public RunVariableRuntime RunVariables { get; }
    public Canvas Canvas { get; }
}
