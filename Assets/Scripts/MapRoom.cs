using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRoom : MonoBehaviour
{
    RectTransform rect;
    RectTransform parentRect;
    public Transform ConnectionParents;
    public GameObject RoomImage;

    public void SetRoom(Vector3 coordinate, List<ConnectionSide> sides, RectTransform parent)
    {
        rect = GetComponent<RectTransform>();
        parentRect = parent;
        rect.anchoredPosition = (coordinate.x * Vector2.right + coordinate.z * Vector2.up) * 150;

        foreach (Transform child in ConnectionParents)
        {
            ConnectionSide side = child.GetComponent<ConnectionController>().connectionSide;

            if (sides.Contains(side))
                child.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (gameObject.activeInHierarchy && !RoomImage.activeSelf)
        {
            if ((rect.anchoredPosition + parentRect.anchoredPosition).magnitude < 75)
            {
                RoomImage.SetActive(true);
                this.enabled = false;
            }
        }
    }
}