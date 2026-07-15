using UnityEngine;
[CreateAssetMenu(menuName = "DungeonStory/AI/Consideration/Visitable", order = 0)]
public class ConsiderationIsVisitable : Consideration
{
    public Shop.Type type;
    [SerializeField] private FacilityRole role = FacilityRole.None;
    public override float ScoreConsideration(CharacterActor actor)
    {
        AbilityShopping shopping = null;
        actor?.TryGetAbility(out shopping);
        if (shopping == null || shopping.visitCount <= 0)
        {
            return 0f;
        }

        GridPathSearchResult searchResult = actor.Brain != null ? actor.Brain.GetPathSearch(actor) : null;
        FacilityRole targetRole = role != FacilityRole.None ? role : ConvertLegacyType(type);
        if (targetRole != FacilityRole.None)
        {
            return FacilityCandidateScorer.HasCandidate(actor, searchResult, targetRole) ? 1f : 0f;
        }

        foreach (BuildableObject building in actor.GetReachableBuilding())
        {
            if (building != null && building.CanVisit(actor, out _))
            {
                return 1f;
            }
        }

        return 0f;
    }

    private static FacilityRole ConvertLegacyType(Shop.Type type)
    {
        return type switch
        {
            Shop.Type.Food => FacilityRole.Meal,
            Shop.Type.Item => FacilityRole.Purchase,
            _ => FacilityRole.None
        };
    }
}
