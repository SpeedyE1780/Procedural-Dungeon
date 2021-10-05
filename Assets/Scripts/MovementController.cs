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
        enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 movement = (Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward) * movementSpeed * Time.deltaTime;
        float angleY = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float angleX = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        transform.position += movement;
        transform.localRotation *= Quaternion.Euler(0, angleY, 0);

        xRotation = Mathf.Clamp(xRotation - angleX, -MaxRotation, MaxRotation);
        transform.GetChild(0).localRotation = Quaternion.Euler(xRotation, 0, 0);

        MapIcon.forward = Quaternion.Euler(0, 0, angleY) * MapIcon.forward;

        UIManager.Instance.MoveMap(transform.position, transform.rotation.eulerAngles.y);
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
                Vector3 destination = other.GetComponentInParent<PortalController>().TeleportPlayer(ref currentFloor);
                Vector3 offset = destination - transform.position;
                transform.position = destination;

                UIManager.Instance.MoveMap(offset, 0);

                EventManager.updateFloor(currentFloor);
            }
        }
    }
}