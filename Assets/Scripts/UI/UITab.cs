using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITab : UIPopUp
{
    public int id;
    public bool ToggleTab(int id)
    {
        if (this.id != id)
        {
            CloseTab();
            return false;
        }
        else
        {
            return Toggle();
        }
    }
    public bool Toggle()
    {
        if(gameObject.activeSelf)
        {
            CloseTab();
            return false;
        }
        else
        {
            UIManager.Instance.OpenPopup(this);
            gameObject.SetActive(true);
            return true;
        }
    }
    public override void OnClose()
    {
        gameObject.SetActive(false);
    }
    public void CloseTab()
    {
        if (!gameObject.activeSelf) return;
        UIManager.Instance.ClosePopupPeek(this);
    }
}
