using System;
using System.Collections.Generic;
using System.Linq;

public readonly struct AiDirectorContextSceneSnapshot
{
    public AiDirectorContextSceneSnapshot(
        IReadOnlyList<CharacterActor> actors,
        IReadOnlyList<BuildableObject> facilities)
    {
        Actors = EventPayloadSnapshot.Copy(actors);
        Facilities = EventPayloadSnapshot.Copy(facilities);
    }

    public IReadOnlyList<CharacterActor> Actors { get; }
    public IReadOnlyList<BuildableObject> Facilities { get; }
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
