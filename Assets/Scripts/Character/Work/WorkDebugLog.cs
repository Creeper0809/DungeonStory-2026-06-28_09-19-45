public static class WorkDebugLog
{
    public static void LogStarted(CharacterActor actor)
    {
        if (TryGetWorkContext(actor, out string context))
        {
            actor.AddLog($"작업 시작 · {context}");
        }
    }

    public static void LogEnd(CharacterActor actor, string reason = null)
    {
        if (!TryGetWorkContext(actor, out string context))
        {
            return;
        }

        string message = $"작업 종료 · {context}";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            message += $" · {reason}";
        }

        actor?.AddLog(message);
    }

    private static bool TryGetWorkContext(CharacterActor actor, out string context)
    {
        if (actor == null || !actor.TryGetAbility(out AbilityWork work))
        {
            context = string.Empty;
            return false;
        }

        if (work.AssignedWorkType == FacilityWorkType.None || work.assignedShop == null)
        {
            context = string.Empty;
            return false;
        }

        string workName = WorkTaskCatalog.GetDisplayName(work.AssignedWorkType);
        BuildableObject target = work.assignedShop;
        string targetName = target != null && target.BuildingData != null
            && !string.IsNullOrWhiteSpace(target.BuildingData.objectName)
                ? target.BuildingData.objectName
                : target.name;

        context = $"{workName} · {targetName}";
        return true;
    }
}
