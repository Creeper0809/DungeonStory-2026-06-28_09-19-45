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

public static class UnifiedUiPlayModeVerifier
{
    public const string RequestPath = "Temp/phase54-unified-ui-verification.request";
    public const string ReportPath = "Temp/phase43-unified-ui-verification-report.txt";
    public const string CharacterCapturePath = "Temp/phase43-character-notice-verification.png";
    public const string CharacterRecordsCapturePath = "Temp/phase48-character-records-tab.png";
    public const string CharacterMoodCapturePath = "Temp/phase54-character-mood-tab.png";
    public const string StaffCapturePath = "Temp/phase43-staff-profile-verification.png";
    public const string BuildingPreviewCapturePath = "Temp/phase64-building-info-preview.png";

    [InitializeOnLoadMethod]
    private static void EnterPlayModeForRequestedVerification()
    {
        if (!File.Exists(RequestPath) || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        EditorApplication.delayCall += () =>
        {
            if (File.Exists(RequestPath) && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = true;
            }
        };
    }

    [InitializeOnEnterPlayMode]
    private static void RunRequestedVerification(EnterPlayModeOptions options)
    {
        if (!File.Exists(RequestPath))
        {
            return;
        }

        File.Delete(RequestPath);
        EditorApplication.delayCall += RunFromMenu;
    }

    [MenuItem("DungeonStory/Debug/QA/Run Unified UI PlayMode Verification")]
    public static void RunFromMenu()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogError("Unified UI verification requires PlayMode.");
            return;
        }

        if (UnityEngine.Object.FindFirstObjectByType<UnifiedUiPlayModeVerificationRunner>() != null)
        {
            Debug.LogWarning("Unified UI verification is already running.");
            return;
        }

        new GameObject("Unified UI PlayMode Verification Runner")
            .AddComponent<UnifiedUiPlayModeVerificationRunner>();
    }
}

public sealed class UnifiedUiPlayModeVerificationRunner : MonoBehaviour
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

        yield return VerifyCharacterSummaryAndNotices();
        yield return VerifyStaffProfile();
        yield return VerifyBuildingInfoPreview();
        Application.logMessageReceived -= OnLogMessageReceived;
        Finish();
        Destroy(gameObject);
    }

    private IEnumerator VerifyBuildingInfoPreview()
    {
        UIBuildingInfo buildingInfo = UnityEngine.Object.FindObjectsByType<UIBuildingInfo>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None)
            .FirstOrDefault();
        BuildableObject previewTarget = UnityEngine.Object.FindObjectsByType<BuildableObject>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .FirstOrDefault(building => building != null
                && building.id >= 1000
                && building.BuildingData != null
                && building.BuildingData.icon != null);

        Check(buildingInfo != null, "BUILDING_INFO", "building info presenter resolved");
        Check(previewTarget != null, "BUILDING_PREVIEW_TARGET", "runtime modular facility resolved");
        if (buildingInfo == null || previewTarget == null)
        {
            yield break;
        }

        buildingInfo.DisplayBuildingInfo(previewTarget);
        yield return new WaitForSecondsRealtime(0.5f);
        Canvas.ForceUpdateCanvases();

        Image preview = buildingInfo.buildingImageObject != null
            ? buildingInfo.buildingImageObject.GetComponent<Image>()
            : null;
        CanvasGroup group = buildingInfo.GetComponent<CanvasGroup>();
        Check(buildingInfo.gameObject.activeInHierarchy
            && group != null
            && group.alpha > 0.99f,
            "BUILDING_PREVIEW_OPEN",
            group != null ? $"active={buildingInfo.gameObject.activeInHierarchy},alpha={group.alpha:0.##}" : "canvas group missing");
        Check(preview != null && preview.sprite == previewTarget.BuildingData.icon,
            "BUILDING_PREVIEW_SPRITE",
            preview != null && preview.sprite != null ? preview.sprite.name : "null");
        Check(preview != null && ColorsMatch(preview.color, Color.white),
            "BUILDING_PREVIEW_COLOR",
            preview != null ? preview.color.ToString() : "null");
        Check(preview != null
            && preview.material != null
            && preview.material.shader != null
            && preview.material.shader.name == "UI/Default",
            "BUILDING_PREVIEW_SHADER",
            preview != null && preview.material != null && preview.material.shader != null
                ? preview.material.shader.name
                : "null");
        Check(preview != null && preview.preserveAspect && !preview.raycastTarget,
            "BUILDING_PREVIEW_PRESENTATION",
            preview != null
                ? $"preserveAspect={preview.preserveAspect},raycastTarget={preview.raycastTarget}"
                : "null");

        yield return CaptureScreenshot(UnifiedUiPlayModeVerifier.BuildingPreviewCapturePath);
        buildingInfo.CloseDispaly();
        yield return new WaitForSecondsRealtime(0.2f);
    }

    private IEnumerator VerifyCharacterSummaryAndNotices()
    {
        Canvas canvas = UnityEngine.Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .FirstOrDefault(item => item != null && item.isRootCanvas);
        CharacterSummeryInfo summary = UnityEngine.Object.FindFirstObjectByType<CharacterSummeryInfo>();
        CharacterActor actor = UnityEngine.Object.FindObjectsByType<CharacterActor>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .FirstOrDefault(item => item != null && item.Stats != null);

        Check(canvas != null, "CANVAS", "root canvas resolved");
        Check(summary != null, "CHARACTER_SUMMARY", "summary presenter resolved");
        Check(actor != null, "CHARACTER_ACTOR", "active actor resolved");
        if (canvas == null || summary == null || actor == null)
        {
            yield break;
        }

        InfoFeedEvent.Trigger(actor);
        yield return null;
        yield return new WaitForEndOfFrame();

        Transform generated = summary.UI != null
            ? summary.UI.transform.Find("CharacterSummaryGeneratedView")
            : null;
        Slider health = generated != null
            ? generated.Find("Content/StatusContent/Health/Track")?.GetComponent<Slider>()
            : null;
        GameObject statusContent = generated != null
            ? generated.Find("Content/StatusContent")?.gameObject
            : null;
        GameObject moodContent = generated != null
            ? generated.Find("Content/MoodContent")?.gameObject
            : null;
        GameObject recordsContent = generated != null
            ? generated.Find("Content/RecordsContent")?.gameObject
            : null;
        Button statusTab = generated != null
            ? generated.Find("TabBar/StatusTab")?.GetComponent<Button>()
            : null;
        Button moodTab = generated != null
            ? generated.Find("TabBar/MoodTab")?.GetComponent<Button>()
            : null;
        Button recordsTab = generated != null
            ? generated.Find("TabBar/RecordsTab")?.GetComponent<Button>()
            : null;
        Slider[] needs = { summary.hunger, summary.fun, summary.sleep, summary.excretion, summary.hygiene };
        string[] requiredLabels = { "상태", "기분", "기록", "체력", "현재 기분", "기분 요인", "포만감", "재미", "휴식", "배변", "위생" };
        TMP_Text[] generatedTexts = generated != null
            ? generated.GetComponentsInChildren<TMP_Text>(true)
            : Array.Empty<TMP_Text>();
        HashSet<string> visibleLabels = generatedTexts
            .Where(item => item != null)
            .Select(item => item.text)
            .ToHashSet();

        Check(summary.UI != null && summary.UI.activeInHierarchy, "CHARACTER_OPEN", "summary opened");
        Check(generated != null && generated.gameObject.activeInHierarchy, "CHARACTER_GENERATED", "generated view active");
        Check(health != null && summary.mood != null && needs.All(item => item != null), "CHARACTER_METERS", "health + mood + five needs bound");
        Check(statusTab != null && moodTab != null && recordsTab != null, "CHARACTER_TABS", "status, mood, and records tabs resolved");
        Check(statusContent != null && statusContent.activeInHierarchy
            && moodContent != null && !moodContent.activeSelf
            && recordsContent != null && !recordsContent.activeSelf,
            "CHARACTER_STATUS_TAB_DEFAULT", "status visible; mood and records hidden on open");
        Check(statusContent != null
            && !statusContent.GetComponentsInChildren<TMP_Text>(true).Any(item => item != null && item.text == "기분")
            && summary.mood != null
            && moodContent != null
            && summary.mood.transform.IsChildOf(moodContent.transform),
            "CHARACTER_MOOD_SEPARATED", "mood is absent from needs and owned by MoodContent");
        Check(requiredLabels.All(visibleLabels.Contains), "CHARACTER_LABELS", string.Join(",", requiredLabels));
        Check(summary.ObjectName != null && !string.IsNullOrWhiteSpace(summary.ObjectName.text)
            && summary.ObjectName.text != "New Text", "CHARACTER_NAME", summary.ObjectName?.text ?? "null");
        Check(!generatedTexts.Any(item => item != null && item.text == "New Text"), "NO_PLACEHOLDER", "generated text contains no placeholder");
        Check(IsRectInsideScreen(summary.UI.GetComponent<RectTransform>()), "CHARACTER_BOUNDS", "summary is inside screen");

        CharacterStats stats = actor.Stats;
        Dictionary<CharacterCondition, float> originalStats = Enum.GetValues(typeof(CharacterCondition))
            .Cast<CharacterCondition>()
            .ToDictionary(condition => condition, condition => stats.Stats.TryGetValue(condition, out float value) ? value : 0f);
        stats.Stats[CharacterCondition.HUNGER] = 50f;
        stats.Stats[CharacterCondition.FUN] = 50f;
        stats.Stats[CharacterCondition.SLEEP] = 50f;
        stats.Stats[CharacterCondition.EXCRETION] = 50f;
        stats.Stats[CharacterCondition.HYGIENE] = 50f;
        stats.Stats[CharacterCondition.MOOD] = 50f;
        stats.ChangesStat(CharacterCondition.HUNGER, 0f);
        yield return null;
        float neutralMood = stats.Stats[CharacterCondition.MOOD];
        stats.ChangesStat(CharacterCondition.HUNGER, 50f);
        yield return null;
        float satisfiedMood = stats.Stats[CharacterCondition.MOOD];
        stats.ChangesStat(CharacterCondition.HUNGER, -100f);
        yield return null;
        float deprivedMood = stats.Stats[CharacterCondition.MOOD];
        CharacterMoodSnapshot deprivedSnapshot = stats.GetMoodSnapshot();
        Check(satisfiedMood > neutralMood
            && deprivedMood < neutralMood
            && deprivedSnapshot.Factors.Any(item => item.Id == "need:hunger" && item.Value < 0f),
            "CHARACTER_LIVE_CHANGE",
            $"need-derived mood={neutralMood:0.#}->{satisfiedMood:0.#}->{deprivedMood:0.#}");

        CharacterActor socialSpeaker = UnityEngine.Object.FindObjectsByType<CharacterActor>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .FirstOrDefault(item => item != null && item != actor) ?? actor;
        actor.SocialMemory?.HearRumor(new SocialRumor
        {
            type = SocialRumorType.Praise,
            targetType = SocialRumorTargetType.Character,
            targetCharacterName = actor.Identity != null ? actor.Identity.DisplayName : actor.name,
            sentiment = 1f,
            spreadChance = 0f,
            validUntil = Time.time + 120f,
            summary = "기분 검증용 반가운 소식",
            source = "Phase54Verifier"
        }, socialSpeaker);
        float healthBeforeInteraction = stats.CurrentHealth;
        stats.ApplyDamage(4f, "기분 상호작용 검증");
        stats.Heal(4f);
        stats.ApplyMoodFactor("qa:brief-discomfort", "잠깐의 불편", -3f, 0.35f, 1);
        yield return null;
        float encouragedMood = stats.Stats[CharacterCondition.MOOD];
        CharacterMoodSnapshot gameplaySnapshot = stats.GetMoodSnapshot();
        bool gameplayFactorsApplied = gameplaySnapshot.Factors.Any(item => item.Id.StartsWith("social:rumor:") && item.Value > 0f)
            && gameplaySnapshot.Factors.Any(item => item.Id == "health:injury" && item.Value < 0f)
            && gameplaySnapshot.Factors.Any(item => item.Id == "health:relief" && item.Value > 0f)
            && Mathf.Approximately(stats.CurrentHealth, healthBeforeInteraction);
        PressButton(moodTab);
        yield return null;
        yield return new WaitForEndOfFrame();
        TMP_Text moodSummary = moodContent != null
            ? moodContent.transform.Find("MoodSummaryText")?.GetComponent<TMP_Text>()
            : null;
        TMP_Text moodFactors = moodContent != null
            ? moodContent.transform.Find("MoodFactorsViewport/MoodFactorsText")?.GetComponent<TMP_Text>()
            : null;
        Check(moodContent != null && moodContent.activeInHierarchy
            && statusContent != null && !statusContent.activeSelf
            && recordsContent != null && !recordsContent.activeSelf,
            "CHARACTER_MOOD_TAB_OPEN", "mood visible; status and records hidden after click");
        Check(encouragedMood > deprivedMood
            && moodSummary != null && moodSummary.text.Contains("기준") && moodSummary.text.Contains("보정")
            && moodFactors != null
            && moodFactors.text.Contains("욕구 영향")
            && moodFactors.text.Contains("굶주림")
            && moodFactors.text.Contains("최근 경험")
            && moodFactors.text.Contains("반가운 소문을 들음")
            && moodFactors.text.Contains("몸을 다침")
            && moodFactors.text.Contains("치료받아 안도함")
            && moodFactors.text.Contains("잠깐의 불편"),
            "CHARACTER_MOOD_FACTORS_VISIBLE",
            moodFactors != null ? moodFactors.text : "null");
        Check(gameplayFactorsApplied,
            "CHARACTER_MOOD_GAMEPLAY_INTERACTIONS",
            "social rumor, injury, and treatment produced named factors");
        Image moodTabImage = moodTab != null
            ? moodTab.targetGraphic as Image ?? moodTab.GetComponent<Image>()
            : null;
        Check(moodTabImage != null && ColorsMatch(moodTabImage.color, DungeonUiTheme.Accent),
            "CHARACTER_MOOD_SELECTED", "mood tab owns selected accent");
        yield return CaptureScreenshot(UnifiedUiPlayModeVerifier.CharacterMoodCapturePath);

        yield return new WaitForSecondsRealtime(0.65f);
        CharacterMoodSnapshot expiredSnapshot = stats.GetMoodSnapshot();
        summary.RefreshMoodDetails();
        Check(!expiredSnapshot.Factors.Any(item => item.Id == "qa:brief-discomfort")
            && moodFactors != null && !moodFactors.text.Contains("잠깐의 불편")
            && moodFactors != null && moodFactors.text.Contains("반가운 소문을 들음"),
            "CHARACTER_MOOD_EXPIRY", "short factor expired while long factor remained visible");

        string[] verificationFactorIds = stats.GetMoodSnapshot().Factors
            .Where(item => item.Kind == CharacterMoodFactorKind.Interaction
                && (item.Id == "qa:brief-discomfort"
                    || item.Id == "health:injury"
                    || item.Id == "health:relief"
                    || item.Id.StartsWith("social:rumor:")))
            .Select(item => item.Id)
            .Distinct()
            .ToArray();
        foreach (string factorId in verificationFactorIds)
        {
            stats.RemoveMoodFactor(factorId);
        }
        foreach (KeyValuePair<CharacterCondition, float> entry in originalStats)
        {
            stats.Stats[entry.Key] = entry.Value;
        }
        stats.ChangesStat(CharacterCondition.HUNGER, 0f);
        PressButton(statusTab);
        yield return null;

        List<CharacterLogEntry> addedRecords = new List<CharacterLogEntry>();
        Action<CharacterLogEntry> recordHandler = entry => addedRecords.Add(entry);
        actor.OnLogAdded += recordHandler;
        actor.AddLog("작업 시작 · 연구 · 연구실");
        actor.AddLog("작업 종료 · 연구 · 연구실 · 연구 완료");
        actor.OnLogAdded -= recordHandler;
        PressButton(recordsTab);
        yield return new WaitForSecondsRealtime(0.25f);
        TMP_Text recordsText = recordsContent != null
            ? recordsContent.GetComponentInChildren<TMP_Text>(true)
            : null;
        Check(recordsContent != null && recordsContent.activeInHierarchy
            && statusContent != null && !statusContent.activeSelf,
            "CHARACTER_RECORDS_TAB_OPEN", "records visible and status hidden after click");
        bool rawEventsPreserved = addedRecords.Count == 2
            && addedRecords[0].OriginalMessage.Contains("작업 시작")
            && addedRecords[1].OriginalMessage.Contains("작업 종료")
            && addedRecords.All(entry => entry.EntryId > 0);
        bool generatedRecordVisible = addedRecords.Any(entry =>
            actor.LogComponent.TryGetVisibleDisplayLine(entry.EntryId, out string displayLine)
            && recordsText != null
            && recordsText.text.Contains(displayLine));
        bool currentDisplayVisible = recordsText != null
            && generatedRecordVisible
            && !recordsText.text.Contains("작업 시작")
            && !recordsText.text.Contains("작업 종료")
            && !recordsText.text.Contains("행동 시작")
            && !recordsText.text.Contains("기록을 다듬는 중");
        Check(rawEventsPreserved && currentDisplayVisible,
            "CHARACTER_RECORDS_DETAIL", recordsText != null ? recordsText.text : "null");
        Image recordsTabImage = recordsTab != null
            ? recordsTab.targetGraphic as Image ?? recordsTab.GetComponent<Image>()
            : null;
        Check(recordsTabImage != null && ColorsMatch(recordsTabImage.color, DungeonUiTheme.Accent),
            "CHARACTER_RECORDS_SELECTED", "records tab owns selected accent");
        yield return CaptureScreenshot(UnifiedUiPlayModeVerifier.CharacterRecordsCapturePath);

        PressButton(statusTab);
        yield return null;
        Check(statusContent != null && statusContent.activeInHierarchy
            && recordsContent != null && !recordsContent.activeSelf,
            "CHARACTER_STATUS_TAB_RETURN", "status restored after click");

        for (int i = 1; i <= 8; i++)
        {
            NoticeFeedEvent.Grade grade = i % 3 == 0
                ? NoticeFeedEvent.Grade.DANGER
                : i % 2 == 0 ? NoticeFeedEvent.Grade.WARNING : NoticeFeedEvent.Grade.NONE;
            NoticeFeedEvent.Trigger($"검증 알림 {i} · 시설과 직원 상태가 갱신되었습니다.", grade);
        }

        yield return null;
        yield return new WaitForEndOfFrame();

        NoticeFeed feed = UnityEngine.Object.FindFirstObjectByType<NoticeFeed>();
        List<RectTransform> activeNotices = feed != null
            ? feed.GetComponentsInChildren<RectTransform>(false)
                .Where(item => item != null && item.parent == feed.transform && item.gameObject.activeSelf)
                .ToList()
            : new List<RectTransform>();
        bool noticeGeometryValid = activeNotices.Count > 0
            && activeNotices.Count <= 6
            && activeNotices.All(item => Mathf.Abs(item.rect.height - 56f) <= 0.1f)
            && activeNotices.All(item => RectContains(feed.GetComponent<RectTransform>(), item));
        bool noticeTextValid = activeNotices
            .Select(item => item.GetComponentInChildren<TMP_Text>(true))
            .All(item => item != null && item.fontSize <= 18.1f && !item.raycastTarget);
        bool noticeBackgroundValid = activeNotices
            .Select(item => item.GetComponent<Image>())
            .All(item => item != null && item.color.a >= 0.99f && !item.raycastTarget);

        Check(feed != null, "NOTICE_FEED", "notice feed resolved");
        Check(noticeGeometryValid, "NOTICE_GEOMETRY", $"active={activeNotices.Count}");
        Check(noticeTextValid, "NOTICE_TEXT", "font <= 18 and non-raycast");
        Check(noticeBackgroundValid, "NOTICE_BACKGROUND", "opaque and non-raycast");
        yield return CaptureScreenshot(UnifiedUiPlayModeVerifier.CharacterCapturePath);

        Button closeButton = generated != null
            ? generated.Find("Header/CloseButton")?.GetComponent<Button>()
            : null;
        Check(closeButton != null, "CHARACTER_CLOSE_BUTTON", "close button resolved");
        if (closeButton != null)
        {
            PressButton(closeButton);
            yield return null;
            Check(summary.UI != null && !summary.UI.activeSelf, "CHARACTER_CLOSE", "pointer click closed summary");
        }

        yield return new WaitForSecondsRealtime(3.2f);
    }

    private IEnumerator VerifyStaffProfile()
    {
        Button staffTab = FindVisibleButtonByLabel("직원");
        Check(staffTab != null, "STAFF_TAB", "bottom staff tab resolved");
        if (staffTab == null)
        {
            yield break;
        }

        PressButton(staffTab);
        yield return null;
        Button managementMode = FindVisibleButtonByName("P1Action_StaffModeManagement");
        Check(managementMode != null, "STAFF_MANAGEMENT_BUTTON", "management mode resolved");
        if (managementMode == null)
        {
            yield break;
        }

        PressButton(managementMode);
        yield return null;
        yield return new WaitForEndOfFrame();

        Button priorityMode = FindVisibleButtonByName("P1Action_StaffModePriorities");
        Image managementImage = managementMode.targetGraphic as Image ?? managementMode.GetComponent<Image>();
        Image priorityImage = priorityMode != null
            ? priorityMode.targetGraphic as Image ?? priorityMode.GetComponent<Image>()
            : null;
        bool managementSelected = managementImage != null
            && ColorsMatch(managementImage.color, DungeonUiTheme.Accent)
            && priorityImage != null
            && !ColorsMatch(priorityImage.color, DungeonUiTheme.Accent);
        Check(managementSelected, "STAFF_MODE_SELECTED", "management accent follows opened mode");

        StaffWorkPriorityPanel panel = UnityEngine.Object.FindFirstObjectByType<StaffWorkPriorityPanel>();
        TMP_Text[] texts = panel != null
            ? panel.GetComponentsInChildren<TMP_Text>(true)
            : Array.Empty<TMP_Text>();
        string combined = string.Join("\n", texts.Where(item => item != null).Select(item => item.text));
        string[] statLabels = { "공격", "판매", "연구", "이동", "근력", "맷집", "민첩", "청소", "지구력" };
        RectTransform profileCard = panel != null
            ? panel.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(item => item.name == "P1Action_StaffProfile_Card")
            : null;

        Check(panel != null && panel.gameObject.activeInHierarchy, "STAFF_PANEL", "staff management panel active");
        Check(statLabels.All(combined.Contains), "STAFF_NINE_STATS", string.Join(",", statLabels));
        Check(profileCard != null && Mathf.Abs(profileCard.rect.height - 156f) <= 0.1f,
            "STAFF_PROFILE_CARD", profileCard != null ? profileCard.rect.size.ToString() : "null");

        ScrollRect scroll = profileCard != null ? profileCard.GetComponentInParent<ScrollRect>() : null;
        if (scroll != null && scroll.viewport != null && scroll.content != null)
        {
            Canvas.ForceUpdateCanvases();
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, profileCard);
            scroll.content.anchoredPosition += new Vector2(0f, -bounds.center.y);
            Canvas.ForceUpdateCanvases();
            yield return new WaitForEndOfFrame();
        }

        yield return CaptureScreenshot(UnifiedUiPlayModeVerifier.StaffCapturePath);
        PressButton(staffTab);
        yield return null;
        Check(!panel.gameObject.activeInHierarchy, "STAFF_CLOSE", "second pointer click closed staff tab");
    }

    private IEnumerator CaptureScreenshot(string path)
    {
        yield return new WaitForEndOfFrame();
        Texture2D capture = ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(path, capture.EncodeToPNG());
        Destroy(capture);
        report.Add($"capture={path}");
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
        File.WriteAllText(UnifiedUiPlayModeVerifier.ReportPath, string.Join("\n", report));
        if (passed)
        {
            Debug.Log("Unified UI PlayMode verification passed. " + UnifiedUiPlayModeVerifier.ReportPath);
        }
        else
        {
            Debug.LogError("Unified UI PlayMode verification failed. " + UnifiedUiPlayModeVerifier.ReportPath);
        }
    }

    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Warning)
        {
            capturedWarnings.Add(condition);
        }
        else if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            capturedErrors.Add(string.IsNullOrWhiteSpace(stackTrace)
                ? condition
                : condition + "\n" + stackTrace);
        }
    }

    private static Button FindVisibleButtonByLabel(string label)
    {
        return UnityEngine.Object.FindObjectsByType<Button>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .FirstOrDefault(button => button != null
                && button.interactable
                && button.GetComponentsInChildren<TMP_Text>(true)
                    .Any(text => text != null && text.text == label));
    }

    private static Button FindVisibleButtonByName(string objectName)
    {
        return UnityEngine.Object.FindObjectsByType<Button>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .FirstOrDefault(button => button != null
                && button.interactable
                && button.name == objectName);
    }

    private static void PressButton(Button button)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            button = PointerEventData.InputButton.Left
        };
        button.OnPointerClick(eventData);
    }

    private static bool IsRectInsideScreen(RectTransform rect)
    {
        if (rect == null)
        {
            return false;
        }

        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return corners.All(corner => corner.x >= -0.5f
            && corner.y >= -0.5f
            && corner.x <= Screen.width + 0.5f
            && corner.y <= Screen.height + 0.5f);
    }

    private static bool RectContains(RectTransform parent, RectTransform child)
    {
        if (parent == null || child == null)
        {
            return false;
        }

        Vector3[] parentCorners = new Vector3[4];
        Vector3[] childCorners = new Vector3[4];
        parent.GetWorldCorners(parentCorners);
        child.GetWorldCorners(childCorners);
        float minX = parentCorners.Min(item => item.x) - 0.5f;
        float maxX = parentCorners.Max(item => item.x) + 0.5f;
        float minY = parentCorners.Min(item => item.y) - 0.5f;
        float maxY = parentCorners.Max(item => item.y) + 0.5f;
        return childCorners.All(item => item.x >= minX && item.x <= maxX && item.y >= minY && item.y <= maxY);
    }

    private static bool ColorsMatch(Color left, Color right)
    {
        return Mathf.Abs(left.r - right.r) <= 0.01f
            && Mathf.Abs(left.g - right.g) <= 0.01f
            && Mathf.Abs(left.b - right.b) <= 0.01f
            && Mathf.Abs(left.a - right.a) <= 0.01f;
    }

    private static string Compact(IReadOnlyList<string> values)
    {
        return values == null || values.Count == 0
            ? "<none>"
            : string.Join(" || ", values.Select(value => value.Replace('\n', ' ').Replace('\r', ' ')));
    }
}
