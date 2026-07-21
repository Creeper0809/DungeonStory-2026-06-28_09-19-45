using System;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/Items/Item Hauling Settings", order = 1)]
public sealed class ItemHaulingSettingsSO : ScriptableObject
{
    public const string ResourcePath = "SO/Items/ItemHaulingSettings";

    [SerializeField, Range(1f, 2.5f)] private float maxCarryMultiplier = 1.5f;

    public float MaxCarryMultiplier => Mathf.Clamp(maxCarryMultiplier, 1f, 2.5f);
}

[Serializable]
public sealed class ItemHaulingSettingsSnapshot
{
    public float maxCarryMultiplier = 1.5f;

    public void Normalize()
    {
        maxCarryMultiplier = Mathf.Clamp(maxCarryMultiplier, 1f, 2.5f);
    }
}

public interface IItemHaulingSettingsProvider
{
    float MaxCarryMultiplier { get; }
    ItemHaulingSettingsSnapshot Capture();
    void Restore(ItemHaulingSettingsSnapshot snapshot);
}

public sealed class ResourceItemHaulingSettingsProvider : IItemHaulingSettingsProvider
{
    private ItemHaulingSettingsSO settings;
    private float? restoredMultiplier;

    public float MaxCarryMultiplier
    {
        get
        {
            float value = DungeonUserSettingsRuntime.Current.maxCarryMultiplier;
            if (value <= 0f && restoredMultiplier.HasValue)
            {
                value = restoredMultiplier.Value;
            }

            if (value <= 0f)
            {
                value = SettingsAsset != null ? SettingsAsset.MaxCarryMultiplier : 1.5f;
            }

            return Mathf.Clamp(Mathf.Round(value / 0.05f) * 0.05f, 1f, 2.5f);
        }
    }

    private ItemHaulingSettingsSO SettingsAsset
    {
        get
        {
            if (settings == null)
            {
                settings = Resources.Load<ItemHaulingSettingsSO>(ItemHaulingSettingsSO.ResourcePath);
            }

            return settings;
        }
    }

    public ItemHaulingSettingsSnapshot Capture()
    {
        return new ItemHaulingSettingsSnapshot
        {
            maxCarryMultiplier = MaxCarryMultiplier
        };
    }

    public void Restore(ItemHaulingSettingsSnapshot snapshot)
    {
        if (snapshot == null)
        {
            restoredMultiplier = null;
            return;
        }

        snapshot.Normalize();
        restoredMultiplier = snapshot.maxCarryMultiplier;
        DungeonUserSettingsData current = DungeonUserSettingsRuntime.Current.Clone();
        current.maxCarryMultiplier = restoredMultiplier.Value;
        DungeonUserSettingsRuntime.Publish(current);
    }
}
