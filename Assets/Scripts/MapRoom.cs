using System.Collections.Generic;
using UnityEngine;

public class MapRoom : MonoBehaviour
{
    private const float PixelThreshold = 75f;
    private const float RoomSize = 150f;

    RectTransform rect;
    RectTransform parentRect;
    public Transform ConnectionParents;
    public GameObject RoomImage;

    public void SetRoom(Vector3 coordinate, List<ConnectionSide> sides, RectTransform parent)
    {
        rect = GetComponent<RectTransform>();
        parentRect = parent;
        rect.anchoredPosition = (coordinate.x * Vector2.right + coordinate.z * Vector2.up) * RoomSize;
        ShowConnections(sides);
    }

    private void ShowConnections(List<ConnectionSide> sides)
    {
        foreach (Transform child in ConnectionParents)
        {
            ConnectionSide side = child.GetComponent<ConnectionController>().connectionSide;

            if (sides.Contains(side))
                child.gameObject.SetActive(true);
        }
    }

    //Wait unitl player is close enough to enable room image
    private void Update()
    {
        if ((rect.anchoredPosition + parentRect.anchoredPosition).magnitude < PixelThreshold)
        {
            RoomImage.SetActive(true);
            enabled = false;
        }
    }
}