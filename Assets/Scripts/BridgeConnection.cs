using UnityEngine;

public class BridgeConnection : ConnectionController
{
    public Transform ConnectionGates;
    public Transform IslandGates;

    //Change gate parent to island gate to prevent destroying with connection
    public override void UpdateConnection() => ConnectionGates.SetParent(IslandGates);
}