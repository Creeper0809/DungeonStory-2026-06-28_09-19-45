using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/Wait", order = 0)]
public class AIWait : AIActionSet
{
    [SerializeField] private float minDuration = 0.5f;
    [SerializeField] private float maxDuration = 1.2f;

    public override bool RequiresDestination => false;

    public override bool CanStart(Character character)
    {
        if (character == null) return false;

        character.TryGetAbility(out AbilityShopping shopping);
        if (shopping != null)
        {
            return shopping.ShouldUseVisitFallback();
        }

        return true;
    }

    public override void Execute(Character character)
    {
        float duration = Random.Range(minDuration, maxDuration);
        character.TryGetAbility(out AbilityMove move);
        if (move != null)
        {
            move.StartWait(duration);
            return;
        }

        if (character.ai != null)
        {
            character.ai.isBestActionEnd = true;
        }
    }
}
