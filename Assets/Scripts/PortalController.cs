using UnityEngine;

public class PortalController : ConnectionController
{
    Vector3 targetPosition;
    int floorIncrement;

    protected override void Awake()
    {
        base.Awake();
        floorIncrement = connectionSide == ConnectionSide.Up ? 1 : -1;
    }

    //Play open animation
    public override void ActivateConnections()
    {
        base.ActivateConnections();
    }

    //Set portal target position
    public void SetNextPosition(Transform portal) => targetPosition = portal.position;

    //Teleport player and return wether we moved up or down a floor
    public int TeleportPlayer(Transform player)
    {
        player.position = targetPosition;
        return floorIncrement;
    }
}