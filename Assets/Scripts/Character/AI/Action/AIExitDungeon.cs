using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/ExitDungeon", order = 0)]
public class AIExitDungeon : AIActionSet
{
    public override bool RequiresDestination => false;

    public override bool CanStart(Character character)
    {
        AbilityShopping shopping = null;
        character?.TryGetAbility(out shopping);
        return character != null
            && shopping != null
            && shopping.ShouldExitDungeon();
    }

    public override void Execute(Character character)
    {
        character.TryGetAbility(out AbilityMove move);
        if (move != null)
        {
            move.StartExitDungeon();
            return;
        }

        if (character.ai != null)
        {
            character.ai.isBestActionEnd = true;
        }
    }
}
