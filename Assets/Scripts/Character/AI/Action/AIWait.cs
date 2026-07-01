using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "DungeonStory/AI/Action/Wait", order = 0)]
public class AIWait : AIActionSet
{
    [SerializeField] private float minDuration = 0.5f;
    [SerializeField] private float maxDuration = 1.2f;
    [SerializeField, Range(0f, 1f)] private float onDutyWorkAvailableScore = 0.15f;
    [SerializeField, Range(0f, 1f)] private float offDutyVisitAvailableScore = 0.1f;

    public override bool RequiresDestination => false;

    public override float AdjustScore(Character character, float baseScore)
    {
        if (!CharacterWorkRoleUtility.TryGetWork(character, out AbilityWork work))
        {
            return Mathf.Clamp01(baseScore);
        }

        if (work.IsOffDuty)
        {
            GridPathSearchResult searchResult = character != null && character.ai != null
                ? character.ai.GetPathSearch(character)
                : null;
            if (HasOffDutyVisitCandidate(character, searchResult))
            {
                return Mathf.Clamp01(Mathf.Min(baseScore, offDutyVisitAvailableScore));
            }

            return Mathf.Clamp01(baseScore);
        }

        if (!work.IsOffDuty)
        {
            GridPathSearchResult searchResult = character.ai != null
                ? character.ai.GetPathSearch(character)
                : null;
            if (work.GetWorkUtilityScore(FacilityWorkType.None, searchResult) > 0f)
            {
                return Mathf.Clamp01(Mathf.Min(baseScore, onDutyWorkAvailableScore));
            }
        }

        return Mathf.Clamp01(baseScore);
    }

    public override bool CanStart(Character character)
    {
        if (character == null) return false;

        return CharacterWorkRoleUtility.TryGetWork(character, out _);
    }

    public override void Execute(Character character)
    {
        if (CharacterWorkRoleUtility.TryGetWork(character, out AbilityWork work)
            && (work.IsOffDuty || work.ShouldUseRestProtection()))
        {
            float recovery = Mathf.Max(0f, work.RestRecoveryOnWait);
            work.RecoverOffDuty(recovery, recovery, recovery, 0f);
        }

        float minimumWait = Mathf.Max(0.75f, minDuration);
        float maximumWait = Mathf.Max(minimumWait, maxDuration);
        float duration = Random.Range(minimumWait, maximumWait);
        bool ranIdleBehavior = IdleBehaviorRunner.TryRunDefault(
            character,
            duration,
            true,
            out string behaviorName,
            out string failureReason);
        if (ranIdleBehavior)
        {
            character?.AddLog($"대기: {behaviorName}");
            return;
        }

        character?.AddLog($"대기 이동 불가: {failureReason}");
        if (IdleBehaviorRunner.TryRunStatic(
            character,
            duration,
            out behaviorName,
            out failureReason))
        {
            character?.AddLog($"대기: {behaviorName}");
            return;
        }

        character?.AddLog($"대기 실패: {failureReason}");

        if (character != null && character.ai != null)
        {
            character.ai.isBestActionEnd = true;
        }
    }

    private static bool HasOffDutyVisitCandidate(Character character, GridPathSearchResult searchResult)
    {
        if (character == null)
        {
            return false;
        }

        return FacilityCandidateScorer.HasCandidate(character, searchResult, FacilityRole.Meal)
            || FacilityCandidateScorer.HasCandidate(character, searchResult, FacilityRole.Rest)
            || FacilityCandidateScorer.HasCandidate(character, searchResult, AIShopping.CustomerInterestRoles);
    }
}
