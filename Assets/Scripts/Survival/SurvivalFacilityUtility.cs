public static class SurvivalFacilityUtility
{
    public static FacilityWorkType AddFallbackWorkTypes(BuildableObject building, FacilityWorkType supportedTypes)
    {
        return AddFallbackWorkTypes(building?.BuildingData, supportedTypes);
    }

    public static FacilityWorkType AddFallbackWorkTypes(BuildingSO building, FacilityWorkType supportedTypes)
    {
        if (building == null)
        {
            return supportedTypes;
        }

        if (building.GetAbility<BuildingWaterSourceAbility>() != null)
        {
            supportedTypes |= FacilityWorkType.DrawWater;
        }

        if (building.GetAbility<BuildingCookingAbility>() != null)
        {
            supportedTypes |= FacilityWorkType.Cook;
        }

        if (building.GetAbility<BuildingMedicalAbility>() != null)
        {
            supportedTypes |= FacilityWorkType.Treat;
        }

        if (building.GetAbility<BuildingFuelConsumerAbility>() != null)
        {
            supportedTypes |= FacilityWorkType.Refuel;
        }

        return supportedTypes;
    }

    public static bool IsSurvivalWork(FacilityWorkType workType)
    {
        return workType == FacilityWorkType.DrawWater
            || workType == FacilityWorkType.Cook
            || workType == FacilityWorkType.Treat
            || workType == FacilityWorkType.Refuel;
    }
}
