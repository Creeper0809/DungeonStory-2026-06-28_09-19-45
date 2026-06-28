using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : UtilSingleton<UIManager>
{
    public TMP_Text timeText;
    public TMP_Text holdingMoneyText;
    public TMP_Text gameSpeedText;
    public CanvasGroup touchGaurd;
    public GameData gameData;
    [ReadOnly]
    [ShowInInspector]
    private Stack<UIPopUp> popups = new Stack<UIPopUp>();

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePopupPeek();
        }
    }
    public void CloseAllPopup()
    {
        while (popups.Count > 0)
        {
            ClosePopupPeek();
        }
    }
    public void OpenPopup(UIPopUp popup)
    {
        if (popup == null) return;
        popup.OnOpen();
        popups.Push(popup);
    }
    public void ClosePopupPeek(UIPopUp popup)
    {
        if (popups.Count == 0 || popups.Peek() != popup) return;
        ClosePopupPeek();
    }
    public void ClosePopupPeek()
    {
        if (popups.Count == 0) return;
        UIPopUp popup = popups.Pop();
        popup.OnClose();
    }
    public void UpdateTime()
    {
        timeText.text = $"Day {gameData.day.Value} - {gameData.hour.Value}:00";
    }
    private void UpdateHoldingMoneyText(int holdingMoney)
    {
        holdingMoneyText.text = holdingMoney.ToString();
    }
    private void UpdateGameSpeedText(int gameSpeed)
    {
        gameSpeedText.text = $"X{gameSpeed}";
    }
    public void MakeTouchFalse()
    {
        touchGaurd.interactable = true ;
        touchGaurd.blocksRaycasts = true;
    }
    public void MakeTouchTrue()
    {
        touchGaurd.interactable = false;
        touchGaurd.blocksRaycasts = false;
    }
    private void OnEnable()
    {
        gameData.gameSpeed.OnValueChange += UpdateGameSpeedText;
        gameData.holdingMoney.OnValueChange += UpdateHoldingMoneyText;
        gameData.hour.OnValueChange +=  (_) => UpdateTime();
        gameData.day.OnValueChange += (_) => UpdateTime();
    }
    private void OnDisable()
    {
        gameData.gameSpeed.OnValueChange -= UpdateGameSpeedText;
        gameData.holdingMoney.OnValueChange -= UpdateHoldingMoneyText;
        gameData.hour.OnValueChange -= (_) => UpdateTime();
        gameData.day.OnValueChange -= (_) => UpdateTime();
    }
}
