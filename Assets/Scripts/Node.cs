using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Node : IEquatable<Node>
{
    public Vector3 NodeCoordinate;
    //public Dimension Dimension;

    Dictionary<ConnectionSide, Vector3> adjacentCoordinates;

    public List<Vector3> GetAdjacents => adjacentCoordinates.Values.ToList();

    public Node(Vector3 coordinate)
    {
        NodeCoordinate = coordinate;
        //Dimension = (Dimension)coordinate.w;
        SetAdjacent();
    }

    public void SetAdjacent()
    {
        adjacentCoordinates = new Dictionary<ConnectionSide, Vector3>
        {
            { ConnectionSide.Forward, NodeCoordinate + Vector3.forward },
            { ConnectionSide.Backward,  NodeCoordinate + Vector3.back},
            { ConnectionSide.Left, NodeCoordinate + Vector3.left },
            { ConnectionSide.Right, NodeCoordinate + Vector3.right },
            { ConnectionSide.Up, NodeCoordinate + Vector3.up },
            { ConnectionSide.Down, NodeCoordinate + Vector3.down }
        };
    }

    public Vector3 GetAdjacent(ConnectionSide side) => adjacentCoordinates[side];

    public bool Equals(Node otherNode) => NodeCoordinate == otherNode.NodeCoordinate;


    public override string ToString() => $"X:{NodeCoordinate.x} , Y:{NodeCoordinate.y} , Z:{NodeCoordinate.z}";
}