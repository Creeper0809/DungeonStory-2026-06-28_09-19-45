using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BuildableObject : MonoBehaviour, IInfoable
{
    public int id { get; private set; }
    public Vector2Int centerPos { get; protected set; }
    public List<Vector2Int> buildPoses { get; private set; }

    protected Grid grid;
    public BuildingCategory category { get; private set; }
    protected GameData gameData;

    public event Action OnBuildingDestroyed;
    public bool isDestroy;
    public virtual void Start()
    {
        grid = GridSystemManager.Instance.grid;
    }
    public virtual void Initialization(BuildingSO buildingSO, Vector2Int buildPos)
    {
        id = buildingSO.id;
        isDestroy = false;
        centerPos = buildPos;
        category = buildingSO.category;
        buildPoses = buildingSO.GetGridPosList(buildPos);
        gameData = GameManager.Instance.gameData;
    }
    public void DestroySelf()
    {
        OnBuildingDestroyed?.Invoke();
        isDestroy = true;
        GridTexture.Instance.DeleteBuilding(DataManager.Instance.GetData<BuildingSO>()[id],centerPos);
        Destroy(gameObject);
    }
    public virtual bool isVisitable()
    {
        return false;
    }
    private void OnMouseDown()
    {
        if (GridSystemManager.Instance.gridMode.Value != GridMode.None) return;
        InfoFeedEvent.Trigger(this);
    }
    public InfoFeedEvent.Type GetInfoType()
    {
        return InfoFeedEvent.Type.BUILDING;
    }
}
