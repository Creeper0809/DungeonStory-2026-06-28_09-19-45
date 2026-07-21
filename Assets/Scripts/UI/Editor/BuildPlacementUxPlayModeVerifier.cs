using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class BuildPlacementUxPlayModeVerifier
{
    public const string ReportPath = "Temp/build-placement-ux-report.txt";
    public const string CapturePath = "Temp/build-placement-ux.png";
    public const string OpenCatalogCapturePath = "Temp/phase66-compact-build-catalog.png";

    [MenuItem("DungeonStory/Debug/QA/Run Build Placement UX Verification")]
    public static void RunFromMenu()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogError("Build placement UX verification requires PlayMode.");
            return;
        }

        if (UnityEngine.Object.FindFirstObjectByType<BuildPlacementUxPlayModeVerificationRunner>() != null)
        {
            Debug.LogWarning("Build placement UX verification is already running.");
            return;
        }

        EditorApplication.ExecuteMenuItem("Window/General/Game");
        new GameObject("Build Placement UX Verification Runner")
            .AddComponent<BuildPlacementUxPlayModeVerificationRunner>();
    }
}

public sealed class BuildPlacementUxPlayModeVerificationRunner : MonoBehaviour
{
    private readonly List<string> report = new List<string>();
    private readonly List<string> failures = new List<string>();
    private readonly List<string> capturedErrors = new List<string>();
    private readonly List<string> capturedWarnings = new List<string>();

    private IEnumerator Start()
    {
        Directory.CreateDirectory("Temp");
        Application.logMessageReceived += OnLogMessageReceived;
        yield return null;
        yield return null;
        yield return null;

        UITabManager tabManager = UnityEngine.Object.FindFirstObjectByType<UITabManager>();
        GridConstructTab constructTab = UnityEngine.Object.FindFirstObjectByType<GridConstructTab>(FindObjectsInactive.Include);
        GridUIManager gridUi = UnityEngine.Object.FindFirstObjectByType<GridUIManager>(FindObjectsInactive.Include);
        DungeonStoryGridBuildingController controller = UnityEngine.Object.FindFirstObjectByType<DungeonStoryGridBuildingController>();
        DungeonStoryGridGhostPresenter ghostPresenter = UnityEngine.Object.FindFirstObjectByType<DungeonStoryGridGhostPresenter>();

        Check(tabManager != null, "TAB_MANAGER", "bottom tab manager resolved");
        Check(constructTab != null, "CONSTRUCT_TAB", "build catalog resolved");
        Check(gridUi != null, "GRID_UI", "placement grid UI resolved");
        Check(controller != null, "BUILD_CONTROLLER", "building controller resolved");
        Check(ghostPresenter != null, "GHOST_PRESENTER", "placement ghost presenter resolved");
        if (tabManager == null || constructTab == null || gridUi == null || controller == null || ghostPresenter == null)
        {
            Finish();
            yield break;
        }

        controller.SetGridModeNone();
        yield return null;

        Button buildTabButton = FindVisibleButtonByLabel("\uAC74\uCD95");
        Check(buildTabButton != null, "BUILD_TAB_BUTTON", "visible build tab button resolved");
        PressButton(buildTabButton);
        yield return null;
        yield return null;
        Check(constructTab.gameObject.activeInHierarchy, "CATALOG_OPENED_BY_POINTER", "build catalog opened from its UI button");
        CheckCompactCatalogRoot(constructTab);

        Button categoryButton = constructTab.GetComponentsInChildren<Button>(false)
            .FirstOrDefault(button => button != null
                && button.GetComponent<UIBuildingSelectButton>() == null
                && button.GetComponentsInChildren<TMP_Text>(true)
                    .Any(text => text != null && text.text == "\uBCBD/\uBB38"));
        Check(categoryButton != null, "CATEGORY_BUTTON", "visible build category button resolved");
        PressButton(categoryButton);
        yield return null;
        yield return null;
        CheckCompactCatalogContent(constructTab);
        yield return new WaitForEndOfFrame();
        CaptureScreen(BuildPlacementUxPlayModeVerifier.OpenCatalogCapturePath);

        UIBuildingSelectButton selection = constructTab
            .GetComponentsInChildren<UIBuildingSelectButton>(false)
            .FirstOrDefault(button => button != null && button.GetComponent<Button>()?.interactable == true);
        Check(selection != null, "BUILD_ITEM_BUTTON", "visible build item button resolved");
        PressButton(selection != null ? selection.GetComponent<Button>() : null);
        yield return null;
        yield return new WaitForEndOfFrame();

        GridGhostObject ghost = ghostPresenter.GetComponent<GridGhostObject>();
        Check(controller.GridSystem.Mode == GridMode.Build, "PLACEMENT_MODE_PRESERVED", $"mode={controller.GridSystem.Mode}");
        Check(controller.SelectedBuilding != null, "BUILD_SELECTION_PRESERVED", controller.SelectedBuilding?.objectName ?? "<none>");
        Check(!constructTab.gameObject.activeSelf, "CATALOG_COLLAPSED", $"active={constructTab.gameObject.activeSelf}");
        Check(constructTab.selectButtonPanelList.All(panel => panel == null || !panel.gameObject.activeSelf),
            "CATEGORY_PANELS_COLLAPSED", "all category panels are hidden");
        Check(gridUi.IsGridVisible && gridUi.BuildableCellCount > 0,
            "PLACEMENT_GRID_VISIBLE", $"visible={gridUi.IsGridVisible}; buildable={gridUi.BuildableCellCount}");
        Check(ghost != null && !ghost.IsHidden, "PLACEMENT_GHOST_VISIBLE", $"ghostHidden={ghost?.IsHidden}");
        Check(!IsCatalogBlockingScreenCenter(constructTab), "WORLD_CENTER_NOT_BLOCKED", "collapsed catalog has no center-screen UI hit");

        CaptureScreen(BuildPlacementUxPlayModeVerifier.CapturePath);

        Button buildingTabButton = FindVisibleButtonByLabel("\uAC74\uBB3C");
        Check(buildingTabButton != null, "OTHER_TAB_BUTTON", "visible building-management tab resolved");
        PressButton(buildingTabButton);
        yield return null;
        Check(controller.GridSystem.Mode == GridMode.None && controller.SelectedBuilding == null,
            "OTHER_TAB_CANCELS_PLACEMENT",
            $"mode={controller.GridSystem.Mode}; selection={controller.SelectedBuilding?.objectName ?? "<none>"}");

        controller.SetGridModeNone();
        Application.logMessageReceived -= OnLogMessageReceived;
        Finish();
        Destroy(gameObject);
    }

    private static Button FindVisibleButtonByLabel(string label)
    {
        return UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(button => button != null
                && button.interactable
                && button.GetComponentsInChildren<TMP_Text>(true).Any(text => text != null && text.text == label));
    }

    private static void PressButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            button = PointerEventData.InputButton.Left
        };
        button.OnPointerClick(eventData);
    }

    private static bool IsCatalogBlockingScreenCenter(GridConstructTab constructTab)
    {
        if (EventSystem.current == null)
        {
            return true;
        }

        PointerEventData pointer = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);
        return results.Any(result => result.gameObject != null
            && result.gameObject.transform.IsChildOf(constructTab.transform));
    }

    private void CheckCompactCatalogRoot(GridConstructTab constructTab)
    {
        RectTransform root = constructTab.transform as RectTransform;
        Check(root != null && root.rect.height <= 150f,
            "CATALOG_COMPACT_HEIGHT",
            $"height={root?.rect.height ?? -1f:0.##}");
        Check(root != null && root.rect.height >= 110f,
            "CATALOG_CONTROLS_HAVE_ROOM",
            $"height={root?.rect.height ?? -1f:0.##}");
    }

    private void CheckCompactCatalogContent(GridConstructTab constructTab)
    {
        RectTransform root = constructTab.transform as RectTransform;
        Transform categoryRootTransform = constructTab.transform.childCount > 0
            ? constructTab.transform.GetChild(0)
            : null;
        RectTransform categoryRoot = categoryRootTransform as RectTransform;
        GridLayoutGroup categoryLayout = categoryRootTransform != null
            ? categoryRootTransform.GetComponent<GridLayoutGroup>()
            : null;
        UITab visiblePanel = constructTab.selectButtonPanelList
            .FirstOrDefault(panel => panel != null && panel.gameObject.activeInHierarchy);
        RectTransform visiblePanelRect = visiblePanel != null ? visiblePanel.transform as RectTransform : null;

        Check(categoryLayout != null
                && categoryLayout.constraint == GridLayoutGroup.Constraint.FixedColumnCount
                && categoryLayout.constraintCount == 8,
            "CATEGORY_GRID_SINGLE_ROW",
            $"constraint={categoryLayout?.constraint}; columns={categoryLayout?.constraintCount ?? 0}");
        Check(root != null && categoryRoot != null && IsInside(root, categoryRoot),
            "CATEGORY_GRID_INSIDE_CATALOG",
            DescribeRect(categoryRoot));
        Check(root != null && visiblePanelRect != null && IsInside(root, visiblePanelRect),
            "ITEM_GRID_INSIDE_CATALOG",
            DescribeRect(visiblePanelRect));
        Check(categoryRoot != null && visiblePanelRect != null
                && visiblePanelRect.anchoredPosition.x >= categoryRoot.rect.width,
            "CATALOG_PANELS_SIDE_BY_SIDE",
            $"categoryWidth={categoryRoot?.rect.width ?? -1f:0.##}; itemX={visiblePanelRect?.anchoredPosition.x ?? -1f:0.##}");
    }

    private static bool IsInside(RectTransform parent, RectTransform child)
    {
        Vector3[] parentCorners = new Vector3[4];
        Vector3[] childCorners = new Vector3[4];
        parent.GetWorldCorners(parentCorners);
        child.GetWorldCorners(childCorners);
        const float tolerance = 0.5f;
        return childCorners[0].x >= parentCorners[0].x - tolerance
            && childCorners[0].y >= parentCorners[0].y - tolerance
            && childCorners[2].x <= parentCorners[2].x + tolerance
            && childCorners[2].y <= parentCorners[2].y + tolerance;
    }

    private static string DescribeRect(RectTransform rect)
    {
        return rect == null
            ? "<null>"
            : $"position={rect.anchoredPosition}; size={rect.rect.size}";
    }

    private static void CaptureScreen(string path)
    {
        Texture2D capture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        capture.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
        capture.Apply();
        File.WriteAllBytes(path, capture.EncodeToPNG());
        UnityEngine.Object.Destroy(capture);
    }

    private void Check(bool passed, string key, string detail)
    {
        report.Add($"{key}={(passed ? "PASS" : "FAIL")}; {detail}");
        if (!passed)
        {
            failures.Add(key + ": " + detail);
        }
    }

    private void Finish()
    {
        report.Add($"capturedErrors={capturedErrors.Count}; {Compact(capturedErrors)}");
        report.Add($"capturedWarnings={capturedWarnings.Count}; {Compact(capturedWarnings)}");
        bool passed = failures.Count == 0 && capturedErrors.Count == 0 && capturedWarnings.Count == 0;
        report.Add($"RESULT={(passed ? "PASS" : "FAIL")}; failures={failures.Count}; {Compact(failures)}");
        File.WriteAllText(BuildPlacementUxPlayModeVerifier.ReportPath, string.Join("\n", report));
        if (passed)
        {
            Debug.Log("Build placement UX verification passed. " + BuildPlacementUxPlayModeVerifier.ReportPath);
        }
        else
        {
            Debug.LogError("Build placement UX verification failed. " + BuildPlacementUxPlayModeVerifier.ReportPath);
        }

        EditorApplication.ExitPlaymode();
    }

    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Warning)
        {
            capturedWarnings.Add(condition);
        }
        else if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            capturedErrors.Add(string.IsNullOrWhiteSpace(stackTrace) ? condition : condition + "\n" + stackTrace);
        }
    }

    private static string Compact(IEnumerable<string> values)
    {
        string value = string.Join(" | ", values.Where(item => !string.IsNullOrWhiteSpace(item)));
        return string.IsNullOrWhiteSpace(value) ? "<none>" : value.Replace("\n", " ");
    }
}
