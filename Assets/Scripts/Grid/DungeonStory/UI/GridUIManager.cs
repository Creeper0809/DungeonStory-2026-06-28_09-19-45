using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridUIManager : MonoBehaviour
{
    private GridSystemManager gridSystemManager;
    private DungeonStoryGridBuildingController buildingController;
    public GameObject gridTextureCanvas;
    public UIBuildingInfo buildingInfoUI;

    public GameObject constructPanel;

    public Action OnUiClose;

    private void Awake()
    {
        if (!gridTextureCanvas) gridTextureCanvas = GameObject.FindGameObjectWithTag("GridCanvas");
        gridSystemManager = GridSystemManager.Instance;
        buildingController = DungeonStoryGridBuildingController.Instance;

    }
    void Start()
    {
        DrawGrid();
        HideGrid();
    }
    // Update is called once per frame
    void Update()
    {
        if (gridSystemManager == null || buildingController == null) return;

        if (gridSystemManager.Mode == GridMode.None&&Input.GetMouseButtonDown(0))
        {
            var building = buildingController.GetBuildingByMousePos();
            if (!building) return;
            buildingInfoUI.DisplayBuildingInfo(building);
        }
    }
    public void ToggleConstructTab()
    {
        if (constructPanel.activeSelf)
        {
            CloseConstructTab();
        }
        else
        {
            ShowConstructTab();
        }
    }
    public void ShowConstructTab()
    {
        constructPanel.SetActive(true);
    }
    public void CloseConstructTab()
    {
        OnUiClose?.Invoke();
        constructPanel.SetActive(false);
    }
    public void HideGrid()
    {
        gridTextureCanvas.SetActive(false);
    }
    public void ShowGrid()
    {
        gridTextureCanvas.SetActive(true);
    }
    public void DrawGrid()
    {
        if (!gridTextureCanvas)
        {
            Debug.Log("GridUIManager.DrawGrid() : 캔버스가 존재하지 않습니다");
            return;
        }
    }
    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
       
    }
}
