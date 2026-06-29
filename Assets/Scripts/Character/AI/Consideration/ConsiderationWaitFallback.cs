using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Consideration/WaitFallback", order = 0)]
public class ConsiderationWaitFallback : Consideration
{
    [SerializeField] private float fallbackScore = 0.05f;

    public override float ScoreConsideration(Character character)
    {
        if (character == null) return 0f;

        character.TryGetAbility(out AbilityShopping shopping);
        if (shopping != null)
        {
            return shopping.ShouldUseVisitFallback() ? fallbackScore : 0f;
        }

        return fallbackScore;
    }
}
