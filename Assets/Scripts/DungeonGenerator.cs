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

    private NodeList availableCoordinates;
    private Dictionary<Vector3, RoomController> generatedRooms;
    private Dictionary<int, Transform> roomsParent;
    private Dictionary<ConnectionSide, List<RoomController>> roomType; //List of all rooms containing the key

    private class RestrictionsData
    {
        private List<ConnectionSide> doorRestrictions; //Sides that need to have doors
        private List<ConnectionSide> wallRestrictions; //Sides that need to have walls

        public bool HasRestrictions => doorRestrictions.Count + wallRestrictions.Count > 0;
        public int DoorCount => doorRestrictions.Count;
        public int WallCount => wallRestrictions.Count;
        public int TotalCount => doorRestrictions.Count + wallRestrictions.Count;

        public RestrictionsData()
        {
            doorRestrictions = new List<ConnectionSide>();
            wallRestrictions = new List<ConnectionSide>();
        }

        public void AddDoorRestriction(ConnectionSide side) => doorRestrictions.Add(side);
        public void AddWallRestriction(ConnectionSide side) => wallRestrictions.Add(side);
        public ConnectionSide GetDoorRestriction(int index) => doorRestrictions[index];
        public ConnectionSide GetWallRestriction(int index) => wallRestrictions[index];

        public void ClearRestrictions()
        {
            doorRestrictions.Clear();
            wallRestrictions.Clear();
        }
    }

    private class ReferenceData
    {
        public ConnectionSide side;
        public Vector3 position;
    }

    private void Awake() => GetCombinations();
    private void OnEnable() => EventManager.updateFloor += TurnOnCurrentFloor;
    private void OnDisable() => EventManager.updateFloor -= TurnOnCurrentFloor;

    //Loop through all possible combinations and instantiate a room
    private void GetCombinations()
    {
        List<int> connections = System.Enum.GetValues(typeof(ConnectionSide)).Cast<int>().ToList();
        roomType = new Dictionary<ConnectionSide, List<RoomController>>();
        Rooms = new List<RoomController>();
        List<ConnectionSide> roomSides = new List<ConnectionSide>();
        double count = Mathf.Pow(2, connections.Count);

        for (int i = 1; i <= count - 1; i++)
        {
            string str = System.Convert.ToString(i, 2).PadLeft(connections.Count, '0');

            roomSides.Clear();
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
        //Wait for room to finish destroying unnecessary connections
        yield return new WaitForSeconds(1);

        generatedRooms = new Dictionary<Vector3, RoomController>();
        roomsParent = new Dictionary<int, Transform>();

        StartCoroutine(BuildDungeon());
    }

    private IEnumerator BuildDungeon()
    {
        Debug.Log($"Generating Rooms Start: {System.DateTime.Now}");

        //Add the starting coordinate
        Node startCoordinate = new Node(Vector3.zero);
        availableCoordinates = new NodeList(BFSExpansion);
        availableCoordinates.Add(startCoordinate);

        RestrictionsData restrictions = new RestrictionsData();
        ReferenceData referenceData = new ReferenceData();
        Node currentNode;
        RoomController currentRoom;
        List<RoomController> connectedRooms = new List<RoomController>();

        UIManager.Instance.ShowStats();

        while (availableCoordinates.Count() != 0)
        {
            restrictions.ClearRestrictions();

            currentNode = availableCoordinates.GetNode();
            connectedRooms = new List<RoomController>();

            GetCurrentRestrictions(currentNode, connectedRooms, restrictions, referenceData);

            //Select a room that fits the restrictions
            if (restrictions.HasRestrictions)
            {
                List<RoomController> acceptableRooms = FilterRooms(restrictions);
                currentRoom = GetRandomRoom(acceptableRooms);
            }
            else
            {
                currentRoom = GetRandomRoom();
            }

            SetUpNewRoom(currentRoom, connectedRooms, currentNode, referenceData);
            SetStats();
            System.GC.Collect();

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

    private void GetCurrentRestrictions(Node currentNode, List<RoomController> connectedRooms, RestrictionsData restrictions, ReferenceData referenceData)
    {
        foreach (Vector3 adjacent in currentNode.GetAdjacentCoordinates)
        {
            if (generatedRooms.ContainsKey(adjacent))
            {
                ConnectionSide restriction = generatedRooms[adjacent].GetAdjacentRestriction(currentNode.nodeCoordinate, out bool isDoorRestriction);

                if (isDoorRestriction)
                {
                    restrictions.AddDoorRestriction(restriction);
                    connectedRooms.Add(generatedRooms[adjacent]);

                    //Set reference side and position
                    referenceData.side = restriction;
                    referenceData.position = generatedRooms[adjacent].GetConnectionPosition(restriction);
                }
                else
                {
                    restrictions.AddWallRestriction(restriction);
                }
            }
        }
    }

    private List<RoomController> FilterRooms(RestrictionsData restrictions)
    {
        List<RoomController> acceptableRooms = new List<RoomController>(Rooms);

        bool deadEnds;

        //True start adding dead ends when the total rooms reach the limit
        //False wait for the main path to reach the limit then add dead ends
        if (BFSExpansion)
            deadEnds = generatedRooms.Count + availableCoordinates.Count() >= NumberOfRooms;
        else
            deadEnds = generatedRooms.Count >= NumberOfRooms;

        FilterRestrictions(ref acceptableRooms, restrictions);
        FilterDeadEnds(ref acceptableRooms, deadEnds, restrictions.DoorCount, restrictions.TotalCount);

        return acceptableRooms;
    }

    private void FilterRestrictions(ref List<RoomController> acceptableRooms, RestrictionsData restrictions)
    {
        ConnectionSide currentRestriction;

        //Get all the rooms that fits the door restrictions
        for (int doorRestrictionIndex = 0; doorRestrictionIndex < restrictions.DoorCount; doorRestrictionIndex++)
        {
            currentRestriction = restrictions.GetDoorRestriction(doorRestrictionIndex);
            acceptableRooms = acceptableRooms.Intersect(roomType[currentRestriction]).ToList();
        }

        //Get all the rooms that fits the wall restrictions
        for (int wallRestrictionIndex = 0; wallRestrictionIndex < restrictions.WallCount; wallRestrictionIndex++)
        {
            currentRestriction = restrictions.GetWallRestriction(wallRestrictionIndex);
            acceptableRooms = acceptableRooms.Except(roomType[currentRestriction]).ToList();
        }
    }

    private void FilterDeadEnds(ref List<RoomController> acceptableRooms, bool keepDeadEnds, int minDoorCount, int restrictionCount)
    {
        //If all sides restricted no extra filter can be added
        bool allSidesRestricted = restrictionCount == roomType.Keys.Count;

        System.Func<int, bool> filterRule;

        if (keepDeadEnds)
            filterRule = RemoveExtraConnections;
        else
            filterRule = RemoveDeadEnds;

        if (allSidesRestricted)
            return;

        for (int i = acceptableRooms.Count - 1; i >= 0; i--)
            if (filterRule(acceptableRooms[i].NumberOfConnections))
                acceptableRooms.RemoveAt(i);

        //Remove room that don't lead to another room
        bool RemoveDeadEnds(int roomConnections) => roomConnections <= minDoorCount;
        //Remove room that lead to another room
        bool RemoveExtraConnections(int roomConnections) => roomConnections > minDoorCount;
    }

    private RoomController GetRandomRoom(List<RoomController> possibleRooms = null)
    {
        RoomController randomRoom;

        //Get random room from possible options
        if (possibleRooms != null && possibleRooms.Count != 0)
            randomRoom = possibleRooms[Random.Range(0, possibleRooms.Count)];
        else
        {
            //Get starting room from all rooms
            if (possibleRooms == null)
                randomRoom = Rooms[Random.Range(0, Rooms.Count)];
            else
                throw new System.Exception("No Available Rooms");
        }

        return randomRoom;
    }

    private void SetUpNewRoom(RoomController currentRoom, List<RoomController> connectedRooms, Node currentNode, ReferenceData reference)
    {
        //Initialize New Room
        currentRoom = Instantiate(currentRoom);
        currentRoom.InitializeRoom(currentNode, reference.side, reference.position, connectedRooms);

        foreach (RoomController controller in connectedRooms)
            controller.AddConnectedRoom(currentRoom);

        //Add the room to the dictionary
        Vector3 coordinate = currentNode.nodeCoordinate;
        generatedRooms.Add(coordinate, currentRoom);

        int currentFloor = (int)currentNode.nodeCoordinate.y;

        if (!roomsParent.ContainsKey(currentFloor))
            roomsParent.Add(currentFloor, CreateFloorParent(currentFloor));

        currentRoom.transform.SetParent(roomsParent[currentFloor]);
        GetNextCoordinate(currentRoom);
    }

    private void GetNextCoordinate(RoomController currentRoom)
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

    //Only show current floor
    private void TurnOnCurrentFloor(int floor)
    {
        foreach (KeyValuePair<int, Transform> kvp in roomsParent)
            kvp.Value.gameObject.SetActive(kvp.Key == floor);
    }

    private Transform CreateFloorParent(int floor)
    {
        GameObject gameObject = new GameObject($"Floor: {floor}");
        gameObject.transform.SetParent(DungeonParent);
        return gameObject.transform;
    }

    private void SetStats()
    {
        string stats = $"Generated Rooms: {generatedRooms.Count}\n Available Coordinates: {availableCoordinates.Count()}";
        UIManager.Instance.UpdateStats(stats);
    }
}