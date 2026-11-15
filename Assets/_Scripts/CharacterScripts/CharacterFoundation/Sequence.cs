using System.Collections.Generic;
using UnityEngine;

public class Sequence : Node
{
    private readonly List<Node> children;

    public Sequence(List<Node> children)
    {
        this.children = children;
    }

    public override NodeState Evaluate()
    {
        foreach (Node child in children)
        {
            switch (child.Evaluate())
            {
                case NodeState.SUCCESS:
                    continue;

                case NodeState.FAILURE:
                    state = NodeState.FAILURE;
                    return state;

                case NodeState.RUNNING:
                    state = NodeState.RUNNING;
                    return state;
            }
        }

        state = NodeState.SUCCESS;
        return state;
    }
}
