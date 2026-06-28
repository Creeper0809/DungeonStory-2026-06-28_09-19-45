using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridInputManager : MonoBehaviour
{
    public GridSystemManager gridSystem;
    public InputAction buildingPlaceKey;
    public InputAction expanGridKey;

    private void Start()
    {
        buildingPlaceKey.performed += context => BuildablePlacementKey(context);
        expanGridKey.performed += context => ExpandGridKey(context);
    }
    private void OnEnable()
    {
        buildingPlaceKey.Enable();
        expanGridKey.Enable();
    }
    private void OnDisable()
    {
        buildingPlaceKey.Disable();
        expanGridKey.Disable();
    }
    private void BuildablePlacementKey(InputAction.CallbackContext context)
    {
        gridSystem.TriggerPlaceBuilding();
        gridSystem.TriggerDestroyBuildableObject();
    }
    private void ExpandGridKey(InputAction.CallbackContext context)
    {
        gridSystem.GridExpand(2, 2);
    }
}
