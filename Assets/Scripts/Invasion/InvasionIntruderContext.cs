using System;
using UnityEngine;

public readonly struct InvasionIntruderEntry
{
    public InvasionIntruderEntry(
        Vector2Int gridPosition,
        Vector3 outsidePosition,
        Vector3 doorPosition)
    {
        GridPosition = gridPosition;
        OutsidePosition = outsidePosition;
        DoorPosition = doorPosition;
    }

    public Vector2Int GridPosition { get; }
    public Vector3 OutsidePosition { get; }
    public Vector3 DoorPosition { get; }
}

public interface IInvasionIntruderContext
{
    bool TryGetGrid(out Grid grid);
    bool TryGetOwner(out CharacterActor owner);
    bool TryResolveEntry(out InvasionIntruderEntry entry);
    InvasionIntruderSettings ApplyRunVariables(InvasionIntruderSettings source);
}

public sealed class InvasionIntruderContext : IInvasionIntruderContext
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private readonly IGridSystemProvider gridSystemProvider;
    private readonly IRunVariableRuntimeReader runVariableReader;
    private CharacterSpawner spawner;
    private OwnerRunManager ownerRunManager;

    public InvasionIntruderContext(
        IDungeonSceneComponentQuery sceneQuery,
        IGridSystemProvider gridSystemProvider,
        IRunVariableRuntimeReader runVariableReader)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
        this.gridSystemProvider = gridSystemProvider
            ?? throw new ArgumentNullException(nameof(gridSystemProvider));
        this.runVariableReader = runVariableReader
            ?? throw new ArgumentNullException(nameof(runVariableReader));
    }

    public bool TryGetGrid(out Grid grid)
    {
        grid = null;
        try
        {
            grid = gridSystemProvider.Grid;
            return grid != null;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public bool TryGetOwner(out CharacterActor owner)
    {
        ownerRunManager ??= sceneQuery.First<OwnerRunManager>(includeInactive: true);
        owner = ownerRunManager != null ? ownerRunManager.CurrentOwnerActor : null;
        return owner != null;
    }

    public bool TryResolveEntry(out InvasionIntruderEntry entry)
    {
        TryGetGrid(out Grid grid);
        spawner ??= sceneQuery.First<CharacterSpawner>(includeInactive: true);
        return InvasionIntruderEntryResolver.TryResolve(spawner, grid, out entry);
    }

    public InvasionIntruderSettings ApplyRunVariables(InvasionIntruderSettings source)
    {
        return runVariableReader.ApplyInvasionSettings(source);
    }
}
