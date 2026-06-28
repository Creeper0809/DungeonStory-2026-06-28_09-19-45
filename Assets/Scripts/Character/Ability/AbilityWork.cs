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
        schedule.nowSheduleData.OnValueChange += CheckSchedule;
    }
    public override void Initializtion(CharacterSO data)
    {
        //디버그
        var b = grid.GetXY(transform.position);
        b = grid.IsValidGridPos(b) ? b : Vector2Int.zero;
        assignedShop = grid.GetAllVisitableBuilding(b).Where((building) => building is Shop).First();
        //디버그
    }
    public void StartWorking()
    {
        StartCoroutine(Work());
    }
    private IEnumerator Work()
    {
        yield return move.MoveByPath(character.ai.bestAction.path);
        if (grid.GetGridCell(grid.GetXY(transform.position)).GetBuildingInlayer() == assignedShop && assignedShop is Shop shop)
        {
            yield return shop.AllocateWorker(character);
            isWorking = true;
            StartCoroutine(CheckActionWork());
            yield return new WaitUntil(() => isWorking == false);
            shop.DeallocateWorker(character);
        }
        character.ai.isBestActionEnd = true;
    }
    public IEnumerator CheckActionWork()
    {
        while(isWorking&&character.ai.bestAction.actionset is AIWork)
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
}
