using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

[DrawWithUnity]
public class CharacterAbility : SerializedMonoBehaviour
{
    private static readonly IGridSystemProvider FallbackGridSystemProvider =
        new GridSystemProvider(new DungeonSceneComponentQuery());

    protected CharacterActor actor;
    protected CharacterIdentity identity;
    protected CharacterAbilityCache abilityCache;
    protected CharacterStats stats;
    protected CharacterLifecycle lifecycle;
    protected CharacterLog log;
    protected Grid grid;
    protected AbilityMove move;
    private IGridSystemProvider gridSystemProvider;

    [Inject]
    public void ConstructCharacterAbility(IGridSystemProvider gridSystemProvider)
    {
        this.gridSystemProvider = gridSystemProvider
            ?? throw new System.ArgumentNullException(nameof(gridSystemProvider));
        CacheCommonReferences();
    }

    protected virtual void Awake()
    {
        actor = GetComponent<CharacterActor>();
        CacheSplitComponents();
    }

    protected virtual void Start()
    {
        CacheCommonReferences();
    }

    protected void CacheCommonReferences()
    {
        if (actor == null)
        {
            actor = GetComponent<CharacterActor>();
        }

        CacheSplitComponents();

        if (this is AbilityMove selfMove)
        {
            move = selfMove;
        }
        else if (abilityCache != null)
        {
            move = abilityCache.GetAbility<AbilityMove>();
        }

        grid = TryGetGrid(out Grid resolvedGrid) ? resolvedGrid : null;
    }

    protected bool TryGetGrid(out Grid resolvedGrid)
    {
        IGridSystemProvider provider = gridSystemProvider ?? FallbackGridSystemProvider;
        return provider.TryGetGrid(out resolvedGrid);
    }

    private void CacheSplitComponents()
    {
        identity = GetComponent<CharacterIdentity>();
        abilityCache = GetComponent<CharacterAbilityCache>();
        stats = GetComponent<CharacterStats>();
        lifecycle = GetComponent<CharacterLifecycle>();
        log = GetComponent<CharacterLog>();
    }

    public virtual void Initializtion(CharacterSO data)
    {
        CacheCommonReferences();
    }
}
