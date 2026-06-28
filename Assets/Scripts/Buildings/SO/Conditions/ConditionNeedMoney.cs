using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionNeedMoney : IBuildingCondition
{
    [SerializeField]private int cost;

    public void OnBuild()
    {
        GameManager.Instance.gameData.holdingMoney.Value -= cost;
    }

    public bool IsSatisfy(Grid grid, List<Vector2Int> buildPos,out string errorMessage)
    {
        if (cost > GameManager.Instance.gameData.holdingMoney.Value)
        {
            errorMessage = $"소지중인 돈이 부족합니다";
            return false;
        }
        errorMessage = string.Empty;
        return true;
    }
}
