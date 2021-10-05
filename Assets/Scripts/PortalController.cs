using UnityEngine;

public class PortalController : ConnectionController
{
    Vector3 targetPosition;
    bool isActive;
    int floorIncrement;

    protected override void Awake()
    {
        base.Awake();
        isActive = false;
        floorIncrement = connectionSide == ConnectionSide.Up ? 1 : -1;
    }

    //Play open animation again
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

    //Set portal target position
    public void SetNextPosition(Transform portal) => targetPosition = portal.position;

    //Update floor and return position
    public Vector3 GetTargetPosition(ref int floor)
    {
        floor += floorIncrement;
        return targetPosition;
    }
}