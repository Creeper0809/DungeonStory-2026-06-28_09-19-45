using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

[InitializeOnLoad]
public static class P0FeatureSurfacePlayModeVerifier
{
    public const string RequestPath = "Temp/p0-ui-surface-verification.request";
    public const string ReportPath = "Temp/p0-ui-surface-verification-report.txt";
    public const string ScreenshotPath = "Temp/p0-ui-surface-verification.png";

    private const string OriginalActiveSceneKey = "DungeonStory.P0UiVerifier.OriginalActiveScene";
    private const string SceneRootStatesKey = "DungeonStory.P0UiVerifier.SceneRootStates";
    private const string OpenedSampleSceneKey = "DungeonStory.P0UiVerifier.OpenedSampleScene";
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

    private static bool runnerCreated;
    private static bool preparingScenes;

    [Serializable]
    private sealed class SceneRootState
    {
        public string scenePath;
        public string rootName;
        public int rootIndex;
        public bool activeSelf;
    }

    [Serializable]
    private sealed class SceneRootStateCollection
    {
        public List<SceneRootState> entries = new List<SceneRootState>();
    }

    static P0FeatureSurfacePlayModeVerifier()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }

    [MenuItem("DungeonStory/Debug/QA/Request P0 UI Surface Verification")]
    public static void RequestRunFromMenu()
    {
        Directory.CreateDirectory("Temp");
        File.WriteAllText(RequestPath, DateTime.Now.ToString("O"));
    }

    private static void OnEditorUpdate()
    {
        if (preparingScenes
            || EditorApplication.isPlayingOrWillChangePlaymode
            || !File.Exists(RequestPath))
        {
            return;
        }

        preparingScenes = true;
        try
        {
            PrepareScenesForPlayMode();
            EditorApplication.EnterPlaymode();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            preparingScenes = false;
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredEditMode)
        {
            RestoreSceneState();
            runnerCreated = false;
            preparingScenes = false;
            return;
        }

        if (change != PlayModeStateChange.EnteredPlayMode
            || !File.Exists(RequestPath)
            || runnerCreated)
        {
            return;
        }

        runnerCreated = true;
        GameObject runnerObject = new GameObject("P0 UI Surface Verification Runner");
        UnityEngine.Object.DontDestroyOnLoad(runnerObject);
        runnerObject.AddComponent<P0FeatureSurfaceVerificationRunner>();
    }

    private static void PrepareScenesForPlayMode()
    {
        Scene originalActiveScene = SceneManager.GetActiveScene();
        SessionState.SetString(OriginalActiveSceneKey, originalActiveScene.path ?? string.Empty);

        SceneRootStateCollection stateCollection = new SceneRootStateCollection();
        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);
            if (!scene.IsValid() || !scene.isLoaded || scene.path == SampleScenePath)
            {
                continue;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                GameObject root = roots[rootIndex];
                stateCollection.entries.Add(new SceneRootState
                {
                    scenePath = scene.path,
                    rootName = root.name,
                    rootIndex = rootIndex,
                    activeSelf = root.activeSelf
                });

                if (root.activeSelf)
                {
                    root.SetActive(false);
                }
            }
        }

        SessionState.SetString(SceneRootStatesKey, JsonUtility.ToJson(stateCollection));

        Scene sampleScene = SceneManager.GetSceneByPath(SampleScenePath);
        bool openedSampleScene = !sampleScene.IsValid() || !sampleScene.isLoaded;
        if (openedSampleScene)
        {
            sampleScene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Additive);
        }

        SessionState.SetBool(OpenedSampleSceneKey, openedSampleScene);
        if (!sampleScene.IsValid() || !sampleScene.isLoaded)
        {
            throw new InvalidOperationException("P0 UI verification could not load SampleScene.");
        }

        SceneManager.SetActiveScene(sampleScene);
    }

    private static void RestoreSceneState()
    {
        string originalScenePath = SessionState.GetString(OriginalActiveSceneKey, string.Empty);
        Scene originalScene = SceneManager.GetSceneByPath(originalScenePath);
        if (originalScene.IsValid() && originalScene.isLoaded)
        {
            SceneManager.SetActiveScene(originalScene);
        }

        if (SessionState.GetBool(OpenedSampleSceneKey, false))
        {
            Scene sampleScene = SceneManager.GetSceneByPath(SampleScenePath);
            if (sampleScene.IsValid() && sampleScene.isLoaded && sampleScene != originalScene)
            {
                EditorSceneManager.CloseScene(sampleScene, true);
            }
        }

        string json = SessionState.GetString(SceneRootStatesKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(json))
        {
            SceneRootStateCollection stateCollection = JsonUtility.FromJson<SceneRootStateCollection>(json);
            foreach (SceneRootState entry in stateCollection?.entries ?? new List<SceneRootState>())
            {
                Scene scene = SceneManager.GetSceneByPath(entry.scenePath);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    continue;
                }

                GameObject[] roots = scene.GetRootGameObjects();
                GameObject root = entry.rootIndex >= 0 && entry.rootIndex < roots.Length
                    ? roots[entry.rootIndex]
                    : roots.FirstOrDefault((candidate) => candidate != null && candidate.name == entry.rootName);
                if (root != null && root.activeSelf != entry.activeSelf)
                {
                    root.SetActive(entry.activeSelf);
                }
            }
        }

        SessionState.EraseString(OriginalActiveSceneKey);
        SessionState.EraseString(SceneRootStatesKey);
        SessionState.SetBool(OpenedSampleSceneKey, false);
    }

    private sealed class P0FeatureSurfaceVerificationRunner : MonoBehaviour
    {
        private readonly List<string> capturedErrors = new List<string>();
        private readonly List<string> capturedWarnings = new List<string>();
        private readonly List<UnityEngine.Object> tempObjects = new List<UnityEngine.Object>();
        private bool capturingLogs;

        private IEnumerator Start()
        {
            yield return Run();
        }

        private IEnumerator Run()
        {
            List<string> lines = new List<string>();
            Directory.CreateDirectory("Temp");

            yield return EnsureSampleSceneActive(lines);
            yield return null;
            yield return null;

            ClearConsole();
            StartLogCapture();

            float originalTimeScale = Time.timeScale;
            try
            {
                Time.timeScale = 1f;
                UITabManager manager = null;
                RunStep("SETUP", lines, () =>
                {
                    Scene activeScene = SceneManager.GetActiveScene();
                    lines.Add($"activeScene={activeScene.path}");
                    manager = FindActiveSceneComponent<UITabManager>();
                    lines.Add($"tabManager={manager != null}");
                    PrepareGameState(lines);
                });
                if (manager == null)
                {
                    yield break;
                }

                yield return null;

                RunStep("SHOP", lines, () =>
                {
                    VerifyShop(manager, lines);
                    CaptureTab("Temp/p0-ui-shop.png", lines);
                });
                yield return null;

                RunStep("SHOP-BASIC-VISUAL", lines, () =>
                {
                    ScrollActiveP0Panel(0f);
                });
                yield return null;
                RunStep("SHOP-BASIC-CAPTURE", lines, () =>
                    CaptureTab("Temp/p0-ui-shop-basic.png", lines));
                yield return null;

                RunStep("WAREHOUSE", lines, () =>
                {
                    VerifyWarehouse(manager, lines);
                    CaptureTab("Temp/p0-ui-warehouse.png", lines);
                });
                yield return null;

                RunStep("WAREHOUSE-ACTIONS-VISUAL", lines, () =>
                {
                    ScrollActiveP0Panel(0.52f);
                });
                yield return null;
                RunStep("WAREHOUSE-ACTIONS-CAPTURE", lines, () =>
                    CaptureTab("Temp/p0-ui-warehouse-actions.png", lines));
                yield return null;

                RunStep("OPERATION", lines, () =>
                {
                    VerifyOperationRecruitmentMeta(manager, lines);
                    CaptureTab("Temp/p0-ui-operation.png", lines);
                });
                yield return null;

                RunStep("RECRUITMENT-VISUAL", lines, () =>
                {
                    ScrollActiveP0Panel(0.55f);
                });
                yield return null;
                RunStep("RECRUITMENT-CAPTURE", lines, () =>
                    CaptureTab("Temp/p0-ui-recruitment.png", lines));
                yield return null;

                RunStep("META-VISUAL", lines, () =>
                {
                    ScrollActiveP0Panel(0f);
                });
                yield return null;
                RunStep("META-CAPTURE", lines, () =>
                    CaptureTab("Temp/p0-ui-meta.png", lines));
                yield return null;

                RunStep("RESEARCH", lines, () =>
                {
                    VerifyResearch(manager, lines);
                    CaptureTab("Temp/p0-ui-research.png", lines);
                });
                yield return null;

                RunStep("VISUAL", lines, () =>
                {
                    RunUiBounds(lines);
                    ScreenCapture.CaptureScreenshot(ScreenshotPath);
                    lines.Add($"screenCapture={ScreenshotPath}");
                });
            }
            finally
            {
                Time.timeScale = originalTimeScale;
                DestroyTempObjects();
                Finish(lines);
            }
        }

        private static void RunStep(string name, List<string> lines, Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                lines.Add($"{name}-EXCEPTION={ex.GetType().Name}: {ex.Message}");
                if (!string.IsNullOrWhiteSpace(ex.StackTrace))
                {
                    lines.Add(ex.StackTrace);
                }
            }
        }

        private static void CaptureTab(string path, List<string> lines)
        {
            ScreenCapture.CaptureScreenshot(path);
            lines.Add($"tabCapture={path}");
        }

        private static void ScrollActiveP0Panel(float normalizedPosition)
        {
            P0FeatureSurfacePanel panel = UnityEngine.Object.FindObjectsByType<P0FeatureSurfacePanel>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None)
                .FirstOrDefault((candidate) => candidate != null && candidate.gameObject.activeInHierarchy);
            ScrollRect scroll = panel != null
                ? panel.GetComponentInChildren<ScrollRect>(includeInactive: false)
                : null;
            if (scroll == null)
            {
                throw new InvalidOperationException("Active P0 scroll view was not found.");
            }

            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = Mathf.Clamp01(normalizedPosition);
            if (scroll.content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            }
            Canvas.ForceUpdateCanvases();
        }

        private IEnumerator EnsureSampleSceneActive(List<string> lines)
        {
            const string sampleScenePath = "Assets/Scenes/SampleScene.unity";
            Scene sampleScene = SceneManager.GetSceneByPath(sampleScenePath);
            if (!sampleScene.IsValid() || !sampleScene.isLoaded)
            {
                AsyncOperation load = SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Additive);
                while (load != null && !load.isDone)
                {
                    yield return null;
                }

                sampleScene = SceneManager.GetSceneByPath(sampleScenePath);
                lines.Add($"loadedSampleScene={sampleScene.IsValid() && sampleScene.isLoaded}");
            }

            if (sampleScene.IsValid() && sampleScene.isLoaded)
            {
                SceneManager.SetActiveScene(sampleScene);
                lines.Add("sampleSceneActive=True");
            }
            else
            {
                lines.Add("sampleSceneActive=False");
            }
        }

        private void PrepareGameState(List<string> lines)
        {
            GameManager gameManager = FindActiveSceneComponent<GameManager>();
            GameData gameData = gameManager != null ? gameManager.gameData : null;
            if (gameData != null && gameData.holdingMoney != null)
            {
                int beforeMoney = gameData.holdingMoney.Value;
                gameData.holdingMoney.Value = Mathf.Max(beforeMoney, 50000);
                lines.Add($"preparedMoney={beforeMoney}->{gameData.holdingMoney.Value}");
            }

            MetaProgressionRuntime meta = FindActiveSceneComponent<MetaProgressionRuntime>();
            if (meta != null)
            {
                int beforeCurrency = meta.State.AvailableCurrency;
                meta.State.AddCurrency(600);
                lines.Add($"preparedMetaCurrency={beforeCurrency}->{meta.State.AvailableCurrency}");
            }

            PrepareBasicPurchaseOffer(lines);
            PrepareRecruitCandidate(lines);
            PrepareWarehouseRestockTarget(lines);
        }

        private void PrepareBasicPurchaseOffer(List<string> lines)
        {
            DailyFacilityShopRuntime runtime = FindActiveSceneComponent<DailyFacilityShopRuntime>();
            LifetimeScope scope = FindActiveSceneLifetimeScope();
            IFacilityShopCatalog catalog = scope != null && scope.Container != null
                ? scope.Container.Resolve(typeof(IFacilityShopCatalog)) as IFacilityShopCatalog
                : null;
            BuildingSO building = catalog?.Buildings
                .FirstOrDefault((candidate) => candidate != null && FacilityShopService.CanEnterBasicPurchase(candidate));
            bool unlocked = runtime != null && building != null && runtime.UnlockState.UnlockBasicPurchase(building);
            int offerCount = runtime != null ? runtime.CurrentBasicPurchaseOffers.Count : -1;
            lines.Add($"preparedBasicPurchase={unlocked || offerCount > 0}; building={(building != null ? building.name : "<none>")}; offers={offerCount}");
        }

        private void PrepareRecruitCandidate(List<string> lines)
        {
            RegularCustomerRuntime runtime = FindActiveSceneComponent<RegularCustomerRuntime>();
            if (runtime == null)
            {
                lines.Add("preparedRecruitCandidate=False; reason=no runtime");
                return;
            }

            CharacterActor customer = CreateQaCustomer("P0 UI Recruit Candidate");
            tempObjects.Add(customer.data);
            tempObjects.Add(customer.gameObject);
            BuildableObject facility = FindActiveSceneComponents<BuildableObject>()
                .FirstOrDefault((building) => building != null && !building.isDestroy);

            for (int i = 0; i < 4; i++)
            {
                FacilityVisitEvent.Trigger(customer, facility);
            }

            bool hasRecord = runtime.State.TryGetRecord(
                RegularCustomerService.GetCustomerId(customer),
                out RegularCustomerRecord record);
            lines.Add($"preparedRecruitCandidate={hasRecord}; status={(record != null ? record.Status.ToString() : "<none>")}; visits={(record != null ? record.VisitCount : -1)}");
        }

        private CharacterActor CreateQaCustomer(string name)
        {
            CharacterSO data = ScriptableObject.CreateInstance<CharacterSO>();
            data.id = 990000 + tempObjects.Count;
            data.characterName = name;
            data.characterType = CharacterType.Customer;
            data.role = CharacterRole.Regular;
            data.speciesTag = "QA";
            data.baseStats = CharacterStatBlock.CreateDefault(90);
            data.defaultWorkPriorities = WorkPriorityProfile.CreateDefault();

            GameObject obj = new GameObject(name);
            obj.AddComponent<SpriteRenderer>();
            CharacterActor actor = obj.AddComponent<CharacterActor>();
            obj.AddComponent<AbilityMove>();
            obj.AddComponent<AbilityShopping>();
            AIBrain brain = obj.AddComponent<AIBrain>();
            brain.availableActions = AiDebugScenarioActionFactory.CreateCustomerActions();

            InjectGameObjectFromLifetimeScope(obj);
            actor.RefreshAbilityCache();
            actor.Initialization(data);
            actor.EnsureRuntimeState();
            InjectGameObjectFromLifetimeScope(obj);
            actor.RefreshAbilityCache();
            actor.SetLifecycleState(CharacterLifecycleState.Active);
            actor.stats[CharacterCondition.MOOD] = 100f;
            obj.SetActive(false);
            return actor;
        }

        private void PrepareWarehouseRestockTarget(List<string> lines)
        {
            IWarehouseFacility warehouse = FindActiveSceneComponents<MonoBehaviour>()
                .OfType<IWarehouseFacility>()
                .FirstOrDefault((candidate) => candidate != null && candidate.HasWarehouseInventory && candidate.Inventory != null);
            if (warehouse != null)
            {
                int beforeWarehouse = warehouse.Inventory.TotalStock;
                int removed = 0;
                foreach (StockCategory category in Enum.GetValues(typeof(StockCategory)))
                {
                    removed += warehouse.Inventory.Withdraw(category, 10);
                    if (removed >= 30)
                    {
                        break;
                    }
                }

                lines.Add($"preparedDeliveryCapacity=True; warehouseStock={beforeWarehouse}->{warehouse.Inventory.TotalStock}; removed={removed}");
            }
            else
            {
                lines.Add("preparedDeliveryCapacity=False; reason=no warehouse");
            }

            Shop shop = FindActiveSceneComponents<Shop>()
                .FirstOrDefault((candidate) => candidate != null && !candidate.isDestroy && candidate.CurrentStock > 0);
            if (shop == null)
            {
                lines.Add("preparedRestockTarget=False; reason=no stocked shop");
                return;
            }

            int before = shop.CurrentStock;
            shop.DebugClearStock();
            lines.Add($"preparedRestockTarget=True; shop={shop.name}; stock={before}->{shop.CurrentStock}");
        }

        private void VerifyShop(UITabManager manager, List<string> lines)
        {
            DailyFacilityShopRuntime shopRuntime = FindActiveSceneComponent<DailyFacilityShopRuntime>();
            BlueprintResearchRuntime research = FindActiveSceneComponent<BlueprintResearchRuntime>();
            int beforeMoney = GetHoldingMoney();
            int beforeTasks = research != null ? research.State.Tasks.Count : -1;

            manager.ToggleSelectButton(3);
            Canvas.ForceUpdateCanvases();
            int dailyButtonsBefore = CountActiveButtons("P0Action_ShopDaily_");
            int basicButtonsBefore = CountActiveButtons("P0Action_ShopBasic_");
            int dailyClicked = ClickSequential("P0Action_ShopDaily_", 8);
            Canvas.ForceUpdateCanvases();
            int basicClicked = ClickSequential("P0Action_ShopBasic_", 8);
            int afterMoney = GetHoldingMoney();
            int afterTasks = research != null ? research.State.Tasks.Count : -1;

            lines.Add(
                $"SHOP visible={IsTabActive(3)}; runtime={shopRuntime != null}; dailyButtons={dailyButtonsBefore}; dailyClicked={dailyClicked}; basicButtons={basicButtonsBefore}; basicClicked={basicClicked}; money={beforeMoney}->{afterMoney}; researchTasks={beforeTasks}->{afterTasks}; stateChanged={afterMoney != beforeMoney || afterTasks != beforeTasks}");
        }

        private void VerifyWarehouse(UITabManager manager, List<string> lines)
        {
            List<IWarehouseFacility> warehouses = FindActiveSceneComponents<MonoBehaviour>()
                .OfType<IWarehouseFacility>()
                .Where((warehouse) => warehouse != null && warehouse.HasWarehouseInventory && warehouse.Inventory != null)
                .ToList();
            List<Shop> shops = FindActiveSceneComponents<Shop>()
                .Where((shop) => shop != null && !shop.isDestroy)
                .ToList();
            int beforeWarehouse = warehouses.Sum((warehouse) => warehouse.Inventory.TotalStock);
            int beforeShopStock = shops.Sum((shop) => shop.CurrentStock);
            int beforeMoney = GetHoldingMoney();

            manager.ToggleSelectButton(4);
            Canvas.ForceUpdateCanvases();
            bool restockClicked = ClickFirst("P0Action_WarehouseRestock_");
            Canvas.ForceUpdateCanvases();
            bool deliveryClicked = ClickFirst("P0Action_WarehouseDelivery_");

            int afterWarehouse = warehouses.Sum((warehouse) => warehouse.Inventory.TotalStock);
            int afterShopStock = shops.Sum((shop) => shop.CurrentStock);
            int afterMoney = GetHoldingMoney();
            lines.Add(
                $"WAREHOUSE visible={IsTabActive(4)}; warehouses={warehouses.Count}; shops={shops.Count}; restockClicked={restockClicked}; deliveryClicked={deliveryClicked}; warehouseStock={beforeWarehouse}->{afterWarehouse}; shopStock={beforeShopStock}->{afterShopStock}; money={beforeMoney}->{afterMoney}; stateChanged={beforeWarehouse != afterWarehouse || beforeShopStock != afterShopStock || beforeMoney != afterMoney}");
        }

        private void VerifyOperationRecruitmentMeta(UITabManager manager, List<string> lines)
        {
            OperatingDaySettlementRuntime settlement = FindActiveSceneComponent<OperatingDaySettlementRuntime>();
            RegularCustomerRuntime regularCustomer = FindActiveSceneComponent<RegularCustomerRuntime>();
            MetaProgressionRuntime meta = FindActiveSceneComponent<MetaProgressionRuntime>();

            int beforeDay = GetCurrentDay();
            int beforeRecruited = regularCustomer != null ? regularCustomer.State.RecruitedCharacters.Count : -1;
            int beforeCurrency = meta != null ? meta.State.AvailableCurrency : -1;
            int beforeLevels = meta != null ? meta.State.UpgradeLevels.Values.Sum() : -1;

            manager.ToggleSelectButton(5);
            Canvas.ForceUpdateCanvases();
            bool settleClicked = ClickFirstExact("P0Action_OperationSettleDay");
            bool recruitClicked = ClickFirst("P0Action_Recruit_");
            bool metaClicked = ClickFirst("P0Action_MetaUpgrade_");

            int afterDay = GetCurrentDay();
            int afterRecruited = regularCustomer != null ? regularCustomer.State.RecruitedCharacters.Count : -1;
            int afterCurrency = meta != null ? meta.State.AvailableCurrency : -1;
            int afterLevels = meta != null ? meta.State.UpgradeLevels.Values.Sum() : -1;
            OperatingDayReport report = settlement != null ? settlement.LatestReport : null;

            lines.Add(
                $"OPERATION visible={IsTabActive(5)}; settleClicked={settleClicked}; report={(report != null)}; day={beforeDay}->{afterDay}; recruitClicked={recruitClicked}; recruited={beforeRecruited}->{afterRecruited}; metaClicked={metaClicked}; metaCurrency={beforeCurrency}->{afterCurrency}; metaLevels={beforeLevels}->{afterLevels}; stateChanged={(report != null) || beforeDay != afterDay || beforeRecruited != afterRecruited || beforeLevels != afterLevels}");
        }

        private void VerifyResearch(UITabManager manager, List<string> lines)
        {
            BlueprintResearchRuntime research = FindActiveSceneComponent<BlueprintResearchRuntime>();
            if (research != null && !research.State.HasActiveTask)
            {
                FacilityBlueprintSO blueprint = research.ShopUnlockState.AcquiredBlueprintIds.Count > 0
                    ? null
                    : FindFirstBlueprint();
                if (blueprint != null)
                {
                    research.ShopUnlockState.MarkBlueprintAcquired(blueprint);
                }
            }

            int beforeTasks = research != null ? research.State.Tasks.Count : -1;
            int beforeCompleted = research != null ? research.State.CompletedBlueprintIds.Count : -1;
            float beforeProgress = GetActiveResearchProgress(research);

            manager.ToggleSelectButton(8);
            Canvas.ForceUpdateCanvases();
            bool cancelClicked = ClickFirst("P0Action_ResearchCancel_");
            Canvas.ForceUpdateCanvases();
            bool startClicked = ClickFirst("P0Action_ResearchStart_");
            Canvas.ForceUpdateCanvases();
            bool progressClicked = ClickFirstExact("P0Action_ResearchProgress");
            int afterTasks = research != null ? research.State.Tasks.Count : -1;
            int afterCompleted = research != null ? research.State.CompletedBlueprintIds.Count : -1;
            float afterProgress = GetActiveResearchProgress(research);

            lines.Add(
                $"RESEARCH visible={IsTabActive(8)}; runtime={research != null}; progressClicked={progressClicked}; cancelClicked={cancelClicked}; startClicked={startClicked}; tasks={beforeTasks}->{afterTasks}; completed={beforeCompleted}->{afterCompleted}; progress={beforeProgress:0.##}->{afterProgress:0.##}; stateChanged={beforeTasks != afterTasks || beforeCompleted != afterCompleted || !Mathf.Approximately(beforeProgress, afterProgress)}");
        }

        private FacilityBlueprintSO FindFirstBlueprint()
        {
            LifetimeScope scope = FindActiveSceneLifetimeScope();
            if (scope == null || scope.Container == null)
            {
                return null;
            }

            IFacilityShopCatalog catalog = scope.Container.Resolve(typeof(IFacilityShopCatalog)) as IFacilityShopCatalog;
            return catalog != null
                ? catalog.Blueprints.FirstOrDefault((blueprint) => blueprint != null)
                : null;
        }

        private int ClickSequential(string prefix, int max)
        {
            int clicked = 0;
            for (int i = 0; i < max; i++)
            {
                if (ClickFirstExact(prefix + i))
                {
                    clicked++;
                }
            }

            return clicked;
        }

        private bool ClickFirst(string prefix)
        {
            Button button = UnityEngine.Object.FindObjectsByType<Button>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None)
                .FirstOrDefault((candidate) => candidate != null
                    && candidate.gameObject.activeInHierarchy
                    && candidate.name.StartsWith(prefix, StringComparison.Ordinal));
            return Click(button);
        }

        private bool ClickFirstExact(string name)
        {
            Button button = UnityEngine.Object.FindObjectsByType<Button>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None)
                .FirstOrDefault((candidate) => candidate != null
                    && candidate.gameObject.activeInHierarchy
                    && candidate.name == name);
            return Click(button);
        }

        private static bool Click(Button button)
        {
            if (button == null || !button.IsInteractable())
            {
                return false;
            }

            button.onClick.Invoke();
            Canvas.ForceUpdateCanvases();
            return true;
        }

        private int CountActiveButtons(string prefix)
        {
            return UnityEngine.Object.FindObjectsByType<Button>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None)
                .Count((button) => button != null
                    && button.gameObject.activeInHierarchy
                    && button.name.StartsWith(prefix, StringComparison.Ordinal));
        }

        private bool IsTabActive(int id)
        {
            return UnityEngine.Object.FindObjectsByType<UITab>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Any((tab) => tab != null && tab.id == id && tab.gameObject.activeInHierarchy);
        }

        private int GetHoldingMoney()
        {
            GameManager gameManager = FindActiveSceneComponent<GameManager>();
            GameData gameData = gameManager != null ? gameManager.gameData : null;
            return gameData != null && gameData.holdingMoney != null ? gameData.holdingMoney.Value : -1;
        }

        private int GetCurrentDay()
        {
            GameManager gameManager = FindActiveSceneComponent<GameManager>();
            GameData gameData = gameManager != null ? gameManager.gameData : null;
            return gameData != null && gameData.day != null ? gameData.day.Value : -1;
        }

        private static float GetActiveResearchProgress(BlueprintResearchRuntime research)
        {
            if (research == null || !research.State.TryGetActiveTask(out BlueprintResearchTask task))
            {
                return -1f;
            }

            return task.Progress;
        }

        private void RunUiBounds(List<string> lines)
        {
            RectTransform[] rects = UnityEngine.Object.FindObjectsByType<RectTransform>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None)
                .Where((rect) => rect != null && rect.gameObject.activeInHierarchy)
                .ToArray();
            TMP_Text[] texts = UnityEngine.Object.FindObjectsByType<TMP_Text>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None)
                .Where((text) => text != null && text.gameObject.activeInHierarchy)
                .ToArray();
            Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None)
                .Where((button) => button != null && button.gameObject.activeInHierarchy)
                .ToArray();

            int invalid = 0;
            int oversized = 0;
            foreach (RectTransform rect in rects)
            {
                Rect worldRect = GetWorldRect(rect);
                if (float.IsNaN(worldRect.width) || float.IsNaN(worldRect.height))
                {
                    invalid++;
                }

                if (worldRect.width > Screen.width * 1.2f || worldRect.height > Screen.height * 1.2f)
                {
                    oversized++;
                }
            }

            lines.Add($"UI activeRects={rects.Length}; invalid={invalid}; oversized={oversized}; activeTexts={texts.Length}; activeButtons={buttons.Length}");
        }

        private static Rect GetWorldRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float minX = corners.Min((corner) => corner.x);
            float maxX = corners.Max((corner) => corner.x);
            float minY = corners.Min((corner) => corner.y);
            float maxY = corners.Max((corner) => corner.y);
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private T FindActiveSceneComponent<T>() where T : Component
        {
            return FindActiveSceneComponents<T>().FirstOrDefault();
        }

        private IReadOnlyList<T> FindActiveSceneComponents<T>() where T : Component
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return UnityEngine.Object.FindObjectsByType<T>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where((component) => component != null && component.gameObject.scene == activeScene)
                .ToArray();
        }

        private void InjectGameObjectFromLifetimeScope(GameObject target)
        {
            LifetimeScope scope = FindActiveSceneLifetimeScope();
            if (scope == null || scope.Container == null || target == null)
            {
                return;
            }

            foreach (MonoBehaviour component in target.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component != null)
                {
                    scope.Container.Inject(component);
                }
            }
        }

        private static LifetimeScope FindActiveSceneLifetimeScope()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            LifetimeScope[] scopes = UnityEngine.Object.FindObjectsByType<LifetimeScope>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            return scopes.FirstOrDefault((scope) =>
                    scope != null
                    && scope.Container != null
                    && scope.gameObject.scene == activeScene)
                ?? scopes.FirstOrDefault((scope) => scope != null && scope.Container != null);
        }

        private void StartLogCapture()
        {
            capturedErrors.Clear();
            capturedWarnings.Clear();
            if (capturingLogs)
            {
                return;
            }

            Application.logMessageReceived += OnLogMessageReceived;
            capturingLogs = true;
        }

        private void StopLogCapture()
        {
            if (!capturingLogs)
            {
                return;
            }

            Application.logMessageReceived -= OnLogMessageReceived;
            capturingLogs = false;
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Warning)
            {
                capturedWarnings.Add(condition);
                return;
            }

            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                capturedErrors.Add(string.IsNullOrWhiteSpace(stackTrace)
                    ? condition
                    : $"{condition}\n{stackTrace}");
            }
        }

        private void Finish(List<string> lines)
        {
            StopLogCapture();
            lines.Add($"capturedErrors={capturedErrors.Count}; errors={CompactList(capturedErrors)}");
            lines.Add($"capturedWarnings={capturedWarnings.Count}; warnings={CompactList(capturedWarnings)}");
            File.WriteAllText(ReportPath, string.Join("\n", lines));
            if (File.Exists(RequestPath))
            {
                File.Delete(RequestPath);
            }
        }

        private void DestroyTempObjects()
        {
            foreach (UnityEngine.Object obj in tempObjects.Where((item) => item != null).Reverse())
            {
                if (Application.isPlaying)
                {
                    Destroy(obj);
                }
                else
                {
                    DestroyImmediate(obj);
                }
            }
        }

        private static string CompactList(IReadOnlyList<string> values)
        {
            return values == null || values.Count == 0
                ? "<none>"
                : string.Join(" || ", values.Select((value) => CompactText(value, 180)));
        }

        private static string CompactText(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "<none>";
            }

            string singleLine = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
            return singleLine.Length <= maxLength
                ? singleLine
                : singleLine.Substring(0, maxLength) + "...";
        }

        private static void ClearConsole()
        {
            Type logEntries = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            MethodInfo clear = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clear?.Invoke(null, null);
        }
    }
}
