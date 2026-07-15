using System.Collections;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

[DisallowMultipleComponent]
public class CharacterStats : SerializedMonoBehaviour
{
    private static readonly IOwnerRunLifecycleService FallbackOwnerRunLifecycleService =
        new CharacterStatsNoopOwnerRunLifecycleService();
    private static readonly IMetaProgressionRuntimeReader FallbackMetaProgressionRuntimeReader =
        new CharacterStatsDefaultMetaProgressionReader();
    private static readonly IStaffDiscontentRuntimeService FallbackStaffDiscontentRuntimeService =
        new CharacterStatsNoopStaffDiscontentRuntimeService();

    [SerializeField]
    [ReadOnly]
    private CharacterActor actor;
    [SerializeField]
    [ReadOnly]
    private CharacterIdentity identity;
    [SerializeField]
    [ReadOnly]
    private CharacterVisual visual;
    [SerializeField]
    [ReadOnly]
    private CharacterLifecycle lifecycle;
    [SerializeField]
    [ReadOnly]
    private CharacterLog log;
    [SerializeField]
    private Dictionary<CharacterCondition, float> stats;
    [SerializeField]
    [ReadOnly]
    private float maxHealth = 100f;
    [SerializeField]
    [ReadOnly]
    private float currentHealth = 100f;
    [SerializeField]
    [ReadOnly]
    [Range(0f, 1f)]
    private float injurySeverity;
    [SerializeField, Range(0f, 100f)]
    private float baseMood = CharacterMoodRules.DefaultBaseMood;
    [SerializeField, ReadOnly]
    private List<CharacterMoodMemory> interactionMoodFactors = new List<CharacterMoodMemory>();
    private float lastCalculatedMood = float.NaN;
    private float nextMoodExpiryCheckAt;
    private IStaffDiscontentRuntimeService staffDiscontentRuntimeService;
    private IOwnerRunLifecycleService ownerRunLifecycleService;
    private IMetaProgressionRuntimeReader metaProgressionRuntimeReader;

    public Dictionary<CharacterCondition, float> Stats
    {
        get
        {
            EnsureStats();
            return stats;
        }
        set
        {
            stats = value;
            EnsureStats();
            AdoptAssignedMoodAsBase();
            RecalculateMood(notify: false, forceNotify: false, adoptExternalOverride: false);
            OnStatChange?.Invoke(stats);
        }
    }

    public bool IsDead => currentHealth <= 0f;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float InjurySeverity => injurySeverity;
    public float Mood => GetMoodSnapshot().Value;
    public event Action<Dictionary<CharacterCondition, float>> OnStatChange;
    public event Action<CharacterMoodSnapshot> OnMoodChange;

    private void Awake()
    {
        Bind(GetComponent<CharacterActor>());
    }

    [Inject]
    public void ConstructCharacterStats(
        IStaffDiscontentRuntimeService staffDiscontentRuntimeService,
        IOwnerRunLifecycleService ownerRunLifecycleService,
        IMetaProgressionRuntimeReader metaProgressionRuntimeReader)
    {
        this.staffDiscontentRuntimeService = staffDiscontentRuntimeService
            ?? throw new ArgumentNullException(nameof(staffDiscontentRuntimeService));
        this.ownerRunLifecycleService = ownerRunLifecycleService
            ?? throw new ArgumentNullException(nameof(ownerRunLifecycleService));
        this.metaProgressionRuntimeReader = metaProgressionRuntimeReader
            ?? throw new ArgumentNullException(nameof(metaProgressionRuntimeReader));
    }

    public void Bind(CharacterActor owner)
    {
        actor = owner;
        identity = GetComponent<CharacterIdentity>();
        visual = GetComponent<CharacterVisual>();
        lifecycle = GetComponent<CharacterLifecycle>();
        log = GetComponent<CharacterLog>();
        EnsureStats();
        lastCalculatedMood = float.NaN;
        RecalculateMood(notify: false, forceNotify: false, adoptExternalOverride: false);
    }

    private void Update()
    {
        if (interactionMoodFactors == null
            || interactionMoodFactors.Count == 0
            || Time.time < nextMoodExpiryCheckAt)
        {
            return;
        }

        nextMoodExpiryCheckAt = Time.time + 0.25f;
        RecalculateMood(notify: true, forceNotify: false, adoptExternalOverride: true);
    }

    public IEnumerator ChangeStatByTick()
    {
        while (true)
        {
            float hungerMultiplier = actor != null && actor.PersonaRuntime != null
                ? actor.PersonaRuntime.GetConditionCurveMultiplier(CharacterCondition.HUNGER)
                : 1f;
            float excretionMultiplier = actor != null && actor.PersonaRuntime != null
                ? actor.PersonaRuntime.GetConditionCurveMultiplier(CharacterCondition.EXCRETION)
                : 1f;
            float hygieneMultiplier = actor != null && actor.PersonaRuntime != null
                ? actor.PersonaRuntime.GetConditionCurveMultiplier(CharacterCondition.HYGIENE)
                : 1f;
            ChangesStat(CharacterCondition.HUNGER, -5f * hungerMultiplier);
            ChangesStat(CharacterCondition.EXCRETION, -3f * excretionMultiplier);
            ChangesStat(CharacterCondition.HYGIENE, -1.5f * hygieneMultiplier);
            yield return new WaitForSeconds(5f);
        }
    }

    public void ChangesStat(CharacterCondition condition, float value)
    {
        EnsureStats();
        if (condition == CharacterCondition.MOOD)
        {
            ApplyMoodFactor(
                "legacy:mood-adjustment",
                value >= 0f ? "최근 좋은 경험" : "최근 불편한 경험",
                value,
                120f,
                4);
            return;
        }

        SynchronizeExternalMoodOverride();
        stats[condition] = Mathf.Clamp(stats[condition] + value, 0, 100);
        RecalculateMood(notify: false, forceNotify: false, adoptExternalOverride: false);
        OnStatChange?.Invoke(stats);
        OnMoodChange?.Invoke(BuildMoodSnapshot(Time.time));
    }

    public void ApplyMoodFactor(
        string id,
        string label,
        float value,
        float durationSeconds = 180f,
        int maxStacks = 1)
    {
        if (string.IsNullOrWhiteSpace(id)
            || string.IsNullOrWhiteSpace(label)
            || Mathf.Approximately(value, 0f))
        {
            return;
        }

        EnsureStats();
        SynchronizeExternalMoodOverride();
        float now = Time.time;
        PruneExpiredMoodFactors(now);
        CharacterMoodMemory factor = interactionMoodFactors.Find(item => item != null && item.Id == id);
        if (factor == null)
        {
            factor = new CharacterMoodMemory(id, label, value, durationSeconds, maxStacks, now);
            interactionMoodFactors.Add(factor);
        }
        else
        {
            factor.Apply(label, value, durationSeconds, maxStacks, now);
        }

        nextMoodExpiryCheckAt = now + 0.25f;
        RecalculateMood(notify: true, forceNotify: true, adoptExternalOverride: false);
    }

    public bool RemoveMoodFactor(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || interactionMoodFactors == null)
        {
            return false;
        }

        EnsureStats();
        SynchronizeExternalMoodOverride();
        int removed = interactionMoodFactors.RemoveAll(item => item != null && item.Id == id);
        if (removed > 0)
        {
            RecalculateMood(notify: true, forceNotify: true, adoptExternalOverride: false);
        }

        return removed > 0;
    }

    public CharacterMoodSnapshot GetMoodSnapshot()
    {
        EnsureStats();
        SynchronizeExternalMoodOverride();
        RecalculateMood(notify: false, forceNotify: false, adoptExternalOverride: false);
        return BuildMoodSnapshot(Time.time);
    }

    public int GetCharacterStat(CharacterStatType statType)
    {
        return identity != null && identity.Profile != null ? identity.Profile.GetStat(statType) : 5;
    }

    public float GetMoveSpeed()
    {
        float baseSpeed = identity != null && identity.Data != null ? identity.Data.moveSpeed : 1f;
        return baseSpeed
            * (identity != null && identity.Profile != null ? identity.Profile.GetMoveSpeedMultiplier() : 1f)
            * GetFatigueEfficiencyMultiplier()
            * GetInjuryEfficiencyMultiplier();
    }

    public float GetConsumptionMultiplier()
    {
        return identity != null && identity.Profile != null ? identity.Profile.GetConsumptionMultiplier() : 1f;
    }

    public float GetStayDurationMultiplier()
    {
        return identity != null && identity.Profile != null ? identity.Profile.GetStayDurationMultiplier() : 1f;
    }

    public float GetCrowdSensitivityMultiplier()
    {
        return identity != null && identity.Profile != null ? identity.Profile.GetCrowdSensitivityMultiplier() : 1f;
    }

    public float GetWorkSpeedMultiplier(FacilityWorkType workTypes)
    {
        IStaffDiscontentRuntimeService discontentService = staffDiscontentRuntimeService
            ?? FallbackStaffDiscontentRuntimeService;
        float discontentMultiplier = actor != null
            ? discontentService.GetWorkEfficiencyMultiplier(actor)
            : 1f;
        return (identity != null && identity.Profile != null ? identity.Profile.GetWorkSpeedMultiplier(workTypes) : 1f)
            * GetFatigueEfficiencyMultiplier()
            * GetInjuryEfficiencyMultiplier()
            * discontentMultiplier;
    }

    public float GetWorkPreferenceScore(FacilityWorkType workTypes)
    {
        return identity != null && identity.Profile != null ? identity.Profile.GetWorkPreferenceScore(workTypes) : 0.5f;
    }

    public float GetFacilityPreferenceScore(FacilityRole roles)
    {
        return identity != null && identity.Profile != null ? identity.Profile.GetFacilityPreferenceScore(roles) : 0.5f;
    }

    public float GetAccidentChanceMultiplier()
    {
        return identity != null && identity.Profile != null ? identity.Profile.GetAccidentChanceMultiplier() : 1f;
    }

    public CharacterSpeciesIncidentType GetIncidentType()
    {
        return identity != null && identity.Profile != null ? identity.Profile.GetIncidentType() : CharacterSpeciesIncidentType.None;
    }

    public float GetCombatPowerMultiplier()
    {
        return (identity != null && identity.Profile != null ? identity.Profile.GetCombatPowerMultiplier() : 1f)
            * GetInjuryEfficiencyMultiplier();
    }

    public float GetFatigueEfficiencyMultiplier()
    {
        EnsureStats();
        if (!stats.TryGetValue(CharacterCondition.SLEEP, out float sleep))
        {
            return 1f;
        }

        return Mathf.Lerp(0.65f, 1f, Mathf.Clamp01(sleep / 100f));
    }

    public float GetInjuryEfficiencyMultiplier()
    {
        return Mathf.Lerp(1f, 0.45f, Mathf.Clamp01(injurySeverity));
    }

    public void ApplyDamage(float amount, string reason = "")
    {
        if (amount <= 0f || IsDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        injurySeverity = Mathf.Clamp01(1f - (currentHealth / Mathf.Max(1f, maxHealth)));
        ApplyMoodFactor(
            "health:injury",
            "몸을 다침",
            -Mathf.Clamp(amount * 0.25f, 2f, 10f),
            180f,
            2);
        log?.AddLog(string.IsNullOrWhiteSpace(reason)
            ? $"피해 {amount:0.#}"
            : $"피해 {amount:0.#}: {reason}");

        if (currentHealth <= 0f)
        {
            Die(reason);
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        injurySeverity = Mathf.Clamp01(1f - (currentHealth / Mathf.Max(1f, maxHealth)));
        ApplyMoodFactor(
            "health:relief",
            "치료받아 안도함",
            Mathf.Clamp(amount * 0.15f, 1f, 6f),
            120f,
            1);
        log?.AddLog($"회복 {amount:0.#}");
    }

    public void SetInjurySeverity(float value)
    {
        injurySeverity = Mathf.Clamp01(value);
        currentHealth = Mathf.Clamp(maxHealth * (1f - injurySeverity), 1f, maxHealth);
        log?.AddLog($"부상도 변경: {Mathf.RoundToInt(injurySeverity * 100f)}%");
    }

    public void Die(string reason = "")
    {
        if (lifecycle != null && lifecycle.CurrentState == CharacterLifecycleState.Despawned) return;

        visual?.SetRenderersVisible(true);
        currentHealth = 0f;
        injurySeverity = 1f;
        log?.AddLog(string.IsNullOrWhiteSpace(reason) ? "사망" : $"사망: {reason}");
        lifecycle?.SetLifecycleState(CharacterLifecycleState.Despawned);

        CharacterDeathEvent.Trigger(actor, reason);

        if (identity != null && identity.IsOwner && actor != null)
        {
            (ownerRunLifecycleService ?? FallbackOwnerRunLifecycleService).HandleOwnerDeath(actor, reason);
        }
    }

    public void RecalculateVitals(bool resetCurrentHealth)
    {
        int toughness = GetCharacterStat(CharacterStatType.Toughness);
        int endurance = GetCharacterStat(CharacterStatType.Endurance);
        maxHealth = 60f + (toughness * 8f) + (endurance * 4f);
        if (identity != null && identity.IsOwner)
        {
            maxHealth *= ResolveMetaProgressionRuntimeReader().GetOwnerMaxHealthMultiplier();
        }

        if (resetCurrentHealth || currentHealth <= 0f)
        {
            currentHealth = maxHealth;
            injurySeverity = 0f;
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth, 1f, maxHealth);
            injurySeverity = Mathf.Clamp01(1f - (currentHealth / Mathf.Max(1f, maxHealth)));
        }
    }

    private void EnsureStats()
    {
        stats ??= new Dictionary<CharacterCondition, float>();
        interactionMoodFactors ??= new List<CharacterMoodMemory>();
        EnsureStat(CharacterCondition.SLEEP, 100f);
        EnsureStat(CharacterCondition.HUNGER, 100f);
        EnsureStat(CharacterCondition.FUN, 100f);
        EnsureStat(CharacterCondition.MOOD, baseMood);
        EnsureStat(CharacterCondition.EXCRETION, 100f);
        EnsureStat(CharacterCondition.HYGIENE, 100f);
    }

    private void AdoptAssignedMoodAsBase()
    {
        float requestedMood = stats.TryGetValue(CharacterCondition.MOOD, out float assigned)
            ? Mathf.Clamp(assigned, 0f, 100f)
            : CharacterMoodRules.DefaultBaseMood;
        float factorTotal = CalculateFactorTotal(BuildMoodSnapshot(Time.time).Factors);
        baseMood = Mathf.Clamp(requestedMood - factorTotal, 0f, 100f);
        lastCalculatedMood = requestedMood;
    }

    private void SynchronizeExternalMoodOverride()
    {
        if (float.IsNaN(lastCalculatedMood)
            || !stats.TryGetValue(CharacterCondition.MOOD, out float currentMood)
            || Mathf.Approximately(currentMood, lastCalculatedMood))
        {
            return;
        }

        float factorTotal = CalculateFactorTotal(BuildMoodSnapshot(Time.time).Factors);
        baseMood = Mathf.Clamp(currentMood - factorTotal, 0f, 100f);
        lastCalculatedMood = Mathf.Clamp(currentMood, 0f, 100f);
    }

    private void RecalculateMood(
        bool notify,
        bool forceNotify,
        bool adoptExternalOverride)
    {
        EnsureStats();
        if (adoptExternalOverride)
        {
            SynchronizeExternalMoodOverride();
        }

        float now = Time.time;
        bool expired = PruneExpiredMoodFactors(now);
        CharacterMoodSnapshot snapshot = BuildMoodSnapshot(now);
        float previous = stats.TryGetValue(CharacterCondition.MOOD, out float current)
            ? current
            : snapshot.Value;
        stats[CharacterCondition.MOOD] = snapshot.Value;
        lastCalculatedMood = snapshot.Value;

        if (notify && (forceNotify || expired || !Mathf.Approximately(previous, snapshot.Value)))
        {
            OnStatChange?.Invoke(stats);
            OnMoodChange?.Invoke(snapshot);
        }
    }

    private bool PruneExpiredMoodFactors(float now)
    {
        if (interactionMoodFactors == null)
        {
            return false;
        }

        return interactionMoodFactors.RemoveAll(item => item == null || item.IsExpired(now)) > 0;
    }

    private CharacterMoodSnapshot BuildMoodSnapshot(float now)
    {
        List<CharacterMoodFactorSnapshot> factors = CharacterMoodRules.BuildNeedFactors(stats);
        if (interactionMoodFactors != null)
        {
            foreach (CharacterMoodMemory factor in interactionMoodFactors)
            {
                if (factor != null && !factor.IsExpired(now))
                {
                    factors.Add(factor.CreateSnapshot(now));
                }
            }
        }

        float mood = Mathf.Clamp(baseMood + CalculateFactorTotal(factors), 0f, 100f);
        return new CharacterMoodSnapshot(mood, baseMood, factors);
    }

    private static float CalculateFactorTotal(IReadOnlyList<CharacterMoodFactorSnapshot> factors)
    {
        float total = 0f;
        if (factors == null)
        {
            return total;
        }

        for (int i = 0; i < factors.Count; i++)
        {
            if (factors[i] != null)
            {
                total += factors[i].Value;
            }
        }

        return total;
    }

    private IMetaProgressionRuntimeReader ResolveMetaProgressionRuntimeReader()
    {
        return metaProgressionRuntimeReader ?? FallbackMetaProgressionRuntimeReader;
    }

    private void EnsureStat(CharacterCondition condition, float defaultValue)
    {
        if (!stats.ContainsKey(condition))
        {
            stats[condition] = defaultValue;
        }
    }

    private sealed class CharacterStatsNoopOwnerRunLifecycleService : IOwnerRunLifecycleService
    {
        public void HandleOwnerDeath(CharacterActor owner, string reason) { }
    }

    private sealed class CharacterStatsNoopStaffDiscontentRuntimeService : IStaffDiscontentRuntimeService
    {
        public float GetWorkEfficiencyMultiplier(CharacterActor staff) => 1f;

        public bool ShouldBlockWork(CharacterActor staff, out string reason)
        {
            reason = string.Empty;
            return false;
        }

        public bool IsRebellionTarget(CharacterActor target) => false;
        public bool ResolveSuppressedRebel(CharacterActor rebel, CharacterActor defender) => false;
    }

    private sealed class CharacterStatsDefaultMetaProgressionReader : IMetaProgressionRuntimeReader
    {
        public int GetStartingFacilityCandidateBonus() => 0;
        public int GetStartingOwnerTraitCandidateBonus() => 0;
        public float GetOwnerMaxHealthMultiplier() => 1f;
        public float GetInvasionWarningThresholdMultiplier() => 1f;
        public bool IsRecipePreserved(string recipeId) => false;

        public IReadOnlyCollection<int> GetExpandedBasicPurchaseBuildingIds(IEnumerable<BuildingSO> buildings)
        {
            return Array.Empty<int>();
        }
    }
}
