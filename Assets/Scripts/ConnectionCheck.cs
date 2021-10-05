using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionCheck : MonoBehaviour
{
    readonly static List<Transform> disconnectedDoors = new List<Transform>();
    public static int GetDisconnected => disconnectedDoors.Count;

    private void Awake()
    {
        //Door starts disconnected
        GetComponent<Renderer>().material.color = Color.red;
        disconnectedDoors.Add(transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Checks if door is connected
        if (other.CompareTag("Player"))
        {
            GetComponent<Renderer>().material.color = Color.green;
            other.GetComponent<Renderer>().material.color = Color.green;
            disconnectedDoors.Remove(transform);
            disconnectedDoors.Remove(other.transform);
        }
    }
}