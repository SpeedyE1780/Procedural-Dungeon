using UnityEngine;

public class ConnectionController : MonoBehaviour
{
    public ConnectionSide connectionSide;

    protected Animator anim;

    protected virtual void Awake() => anim = GetComponent<Animator>();
    public virtual void UpdateConnection() { }
    public virtual void ActivateConnections() => anim.SetBool("RoomCleared", true);
}