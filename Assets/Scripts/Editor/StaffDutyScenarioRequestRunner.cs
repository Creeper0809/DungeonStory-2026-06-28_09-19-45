using System;
using System.IO;
using UnityEditor;

[InitializeOnLoad]
public static class StaffDutyScenarioRequestRunner
{
    public const string RequestPath = "Temp/staff-duty-scenarios.request";
    public const string ReportPath = "Temp/staff-duty-scenarios-report.txt";

    static StaffDutyScenarioRequestRunner()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }

    [MenuItem("DungeonStory/Debug/Character/Request Staff Duty Scenarios")]
    public static void RequestRunFromMenu()
    {
        Directory.CreateDirectory("Temp");
        File.Delete(ReportPath);
        File.WriteAllText(RequestPath, DateTime.UtcNow.ToString("O"));
    }

    private static void OnEditorUpdate()
    {
        if (EditorApplication.isCompiling
            || EditorApplication.isPlayingOrWillChangePlaymode
            || !File.Exists(RequestPath))
        {
            return;
        }

        File.Delete(RequestPath);
        bool success = false;
        string error = string.Empty;
        try
        {
            success = StaffDutyDebugScenarios.RunAll(false);
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            UnityEngine.Debug.LogException(ex);
        }

        Directory.CreateDirectory("Temp");
        File.WriteAllText(
            ReportPath,
            string.Join(
                "\n",
                success ? "STAFF_DUTY PASS" : "STAFF_DUTY FAIL",
                $"Generated: {DateTime.UtcNow:O}",
                string.IsNullOrWhiteSpace(error) ? "Error: none" : $"Error: {error}"));
    }
}
