using System.Collections.Generic;
using System.Linq;

public interface IStaffWorkforceQueryService
{
    IReadOnlyList<CharacterActor> FindActiveWorkers();
    bool IsActiveWorker(CharacterActor character);
    string GetDisplayName(CharacterActor character);
}

public sealed class StaffWorkforceRuntimeQueryService : IStaffWorkforceQueryService
{
    private readonly IDungeonSceneComponentQuery sceneQuery;

    public StaffWorkforceRuntimeQueryService(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery ?? throw new System.ArgumentNullException(nameof(sceneQuery));
    }

    public IReadOnlyList<CharacterActor> FindActiveWorkers()
    {
        return sceneQuery.All<CharacterActor>()
            .Where(IsActiveWorker)
            .OrderByDescending((character) => character.IsOwner)
            .ThenBy(GetDisplayName)
            .ToList();
    }

    public bool IsActiveWorker(CharacterActor character)
    {
        return character != null
            && !character.IsDead
            && CharacterWorkRoleUtility.TryGetWork(character, out _);
    }

    public string GetDisplayName(CharacterActor character)
    {
        if (character == null)
        {
            return string.Empty;
        }

        CharacterIdentity identity = character.Identity;
        if (!string.IsNullOrWhiteSpace(identity != null ? identity.DisplayName : null))
        {
            return identity.DisplayName;
        }

        return character.name;
    }
}
