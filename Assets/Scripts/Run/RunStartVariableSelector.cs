using System;
using System.Linq;
using UnityEngine;

public interface IRunStartVariableSelector
{
    RunStartVariableSnapshot Create(
        int seed,
        CharacterSO ownerData,
        InvasionThreatDifficulty difficulty);
}

public sealed class RunStartVariableSelector : IRunStartVariableSelector
{
    private readonly IRunStartVariableCatalog catalog;
    private readonly IMetaProgressionRuntimeReader metaProgressionReader;

    public RunStartVariableSelector(
        IRunStartVariableCatalog catalog,
        IMetaProgressionRuntimeReader metaProgressionReader)
    {
        this.catalog = catalog
            ?? throw new ArgumentNullException(nameof(catalog));
        this.metaProgressionReader = metaProgressionReader
            ?? throw new ArgumentNullException(nameof(metaProgressionReader));
    }

    public RunStartVariableSnapshot Create(
        int seed,
        CharacterSO ownerData,
        InvasionThreatDifficulty difficulty)
    {
        System.Random startRandom = new System.Random(seed);
        int startingFacilityCount = 3 + metaProgressionReader.GetStartingFacilityCandidateBonus();
        BuildingSO[] buildings = catalog
            .Buildings
            .Where((building) => building != null
                && !building.IsGridMovement
                && !building.IsWall
                && FacilityShopService.GetBuildingStar(building) <= 1)
            .OrderBy((_) => startRandom.NextDouble())
            .Take(Mathf.Max(1, startingFacilityCount))
            .ToArray();
        CharacterSO[] customers = catalog
            .Characters
            .Where((character) => character != null && character.characterType == CharacterType.Customer)
            .OrderBy((_) => startRandom.NextDouble())
            .Take(3)
            .ToArray();
        FacilityBlueprintSO[] blueprints = catalog
            .Blueprints
            .Where((blueprint) => blueprint != null)
            .OrderBy((_) => startRandom.NextDouble())
            .Take(2)
            .ToArray();

        return new RunStartVariableSnapshot
        {
            seed = seed,
            ownerSpeciesTag = !string.IsNullOrWhiteSpace(ownerData?.SpeciesTag) ? ownerData.SpeciesTag : "Unknown",
            difficulty = difficulty,
            startingFacilityCandidateIds = buildings.Select((building) => building.id).DefaultIfEmpty(-1).Where((id) => id >= 0).ToArray(),
            startingGuestSpeciesCandidates = customers.Select((customer) => customer.SpeciesTag).Where((tag) => !string.IsNullOrWhiteSpace(tag)).Distinct().ToArray(),
            startingBlueprintCandidateIds = blueprints.Select((blueprint) => blueprint.id).DefaultIfEmpty(-1).Where((id) => id >= 0).ToArray(),
            initialShopSeed = seed ^ 0x5F3759DF,
            initialDungeonLayoutId = ResolveLayoutId(ownerData, startRandom),
            threatRiseMultiplier = difficulty switch
            {
                InvasionThreatDifficulty.Easy => 0.85f,
                InvasionThreatDifficulty.Hard => 1.2f,
                _ => 1f
            }
        };
    }

    private static string ResolveLayoutId(CharacterSO ownerData, System.Random startRandom)
    {
        string species = !string.IsNullOrWhiteSpace(ownerData?.SpeciesTag) ? ownerData.SpeciesTag : string.Empty;
        if (species.Equals("Slime", StringComparison.OrdinalIgnoreCase))
        {
            return "wet-front";
        }

        if (species.Equals("Orc", StringComparison.OrdinalIgnoreCase))
        {
            return "training-front";
        }

        if (species.Equals("Vampire", StringComparison.OrdinalIgnoreCase))
        {
            return "research-front";
        }

        string[] defaultLayouts =
        {
            "compact-shop",
            "split-hall",
            "stairs-first"
        };
        return defaultLayouts[startRandom.Next(defaultLayouts.Length)];
    }
}
