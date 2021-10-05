using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : ConnectionController
{
    Transform nextPortal;
    public Vector3 GetNextPosition => nextPortal.position;
    bool isActive;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        EventManager.meshCalculated += () =>
        {
            if (nextPortal == null || nextPortal == transform.GetChild(0))
            {
                Debug.LogError("Portal Not connected");
            }
        };

        isActive = false;
    }

    private void OnEnable()
    {
        if (isActive)
            base.ActivateConnections();
    }

    public override void ActivateConnections()
    {
        isActive = true;
        base.ActivateConnections();
    }

    public void SetNextPosition(Transform portal)
    {
        nextPortal = portal;
    }

    public Vector3 TeleportPlayer(ref int floor)
    {
        floor += connectionSide == ConnectionSide.Up ? 1 : -1;
        return nextPortal.position;
    }
}