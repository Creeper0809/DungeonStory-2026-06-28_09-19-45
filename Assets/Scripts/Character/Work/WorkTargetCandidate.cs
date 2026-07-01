public readonly struct WorkTargetCandidate
{
    public WorkTargetCandidate(
        BuildableObject building,
        FacilityWorkType workType,
        WorkPriorityLevel priority,
        float score,
        float urgencyScore,
        string failureReason,
        AIActionFailureKind failureKind = AIActionFailureKind.None)
    {
        Building = building;
        WorkType = workType;
        Priority = priority;
        Score = score;
        UrgencyScore = urgencyScore;
        FailureReason = failureReason;
        FailureKind = failureKind;
        IsValid = building != null && workType != FacilityWorkType.None && priority != WorkPriorityLevel.Off;
    }

    public BuildableObject Building { get; }
    public FacilityWorkType WorkType { get; }
    public WorkPriorityLevel Priority { get; }
    public float Score { get; }
    public float UrgencyScore { get; }
    public string FailureReason { get; }
    public AIActionFailureKind FailureKind { get; }
    public bool IsValid { get; }

    public static WorkTargetCandidate Invalid(
        BuildableObject building,
        string failureReason,
        AIActionFailureKind failureKind = AIActionFailureKind.Unknown)
    {
        return new WorkTargetCandidate(
            building,
            FacilityWorkType.None,
            WorkPriorityLevel.Off,
            float.NegativeInfinity,
            0f,
            failureReason,
            failureKind);
    }
}
