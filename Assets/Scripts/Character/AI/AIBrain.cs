using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class AIBrain : CharacterAbility
{
    public AIAction[] availableActions;
    [ReadOnly]public AIAction bestAction;
    public bool isBestActionEnd = true;
    public bool isExecuted = false;
    public override void Initializtion(CharacterSO data)
    {
        isBestActionEnd = true;
    }
    public void DecideAction()
    {
        float highestScore = float.MinValue;
        AIAction tempBestAction = null;
        isBestActionEnd = false;
        isExecuted = false;

        foreach (var action in availableActions)
        {
            if (action.CalculateScore(character) > highestScore)
            {
                highestScore = action.score;
                tempBestAction = action;
            }
        }
        bestAction = tempBestAction;
    }
}
public class AIAction
{
    public AIActionSet actionset;
    [ShowInInspector] private float _score;
    public float score
    {
        get { return _score; }
        set
        {
            _score = Mathf.Clamp01(value);
        }
    }
    public BuildableObject destination;
    public Queue<BuildableObject> path;
    public float CalculateScore(Character character)
    {
        float score = 1f;
        foreach (var consideration in actionset.considerations)
        {
            score *= consideration.ScoreConsideration(character);
            if (score == 0f)
            {
                this.score = 0;
                return this.score;
            }
        }
        float modFactor = 1 - (1 - actionset.considerations.Length);
        float makeupValue = (1 - score) * modFactor;
        this.score = score + (makeupValue * score);
        return this.score;
    }
    public void SetDestination(Character character)
    {
        destination = actionset.GetDestination(character);
        Func<Vector2Int, bool> condition = (pos) => GridSystemManager.Instance.grid.GetGridCell(pos).GetBuildingInlayer() == character.ai.bestAction.destination;
        path = GridSystemManager.Instance.grid.SmoothingPath(GridSystemManager.Instance.grid.GetGridPath(character.GetNowXY(), condition));
    }
}
