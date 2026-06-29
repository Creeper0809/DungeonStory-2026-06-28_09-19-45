using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSummaryInfo : UIPopUp,UtilEventListener<InfoFeedEvent>
{
    public GameObject UI;
    public TMP_Text objectName;
    public TMP_Text stock;
    private void Start()
    {
        UI.SetActive(false);
    }
    public void OnTriggerEvent(InfoFeedEvent eventType)
    {
        if (eventType.infoable.GetInfoType() != InfoFeedEvent.Type.BUILDING) return;
        UIManager.Instance.CloseAllPopup();
        if (eventType.infoable is BuildingInfoTarget buildingInfo)
        {
            BuildableObject building = buildingInfo.Building;
            if (building == null) return;

            UI.gameObject.SetActive(true);
            UIManager.Instance.OpenPopup(this);
            BuildingSO data = DataManager.Instance.GetData<BuildingSO>()[building.id];
            objectName.text = data.objectName;
            stock.text = "";
            if(building is Shop shop)
            {
                stock.text = $"남은 재고 : {shop.GetStockCount()}";
            }
        }
    }
    public override void OnClose()
    {
        UI.SetActive(false);
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

public class BuildingInfoTarget : IInfoable
{
    public BuildableObject Building { get; }

    public BuildingInfoTarget(BuildableObject building)
    {
        Building = building;
    }

    public InfoFeedEvent.Type GetInfoType()
    {
        return InfoFeedEvent.Type.BUILDING;
    }
}

public struct InfoFeedEvent
{
    public IInfoable infoable;
    public enum Type
    {
        BUILDING,
        CHARACTER
    }
    public InfoFeedEvent(IInfoable infoable)
    {
        this.infoable = infoable;
    }
    static InfoFeedEvent e;
    public static void Trigger(IInfoable infoable)
    {
        e.infoable = infoable;
        EventObserver.TriggerEvent(e);
    }
}
