using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/Building/StockInfo", order = 0)]
public class StockInfo : DataScriptableObject
{
    public Shop.Type type;
    public int shopId;
    public List<Tuple<SaleItem,int>> stocks;
    public float multifly;
}