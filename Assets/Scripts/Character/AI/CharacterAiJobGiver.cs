using System;
using UnityEngine;

public readonly struct CharacterAiActionCandidate
{
    public CharacterAiActionCandidate(
        AIAction action,
        float score,
        AIActionFailure failure,
        string debugLabel)
    {
        Action = action;
        Score = Mathf.Clamp01(score);
        Failure = failure;
        DebugLabel = debugLabel ?? string.Empty;
    }

    public AIAction Action { get; }
    public AIActionSet ActionSet => Action != null ? Action.actionset : null;
    public float Score { get; }
    public AIActionFailure Failure { get; }
    public string DebugLabel { get; }
    public bool HasAction => Action != null && Action.actionset != null && Score > 0f;
}

public readonly struct CharacterAiJobCandidate
{
    public CharacterAiJobCandidate(
        CharacterAiBranch branch,
        string jobGiverName,
        CharacterAiActionCandidate actionCandidate,
        float domainScore,
        float utility,
        string reason)
    {
        Branch = branch;
        JobGiverName = jobGiverName ?? string.Empty;
        ActionCandidate = actionCandidate;
        DomainScore = Mathf.Clamp01(domainScore);
        Utility = Mathf.Clamp01(utility);
        Reason = reason ?? string.Empty;
    }

    public CharacterAiBranch Branch { get; }
    public string JobGiverName { get; }
    public CharacterAiActionCandidate ActionCandidate { get; }
    public float DomainScore { get; }
    public float Utility { get; }
    public string Reason { get; }
    public bool IsValid => Utility > 0f && ActionCandidate.HasAction;
    public string DebugSummary =>
        $"{JobGiverName} domain={DomainScore:0.###} action={ActionCandidate.Score:0.###} utility={Utility:0.###} {Reason}".Trim();
}

public abstract class CharacterAiJobGiver
{
    public abstract CharacterAiBranch Branch { get; }
    public abstract string Name { get; }

    public bool TryEvaluate(CharacterActor actor, out CharacterAiJobCandidate candidate)
    {
        float domainScore = GetDomainScore(actor, out string domainReason);
        domainScore = CharacterMoodImpulseUtility.ApplyJobGiverBias(
            actor,
            Branch,
            domainScore,
            out string moodReason);
        domainReason = CharacterMoodImpulseUtility.AppendReason(domainReason, moodReason);
        if (domainScore <= 0f)
        {
            candidate = CreateRejected(domainScore, domainReason);
            return false;
        }

        AIBrain brain = actor != null ? actor.Brain : null;
        if (brain == null)
        {
            candidate = CreateRejected(domainScore, "AIBrain is missing.");
            return false;
        }

        if (!brain.TryFindBestScoredAction(MatchesAction, out CharacterAiActionCandidate actionCandidate))
        {
            candidate = CreateRejected(
                domainScore,
                string.IsNullOrWhiteSpace(actionCandidate.DebugLabel)
                    ? actionCandidate.Failure.ToString()
                    : actionCandidate.DebugLabel);
            return false;
        }

        float utility = CombineUtility(domainScore, actionCandidate.Score);
        candidate = new CharacterAiJobCandidate(
            Branch,
            Name,
            actionCandidate,
            domainScore,
            utility,
            domainReason);
        return candidate.IsValid;
    }

    public abstract bool MatchesAction(AIActionSet actionSet);

    protected abstract float GetDomainScore(CharacterActor actor, out string reason);

    protected virtual float CombineUtility(float domainScore, float actionScore)
    {
        return Mathf.Clamp01(domainScore * actionScore);
    }

    public static float Need(CharacterActor actor, CharacterCondition condition)
    {
        CharacterStats stats = actor != null ? actor.Stats : null;
        if (stats == null
            || stats.Stats == null
            || !stats.Stats.TryGetValue(condition, out float value))
        {
            return 0.5f;
        }

        return Mathf.Clamp01(1f - value / 100f);
    }

    public static float StatRatio(CharacterActor actor, CharacterCondition condition)
    {
        CharacterStats stats = actor != null ? actor.Stats : null;
        if (stats == null
            || stats.Stats == null
            || !stats.Stats.TryGetValue(condition, out float value))
        {
            return 0.5f;
        }

        return Mathf.Clamp01(value / 100f);
    }

    protected static float InterestMultiplier(CharacterActor actor, AIActionSet actionSet)
    {
        return Mathf.Clamp01(CharacterAiPersonalityUtility.GetActionScoreMultiplier(actor, actionSet) / 2f);
    }

    private CharacterAiJobCandidate CreateRejected(float domainScore, string reason)
    {
        return new CharacterAiJobCandidate(
            Branch,
            Name,
            default,
            domainScore,
            0f,
            reason);
    }
}

public static class CharacterAiRoutinePriority
{
    private const float SevereNeed = 0.65f;
    private const float MildNeed = 0.25f;

    public static float GetPriority(
        CharacterActor actor,
        CharacterAiBranch routineBranch,
        out string reason)
    {
        if (actor == null || !actor.CanRunAi)
        {
            reason = "AI cannot run";
            return 0f;
        }

        float priority = routineBranch switch
        {
            CharacterAiBranch.SurvivalNeeds => GetSurvivalPriority(actor, out reason),
            CharacterAiBranch.DutyWork => GetDutyPriority(actor, out reason),
            CharacterAiBranch.LeisureVisit => GetLeisurePriority(actor, out reason),
            CharacterAiBranch.Idle => GetIdlePriority(actor, out reason),
            _ => ReturnNoPriority(out reason)
        };

        priority = CharacterMoodImpulseUtility.ApplyRoutineBias(
            actor,
            routineBranch,
            priority,
            out string moodReason);
        reason = CharacterMoodImpulseUtility.AppendReason(reason, moodReason);
        return priority;
    }

    private static float GetSurvivalPriority(CharacterActor actor, out string reason)
    {
        float hungerNeed = CharacterAiJobGiver.Need(actor, CharacterCondition.HUNGER);
        float sleepNeed = CharacterAiJobGiver.Need(actor, CharacterCondition.SLEEP);
        float excretionNeed = CharacterAiJobGiver.Need(actor, CharacterCondition.EXCRETION);
        float hygieneNeed = CharacterAiJobGiver.Need(actor, CharacterCondition.HYGIENE);
        float restNeed = FacilityCandidateScorer.GetNeedScore(actor, FacilityRole.Rest);
        float injuryNeed = actor != null ? actor.InjurySeverity : 0f;
        float exitNeed = ShouldExitDungeon(actor) ? 1f : 0f;
        float strongestNeed = Mathf.Max(
            exitNeed,
            hungerNeed,
            sleepNeed,
            restNeed,
            excretionNeed,
            hygieneNeed,
            injuryNeed);
        reason =
            $"need={strongestNeed:0.###} hunger={hungerNeed:0.###} sleep={sleepNeed:0.###} "
            + $"toilet={excretionNeed:0.###} hygiene={hygieneNeed:0.###} rest={restNeed:0.###} exit={exitNeed:0.###}";
        if (strongestNeed <= 0.05f)
        {
            return 0f;
        }

        if (strongestNeed >= SevereNeed)
        {
            return 95f + strongestNeed * 5f;
        }

        if (strongestNeed >= MildNeed)
        {
            return 35f + strongestNeed * 30f;
        }

        return strongestNeed * 25f;
    }

    private static float GetDutyPriority(CharacterActor actor, out string reason)
    {
        if (!CharacterWorkRoleUtility.TryGetWork(actor, out AbilityWork work))
        {
            reason = "not a worker";
            return 0f;
        }

        if (work.IsOffDuty)
        {
            reason = "off duty";
            return 0f;
        }

        float survivalPressure = GetSurvivalPressure(actor);
        float wellness = 1f - Mathf.Clamp01((survivalPressure - MildNeed) / (1f - MildNeed));
        float priority = Mathf.Lerp(8f, 82f, wellness);
        reason = $"onDuty survival={survivalPressure:0.###} wellness={wellness:0.###}";
        return priority;
    }

    private static float GetLeisurePriority(CharacterActor actor, out string reason)
    {
        if (!CanUseLeisure(actor))
        {
            reason = "leisure unavailable";
            return 0f;
        }

        float funNeed = CharacterAiJobGiver.Need(actor, CharacterCondition.FUN);
        float moodNeed = CharacterAiJobGiver.Need(actor, CharacterCondition.MOOD);
        float shoppingNeed = FacilityCandidateScorer.GetNeedScore(actor, FacilityRole.Purchase);
        float urgentSurvival = Mathf.Max(
            CharacterAiJobGiver.Need(actor, CharacterCondition.HUNGER),
            CharacterAiJobGiver.Need(actor, CharacterCondition.SLEEP),
            CharacterAiJobGiver.Need(actor, CharacterCondition.EXCRETION),
            CharacterAiJobGiver.Need(actor, CharacterCondition.HYGIENE) * 0.7f);
        float leisureNeed = Mathf.Max(funNeed, moodNeed * 0.75f, shoppingNeed);
        float survivalWindow = Mathf.Clamp01(1f - urgentSurvival * 0.85f);
        float priority = Mathf.Clamp01(leisureNeed * survivalWindow) * 70f;
        reason = $"leisure={leisureNeed:0.###} fun={funNeed:0.###} mood={moodNeed:0.###} shopping={shoppingNeed:0.###} survivalWindow={survivalWindow:0.###}";
        return priority;
    }

    private static float GetIdlePriority(CharacterActor actor, out string reason)
    {
        reason = "no stronger routine";
        return actor != null && actor.CanRunAi ? 1f : 0f;
    }

    private static float ReturnNoPriority(out string reason)
    {
        reason = "unsupported routine";
        return 0f;
    }

    private static float GetSurvivalPressure(CharacterActor actor)
    {
        return Mathf.Max(
            CharacterAiJobGiver.Need(actor, CharacterCondition.HUNGER),
            CharacterAiJobGiver.Need(actor, CharacterCondition.SLEEP),
            CharacterAiJobGiver.Need(actor, CharacterCondition.EXCRETION),
            CharacterAiJobGiver.Need(actor, CharacterCondition.HYGIENE) * 0.7f,
            CharacterAiJobGiver.Need(actor, CharacterCondition.MOOD) * 0.8f,
            actor != null ? actor.InjurySeverity : 0f);
    }

    private static bool CanUseLeisure(CharacterActor actor)
    {
        if (actor == null)
        {
            return false;
        }

        if (CharacterWorkRoleUtility.TryGetWork(actor, out AbilityWork work))
        {
            return work.IsOffDuty;
        }

        return actor.TryGetAbility(out AbilityShopping _);
    }

    private static bool ShouldExitDungeon(CharacterActor actor)
    {
        return actor != null
            && actor.TryGetAbility(out AbilityShopping shopping)
            && shopping.ShouldExitDungeon();
    }
}

public sealed class ExitDungeonJobGiver : CharacterAiJobGiver
{
    public override CharacterAiBranch Branch => CharacterAiBranch.ExitDungeon;
    public override string Name => "ExitDungeonJobGiver";
    public override bool MatchesAction(AIActionSet actionSet) => actionSet is AIExitDungeon;

    protected override float GetDomainScore(CharacterActor actor, out string reason)
    {
        reason = "exit intent";
        return actor != null && actor.CanRunAi ? 1f : 0f;
    }
}

public sealed class GetFoodJobGiver : CharacterAiJobGiver
{
    public override CharacterAiBranch Branch => CharacterAiBranch.Eat;
    public override string Name => "GetFoodJobGiver";
    public override bool MatchesAction(AIActionSet actionSet) => actionSet is AIEat;

    protected override float GetDomainScore(CharacterActor actor, out string reason)
    {
        float hungerNeed = FacilityCandidateScorer.GetNeedScore(actor, FacilityRole.Meal);
        reason = $"hungerNeed={hungerNeed:0.###}";
        return hungerNeed;
    }
}

public sealed class RestJobGiver : CharacterAiJobGiver
{
    public override CharacterAiBranch Branch => CharacterAiBranch.Rest;
    public override string Name => "RestJobGiver";
    public override bool MatchesAction(AIActionSet actionSet) => actionSet is AIRest;

    protected override float GetDomainScore(CharacterActor actor, out string reason)
    {
        float restNeed = FacilityCandidateScorer.GetNeedScore(actor, FacilityRole.Rest);
        float injuryNeed = actor != null ? actor.InjurySeverity : 0f;
        float domain = Mathf.Max(restNeed, injuryNeed * 0.85f);
        reason = $"restNeed={restNeed:0.###} injury={injuryNeed:0.###}";
        return domain;
    }
}

public sealed class ToiletJobGiver : CharacterAiJobGiver
{
    public override CharacterAiBranch Branch => CharacterAiBranch.Toilet;
    public override string Name => "ToiletJobGiver";

    public override bool MatchesAction(AIActionSet actionSet)
    {
        return actionSet is AIFacilityRoleAction roleAction
            && roleAction.Role == FacilityRole.Toilet;
    }

    protected override float GetDomainScore(CharacterActor actor, out string reason)
    {
        float toiletNeed = FacilityCandidateScorer.GetNeedScore(actor, FacilityRole.Toilet);
        reason = $"toiletNeed={toiletNeed:0.###}";
        return toiletNeed;
    }
}

public sealed class HygieneJobGiver : CharacterAiJobGiver
{
    public override CharacterAiBranch Branch => CharacterAiBranch.Hygiene;
    public override string Name => "HygieneJobGiver";

    public override bool MatchesAction(AIActionSet actionSet)
    {
        return actionSet is AIFacilityRoleAction roleAction
            && roleAction.Role == FacilityRole.Hygiene;
    }

    protected override float GetDomainScore(CharacterActor actor, out string reason)
    {
        float hygieneNeed = FacilityCandidateScorer.GetNeedScore(actor, FacilityRole.Hygiene);
        reason = $"hygieneNeed={hygieneNeed:0.###}";
        return hygieneNeed;
    }
}

public sealed class WorkJobGiver : CharacterAiJobGiver
{
    public override CharacterAiBranch Branch => CharacterAiBranch.Work;
    public override string Name => "WorkJobGiver";
    public override bool MatchesAction(AIActionSet actionSet) => actionSet is AIWork;

    protected override float GetDomainScore(CharacterActor actor, out string reason)
    {
        if (!CharacterWorkRoleUtility.TryGetWork(actor, out AbilityWork work))
        {
            reason = "not a worker";
            return 0f;
        }

        if (work.IsOffDuty)
        {
            reason = "off duty";
            return 0f;
        }

        float survivalPressure = Mathf.Max(
            Need(actor, CharacterCondition.HUNGER),
            Need(actor, CharacterCondition.SLEEP),
            Need(actor, CharacterCondition.EXCRETION),
            Need(actor, CharacterCondition.HYGIENE) * 0.7f,
            Need(actor, CharacterCondition.MOOD) * 0.8f);
        float wellness = 1f - Mathf.Clamp01((survivalPressure - 0.25f) / 0.75f);
        float domain = Mathf.Lerp(0.2f, 1f, wellness);
        reason = $"onDuty survivalPressure={survivalPressure:0.###} wellness={wellness:0.###}";
        return domain;
    }
}

public sealed class ShoppingJobGiver : CharacterAiJobGiver
{
    public override CharacterAiBranch Branch => CharacterAiBranch.Shopping;
    public override string Name => "ShoppingJobGiver";
    public override bool MatchesAction(AIActionSet actionSet) => actionSet is AIShopping;

    protected override float GetDomainScore(CharacterActor actor, out string reason)
    {
        float visitNeed = FacilityCandidateScorer.GetNeedScore(actor, FacilityRole.Purchase);
        reason = $"visitNeed={visitNeed:0.###}";
        return visitNeed;
    }
}

public sealed class LookAroundJobGiver : CharacterAiJobGiver
{
    public override CharacterAiBranch Branch => CharacterAiBranch.LookAround;
    public override string Name => "LookAroundJobGiver";
    public override bool MatchesAction(AIActionSet actionSet) => actionSet is AILookAround;

    protected override float GetDomainScore(CharacterActor actor, out string reason)
    {
        float hungerNeed = Need(actor, CharacterCondition.HUNGER);
        float sleepNeed = Need(actor, CharacterCondition.SLEEP);
        float excretionNeed = Need(actor, CharacterCondition.EXCRETION);
        float hygieneNeed = Need(actor, CharacterCondition.HYGIENE);
        float funNeed = Need(actor, CharacterCondition.FUN);
        float moodNeed = Need(actor, CharacterCondition.MOOD);
        float urgentNeed = Mathf.Max(hungerNeed, sleepNeed, excretionNeed, hygieneNeed * 0.7f);
        float curiosityWindow = Mathf.Clamp01(1f - urgentNeed);
        float domain = Mathf.Clamp01((0.15f + funNeed * 0.35f + moodNeed * 0.2f) * curiosityWindow);
        reason = $"curiosityWindow={curiosityWindow:0.###} funNeed={funNeed:0.###} moodNeed={moodNeed:0.###}";
        return domain;
    }
}

public sealed class WaitJobGiver : CharacterAiJobGiver
{
    public override CharacterAiBranch Branch => CharacterAiBranch.Wait;
    public override string Name => "WaitJobGiver";
    public override bool MatchesAction(AIActionSet actionSet) => actionSet is AIWait;

    protected override float GetDomainScore(CharacterActor actor, out string reason)
    {
        if (!CharacterWorkRoleUtility.TryGetWork(actor, out AbilityWork work))
        {
            reason = "not a worker";
            return 0f;
        }

        float strongestNeed = Mathf.Max(
            Need(actor, CharacterCondition.HUNGER),
            Need(actor, CharacterCondition.SLEEP),
            Need(actor, CharacterCondition.FUN),
            Need(actor, CharacterCondition.MOOD),
            Need(actor, CharacterCondition.EXCRETION),
            Need(actor, CharacterCondition.HYGIENE));
        float domain = work.IsOffDuty
            ? Mathf.Clamp01(0.35f + strongestNeed * 0.35f)
            : Mathf.Clamp01(0.05f + (1f - strongestNeed) * 0.25f);
        reason = $"waitWindow={domain:0.###} strongestNeed={strongestNeed:0.###}";
        return domain;
    }
}

public interface ICharacterAiJobGiverCatalog
{
    CharacterAiJobGiver ExitDungeon { get; }
    CharacterAiJobGiver GetFood { get; }
    CharacterAiJobGiver Rest { get; }
    CharacterAiJobGiver Toilet { get; }
    CharacterAiJobGiver Hygiene { get; }
    CharacterAiJobGiver Work { get; }
    CharacterAiJobGiver Shopping { get; }
    CharacterAiJobGiver LookAround { get; }
    CharacterAiJobGiver Wait { get; }
    CharacterAiJobGiver Get(CharacterAiBranch branch);
}

public sealed class CharacterAiJobGiverCatalog : ICharacterAiJobGiverCatalog
{
    public CharacterAiJobGiver ExitDungeon { get; } = new ExitDungeonJobGiver();
    public CharacterAiJobGiver GetFood { get; } = new GetFoodJobGiver();
    public CharacterAiJobGiver Rest { get; } = new RestJobGiver();
    public CharacterAiJobGiver Toilet { get; } = new ToiletJobGiver();
    public CharacterAiJobGiver Hygiene { get; } = new HygieneJobGiver();
    public CharacterAiJobGiver Work { get; } = new WorkJobGiver();
    public CharacterAiJobGiver Shopping { get; } = new ShoppingJobGiver();
    public CharacterAiJobGiver LookAround { get; } = new LookAroundJobGiver();
    public CharacterAiJobGiver Wait { get; } = new WaitJobGiver();

    public CharacterAiJobGiver Get(CharacterAiBranch branch)
    {
        return branch switch
        {
            CharacterAiBranch.ExitDungeon => ExitDungeon,
            CharacterAiBranch.Eat => GetFood,
            CharacterAiBranch.Rest => Rest,
            CharacterAiBranch.Toilet => Toilet,
            CharacterAiBranch.Hygiene => Hygiene,
            CharacterAiBranch.Work => Work,
            CharacterAiBranch.Shopping => Shopping,
            CharacterAiBranch.LookAround => LookAround,
            CharacterAiBranch.Wait => Wait,
            _ => null
        };
    }
}
