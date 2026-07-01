using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Consideration/CanLookAround", order = 0)]
public class ConsiderationCanLookAround : Consideration
{
    public override float ScoreConsideration(Character character)
    {
        return AILookAround.CanUseVisitLookAround(character) ? 1f : 0f;
    }
}
