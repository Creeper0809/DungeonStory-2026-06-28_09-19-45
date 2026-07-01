using UnityEngine;
[CreateAssetMenu(menuName = "DungeonStory/AI/Consideration/Visitable", order = 0)]
public class ConsiderationIsVisitable : Consideration
{
    public Shop.Type type;
    [SerializeField] private FacilityRole role = FacilityRole.None;
    public override float ScoreConsideration(Character character)
    {
        AbilityShopping shopping = null;
        character?.TryGetAbility(out shopping);
        if (shopping == null || shopping.visitCount <= 0)
        {
            return 0f;
        }

        GridPathSearchResult searchResult = character.ai != null ? character.ai.GetPathSearch(character) : null;
        FacilityRole targetRole = role != FacilityRole.None ? role : ConvertLegacyType(type);
        if (targetRole != FacilityRole.None)
        {
            return FacilityCandidateScorer.HasCandidate(character, searchResult, targetRole) ? 1f : 0f;
        }

        foreach (BuildableObject building in character.GetReachableBuilding())
        {
            if (building != null && building.CanVisit(character, out _))
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
