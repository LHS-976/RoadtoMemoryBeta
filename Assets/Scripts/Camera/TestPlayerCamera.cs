using UnityEngine;

public class TestPlayerCamera : MonoBehaviour
{
    [Header("Cinemachine Settings")]
    public GameObject CinemachineCameraTarget;

    [Header("Camera Clamping")]
    public float TopClamp = 80.0f;
    public float BottomClamp = -20.0f;

    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = false;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;

    [Header("감도조절 UI로추가예정")]
    public float sensitivity = 1.0f;


    // Start is called before the first frame update
    void Start()
    {
        if (CinemachineCameraTarget != null)
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _cinemachineTargetPitch = CinemachineCameraTarget.transform.rotation.eulerAngles.x;
        }
    }
    private void LateUpdate()
    {
        HandleCameraRotation();
    }
    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            RotateCamera(new Vector2(mouseX, mouseY), true);
        }

    }

    public void RotateCamera(Vector2 lookInput, bool isMouse)
    {
        if (lookInput.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            _cinemachineTargetYaw += lookInput.x * sensitivity;
            _cinemachineTargetPitch += lookInput.y * sensitivity;
        }
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }
    public float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}