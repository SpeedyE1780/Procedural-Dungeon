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

    public int GetRoomExits => numberOfConnections;
    public List<Vector3> GetAdjacentCoordinate => adjacentCoordinate;
    public Vector3 GetCoordinate => RoomCoordinate.nodeCoordinate;

    Dictionary<ConnectionSide, ConnectionController> roomConnections;
    Dictionary<Vector3, ConnectionSide> adjacentRestriction;
    List<RoomController> connectedRooms;
    List<Vector3> adjacentCoordinate;
    List<Transform> waypoints;
    int numberOfConnections;

    private void OnEnable()
    {
        EventManager.meshCalculated += ActivateRoom;
    }

    private void OnDisable()
    {
        EventManager.meshCalculated -= ActivateRoom;
    }

    #region SET ROOM
    public void SetUpRoom(List<ConnectionSide> roomSides)
    {
        roomConnections = new Dictionary<ConnectionSide, ConnectionController>();
        numberOfConnections = 0;
        StringBuilder nameBuilder = new StringBuilder();

        foreach (Transform connection in Connections)
        {
            ConnectionController controller = connection.GetComponent<ConnectionController>();
            ConnectionSide currentSide = controller.connectionSide;

            if (roomSides.Contains(currentSide))
            {
                if (!roomConnections.ContainsKey(currentSide))
                {
                    numberOfConnections++;
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

    public void InitializeRoom(Node node, ConnectionSide? connectionSide, Vector3 connectionPosition, List<RoomController> connectedRoom)
    {
        roomConnections = new Dictionary<ConnectionSide, ConnectionController>();
        adjacentCoordinate = new List<Vector3>();
        connectedRooms = connectedRoom;
        numberOfConnections = 0;
        SetCoordinate(node);

        waypoints = new List<Transform>();
        foreach (Transform waypoint in Waypoints)
        {
            waypoints.Add(waypoint);
        }

        foreach (Transform connection in Connections)
        {
            ConnectionController controller = connection.GetComponent<ConnectionController>();
            ConnectionSide currentSide = controller.connectionSide;

            if (!roomConnections.ContainsKey(currentSide))
            {
                numberOfConnections++;
                roomConnections.Add(currentSide, controller);
                adjacentCoordinate.Add(RoomCoordinate.GetAdjacent(currentSide));
            }
            else
            {
                Debug.LogWarning($"Room already contains {currentSide} door");
            }
        }

        SetPosition(connectionSide, connectionPosition);

        EventManager.addRoom.Invoke(node.nodeCoordinate, roomConnections.Keys.ToList());
    }

    void SetCoordinate(Node coordinate)
    {
        RoomCoordinate = coordinate;
        CoordinateText.text = RoomCoordinate.ToString();

        adjacentRestriction = new Dictionary<Vector3, ConnectionSide>
        {
            { RoomCoordinate.GetAdjacent(ConnectionSide.Forward), ConnectionSide.Backward },
            { RoomCoordinate.GetAdjacent(ConnectionSide.Backward), ConnectionSide.Forward },
            { RoomCoordinate.GetAdjacent(ConnectionSide.Left), ConnectionSide.Right },
            { RoomCoordinate.GetAdjacent(ConnectionSide.Right), ConnectionSide.Left },
            { RoomCoordinate.GetAdjacent(ConnectionSide.Up), ConnectionSide.Down },
            { RoomCoordinate.GetAdjacent(ConnectionSide.Down), ConnectionSide.Up }
        };
    }

    void SetPosition(ConnectionSide? connectionSide, Vector3 connectionPosition)
    {
        if (connectionSide.HasValue)
        {
            transform.position = connectionPosition - roomConnections[connectionSide.Value].transform.localPosition;
        }
        else
        {
            transform.position = Vector3.zero;
        }
    }

    public void AddConnectedRoom(RoomController room)
    {
        connectedRooms.Add(room);

        try
        {
            ConnectionSide side = adjacentRestriction[room.GetCoordinate];

            if (side == ConnectionSide.Up || side == ConnectionSide.Down)
            {
                ConnectPortals(room, side);
            }
        }
        catch
        {
            Debug.Log("Portal error");
            Debug.Log(RoomCoordinate);
            Debug.Log(room.GetCoordinate);
            throw new System.ArgumentException();
        }

    }

    public void ConnectPortals(RoomController neighbourRoom, ConnectionSide side)
    {
        PortalController neighbourPortal = (PortalController)neighbourRoom.roomConnections[side];

        ConnectionSide currentSide = FlipSide(side);
        PortalController currentPortal = (PortalController)roomConnections[currentSide];

        currentPortal.SetNextPosition(neighbourPortal.transform.GetChild(0));
        neighbourPortal.SetNextPosition(currentPortal.transform.GetChild(0));
    }

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

    public ConnectionSide GetAdjacentRestriction(Vector4 adjacent, out bool isDoorRestriction)
    {
        ConnectionSide side = adjacentRestriction[adjacent];
        isDoorRestriction = adjacentCoordinate.Contains(adjacent);
        return side;
    }

    void ActivateRoom()
    {
        if (spawnEnemy)
            SpawnEnemy();

        ShowBridges();
        StartCoroutine(WaitForConnections());
    }

    void SpawnEnemy()
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

    void ShowBridges()
    {
        Connections.gameObject.SetActive(true);
        Bridges.SetActive(false);
    }

    public void ConnectBridges()
    {
        foreach (ConnectionController controller in roomConnections.Values)
            controller.ActivateConnections();

        foreach (RoomController room in connectedRooms)
            room.ConnectBridges(GetCoordinate);
    }

    public void ConnectBridges(Vector3 neighbourCoordinate)
    {
        ConnectionSide side = adjacentRestriction[neighbourCoordinate];
        side = FlipSide(side);

        roomConnections[side].ActivateConnections();
    }

    private IEnumerator WaitForConnections()
    {
        yield return new WaitUntil(() => Enemies.childCount <= 0);
        ConnectBridges();
    }
}