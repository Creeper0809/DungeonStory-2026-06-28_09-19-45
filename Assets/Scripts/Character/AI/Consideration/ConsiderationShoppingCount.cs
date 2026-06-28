using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Consideration/ShoppingCount", order = 0)]
public class ConsiderationShoppingCount : Consideration
{
    public override float ScoreConsideration(Character character)
    {
        AbilityShopping shopping = character.GetAbility<AbilityShopping>();
        return shopping.visitCount == 0 ? 0 : shopping.visitCount * 0.4f;
    }
}
