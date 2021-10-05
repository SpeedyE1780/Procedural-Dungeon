using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Transform mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
    }

    private void Update()
    {
        Vector3 newForward = transform.position - mainCamera.position;
        transform.forward = newForward;
    }
}
