using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Consideration/CanLookAround", order = 0)]
public class ConsiderationCanLookAround : Consideration
{
    public override float ScoreConsideration(Character character)
    {
        AbilityShopping shopping = null;
        character?.TryGetAbility(out shopping);
        return shopping != null && shopping.CanLookAround() ? 1f : 0f;
    }
}
