using UnityEngine;

public class ConnectionController : MonoBehaviour
{
    public ConnectionSide connectionSide;

    protected Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public virtual void UpdateConnection()
    {
        return;
    }

    public virtual void ActivateConnections()
    {
        anim.SetBool("RoomCleared", true);
    }
}

public enum ConnectionSide
{
    Forward,
    Backward,
    Left,
    Right,
    Up,
    Down
    //PortalIn,
    //PortalOut
}