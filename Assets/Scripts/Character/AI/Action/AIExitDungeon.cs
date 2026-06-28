using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/ExitDungeon", order = 0)]
public class AIExitDungeon : AIActionSet
{
    public override void Execute(Character character)
    {
        character.GetAbility<AbilityMove>().StartExitDungeon();
    }
}
