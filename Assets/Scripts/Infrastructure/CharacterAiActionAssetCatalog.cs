using System;

public interface ICharacterAiActionAssetCatalog
{
    T GetRequiredAction<T>(string resourcePath) where T : AIActionSet;
    AIFacilityRoleAction GetRequiredFacilityRoleAction(string resourcePath, FacilityRole role);
}

public sealed class ResourceCharacterAiActionAssetCatalog : ICharacterAiActionAssetCatalog
{
    private readonly IResourcesAssetLoader resourcesAssetLoader;

    public ResourceCharacterAiActionAssetCatalog(IResourcesAssetLoader resourcesAssetLoader)
    {
        this.resourcesAssetLoader = resourcesAssetLoader
            ?? throw new ArgumentNullException(nameof(resourcesAssetLoader));
    }

    public T GetRequiredAction<T>(string resourcePath) where T : AIActionSet
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            throw new ArgumentException("AI action resource path is required.", nameof(resourcePath));
        }

        return resourcesAssetLoader.LoadRequired<T>(resourcePath);
    }

    public AIFacilityRoleAction GetRequiredFacilityRoleAction(string resourcePath, FacilityRole role)
    {
        AIFacilityRoleAction actionSet = GetRequiredAction<AIFacilityRoleAction>(resourcePath);
        if (actionSet.Role != role)
        {
            throw new InvalidOperationException(
                $"Required AI action asset has wrong role: Resources/{resourcePath} expected={role} actual={actionSet.Role}");
        }

        return actionSet;
    }
}
