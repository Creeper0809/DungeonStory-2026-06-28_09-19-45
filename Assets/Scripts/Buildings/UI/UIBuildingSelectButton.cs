using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingSelectButton : MonoBehaviour
{
    public int id;
    public void Initialization(BuildingSO so)
    {
        id = so.id;
        transform.GetChild(0).GetComponent<Image>().sprite = so.icon;
    }
    public void OnClick()
    {
        GridSystemManager.Instance.SetGridModeBuild();
        GridSystemManager.Instance.SelectBuildingById(id);
    }
    public void ActiveDestroyMode()
    {
        GridSystemManager.Instance.SetGridMode(GridMode.Destory);
    }
}
