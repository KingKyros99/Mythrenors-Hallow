using UnityEngine;

public class BehaviourTree : MonoBehaviour
{
    private Node root;

    public void Initialize(Node rootNode)
    {
        root = rootNode;
    }

    private void Update()
    {
        if (root == null)
            return;

        root.Evaluate();
    }
}
