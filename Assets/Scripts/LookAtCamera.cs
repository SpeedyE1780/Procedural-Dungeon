using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Transform mainCamera;
    Quaternion initalRotation;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
        initalRotation = transform.rotation;
    }

    private void LateUpdate() => transform.rotation = initalRotation * mainCamera.rotation;
}