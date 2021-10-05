using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class DungeonGenerator : MonoBehaviour
{
    public RoomController ReferenceRoom; //Room with all connections available
    public Transform AllRoomsParent; //Store instantiated room variations
    public Transform DungeonParent;
    public NavMeshSurface DungeonNavMesh;
    public List<RoomController> Rooms; //All rooms available to place
    public int NumberOfRooms;
    public bool BFSExpansion;
    public bool FrameByFrame;
    public bool WaitForKeyPress;

    NodeList availableCoordinates;
    Dictionary<Vector3, RoomController> generatedRooms;
    Dictionary<int, Transform> roomsParent;
    Dictionary<ConnectionSide, List<RoomController>> roomType; //List of all rooms containing the key

    private void Awake() => GetCombination();
    private void OnEnable() => EventManager.updateFloor += TurnOnCurrentFloor;
    private void OnDisable() => EventManager.updateFloor -= TurnOnCurrentFloor;

    void GetCombination()
    {
        List<int> connections = System.Enum.GetValues(typeof(ConnectionSide)).Cast<int>().ToList();
        roomType = new Dictionary<ConnectionSide, List<RoomController>>();
        Rooms = new List<RoomController>();

        double count = System.Math.Pow(2, connections.Count);
        for (int i = 1; i <= count - 1; i++)
        {
            string str = System.Convert.ToString(i, 2).PadLeft(connections.Count, '0');

            List<ConnectionSide> roomSides = new List<ConnectionSide>();
            RoomController currentRoom = Instantiate(ReferenceRoom, AllRoomsParent);

            for (int j = 0; j < str.Length; j++)
            {
                if (str[j] == '1')
                {
                    ConnectionSide side = (ConnectionSide)connections[j];
                    roomSides.Add(side);

                    if (roomType.ContainsKey(side))
                        roomType[side].Add(currentRoom);
                    else
                        roomType.Add(side, new List<RoomController> { currentRoom });
                }
            }

            currentRoom.SetUpRoom(roomSides);
            Rooms.Add(currentRoom);
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        generatedRooms = new Dictionary<Vector3, RoomController>();
        roomsParent = new Dictionary<int, Transform>();

        StartCoroutine(BuildDungeon());
    }

    IEnumerator BuildDungeon()
    {
        Debug.Log($"Generating Rooms Start: {System.DateTime.Now}");

        //Add the starting coordinate
        Node startCoordinate = new Node(Vector3.zero);
        availableCoordinates = new NodeList(BFSExpansion);
        availableCoordinates.Add(startCoordinate);

        List<ConnectionSide> doorRestrictions = new List<ConnectionSide>(); //Doors that need to be available when building the room
        List<ConnectionSide> wallRestrictions = new List<ConnectionSide>(); //Walls that need to be available when building the room
        Node currentCoordinate; //Current Coordinate where placing the room
        RoomController currentRoom; //Current Room to place
        List<RoomController> connectedRooms;

        ConnectionSide? referenceSide = null;
        Vector3 referencePosition = Vector3.zero;

        UIManager.Instance.ShowStats();

        while (availableCoordinates.Count() != 0)
        {
            doorRestrictions.Clear(); //Empty door restrictions for the current coordinate
            wallRestrictions.Clear(); //Empty wall restrictions for the current coordinate

            //currentCoordinate = availableCoordinates.Dequeue(); //Get next available coordinate
            currentCoordinate = availableCoordinates.GetNode(); //Get next available coordinate
            connectedRooms = new List<RoomController>();

            GetCurrentRestrictions(currentCoordinate, doorRestrictions, wallRestrictions, connectedRooms, ref referenceSide, ref referencePosition);

            //Select a room that fits the restrictions
            if (doorRestrictions.Count != 0 || wallRestrictions.Count != 0)
            {
                List<RoomController> acceptableRooms = FilterRooms(doorRestrictions, wallRestrictions);
                currentRoom = SelectRandomRoom(acceptableRooms);
            }

            //Create a random room
            else
            {
                currentRoom = SelectRandomRoom();
            }

            SetUpNewRoom(ref currentRoom, connectedRooms, currentCoordinate, referenceSide, referencePosition);
            GetNextCoordinate(currentRoom);
            SetStats();

            if (FrameByFrame)
            {
                if (WaitForKeyPress)
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

                yield return null;
            }
        }

        Debug.Log($"Generating Rooms End: {System.DateTime.Now}");

        DungeonNavMesh.BuildNavMesh();
        EventManager.meshCalculated?.Invoke();
        TurnOnCurrentFloor(0);
        UIManager.Instance.HideStats();
        yield return null;
    }

    void GetCurrentRestrictions(Node currentNode, List<ConnectionSide> doorRestrictions, List<ConnectionSide> wallRestrictions,
        List<RoomController> connectedRooms, ref ConnectionSide? referenceSide, ref Vector3 referencePosition)
    {
        foreach (Vector3 adjacent in currentNode.GetAdjacentCoordinates)
        {
            //Get restriction of the current room
            if (generatedRooms.ContainsKey(adjacent))
            {
                ConnectionSide restriction = generatedRooms[adjacent].GetAdjacentRestriction(currentNode.nodeCoordinate, out bool isDoorRestriction);

                //Add door restriction if availabe
                if (isDoorRestriction)
                {
                    doorRestrictions.Add(restriction);
                    referenceSide = restriction;

                    connectedRooms.Add(generatedRooms[adjacent]);
                    referencePosition = generatedRooms[adjacent].GetConnectionPosition(restriction);
                }

                //Add wall restriction
                else
                {
                    wallRestrictions.Add(restriction);
                }
            }
        }
    }

    List<RoomController> FilterRooms(List<ConnectionSide> doorRestrictions, List<ConnectionSide> wallRestrictions)
    {
        List<RoomController> acceptableRooms = new List<RoomController>(Rooms);

        bool deadEnds = BFSExpansion ? generatedRooms.Count + availableCoordinates.Count() >= NumberOfRooms : generatedRooms.Count >= NumberOfRooms;

        FilterRestrictions(ref acceptableRooms, doorRestrictions, wallRestrictions);
        FilterDeadEnds(ref acceptableRooms, deadEnds,
            doorRestrictions.Count, doorRestrictions.Count + wallRestrictions.Count);
        return acceptableRooms;
    }

    void FilterRestrictions(ref List<RoomController> acceptableRooms, List<ConnectionSide> doorRestrictions, List<ConnectionSide> wallRestrictions)
    {
        ConnectionSide currentRestriction;

        //Get all the rooms that fits the door restrictions
        for (int doorRestrictionIndex = 0; doorRestrictionIndex < doorRestrictions.Count; doorRestrictionIndex++)
        {
            currentRestriction = doorRestrictions[doorRestrictionIndex];
            acceptableRooms = acceptableRooms.Intersect(roomType[currentRestriction]).ToList();
        }
        //Get all the rooms that fits the wall restrictions
        for (int wallRestrictionIndex = 0; wallRestrictionIndex < wallRestrictions.Count; wallRestrictionIndex++)
        {
            currentRestriction = wallRestrictions[wallRestrictionIndex];
            acceptableRooms = acceptableRooms.Except(roomType[currentRestriction]).ToList();
        }
    }

    void FilterDeadEnds(ref List<RoomController> acceptableRooms, bool keepDeadEnds, int minDoorCount, int restrictionCount)
    {
        for (int i = 0; i < acceptableRooms.Count;)
        {
            //Create Dead Ends
            if (keepDeadEnds)
            {
                if (acceptableRooms[i].NumberOfConnections > minDoorCount)
                    acceptableRooms.RemoveAt(i);
                else
                    i++;
            }

            //Create Room that leads to another room
            else
            {
                if (restrictionCount < roomType.Keys.Count && acceptableRooms[i].NumberOfConnections <= minDoorCount)
                    acceptableRooms.RemoveAt(i);
                else
                    i++;
            }
        }
    }

    RoomController SelectRandomRoom(List<RoomController> possibleRooms = null)
    {
        RoomController randomRoom;

        if (possibleRooms != null && possibleRooms.Count != 0)
            randomRoom = possibleRooms[Random.Range(0, possibleRooms.Count)];
        else
        {
            if (possibleRooms == null)
                randomRoom = Rooms[Random.Range(0, Rooms.Count)];
            else
                throw new System.Exception("No Available Rooms");
        }

        return randomRoom;
    }

    void SetUpNewRoom(ref RoomController currentRoom, List<RoomController> connectedRooms, Node currentNode, ConnectionSide? referenceSide, Vector3 referencePosition)
    {
        //Initialize New Room
        currentRoom = Instantiate(currentRoom);
        currentRoom.InitializeRoom(currentNode, referenceSide, referencePosition, connectedRooms);

        foreach (RoomController controller in connectedRooms)
            controller.AddConnectedRoom(currentRoom);

        //Add the room to the dictionary
        Vector3 coordinate = currentNode.nodeCoordinate;
        generatedRooms.Add(coordinate, currentRoom);

        int currentFloor = (int)currentNode.nodeCoordinate.y;

        if (!roomsParent.ContainsKey(currentFloor))
        {
            roomsParent.Add(currentFloor, CreateFloorParent(currentFloor));
        }

        currentRoom.transform.SetParent(roomsParent[currentFloor]);
    }

    void GetNextCoordinate(RoomController currentRoom)
    {
        //Get the current neighbours
        List<Vector3> adjacentCoordinate = currentRoom.GetAdjacentCoordinate;

        //Add all available coordinate
        foreach (Vector3 coordinate in adjacentCoordinate)
        {
            Node node = new Node(coordinate);
            if (!generatedRooms.ContainsKey(coordinate) && !availableCoordinates.Contains(node))
                availableCoordinates.Add(node);
        }
    }

    void TurnOnCurrentFloor(int floor)
    {
        foreach (KeyValuePair<int, Transform> kvp in roomsParent)
            kvp.Value.gameObject.SetActive(kvp.Key == floor);
    }

    Transform CreateFloorParent(int floor)
    {
        GameObject gameObject = new GameObject($"Floor: {floor}");
        gameObject.transform.SetParent(DungeonParent);
        return gameObject.transform;
    }

    void SetStats()
    {
        string stats = $"Generated Rooms: {generatedRooms.Count}\n Available Coordinates: {availableCoordinates.Count()}";
        UIManager.Instance.UpdateStats(stats);
    }
}