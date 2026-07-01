using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingInfo : UtilSingleton<UIBuildingInfo>
{
    private BuildableObject selectedBuilding;
    private CanvasGroup canvasGroup;
    private Image buildingImage;
    private RectTransform buildingImageSize;

    public GameObject buildingImageObject;

    public List<UIConfig<TMP_Text>> simpleInfoText;
    public TMP_Text nameText;

    public GameObject textPrefab;
    public GameObject simpleInfoPanel;
    
    private bool hidden = true;

    protected override void Awake()
    {
        base.Awake();
        canvasGroup = GetComponent<CanvasGroup>();
        buildingImage = buildingImageObject.GetComponent<Image>();
        buildingImageSize = buildingImageObject.GetComponent<RectTransform>();
    }
    void Start()
    {
        canvasGroup.alpha = 0f;
        CloseDispaly();
    }
    
    public void DisplayBuildingInfo(BuildableObject building)
    {
        return;
        if (building != selectedBuilding && !hidden) return;
        selectedBuilding = building;
        BuildingSO buildingData = DataManager.Instance.GetData<BuildingSO>()[building.id];
        if(buildingData == null)
        {
            return;
        }
        OpenDispaly();
        if (buildingImageObject)
        {
            Vector2 size = new Vector2((buildingData.width/3) * 160, 160);
            buildingImageSize.sizeDelta = size;
            buildingImage.sprite = buildingData.icon;
        }
        nameText.text = buildingData.objectName;
        
        foreach(UIConfig<TMP_Text> ui in simpleInfoText)
        {
            ui.uiObject.gameObject.SetActive(false);
        }
        var explains = new Dictionary<string, string>();
        foreach (UIConfig<TMP_Text> ui in simpleInfoText)
        {
            if (explains.ContainsKey(ui.name))
            {
                ui.uiObject.gameObject.SetActive(true);
                ui.uiObject.text = explains[ui.name];
            }
        }
    }
    public void OpenDispaly()
    {
        canvasGroup.DOFade(1.0f, 0.1f).OnComplete(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            hidden = false;
            UIManager.Instance.MakeTouchFalse();
        });
    }
    public void CloseDispaly()
    {
        canvasGroup.DOFade(0f, 0.1f).OnComplete(() =>
        {
            hidden = true;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            selectedBuilding = null;
            UIManager.Instance.MakeTouchTrue();
        });
    }
}
[Serializable]
public class UIConfig<T>
{
    public string name;
    public T uiObject;
}
