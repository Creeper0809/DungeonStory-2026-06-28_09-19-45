using UnityEngine;

public sealed class AIHaul : AIActionSet
{
    private static readonly CharacterAiActionDescriptor ActionDescriptor = new CharacterAiActionDescriptor(
        CharacterAiBranch.Work,
        "운반",
        CharacterAiActionTags.Work);

    public override CharacterAiActionDescriptor Descriptor => ActionDescriptor;
    public override bool RequiresDestination => false;
    public override bool IsContinuous => true;
    public override float MinimumDuration => 0.25f;
    public override int InterruptPriority => 42;

    public override float AdjustScore(CharacterActor actor, float baseScore)
    {
        if (!TryGetEnabledPriority(actor, out WorkPriorityLevel priority))
        {
            return 0f;
        }

        AbilityHaul haul = AbilityHaul.Ensure(actor);
        if (haul == null || !haul.CanStartHauling(out _))
        {
            return 0f;
        }

        float priorityWeight = priority switch
        {
            WorkPriorityLevel.Priority1 => 1f,
            WorkPriorityLevel.Priority2 => 0.78f,
            WorkPriorityLevel.Priority3 => 0.48f,
            _ => 0f
        };
        return Mathf.Clamp01(baseScore * priorityWeight);
    }

    public override bool CanStart(CharacterActor actor)
    {
        return TryGetEnabledPriority(actor, out _)
            && AbilityHaul.Ensure(actor)?.CanStartHauling(out _) == true;
    }

    public override bool CanContinue(CharacterActor actor, AIAction runningAction, out string stopReason)
    {
        stopReason = string.Empty;
        AbilityHaul haul = actor != null ? actor.GetComponent<AbilityHaul>() : null;
        return haul != null && haul.IsHauling;
    }

    public override bool CanInterrupt(CharacterActor actor, AIAction runningAction, out string interruptReason)
    {
        interruptReason = string.Empty;
        return false;
    }

    public override void Execute(CharacterActor actor)
    {
        AbilityHaul.Ensure(actor)?.StartHauling();
    }

    public override void OnStop(CharacterActor actor, AIAction runningAction, string reason)
    {
        actor?.GetComponent<AbilityHaul>()?.StopHauling(reason);
    }

    private static bool TryGetEnabledPriority(CharacterActor actor, out WorkPriorityLevel priority)
    {
        priority = WorkPriorityLevel.Off;
        return actor != null
            && actor.TryGetAbility(out AbilityWork work)
            && work.WorkPriorities != null
            && (priority = work.WorkPriorities.GetPriority(FacilityWorkType.Haul)) != WorkPriorityLevel.Off;
    }
}
