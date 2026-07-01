using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Consideration/Random", order = 0)]
public class ConsiderationRandom : Consideration
{
    [SerializeField][Range(0,1)] private float maxNum;
    [SerializeField][Range(0, 1)] private float minNum;
    public override float ScoreConsideration(Character character)
    {
        return Mathf.Clamp01(Random.Range(minNum, maxNum));
    }
}
