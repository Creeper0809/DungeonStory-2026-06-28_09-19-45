using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public interface ICharacterBehaviorTreeRuntimeConfigurator
{
    BehaviorTree Configure(CharacterActor actor, ExternalBehaviorTree externalBehavior);
}

public sealed class CharacterBehaviorTreeRuntimeConfigurator : ICharacterBehaviorTreeRuntimeConfigurator
{
    public BehaviorTree Configure(CharacterActor actor, ExternalBehaviorTree externalBehavior)
    {
        BehaviorTree behaviorTree = actor != null ? actor.GetComponent<BehaviorTree>() : null;
        if (behaviorTree == null)
        {
            behaviorTree = actor != null ? actor.gameObject.AddComponent<BehaviorTree>() : null;
            if (behaviorTree == null)
            {
                return null;
            }
        }

        if (behaviorTree.ExternalBehavior == null && externalBehavior != null)
        {
            behaviorTree.ExternalBehavior = externalBehavior;
        }

        behaviorTree.StartWhenEnabled = false;
        if (Application.isPlaying)
        {
            behaviorTree.enabled = true;
            if (behaviorTree.ExternalBehavior != null)
            {
                behaviorTree.DungeonStoryRefreshVisualStatus(actor);

                if (behaviorTree.ExecutionStatus != TaskStatus.Running)
                {
                    behaviorTree.EnableBehavior();
                }
            }
        }

        return behaviorTree;
    }
}
