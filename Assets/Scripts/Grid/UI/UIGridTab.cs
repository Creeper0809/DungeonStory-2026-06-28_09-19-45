using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGridTab : MonoBehaviour
{
    public int buildingCategory;
    private void Start()
    {
        CloseTab();
    }
    public bool ToggleTab(int buildingCategory)
    {
        if (this.buildingCategory != buildingCategory)
        {
            CloseTab();
            return false;
        }
        if (gameObject.activeSelf)
        {
            CloseTab();
            return false;
        }
        else
        {
            OpenTap();
            return true;
        }
    }
    public void OpenTap()
    {
        gameObject.SetActive(true);
    }
    public void CloseTab()
    {
        gameObject.SetActive(false);
    }
}
