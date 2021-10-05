using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class RoomController : MonoBehaviour
{
    public Text CoordinateText;
    public Transform Connections;
    public Node RoomCoordinate;
    public GameObject Bridges;
    public Transform Waypoints;
    public bool spawnEnemy;
    public EnemyController Enemy;
    public Transform Enemies;

    Dictionary<ConnectionSide, ConnectionController> roomConnections;
    Dictionary<Vector3, ConnectionSide> adjacentRestriction;
    List<RoomController> connectedRooms;
    List<Vector3> adjacentCoordinate;
    List<Transform> waypoints;

    public int NumberOfConnections { get; private set; }
    public List<Vector3> GetAdjacentCoordinate => adjacentCoordinate;
    public Vector3 GetCoordinate => RoomCoordinate.nodeCoordinate;

    private void OnEnable() => EventManager.meshCalculated += ActivateRoom;
    private void OnDisable() => EventManager.meshCalculated -= ActivateRoom;

    #region SET ROOM

    /// <summary>
    /// Destroy all connections not available in room sides and name it based on room sides
    /// </summary>
    /// <param name="roomSides">The connections this room can make</param>
    public void SetUpRoom(List<ConnectionSide> roomSides)
    {
        roomConnections = new Dictionary<ConnectionSide, ConnectionController>();
        NumberOfConnections = 0;
        StringBuilder nameBuilder = new StringBuilder();

        foreach (Transform connection in Connections)
        {
            ConnectionController controller = connection.GetComponent<ConnectionController>();
            ConnectionSide currentSide = controller.connectionSide;

            if (roomSides.Contains(currentSide))
            {
                if (!roomConnections.ContainsKey(currentSide))
                {
                    NumberOfConnections += 1;
                    roomConnections.Add(currentSide, controller);
                    nameBuilder.Append(currentSide.ToString()[0]); //Get the first letter of side
                }
                else
                {
                    Debug.LogWarning($"Room already contains {currentSide} door");
                }
            }
            else
            {
                controller.UpdateConnection();
                Destroy(connection.gameObject);
            }
        }

        gameObject.name = nameBuilder.ToString();
    }

    #endregion

    public void InitializeRoom(Node node, ConnectionSide? connectionSide, Vector3 connectionPosition, List<RoomController> connectedRooms)
    {
        roomConnections = new Dictionary<ConnectionSide, ConnectionController>();
        adjacentCoordinate = new List<Vector3>();
        this.connectedRooms = connectedRooms;
        NumberOfConnections = 0;
        SetCoordinate(node);
        SetWaypoints();
        SetConnections();
        SetPosition(connectionSide, connectionPosition);
        EventManager.addRoom.Invoke(node.nodeCoordinate, roomConnections.Keys.ToList());
    }

    private void SetCoordinate(Node coordinate)
    {
        RoomCoordinate = coordinate;
        CoordinateText.text = RoomCoordinate.ToString();

        adjacentRestriction = new Dictionary<Vector3, ConnectionSide>
        {
            { RoomCoordinate.GetAdjacentCoordinate(ConnectionSide.Forward), ConnectionSide.Backward },
            { RoomCoordinate.GetAdjacentCoordinate(ConnectionSide.Backward), ConnectionSide.Forward },
            { RoomCoordinate.GetAdjacentCoordinate(ConnectionSide.Left), ConnectionSide.Right },
            { RoomCoordinate.GetAdjacentCoordinate(ConnectionSide.Right), ConnectionSide.Left },
            { RoomCoordinate.GetAdjacentCoordinate(ConnectionSide.Up), ConnectionSide.Down },
            { RoomCoordinate.GetAdjacentCoordinate(ConnectionSide.Down), ConnectionSide.Up }
        };
    }

    private void SetWaypoints()
    {
        waypoints = new List<Transform>();

        foreach (Transform waypoint in Waypoints)
            waypoints.Add(waypoint);
    }

    private void SetConnections()
    {
        foreach (Transform connection in Connections)
        {
            ConnectionController controller = connection.GetComponent<ConnectionController>();
            ConnectionSide currentSide = controller.connectionSide;

            if (!roomConnections.ContainsKey(currentSide))
            {
                NumberOfConnections++;
                roomConnections.Add(currentSide, controller);
                adjacentCoordinate.Add(RoomCoordinate.GetAdjacentCoordinate(currentSide));
            }
            else
            {
                Debug.LogWarning($"Room already contains {currentSide} door");
            }
        }
    }

    private void SetPosition(ConnectionSide? connectionSide, Vector3 connectionPosition)
    {
        //True means this is the starting room
        //False connect based on the connected room
        if (!connectionSide.HasValue)
            transform.position = Vector3.zero;
        else
            transform.position = connectionPosition - roomConnections[connectionSide.Value].transform.localPosition;
    }

    public void AddConnectedRoom(RoomController room)
    {
        connectedRooms.Add(room);

        //Check if room are connected through portal
        ConnectionSide side = adjacentRestriction[room.GetCoordinate];

        if (side == ConnectionSide.Up || side == ConnectionSide.Down)
            ConnectPortals(room, side);
    }

    private void ConnectPortals(RoomController neighbourRoom, ConnectionSide side)
    {
        PortalController neighbourPortal = (PortalController)neighbourRoom.roomConnections[side];

        ConnectionSide currentSide = FlipSide(side);
        PortalController currentPortal = (PortalController)roomConnections[currentSide];

        //Set portals target positions
        currentPortal.SetNextPosition(neighbourPortal.transform.GetChild(0));
        neighbourPortal.SetNextPosition(currentPortal.transform.GetChild(0));
    }

    //Flip the neighbour side to get the new side based on the room and return the position of its connection 
    public Vector3 GetConnectionPosition(ConnectionSide side)
    {
        ConnectionSide referenceSide = FlipSide(side);
        return roomConnections[referenceSide].transform.position;
    }

    ConnectionSide FlipSide(ConnectionSide side)
    {
        ConnectionSide referenceSide;
        switch (side)
        {
            case ConnectionSide.Forward:
                referenceSide = ConnectionSide.Backward;
                break;
            case ConnectionSide.Backward:
                referenceSide = ConnectionSide.Forward;
                break;
            case ConnectionSide.Left:
                referenceSide = ConnectionSide.Right;
                break;
            case ConnectionSide.Right:
                referenceSide = ConnectionSide.Left;
                break;
            case ConnectionSide.Up:
                referenceSide = ConnectionSide.Down;
                break;
            case ConnectionSide.Down:
                referenceSide = ConnectionSide.Up;
                break;
            default:
                throw new System.ArgumentException();
        }

        return referenceSide;
    }

    //Return which side needs to be a wall/door
    public ConnectionSide GetAdjacentRestriction(Vector3 adjacent, out bool isDoorRestriction)
    {
        ConnectionSide side = adjacentRestriction[adjacent];
        isDoorRestriction = adjacentCoordinate.Contains(adjacent);
        return side;
    }

    private void ActivateRoom()
    {
        if (spawnEnemy)
            SpawnEnemy();

        ShowBridges();
        StartCoroutine(WaitUntilClear());
    }

    private void SpawnEnemy()
    {
        int index = 0;

        foreach (Transform waypoint in waypoints)
        {
            EnemyController enemy = Instantiate(Enemy, waypoint.position, Quaternion.identity);
            enemy.transform.SetParent(Enemies);
            enemy.SetEnemy(waypoints, index++);
            enemy.name = $"{name}_enemy{index}";
        }
    }

    private void ShowBridges()
    {
        Connections.gameObject.SetActive(true);
        Bridges.SetActive(false);
    }

    private IEnumerator WaitUntilClear()
    {
        yield return new WaitUntil(() => Enemies.childCount == 0);
        ConnectBridges();
    }

    private void ConnectBridges()
    {
        //Activate all room connections
        foreach (ConnectionController controller in roomConnections.Values)
            controller.ActivateConnections();

        foreach (RoomController room in connectedRooms)
            room.ConnectBridges(GetCoordinate);
    }

    //Activate connection to neighbour
    private void ConnectBridges(Vector3 neighbourCoordinate)
    {
        //Get side from neighbour to current then flip it
        ConnectionSide side = adjacentRestriction[neighbourCoordinate];
        side = FlipSide(side);
        roomConnections[side].ActivateConnections();
    }
}