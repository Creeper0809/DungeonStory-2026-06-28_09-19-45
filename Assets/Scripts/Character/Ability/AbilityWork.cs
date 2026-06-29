using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityWork : CharacterAbility
{
    public BuildableObject assignedShop;
    public bool isWorking;
    private AbilitySchedule schedule;
    protected override void Awake()
    {
        base.Awake();
        schedule = character.GetAbility<AbilitySchedule>();
        if (schedule != null && schedule.nowSheduleData != null)
        {
            schedule.nowSheduleData.OnValueChange += CheckSchedule;
        }
    }
    public override void Initializtion(CharacterSO data)
    {
        base.Initializtion(data);
        TryAssignShop();
    }

    public bool TryAssignShop(GridPathSearchResult searchResult = null)
    {
        if (assignedShop != null && !assignedShop.isDestroy)
        {
            return true;
        }

        if (grid == null && searchResult == null)
        {
            CacheCommonReferences();
        }
        if (grid == null && searchResult == null)
        {
            return false;
        }

        IEnumerable<BuildableObject> reachableBuildings;
        if (searchResult != null)
        {
            reachableBuildings = searchResult.GetAllVisitableBuilding();
        }
        else
        {
            Vector2Int startPos = grid.GetXY(transform.position);
            startPos = grid.IsValidGridPos(startPos) ? startPos : Vector2Int.zero;
            reachableBuildings = grid.GetAllVisitableBuilding(startPos);
        }

        assignedShop = reachableBuildings
            .FirstOrDefault((building) => building != null && !building.isDestroy && building is Shop);

        return assignedShop != null;
    }

    public void StartWorking()
    {
        if (!TryAssignShop() || character == null || character.ai == null)
        {
            if (character != null && character.ai != null)
            {
                character.AddLog("작업 실패: 작업장 없음");
                character.ai.isBestActionEnd = true;
            }
            return;
        }
        StartCoroutine(Work());
    }
    private IEnumerator Work()
    {
        if (move == null)
        {
            CacheCommonReferences();
        }
        if (move == null || grid == null)
        {
            character.AddLog("작업 실패: 이동 정보 없음");
            character.ai.isBestActionEnd = true;
            yield break;
        }

        yield return move.MoveByCurrentBestActionPath();
        if (grid.GetGridCell(grid.GetXY(transform.position))?.GetBuildingInlayer() == assignedShop && assignedShop is Shop shop)
        {
            yield return shop.AllocateWorker(character);
            isWorking = true;
            StartCoroutine(CheckActionWork());
            yield return new WaitUntil(() => isWorking == false);
            shop.DeallocateWorker(character);
        }
        else
        {
            character.AddLog("작업 실패: 작업장 도달 실패");
        }
        character.ai.isBestActionEnd = true;
    }
    public IEnumerator CheckActionWork()
    {
        while(isWorking && character.ai.bestAction?.actionset is AIWork)
        {
            character.ai.DecideAction();
            yield return new WaitForSeconds(1f);
        }
        isWorking = false;
    }
    public void CheckSchedule(Schedule schedule)
    {
        if(isWorking && schedule != Schedule.WORK)
        {
            isWorking = false;
        }
    }
    private void OnDisable()
    {
        if (schedule != null && schedule.nowSheduleData != null)
        {
            schedule.nowSheduleData.OnValueChange -= CheckSchedule;
        }
    }
}
