using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterIdentity : SerializedMonoBehaviour
{
    [SerializeField]
    [ReadOnly]
    private CharacterActor actor;
    [SerializeField]
    private CharacterSO data;
    [SerializeField]
    [ReadOnly]
    private CharacterRuntimeProfile profile;
    [SerializeField]
    [ReadOnly]
    private CharacterType characterType = CharacterType.Customer;
    [SerializeField]
    [ReadOnly]
    private CharacterRole role = CharacterRole.Regular;

    public CharacterSO Data => data;
    public CharacterRuntimeProfile Profile => profile;
    public CharacterType CharacterType => characterType;
    public CharacterRole Role => role;
    public bool IsOwner => role == CharacterRole.Owner;
    public bool CanLeaveByDissatisfaction => !IsOwner;
    public bool CanRebel => !IsOwner;
    public int StableId => data != null
        ? data.id
        : actor != null ? actor.GetInstanceID() : GetInstanceID();
    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(data != null ? data.characterName : null))
            {
                return data.characterName;
            }

            if (!string.IsNullOrWhiteSpace(actor != null ? actor.name : null))
            {
                return actor.name;
            }

            return name;
        }
    }
    public string SpeciesTag => profile != null ? profile.SpeciesTag : data != null ? data.SpeciesTag : string.Empty;

    private void Awake()
    {
        Bind(GetComponent<CharacterActor>());
        if (data != null && profile == null)
        {
            SetData(data);
        }
    }

    public void Bind(CharacterActor owner)
    {
        actor = owner;
    }

    public void SetData(CharacterSO nextData)
    {
        data = nextData;
        profile = data != null ? data.CreateRuntimeProfile() : null;
        characterType = data != null ? data.characterType : CharacterType.Customer;
        role = data != null ? data.role : CharacterRole.Regular;
    }

    public void SetCharacterType(CharacterType nextType)
    {
        characterType = nextType;
    }

    public string GetSpeciesShortDescription()
    {
        return profile != null ? profile.GetShortDescription() : string.Empty;
    }
}
