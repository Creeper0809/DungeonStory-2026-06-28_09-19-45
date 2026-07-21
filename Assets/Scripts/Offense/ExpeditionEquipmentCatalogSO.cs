using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ExpeditionEquipmentCatalog",
    menuName = "DungeonStory/Offense/Expedition Equipment Catalog",
    order = 0)]
public sealed class ExpeditionEquipmentCatalogSO : ScriptableObject
{
    [SerializeField] private List<ExpeditionEquipmentDefinition> equipment =
        new List<ExpeditionEquipmentDefinition>();

    public IReadOnlyList<ExpeditionEquipmentDefinition> Equipment => equipment;

    public bool TryGet(string id, out ExpeditionEquipmentDefinition definition)
    {
        definition = equipment.FirstOrDefault(item => item != null
            && string.Equals(item.id, id, StringComparison.Ordinal));
        return definition != null;
    }

    public static ExpeditionEquipmentCatalogSO CreateRuntimeDefaults()
    {
        ExpeditionEquipmentCatalogSO catalog = CreateInstance<ExpeditionEquipmentCatalogSO>();
        catalog.equipment = new List<ExpeditionEquipmentDefinition>
        {
            Weapon("weapon:attack-iron", "Iron Edge", attack: 3, cost: 2),
            Weapon("weapon:strength-maul", "Training Maul", strength: 3, cost: 2),
            Weapon("weapon:dexterity-needle", "Quick Needle", dexterity: 3, cost: 2),
            Armor("armor:toughness-plate", "Guard Plate", toughness: 3, health: 8, cost: 2),
            Armor("armor:move-cloak", "Travel Cloak", moveSpeed: 2, dexterity: 1, cost: 2),
            Armor("armor:endurance-mail", "Steady Mail", toughness: 2, health: 12, cost: 2)
        };
        return catalog;
    }

    private static ExpeditionEquipmentDefinition Weapon(
        string id,
        string name,
        int attack = 0,
        int strength = 0,
        int dexterity = 0,
        int cost = 1)
    {
        return new ExpeditionEquipmentDefinition
        {
            id = id,
            displayName = name,
            slot = ExpeditionEquipmentSlot.Weapon,
            stats = new ExpeditionEquipmentStatBlock
            {
                attack = attack,
                strength = strength,
                dexterity = dexterity
            },
            craftCosts = Cost(cost),
            craftSeconds = 6f
        };
    }

    private static ExpeditionEquipmentDefinition Armor(
        string id,
        string name,
        int toughness = 0,
        int moveSpeed = 0,
        int dexterity = 0,
        int health = 0,
        int cost = 1)
    {
        return new ExpeditionEquipmentDefinition
        {
            id = id,
            displayName = name,
            slot = ExpeditionEquipmentSlot.Armor,
            stats = new ExpeditionEquipmentStatBlock
            {
                maxHealth = health,
                toughness = toughness,
                moveSpeed = moveSpeed,
                dexterity = dexterity
            },
            craftCosts = Cost(cost),
            craftSeconds = 6f
        };
    }

    private static List<ExpeditionEquipmentCraftCost> Cost(int amount)
    {
        return new List<ExpeditionEquipmentCraftCost>
        {
            new ExpeditionEquipmentCraftCost
            {
                category = StockCategory.Weapon,
                amount = Mathf.Max(0, amount)
            }
        };
    }
}
