public interface IIdleBehavior
{
    string DisplayName { get; }
    bool CanRun(Character character);
    bool TryRun(Character character, float duration, out string failureReason);
}

public sealed class StaffWanderIdleBehavior : IIdleBehavior
{
    public string DisplayName => "던전 배회";

    public bool CanRun(Character character)
    {
        return CharacterWorkRoleUtility.TryGetWork(character, out _)
            && character.TryGetAbility(out AbilityMove _);
    }

    public bool TryRun(Character character, float duration, out string failureReason)
    {
        failureReason = string.Empty;
        if (!CanRun(character))
        {
            failureReason = "던전 배회 조건 불만족";
            return false;
        }

        AbilityMove move = character.GetAbility<AbilityMove>();
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

    public bool CanRun(Character character)
    {
        return character != null && character.TryGetAbility(out AbilityMove _);
    }

    public bool TryRun(Character character, float duration, out string failureReason)
    {
        failureReason = string.Empty;
        if (!CanRun(character))
        {
            failureReason = "이동 능력 없음";
            return false;
        }

        character.GetAbility<AbilityMove>().StartWait(duration);
        return true;
    }
}

public static class IdleBehaviorRunner
{
    private static readonly IIdleBehavior StaffWander = new StaffWanderIdleBehavior();
    private static readonly IIdleBehavior StaticWait = new StaticWaitIdleBehavior();

    public static bool TryRunDefault(
        Character character,
        float duration,
        bool allowMovement,
        out string behaviorName,
        out string failureReason)
    {
        behaviorName = string.Empty;
        failureReason = string.Empty;

        IIdleBehavior behavior = SelectBehavior(character, allowMovement);
        if (behavior == null)
        {
            failureReason = "No idle behavior configured for current state.";
            return false;
        }

        if (!behavior.CanRun(character))
        {
            failureReason = $"{behavior.DisplayName} cannot run.";
            return false;
        }

        if (!behavior.TryRun(character, duration, out failureReason))
        {
            return false;
        }

        behaviorName = behavior.DisplayName;
        return true;
    }

    public static bool TryRunStatic(
        Character character,
        float duration,
        out string behaviorName,
        out string failureReason)
    {
        behaviorName = string.Empty;
        failureReason = string.Empty;
        if (!StaticWait.TryRun(character, duration, out failureReason))
        {
            return false;
        }

        behaviorName = StaticWait.DisplayName;
        return true;
    }

    public static string GetSelectedBehaviorTypeNameForDebug(Character character, bool allowMovement)
    {
        IIdleBehavior behavior = SelectBehavior(character, allowMovement);
        return behavior != null ? behavior.GetType().Name : string.Empty;
    }

    private static IIdleBehavior SelectBehavior(Character character, bool allowMovement)
    {
        if (!allowMovement)
        {
            return StaticWait;
        }

        if (CharacterWorkRoleUtility.TryGetWork(character, out _))
        {
            return StaffWander;
        }

        return StaticWait;
    }
}
