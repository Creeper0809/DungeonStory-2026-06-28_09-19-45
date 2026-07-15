using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class OwnerSelectionPanel : MonoBehaviour
{
    [SerializeField] private OwnerRunManager ownerRunManager;
    [SerializeField] private Transform optionRoot;
    [SerializeField] private Button optionButtonPrefab;
    [SerializeField] private TMP_Text selectedOwnerText;
    [SerializeField] private bool buildOptionsOnStart = true;
    [SerializeField] private bool hideAfterOwnerSelected = true;

    private IOwnerRunManagerProvider ownerRunManagerProvider;
    private ITmpKoreanFontService tmpKoreanFontService;
    private IOwnerSelectionOptionButtonFactory optionButtonFactory;

    [Inject]
    public void ConstructOwnerSelectionPanel(
        IOwnerRunManagerProvider ownerRunManagerProvider,
        ITmpKoreanFontService tmpKoreanFontService,
        IOwnerSelectionOptionButtonFactory optionButtonFactory)
    {
        this.ownerRunManagerProvider = ownerRunManagerProvider
            ?? throw new ArgumentNullException(nameof(ownerRunManagerProvider));
        this.tmpKoreanFontService = tmpKoreanFontService
            ?? throw new ArgumentNullException(nameof(tmpKoreanFontService));
        this.optionButtonFactory = optionButtonFactory
            ?? throw new ArgumentNullException(nameof(optionButtonFactory));
    }

    private void Start()
    {
        RequireTmpKoreanFontService().ApplyToChildren(transform);

        OwnerRunManager manager = ResolveOwnerRunManager();

        if (buildOptionsOnStart)
        {
            BuildOptions();
        }

        if (manager.selectedOwnerData != null)
        {
            RefreshSelectedOwner(manager.selectedOwnerData.Value);
            manager.selectedOwnerData.OnValueChange += HandleSelectedOwnerChanged;
            HideIfOwnerSelected(manager.selectedOwnerData.Value);
        }
    }

    public void BuildOptions()
    {
        OwnerRunManager manager = ResolveOwnerRunManager();
        if (optionRoot == null || optionButtonPrefab == null)
        {
            return;
        }

        for (int i = optionRoot.childCount - 1; i >= 0; i--)
        {
            RequireOptionButtonFactory().Release(optionRoot.GetChild(i).gameObject);
        }

        CharacterSO[] candidates = manager.OwnerCandidates;
        for (int i = 0; i < candidates.Length; i++)
        {
            CharacterSO candidate = candidates[i];
            int index = i;
            RequireOptionButtonFactory().Create(
                optionButtonPrefab,
                optionRoot,
                $"OwnerOption_{candidate.characterName}",
                MakeButtonLabel(candidate),
                () => manager.SelectOwnerByIndex(index));
        }
    }

    private OwnerRunManager ResolveOwnerRunManager()
    {
        if (ownerRunManager != null)
        {
            return ownerRunManager;
        }

        if (ownerRunManagerProvider == null)
        {
            throw new InvalidOperationException($"{nameof(OwnerSelectionPanel)} requires {nameof(IOwnerRunManagerProvider)} injection.");
        }

        if (!ownerRunManagerProvider.TryGetManager(out OwnerRunManager resolvedManager))
        {
            throw new InvalidOperationException($"{nameof(OwnerSelectionPanel)} could not resolve {nameof(OwnerRunManager)}.");
        }

        ownerRunManager = resolvedManager;
        return ownerRunManager;
    }

    private void RefreshSelectedOwner(CharacterSO ownerData)
    {
        if (selectedOwnerText == null) return;

        RequireTmpKoreanFontService().Apply(selectedOwnerText);
        selectedOwnerText.text = ownerData != null
            ? $"{ownerData.characterName}\n{ownerData.ownerSummary}"
            : "사장 미선택";
    }

    private void HandleSelectedOwnerChanged(CharacterSO ownerData)
    {
        RefreshSelectedOwner(ownerData);
        HideIfOwnerSelected(ownerData);
    }

    private void HideIfOwnerSelected(CharacterSO ownerData)
    {
        if (hideAfterOwnerSelected && ownerData != null)
        {
            gameObject.SetActive(false);
        }
    }

    private ITmpKoreanFontService RequireTmpKoreanFontService()
    {
        return tmpKoreanFontService
            ?? throw new InvalidOperationException(
                $"{nameof(OwnerSelectionPanel)} requires VContainer injection of {nameof(ITmpKoreanFontService)}.");
    }

    private IOwnerSelectionOptionButtonFactory RequireOptionButtonFactory()
    {
        return optionButtonFactory
            ?? throw new InvalidOperationException(
                $"{nameof(OwnerSelectionPanel)} requires VContainer injection of {nameof(IOwnerSelectionOptionButtonFactory)}.");
    }

    private static string MakeButtonLabel(CharacterSO candidate)
    {
        if (candidate == null) return "없음";

        string species = !string.IsNullOrWhiteSpace(candidate.SpeciesTag)
            ? candidate.SpeciesTag
            : "Unknown";
        return $"{candidate.characterName}\n{species}";
    }

    private void OnDestroy()
    {
        if (ownerRunManager != null && ownerRunManager.selectedOwnerData != null)
        {
            ownerRunManager.selectedOwnerData.OnValueChange -= HandleSelectedOwnerChanged;
        }
    }
}
