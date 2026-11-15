using System;
using UnityEngine;

public class ConditionNode : Node
{
    private readonly Func<EnemyContext, bool> condition;
    private readonly EnemyContext context;

    public ConditionNode(EnemyContext context, Func<EnemyContext, bool> condition)
    {
        this.context = context;
        this.condition = condition;
    }

    public override NodeState Evaluate()
    {
        if (condition == null || context == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        state = condition(context)
            ? NodeState.SUCCESS
            : NodeState.FAILURE;

        return state;
    }
}
