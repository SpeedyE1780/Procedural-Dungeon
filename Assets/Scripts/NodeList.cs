using System.Collections.Generic;

public class NodeList
{
    readonly Queue<Node> nodeQueue;
    readonly Stack<Node> nodeStack;

    public delegate Node ReturnNode();
    public delegate void AddNode(Node node);
    public delegate bool ContainsNode(Node node);
    public delegate int NodeCount();
    public ReturnNode GetNode;
    public AddNode Add;
    public ContainsNode Contains;
    public NodeCount Count;

    public NodeList(bool isBFS)
    {
        //If breadth first search use queue else use stack
        if (isBFS)
        {
            nodeQueue = new Queue<Node>();
            GetNode = nodeQueue.Dequeue;
            Add = nodeQueue.Enqueue;
            Contains = nodeQueue.Contains;
            Count = () => nodeQueue.Count;
        }
        else
        {
            nodeStack = new Stack<Node>();
            GetNode = nodeStack.Pop;
            Add = nodeStack.Push;
            Contains = nodeStack.Contains;
            Count = () => nodeStack.Count;
        }
    }
}