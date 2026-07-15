using System;
using System.Linq;

public readonly struct AiDirectorContextSceneSnapshot
{
    public AiDirectorContextSceneSnapshot(CharacterActor[] actors, BuildableObject[] facilities)
    {
        Actors = actors ?? Array.Empty<CharacterActor>();
        Facilities = facilities ?? Array.Empty<BuildableObject>();
    }

    public CharacterActor[] Actors { get; }
    public BuildableObject[] Facilities { get; }
}

public interface IAiDirectorContextSceneQuery
{
    AiDirectorContextSceneSnapshot Capture();
}

public sealed class AiDirectorContextSceneQuery : IAiDirectorContextSceneQuery
{
    private readonly IDungeonSceneComponentQuery sceneQuery;

    public AiDirectorContextSceneQuery(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public AiDirectorContextSceneSnapshot Capture()
    {
        return new AiDirectorContextSceneSnapshot(
            sceneQuery.All<CharacterActor>().ToArray(),
            sceneQuery.All<BuildableObject>().ToArray());
    }
}
