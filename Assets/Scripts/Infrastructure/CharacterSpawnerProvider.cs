using System;

public interface ICharacterSpawnerProvider
{
    bool TryGetSpawner(out CharacterSpawner spawner);
}

public sealed class CharacterSpawnerProvider : ICharacterSpawnerProvider
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private CharacterSpawner spawner;

    public CharacterSpawnerProvider(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public bool TryGetSpawner(out CharacterSpawner resolvedSpawner)
    {
        spawner ??= sceneQuery.First<CharacterSpawner>(includeInactive: true);
        resolvedSpawner = spawner;
        return resolvedSpawner != null;
    }
}
