using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum SocialRumorType
{
    None = 0,
    Complaint = 1,
    Recommendation = 2,
    Warning = 3,
    Praise = 4
}

public enum SocialRumorTargetType
{
    None = 0,
    Facility = 1,
    Character = 2
}

[Serializable]
public sealed class SocialRumor
{
    public SocialRumorType type;
    public SocialRumorTargetType targetType;
    public int sourceActorId = -1;
    public string sourceActorName;
    public int targetFacilityId = -1;
    public string targetFacilityTag;
    public int targetCharacterId = -1;
    public string targetCharacterName;
    [Range(-1f, 1f)] public float sentiment;
    [Range(0f, 1f)] public float spreadChance;
    [Range(-1f, 1f)] public float trustImpact;
    public float validUntil;
    public string summary;
    public string source;

    public bool IsActionable => type != SocialRumorType.None
        && targetType != SocialRumorTargetType.None
        && !IsExpired;

    public bool IsExpired => validUntil > 0f && Time.time > validUntil;

    public SocialRumor Clone()
    {
        return (SocialRumor)MemberwiseClone();
    }
}

[Serializable]
public sealed class SocialMemoryFloat
{
    public string key;
    public float value;

    public SocialMemoryFloat(string key, float value)
    {
        this.key = key;
        this.value = value;
    }
}

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterActor))]
public sealed class CharacterSocialMemory : SerializedMonoBehaviour
{
    [SerializeField, Min(1)] private int maxRumorMemory = 24;
    [SerializeField, Range(0f, 1f)] private float newRumorBlend = 0.35f;
    [SerializeField, ReadOnly] private CharacterActor actor;
    [SerializeField, ReadOnly] private List<SocialRumor> recentRumors = new List<SocialRumor>();
    [SerializeField, ReadOnly] private List<SocialMemoryFloat> facilitySentimentDebug = new List<SocialMemoryFloat>();
    [SerializeField, ReadOnly] private List<SocialMemoryFloat> characterSentimentDebug = new List<SocialMemoryFloat>();
    [SerializeField, ReadOnly] private List<SocialMemoryFloat> sourceTrustDebug = new List<SocialMemoryFloat>();

    private readonly Dictionary<string, float> facilitySentimentByKey = new Dictionary<string, float>();
    private readonly Dictionary<string, float> characterSentimentByKey = new Dictionary<string, float>();
    private readonly Dictionary<string, float> sourceTrustByKey = new Dictionary<string, float>();

    public IReadOnlyList<SocialRumor> RecentRumors => recentRumors;

    private void Awake()
    {
        Bind(GetComponent<CharacterActor>());
    }

    public void Bind(CharacterActor owner)
    {
        actor = owner;
    }

    public void HearRumor(SocialRumor rumor, CharacterActor speaker)
    {
        if (rumor == null || !rumor.IsActionable)
        {
            return;
        }

        SocialRumor copy = rumor.Clone();
        float trust = GetSourceTrustScore(speaker, copy);
        float weightedSentiment = Mathf.Clamp(copy.sentiment * trust, -1f, 1f);
        copy.sentiment = weightedSentiment;
        RememberRumor(copy);

        float moodImpact = Mathf.Clamp(weightedSentiment * 6f, -6f, 6f);
        if (actor != null && Mathf.Abs(moodImpact) >= 0.5f)
        {
            string sourceKey = speaker != null
                ? SocialRumorUtility.GetActorKey(speaker)
                : "unknown";
            actor.ApplyMoodFactor(
                $"social:rumor:{sourceKey}:{copy.targetType}",
                moodImpact > 0f ? "반가운 소문을 들음" : "불쾌한 소문을 들음",
                moodImpact,
                180f,
                2);
        }

        if (copy.targetType == SocialRumorTargetType.Facility)
        {
            foreach (string key in SocialRumorUtility.GetFacilityKeys(copy))
            {
                Blend(facilitySentimentByKey, key, copy.sentiment, newRumorBlend);
            }
        }
        else if (copy.targetType == SocialRumorTargetType.Character)
        {
            foreach (string key in SocialRumorUtility.GetCharacterKeys(copy))
            {
                Blend(characterSentimentByKey, key, copy.sentiment, newRumorBlend);
            }
        }

        if (speaker != null)
        {
            string trustKey = SocialRumorUtility.GetActorKey(speaker);
            float nextTrust = GetDictionaryValue(sourceTrustByKey, trustKey, 1f) + copy.trustImpact * 0.15f;
            sourceTrustByKey[trustKey] = Mathf.Clamp(nextTrust, 0.25f, 1.5f);
        }

        SyncDebugLists();
    }

    public float GetFacilitySentiment(BuildableObject building)
    {
        if (building == null)
        {
            return 0f;
        }

        PruneExpiredRumors();
        float sum = 0f;
        int count = 0;
        foreach (KeyValuePair<string, float> entry in facilitySentimentByKey)
        {
            if (!SocialRumorUtility.MatchesFacilityKey(building, entry.Key))
            {
                continue;
            }

            sum += entry.Value;
            count++;
        }

        return count > 0 ? Mathf.Clamp(sum / count, -1f, 1f) : 0f;
    }

    public float GetRelationshipSentiment(CharacterActor target)
    {
        if (target == null)
        {
            return 0f;
        }

        string idKey = SocialRumorUtility.GetActorKey(target);
        if (characterSentimentByKey.TryGetValue(idKey, out float idValue))
        {
            return Mathf.Clamp(idValue, -1f, 1f);
        }

        string nameKey = SocialRumorUtility.GetActorNameKey(target);
        return characterSentimentByKey.TryGetValue(nameKey, out float nameValue)
            ? Mathf.Clamp(nameValue, -1f, 1f)
            : 0f;
    }

    public float GetSourceTrust(CharacterActor source)
    {
        if (source == null)
        {
            return 1f;
        }

        string key = SocialRumorUtility.GetActorKey(source);
        return Mathf.Clamp(GetDictionaryValue(sourceTrustByKey, key, 1f), 0.25f, 1.5f);
    }

    private float GetSourceTrustScore(CharacterActor speaker, SocialRumor rumor)
    {
        if (speaker == null)
        {
            return 1f;
        }

        string key = SocialRumorUtility.GetActorKey(speaker);
        float trust = GetDictionaryValue(sourceTrustByKey, key, 1f);
        if (actor != null
            && actor.Identity != null
            && speaker.Identity != null
            && !string.IsNullOrWhiteSpace(actor.Identity.SpeciesTag)
            && string.Equals(actor.Identity.SpeciesTag, speaker.Identity.SpeciesTag, StringComparison.OrdinalIgnoreCase))
        {
            trust += 0.1f;
        }

        if (speaker.Identity != null && rumor.sourceActorId == speaker.Identity.StableId)
        {
            trust += 0.05f;
        }

        return Mathf.Clamp(trust, 0.25f, 1.5f);
    }

    private void RememberRumor(SocialRumor rumor)
    {
        recentRumors.Add(rumor);
        PruneExpiredRumors();
        while (recentRumors.Count > Mathf.Max(1, maxRumorMemory))
        {
            recentRumors.RemoveAt(0);
        }
    }

    private void PruneExpiredRumors()
    {
        bool removed = false;
        for (int i = recentRumors.Count - 1; i >= 0; i--)
        {
            if (recentRumors[i] == null || recentRumors[i].IsExpired)
            {
                recentRumors.RemoveAt(i);
                removed = true;
            }
        }

        if (removed)
        {
            RebuildSentimentMaps();
        }
    }

    private void RebuildSentimentMaps()
    {
        facilitySentimentByKey.Clear();
        characterSentimentByKey.Clear();
        foreach (SocialRumor rumor in recentRumors)
        {
            if (rumor == null || rumor.IsExpired)
            {
                continue;
            }

            if (rumor.targetType == SocialRumorTargetType.Facility)
            {
                foreach (string key in SocialRumorUtility.GetFacilityKeys(rumor))
                {
                    Blend(facilitySentimentByKey, key, rumor.sentiment, newRumorBlend);
                }
            }
            else if (rumor.targetType == SocialRumorTargetType.Character)
            {
                foreach (string key in SocialRumorUtility.GetCharacterKeys(rumor))
                {
                    Blend(characterSentimentByKey, key, rumor.sentiment, newRumorBlend);
                }
            }
        }

        SyncDebugLists();
    }

    private void SyncDebugLists()
    {
        SyncDebugList(facilitySentimentByKey, facilitySentimentDebug);
        SyncDebugList(characterSentimentByKey, characterSentimentDebug);
        SyncDebugList(sourceTrustByKey, sourceTrustDebug);
    }

    private static void Blend(Dictionary<string, float> map, string key, float value, float blend)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        float current = GetDictionaryValue(map, key, 0f);
        map[key] = Mathf.Clamp(Mathf.Lerp(current, value, Mathf.Clamp01(blend)), -1f, 1f);
    }

    private static float GetDictionaryValue(Dictionary<string, float> map, string key, float fallback)
    {
        return !string.IsNullOrWhiteSpace(key) && map.TryGetValue(key, out float value)
            ? value
            : fallback;
    }

    private static void SyncDebugList(Dictionary<string, float> source, List<SocialMemoryFloat> target)
    {
        target.Clear();
        foreach (KeyValuePair<string, float> entry in source)
        {
            target.Add(new SocialMemoryFloat(entry.Key, entry.Value));
        }
    }
}

public static class SocialRumorUtility
{
    public static IEnumerable<string> GetFacilityKeys(SocialRumor rumor)
    {
        if (rumor == null)
        {
            yield break;
        }

        if (rumor.targetFacilityId >= 0)
        {
            yield return $"facility:{rumor.targetFacilityId}";
        }

        if (!string.IsNullOrWhiteSpace(rumor.targetFacilityTag))
        {
            yield return $"tag:{Normalize(rumor.targetFacilityTag)}";
        }
    }

    public static IEnumerable<string> GetCharacterKeys(SocialRumor rumor)
    {
        if (rumor == null)
        {
            yield break;
        }

        if (rumor.targetCharacterId >= 0)
        {
            yield return $"actor:{rumor.targetCharacterId}";
        }

        if (!string.IsNullOrWhiteSpace(rumor.targetCharacterName))
        {
            yield return $"actor-name:{Normalize(rumor.targetCharacterName)}";
        }
    }

    public static string GetActorKey(CharacterActor actor)
    {
        if (actor == null)
        {
            return string.Empty;
        }

        int id = actor.Identity != null ? actor.Identity.StableId : actor.GetInstanceID();
        return $"actor:{id}";
    }

    public static string GetActorNameKey(CharacterActor actor)
    {
        string displayName = actor != null && actor.Identity != null
            ? actor.Identity.DisplayName
            : actor != null ? actor.name : string.Empty;
        return string.IsNullOrWhiteSpace(displayName)
            ? string.Empty
            : $"actor-name:{Normalize(displayName)}";
    }

    public static bool MatchesFacilityKey(BuildableObject building, string key)
    {
        if (building == null || string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (key.StartsWith("facility:", StringComparison.Ordinal))
        {
            return key == $"facility:{building.id}";
        }

        if (!key.StartsWith("tag:", StringComparison.Ordinal))
        {
            return false;
        }

        string tag = key.Substring("tag:".Length);
        return MatchesFacilityTag(building, tag);
    }

    public static bool MatchesFacilityTag(BuildableObject building, string tag)
    {
        if (building == null || string.IsNullOrWhiteSpace(tag))
        {
            return false;
        }

        string normalizedTag = Normalize(tag);
        string objectName = building.BuildingData != null ? building.BuildingData.objectName : string.Empty;
        string roleName = building.Facility != null ? building.Facility.roles.ToString() : string.Empty;
        string objectRuntimeName = building.name ?? string.Empty;
        return ContainsNormalized(objectName, normalizedTag)
            || ContainsNormalized(roleName, normalizedTag)
            || ContainsNormalized(objectRuntimeName, normalizedTag);
    }

    public static string GetActorLabel(CharacterActor actor)
    {
        if (actor == null)
        {
            return "Unknown";
        }

        return actor.Identity != null ? actor.Identity.DisplayName : actor.name;
    }

    public static string GetFacilityLabel(BuildableObject building)
    {
        if (building == null)
        {
            return "none";
        }

        if (building.BuildingData != null && !string.IsNullOrWhiteSpace(building.BuildingData.objectName))
        {
            return building.BuildingData.objectName;
        }

        return building.name;
    }

    public static string GetFacilityTag(BuildableObject building)
    {
        if (building == null)
        {
            return string.Empty;
        }

        if (building.Facility != null && building.Facility.roles != FacilityRole.None)
        {
            return building.Facility.roles.ToString();
        }

        return building.BuildingData != null ? building.BuildingData.objectName : building.name;
    }

    private static bool ContainsNormalized(string source, string normalizedNeedle)
    {
        return !string.IsNullOrWhiteSpace(source)
            && Normalize(source).IndexOf(normalizedNeedle, StringComparison.Ordinal) >= 0;
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }
}
