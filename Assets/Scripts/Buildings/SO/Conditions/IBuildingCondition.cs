using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuildingCondition
{
    public bool IsSatisfy(Grid grid,List<Vector2Int> buildPos,out string errorMessage);
    public void OnBuild();
}
