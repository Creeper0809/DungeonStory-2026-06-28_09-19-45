using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class WorkTypeDefinition
{
    public WorkTypeDefinition(
        string id,
        FacilityWorkType type,
        string displayName,
        int sortOrder,
        WorkPriorityLevel defaultPriority,
        string capabilityId)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Work type id is required.", nameof(id));
        }

        if (type == FacilityWorkType.None || !IsSingleBit(type))
        {
            throw new ArgumentException("A work type must use one non-zero flag bit.", nameof(type));
        }

        Id = id.Trim();
        Type = type;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName.Trim();
        SortOrder = sortOrder;
        DefaultPriority = defaultPriority;
        CapabilityId = capabilityId?.Trim() ?? string.Empty;
    }

    public string Id { get; }
    public FacilityWorkType Type { get; }
    public string DisplayName { get; }
    public int SortOrder { get; }
    public WorkPriorityLevel DefaultPriority { get; }
    public string CapabilityId { get; }

    private static bool IsSingleBit(FacilityWorkType type)
    {
        int value = (int)type;
        return (value & (value - 1)) == 0;
    }
}

public static class WorkTypeCatalog
{
    private static readonly Dictionary<string, WorkTypeDefinition> ById =
        new Dictionary<string, WorkTypeDefinition>(StringComparer.Ordinal);
    private static readonly Dictionary<FacilityWorkType, WorkTypeDefinition> ByType =
        new Dictionary<FacilityWorkType, WorkTypeDefinition>();
    private static bool initialized;

    public static IReadOnlyList<WorkTypeDefinition> All
    {
        get
        {
            EnsureInitialized();
            return ById.Values
                .OrderBy((definition) => definition.SortOrder)
                .ThenBy((definition) => definition.Id, StringComparer.Ordinal)
                .ToArray();
        }
    }

    public static void Register(WorkTypeDefinition definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        EnsureInitialized();
        if (ById.TryGetValue(definition.Id, out WorkTypeDefinition existingById)
            && existingById.Type != definition.Type)
        {
            throw new InvalidOperationException(
                $"Work type id '{definition.Id}' is already assigned to {existingById.Type}.");
        }

        if (ByType.TryGetValue(definition.Type, out WorkTypeDefinition existingByType)
            && !string.Equals(existingByType.Id, definition.Id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Work type flag '{definition.Type}' is already assigned to '{existingByType.Id}'.");
        }

        ById[definition.Id] = definition;
        ByType[definition.Type] = definition;
    }

    public static bool TryGet(string id, out WorkTypeDefinition definition)
    {
        EnsureInitialized();
        return ById.TryGetValue(id?.Trim() ?? string.Empty, out definition);
    }

    public static bool TryGet(FacilityWorkType type, out WorkTypeDefinition definition)
    {
        EnsureInitialized();
        return ByType.TryGetValue(type, out definition);
    }

    public static WorkTypeDefinition GetRequired(FacilityWorkType type)
    {
        if (TryGet(type, out WorkTypeDefinition definition))
        {
            return definition;
        }

        throw new KeyNotFoundException($"No work type definition is registered for '{type}' ({(int)type}).");
    }

    public static IEnumerable<WorkTypeDefinition> Enumerate(FacilityWorkType workTypes)
    {
        return All.Where((definition) => (workTypes & definition.Type) != 0);
    }

    public static void ResetToBuiltIns()
    {
        ById.Clear();
        ByType.Clear();
        initialized = true;
        RegisterBuiltIns();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        ById.Clear();
        ByType.Clear();
        initialized = false;
    }

    private static void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        RegisterBuiltIns();
    }

    private static void RegisterBuiltIns()
    {
        RegisterBuiltIn("work:operate", FacilityWorkType.Operate, "운영", 10, WorkPriorityLevel.Priority1, "building:use");
        RegisterBuiltIn("work:restock", FacilityWorkType.Restock, "보충", 20, WorkPriorityLevel.Priority2, "building:stock");
        RegisterBuiltIn("work:repair", FacilityWorkType.Repair, "수리", 30, WorkPriorityLevel.Priority2, "building:durability");
        RegisterBuiltIn("work:clean", FacilityWorkType.Clean, "청소", 40, WorkPriorityLevel.Priority3, "building:cleaning");
        RegisterBuiltIn("work:research", FacilityWorkType.Research, "연구", 50, WorkPriorityLevel.Priority2, "building:research");
        RegisterBuiltIn("work:guard", FacilityWorkType.Guard, "경비", 60, WorkPriorityLevel.Priority3, "building:security");
        RegisterBuiltIn("work:rescue", FacilityWorkType.Rescue, "구조", 70, WorkPriorityLevel.Priority2, "character:rescue");
        RegisterBuiltIn("work:rest", FacilityWorkType.Rest, "휴식", 80, WorkPriorityLevel.Priority3, "character:rest");
        RegisterBuiltIn("work:craft", FacilityWorkType.Craft, "Craft", 90, WorkPriorityLevel.Priority2, "building:craft");
        RegisterBuiltIn("work:haul", FacilityWorkType.Haul, "운반", 95, WorkPriorityLevel.Priority2, "item:haul");
    }

    private static void RegisterBuiltIn(
        string id,
        FacilityWorkType type,
        string displayName,
        int sortOrder,
        WorkPriorityLevel defaultPriority,
        string capabilityId)
    {
        WorkTypeDefinition definition = new WorkTypeDefinition(
            id,
            type,
            displayName,
            sortOrder,
            defaultPriority,
            capabilityId);
        ById.Add(definition.Id, definition);
        ByType.Add(definition.Type, definition);
    }
}
