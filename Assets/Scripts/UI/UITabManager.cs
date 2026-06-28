using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITabManager : MonoBehaviour
{
    public List<UITab> tabList;
    public Transform buttonPanel;
    public void ToggleSelectButton(int category)
    {
        foreach (UITab selects in tabList)
        {
            selects.ToggleTab(category);
        }
    }
    public void ResgisterTab(UITab tab)
    {
        if (tabList.Contains(tab)) return;
        tabList.Add(tab);
    }
    public void UnRegisterTab(UITab tab)
    {
        if (!tabList.Contains(tab)) return;
        tabList.Remove(tab);
    }
}
