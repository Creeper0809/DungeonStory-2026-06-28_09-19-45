using System;
using System.IO;
using UnityEditor;

[InitializeOnLoad]
public static class RegularCustomerScenarioRequestRunner
{
    public const string RequestPath = "Temp/regular-customer-scenarios.request";
    public const string ReportPath = "Temp/regular-customer-scenarios-report.txt";

    static RegularCustomerScenarioRequestRunner()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }

    [MenuItem("DungeonStory/Debug/Recruitment/Request Regular Customer Scenarios")]
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
            success = RegularCustomerDebugScenarios.RunAll(false);
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
                success ? "REGULAR_CUSTOMER PASS" : "REGULAR_CUSTOMER FAIL",
                $"Generated: {DateTime.UtcNow:O}",
                string.IsNullOrWhiteSpace(error) ? "Error: none" : $"Error: {error}"));
    }
}
