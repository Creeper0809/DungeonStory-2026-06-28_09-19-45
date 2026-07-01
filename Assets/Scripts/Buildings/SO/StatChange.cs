using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/Item/Onbuy/Statchange", order = 0)]
public class StatChange : OnBuyItemSO
{
    public int value;
    public override void Onbuy(Character character)
    {
        character.ChangesStat(Character.Condition.HUNGER, value);
    }
}
