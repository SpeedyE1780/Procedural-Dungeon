using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Node : IEquatable<Node>
{
    public Vector3 nodeCoordinate;
    Dictionary<ConnectionSide, Vector3> adjacentCoordinates;

    public List<Vector3> GetAdjacents => adjacentCoordinates.Values.ToList();

    public Node(Vector3 coordinate)
    {
        nodeCoordinate = coordinate;
        SetAdjacentCoordinates();
    }

    public void SetAdjacentCoordinates()
    {
        adjacentCoordinates = new Dictionary<ConnectionSide, Vector3>
        {
            { ConnectionSide.Forward, nodeCoordinate + Vector3.forward },
            { ConnectionSide.Backward,  nodeCoordinate + Vector3.back},
            { ConnectionSide.Left, nodeCoordinate + Vector3.left },
            { ConnectionSide.Right, nodeCoordinate + Vector3.right },
            { ConnectionSide.Up, nodeCoordinate + Vector3.up },
            { ConnectionSide.Down, nodeCoordinate + Vector3.down }
        };
    }

    public Vector3 GetAdjacent(ConnectionSide side) => adjacentCoordinates[side];
    public bool Equals(Node otherNode) => nodeCoordinate == otherNode.nodeCoordinate;
    public override string ToString() => $"X:{nodeCoordinate.x} , Y:{nodeCoordinate.y} , Z:{nodeCoordinate.z}";
}