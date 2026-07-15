using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomEnvironmentSettings", menuName = "DungeonStory/Rooms/Environment Settings", order = 0)]
public sealed class RoomEnvironmentSettingsSO : ScriptableObject
{
    [Header("Environment Formula")]
    [SerializeField, Min(1f)] private float spaciousAreaMinimum = 2f;
    [SerializeField, Min(2f)] private float spaciousAreaMaximum = 16f;
    [SerializeField, Range(0f, 1f)] private float spaciousAreaWeight = 0.45f;
    [SerializeField, Range(0f, 1f)] private float spaciousFreeCellWeight = 0.55f;
    [SerializeField] private float beautyBaseline = 50f;
    [SerializeField] private float luxuryMultiplier = 1.25f;
    [SerializeField] private float beautyDamagePenalty = 30f;
    [SerializeField, Range(0f, 1f)] private float beautyCrowdingThreshold = 0.6f;
    [SerializeField] private float beautyCrowdingPenalty = 50f;
    [SerializeField] private float cleanlinessBaseline = 60f;
    [SerializeField] private float hygieneFacilityContribution = 10f;
    [SerializeField] private float hygieneContributionMaximum = 25f;
    [SerializeField] private float cleanStreakContribution = 2f;
    [SerializeField] private float cleanStreakContributionMaximum = 10f;
    [SerializeField] private float cleanlinessDamagePenalty = 35f;
    [SerializeField, Range(0f, 1f)] private float cleanlinessCrowdingThreshold = 0.5f;
    [SerializeField] private float cleanlinessCrowdingPenalty = 40f;
    [SerializeField, Range(0f, 1f)] private float impressivenessBeautyWeight = 0.35f;
    [SerializeField, Range(0f, 1f)] private float impressivenessSpaciousnessWeight = 0.3f;
    [SerializeField, Range(0f, 1f)] private float impressivenessCleanlinessWeight = 0.2f;
    [SerializeField, Range(0f, 1f)] private float impressivenessQualityWeight = 0.15f;

    [Header("Mood")]
    [SerializeField, Min(0.25f)] private float moodDurationSeconds = 180f;
    [SerializeField] private float awfulRoomMood = -6f;
    [SerializeField] private float poorRoomMood = -3f;
    [SerializeField] private float goodRoomMood = 3f;
    [SerializeField] private float excellentRoomMood = 6f;
    [SerializeField] private float filthyRoomMood = -4f;
    [SerializeField] private float dirtyRoomMood = -2f;
    [SerializeField] private float cleanRoomMood = 2f;

    [Header("Room Role Colors")]
    [SerializeField] private Color diningColor = new Color(0.84f, 0.65f, 0.29f, 1f);
    [SerializeField] private Color shopColor = new Color(0.91f, 0.78f, 0.36f, 1f);
    [SerializeField] private Color restColor = new Color(0.31f, 0.65f, 0.78f, 1f);
    [SerializeField] private Color trainingColor = new Color(0.79f, 0.36f, 0.36f, 1f);
    [SerializeField] private Color researchColor = new Color(0.31f, 0.69f, 0.46f, 1f);
    [SerializeField] private Color manaColor = new Color(0.60f, 0.42f, 0.78f, 1f);
    [SerializeField] private Color storageColor = new Color(0.53f, 0.56f, 0.61f, 1f);
    [SerializeField] private Color toiletColor = new Color(0.31f, 0.51f, 0.72f, 1f);
    [SerializeField] private Color hygieneColor = new Color(0.33f, 0.72f, 0.63f, 1f);
    [SerializeField] private Color administrationColor = new Color(0.74f, 0.57f, 0.32f, 1f);
    [SerializeField] private Color securityColor = new Color(0.67f, 0.33f, 0.31f, 1f);
    [SerializeField] private Color mixedColor = new Color(0.77f, 0.80f, 0.83f, 1f);
    [SerializeField] private Color undefinedColor = new Color(0.55f, 0.58f, 0.61f, 1f);

    public float SpaciousAreaMinimum => spaciousAreaMinimum;
    public float SpaciousAreaMaximum => Mathf.Max(spaciousAreaMinimum + 1f, spaciousAreaMaximum);
    public float SpaciousAreaWeight => spaciousAreaWeight;
    public float SpaciousFreeCellWeight => spaciousFreeCellWeight;
    public float BeautyBaseline => beautyBaseline;
    public float LuxuryMultiplier => luxuryMultiplier;
    public float BeautyDamagePenalty => beautyDamagePenalty;
    public float BeautyCrowdingThreshold => beautyCrowdingThreshold;
    public float BeautyCrowdingPenalty => beautyCrowdingPenalty;
    public float CleanlinessBaseline => cleanlinessBaseline;
    public float HygieneFacilityContribution => hygieneFacilityContribution;
    public float HygieneContributionMaximum => hygieneContributionMaximum;
    public float CleanStreakContribution => cleanStreakContribution;
    public float CleanStreakContributionMaximum => cleanStreakContributionMaximum;
    public float CleanlinessDamagePenalty => cleanlinessDamagePenalty;
    public float CleanlinessCrowdingThreshold => cleanlinessCrowdingThreshold;
    public float CleanlinessCrowdingPenalty => cleanlinessCrowdingPenalty;
    public float ImpressivenessBeautyWeight => impressivenessBeautyWeight;
    public float ImpressivenessSpaciousnessWeight => impressivenessSpaciousnessWeight;
    public float ImpressivenessCleanlinessWeight => impressivenessCleanlinessWeight;
    public float ImpressivenessQualityWeight => impressivenessQualityWeight;
    public float MoodDurationSeconds => moodDurationSeconds;

    public float GetImpressivenessMood(float value)
    {
        if (value < 20f) return awfulRoomMood;
        if (value < 40f) return poorRoomMood;
        if (value < 60f) return 0f;
        if (value < 80f) return goodRoomMood;
        return excellentRoomMood;
    }

    public float GetCleanlinessMood(float value)
    {
        if (value < 20f) return filthyRoomMood;
        if (value < 40f) return dirtyRoomMood;
        if (value < 80f) return 0f;
        return cleanRoomMood;
    }

    public Color GetRoleColor(RoomRole role, bool mixed)
    {
        if (mixed) return mixedColor;

        return role switch
        {
            RoomRole.Dining => diningColor,
            RoomRole.Shop => shopColor,
            RoomRole.Rest => restColor,
            RoomRole.Training => trainingColor,
            RoomRole.Research => researchColor,
            RoomRole.Mana => manaColor,
            RoomRole.Storage => storageColor,
            RoomRole.Toilet => toiletColor,
            RoomRole.Hygiene => hygieneColor,
            RoomRole.Administration => administrationColor,
            RoomRole.Security => securityColor,
            _ => undefinedColor
        };
    }
}

public interface IRoomEnvironmentSettingsProvider
{
    RoomEnvironmentSettingsSO Settings { get; }
}

public sealed class ResourceRoomEnvironmentSettingsProvider : IRoomEnvironmentSettingsProvider
{
    public const string ResourcePath = "Config/RoomEnvironmentSettings";

    private readonly IResourcesAssetLoader assetLoader;
    private RoomEnvironmentSettingsSO settings;

    public ResourceRoomEnvironmentSettingsProvider(IResourcesAssetLoader assetLoader)
    {
        this.assetLoader = assetLoader
            ?? throw new ArgumentNullException(nameof(assetLoader));
    }

    public RoomEnvironmentSettingsSO Settings =>
        settings ??= assetLoader.LoadRequired<RoomEnvironmentSettingsSO>(ResourcePath);
}
