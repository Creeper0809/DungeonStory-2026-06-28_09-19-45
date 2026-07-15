using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class OffenseWorldMapService
{
    public const int MaxReconLevel = 3;

    public static float GetScanRange(int reconLevel)
    {
        return Mathf.Max(0, reconLevel) switch
        {
            0 => 8f,
            1 => 14f,
            2 => 22f,
            _ => 999f
        };
    }

    public static IReadOnlyList<OffenseTargetDefinition> CreateDefaultTargets()
    {
        return new[]
        {
            CreateTarget(
                "food_farm",
                "외곽 식재료 농장",
                "던전 근처의 작은 보급지입니다. 초반 식재료 수급을 노리기 쉽습니다.",
                OffenseTargetKind.ResourceSite,
                4f,
                8f,
                55f,
                1,
                10f,
                Reward(OffenseRewardCategory.Stock, "식재료", 40),
                Reward(OffenseRewardCategory.Money, "약탈금", 80)),
            CreateTarget(
                "merchant_road",
                "상단 길목",
                "인간 상단이 자주 지나는 길목입니다. 잡화와 돈을 노릴 수 있습니다.",
                OffenseTargetKind.HumanOutpost,
                6f,
                14f,
                70f,
                1,
                16f,
                Reward(OffenseRewardCategory.Stock, "잡화", 30),
                Reward(OffenseRewardCategory.Money, "약탈금", 120)),
            CreateTarget(
                "old_armory",
                "낡은 무기고",
                "방치된 무기고입니다. 방어 시설 합성 재료와 무기 재고를 기대할 수 있습니다.",
                OffenseTargetKind.HumanOutpost,
                12f,
                28f,
                100f,
                2,
                32f,
                Reward(OffenseRewardCategory.Stock, "무기", 25),
                Reward(OffenseRewardCategory.Blueprint, "방어 설계도", 1)),
            CreateTarget(
                "mana_ruins",
                "마력 유적",
                "마력 흔적이 강한 폐허입니다. 위험하지만 연구와 합성 보상이 좋습니다.",
                OffenseTargetKind.SpecialEvent,
                18f,
                38f,
                130f,
                2,
                42f,
                Reward(OffenseRewardCategory.Stock, "마력", 35),
                Reward(OffenseRewardCategory.Blueprint, "특수 설계도", 1)),
            CreateTarget(
                "rival_dungeon",
                "경쟁 던전 전초기지",
                "다른 몬스터 세력의 전초기지입니다. 성공하면 희귀 시설과 세력 약화를 기대할 수 있습니다.",
                OffenseTargetKind.RivalDungeon,
                26f,
                55f,
                180f,
                3,
                60f,
                Reward(OffenseRewardCategory.RareFacility, "희귀 시설", 1),
                Reward(OffenseRewardCategory.FactionWeakening, "인간 세력 약화", 1),
                Reward(OffenseRewardCategory.RecruitCandidate, "직원 후보", 1),
                Reward(OffenseRewardCategory.Prisoner, "특수 몬스터", 1))
        };
    }

    public static IReadOnlyList<OffenseTargetDefinition> NormalizeTargets(IEnumerable<OffenseTargetDefinition> targets)
    {
        List<OffenseTargetDefinition> source = targets?
            .Where((target) => target != null && target.IsValid)
            .OrderBy((target) => target.distance)
            .ThenBy((target) => target.id)
            .ToList()
            ?? new List<OffenseTargetDefinition>();

        return source.Count > 0 ? source : CreateDefaultTargets();
    }

    public static int RevealTargetsInRange(
        OffenseWorldMapState state,
        IEnumerable<OffenseTargetDefinition> targets)
    {
        if (state == null)
        {
            return 0;
        }

        float scanRange = GetScanRange(state.ReconLevel);
        int added = 0;
        foreach (OffenseTargetDefinition target in NormalizeTargets(targets))
        {
            if (target.distance <= scanRange && state.AddKnownTarget(target.id))
            {
                added++;
            }
        }

        return added;
    }

    public static IReadOnlyList<OffenseTargetSnapshot> GetVisibleTargetSnapshots(
        OffenseWorldMapState state,
        IEnumerable<OffenseTargetDefinition> targets,
        bool preciseIntel)
    {
        if (state == null)
        {
            return Array.Empty<OffenseTargetSnapshot>();
        }

        return NormalizeTargets(targets)
            .Where((target) => state.KnowTarget(target.id))
            .Select((target) => target.ToSnapshot(preciseIntel))
            .ToList();
    }

    public static OffenseTargetDefinition FindKnownTarget(
        OffenseWorldMapState state,
        IEnumerable<OffenseTargetDefinition> targets,
        string targetId)
    {
        if (state == null || string.IsNullOrWhiteSpace(targetId) || !state.KnowTarget(targetId))
        {
            return null;
        }

        return NormalizeTargets(targets).FirstOrDefault((target) => target.id == targetId);
    }

    private static OffenseTargetDefinition CreateTarget(
        string id,
        string title,
        string description,
        OffenseTargetKind kind,
        float distance,
        float danger,
        float durationSeconds,
        int requiredMembers,
        float requiredPower,
        params OffenseRewardPreview[] rewards)
    {
        return new OffenseTargetDefinition
        {
            id = id,
            title = title,
            description = description,
            kind = kind,
            distance = distance,
            danger = danger,
            durationSeconds = durationSeconds,
            requiredMembers = requiredMembers,
            requiredPower = requiredPower,
            rewards = rewards ?? Array.Empty<OffenseRewardPreview>()
        };
    }

    private static OffenseRewardPreview Reward(OffenseRewardCategory category, string label, int amount)
    {
        return new OffenseRewardPreview
        {
            category = category,
            label = label,
            amount = amount
        };
    }
}
