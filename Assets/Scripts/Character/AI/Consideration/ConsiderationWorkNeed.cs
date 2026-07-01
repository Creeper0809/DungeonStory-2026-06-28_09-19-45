using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Consideration/WorkNeed", order = 0)]
public class ConsiderationWorkNeed : Consideration
{
    [SerializeField] private FacilityWorkType workType = FacilityWorkType.None;
    [SerializeField, Range(0f, 1f)] private float minimumScoreWhenAvailable = 0.05f;

    public FacilityWorkType WorkType
    {
        get => workType;
        set => workType = value;
    }

    public override float ScoreConsideration(Character character)
    {
        if (character == null || !character.TryGetAbility(out AbilityWork work))
        {
            return 0f;
        }

        GridPathSearchResult searchResult = character.ai != null
            ? character.ai.GetPathSearch(character)
            : null;
        float utilityScore = work.GetWorkUtilityScore(workType, searchResult);
        if (utilityScore <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(Mathf.Max(minimumScoreWhenAvailable, utilityScore));
    }
}
