using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text StatsText;
    public RectTransform Map;
    public MapRoom MapRooms;

    private static UIManager _instance;
    public static UIManager Instance => _instance;

    Dictionary<int, Transform> mapLevel;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);

        mapLevel = new Dictionary<int, Transform>();
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

    void AddRoom(Vector3 coordinate, List<ConnectionSide> sides)
    {
        if (!mapLevel.ContainsKey((int)coordinate.y))
        {
            mapLevel.Add((int)coordinate.y, CreateFloorParent((int)coordinate.y));
        }

        MapRoom current = Instantiate(MapRooms, mapLevel[(int)coordinate.y]);
        current.SetRoom(coordinate, sides, Map);
    }

    public void SetStats(string stats)
    {
        StatsText.text = stats;
    }

    RectTransform CreateFloorParent(int floor)
    {
        GameObject gameObject = new GameObject($"Floor: {floor}");
        gameObject.SetActive(floor == 0);
        RectTransform rect = gameObject.AddComponent<RectTransform>();
        rect.SetParent(Map);
        rect.anchoredPosition = Vector3.zero;
        rect.localScale = Vector3.one;
        return rect;
    }

    public void MoveMap(Vector3 movement, float angleZ)
    {
        Vector2 offset = new Vector2(movement.x / 53, movement.z / 48) * 150;
        Map.anchoredPosition = -offset;
        Map.transform.parent.rotation = Quaternion.Euler(0, 0, angleZ);
    }

    void TurnOnCurrentFloor(int floor)
    {
        foreach (int floorLevel in mapLevel.Keys)
            mapLevel[floorLevel].gameObject.SetActive(floor == floorLevel);
    }
}