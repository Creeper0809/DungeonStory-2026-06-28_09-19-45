using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class ModularFacilityRuntimeEffects
{
    public static void ConfigureVisual(BuildableObject building)
    {
        FacilityOperationalData data = building?.Operational;
        if (data == null || !data.HasFunction(FacilityFunction.Lighting) || data.lightIntensity <= 0f)
        {
            return;
        }

        Light2D light = building.GetComponent<Light2D>();
        if (light == null)
        {
            light = building.gameObject.AddComponent<Light2D>();
        }

        light.lightType = Light2D.LightType.Point;
        light.intensity = data.lightIntensity;
        light.pointLightInnerRadius = Mathf.Max(0.1f, data.lightRadius * 0.35f);
        light.pointLightOuterRadius = Mathf.Max(light.pointLightInnerRadius + 0.1f, data.lightRadius);
        light.color = new Color(1f, 0.72f, 0.38f, 1f);
        light.falloffIntensity = 0.65f;
    }

    public static void ApplyUseCompleted(CharacterActor actor, BuildableObject building)
    {
        if (building?.Operational == null || !building.Operational.IsModular)
        {
            return;
        }

        if (building.Operational.HasFunction(FacilityFunction.MeleeTraining))
        {
            ApplyExperienceMood(actor, building, "근접 훈련의 감각이 살아남", 4f);
        }
        else if (building.Operational.HasFunction(FacilityFunction.RangedTraining))
        {
            ApplyExperienceMood(actor, building, "과녁에 집중해 마음이 맑아짐", 3f);
        }
        else if (building.Operational.HasFunction(FacilityFunction.StrengthTraining))
        {
            ApplyExperienceMood(actor, building, "고된 훈련을 끝낸 성취감", 5f);
        }
    }

    public static int ApplyWorkCompleted(
        CharacterActor actor,
        BuildableObject building,
        FacilityWorkType workType)
    {
        FacilityOperationalData data = building?.Operational;
        if (data == null || !data.IsModular)
        {
            return 0;
        }

        FacilityOperationalState state = building.OperationalState;
        state.completedWorkCycles++;

        if (workType == FacilityWorkType.Clean)
        {
            foreach (BuildableObject part in building.GetRoomOperationalProfile().Parts)
            {
                if (part != null)
                {
                    part.OperationalState.cleanliness = 100f;
                }
            }

            actor?.AddLog($"{GetName(building)} 청소를 마쳐 방이 말끔해졌다.");
            return 0;
        }

        if (workType == FacilityWorkType.Guard
            && (data.HasFunction(FacilityFunction.Alarm)
                || data.HasFunction(FacilityFunction.GuardPost)))
        {
            state.alarmCharges = Mathf.Min(3, state.alarmCharges + 1);
            actor?.AddLog($"{GetName(building)} 경계 태세를 갖췄다. ({state.alarmCharges}/3)");
        }

        if (workType != FacilityWorkType.Operate && workType != FacilityWorkType.Research)
        {
            return 0;
        }

        if (data.HasFunction(FacilityFunction.MealProduction))
        {
            int amount = Produce(building, StockCategory.Food, data.workOutputAmount);
            state.producedStock += amount;
            actor?.AddLog($"{GetName(building)}에서 식량 {amount}개를 준비했다.");
            return amount;
        }

        if (data.HasFunction(FacilityFunction.WeaponCrafting))
        {
            int amount = Produce(building, StockCategory.Weapon, data.workOutputAmount);
            state.producedStock += amount;
            actor?.AddLog($"{GetName(building)}에서 무기 물자 {amount}개를 만들었다.");
            return amount;
        }

        if (data.HasFunction(FacilityFunction.Alchemy)
            || data.HasFunction(FacilityFunction.ManaStorage)
            || data.HasFunction(FacilityFunction.ManaRitual))
        {
            int amount = Produce(building, StockCategory.Mana, data.workOutputAmount);
            state.producedStock += amount;
            actor?.AddLog($"{GetName(building)}에 마력 {amount}개를 축적했다.");
            return amount;
        }

        return 0;
    }

    private static int Produce(BuildableObject source, StockCategory category, int requested)
    {
        int remaining = Mathf.Max(0, requested);
        if (remaining <= 0 || source == null)
        {
            return 0;
        }

        IEnumerable<IWarehouseFacility> roomWarehouses = source
            .GetRoomOperationalProfile()
            .Parts
            .OfType<IWarehouseFacility>()
            .Where(IsUsableWarehouse);
        int produced = Deposit(roomWarehouses, category, remaining, out remaining);

        if (remaining > 0 && source.Grid != null)
        {
            IEnumerable<IWarehouseFacility> allWarehouses = source.Grid
                .FindAllOccupants(null)
                .OfType<IWarehouseFacility>()
                .Where(IsUsableWarehouse);
            produced += Deposit(allWarehouses, category, remaining, out remaining);
        }

        return produced;
    }

    private static int Deposit(
        IEnumerable<IWarehouseFacility> warehouses,
        StockCategory category,
        int requested,
        out int remaining)
    {
        remaining = Mathf.Max(0, requested);
        int deposited = 0;
        foreach (IWarehouseFacility warehouse in warehouses ?? Enumerable.Empty<IWarehouseFacility>())
        {
            if (remaining <= 0) break;
            int amount = warehouse.Inventory.Deposit(category, remaining);
            deposited += amount;
            remaining -= amount;
        }

        return deposited;
    }

    private static bool IsUsableWarehouse(IWarehouseFacility warehouse)
    {
        return warehouse != null && warehouse.HasWarehouseInventory && warehouse.Inventory != null;
    }

    private static void ApplyExperienceMood(
        CharacterActor actor,
        BuildableObject building,
        string label,
        float mood)
    {
        actor?.ApplyMoodFactor(
            $"facility-training:{building.GetInstanceID()}",
            label,
            mood,
            180f,
            1);
    }

    private static string GetName(BuildableObject building)
    {
        return building?.BuildingData != null
            ? building.BuildingData.objectName
            : "시설";
    }
}
