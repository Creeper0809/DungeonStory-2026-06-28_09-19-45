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
        DungeonStoryGridBuildingController.Instance.SetGridModeBuild();
        DungeonStoryGridBuildingController.Instance.SelectBuildingById(id);
    }
    public void ActiveDestroyMode()
    {
        DungeonStoryGridBuildingController.Instance.SetDestroyMode();
    }
}
