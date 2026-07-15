using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;

public class DungeonSceneBackdropFitter : MonoBehaviour
{
    [SerializeField] private Transform backgroundRoot;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Vector2Int horizontalPaddingTiles = new Vector2Int(8, 8);

    private IGridSystemProvider gridSystemProvider;
    private IDungeonBackdropReferenceProvider backdropReferenceProvider;
    private IDungeonBackdropSpriteTilingFactory spriteTilingFactory;
    private GridSystemManager gridSystemManager;

    [Inject]
    public void Construct(
        IGridSystemProvider gridSystemProvider,
        IDungeonBackdropReferenceProvider backdropReferenceProvider,
        IDungeonBackdropSpriteTilingFactory spriteTilingFactory)
    {
        this.gridSystemProvider = gridSystemProvider ?? throw new ArgumentNullException(nameof(gridSystemProvider));
        this.backdropReferenceProvider = backdropReferenceProvider ?? throw new ArgumentNullException(nameof(backdropReferenceProvider));
        this.spriteTilingFactory = spriteTilingFactory ?? throw new ArgumentNullException(nameof(spriteTilingFactory));
        SubscribeToGridExpansionIfInjected();
    }

    private void OnEnable()
    {
        SubscribeToGridExpansionIfInjected();
    }

    private void Start()
    {
        SubscribeToGridExpansion();
        FitToGrid();
    }

    private void OnDisable()
    {
        UnsubscribeFromGridExpansion();
    }

    public void FitToGrid()
    {
        Grid grid = RequireGridSystemProvider().Grid;
        int width = grid.width;
        if (width <= 0)
        {
            throw new InvalidOperationException($"{nameof(DungeonSceneBackdropFitter)} received an invalid grid width.");
        }

        int leftTile = -width - Mathf.Max(0, horizontalPaddingTiles.x);
        int rightTile = Mathf.Max(0, horizontalPaddingTiles.y);
        ExtendGround(leftTile, rightTile);
        ExtendBackgroundSprites(leftTile, rightTile);
        FitSolidBackground(leftTile, rightTile);
    }

    private void SubscribeToGridExpansionIfInjected()
    {
        if (gridSystemProvider != null && isActiveAndEnabled)
        {
            SubscribeToGridExpansion();
        }
    }

    private void SubscribeToGridExpansion()
    {
        GridSystemManager manager = RequireGridSystemProvider().Manager;
        if (gridSystemManager == manager)
        {
            return;
        }

        UnsubscribeFromGridExpansion();
        gridSystemManager = manager;
        gridSystemManager.OnGridExpand += FitToGrid;
    }

    private void UnsubscribeFromGridExpansion()
    {
        if (gridSystemManager == null)
        {
            return;
        }

        gridSystemManager.OnGridExpand -= FitToGrid;
        gridSystemManager = null;
    }

    private IGridSystemProvider RequireGridSystemProvider()
    {
        return gridSystemProvider
            ?? throw new InvalidOperationException($"{nameof(DungeonSceneBackdropFitter)} requires {nameof(IGridSystemProvider)} injection.");
    }

    private IDungeonBackdropReferenceProvider RequireBackdropReferenceProvider()
    {
        return backdropReferenceProvider
            ?? throw new InvalidOperationException($"{nameof(DungeonSceneBackdropFitter)} requires {nameof(IDungeonBackdropReferenceProvider)} injection.");
    }

    private IDungeonBackdropSpriteTilingFactory RequireSpriteTilingFactory()
    {
        return spriteTilingFactory
            ?? throw new InvalidOperationException($"{nameof(DungeonSceneBackdropFitter)} requires {nameof(IDungeonBackdropSpriteTilingFactory)} injection.");
    }

    private Transform ResolveBackgroundRoot()
    {
        if (backgroundRoot != null) return backgroundRoot;

        return RequireBackdropReferenceProvider().BackgroundRoot;
    }

    private Tilemap ResolveGroundTilemap()
    {
        if (groundTilemap != null) return groundTilemap;

        return RequireBackdropReferenceProvider().GroundTilemap;
    }

    private void ExtendGround(int leftTile, int rightTile)
    {
        Tilemap targetTilemap = ResolveGroundTilemap();

        Dictionary<int, TileBase> sampleByY = new Dictionary<int, TileBase>();
        BoundsInt bounds = targetTilemap.cellBounds;
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                TileBase tile = targetTilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile == null) continue;

                sampleByY[y] = tile;
                break;
            }
        }

        foreach (KeyValuePair<int, TileBase> pair in sampleByY)
        {
            for (int x = leftTile; x <= rightTile; x++)
            {
                Vector3Int position = new Vector3Int(x, pair.Key, 0);
                if (targetTilemap.GetTile(position) == null)
                {
                    targetTilemap.SetTile(position, pair.Value);
                }
            }
        }
    }

    private void ExtendBackgroundSprites(int leftTile, int rightTile)
    {
        Transform targetBackgroundRoot = ResolveBackgroundRoot();

        Dictionary<string, List<SpriteRenderer>> renderersBySprite = new Dictionary<string, List<SpriteRenderer>>();
        foreach (SpriteRenderer renderer in targetBackgroundRoot.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (!IsTiledBackgroundRenderer(renderer)) continue;

            string spriteName = renderer.sprite.name;
            if (!renderersBySprite.TryGetValue(spriteName, out List<SpriteRenderer> renderers))
            {
                renderers = new List<SpriteRenderer>();
                renderersBySprite[spriteName] = renderers;
            }

            renderers.Add(renderer);
        }

        foreach (List<SpriteRenderer> renderers in renderersBySprite.Values)
        {
            ExtendBackgroundSpriteGroup(renderers, leftTile, rightTile);
        }
    }

    private static bool IsTiledBackgroundRenderer(SpriteRenderer renderer)
    {
        return renderer != null
            && renderer.sprite != null
            && renderer.sprite.name.StartsWith("BACKGROUND")
            && renderer.sprite.name != "Square";
    }

    private void ExtendBackgroundSpriteGroup(List<SpriteRenderer> renderers, int leftTile, int rightTile)
    {
        if (renderers == null || renderers.Count == 0) return;

        renderers.Sort((a, b) => a.bounds.min.x.CompareTo(b.bounds.min.x));
        SpriteRenderer template = renderers[0];
        float tileWidth = template.bounds.size.x;
        if (tileWidth <= 0.01f) return;

        float currentMin = GetMinX(renderers);
        while (currentMin > leftTile)
        {
            SpriteRenderer copy = RequireSpriteTilingFactory().Duplicate(template, currentMin - tileWidth);
            renderers.Add(copy);
            currentMin -= tileWidth;
        }

        float currentMax = GetMaxX(renderers);
        while (currentMax < rightTile)
        {
            SpriteRenderer copy = RequireSpriteTilingFactory().Duplicate(template, currentMax);
            renderers.Add(copy);
            currentMax += tileWidth;
        }
    }

    private void FitSolidBackground(int leftTile, int rightTile)
    {
        Transform targetBackgroundRoot = ResolveBackgroundRoot();

        foreach (SpriteRenderer renderer in targetBackgroundRoot.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer == null || renderer.sprite == null || renderer.sprite.name != "Square") continue;

            Vector3 position = renderer.transform.position;
            position.x = (leftTile + rightTile) * 0.5f;
            renderer.transform.position = position;

            Vector3 scale = renderer.transform.localScale;
            scale.x = Mathf.Max(scale.x, Mathf.Abs(rightTile - leftTile) + 4f);
            renderer.transform.localScale = scale;
        }
    }

    private static float GetMinX(IEnumerable<SpriteRenderer> renderers)
    {
        float min = float.PositiveInfinity;
        foreach (SpriteRenderer renderer in renderers)
        {
            min = Mathf.Min(min, renderer.bounds.min.x);
        }

        return min;
    }

    private static float GetMaxX(IEnumerable<SpriteRenderer> renderers)
    {
        float max = float.NegativeInfinity;
        foreach (SpriteRenderer renderer in renderers)
        {
            max = Mathf.Max(max, renderer.bounds.max.x);
        }

        return max;
    }
}
