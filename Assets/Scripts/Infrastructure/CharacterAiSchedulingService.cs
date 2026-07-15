using System;

public interface ICharacterAiSchedulingService
{
    bool IsDrivingAi { get; }
    void Register(CharacterActor actor);
    void Unregister(CharacterActor actor);
    void RequestImmediateDecision(CharacterActor actor);
    bool TryConsumePathSearchBudget();
    bool ShouldShowCharacterFeedback(CharacterActor actor);
    int GetMovementFrameStride(CharacterActor actor);
    void ResetPathSearchBudgetForDebug();
}

public sealed class CharacterAiSchedulingService : ICharacterAiSchedulingService
{
    private readonly IDungeonSceneComponentQuery sceneQuery;
    private CharacterAiScheduler scheduler;

    public CharacterAiSchedulingService(IDungeonSceneComponentQuery sceneQuery)
    {
        this.sceneQuery = sceneQuery
            ?? throw new ArgumentNullException(nameof(sceneQuery));
    }

    public bool IsDrivingAi => ResolveScheduler().IsDrivingAi;

    public void Register(CharacterActor actor)
    {
        ResolveScheduler().RegisterActor(actor);
    }

    public void Unregister(CharacterActor actor)
    {
        if (TryResolveScheduler(out CharacterAiScheduler resolvedScheduler))
        {
            resolvedScheduler.UnregisterActor(actor);
        }
    }

    public void RequestImmediateDecision(CharacterActor actor)
    {
        ResolveScheduler().RequestImmediateDecisionFor(actor);
    }

    public bool TryConsumePathSearchBudget()
    {
        return ResolveScheduler().TryConsumePathSearchBudget();
    }

    public bool ShouldShowCharacterFeedback(CharacterActor actor)
    {
        return ResolveScheduler().ShouldShowCharacterFeedbackFor(actor);
    }

    public int GetMovementFrameStride(CharacterActor actor)
    {
        return ResolveScheduler().GetMovementFrameStrideFor(actor);
    }

    public void ResetPathSearchBudgetForDebug()
    {
        ResolveScheduler().ResetPathSearchBudgetForDebugInstance();
    }

    private CharacterAiScheduler ResolveScheduler()
    {
        return TryResolveScheduler(out CharacterAiScheduler resolvedScheduler)
            ? resolvedScheduler
            : throw new InvalidOperationException($"{nameof(ICharacterAiSchedulingService)} requires a loaded {nameof(CharacterAiScheduler)}.");
    }

    private bool TryResolveScheduler(out CharacterAiScheduler resolvedScheduler)
    {
        scheduler ??= sceneQuery.First<CharacterAiScheduler>(includeInactive: true);
        resolvedScheduler = scheduler;
        return resolvedScheduler != null;
    }
}
