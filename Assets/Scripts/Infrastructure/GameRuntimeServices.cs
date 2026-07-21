using System;
using DamageNumbersPro;
using UnityEngine;

public interface IGameDataProvider
{
    bool TryGetGameData(out GameData gameData);
}

public interface IFloatingNumberFeedbackService
{
    bool TryShow(NumberCondition condition, Vector3 worldPosition, float value);
}

public sealed class GameManagerGameDataProvider : IGameDataProvider
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private GameManager gameManager;

    public GameManagerGameDataProvider(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public bool TryGetGameData(out GameData gameData)
    {
        if (gameManager == null)
        {
            gameManager = sceneQuery.First<GameManager>(includeInactive: true);
        }

        gameData = gameManager != null ? gameManager.gameData : null;
        return gameData != null;
    }
}

public sealed class GameManagerFloatingNumberFeedbackService : IFloatingNumberFeedbackService
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private GameManager gameManager;

    public GameManagerFloatingNumberFeedbackService(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public bool TryShow(NumberCondition condition, Vector3 worldPosition, float value)
    {
        GameManager manager = ResolveGameManager();
        if (manager.numbers == null
            || !manager.numbers.TryGetValue(condition, out DamageNumber number)
            || number == null)
        {
            return false;
        }

        number.Spawn(worldPosition, value);
        return true;
    }

    private GameManager ResolveGameManager()
    {
        if (gameManager == null)
        {
            gameManager = sceneQuery.First<GameManager>(includeInactive: true);
        }

        return gameManager != null
            ? gameManager
            : throw new InvalidOperationException($"{nameof(IFloatingNumberFeedbackService)} requires a loaded {nameof(GameManager)}.");
    }
}
