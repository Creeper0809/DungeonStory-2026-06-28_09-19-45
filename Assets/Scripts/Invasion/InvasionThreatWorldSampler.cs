using System.Collections.Generic;
using UnityEngine;

public interface IInvasionThreatWorldSampler
{
    InvasionThreatFactors Sample(float secondsSinceLastInvasion);
}

public sealed class InvasionThreatWorldSampler : IInvasionThreatWorldSampler
{
    private readonly IDungeonSceneComponentQuery sceneQuery;

    public InvasionThreatWorldSampler(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery
            ?? throw new System.ArgumentNullException(nameof(sceneQuery));
    }

    public InvasionThreatFactors Sample(float secondsSinceLastInvasion)
    {
        IReadOnlyList<BuildableObject> buildings = sceneQuery.All<BuildableObject>();
        IReadOnlyList<CharacterActor> characters = sceneQuery.All<CharacterActor>();

        float dungeonValue = CalculateDungeonValue(buildings);
        float reputation = CalculateReputation(characters);
        float time = Mathf.Clamp(secondsSinceLastInvasion / 180f, 0f, 10f);
        float risk = CalculateRisk(buildings);
        return new InvasionThreatFactors(dungeonValue, reputation, time, risk);
    }

    private static float CalculateDungeonValue(IEnumerable<BuildableObject> buildings)
    {
        if (buildings == null)
        {
            return 0f;
        }

        float value = 0f;
        foreach (BuildableObject building in buildings)
        {
            if (building == null || building.isDestroy)
            {
                continue;
            }

            value += 1f;
            if (building.BuildingData != null)
            {
                value += Mathf.Max(0, building.BuildingData.maintenance) / 100f;
                if (building.BuildingData.Facility != null && building.BuildingData.Facility.roles != FacilityRole.None)
                {
                    value += 0.5f;
                }
            }
        }

        return value;
    }

    private static float CalculateReputation(IEnumerable<CharacterActor> characters)
    {
        if (characters == null)
        {
            return 0f;
        }

        int customers = 0;
        float mood = 0f;
        foreach (CharacterActor character in characters)
        {
            CharacterIdentity identity = character != null ? character.Identity : null;
            if (identity == null || identity.CharacterType != CharacterType.Customer)
            {
                continue;
            }

            customers++;
            CharacterStats stats = character.Stats;
            if (stats != null && stats.Stats.TryGetValue(CharacterCondition.MOOD, out float sample))
            {
                mood += Mathf.Clamp(sample, 0f, 100f) / 100f;
            }
        }

        if (customers == 0)
        {
            return 0f;
        }

        return customers + (mood / Mathf.Max(1, customers));
    }

    private static float CalculateRisk(IEnumerable<BuildableObject> buildings)
    {
        if (buildings == null)
        {
            return 0f;
        }

        float risk = 0f;
        foreach (BuildableObject building in buildings)
        {
            if (building == null || building.isDestroy)
            {
                continue;
            }

            if (building.IsDamaged)
            {
                risk += 1.5f;
            }

            if (building is Shop shop && shop.Facility != null)
            {
                risk += FacilityCrimeRiskUtility.CalculateOperationalRisk(new FacilityCrimeRiskContext(
                    shop.Facility,
                    actor: null,
                    shop.HasServingWorker,
                    shop.HasWaitingCheckout,
                    shop.CurrentUserCount,
                    cartItemCount: 1,
                    cartValue: 0,
                    shop.CurrentStock,
                    shop.IsDamaged));

                if (shop.CurrentStock <= shop.Facility.restockRequestThreshold)
                {
                    risk += 0.5f;
                }
            }
        }

        return risk;
    }
}
