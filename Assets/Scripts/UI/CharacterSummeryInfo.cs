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
    public Slider mood;
    public Slider fun;
    public Slider hunger;
    public Slider sleep;
    private Character character;
    void Start()
    {
        UI.gameObject.SetActive(false);
    }
    public void OnTriggerEvent(InfoFeedEvent eventType)
    {
         
        if (eventType.infoable.GetInfoType() != InfoFeedEvent.Type.CHARACTER) return;
        UIManager.Instance.CloseAllPopup();
        if(eventType.infoable is Character character)
        {
            UI.gameObject.SetActive(true);
            UIManager.Instance.OpenPopup(this);
            ObjectName.text = character.name;
            OnStatChange(character.stats);
            this.character = character;
            character.OnStatChange += OnStatChange;
        }
    }
    public override void OnClose()
    {
        UI.gameObject.SetActive(false);
        character.OnStatChange -= OnStatChange;
    }
    public void OnStatChange(Dictionary<Character.Condition,float> stats)
    {
        mood.value = stats[Character.Condition.MOOD] / 100f;
        fun.value = stats[Character.Condition.FUN] / 100f;
        hunger.value = stats[Character.Condition.HUNGER] / 100f;
        sleep.value = stats[Character.Condition.SLEEP] / 100f;
    }
    public void OnEnable()
    {
        this.EventStartListening<InfoFeedEvent>();
    }
    private void OnDisable()
    {
        this.EventStopListening<InfoFeedEvent>();
    }
}
