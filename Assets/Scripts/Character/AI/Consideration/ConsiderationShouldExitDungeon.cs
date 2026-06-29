using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Consideration/ShouldExitDungeon", order = 0)]
public class ConsiderationShouldExitDungeon : Consideration
{
    public override float ScoreConsideration(Character character)
    {
        AbilityShopping shopping = null;
        character?.TryGetAbility(out shopping);
        return shopping != null && shopping.ShouldExitDungeon() ? 1f : 0f;
    }
}
