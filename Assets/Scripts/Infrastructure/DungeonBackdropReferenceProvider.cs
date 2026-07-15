using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IDungeonBackdropReferenceProvider
{
    Transform BackgroundRoot { get; }
    Tilemap GroundTilemap { get; }
}

public sealed class DungeonBackdropReferenceProvider : IDungeonBackdropReferenceProvider
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private Transform backgroundRoot;
    private Tilemap groundTilemap;

    public DungeonBackdropReferenceProvider(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public Transform BackgroundRoot
    {
        get
        {
            backgroundRoot ??= FindTransform("BackGround");
            return backgroundRoot;
        }
    }

    public Tilemap GroundTilemap
    {
        get
        {
            groundTilemap ??= FindTilemap("Ground");
            return groundTilemap;
        }
    }

    private Transform FindTransform(string objectName)
    {
        foreach (Transform transform in sceneQuery.All<Transform>(includeInactive: true))
        {
            if (transform != null && transform.name == objectName)
            {
                return transform;
            }
        }

        throw new InvalidOperationException($"{nameof(IDungeonBackdropReferenceProvider)} requires a loaded '{objectName}' transform.");
    }

    private Tilemap FindTilemap(string tilemapName)
    {
        foreach (Tilemap tilemap in sceneQuery.All<Tilemap>(includeInactive: true))
        {
            if (tilemap != null && tilemap.name == tilemapName)
            {
                return tilemap;
            }
        }

        throw new InvalidOperationException($"{nameof(IDungeonBackdropReferenceProvider)} requires a loaded '{tilemapName}' tilemap.");
    }
}
