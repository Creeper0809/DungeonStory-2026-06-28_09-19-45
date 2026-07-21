using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

public enum WorldItemStackState
{
    Loose = 0,
    Stored = 1,
    FacilityBuffer = 2,
    Carried = 3,
    ExpeditionPacked = 4
}

[Serializable]
public sealed class DungeonPhysicalItemSaveData
{
    public const int CurrentVersion = 1;

    public int version = CurrentVersion;
    public int nextStackSequence = 1;
    public ItemHaulingSettingsSnapshot haulingSettings = new ItemHaulingSettingsSnapshot();
    public List<WorldItemStackSaveData> stacks = new List<WorldItemStackSaveData>();
}

[Serializable]
public sealed class WorldItemStackSaveData
{
    public string stackId = string.Empty;
    public string itemId = string.Empty;
    public int quantity;
    public WorldItemStackState state = WorldItemStackState.Loose;
    public int gridX;
    public int gridY;
    public string reservedByPersistentId = string.Empty;
    public string destinationId = string.Empty;
    public bool hasDestinationPosition;
    public int destinationGridX;
    public int destinationGridY;
    public bool forbidden;
}

public sealed class WorldItemStackSnapshot
{
    public string StackId { get; set; }
    public string ItemId { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public StockCategory StockCategory { get; set; }
    public int Quantity { get; set; }
    public int UnitPrice { get; set; }
    public float UnitWeight { get; set; }
    public Sprite Sprite { get; set; }
    public WorldItemStackState State { get; set; }
    public Vector2Int Position { get; set; }
    public string ReservedByPersistentId { get; set; }
    public string DestinationId { get; set; }
    public bool HasDestinationPosition { get; set; }
    public Vector2Int DestinationPosition { get; set; }
    public bool Forbidden { get; set; }
    public float TotalWeight => UnitWeight * Quantity;
    public int TotalValue => UnitPrice * Quantity;
    public bool IsReserved => !string.IsNullOrWhiteSpace(ReservedByPersistentId);
}

public sealed class WorldItemPileSnapshot
{
    public Vector2Int Position { get; set; }
    public IReadOnlyList<WorldItemStackSnapshot> Stacks { get; set; } =
        Array.Empty<WorldItemStackSnapshot>();
    public WorldItemStackSnapshot Representative { get; set; }
    public int TotalQuantity => Stacks.Sum(stack => stack.Quantity);
    public int KindCount => Stacks.Select(stack => stack.ItemId).Distinct(StringComparer.Ordinal).Count();
    public float TotalWeight => Stacks.Sum(stack => stack.TotalWeight);
    public bool HasReservedItems => Stacks.Any(stack => stack.IsReserved);
}

public sealed class ItemPileInfoTarget : IInfoable
{
    public ItemPileInfoTarget(Vector2Int position)
    {
        Position = position;
    }

    public Vector2Int Position { get; }
}

public enum WorldItemHaulDestinationKind
{
    Warehouse = 0,
    FacilityBuffer = 1
}

public readonly struct WorldItemHaulJob
{
    public WorldItemHaulJob(
        string stackId,
        Vector2Int itemPosition,
        Vector2Int pickupStandPosition,
        IWarehouseFacility warehouse,
        Vector2Int deliveryPosition,
        WorldItemHaulDestinationKind destinationKind = WorldItemHaulDestinationKind.Warehouse,
        string destinationId = "",
        Vector2Int dropPosition = default,
        bool useDropPosition = false)
    {
        StackId = stackId ?? string.Empty;
        ItemPosition = itemPosition;
        PickupStandPosition = pickupStandPosition;
        Warehouse = warehouse;
        DeliveryPosition = deliveryPosition;
        DestinationKind = destinationKind;
        DestinationId = destinationId ?? string.Empty;
        DropPosition = useDropPosition ? dropPosition : deliveryPosition;
    }

    public string StackId { get; }
    public Vector2Int ItemPosition { get; }
    public Vector2Int PickupStandPosition { get; }
    public IWarehouseFacility Warehouse { get; }
    public Vector2Int DeliveryPosition { get; }
    public Vector2Int DropPosition { get; }
    public WorldItemHaulDestinationKind DestinationKind { get; }
    public string DestinationId { get; }
    public bool IsValid => !string.IsNullOrWhiteSpace(StackId)
        && (DestinationKind == WorldItemHaulDestinationKind.FacilityBuffer || Warehouse != null);
}

public interface IWorldItemStackRuntime
{
    IDungeonItemCatalogProvider CatalogProvider { get; }
    IItemHaulingSettingsProvider HaulingSettingsProvider { get; }
    bool StoredItemMarkersVisible { get; }
    DungeonPhysicalItemSaveData Capture();
    void Restore(DungeonPhysicalItemSaveData snapshot);
    void SetStoredItemMarkersVisible(bool visible);
    bool SpawnStockAtDropoff(StockCategory category, int amount, string sourceLabel, out int spawned);
    bool SpawnStockAtDropoff(
        StockCategory category,
        int amount,
        string sourceLabel,
        WorldItemStackState state,
        string destinationId,
        out int spawned);
    bool SpawnItemAt(
        string itemId,
        int amount,
        Vector2Int position,
        WorldItemStackState state,
        string destinationId,
        out int spawned);
    bool TryRequestFacilityDelivery(
        StockCategory category,
        int amount,
        Vector2Int destinationPosition,
        string destinationId,
        out int requested,
        out string failureReason);
    bool TryGetPileAt(Vector2Int position, out WorldItemPileSnapshot pile);
    bool TryGetPileTargetAt(
        Vector2Int position,
        out ItemPileInfoTarget target,
        out UnityEngine.Object markerObject);
    IReadOnlyList<WorldItemStackSnapshot> GetStacksAt(Vector2Int position, bool includeStored = false);
    IReadOnlyList<WorldItemStackSnapshot> GetAllStacks();
    bool HasAvailableHaulJob(CharacterActor actor);
    bool TryReserveBestHaulJob(CharacterActor actor, out WorldItemHaulJob job, out string failureReason);
    bool TryPickupReservedStack(
        CharacterActor actor,
        CharacterCarryInventory inventory,
        WorldItemHaulJob job,
        out string failureReason);
    bool TryDepositCarriedItems(
        CharacterActor actor,
        CharacterCarryInventory inventory,
        IWarehouseFacility warehouse,
        out string failureReason);
    bool TryDepositCarriedItemsToFacility(
        CharacterActor actor,
        CharacterCarryInventory inventory,
        Vector2Int destinationPosition,
        string destinationId,
        out string failureReason);
    bool TryConsumeFacilityBuffer(
        string destinationId,
        IReadOnlyDictionary<StockCategory, int> costs,
        out string failureReason);
    bool TryStealLooseItem(
        CharacterActor actor,
        int searchRadius,
        out WorldItemStackSnapshot stolenItem,
        out string failureReason);
    void ReleaseReservation(string stackId, string persistentId);
    bool TryClearReservation(string stackId);
    bool SetForbidden(string stackId, bool forbidden);
    bool PrioritizeHaul(string stackId);
    bool DeleteStack(string stackId);
    int RemoveStacksByStateAndDestination(WorldItemStackState state, string destinationId);
}

internal sealed class WorldItemStackRecord
{
    public string stackId = string.Empty;
    public string itemId = string.Empty;
    public int quantity;
    public WorldItemStackState state;
    public Vector2Int position;
    public string reservedByPersistentId = string.Empty;
    public string destinationId = string.Empty;
    public bool hasDestinationPosition;
    public Vector2Int destinationPosition;
    public bool forbidden;
}

public sealed class WorldItemStackRuntime :
    IWorldItemStackRuntime,
    IStartable,
    IDisposable
{
    public const string FacilityInputDestinationPrefix = "facility-input:";
    public const string WarehouseStorageDestinationPrefix = "warehouse:";

    private readonly IGridSystemProvider gridSystemProvider;
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private readonly IDungeonItemCatalogProvider catalogProvider;
    private readonly IItemHaulingSettingsProvider haulingSettingsProvider;
    private readonly ICharacterIdRegistry characterIdRegistry;
    private readonly IWorldDropZoneQuery worldDropZoneQuery;
    private readonly ICharacterSpawnerProvider characterSpawnerProvider;
    private readonly List<WorldItemStackRecord> stacks = new List<WorldItemStackRecord>();
    private readonly Dictionary<string, WorldItemStackRecord> stacksById =
        new Dictionary<string, WorldItemStackRecord>(StringComparer.Ordinal);
    private readonly Dictionary<Vector2Int, WorldItemStackMarker> markersByPosition =
        new Dictionary<Vector2Int, WorldItemStackMarker>();
    private int nextStackSequence = 1;
    private bool storedItemMarkersVisible;

    public WorldItemStackRuntime(
        IGridSystemProvider gridSystemProvider,
        IDungeonSceneComponentQuery sceneQuery,
        IDungeonItemCatalogProvider catalogProvider,
        IItemHaulingSettingsProvider haulingSettingsProvider,
        ICharacterIdRegistry characterIdRegistry,
        IWorldDropZoneQuery worldDropZoneQuery,
        ICharacterSpawnerProvider characterSpawnerProvider)
    {
        this.gridSystemProvider = gridSystemProvider ?? throw new ArgumentNullException(nameof(gridSystemProvider));
        this.sceneQuery = sceneQuery ?? throw new ArgumentNullException(nameof(sceneQuery));
        this.catalogProvider = catalogProvider ?? throw new ArgumentNullException(nameof(catalogProvider));
        this.haulingSettingsProvider = haulingSettingsProvider
            ?? throw new ArgumentNullException(nameof(haulingSettingsProvider));
        this.characterIdRegistry = characterIdRegistry ?? throw new ArgumentNullException(nameof(characterIdRegistry));
        this.worldDropZoneQuery = worldDropZoneQuery
            ?? throw new ArgumentNullException(nameof(worldDropZoneQuery));
        this.characterSpawnerProvider = characterSpawnerProvider
            ?? throw new ArgumentNullException(nameof(characterSpawnerProvider));
    }

    public static WorldItemStackRuntime Active { get; private set; }
    public IDungeonItemCatalogProvider CatalogProvider => catalogProvider;
    public IItemHaulingSettingsProvider HaulingSettingsProvider => haulingSettingsProvider;
    public bool StoredItemMarkersVisible => storedItemMarkersVisible;

    public void Start()
    {
        Active = this;
        RefreshAllMarkers();
    }

    public void Dispose()
    {
        if (Active == this)
        {
            Active = null;
        }

        foreach (WorldItemStackMarker marker in markersByPosition.Values.Where(marker => marker != null))
        {
            UnityEngine.Object.Destroy(marker.gameObject);
        }

        markersByPosition.Clear();
    }

    public static bool TrySpawnStockDelivery(
        StockCategory category,
        int amount,
        string sourceLabel,
        out int spawned,
        out string failureReason)
    {
        spawned = 0;
        failureReason = string.Empty;
        if (Active == null)
        {
            failureReason = "physical item runtime is not active";
            return false;
        }

        bool result = Active.SpawnStockAtDropoff(category, amount, sourceLabel, out spawned);
        if (!result)
        {
            failureReason = "dropoff position unavailable";
        }

        return result;
    }

    public DungeonPhysicalItemSaveData Capture()
    {
        return new DungeonPhysicalItemSaveData
        {
            version = DungeonPhysicalItemSaveData.CurrentVersion,
            nextStackSequence = nextStackSequence,
            haulingSettings = haulingSettingsProvider.Capture(),
            stacks = stacks
                .Where(stack => stack != null && stack.quantity > 0)
                .OrderBy(stack => stack.position.y)
                .ThenBy(stack => stack.position.x)
                .ThenBy(stack => stack.itemId, StringComparer.Ordinal)
                .Select(ToSaveData)
                .ToList()
        };
    }

    public void SetStoredItemMarkersVisible(bool visible)
    {
        if (storedItemMarkersVisible == visible)
        {
            return;
        }

        storedItemMarkersVisible = visible;
        RefreshAllMarkers();
    }

    public void Restore(DungeonPhysicalItemSaveData snapshot)
    {
        ClearRuntimeStacks();
        if (snapshot == null)
        {
            return;
        }

        if (snapshot.version != DungeonPhysicalItemSaveData.CurrentVersion)
        {
            throw new InvalidOperationException(
                $"Unsupported physical item save version {snapshot.version}.");
        }

        haulingSettingsProvider.Restore(snapshot.haulingSettings);
        nextStackSequence = Mathf.Max(1, snapshot.nextStackSequence);
        foreach (WorldItemStackSaveData entry in snapshot.stacks ?? new List<WorldItemStackSaveData>())
        {
            if (entry == null || entry.quantity <= 0 || string.IsNullOrWhiteSpace(entry.itemId))
            {
                continue;
            }

            WorldItemStackRecord record = new WorldItemStackRecord
            {
                stackId = string.IsNullOrWhiteSpace(entry.stackId) ? AllocateStackId() : entry.stackId.Trim(),
                itemId = entry.itemId.Trim(),
                quantity = Mathf.Max(0, entry.quantity),
                state = Enum.IsDefined(typeof(WorldItemStackState), entry.state)
                    ? entry.state
                    : WorldItemStackState.Loose,
                position = new Vector2Int(entry.gridX, entry.gridY),
                reservedByPersistentId = entry.reservedByPersistentId?.Trim() ?? string.Empty,
                destinationId = entry.destinationId?.Trim() ?? string.Empty,
                hasDestinationPosition = entry.hasDestinationPosition,
                destinationPosition = new Vector2Int(entry.destinationGridX, entry.destinationGridY),
                forbidden = entry.forbidden
            };
            AddRecord(record);
        }

        SyncWarehouseInventoriesFromStoredStacks();
        RefreshAllMarkers();
    }

    public bool SpawnStockAtDropoff(StockCategory category, int amount, string sourceLabel, out int spawned)
    {
        return SpawnStockAtDropoff(
            category,
            amount,
            sourceLabel,
            WorldItemStackState.Loose,
            string.Empty,
            out spawned);
    }

    public bool SpawnStockAtDropoff(
        StockCategory category,
        int amount,
        string sourceLabel,
        WorldItemStackState state,
        string destinationId,
        out int spawned)
    {
        spawned = 0;
        if (amount <= 0 || !TryGetDropoffPosition(out Vector2Int dropoff))
        {
            return false;
        }

        DungeonItemDefinition definition = catalogProvider.GetDefinition(category);
        spawned = Spawn(definition.ItemId, amount, dropoff, state, destinationId ?? string.Empty);
        return spawned == amount;
    }

    public bool SpawnItemAt(
        string itemId,
        int amount,
        Vector2Int position,
        WorldItemStackState state,
        string destinationId,
        out int spawned)
    {
        spawned = 0;
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return false;
        }

        spawned = Spawn(itemId.Trim(), amount, position, state, destinationId ?? string.Empty);
        return spawned == amount;
    }

    public bool TryRequestFacilityDelivery(
        StockCategory category,
        int amount,
        Vector2Int destinationPosition,
        string destinationId,
        out int requested,
        out string failureReason)
    {
        requested = 0;
        failureReason = string.Empty;
        int remaining = Mathf.Max(0, amount);
        string normalizedDestination = destinationId?.Trim() ?? string.Empty;
        if (remaining <= 0)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(normalizedDestination))
        {
            failureReason = "destination missing";
            return false;
        }

        IWarehouseFacility[] warehouses = GetWarehouses().ToArray();
        if (warehouses.Sum(candidate => candidate.Inventory.GetStock(category)) < remaining)
        {
            failureReason = "stock unavailable";
            return false;
        }

        DungeonItemDefinition definition = catalogProvider.GetDefinition(category);
        foreach (IWarehouseFacility warehouse in warehouses
            .Where(candidate => candidate.Inventory.GetStock(category) > 0)
            .OrderBy(candidate => candidate is BuildableObject building
                ? Mathf.Abs(building.centerPos.x - destinationPosition.x)
                    + Mathf.Abs(building.centerPos.y - destinationPosition.y)
                : int.MaxValue))
        {
            int withdrawn = warehouse.Inventory.Withdraw(category, remaining);
            if (withdrawn <= 0)
            {
                continue;
            }

            int storedRemoved = RemoveStoredWarehouseItems(warehouse, definition.ItemId, withdrawn);
            Vector2Int sourcePosition = warehouse is BuildableObject building
                ? building.centerPos
                : Vector2Int.zero;
            int spawned = Spawn(
                definition.ItemId,
                withdrawn,
                sourcePosition,
                WorldItemStackState.Loose,
                normalizedDestination,
                hasDestinationPosition: true,
                destinationPosition: destinationPosition);
            requested += spawned;
            remaining -= spawned;
            if (spawned < withdrawn)
            {
                int rollback = withdrawn - spawned;
                warehouse.Inventory.AddStock(category, rollback);
                if (storedRemoved > 0)
                {
                    AddStoredWarehouseItems(warehouse, definition.ItemId, Mathf.Min(rollback, storedRemoved));
                }
            }

            if (remaining <= 0)
            {
                break;
            }
        }

        if (requested <= 0)
        {
            failureReason = "stock unavailable";
            return false;
        }

        if (requested < amount)
        {
            failureReason = "partial stock delivery requested";
            return false;
        }

        return true;
    }

    public bool TryGetPileAt(Vector2Int position, out WorldItemPileSnapshot pile)
    {
        IReadOnlyList<WorldItemStackSnapshot> snapshots = GetStacksAt(
            position,
            includeStored: storedItemMarkersVisible);
        if (snapshots.Count == 0)
        {
            pile = null;
            return false;
        }

        pile = new WorldItemPileSnapshot
        {
            Position = position,
            Stacks = snapshots,
            Representative = SelectRepresentative(snapshots)
        };
        return true;
    }

    public bool TryGetPileTargetAt(
        Vector2Int position,
        out ItemPileInfoTarget target,
        out UnityEngine.Object markerObject)
    {
        target = null;
        markerObject = null;
        if (!TryGetPileAt(position, out _))
        {
            return false;
        }

        target = new ItemPileInfoTarget(position);
        if (markersByPosition.TryGetValue(position, out WorldItemStackMarker marker))
        {
            markerObject = marker;
        }

        return true;
    }

    public IReadOnlyList<WorldItemStackSnapshot> GetStacksAt(Vector2Int position, bool includeStored = false)
    {
        return stacks
            .Where(stack => stack != null
                && stack.quantity > 0
                && stack.position == position
                && IsVisibleState(stack.state, includeStored))
            .Select(ToSnapshot)
            .OrderBy(GetStateSortOrder)
            .ThenBy(stack => stack.IsReserved ? 1 : 0)
            .ThenByDescending(stack => stack.TotalValue)
            .ThenBy(stack => stack.DisplayName, StringComparer.Ordinal)
            .ToArray();
    }

    public IReadOnlyList<WorldItemStackSnapshot> GetAllStacks()
    {
        return stacks
            .Where(stack => stack != null && stack.quantity > 0)
            .Select(ToSnapshot)
            .ToArray();
    }

    public bool HasAvailableHaulJob(CharacterActor actor)
    {
        return TryFindBestHaulJob(actor, reserve: false, out _, out _);
    }

    public bool TryReserveBestHaulJob(
        CharacterActor actor,
        out WorldItemHaulJob job,
        out string failureReason)
    {
        return TryFindBestHaulJob(actor, reserve: true, out job, out failureReason);
    }

    public bool TryPickupReservedStack(
        CharacterActor actor,
        CharacterCarryInventory inventory,
        WorldItemHaulJob job,
        out string failureReason)
    {
        failureReason = string.Empty;
        if (actor == null || inventory == null || !job.IsValid)
        {
            failureReason = "invalid haul job";
            return false;
        }

        string actorId = characterIdRegistry.GetOrAssignPersistentId(actor);
        if (!stacksById.TryGetValue(job.StackId, out WorldItemStackRecord record)
            || record.quantity <= 0)
        {
            failureReason = "stack disappeared";
            return false;
        }

        if (!string.Equals(record.reservedByPersistentId, actorId, StringComparison.Ordinal))
        {
            failureReason = "stack reserved by someone else";
            return false;
        }

        int accepted = inventory.GetMaxAcceptableQuantity(
            record.itemId,
            record.quantity,
            catalogProvider,
            haulingSettingsProvider);
        if (accepted <= 0)
        {
            failureReason = "carry limit";
            return false;
        }

        inventory.TryAdd(
            record.stackId,
            record.itemId,
            accepted,
            catalogProvider,
            haulingSettingsProvider,
            out _);
        record.quantity -= accepted;
        record.reservedByPersistentId = string.Empty;
        if (record.quantity <= 0)
        {
            RemoveRecord(record);
        }

        RefreshMarkerAt(job.ItemPosition);
        return true;
    }

    public bool TryDepositCarriedItems(
        CharacterActor actor,
        CharacterCarryInventory inventory,
        IWarehouseFacility warehouse,
        out string failureReason)
    {
        failureReason = string.Empty;
        if (actor == null || inventory == null || warehouse == null
            || !warehouse.HasWarehouseInventory || warehouse.Inventory == null)
        {
            failureReason = "warehouse unavailable";
            return false;
        }

        List<CharacterCarriedItemSaveData> carried = inventory.RemoveAllItems();
        if (carried.Count == 0)
        {
            failureReason = "nothing carried";
            return false;
        }

        Vector2Int dropPosition = ResolveActorGridPosition(actor);
        bool depositedAny = false;
        foreach (CharacterCarriedItemSaveData item in carried)
        {
            if (item == null || item.quantity <= 0)
            {
                continue;
            }

            int remaining = item.quantity;
            if (DungeonItemCatalogSO.TryGetStockCategoryFromItemId(item.itemId, out StockCategory category))
            {
                int deposited = warehouse.Inventory.Deposit(category, remaining);
                if (deposited > 0)
                {
                    AddStoredWarehouseItems(warehouse, item.itemId, deposited);
                }

                remaining -= deposited;
                depositedAny |= deposited > 0;
            }
            else if (DungeonItemCatalogSO.TryGetEquipmentIdFromItemId(item.itemId, out string equipmentId)
                && ExpeditionEquipmentRuntime.Active != null)
            {
                ExpeditionEquipmentRuntime.Active.AddInventory(equipmentId, remaining);
                depositedAny |= remaining > 0;
                remaining = 0;
            }

            if (remaining > 0)
            {
                Spawn(item.itemId, remaining, dropPosition, WorldItemStackState.Loose, string.Empty);
            }
        }

        if (!depositedAny)
        {
            failureReason = "warehouse rejected carried items";
        }

        return depositedAny;
    }

    public bool TryDepositCarriedItemsToFacility(
        CharacterActor actor,
        CharacterCarryInventory inventory,
        Vector2Int destinationPosition,
        string destinationId,
        out string failureReason)
    {
        failureReason = string.Empty;
        if (actor == null || inventory == null)
        {
            failureReason = "carrier unavailable";
            return false;
        }

        string normalizedDestination = destinationId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedDestination))
        {
            failureReason = "destination missing";
            return false;
        }

        List<CharacterCarriedItemSaveData> carried = inventory.RemoveAllItems();
        if (carried.Count == 0)
        {
            failureReason = "nothing carried";
            return false;
        }

        bool depositedAny = false;
        foreach (CharacterCarriedItemSaveData item in carried)
        {
            if (item == null || item.quantity <= 0 || string.IsNullOrWhiteSpace(item.itemId))
            {
                continue;
            }

            int spawned = Spawn(
                item.itemId,
                item.quantity,
                destinationPosition,
                WorldItemStackState.FacilityBuffer,
                normalizedDestination);
            depositedAny |= spawned > 0;
        }

        if (!depositedAny)
        {
            failureReason = "facility rejected carried items";
        }

        return depositedAny;
    }

    public bool TryConsumeFacilityBuffer(
        string destinationId,
        IReadOnlyDictionary<StockCategory, int> costs,
        out string failureReason)
    {
        failureReason = string.Empty;
        string normalizedDestination = destinationId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedDestination))
        {
            failureReason = "destination missing";
            return false;
        }

        Dictionary<StockCategory, int> required = costs?
            .Where(pair => pair.Value > 0)
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Sum(pair => Mathf.Max(0, pair.Value)))
            ?? new Dictionary<StockCategory, int>();
        if (required.Count == 0)
        {
            return true;
        }

        Dictionary<StockCategory, int> available = new Dictionary<StockCategory, int>();
        foreach (WorldItemStackRecord stack in stacks)
        {
            if (stack == null
                || stack.quantity <= 0
                || stack.state != WorldItemStackState.FacilityBuffer
                || !string.Equals(stack.destinationId ?? string.Empty, normalizedDestination, StringComparison.Ordinal)
                || !DungeonItemCatalogSO.TryGetStockCategoryFromItemId(stack.itemId, out StockCategory category))
            {
                continue;
            }

            available.TryGetValue(category, out int current);
            available[category] = current + stack.quantity;
        }

        foreach (KeyValuePair<StockCategory, int> pair in required)
        {
            if (!available.TryGetValue(pair.Key, out int stock) || stock < pair.Value)
            {
                failureReason = "facility materials missing";
                return false;
            }
        }

        foreach (KeyValuePair<StockCategory, int> pair in required)
        {
            int remaining = pair.Value;
            foreach (WorldItemStackRecord stack in stacks.ToArray())
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (stack == null
                    || stack.quantity <= 0
                    || stack.state != WorldItemStackState.FacilityBuffer
                    || !string.Equals(stack.destinationId ?? string.Empty, normalizedDestination, StringComparison.Ordinal)
                    || !DungeonItemCatalogSO.TryGetStockCategoryFromItemId(stack.itemId, out StockCategory category)
                    || category != pair.Key)
                {
                    continue;
                }

                int consumed = Mathf.Min(remaining, stack.quantity);
                Vector2Int position = stack.position;
                stack.quantity -= consumed;
                remaining -= consumed;
                if (stack.quantity <= 0)
                {
                    RemoveRecord(stack);
                }

                RefreshMarkerAt(position);
            }
        }

        return true;
    }

    public bool TryStealLooseItem(
        CharacterActor actor,
        int searchRadius,
        out WorldItemStackSnapshot stolenItem,
        out string failureReason)
    {
        stolenItem = null;
        failureReason = string.Empty;
        if (actor == null || actor.characterType != CharacterType.Customer)
        {
            failureReason = "not a customer";
            return false;
        }

        CharacterCarryInventory inventory = CharacterCarryInventory.Ensure(actor);
        if (inventory == null)
        {
            failureReason = "no carry inventory";
            return false;
        }

        Vector2Int origin = ResolveActorGridPosition(actor);
        int radius = Mathf.Max(0, searchRadius);
        HashSet<Vector2Int> reachable = actor.Brain != null
            ? actor.Brain.GetPathSearch(actor)?.GetReachablePositions().ToHashSet()
            : null;

        WorldItemStackRecord bestStack = null;
        DungeonItemDefinition bestDefinition = null;
        float bestScore = float.MinValue;
        foreach (WorldItemStackRecord stack in stacks)
        {
            if (stack == null
                || stack.quantity <= 0
                || stack.state != WorldItemStackState.Loose
                || stack.forbidden
                || !string.IsNullOrWhiteSpace(stack.reservedByPersistentId))
            {
                continue;
            }

            int distance = Mathf.Abs(stack.position.x - origin.x) + Mathf.Abs(stack.position.y - origin.y);
            if (distance > radius
                || (reachable != null && !reachable.Contains(stack.position)))
            {
                continue;
            }

            DungeonItemDefinition definition = catalogProvider.GetDefinition(stack.itemId);
            if (inventory.GetMaxAcceptableQuantity(
                    stack.itemId,
                    1,
                    catalogProvider,
                    haulingSettingsProvider) <= 0)
            {
                continue;
            }

            float score = definition.UnitPrice * 10f
                + Mathf.Min(50, stack.quantity)
                - distance * 5f;
            if (score <= bestScore)
            {
                continue;
            }

            bestStack = stack;
            bestDefinition = definition;
            bestScore = score;
        }

        if (bestStack == null)
        {
            failureReason = "no loose item nearby";
            return false;
        }

        if (!inventory.TryAdd(
                $"floor-theft:{bestStack.stackId}:{Time.frameCount}",
                bestStack.itemId,
                1,
                catalogProvider,
                haulingSettingsProvider,
                out failureReason))
        {
            return false;
        }

        stolenItem = ToSnapshot(bestStack);
        stolenItem.Quantity = 1;
        if (bestDefinition != null)
        {
            stolenItem.DisplayName = bestDefinition.DisplayName;
            stolenItem.Description = bestDefinition.Description;
            stolenItem.StockCategory = bestDefinition.StockCategory;
            stolenItem.UnitPrice = bestDefinition.UnitPrice;
            stolenItem.UnitWeight = bestDefinition.UnitWeight;
            stolenItem.Sprite = bestDefinition.Sprite;
        }

        Vector2Int position = bestStack.position;
        bestStack.quantity--;
        if (bestStack.quantity <= 0)
        {
            RemoveRecord(bestStack);
        }

        RefreshMarkerAt(position);
        return true;
    }

    public void ReleaseReservation(string stackId, string persistentId)
    {
        if (string.IsNullOrWhiteSpace(stackId)
            || !stacksById.TryGetValue(stackId, out WorldItemStackRecord record))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(persistentId)
            || string.Equals(record.reservedByPersistentId, persistentId, StringComparison.Ordinal))
        {
            record.reservedByPersistentId = string.Empty;
            RefreshMarkerAt(record.position);
        }
    }

    public bool TryClearReservation(string stackId)
    {
        if (string.IsNullOrWhiteSpace(stackId)
            || !stacksById.TryGetValue(stackId, out WorldItemStackRecord record))
        {
            return false;
        }

        record.reservedByPersistentId = string.Empty;
        RefreshMarkerAt(record.position);
        return true;
    }

    public bool SetForbidden(string stackId, bool forbidden)
    {
        if (string.IsNullOrWhiteSpace(stackId)
            || !stacksById.TryGetValue(stackId, out WorldItemStackRecord record))
        {
            return false;
        }

        record.forbidden = forbidden;
        RefreshMarkerAt(record.position);
        return true;
    }

    public bool PrioritizeHaul(string stackId)
    {
        if (string.IsNullOrWhiteSpace(stackId)
            || !stacksById.TryGetValue(stackId, out WorldItemStackRecord record))
        {
            return false;
        }

        record.forbidden = false;
        record.reservedByPersistentId = string.Empty;
        RefreshMarkerAt(record.position);
        return true;
    }

    public bool DeleteStack(string stackId)
    {
        if (string.IsNullOrWhiteSpace(stackId)
            || !stacksById.TryGetValue(stackId, out WorldItemStackRecord record))
        {
            return false;
        }

        Vector2Int position = record.position;
        RemoveRecord(record);
        RefreshMarkerAt(position);
        return true;
    }

    public int RemoveStacksByStateAndDestination(WorldItemStackState state, string destinationId)
    {
        string normalizedDestination = destinationId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedDestination))
        {
            return 0;
        }

        WorldItemStackRecord[] targets = stacks
            .Where(stack => stack != null
                && stack.state == state
                && string.Equals(
                    stack.destinationId ?? string.Empty,
                    normalizedDestination,
                    StringComparison.Ordinal))
            .ToArray();
        int removed = 0;
        foreach (WorldItemStackRecord target in targets)
        {
            Vector2Int position = target.position;
            removed += Mathf.Max(0, target.quantity);
            RemoveRecord(target);
            RefreshMarkerAt(position);
        }

        return removed;
    }

    private int Spawn(
        string itemId,
        int amount,
        Vector2Int position,
        WorldItemStackState state,
        string destinationId,
        bool hasDestinationPosition = false,
        Vector2Int destinationPosition = default)
    {
        int remaining = Mathf.Max(0, amount);
        int spawned = 0;
        DungeonItemDefinition definition = catalogProvider.GetDefinition(itemId);
        int maxStack = definition.MaxStack;
        while (remaining > 0)
        {
            int amountForStack = Mathf.Min(remaining, maxStack);
            WorldItemStackRecord mergeTarget = FindMergeTarget(
                itemId,
                position,
                state,
                destinationId,
                string.Empty,
                hasDestinationPosition,
                destinationPosition,
                maxStack);
            if (mergeTarget != null)
            {
                int merged = Mathf.Min(amountForStack, maxStack - mergeTarget.quantity);
                mergeTarget.quantity += merged;
                amountForStack -= merged;
                remaining -= merged;
                spawned += merged;
            }

            if (amountForStack <= 0)
            {
                continue;
            }

            WorldItemStackRecord record = new WorldItemStackRecord
            {
                stackId = AllocateStackId(),
                itemId = itemId,
                quantity = amountForStack,
                state = state,
                position = position,
                destinationId = destinationId ?? string.Empty,
                hasDestinationPosition = hasDestinationPosition,
                destinationPosition = destinationPosition
            };
            AddRecord(record);
            remaining -= amountForStack;
            spawned += amountForStack;
        }

        RefreshMarkerAt(position);
        return spawned;
    }

    private bool TryFindBestHaulJob(
        CharacterActor actor,
        bool reserve,
        out WorldItemHaulJob job,
        out string failureReason)
    {
        job = default;
        failureReason = string.Empty;
        if (actor == null || !TryGetGrid(out Grid grid))
        {
            failureReason = "no grid";
            return false;
        }

        CharacterCarryInventory inventory = CharacterCarryInventory.Ensure(actor);
        if (inventory == null)
        {
            failureReason = "no carry inventory";
            return false;
        }

        string actorId = characterIdRegistry.GetOrAssignPersistentId(actor);
        HashSet<Vector2Int> reachable = actor.Brain != null
            ? actor.Brain.GetPathSearch(actor)?.GetReachablePositions().ToHashSet()
            : grid.SearchPath(actor.GetNowXY()).GetReachablePositions().ToHashSet();
        if (reachable == null || reachable.Count == 0)
        {
            failureReason = "no reachable cells";
            return false;
        }

        WorldItemStackRecord bestStack = null;
        IWarehouseFacility bestWarehouse = null;
        Vector2Int bestPickupStand = default;
        Vector2Int bestDelivery = default;
        Vector2Int bestDrop = default;
        WorldItemHaulDestinationKind bestDestinationKind = WorldItemHaulDestinationKind.Warehouse;
        string bestDestinationId = string.Empty;
        float bestScore = float.NegativeInfinity;

        foreach (WorldItemStackRecord stack in stacks)
        {
            if (!CanHaulStack(stack)
                || (!string.IsNullOrWhiteSpace(stack.reservedByPersistentId)
                    && !string.Equals(stack.reservedByPersistentId, actorId, StringComparison.Ordinal)))
            {
                continue;
            }

            int acceptable = inventory.GetMaxAcceptableQuantity(
                stack.itemId,
                stack.quantity,
                catalogProvider,
                haulingSettingsProvider);
            if (acceptable <= 0)
            {
                continue;
            }

            if (!TryResolvePickupStandCell(grid, stack.position, out Vector2Int pickupStand)
                || !reachable.Contains(pickupStand))
            {
                continue;
            }

            IWarehouseFacility warehouse = null;
            Vector2Int deliveryCell;
            Vector2Int dropCell;
            WorldItemHaulDestinationKind destinationKind;
            string destinationId = stack.destinationId ?? string.Empty;
            if (stack.hasDestinationPosition && !string.IsNullOrWhiteSpace(destinationId))
            {
                if (!TryResolveFacilityDeliveryCell(grid, stack.destinationPosition, out deliveryCell)
                    || !reachable.Contains(deliveryCell))
                {
                    continue;
                }

                dropCell = stack.destinationPosition;
                destinationKind = WorldItemHaulDestinationKind.FacilityBuffer;
            }
            else if (TryFindWarehouseForStack(
                         grid,
                         reachable,
                         stack,
                         out warehouse,
                         out deliveryCell))
            {
                dropCell = deliveryCell;
                destinationKind = WorldItemHaulDestinationKind.Warehouse;
                destinationId = string.Empty;
            }
            else
            {
                continue;
            }

            DungeonItemDefinition definition = catalogProvider.GetDefinition(stack.itemId);
            int distance = Mathf.Abs(actor.GetNowXY().x - pickupStand.x)
                + Mathf.Abs(actor.GetNowXY().y - pickupStand.y)
                + Mathf.Abs(pickupStand.x - deliveryCell.x)
                + Mathf.Abs(pickupStand.y - deliveryCell.y);
            float score = (definition.UnitPrice * acceptable * 0.02f)
                + Mathf.Min(acceptable, definition.MaxStack) * 0.01f
                - distance * 0.01f;
            if (score <= bestScore)
            {
                continue;
            }

            bestStack = stack;
            bestWarehouse = warehouse;
            bestPickupStand = pickupStand;
            bestDelivery = deliveryCell;
            bestDrop = dropCell;
            bestDestinationKind = destinationKind;
            bestDestinationId = destinationId;
            bestScore = score;
        }

        if (bestStack == null
            || (bestDestinationKind == WorldItemHaulDestinationKind.Warehouse && bestWarehouse == null))
        {
            failureReason = "no haulable stack";
            return false;
        }

        if (reserve)
        {
            bestStack.reservedByPersistentId = actorId;
            RefreshMarkerAt(bestStack.position);
        }

        job = new WorldItemHaulJob(
            bestStack.stackId,
            bestStack.position,
            bestPickupStand,
            bestWarehouse,
            bestDelivery,
            bestDestinationKind,
            bestDestinationId,
            bestDrop,
            useDropPosition: true);
        return true;
    }

    private bool TryFindWarehouseForStack(
        Grid grid,
        HashSet<Vector2Int> reachable,
        WorldItemStackRecord stack,
        out IWarehouseFacility warehouse,
        out Vector2Int deliveryCell)
    {
        warehouse = null;
        deliveryCell = default;
        bool isStockItem = DungeonItemCatalogSO.TryGetStockCategoryFromItemId(
            stack.itemId,
            out StockCategory category);
        bool isEquipmentItem = DungeonItemCatalogSO.TryGetEquipmentIdFromItemId(stack.itemId, out _);
        if (!isStockItem && !isEquipmentItem)
        {
            return false;
        }

        int bestDistance = int.MaxValue;
        foreach (IWarehouseFacility candidate in GetWarehouses()
            .Where(candidate => candidate.HasWarehouseInventory
                && candidate.Inventory != null
                && (isEquipmentItem || candidate.Inventory.CanStore(category, 1))))
        {
            if (candidate is not BuildableObject building || building.isDestroy)
            {
                continue;
            }

            if (!TryResolveDeliveryCell(grid, building, out Vector2Int candidateDelivery)
                || !reachable.Contains(candidateDelivery))
            {
                continue;
            }

            int distance = Mathf.Abs(stack.position.x - candidateDelivery.x)
                + Mathf.Abs(stack.position.y - candidateDelivery.y);
            if (distance >= bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            warehouse = candidate;
            deliveryCell = candidateDelivery;
        }

        return warehouse != null;
    }

    private IEnumerable<IWarehouseFacility> GetWarehouses()
    {
        return sceneQuery.All<MonoBehaviour>(includeInactive: false)
            .OfType<IWarehouseFacility>()
            .Where(facility => facility != null
                && facility.HasWarehouseInventory
                && facility.Inventory != null);
    }

    private void SyncWarehouseInventoriesFromStoredStacks()
    {
        Dictionary<string, Dictionary<StockCategory, int>> stockByWarehouse =
            new Dictionary<string, Dictionary<StockCategory, int>>(StringComparer.Ordinal);
        foreach (WorldItemStackRecord stack in stacks)
        {
            if (stack == null
                || stack.quantity <= 0
                || stack.state != WorldItemStackState.Stored
                || string.IsNullOrWhiteSpace(stack.destinationId)
                || !DungeonItemCatalogSO.TryGetStockCategoryFromItemId(stack.itemId, out StockCategory category))
            {
                continue;
            }

            string destinationId = stack.destinationId.Trim();
            if (!destinationId.StartsWith(WarehouseStorageDestinationPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (!stockByWarehouse.TryGetValue(destinationId, out Dictionary<StockCategory, int> stockByCategory))
            {
                stockByCategory = new Dictionary<StockCategory, int>();
                stockByWarehouse[destinationId] = stockByCategory;
            }

            long combined = (long)(stockByCategory.TryGetValue(category, out int current) ? current : 0)
                + stack.quantity;
            stockByCategory[category] = combined >= int.MaxValue ? int.MaxValue : (int)combined;
        }

        if (stockByWarehouse.Count == 0)
        {
            return;
        }

        foreach (IWarehouseFacility warehouse in GetWarehouses())
        {
            WarehouseInventorySnapshot snapshot = warehouse.Inventory.CreateSnapshot();
            string destinationId = GetWarehouseStorageDestinationId(warehouse);
            stockByWarehouse.TryGetValue(destinationId, out Dictionary<StockCategory, int> stockByCategory);
            snapshot.stocks = (stockByCategory ?? new Dictionary<StockCategory, int>())
                .Where(pair => pair.Value > 0 && warehouse.Inventory.Accepts(pair.Key))
                .OrderBy(pair => StockCategoryCatalog.TryGet(pair.Key, out StockCategoryDefinition definition)
                    ? definition.SortOrder
                    : int.MaxValue)
                .ThenBy(pair => Convert.ToInt32(pair.Key, CultureInfo.InvariantCulture))
                .Select(pair => StockAmountSnapshot.From(pair.Key, pair.Value))
                .ToList();
            warehouse.Inventory.ApplySnapshot(snapshot);
        }
    }

    private int AddStoredWarehouseItems(IWarehouseFacility warehouse, string itemId, int amount)
    {
        if (warehouse == null || string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return 0;
        }

        return Spawn(
            itemId,
            amount,
            ResolveWarehouseStoragePosition(warehouse),
            WorldItemStackState.Stored,
            GetWarehouseStorageDestinationId(warehouse));
    }

    private int RemoveStoredWarehouseItems(IWarehouseFacility warehouse, string itemId, int amount)
    {
        if (warehouse == null || string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return 0;
        }

        string destinationId = GetWarehouseStorageDestinationId(warehouse);
        int remaining = amount;
        int removed = 0;
        foreach (WorldItemStackRecord stack in stacks.ToArray())
        {
            if (remaining <= 0)
            {
                break;
            }

            if (stack == null
                || stack.quantity <= 0
                || stack.state != WorldItemStackState.Stored
                || !string.Equals(stack.itemId, itemId, StringComparison.Ordinal)
                || !string.Equals(stack.destinationId ?? string.Empty, destinationId, StringComparison.Ordinal))
            {
                continue;
            }

            int consumed = Mathf.Min(remaining, stack.quantity);
            Vector2Int position = stack.position;
            stack.quantity -= consumed;
            remaining -= consumed;
            removed += consumed;
            if (stack.quantity <= 0)
            {
                RemoveRecord(stack);
            }

            RefreshMarkerAt(position);
        }

        return removed;
    }

    private static string GetWarehouseStorageDestinationId(IWarehouseFacility warehouse)
    {
        if (warehouse is BuildableObject building)
        {
            return WarehouseStorageDestinationPrefix + building.GridId.ToString(CultureInfo.InvariantCulture);
        }

        return WarehouseStorageDestinationPrefix + warehouse.GetHashCode().ToString(CultureInfo.InvariantCulture);
    }

    private static Vector2Int ResolveWarehouseStoragePosition(IWarehouseFacility warehouse)
    {
        return warehouse is BuildableObject building ? building.centerPos : Vector2Int.zero;
    }

    private static bool CanHaulStack(WorldItemStackRecord stack)
    {
        return stack != null
            && stack.quantity > 0
            && !stack.forbidden
            && !IsFacilityInputBuffer(stack)
            && (stack.state == WorldItemStackState.Loose
                || stack.state == WorldItemStackState.FacilityBuffer);
    }

    private static bool IsFacilityInputBuffer(WorldItemStackRecord stack)
    {
        return stack != null
            && stack.state == WorldItemStackState.FacilityBuffer
            && IsFacilityInputDestination(stack.destinationId);
    }

    private static bool IsFacilityInputDestination(string destinationId)
    {
        return !string.IsNullOrWhiteSpace(destinationId)
            && destinationId.StartsWith(FacilityInputDestinationPrefix, StringComparison.Ordinal);
    }

    private static bool TryResolvePickupStandCell(
        Grid grid,
        Vector2Int itemPosition,
        out Vector2Int standCell)
    {
        if (grid.IsValidGridPos(itemPosition) && grid.IsWalkable(itemPosition))
        {
            standCell = itemPosition;
            return true;
        }

        return grid.TryFindNearbyWalkablePositionOnSameFloor(itemPosition, out standCell, maxDistance: 1);
    }

    private static bool TryResolveDeliveryCell(
        Grid grid,
        BuildableObject warehouse,
        out Vector2Int deliveryCell)
    {
        deliveryCell = default;
        if (grid == null || warehouse == null)
        {
            return false;
        }

        foreach (Vector2Int position in warehouse.buildPoses ?? Array.Empty<Vector2Int>())
        {
            if (grid.IsValidGridPos(position) && grid.IsWalkable(position))
            {
                deliveryCell = position;
                return true;
            }
        }

        return grid.TryFindNearbyWalkablePositionOnSameFloor(
            warehouse.centerPos,
            out deliveryCell,
            maxDistance: 2);
    }

    private static bool TryResolveFacilityDeliveryCell(
        Grid grid,
        Vector2Int destinationPosition,
        out Vector2Int deliveryCell)
    {
        deliveryCell = default;
        if (grid == null)
        {
            return false;
        }

        if (grid.IsValidGridPos(destinationPosition) && grid.IsWalkable(destinationPosition))
        {
            deliveryCell = destinationPosition;
            return true;
        }

        return grid.TryFindNearbyWalkablePositionOnSameFloor(
            destinationPosition,
            out deliveryCell,
            maxDistance: 2);
    }

    private bool TryGetDropoffPosition(out Vector2Int dropoff)
    {
        if (worldDropZoneQuery.TryGetDeliveryDropoff(out dropoff))
        {
            return true;
        }

        if (characterSpawnerProvider.TryGetSpawner(out CharacterSpawner spawner)
            && spawner.TryGetEntryGridPosition(out dropoff))
        {
            return true;
        }

        if (TryGetGrid(out Grid grid))
        {
            GridCell cell = grid.GetCells()
                .Where(candidate => candidate != null && grid.IsWalkable(candidate.Position))
                .OrderBy(candidate => candidate.Position.y)
                .ThenBy(candidate => candidate.Position.x)
                .FirstOrDefault();
            if (cell != null)
            {
                dropoff = cell.Position;
                return true;
            }
        }

        dropoff = default;
        return false;
    }

    private Vector2Int ResolveActorGridPosition(CharacterActor actor)
    {
        if (actor != null && TryGetGrid(out Grid grid))
        {
            return grid.GetXY(actor.transform.position);
        }

        return Vector2Int.zero;
    }

    private WorldItemStackRecord FindMergeTarget(
        string itemId,
        Vector2Int position,
        WorldItemStackState state,
        string destinationId,
        string reservedBy,
        bool hasDestinationPosition,
        Vector2Int destinationPosition,
        int maxStack)
    {
        return stacks.FirstOrDefault(stack => stack != null
            && stack.quantity > 0
            && stack.quantity < maxStack
            && stack.position == position
            && stack.state == state
            && string.Equals(stack.itemId, itemId, StringComparison.Ordinal)
            && string.Equals(stack.destinationId ?? string.Empty, destinationId ?? string.Empty, StringComparison.Ordinal)
            && string.Equals(stack.reservedByPersistentId ?? string.Empty, reservedBy ?? string.Empty, StringComparison.Ordinal)
            && stack.hasDestinationPosition == hasDestinationPosition
            && (!hasDestinationPosition || stack.destinationPosition == destinationPosition));
    }

    private void AddRecord(WorldItemStackRecord record)
    {
        if (record == null || string.IsNullOrWhiteSpace(record.stackId))
        {
            return;
        }

        if (stacksById.ContainsKey(record.stackId))
        {
            record.stackId = AllocateStackId();
        }

        stacks.Add(record);
        stacksById[record.stackId] = record;
    }

    private void RemoveRecord(WorldItemStackRecord record)
    {
        if (record == null)
        {
            return;
        }

        stacks.Remove(record);
        stacksById.Remove(record.stackId);
    }

    private void ClearRuntimeStacks()
    {
        stacks.Clear();
        stacksById.Clear();
        foreach (WorldItemStackMarker marker in markersByPosition.Values.Where(marker => marker != null))
        {
            UnityEngine.Object.Destroy(marker.gameObject);
        }

        markersByPosition.Clear();
    }

    private string AllocateStackId()
    {
        return "stack:" + nextStackSequence++.ToString("D8", CultureInfo.InvariantCulture);
    }

    private WorldItemStackSaveData ToSaveData(WorldItemStackRecord stack)
    {
        return new WorldItemStackSaveData
        {
            stackId = stack.stackId,
            itemId = stack.itemId,
            quantity = Mathf.Max(0, stack.quantity),
            state = stack.state,
            gridX = stack.position.x,
            gridY = stack.position.y,
            reservedByPersistentId = stack.reservedByPersistentId ?? string.Empty,
            destinationId = stack.destinationId ?? string.Empty,
            hasDestinationPosition = stack.hasDestinationPosition,
            destinationGridX = stack.destinationPosition.x,
            destinationGridY = stack.destinationPosition.y,
            forbidden = stack.forbidden
        };
    }

    private WorldItemStackSnapshot ToSnapshot(WorldItemStackRecord stack)
    {
        DungeonItemDefinition definition = catalogProvider.GetDefinition(stack.itemId);
        return new WorldItemStackSnapshot
        {
            StackId = stack.stackId,
            ItemId = stack.itemId,
            DisplayName = definition.DisplayName,
            Description = definition.Description,
            StockCategory = definition.StockCategory,
            Quantity = Mathf.Max(0, stack.quantity),
            UnitPrice = definition.UnitPrice,
            UnitWeight = definition.UnitWeight,
            Sprite = definition.Sprite,
            State = stack.state,
            Position = stack.position,
            ReservedByPersistentId = stack.reservedByPersistentId ?? string.Empty,
            DestinationId = stack.destinationId ?? string.Empty,
            HasDestinationPosition = stack.hasDestinationPosition,
            DestinationPosition = stack.destinationPosition,
            Forbidden = stack.forbidden
        };
    }

    private void RefreshAllMarkers()
    {
        foreach (Vector2Int position in stacks
            .Where(stack => stack != null)
            .Select(stack => stack.position)
            .Distinct()
            .ToArray())
        {
            RefreshMarkerAt(position);
        }
    }

    private void RefreshMarkerAt(Vector2Int position)
    {
        if (!TryGetGrid(out Grid grid))
        {
            return;
        }

        if (!TryGetPileAt(position, out WorldItemPileSnapshot pile))
        {
            if (markersByPosition.TryGetValue(position, out WorldItemStackMarker existing)
                && existing != null)
            {
                grid.RemoveOccupant(existing, GridLayer.Item, new[] { position }, disconnectPositions: false);
                UnityEngine.Object.Destroy(existing.gameObject);
            }

            markersByPosition.Remove(position);
            return;
        }

        if (!markersByPosition.TryGetValue(position, out WorldItemStackMarker marker) || marker == null)
        {
            marker = WorldItemStackMarker.Create(this, grid, position);
            markersByPosition[position] = marker;
        }

        marker.Refresh(pile);
    }

    private bool TryGetGrid(out Grid grid)
    {
        return gridSystemProvider.TryGetGrid(out grid);
    }

    private static bool IsVisibleState(WorldItemStackState state, bool includeStored)
    {
        return state == WorldItemStackState.Loose
            || state == WorldItemStackState.FacilityBuffer
            || state == WorldItemStackState.ExpeditionPacked
            || (includeStored && state == WorldItemStackState.Stored);
    }

    private static int GetStateSortOrder(WorldItemStackSnapshot stack)
    {
        return stack.State switch
        {
            WorldItemStackState.Loose => 0,
            WorldItemStackState.FacilityBuffer => 1,
            WorldItemStackState.ExpeditionPacked => 2,
            WorldItemStackState.Stored => 3,
            _ => 3
        };
    }

    private static WorldItemStackSnapshot SelectRepresentative(
        IReadOnlyList<WorldItemStackSnapshot> stacks)
    {
        return stacks
            .OrderBy(GetStateSortOrder)
            .ThenByDescending(stack => stack.TotalValue)
            .ThenBy(stack => stack.DisplayName, StringComparer.Ordinal)
            .FirstOrDefault();
    }
}

public sealed class WorldItemStackMarker : MonoBehaviour, IGridOccupant
{
    private static Sprite fallbackSprite;
    private static TMP_FontAsset markerFont;

    private Grid grid;
    private WorldItemStackRuntime runtime;
    private Vector2Int position;
    private SpriteRenderer spriteRenderer;
    private TextMeshPro quantityText;
    private TextMeshPro kindText;
    private GameObject tooltipRoot;
    private SpriteRenderer tooltipBackground;
    private TextMeshPro tooltipText;
    private bool tooltipVisible;

    public int GridId => -500000 - Mathf.Abs(position.GetHashCode());
    public bool IsGridDestroyed => this == null || gameObject == null;
    public bool IsGridVisitable => false;
    public bool IsGridMovement => false;

    public static WorldItemStackMarker Create(
        WorldItemStackRuntime runtime,
        Grid grid,
        Vector2Int position)
    {
        GameObject markerObject = new GameObject($"ItemPile_{position.x}_{position.y}");
        WorldItemStackMarker marker = markerObject.AddComponent<WorldItemStackMarker>();
        marker.Initialize(runtime, grid, position);
        return marker;
    }

    private void Initialize(WorldItemStackRuntime sourceRuntime, Grid sourceGrid, Vector2Int gridPosition)
    {
        runtime = sourceRuntime;
        grid = sourceGrid;
        position = gridPosition;
        transform.position = grid.GetWorldPos(position) + new Vector3(0f, 0.18f, 0f);

        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetFallbackSprite();
        spriteRenderer.color = new Color(0.9f, 0.82f, 0.36f, 0.92f);
        spriteRenderer.sortingLayerName = "DungeonBackObject";
        spriteRenderer.sortingOrder = 640;

        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.62f, 0.54f);

        quantityText = CreateWorldText("Quantity", new Vector3(0f, 0.22f, 0f), 2.2f);
        kindText = CreateWorldText("KindCount", new Vector3(0f, -0.18f, 0f), 1.55f);
        EnsureTooltip();
        SetTooltipVisible(false);

        grid.RegisterOccupant(this, GridLayer.Item, new[] { position }, connectPositions: false);
    }

    private void Update()
    {
        if (runtime == null || grid == null)
        {
            SetTooltipVisible(false);
            return;
        }

        Camera camera = Camera.main;
        if (camera == null || !TryGetPointerPosition(out Vector3 screenPosition))
        {
            SetTooltipVisible(false);
            return;
        }

        screenPosition.z = -camera.transform.position.z;
        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
        WorldItemPileSnapshot pile = null;
        bool shouldShow = grid.GetXY(worldPosition) == position
            && runtime.TryGetPileAt(position, out pile)
            && pile.Representative != null;

        SetTooltipVisible(shouldShow);
        if (shouldShow)
        {
            RefreshTooltip(pile);
        }
    }

    public void Refresh(WorldItemPileSnapshot pile)
    {
        if (pile == null || pile.Representative == null)
        {
            return;
        }

        if (pile.Representative.Sprite != null)
        {
            spriteRenderer.sprite = pile.Representative.Sprite;
            spriteRenderer.color = Color.white;
        }
        else
        {
            spriteRenderer.sprite = GetFallbackSprite();
            spriteRenderer.color = pile.HasReservedItems
                ? new Color(0.82f, 0.72f, 0.32f, 0.82f)
                : new Color(0.9f, 0.82f, 0.36f, 0.92f);
        }

        quantityText.text = pile.TotalQuantity.ToString(CultureInfo.InvariantCulture);
        kindText.text = pile.KindCount > 1
            ? pile.KindCount.ToString(CultureInfo.InvariantCulture) + "종"
            : string.Empty;
    }

    private void EnsureTooltip()
    {
        if (tooltipRoot != null)
        {
            return;
        }

        tooltipRoot = new GameObject("ItemPileTooltip");
        tooltipRoot.transform.SetParent(transform, false);
        tooltipRoot.transform.localPosition = new Vector3(0f, 0.76f, 0f);

        tooltipBackground = tooltipRoot.AddComponent<SpriteRenderer>();
        tooltipBackground.sprite = GetFallbackSprite();
        tooltipBackground.color = new Color(0.03f, 0.06f, 0.07f, 0.92f);
        tooltipBackground.sortingLayerName = "DungeonBackObject";
        tooltipBackground.sortingOrder = 670;
        tooltipBackground.transform.localScale = new Vector3(4.5f, 0.72f, 1f);

        tooltipText = CreateWorldText("TooltipText", Vector3.zero, 0.92f);
        tooltipText.transform.SetParent(tooltipRoot.transform, false);
        tooltipText.transform.localPosition = new Vector3(0f, -0.02f, 0f);
        tooltipText.alignment = TextAlignmentOptions.Center;
        tooltipText.fontStyle = FontStyles.Bold;
        tooltipText.sortingLayerID = tooltipBackground.sortingLayerID;
        tooltipText.sortingOrder = tooltipBackground.sortingOrder + 1;
    }

    private void RefreshTooltip(WorldItemPileSnapshot pile)
    {
        EnsureTooltip();
        string text = BuildTooltipText(pile);
        tooltipText.text = text;
        Vector2 preferred = tooltipText.GetPreferredValues(text);
        float width = Mathf.Clamp(preferred.x + 0.7f, 2.4f, 7.4f);
        tooltipBackground.transform.localScale = new Vector3(width, 0.72f, 1f);
    }

    private void SetTooltipVisible(bool visible)
    {
        if (tooltipVisible == visible)
        {
            return;
        }

        tooltipVisible = visible;
        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(visible);
        }
    }

    private static string BuildTooltipText(WorldItemPileSnapshot pile)
    {
        if (pile == null || pile.Representative == null)
        {
            return string.Empty;
        }

        WorldItemStackSnapshot representative = pile.Representative;
        int otherKinds = Mathf.Max(0, pile.KindCount - 1);
        string label = $"{representative.DisplayName} x{representative.Quantity}";
        if (otherKinds > 0)
        {
            label += $" 외 {otherKinds}종";
        }

        label += $" · {pile.TotalWeight:0.#}kg";
        if (pile.HasReservedItems)
        {
            label += " · 일부 예약됨";
        }

        return label;
    }

    private static bool TryGetPointerPosition(out Vector3 screenPosition)
    {
        if (DungeonAutomationInputState.TryGetPointerPosition(out screenPosition))
        {
            return true;
        }

        if (Mouse.current != null)
        {
            Vector2 inputSystemPosition = Mouse.current.position.ReadValue();
            screenPosition = new Vector3(inputSystemPosition.x, inputSystemPosition.y, 0f);
            return !float.IsNaN(screenPosition.x)
                && !float.IsNaN(screenPosition.y)
                && !float.IsInfinity(screenPosition.x)
                && !float.IsInfinity(screenPosition.y);
        }

        screenPosition = Input.mousePosition;
        return true;
    }

    private static Sprite GetFallbackSprite()
    {
        if (fallbackSprite != null)
        {
            return fallbackSprite;
        }

        Texture2D texture = Texture2D.whiteTexture;
        fallbackSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            4f);
        return fallbackSprite;
    }

    private TextMeshPro CreateWorldText(string objectName, Vector3 localPosition, float fontSize)
    {
        GameObject textObject = new GameObject(objectName, typeof(TextMeshPro));
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = localPosition;
        TextMeshPro text = textObject.GetComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.color = Color.white;
        TMP_FontAsset font = ResolveMarkerFont();
        if (font != null)
        {
            text.font = font;
        }
        text.sortingLayerID = spriteRenderer.sortingLayerID;
        text.sortingOrder = spriteRenderer.sortingOrder + 1;
        return text;
    }

    private static TMP_FontAsset ResolveMarkerFont()
    {
        if (markerFont != null)
        {
            return markerFont;
        }

        TmpKoreanFontSettingsSO settings = Resources.Load<TmpKoreanFontSettingsSO>("Config/TMPKoreanFontSettings");
        markerFont = settings != null ? settings.Font : null;
        return markerFont;
    }

    private void OnDestroy()
    {
        if (grid != null)
        {
            grid.RemoveOccupant(this, GridLayer.Item, new[] { position }, disconnectPositions: false);
        }
    }
}
