using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class DungeonReleaseBuildPipeline
{
    public const string DevelopmentOutput = "Builds/Development/DungeonStory.exe";
    public const string ReleaseOutput = "Builds/Release/DungeonStory.exe";
    public const string HumanPlaytestOutput = "Builds/HumanPlaytest/DungeonStoryPlaytest.exe";
    public const string DevelopmentReportPath = "Temp/development-build-report.txt";
    public const string ReleaseReportPath = "Temp/release-build-report.txt";
    public const string HumanPlaytestReportPath = "Temp/human-playtest-build-report.txt";

    private const string CompanyName = "DungeonStory";
    private const string ProductName = "DungeonStory";
    private const string ApplicationIdentifier = "com.dungeonstory.game";
    private const string PlaytestProductName = "DungeonStoryPlaytest";
    private const string PlaytestApplicationIdentifier = "com.dungeonstory.game.playtest";
    private const string Version = "0.1.0";

    [MenuItem("DungeonStory/Build/Validate Release Configuration")]
    public static void ValidateReleaseConfigurationMenu()
    {
        bool valid = ValidateConfiguration(out string report);
        Debug.Log((valid ? "RELEASE_CONFIG PASS\n" : "RELEASE_CONFIG FAIL\n") + report);
    }

    [MenuItem("DungeonStory/Build/Windows Development")]
    public static void BuildDevelopment()
    {
        BuildWindows(development: true);
    }

    [MenuItem("DungeonStory/Build/Windows Release")]
    public static void BuildRelease()
    {
        BuildWindows(development: false);
    }

    [MenuItem("DungeonStory/Build/Windows Human Playtest")]
    public static void BuildHumanPlaytest()
    {
        PlayerSettingsSnapshot snapshot = PlayerSettingsSnapshot.Capture();
        try
        {
            ConfigurePlayerSettings();
            PlayerSettings.productName = PlaytestProductName;
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Standalone,
                PlaytestApplicationIdentifier);
            BuildWindowsPlayer(
                development: false,
                HumanPlaytestOutput,
                HumanPlaytestReportPath,
                "HumanPlaytest",
                PlaytestProductName,
                PlaytestApplicationIdentifier);
        }
        finally
        {
            snapshot.Restore();
            AssetDatabase.SaveAssets();
        }
    }

    public static bool ValidateConfiguration(out string report)
    {
        return ValidateConfiguration(ProductName, ApplicationIdentifier, out report);
    }

    private static bool ValidateConfiguration(
        string expectedProductName,
        string expectedApplicationIdentifier,
        out string report)
    {
        List<string> lines = new List<string>();
        string[] scenes = GetEnabledScenes();
        Check(lines, scenes.Length > 0, "SCENES", $"enabled={scenes.Length}");
        Check(lines, scenes.Contains("Assets/Scenes/SampleScene.unity", StringComparer.Ordinal),
            "GAME_SCENE", string.Join(",", scenes));
        Check(lines, PlayerSettings.companyName == CompanyName,
            "COMPANY", PlayerSettings.companyName);
        Check(lines, PlayerSettings.productName == expectedProductName,
            "PRODUCT", PlayerSettings.productName);
        Check(lines, PlayerSettings.bundleVersion == Version,
            "VERSION", PlayerSettings.bundleVersion);
        Check(lines,
            PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Standalone) == expectedApplicationIdentifier,
            "APP_ID",
            PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Standalone));
        Check(lines, PlayerSettings.defaultScreenWidth >= 1280 && PlayerSettings.defaultScreenHeight >= 720,
            "RESOLUTION", $"{PlayerSettings.defaultScreenWidth}x{PlayerSettings.defaultScreenHeight}");
        Check(lines, PlayerSettings.resizableWindow, "RESIZABLE", PlayerSettings.resizableWindow.ToString());
        report = string.Join(Environment.NewLine, lines);
        return lines.All(line => line.StartsWith("[PASS]", StringComparison.Ordinal));
    }

    private static void BuildWindows(bool development)
    {
        ConfigurePlayerSettings();
        string outputPath = development ? DevelopmentOutput : ReleaseOutput;
        string reportPath = development ? DevelopmentReportPath : ReleaseReportPath;
        BuildWindowsPlayer(
            development,
            outputPath,
            reportPath,
            development ? "Development" : "Release",
            ProductName,
            ApplicationIdentifier);
    }

    private static void BuildWindowsPlayer(
        bool development,
        string outputPath,
        string reportPath,
        string configurationName,
        string expectedProductName,
        string expectedApplicationIdentifier)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "Builds");
        Directory.CreateDirectory("Temp");

        if (!ValidateConfiguration(
                expectedProductName,
                expectedApplicationIdentifier,
                out string validation))
        {
            File.WriteAllText(reportPath, "BUILD_CONFIG FAIL\n" + validation);
            throw new BuildFailedException("DungeonStory release configuration is invalid.\n" + validation);
        }

        BuildOptions options = development
            ? BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.StrictMode
            : BuildOptions.CompressWithLz4HC | BuildOptions.StrictMode;
        BuildPlayerOptions player = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = options
        };

        BuildReport build = BuildPipeline.BuildPlayer(player);
        BuildSummary summary = build.summary;
        string output = string.Join(
            Environment.NewLine,
            summary.result == BuildResult.Succeeded ? "BUILD PASS" : "BUILD FAIL",
            $"configuration={configurationName}",
            $"result={summary.result}",
            $"errors={summary.totalErrors}",
            $"warnings={summary.totalWarnings}",
            $"sizeBytes={summary.totalSize}",
            $"duration={summary.totalTime}",
            $"output={Path.GetFullPath(outputPath)}",
            validation);
        File.WriteAllText(reportPath, output);

        if (summary.result != BuildResult.Succeeded || summary.totalErrors > 0)
        {
            throw new BuildFailedException(output);
        }

        Debug.Log(output);
    }

    private static void ConfigurePlayerSettings()
    {
        PlayerSettings.companyName = CompanyName;
        PlayerSettings.productName = ProductName;
        PlayerSettings.bundleVersion = Version;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, ApplicationIdentifier);
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
    }

    private static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes
            .Where(scene => scene != null && scene.enabled && !string.IsNullOrWhiteSpace(scene.path))
            .Select(scene => scene.path)
            .ToArray();
    }

    private static void Check(ICollection<string> lines, bool passed, string id, string detail)
    {
        lines.Add($"[{(passed ? "PASS" : "FAIL")}] {id} {detail}");
    }

    private sealed class PlayerSettingsSnapshot
    {
        private string companyName;
        private string productName;
        private string bundleVersion;
        private string applicationIdentifier;
        private int defaultScreenWidth;
        private int defaultScreenHeight;
        private FullScreenMode fullScreenMode;
        private bool resizableWindow;
        private bool runInBackground;

        public static PlayerSettingsSnapshot Capture()
        {
            return new PlayerSettingsSnapshot
            {
                companyName = PlayerSettings.companyName,
                productName = PlayerSettings.productName,
                bundleVersion = PlayerSettings.bundleVersion,
                applicationIdentifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Standalone),
                defaultScreenWidth = PlayerSettings.defaultScreenWidth,
                defaultScreenHeight = PlayerSettings.defaultScreenHeight,
                fullScreenMode = PlayerSettings.fullScreenMode,
                resizableWindow = PlayerSettings.resizableWindow,
                runInBackground = PlayerSettings.runInBackground
            };
        }

        public void Restore()
        {
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = productName;
            PlayerSettings.bundleVersion = bundleVersion;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, applicationIdentifier);
            PlayerSettings.defaultScreenWidth = defaultScreenWidth;
            PlayerSettings.defaultScreenHeight = defaultScreenHeight;
            PlayerSettings.fullScreenMode = fullScreenMode;
            PlayerSettings.resizableWindow = resizableWindow;
            PlayerSettings.runInBackground = runInBackground;
        }
    }
}
