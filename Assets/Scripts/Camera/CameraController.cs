using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Cinemachine Settings")]
    [Tooltip("적용할 프리룩 카메라를 넣어주세요.")]
    [SerializeField] private CinemachineFreeLook _freeLookCamera;

    [Header("Base Speeds")]
    [Tooltip("X축(수평) 기본 회전 속도")]
    [SerializeField] private float _baseSpeedX = 200f;
    [Tooltip("Y축(수직) 기본 회전 속도")]
    [SerializeField] private float _baseSpeedY = 2f;

    private void Awake()
    {
        if (_freeLookCamera == null)
            _freeLookCamera = GetComponent<CinemachineFreeLook>();
    }

    private void Update()
    {
        if (_freeLookCamera == null) return;

        float multiX = OptionSettingsManager.HorizontalSensitivity;
        float multiY = OptionSettingsManager.VerticalSensitivity;

        _freeLookCamera.m_XAxis.m_MaxSpeed = _baseSpeedX * multiX;
        _freeLookCamera.m_YAxis.m_MaxSpeed = _baseSpeedY * multiY;
    }
}