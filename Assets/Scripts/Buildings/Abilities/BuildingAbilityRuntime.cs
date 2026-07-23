using System.Collections.Generic;

public interface IBuildingVisualRuntimeAbility
{
    void ConfigureVisual(BuildableObject building);
}

public interface IBuildingUseCompletedRuntimeAbility
{
    void ApplyUseCompleted(CharacterActor actor, BuildableObject building);
}

public interface IBuildingWorkCompletedRuntimeAbility
{
    int ApplyWorkCompleted(CharacterActor actor, BuildableObject building, FacilityWorkType workType);
}

public interface IBuildingExteriorWorkRuntimeAbility
{
    bool SupportsExteriorWork(FacilityWorkType workType);
    bool IsExteriorWorkAvailable(CharacterActor actor, BuildableObject building, FacilityWorkType workType);
    float GetExteriorWorkSeconds(CharacterActor actor, BuildableObject building, FacilityWorkType workType);
    float GetExteriorWorkUrgency(CharacterActor actor, BuildableObject building, FacilityWorkType workType);
}

public interface IBuildingWorkAmountRuntimeAbility
{
    float GetRequiredWork(BuildableObject building, FacilityWorkType workType);
}

public interface IBuildingRuntimeStateAbility
{
    IBuildingStateModule CreateStateModule(BuildableObject building);
}

public interface IBuildingStockCategorySignal
{
    IEnumerable<StockCategory> GetStockCategorySignals();
}

public interface IBuildingCrimeRiskModifier
{
    float ModifyCrimePressure(float pressure, FacilityCrimeRiskContext context);
}
