using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ConditionNeedToConnect : IBuildingCondition
{
    public bool connectWithGate;
    [HideIf("connectWithGate")]
    public int associatedId;

    public void OnBuild() {}

    public bool IsSatisfy(Grid grid, List<Vector2Int> buildPos,out string errorMessage)
    {
        bool isStisfy = false;
        errorMessage = "";
        if (connectWithGate)
        {
            if (grid.IsConnectedWithGate(buildPos)) isStisfy = true;
            else errorMessage = $"건물이 성문과 연결되지 않았습니다.";
        }
        else
        {
            if (grid.IsConneted(buildPos[0], associatedId)) isStisfy = true;
            else errorMessage = $"건물 연결 조건을 만족하지 못했습니다 ";
        }
        return isStisfy;
    }
}
