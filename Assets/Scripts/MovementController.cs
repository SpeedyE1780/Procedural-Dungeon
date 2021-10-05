using UnityEngine;

public class MovementController : MonoBehaviour
{
    public float movementSpeed;
    public float rotationSpeed;
    public float MaxRotation;
    public Transform MapIcon;
    public bool lockCursor;

    int currentFloor;
    Rigidbody rb;
    float xRotation;

    private const string EnemyTag = "Enemy";

    private void Start()
    {
        xRotation = 0;
        currentFloor = 0;
        rb = GetComponent<Rigidbody>();
        enabled = false;

        //Enable movement after dungeon is generated
        EventManager.meshCalculated += () =>
        {
            enabled = true;
            rb.isKinematic = false;

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        };
    }

    void FixedUpdate()
    {
        Move();
        Rotate();
        UIManager.Instance.MoveMap(transform.position, transform.rotation.eulerAngles.y);
    }

    private void Move()
    {
        Vector3 movement = movementSpeed * Time.deltaTime * (Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward);
        transform.position += movement;
    }

    private void Rotate()
    {
        float angleY = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float angleX = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        //Rotate left-right
        transform.localRotation *= Quaternion.Euler(0, angleY, 0);

        //Rotate up-down
        xRotation = Mathf.Clamp(xRotation - angleX, -MaxRotation, MaxRotation);
        transform.GetChild(0).localRotation = Quaternion.Euler(xRotation, 0, 0);

        //Rotate map icon
        MapIcon.forward = Quaternion.Euler(0, 0, angleY) * MapIcon.forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(EnemyTag))
            Destroy(other.transform.parent.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                //Returns wether player moved one floor up or down
                currentFloor += other.GetComponentInParent<PortalController>().TeleportPlayer(transform);

                UIManager.Instance.MoveMap(transform.position, transform.rotation.eulerAngles.y);
                EventManager.updateFloor(currentFloor);
            }
        }
    }
}