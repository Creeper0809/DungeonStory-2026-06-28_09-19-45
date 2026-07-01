using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CharacterFeedbackDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Character/Run P1 Character Feedback Scenarios")]
    public static void RunFromMenu()
    {
        bool success = RunAll(true);
        if (!success)
        {
            Debug.LogError("P1 character feedback scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();

        RunScenario("로그 태그와 반복 누적", VerifyLogTagsAndRepeatCount, errors);
        RunScenario("풍선 상태 분류", VerifyFeedbackBubbleStateClassification, errors);
        RunScenario("클릭 로그 포맷", VerifySummaryLogFormatting, errors);

        if (errors.Count > 0)
        {
            foreach (string error in errors)
            {
                Debug.LogError(error);
            }

            return false;
        }

        if (logSuccess)
        {
            Debug.Log("P1 character feedback scenarios passed.");
        }

        return true;
    }

    private static void RunScenario(string name, System.Func<bool> scenario, List<string> errors)
    {
        if (scenario()) return;

        errors.Add(name);
    }

    private static bool VerifyLogTagsAndRepeatCount()
    {
        Character character = CreateCharacter("Feedback_Log_Test");
        int eventCount = 0;
        CharacterLogEntry lastEntry = default;
        character.OnLogAdded += (entry) =>
        {
            eventCount++;
            lastEntry = entry;
        };

        character.AddLog("쇼핑 실패: 목적지 없음");
        character.AddLog("AI 실패: 쇼핑 - 목적지 없음");
        character.AddLog("보충 실패: 창고 재고 부족");

        bool valid = character.Log.Count == 2
            && character.Log[0] == "길 막힘 x2"
            && character.Log[1] == "재고 부족"
            && eventCount == 3
            && lastEntry.Tag == "재고 부족";

        Object.DestroyImmediate(character.gameObject);
        return valid;
    }

    private static bool VerifyFeedbackBubbleStateClassification()
    {
        Character character = CreateCharacter("Feedback_Bubble_Test");
        CharacterFeedbackBubble bubble = character.GetComponent<CharacterFeedbackBubble>()
            ?? character.gameObject.AddComponent<CharacterFeedbackBubble>();

        bool tagStates = CharacterFeedbackBubble.ClassifyLogTag("만족") == CharacterFeedbackState.Joy
            && CharacterFeedbackBubble.ClassifyLogTag("재고 부족") == CharacterFeedbackState.Confused
            && CharacterFeedbackBubble.ClassifyLogTag("위험") == CharacterFeedbackState.Anger
            && CharacterFeedbackBubble.ClassifyLogTag("피로") == CharacterFeedbackState.Fatigue;

        character.stats[Character.Condition.SLEEP] = 10f;
        character.stats[Character.Condition.MOOD] = 100f;
        bool fatigue = bubble.EvaluatePersistentState() == CharacterFeedbackState.Fatigue;

        character.stats[Character.Condition.SLEEP] = 100f;
        character.stats[Character.Condition.MOOD] = 10f;
        bool anger = bubble.EvaluatePersistentState() == CharacterFeedbackState.Anger;

        character.stats[Character.Condition.MOOD] = 25f;
        bool discontent = bubble.EvaluatePersistentState() == CharacterFeedbackState.Discontent;

        Object.DestroyImmediate(character.gameObject);
        return tagStates && fatigue && anger && discontent;
    }

    private static bool VerifySummaryLogFormatting()
    {
        Character character = CreateCharacter("Feedback_Summary_Test");
        character.AddLog("쇼핑 실패: 목적지 없음");
        character.AddLog("보충 실패: 창고 재고 부족");
        character.AddLog("휴식: 피로 보호");

        string text = CharacterSummeryInfo.FormatLogText(character, 2);
        bool valid = text.Contains("최근 기록")
            && !text.Contains("길 막힘")
            && text.Contains("재고 부족")
            && text.Contains("피로");

        Object.DestroyImmediate(character.gameObject);
        return valid;
    }

    private static Character CreateCharacter(string name)
    {
        GameObject obj = new GameObject(name);
        Character character = obj.AddComponent<Character>();
        character.stats = new Dictionary<Character.Condition, float>
        {
            { Character.Condition.HUNGER, 100f },
            { Character.Condition.SLEEP, 100f },
            { Character.Condition.FUN, 100f },
            { Character.Condition.MOOD, 100f }
        };
        return character;
    }
}
