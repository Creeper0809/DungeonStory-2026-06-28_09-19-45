using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GridGhostObject : MonoBehaviour
{
    private GameObject ghostObject;
    private GridSystemManager gridSystemManager;
    private SpriteRenderer ghostSpriteRenderer;
    private bool hidden;

    void Awake()
    {
        ghostObject = transform.GetChild(0).gameObject;
        gridSystemManager = GridSystemManager.Instance;
        ghostSpriteRenderer = ghostObject.GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
       
    }
    void Update()
    {
        if (hidden) return;
        Vector3 evenOffset = new Vector2(0.5f, 0);
        if (!gridSystemManager.isDragging)
        {
            Vector2Int targetPos = gridSystemManager.grid.GetXY(GameManager.Instance.GetMouseWorldPos());
            OnBuildableChange(gridSystemManager.selectedBuilding.Value.IsSatisfyConditionOnBuild(gridSystemManager.grid, targetPos));
            Vector3 pos = gridSystemManager.GetMouseWorldPosSnapped();
            Vector2 instatiatePos = gridSystemManager.selectedBuilding.Value.width % 2 == 0 ? pos + evenOffset : pos;
            ghostObject.transform.position = Vector3.Lerp(ghostObject.transform.position, new Vector3(instatiatePos.x, instatiatePos.y, GridSystemManager.Instance.gridOriginPos.z), Time.unscaledDeltaTime * 25f);
            return;
        }
        ghostSpriteRenderer.size = new Vector3(gridSystemManager.selectedBuilding.Value.width, 3);
        var poses = gridSystemManager.totalSelectedPos;
        var miny = poses.Min(pos => pos.y);
        var minx = poses.Min(pos => pos.x) + (gridSystemManager.xCount / 2);
        var pos2 = gridSystemManager.grid.GetWorldPos(new Vector2Int(minx, miny));
        ghostObject.transform.position = gridSystemManager.selectedBuilding.Value.width *  gridSystemManager.xCount% 2 == 0 ? pos2 + evenOffset: pos2;
        ghostSpriteRenderer.size = new Vector3(gridSystemManager.selectedBuilding.Value.width * gridSystemManager.xCount, gridSystemManager.yCount * 3);
    }
    private void OnBuildableChange(bool buildable)
    {
        if (buildable)
        {
            ghostSpriteRenderer.color = Color.green;
        }
        else
        {
            ghostSpriteRenderer.color = Color.red;
        }
    }
    private void OnGridModeChanged(GridMode gridMode)
    {
        if (gridMode == GridMode.None) CleanObject();
    }
    public void OnSelectedChanged(BuildingSO buildingSO)
    {
        if (gridSystemManager.gridMode.Value != GridMode.Build) return;
        CleanObject();
        if(buildingSO != null)
        {
            ghostObject.transform.position = GameManager.Instance.GetMouseWorldPos();
            RefreshVisual(buildingSO);
        }
    }
    private void RefreshVisual(BuildingSO buildingSO)
    {
        hidden = false;
        ghostSpriteRenderer.sprite = buildingSO.sprite;
    }
    private void CleanObject()
    {
        hidden = true;
        ghostSpriteRenderer.sprite = null;
    }
    private void OnEnable()
    {
        gridSystemManager.selectedBuilding.OnValueChange += OnSelectedChanged;
        gridSystemManager.gridMode.OnValueChange += OnGridModeChanged;
    }
    private void OnDisable()
    {
        gridSystemManager.selectedBuilding.OnValueChange -= OnSelectedChanged;
        gridSystemManager.gridMode.OnValueChange -= OnGridModeChanged;
    }
}
