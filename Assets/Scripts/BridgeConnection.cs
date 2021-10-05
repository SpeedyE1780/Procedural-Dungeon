using UnityEngine;

public class BridgeConnection : ConnectionController
{
    public Transform ConnectionGates;
    public Transform IslandGates;

    public override void UpdateConnection()
    {
        ConnectionGates.SetParent(IslandGates);
    }
}