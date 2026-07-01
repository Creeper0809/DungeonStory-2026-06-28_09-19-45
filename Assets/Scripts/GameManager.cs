using DamageNumbersPro;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
public enum TimeOfDay
{
    None,
    Morning,
    Noon,
    Evening,
    Night,
}
public enum NumberCondition
{
    ONBUYINGITEM,
    ONEARNMONEY
}
public class GameManager : UtilSingleton<GameManager>
{
    private static readonly Type[] RequiredRuntimeTypes =
    {
        typeof(OperatingDaySettlementRuntime),
        typeof(CharacterAiScheduler),
        typeof(EventAlertRuntime),
        typeof(OperatingDayReportAlertBridge),
        typeof(RunVariableRuntime),
        typeof(MetaProgressionRuntime),
        typeof(OffenseWorldMapRuntime),
        typeof(OffenseRewardRuntime),
        typeof(OffenseExpeditionRuntime),
        typeof(InvasionThreatRuntime),
        typeof(InvasionDirectorRuntime),
        typeof(InvasionCombatReportRuntime),
        typeof(DailyFacilityShopRuntime),
        typeof(BlueprintResearchRuntime),
        typeof(FacilitySynthesisRuntime),
        typeof(CodexRuntime),
        typeof(RegularCustomerRuntime),
        typeof(StaffDiscontentRuntime)
    };

    public GameData gameData;
    public Dictionary<NumberCondition,DamageNumber> numbers;
    public bool isPause;

    protected override void Awake()
    {
        base.Awake();
        DOTween.Init();
        EnsureOperatingDaySystems();
    }
    void Start()
    {
        StartCoroutine(Timer());
        gameData.gameSpeed.Initialize(1);
        gameData.holdingMoney.Initialize(5000);
        gameData.day.Initialize(1);
        OperatingDayStartedEvent.Trigger(gameData.day.Value);
    }
    public void ConvertSecondsToGameTime()
    {
        float dayFraction = gameData.curTime.Value / 180;

        float gameHours = dayFraction * 24;
        gameData.hour.Value = (int)gameHours % 24;
    }
    public void ChangeGameSpeed()
    {
        gameData.gameSpeed.Value = gameData.gameSpeed.Value % 5 + 1;
        if (!isPause)
        {
            Time.timeScale = gameData.gameSpeed.Value;
        }
    }
    public void TogglePause()
    {
        isPause = !isPause;
        if (isPause)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = gameData.gameSpeed.Value;
        }
    }
    // Update is called once per frame
    void Update()
    {
        ConvertSecondsToGameTime();
        if (gameData.curTime.Value < 40f)
        {
            gameData.timeOfDay.Value = TimeOfDay.Night;
        }
        else if (gameData.curTime.Value < 50f)
        {
            gameData.timeOfDay.Value = TimeOfDay.Morning;
        }
        else if(gameData.curTime.Value < 145f)
        {
            gameData.timeOfDay.Value = TimeOfDay.Noon;
        }
        else if(gameData.curTime.Value < 155f)
        {
            gameData.timeOfDay.Value = TimeOfDay.Evening;
        }
        else
        {
            gameData.timeOfDay.Value = TimeOfDay.Night;
        }
    }
    public IEnumerator Timer()
    {
        while (true)
        {
            gameData.curTime.Value = 0;
            while (gameData.curTime.Value <= 180)
            {
                gameData.curTime.Value += Time.deltaTime;
                yield return null;
            }
            OperatingDayEndedEvent.Trigger(gameData.day.Value);
            gameData.day.Value++;
            OperatingDayStartedEvent.Trigger(gameData.day.Value);
        }
    }

    private void EnsureOperatingDaySystems()
    {
        foreach (Type runtimeType in RequiredRuntimeTypes)
        {
            EnsureRuntimeComponent(runtimeType);
        }
    }

    private Component EnsureRuntimeComponent(Type runtimeType)
    {
        return GetComponent(runtimeType) ?? gameObject.AddComponent(runtimeType);
    }

    public Vector3 GetMouseWorldPos()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,
            -Camera.main.transform.position.z));
    }
}
