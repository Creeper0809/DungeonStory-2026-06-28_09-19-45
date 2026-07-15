using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class OffenseExpeditionService
{
    public static bool CanJoinExpedition(CharacterActor actor, out string reason)
    {
        if (actor == null)
        {
            reason = "캐릭터 없음";
            return false;
        }

        actor.EnsureRuntimeState();
        CharacterIdentity identity = actor.Identity;
        CharacterStats stats = actor.Stats;
        CharacterLifecycle lifecycle = actor.Lifecycle;
        CharacterAbilityCache abilityCache = actor.AbilityCache;

        if (identity != null && identity.IsOwner)
        {
            reason = "사장은 원정에 보낼 수 없습니다";
            return false;
        }

        if (stats != null && stats.IsDead)
        {
            reason = "사망한 캐릭터입니다";
            return false;
        }

        if (lifecycle != null && lifecycle.CurrentState == CharacterLifecycleState.OnExpedition)
        {
            reason = "이미 원정 중입니다";
            return false;
        }

        if (lifecycle == null || lifecycle.CurrentState != CharacterLifecycleState.Active)
        {
            reason = "현재 던전에서 활동 중인 캐릭터가 아닙니다";
            return false;
        }

        if (identity == null || identity.CharacterType != CharacterType.NPC)
        {
            reason = "직원이나 방어 몬스터만 원정에 보낼 수 있습니다";
            return false;
        }

        if (abilityCache == null || !abilityCache.TryGetAbility(out AbilityWork _))
        {
            reason = "원정 가능한 작업/전투 능력이 없습니다";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public static float CalculateMemberPower(CharacterActor actor)
    {
        actor?.EnsureRuntimeState();
        CharacterStats stats = actor != null ? actor.Stats : null;
        if (actor == null || stats == null || stats.IsDead)
        {
            return 0f;
        }

        float basePower =
            stats.GetCharacterStat(CharacterStatType.Attack) * 1.4f
            + stats.GetCharacterStat(CharacterStatType.Strength) * 0.8f
            + stats.GetCharacterStat(CharacterStatType.Toughness) * 0.6f
            + stats.GetCharacterStat(CharacterStatType.Endurance) * 0.4f
            + stats.GetCharacterStat(CharacterStatType.MoveSpeed) * 0.25f;
        return Mathf.Max(0f, basePower * stats.GetCombatPowerMultiplier());
    }

    public static float CalculatePartyPower(IEnumerable<CharacterActor> members)
    {
        return members?.Where((member) => member != null).Sum(CalculateMemberPower) ?? 0f;
    }

    public static bool ShouldSucceed(OffenseExpeditionRun expedition)
    {
        if (expedition == null || expedition.Target == null)
        {
            return false;
        }

        float requiredPower = Mathf.Max(1f, expedition.Target.requiredPower);
        return expedition.TotalPower >= requiredPower;
    }

    public static OffenseExpeditionResult Resolve(
        OffenseExpeditionRun expedition,
        bool? forceSuccess = null)
    {
        if (expedition == null || expedition.Target == null)
        {
            return null;
        }

        bool success = forceSuccess ?? ShouldSucceed(expedition);
        float requiredPower = Mathf.Max(1f, expedition.Target.requiredPower);
        float powerRatio = expedition.TotalPower / requiredPower;
        float danger = Mathf.Max(0f, expedition.Target.danger);
        int memberCount = Mathf.Max(1, expedition.MemberActors.Count);
        float damageMultiplier = success ? 0.16f : Mathf.Lerp(0.75f, 1.35f, Mathf.Clamp01(1f - powerRatio));
        float damagePerMember = danger * damageMultiplier / memberCount;
        List<OffenseExpeditionMemberSnapshot> memberSnapshots = new List<OffenseExpeditionMemberSnapshot>();

        foreach (CharacterActor member in expedition.MemberActors)
        {
            if (member == null)
            {
                continue;
            }

            float memberPower = CalculateMemberPower(member);
            member.EndExpedition(alive: true);
            CharacterStats stats = member.Stats;
            stats?.ChangesStat(CharacterCondition.SLEEP, success ? -12f : -24f);
            stats?.ApplyMoodFactor(
                success ? "expedition:tension" : "expedition:failure",
                success ? "원정의 긴장" : "원정 실패",
                success ? -4f : -12f,
                240f,
                1);
            if (damagePerMember > 0f)
            {
                stats?.ApplyDamage(damagePerMember, "원정 피해");
            }

            CharacterIdentity identity = member.Identity;
            memberSnapshots.Add(new OffenseExpeditionMemberSnapshot
            {
                name = identity != null ? identity.DisplayName : member.name,
                speciesTag = identity != null ? identity.SpeciesTag : string.Empty,
                power = memberPower,
                survived = stats == null || !stats.IsDead,
                damageTaken = damagePerMember
            });
        }

        string[] rewards = success
            ? expedition.Target.rewards?
                .Where((reward) => reward != null)
                .Select((reward) => reward.ToSummaryText())
                .ToArray() ?? Array.Empty<string>()
            : Array.Empty<string>();

        return new OffenseExpeditionResult
        {
            expeditionId = expedition.ExpeditionId,
            targetId = expedition.Target.id,
            targetTitle = expedition.Target.title,
            success = success,
            totalPower = expedition.TotalPower,
            requiredPower = requiredPower,
            danger = danger,
            elapsedSeconds = expedition.TotalDurationSeconds,
            members = memberSnapshots.ToArray(),
            rewardSummaries = rewards
        };
    }
}
