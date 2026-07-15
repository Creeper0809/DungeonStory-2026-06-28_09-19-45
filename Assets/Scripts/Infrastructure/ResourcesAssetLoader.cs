using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IResourcesAssetLoader
{
    T LoadRequired<T>(string resourcePath) where T : UnityEngine.Object;
    IReadOnlyCollection<T> LoadAllRequired<T>(string resourcePath) where T : UnityEngine.Object;
}

public sealed class UnityResourcesAssetLoader : IResourcesAssetLoader
{
    public T LoadRequired<T>(string resourcePath) where T : UnityEngine.Object
    {
        ValidateResourcePath(resourcePath);

        T asset = Resources.Load<T>(resourcePath);
        if (asset == null)
        {
            throw new InvalidOperationException(
                $"Required resource asset is missing: Resources/{resourcePath}");
        }

        return asset;
    }

    public IReadOnlyCollection<T> LoadAllRequired<T>(string resourcePath) where T : UnityEngine.Object
    {
        ValidateResourcePath(resourcePath);

        T[] assets = Resources
            .LoadAll<T>(resourcePath)
            .Where((asset) => asset != null)
            .ToArray();

        if (assets.Length == 0)
        {
            throw new InvalidOperationException(
                $"Required resource asset collection is empty: Resources/{resourcePath}");
        }

        return assets;
    }

    private static void ValidateResourcePath(string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            throw new ArgumentException("Resource path is required.", nameof(resourcePath));
        }
    }
}
