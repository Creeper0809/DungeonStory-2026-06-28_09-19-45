public interface IIdleBehavior
{
    string DisplayName { get; }
    bool CanRun(CharacterActor actor);
    bool TryRun(CharacterActor actor, float duration, out string failureReason);
}

public sealed class StaffWanderIdleBehavior : IIdleBehavior
{
    public string DisplayName => "던전 배회";

    public bool CanRun(CharacterActor actor)
    {
        return CharacterWorkRoleUtility.TryGetWork(actor, out _)
            && actor.TryGetAbility(out AbilityMove _);
    }

    public bool TryRun(CharacterActor actor, float duration, out string failureReason)
    {
        failureReason = string.Empty;
        if (!CanRun(actor))
        {
            failureReason = "던전 배회 조건 불만족";
            return false;
        }

        AbilityMove move = actor.GetAbility<AbilityMove>();
        if (move != null && move.StartIdleWander(duration))
        {
            return true;
        }

        failureReason = "던전 배회 위치 없음";
        return false;
    }
}

public sealed class StaticWaitIdleBehavior : IIdleBehavior
{
    public string DisplayName => "제자리 대기";

    public bool CanRun(CharacterActor actor)
    {
        return actor != null && actor.TryGetAbility(out AbilityMove _);
    }

    public bool TryRun(CharacterActor actor, float duration, out string failureReason)
    {
        failureReason = string.Empty;
        if (!CanRun(actor))
        {
            failureReason = "이동 능력 없음";
            return false;
        }

        actor.GetAbility<AbilityMove>().StartWait(duration);
        return true;
    }
}

public static class IdleBehaviorRunner
{
    private static readonly IIdleBehavior StaffWander = new StaffWanderIdleBehavior();
    private static readonly IIdleBehavior StaticWait = new StaticWaitIdleBehavior();

    public static bool TryRunDefault(
        CharacterActor actor,
        float duration,
        bool allowMovement,
        out string behaviorName,
        out string failureReason)
    {
        behaviorName = string.Empty;
        failureReason = string.Empty;

        IIdleBehavior behavior = SelectBehavior(actor, allowMovement);
        if (behavior == null)
        {
            failureReason = "No idle behavior configured for current state.";
            return false;
        }

        if (!behavior.CanRun(actor))
        {
            failureReason = $"{behavior.DisplayName} cannot run.";
            return false;
        }

        if (!behavior.TryRun(actor, duration, out failureReason))
        {
            return false;
        }

        behaviorName = behavior.DisplayName;
        return true;
    }

    public static bool TryRunStatic(
        CharacterActor actor,
        float duration,
        out string behaviorName,
        out string failureReason)
    {
        behaviorName = string.Empty;
        failureReason = string.Empty;
        if (!StaticWait.TryRun(actor, duration, out failureReason))
        {
            return false;
        }

        behaviorName = StaticWait.DisplayName;
        return true;
    }

    public static string GetSelectedBehaviorTypeNameForDebug(CharacterActor actor, bool allowMovement)
    {
        IIdleBehavior behavior = SelectBehavior(actor, allowMovement);
        return behavior != null ? behavior.GetType().Name : string.Empty;
    }

    private static IIdleBehavior SelectBehavior(CharacterActor actor, bool allowMovement)
    {
        if (!allowMovement)
        {
            return StaticWait;
        }

        if (CharacterWorkRoleUtility.TryGetWork(actor, out _))
        {
            return StaffWander;
        }

        return StaticWait;
    }
}
