using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    public delegate void AddRoom(Vector3 coordinate, List<ConnectionSide> side);
    public static AddRoom addRoom;

    public delegate void NavMeshCalculated();
    public static NavMeshCalculated meshCalculated;

    public delegate void UpdateFloor(int floor);
    public static UpdateFloor updateFloor;
}