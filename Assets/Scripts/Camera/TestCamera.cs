using UnityEngine;

public class TestCamera : MonoBehaviour
{
    [Header("Target & Offset")]
    public Transform targetPlayer;
    public Vector3 offset = new Vector3(0, 2f, -5f);

    [Header("Control Settings")]
    public float rotateSpeed = 5.0f;
    public float smoothSpeed = 10f;

    [Header("Restriction angle")]
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 60f;

    [Header("Wall Collision")]
    public LayerMask collisionLayers;
    public float collisionOffset = 0.2f;

    private float rotationValueRL;
    private float rotationValueTD;

    private void Start()
    {
        Initialize();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void LateUpdate()
    {
        if (targetPlayer == null) return;

        HandleRotation();
        HandlePositionAndCollision();
    }

    void Initialize()
    {
        rotationValueRL = transform.eulerAngles.y;
        rotationValueTD = transform.eulerAngles.x;
    }
    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;

        rotationValueRL += mouseX;
        rotationValueTD -= mouseY;

        rotationValueTD = Mathf.Clamp(rotationValueTD, minVerticalAngle, maxVerticalAngle);

        transform.rotation = Quaternion.Euler(rotationValueTD, rotationValueRL, 0);
    }

    void HandlePositionAndCollision()
    {
        Vector3 desiredPosition = targetPlayer.position + transform.rotation * offset;

        RaycastHit hit;
        Vector3 direction = desiredPosition - targetPlayer.position;
        float distance = direction.magnitude;

        if(Physics.Raycast(targetPlayer.position, direction.normalized, out hit, distance, collisionLayers))
        {
            desiredPosition = hit.point - direction.normalized * collisionOffset;
        }
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }


}
