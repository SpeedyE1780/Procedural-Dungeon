using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private const float RoomPixelSize = 150f;

    public GameObject GameUI;
    public Text StatsText;
    public RectTransform Map;
    public MapRoom MapRooms;

    private static UIManager _instance;
    public static UIManager Instance => _instance;

    Dictionary<int, Transform> mapFloor;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);

        mapFloor = new Dictionary<int, Transform>();
    }

    private void OnEnable()
    {
        EventManager.addRoom += AddRoom;
        EventManager.updateFloor += TurnOnCurrentFloor;
    }

    private void OnDisable()
    {
        EventManager.addRoom -= AddRoom;
        EventManager.updateFloor -= TurnOnCurrentFloor;
    }

    public void ShowStats()
    {
        StatsText.gameObject.SetActive(true);
        GameUI.SetActive(false);
    }

    public void HideStats()
    {
        StatsText.gameObject.SetActive(false);
        GameUI.SetActive(true);
    }

    public void UpdateStats(string stats) => StatsText.text = stats;

    void AddRoom(Vector3 coordinate, List<ConnectionSide> sides)
    {
        if (!mapFloor.ContainsKey((int)coordinate.y))
            mapFloor.Add((int)coordinate.y, CreateFloorParent((int)coordinate.y));

        MapRoom current = Instantiate(MapRooms, mapFloor[(int)coordinate.y]);
        current.SetRoom(coordinate, sides, Map);
    }

    RectTransform CreateFloorParent(int floor)
    {
        //Create new parent and set it active if it's the starting floor
        GameObject gameObject = new GameObject($"Floor: {floor}");
        gameObject.SetActive(floor == 0);

        //Set rect parent, position & scale
        RectTransform rect = gameObject.AddComponent<RectTransform>();
        rect.SetParent(Map);
        rect.anchoredPosition = Vector3.zero;
        rect.localScale = Vector3.one;
        return rect;
    }

    void TurnOnCurrentFloor(int floor)
    {
        foreach (int floorLevel in mapFloor.Keys)
            mapFloor[floorLevel].gameObject.SetActive(floor == floorLevel);
    }

    public void MoveMap(Vector3 movement, float angleZ)
    {
        //Move map by the player offset
        //Divide x and z to convert to pixel coordinates
        Vector2 offset = new Vector2(movement.x / 53, movement.z / 48) * RoomPixelSize;
        Map.anchoredPosition = -offset;

        //Set map rotation to the player's rotation
        Map.transform.parent.rotation = Quaternion.Euler(0, 0, angleZ);
    }
}