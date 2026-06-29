using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildableObject : MonoBehaviour, IGridOccupant, IGridMovementOccupant
{
    public int id { get; private set; }
    public Vector2Int centerPos { get; protected set; }
    public List<Vector2Int> buildPoses { get; private set; }
    public BuildingSO BuildingData { get; private set; }

    protected Grid grid;
    public BuildingCategory category { get; private set; }

    public event Action OnBuildingDestroyed;
    public event Action<BuildableObject> OnBuildingClicked;
    public bool isDestroy;
    public int GridId => id;
    public bool IsGridDestroyed => isDestroy;
    public bool IsGridVisitable => isVisitable();
    public bool IsGridMovement => category == BuildingCategory.Movement;
    public virtual GridMoveType GridMoveType => IsGridMovement ? GridMoveType.Instant : GridMoveType.Walk;
    public Grid Grid => grid;

    public virtual void Start()
    {
    }

    public virtual void SetGrid(Grid grid)
    {
        this.grid = grid;
    }

    public virtual void Initialization(BuildingSO buildingSO, Vector2Int buildPos)
    {
        BuildingData = buildingSO;
        GridBuildingPlacement placement = buildingSO.Placement;
        id = buildingSO.id;
        isDestroy = false;
        centerPos = buildPos;
        category = placement.Category;
        buildPoses = placement.GetGridPosList(buildPos);
    }

    public virtual Vector3 GetMovementWorldPosition(Vector2Int gridPosition)
    {
        if (grid == null)
        {
            return transform.position;
        }

        Vector3 anchor = grid.GetWorldPos(gridPosition);
        if (BuildingData != null)
        {
            anchor += (Vector3)BuildingData.movementAnchorOffset;
        }

        return anchor;
    }

    public void DestroySelf()
    {
        OnBuildingDestroyed?.Invoke();
        isDestroy = true;
        Destroy(gameObject);
    }

    public virtual bool isVisitable()
    {
        return false;
    }

    private void OnMouseDown()
    {
        OnBuildingClicked?.Invoke(this);
    }

}
