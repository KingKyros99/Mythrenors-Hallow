using System;
using UnityEngine;

public class ActionNode : Node
{
    private readonly Func<EnemyContext, NodeState> action;
    private readonly EnemyContext context;

    public ActionNode(EnemyContext context, Func<EnemyContext, NodeState> action)
    {
        this.context = context;
        this.action = action;
    }

    public override NodeState Evaluate()
    {
        if (action == null || context == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        state = action(context);

        return state;
    }
}
