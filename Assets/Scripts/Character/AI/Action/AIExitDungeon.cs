using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/ExitDungeon", order = 0)]
public class AIExitDungeon : AIActionSet
{
    public override bool RequiresDestination => false;

    public override bool CanStart(CharacterActor actor)
    {
        AbilityShopping shopping = null;
        actor?.TryGetAbility(out shopping);
        return actor != null
            && shopping != null
            && shopping.ShouldExitDungeon();
    }

    public override void Execute(CharacterActor actor)
    {
        AbilityMove move = null;
        actor?.TryGetAbility(out move);
        if (move != null)
        {
            move.StartExitDungeon();
            return;
        }

        if (actor != null && actor.Brain != null)
        {
            actor.Brain.isBestActionEnd = true;
        }
    }
}
