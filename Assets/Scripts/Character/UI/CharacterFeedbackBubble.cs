using TMPro;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum CharacterFeedbackState
{
    None,
    Joy,
    Discontent,
    Confused,
    Anger,
    Fatigue
}

[RequireComponent(typeof(Character))]
[DrawWithUnity]
public class CharacterFeedbackBubble : MonoBehaviour
{
    private static readonly Stack<TextMeshPro> TextPool = new Stack<TextMeshPro>();

    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.35f, 0f);
    [SerializeField] private float temporaryDuration = 2.5f;
    [SerializeField] private float logFeedbackCooldown = 0.35f;
    [SerializeField] private TextMeshPro text;

    private Character character;
    private float visibleUntil;
    private float nextLogFeedbackTime;

    public CharacterFeedbackState CurrentState { get; private set; } = CharacterFeedbackState.None;

    private void Awake()
    {
        character = GetComponent<Character>();
        ApplyState(CharacterFeedbackState.None);
    }

    private void OnEnable()
    {
        character ??= GetComponent<Character>();
        if (character == null) return;

        character.OnLogAdded += OnLogAdded;
        character.OnStatChange += OnStatChanged;
    }

    private void OnDisable()
    {
        if (character == null)
        {
            return;
        }

        character.OnLogAdded -= OnLogAdded;
        character.OnStatChange -= OnStatChanged;
        if (text != null)
        {
            text.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (!CharacterAiScheduler.ShouldShowCharacterFeedback(character))
        {
            HideView();
            return;
        }

        if (CurrentState != CharacterFeedbackState.None && Time.time <= visibleUntil)
        {
            EnsureView();
            text.transform.localPosition = GetLocalOffset();
            return;
        }

        ApplyState(EvaluatePersistentState());
    }

    public void Show(CharacterFeedbackState state)
    {
        if (!CharacterAiScheduler.ShouldShowCharacterFeedback(character))
        {
            return;
        }

        ApplyState(state);
        visibleUntil = Time.time + temporaryDuration;
    }

    public CharacterFeedbackState EvaluatePersistentState()
    {
        character ??= GetComponent<Character>();
        if (character == null || character.stats == null)
        {
            return CharacterFeedbackState.None;
        }

        float sleep = GetStat(Character.Condition.SLEEP, 100f);
        float mood = GetStat(Character.Condition.MOOD, 100f);
        if (sleep <= 25f)
        {
            return CharacterFeedbackState.Fatigue;
        }

        if (mood <= 15f)
        {
            return CharacterFeedbackState.Anger;
        }

        if (mood <= 35f)
        {
            return CharacterFeedbackState.Discontent;
        }

        return CharacterFeedbackState.None;
    }

    public static CharacterFeedbackState ClassifyLogTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return CharacterFeedbackState.None;
        }

        if (ContainsAny(tag, "만족", "완료", "회복", "근무 복귀"))
        {
            return CharacterFeedbackState.Joy;
        }

        if (ContainsAny(tag, "위험", "사망", "사고", "분노"))
        {
            return CharacterFeedbackState.Anger;
        }

        if (ContainsAny(tag, "피로", "비번", "휴식"))
        {
            return CharacterFeedbackState.Fatigue;
        }

        if (ContainsAny(tag, "길 막힘", "재고 부족", "시설 파손", "목적지", "혼잡함", "돈 부족"))
        {
            return CharacterFeedbackState.Confused;
        }

        if (ContainsAny(tag, "실패", "보류", "불만"))
        {
            return CharacterFeedbackState.Discontent;
        }

        return CharacterFeedbackState.None;
    }

    public static string GetSymbol(CharacterFeedbackState state)
    {
        return state switch
        {
            CharacterFeedbackState.Joy => ":)",
            CharacterFeedbackState.Discontent => ":/",
            CharacterFeedbackState.Confused => "?",
            CharacterFeedbackState.Anger => "!",
            CharacterFeedbackState.Fatigue => "Zz",
            _ => string.Empty
        };
    }

    private void OnLogAdded(CharacterLogEntry entry)
    {
        if (Time.time < nextLogFeedbackTime
            || !CharacterAiScheduler.ShouldShowCharacterFeedback(character))
        {
            return;
        }

        CharacterFeedbackState state = ClassifyLogTag(entry.Tag);
        if (state != CharacterFeedbackState.None)
        {
            nextLogFeedbackTime = Time.time + logFeedbackCooldown;
            Show(state);
        }
    }

    private void OnStatChanged(System.Collections.Generic.Dictionary<Character.Condition, float> stats)
    {
        if (CurrentState == CharacterFeedbackState.None || Time.time > visibleUntil)
        {
            ApplyState(EvaluatePersistentState());
        }
    }

    private void ApplyState(CharacterFeedbackState state)
    {
        CurrentState = state;
        if (state != CharacterFeedbackState.None)
        {
            EnsureView();
        }

        if (text == null)
        {
            return;
        }

        string symbol = GetSymbol(state);
        text.text = symbol;
        text.gameObject.SetActive(!string.IsNullOrWhiteSpace(symbol));
        text.color = GetColor(state);
    }

    private void HideView()
    {
        CurrentState = CharacterFeedbackState.None;
        ReleaseView();
    }

    private void EnsureView()
    {
        if (text != null)
        {
            return;
        }

        text = TextPool.Count > 0 ? TextPool.Pop() : CreateTextView();
        text.transform.SetParent(transform, false);
        text.transform.localPosition = GetLocalOffset();
        text.gameObject.SetActive(true);

        MeshRenderer renderer = text.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 200;
        }
    }

    private static TextMeshPro CreateTextView()
    {
        GameObject bubbleObject = new GameObject("CharacterFeedbackBubble", typeof(TextMeshPro));
        TextMeshPro view = bubbleObject.GetComponent<TextMeshPro>();
        view.alignment = TextAlignmentOptions.Center;
        view.fontSize = 3.2f;
        view.textWrappingMode = TextWrappingModes.NoWrap;
        TMPKoreanFont.Apply(view);
        return view;
    }

    private void ReleaseView()
    {
        if (text == null)
        {
            return;
        }

        text.text = string.Empty;
        text.gameObject.SetActive(false);
        text.transform.SetParent(null, false);
        TextPool.Push(text);
        text = null;
    }

    private Vector3 GetLocalOffset()
    {
        character ??= GetComponent<Character>();
        if (character == null)
        {
            return localOffset;
        }

        float y = Mathf.Max(localOffset.y, character.GetVisualTopLocalY() + 0.35f);
        return new Vector3(localOffset.x, y, localOffset.z);
    }

    private float GetStat(Character.Condition condition, float defaultValue)
    {
        return character.stats.TryGetValue(condition, out float value) ? value : defaultValue;
    }

    private static Color GetColor(CharacterFeedbackState state)
    {
        return state switch
        {
            CharacterFeedbackState.Joy => new Color(0.45f, 1f, 0.55f),
            CharacterFeedbackState.Discontent => new Color(1f, 0.9f, 0.35f),
            CharacterFeedbackState.Confused => new Color(0.65f, 0.85f, 1f),
            CharacterFeedbackState.Anger => new Color(1f, 0.25f, 0.2f),
            CharacterFeedbackState.Fatigue => new Color(0.75f, 0.7f, 1f),
            _ => Color.white
        };
    }

    private static bool ContainsAny(string value, params string[] patterns)
    {
        foreach (string pattern in patterns)
        {
            if (value.Contains(pattern, System.StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
