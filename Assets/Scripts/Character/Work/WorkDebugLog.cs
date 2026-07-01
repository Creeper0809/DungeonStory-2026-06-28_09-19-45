public static class WorkDebugLog
{
    public static void LogProgress(Character character)
    {
        character?.AddLog($"[{GetCharacterName(character)}] 작업 진행");
    }

    public static void LogEnd(Character character, string reason = null)
    {
        string message = $"[{GetCharacterName(character)}] 작업 종료";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            message += $" ({reason})";
        }

        character?.AddLog(message);
    }

    private static string GetCharacterName(Character character)
    {
        if (character == null)
        {
            return "알 수 없음";
        }

        if (character.data != null && !string.IsNullOrWhiteSpace(character.data.characterName))
        {
            return character.data.characterName;
        }

        return !string.IsNullOrWhiteSpace(character.name)
            ? character.name
            : "알 수 없음";
    }
}
