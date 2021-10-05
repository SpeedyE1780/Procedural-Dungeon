using System.Collections.Generic;
using UnityEngine.Events;

public class NodeList
{
    readonly Queue<Node> nodeQueue;
    readonly Stack<Node> nodeStack;
    readonly bool BFS;

    public delegate Node ReturnNode();
    public ReturnNode GetNode;

    public int Count => BFS ? nodeQueue.Count : nodeStack.Count;

    public NodeList(bool isBFS)
    {
        BFS = isBFS;

        if (isBFS)
        {
            nodeQueue = new Queue<Node>();
            GetNode = nodeQueue.Dequeue;
        }
        else
        {
            nodeStack = new Stack<Node>();
            GetNode = nodeStack.Pop;
        }
    }

    public void AddNode(Node node)
    {
        if (BFS)
            nodeQueue.Enqueue(node);
        else
            nodeStack.Push(node);
    }

    public bool Contains(Node node)
    {
        if (BFS)
            return nodeQueue.Contains(node);
        else
            return nodeStack.Contains(node);
    }
}