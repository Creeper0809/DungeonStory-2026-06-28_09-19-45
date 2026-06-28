using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/AI/Consideration/Stat", order = 0)]
public class ConsiderationStat : Consideration
{
    [SerializeField] private Character.Condition affectedStat;
    [SerializeField] private AnimationCurve curve;
    public override float ScoreConsideration(Character character)
    {
        return curve.Evaluate(Mathf.Clamp01(character.stats[affectedStat] / 100f));
    }
}
