using System.Collections.Generic;
using UnityEngine;

public class ConditionNeedMoney : IBuildingCondition
{
    [SerializeField]private int cost;

    public void OnBuild()
    {
        GameData gameData = GetGameData();
        if (gameData == null) return;

        gameData.holdingMoney.Value -= cost;
    }

    public bool IsSatisfy(Grid grid, List<Vector2Int> buildPos,out string errorMessage)
    {
        GameData gameData = GetGameData();
        if (gameData == null)
        {
            errorMessage = "게임 데이터가 초기화되지 않았습니다";
            return false;
        }

        if (cost > gameData.holdingMoney.Value)
        {
            errorMessage = "소지중인 돈이 부족합니다";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private GameData GetGameData()
    {
        return GameManager.Current != null ? GameManager.Current.gameData : null;
    }
}
