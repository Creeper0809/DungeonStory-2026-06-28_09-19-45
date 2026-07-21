using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class CharacterCarriedItemSaveData
{
    public string sourceStackId = string.Empty;
    public string itemId = string.Empty;
    public int quantity;
}

[Serializable]
public sealed class CharacterCarryInventorySaveData
{
    public List<CharacterCarriedItemSaveData> items = new List<CharacterCarriedItemSaveData>();
}

[DisallowMultipleComponent]
public sealed class CharacterCarryInventory : MonoBehaviour
{
    [SerializeField] private List<CharacterCarriedItemSaveData> carriedItems =
        new List<CharacterCarriedItemSaveData>();

    private CharacterActor actor;

    public IReadOnlyList<CharacterCarriedItemSaveData> Items => carriedItems;
    public bool HasItems => carriedItems.Any(item => item != null && item.quantity > 0);

    private void Awake()
    {
        actor = GetComponent<CharacterActor>();
    }

    public static CharacterCarryInventory Ensure(CharacterActor actor)
    {
        if (actor == null)
        {
            return null;
        }

        CharacterCarryInventory inventory = actor.GetComponent<CharacterCarryInventory>();
        if (inventory == null && Application.isPlaying)
        {
            inventory = actor.gameObject.AddComponent<CharacterCarryInventory>();
        }

        return inventory;
    }

    public float GetBaseCarryLimit()
    {
        actor ??= GetComponent<CharacterActor>();
        CharacterStats stats = actor != null ? actor.Stats : null;
        int strength = stats != null ? stats.GetCharacterStat(CharacterStatType.Strength) : 5;
        int endurance = stats != null ? stats.GetCharacterStat(CharacterStatType.Endurance) : 5;
        return 8f + (strength * 1.5f) + (endurance * 0.75f);
    }

    public float GetMaxAllowedWeight(IItemHaulingSettingsProvider settingsProvider = null)
    {
        float multiplier = settingsProvider != null
            ? settingsProvider.MaxCarryMultiplier
            : Mathf.Clamp(DungeonUserSettingsRuntime.Current.maxCarryMultiplier, 1f, 2.5f);
        return GetBaseCarryLimit() * Mathf.Clamp(multiplier, 1f, 2.5f);
    }

    public float GetCurrentWeight(IDungeonItemCatalogProvider catalogProvider = null)
    {
        float total = 0f;
        foreach (CharacterCarriedItemSaveData item in carriedItems)
        {
            if (item == null || item.quantity <= 0)
            {
                continue;
            }

            DungeonItemDefinition definition = ResolveDefinition(item.itemId, catalogProvider);
            total += definition.UnitWeight * Mathf.Max(0, item.quantity);
        }

        return total;
    }

    public float GetMoveSpeedMultiplier(
        IDungeonItemCatalogProvider catalogProvider = null,
        IItemHaulingSettingsProvider settingsProvider = null)
    {
        float baseLimit = Mathf.Max(0.01f, GetBaseCarryLimit());
        float maxAllowed = Mathf.Max(baseLimit, GetMaxAllowedWeight(settingsProvider));
        float current = GetCurrentWeight(catalogProvider);
        if (current <= baseLimit)
        {
            return 1f;
        }

        float t = Mathf.InverseLerp(baseLimit, maxAllowed, current);
        return Mathf.Lerp(1f, 0.45f, Mathf.Clamp01(t));
    }

    public int GetMaxAcceptableQuantity(
        string itemId,
        int requestedQuantity,
        IDungeonItemCatalogProvider catalogProvider,
        IItemHaulingSettingsProvider settingsProvider)
    {
        int safeQuantity = Mathf.Max(0, requestedQuantity);
        if (safeQuantity == 0)
        {
            return 0;
        }

        DungeonItemDefinition definition = ResolveDefinition(itemId, catalogProvider);
        float unitWeight = Mathf.Max(0.01f, definition.UnitWeight);
        float remainingWeight = Mathf.Max(0f, GetMaxAllowedWeight(settingsProvider) - GetCurrentWeight(catalogProvider));
        return Mathf.Clamp(Mathf.FloorToInt(remainingWeight / unitWeight), 0, safeQuantity);
    }

    public bool TryAdd(
        string sourceStackId,
        string itemId,
        int quantity,
        IDungeonItemCatalogProvider catalogProvider,
        IItemHaulingSettingsProvider settingsProvider,
        out string failureReason)
    {
        failureReason = string.Empty;
        int accepted = GetMaxAcceptableQuantity(itemId, quantity, catalogProvider, settingsProvider);
        if (accepted <= 0)
        {
            failureReason = "carry limit";
            return false;
        }

        CharacterCarriedItemSaveData existing = carriedItems.FirstOrDefault(item => item != null
            && string.Equals(item.itemId, itemId, StringComparison.Ordinal)
            && string.Equals(item.sourceStackId, sourceStackId, StringComparison.Ordinal));
        if (existing == null)
        {
            carriedItems.Add(new CharacterCarriedItemSaveData
            {
                sourceStackId = sourceStackId ?? string.Empty,
                itemId = itemId ?? string.Empty,
                quantity = accepted
            });
        }
        else
        {
            existing.quantity += accepted;
        }

        return accepted == quantity;
    }

    public List<CharacterCarriedItemSaveData> RemoveAllItems()
    {
        List<CharacterCarriedItemSaveData> result = carriedItems
            .Where(item => item != null && item.quantity > 0)
            .Select(item => new CharacterCarriedItemSaveData
            {
                sourceStackId = item.sourceStackId,
                itemId = item.itemId,
                quantity = item.quantity
            })
            .ToList();
        carriedItems.Clear();
        return result;
    }

    public CharacterCarryInventorySaveData Capture()
    {
        return new CharacterCarryInventorySaveData
        {
            items = carriedItems
                .Where(item => item != null && item.quantity > 0)
                .Select(item => new CharacterCarriedItemSaveData
                {
                    sourceStackId = item.sourceStackId,
                    itemId = item.itemId,
                    quantity = Mathf.Max(0, item.quantity)
                })
                .ToList()
        };
    }

    public void Restore(CharacterCarryInventorySaveData snapshot)
    {
        carriedItems.Clear();
        foreach (CharacterCarriedItemSaveData item in snapshot?.items ?? new List<CharacterCarriedItemSaveData>())
        {
            if (item == null || item.quantity <= 0 || string.IsNullOrWhiteSpace(item.itemId))
            {
                continue;
            }

            carriedItems.Add(new CharacterCarriedItemSaveData
            {
                sourceStackId = item.sourceStackId ?? string.Empty,
                itemId = item.itemId.Trim(),
                quantity = Mathf.Max(0, item.quantity)
            });
        }
    }

    private static DungeonItemDefinition ResolveDefinition(
        string itemId,
        IDungeonItemCatalogProvider catalogProvider)
    {
        if (catalogProvider != null)
        {
            return catalogProvider.GetDefinition(itemId);
        }

        return WorldItemStackRuntime.Active != null
            ? WorldItemStackRuntime.Active.CatalogProvider.GetDefinition(itemId)
            : new ResourceDungeonItemCatalogProvider().GetDefinition(itemId);
    }
}
