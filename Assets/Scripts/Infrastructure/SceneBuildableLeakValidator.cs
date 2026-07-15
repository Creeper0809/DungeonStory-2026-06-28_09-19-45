using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

public sealed class SceneBuildableLeakValidator : IInitializable
{
    private readonly IDungeonSceneComponentQuery sceneQuery;

    public SceneBuildableLeakValidator(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public void Initialize()
    {
        List<string> invalidSceneObjects = new List<string>();

        CollectMissingScriptObjects(invalidSceneObjects);
        CollectLeakedFacilities(invalidSceneObjects);

        if (invalidSceneObjects.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "Invalid scene objects are saved in the active scene. " +
            "These objects can overlap runtime grid buildings and pollute click/collider selection. " +
            "Remove or repair the following scene objects before PlayMode:\n" +
            string.Join("\n", invalidSceneObjects));
    }

    private void CollectLeakedFacilities(List<string> invalidSceneObjects)
    {
        IReadOnlyList<BuildableObject> buildables = sceneQuery.All<BuildableObject>(includeInactive: true);

        for (int i = 0; i < buildables.Count; i++)
        {
            BuildableObject buildable = buildables[i];
            if (!IsLeakedFacilityRoot(buildable))
            {
                continue;
            }

            invalidSceneObjects.Add(DescribeLeakedBuildable(buildable));
        }
    }

    private static void CollectMissingScriptObjects(List<string> invalidSceneObjects)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            CollectMissingScriptObjects(roots[i], invalidSceneObjects);
        }
    }

    private static void CollectMissingScriptObjects(GameObject gameObject, List<string> invalidSceneObjects)
    {
        if (gameObject == null)
        {
            return;
        }

        Component[] components = gameObject.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                invalidSceneObjects.Add(DescribeMissingScript(gameObject));
                break;
            }
        }

        Transform transform = gameObject.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            CollectMissingScriptObjects(transform.GetChild(i).gameObject, invalidSceneObjects);
        }
    }

    private static bool IsLeakedFacilityRoot(BuildableObject buildable)
    {
        if (buildable == null || buildable.gameObject == null)
        {
            return false;
        }

        bool isFacility = buildable is Shop || buildable is DefenseFacility;
        if (!isFacility)
        {
            return false;
        }

        return buildable.id == 0
            && (buildable.buildPoses == null || buildable.buildPoses.Count == 0)
            && buildable.transform.parent == null;
    }

    private static string DescribeLeakedBuildable(BuildableObject buildable)
    {
        GameObject gameObject = buildable.gameObject;
        return $"- Uninitialized buildable: {gameObject.scene.path} :: {GetHierarchyPath(gameObject)} ({buildable.GetType().Name})";
    }

    private static string DescribeMissingScript(GameObject gameObject)
    {
        return $"- Missing script: {gameObject.scene.path} :: {GetHierarchyPath(gameObject)}";
    }

    private static string GetHierarchyPath(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return "<null>";
        }

        Stack<string> names = new Stack<string>();
        Transform current = gameObject.transform;
        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", names);
    }
}
