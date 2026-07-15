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

    public override float AdjustScore(CharacterActor actor, float baseScore)
    {
        if (!CharacterWorkRoleUtility.TryGetWork(actor, out AbilityWork work))
        {
            return Mathf.Clamp01(baseScore);
        }

        if (work.IsOffDuty)
        {
            GridPathSearchResult searchResult = actor != null && actor.Brain != null
                ? actor.Brain.GetPathSearch(actor)
                : null;
            if (HasOffDutyVisitCandidate(actor, searchResult))
            {
                return Mathf.Clamp01(Mathf.Min(baseScore, offDutyVisitAvailableScore));
            }

            return Mathf.Clamp01(baseScore);
        }

        if (!work.IsOffDuty)
        {
            GridPathSearchResult searchResult = actor.Brain != null
                ? actor.Brain.GetPathSearch(actor)
                : null;
            if (work.GetWorkUtilityScore(FacilityWorkType.None, searchResult) > 0f)
            {
                return Mathf.Clamp01(Mathf.Min(baseScore, onDutyWorkAvailableScore));
            }
        }

        return Mathf.Clamp01(baseScore);
    }

    public override bool CanStart(CharacterActor actor)
    {
        return actor != null && CharacterWorkRoleUtility.TryGetWork(actor, out _);
    }

    public override void Execute(CharacterActor actor)
    {
        if (CharacterWorkRoleUtility.TryGetWork(actor, out AbilityWork work)
            && (work.IsOffDuty || work.ShouldUseRestProtection()))
        {
            float recovery = Mathf.Max(0f, work.RestRecoveryOnWait);
            work.RecoverOffDuty(recovery, recovery, recovery, 0f);
        }

        float minimumWait = Mathf.Max(0.75f, minDuration);
        float maximumWait = Mathf.Max(minimumWait, maxDuration);
        float duration = Random.Range(minimumWait, maximumWait);
        bool ranIdleBehavior = IdleBehaviorRunner.TryRunDefault(
            actor,
            duration,
            true,
            out string behaviorName,
            out string failureReason);
        if (ranIdleBehavior)
        {
            return;
        }

        actor?.AddLog($"대기 이동 불가: {failureReason}");
        if (IdleBehaviorRunner.TryRunStatic(
            actor,
            duration,
            out behaviorName,
            out failureReason))
        {
            return;
        }

        actor?.AddLog($"대기 실패: {failureReason}");

        if (actor != null && actor.Brain != null)
        {
            actor.Brain.isBestActionEnd = true;
        }
    }

    private static bool HasOffDutyVisitCandidate(CharacterActor actor, GridPathSearchResult searchResult)
    {
        if (actor == null)
        {
            return false;
        }

        FacilityRole interestRoles = actor.TryGetAbility(out AbilityShopping shopping)
            ? shopping.GetInterestRoles()
            : CharacterVisitPolicy.CustomerInterestRoles;
        return FacilityCandidateScorer.HasCandidate(actor, searchResult, FacilityRole.Meal)
            || FacilityCandidateScorer.HasCandidate(actor, searchResult, FacilityRole.Rest)
            || FacilityCandidateScorer.HasCandidate(actor, searchResult, interestRoles);
    }
}
