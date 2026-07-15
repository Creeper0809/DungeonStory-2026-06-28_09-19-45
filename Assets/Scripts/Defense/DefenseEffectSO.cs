using UnityEngine;

[CreateAssetMenu(menuName = "DungeonStory/Defense/Effect", order = 0)]
public class DefenseEffectSO : ScriptableObject
{
    [SerializeField] private DefenseEffectData effect = new DefenseEffectData();

    public DefenseEffectData Effect => effect;

    public DefenseEffectKind Kind
    {
        get => effect.kind;
        set => effect.kind = value;
    }

    public float Amount
    {
        get => effect.amount;
        set => effect.amount = Mathf.Max(0f, value);
    }

    public float Duration
    {
        get => effect.duration;
        set => effect.duration = Mathf.Max(0f, value);
    }

    public int Stacks
    {
        get => effect.stacks;
        set => effect.stacks = Mathf.Max(1, value);
    }

    public string LogTag
    {
        get => effect.logTag;
        set => effect.logTag = value;
    }

    public void Apply(
        CharacterActor target,
        DefenseStatusRuntime statusRuntime,
        DefenseActivationReport report,
        DefenseFacilityData defense)
    {
        DefenseEffectResolver.ApplyEffect(effect, target, statusRuntime, report, defense);
    }

}
