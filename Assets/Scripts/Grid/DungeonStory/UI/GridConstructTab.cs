using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridConstructTab : UITab
{
    [Tooltip("카테고리 버튼")]
    public GameObject buildingCategorySelectButtonPrefab;
    [Tooltip("건물 선택 버튼의 패널")]
    public GameObject seleButtonPanelPrefab;
    [Tooltip("건물 선택 버튼")]
    public GameObject selectButtonPrefab;

    public List<UITab> selectButtonPanelList;
    void Start()
    {
        MakeSelectButton();
    }
    public override void OnClose()
    {
        base.OnClose();
        DungeonStoryGridBuildingController.Instance.SetGridModeNone();
    }
    public override void OnOpen()
    {
        base.OnOpen();
        UIManager.Instance.CloseAllPopup();
        RefreshCategoryLabels();
    }
    private void MakeSelectButton()
    {
        foreach (BuildingSO so in DataManager.Instance.GetData<BuildingSO>().Values.Where((x)=>x.unlocked).OrderBy((x) => x.id))
        {
            Debug.Log($"{so.objectName} 건물 설치 버튼 생성");
            // 빌딩 카테고리 UI 생성
            if (!selectButtonPanelList.Where((x) => x.id == (int)so.category).Any())
            {
                UITab panel = Instantiate(seleButtonPanelPrefab, gameObject.transform).GetComponent<UITab>();
                panel.id = (int)so.category;
                panel.gameObject.name = "BuildingSelect" + so.category;
                Button selectButton = Instantiate(buildingCategorySelectButtonPrefab, transform.GetChild(0)).GetComponent<Button>();
                TMP_Text label = selectButton.transform.GetChild(0).GetComponent<TMP_Text>();
                TMPKoreanFont.Apply(label);
                label.text = GetCategoryDisplayName(so.category);
                selectButton.onClick.AddListener(() =>
                {
                    ToggleSelectButton((int)so.category);
                });
                selectButtonPanelList.Add(panel);
            }
            // 빌딩 선택 UI 생성
            Transform selectPanel = selectButtonPanelList.Where((x) => x.id == (int)so.category).First().transform;
            Instantiate(selectButtonPrefab, selectPanel).GetComponent<UIBuildingSelectButton>().Initialization(so);
        }
        foreach(UITab panel in selectButtonPanelList)
        {
            panel.gameObject.SetActive(false);
        }
        RefreshCategoryLabels();
    }
    public void ToggleSelectButton(int category)
    {
        UITab temp = null;
        foreach(var tab in selectButtonPanelList)
        {
            if (tab.id == category) temp = tab;
            else tab.CloseTab();
        }
        DungeonStoryGridBuildingController.Instance.SetGridModeNone();
        temp.Toggle();
    }

    public void RefreshCategoryLabels()
    {
        foreach (TMP_Text label in transform.GetComponentsInChildren<TMP_Text>(true))
        {
            if (label == null) continue;

            if (TryGetCategoryDisplayName(label.text, out string displayName))
            {
                TMPKoreanFont.Apply(label);
                label.text = displayName;
            }
        }
    }

    private static string GetCategoryDisplayName(BuildingCategory category)
    {
        return category switch
        {
            BuildingCategory.Wall => "벽",
            BuildingCategory.Shop => "상점",
            BuildingCategory.Special => "특수",
            BuildingCategory.Movement => "이동",
            BuildingCategory.Production => "생산",
            BuildingCategory.Crafting => "제작",
            BuildingCategory.Resource => "자원",
            _ => "기타"
        };
    }

    private static bool TryGetCategoryDisplayName(string rawText, out string displayName)
    {
        displayName = string.Empty;
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return false;
        }

        string normalized = rawText.Trim();
        if (!System.Enum.TryParse(normalized, true, out BuildingCategory category))
        {
            return false;
        }

        displayName = GetCategoryDisplayName(category);
        return true;
    }
}
