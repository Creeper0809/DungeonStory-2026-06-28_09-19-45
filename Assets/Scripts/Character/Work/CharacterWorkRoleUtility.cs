public static class CharacterWorkRoleUtility
{
    public static bool TryGetWork(Character character, out AbilityWork work)
    {
        work = null;
        return character != null
            && character.TryGetAbility(out work)
            && work != null;
    }

    public static bool IsWorker(Character character)
    {
        return TryGetWork(character, out _);
    }

    public static bool IsOnDutyWorker(Character character)
    {
        return TryGetWork(character, out AbilityWork work)
            && !work.IsOffDuty;
    }
}
