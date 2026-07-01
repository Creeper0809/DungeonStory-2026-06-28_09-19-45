using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterSummeryInfo : UIPopUp, UtilEventListener<InfoFeedEvent>
{
    public GameObject UI;
    public TMP_Text ObjectName;
    public TMP_Text logText;
    public Slider mood;
    public Slider fun;
    public Slider hunger;
    public Slider sleep;
    private Character character;
    void Start()
    {
        TMPKoreanFont.ApplyToChildren(UI != null ? UI.transform : transform);
        UI.gameObject.SetActive(false);
    }
    public void OnTriggerEvent(InfoFeedEvent eventType)
    {
        if (eventType.infoable.GetInfoType() != InfoFeedEvent.Type.CHARACTER) return;
        UIManager.Instance?.CloseAllPopup();
        if(eventType.infoable is Character character)
        {
            UnbindCharacter();
            EnsureRuntimeLogText();
            TMPKoreanFont.ApplyToChildren(UI.transform);
            UI.gameObject.SetActive(true);
            UIManager.Instance?.OpenPopup(this);
            ObjectName.text = character.name;
            OnStatChange(character.stats);
            this.character = character;
            character.OnStatChange += OnStatChange;
            character.OnLogAdded += OnLogAdded;
            RefreshLogText();
        }
    }
    public override void OnClose()
    {
        UI.gameObject.SetActive(false);
        UnbindCharacter();
    }
    public void OnStatChange(Dictionary<Character.Condition,float> stats)
    {
        mood.value = stats[Character.Condition.MOOD] / 100f;
        fun.value = stats[Character.Condition.FUN] / 100f;
        hunger.value = stats[Character.Condition.HUNGER] / 100f;
        sleep.value = stats[Character.Condition.SLEEP] / 100f;
    }
    public void OnLogAdded(CharacterLogEntry entry)
    {
        RefreshLogText();
    }

    public void RefreshLogText()
    {
        if (logText == null)
        {
            return;
        }

        logText.text = FormatLogText(character);
    }

    public static string FormatLogText(Character character, int maxLines = 8)
    {
        if (character == null || character.Log == null || character.Log.Count == 0)
        {
            return "최근 기록 없음";
        }

        int start = Mathf.Max(0, character.Log.Count - Mathf.Max(1, maxLines));
        List<string> rows = new List<string>();
        for (int i = start; i < character.Log.Count; i++)
        {
            rows.Add(character.Log[i]);
        }

        return $"최근 기록\n- {string.Join("\n- ", rows)}";
    }

    public void OnEnable()
    {
        this.EventStartListening<InfoFeedEvent>();
    }
    private void OnDisable()
    {
        UnbindCharacter();
        this.EventStopListening<InfoFeedEvent>();
    }

    private void UnbindCharacter()
    {
        if (character == null)
        {
            return;
        }

        character.OnStatChange -= OnStatChange;
        character.OnLogAdded -= OnLogAdded;
        character = null;
    }

    private void EnsureRuntimeLogText()
    {
        if (logText != null || UI == null)
        {
            return;
        }

        GameObject logObject = new GameObject("CharacterLogText", typeof(RectTransform), typeof(TextMeshProUGUI));
        logObject.transform.SetParent(UI.transform, false);
        RectTransform rect = logObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0.45f);
        rect.offsetMin = new Vector2(16f, 16f);
        rect.offsetMax = new Vector2(-16f, -8f);

        logText = logObject.GetComponent<TMP_Text>();
        TMPKoreanFont.Apply(logText);
        logText.fontSize = 16f;
        logText.color = Color.white;
        logText.alignment = TextAlignmentOptions.TopLeft;
        logText.textWrappingMode = TextWrappingModes.Normal;
        logText.overflowMode = TextOverflowModes.Truncate;
    }

}
