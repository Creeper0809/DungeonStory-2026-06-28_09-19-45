using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(menuName = "DungeonStory/AI/Consideration/Visitable", order = 0)]
public class ConsiderationIsVisitable : Consideration
{
    public Shop.Type type;
    public override float ScoreConsideration(Character character)
    {
        AbilityShopping shopping = null;
        character?.TryGetAbility(out shopping);
        if (shopping == null || shopping.visitCount <= 0)
        {
            return 0f;
        }

        List<BuildableObject> reachableBulding = character.GetReachableBuilding()
                                                        .Where((x) => !shopping.visitedBuilding.Contains(x))
                                                        .ToList();
        if(type == Shop.Type.X) return reachableBulding.Any() ? 1 : 0;

        List<BuildableObject> filter = reachableBulding.Where((building) =>
        {
            if (building is Shop shop)
            {
                if (shop.type == type) return true;
            }
            return false;
        }).ToList();

        return filter.Any() ? 1 : 0;
    }
}
