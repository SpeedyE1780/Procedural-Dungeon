using UnityEngine;

public class ConnectionController : MonoBehaviour
{
    public ConnectionSide connectionSide;

    protected Animator anim;

    protected virtual void Awake() => anim = GetComponent<Animator>();

    //Activate connections when room are set active again
    protected virtual void OnEnable()
    {
        if (anim == null)
            return;

        ActivateConnections();
    }

    public virtual void UpdateConnection() { }
    public virtual void ActivateConnections() => anim.SetBool("RoomCleared", true);
}