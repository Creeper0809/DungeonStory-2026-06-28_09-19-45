using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OwnerSelectionPanel : MonoBehaviour
{
    [SerializeField] private OwnerRunManager ownerRunManager;
    [SerializeField] private Transform optionRoot;
    [SerializeField] private Button optionButtonPrefab;
    [SerializeField] private TMP_Text selectedOwnerText;
    [SerializeField] private bool buildOptionsOnStart = true;

    private void Start()
    {
        TMPKoreanFont.ApplyToChildren(transform);

        if (ownerRunManager == null)
        {
            ownerRunManager = OwnerRunManager.Instance;
        }

        if (buildOptionsOnStart)
        {
            BuildOptions();
        }

        RefreshSelectedOwner(ownerRunManager != null ? ownerRunManager.selectedOwnerData.Value : null);
        if (ownerRunManager != null && ownerRunManager.selectedOwnerData != null)
        {
            ownerRunManager.selectedOwnerData.OnValueChange += RefreshSelectedOwner;
        }
    }

    public void BuildOptions()
    {
        if (ownerRunManager == null)
        {
            ownerRunManager = OwnerRunManager.Instance;
        }
        if (ownerRunManager == null || optionRoot == null || optionButtonPrefab == null)
        {
            return;
        }

        for (int i = optionRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(optionRoot.GetChild(i).gameObject);
        }

        CharacterSO[] candidates = ownerRunManager.OwnerCandidates;
        for (int i = 0; i < candidates.Length; i++)
        {
            CharacterSO candidate = candidates[i];
            Button button = Instantiate(optionButtonPrefab, optionRoot);
            button.name = $"OwnerOption_{candidate.characterName}";

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                TMPKoreanFont.Apply(label);
                label.text = MakeButtonLabel(candidate);
            }

            int index = i;
            button.onClick.AddListener(() => ownerRunManager.SelectOwnerByIndex(index));
        }
    }

    private void RefreshSelectedOwner(CharacterSO ownerData)
    {
        if (selectedOwnerText == null) return;

        TMPKoreanFont.Apply(selectedOwnerText);
        selectedOwnerText.text = ownerData != null
            ? $"{ownerData.characterName}\n{ownerData.ownerSummary}"
            : "사장 미선택";
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
            ownerRunManager.selectedOwnerData.OnValueChange -= RefreshSelectedOwner;
        }
    }
}
