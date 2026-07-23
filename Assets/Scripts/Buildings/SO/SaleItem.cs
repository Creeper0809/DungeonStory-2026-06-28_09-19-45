using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StockCategory
{
    Food = 0,
    General = 1,
    Weapon = 2,
    Mana = 3,
    Water = 4,
    Medicine = 5,
    Fuel = 6,
    Ammunition = 7
}

[CreateAssetMenu(menuName = "DungeonStory/Building/SaleItem", order = 0)]
public class SaleItem : DataScriptableObject
{
    public string itemName;
    public StockCategory category;
    public int cost;
    public Sprite itemSprite;
    public OnBuyItemSO[] buyevent = new OnBuyItemSO[0];
}
