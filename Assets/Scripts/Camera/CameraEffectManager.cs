using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraEffectManager : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineFreeLook _freeLookCamera;
    [SerializeField] private CinemachineImpulseSource _impulseSource;

    [Header("Hit Zoom Settings")]
    [SerializeField] private float _hitZoomAmount = 5f;
    [SerializeField] private float _hitZoomInDuration = 0.05f;
    [SerializeField] private float _hitZoomOutDuration = 0.15f;

    [Header("Heavy Hit Zoom Settings")]
    [SerializeField] private float _heavyZoomAmount = 10f;
    [SerializeField] private float _heavyZoomInDuration = 0.05f;
    [SerializeField] private float _heavyZoomOutDuration = 0.3f;

    [Header("Shake Settings")]
    [SerializeField] private float _hitShakeForce = 0.5f;
    [SerializeField] private float _heavyShakeForce = 1.5f;

    [Header("Listening Channels")]
    [SerializeField] private VoidEventChannelSO _hitEffectChannel;
    [SerializeField] private VoidEventChannelSO _heavyHitEffectChannel;

    private float _defaultFOV;
    private Coroutine _zoomCoroutine;

    private void Awake()
    {
        if (_impulseSource == null)
            _impulseSource = GetComponent<CinemachineImpulseSource>();

        if (_freeLookCamera != null)
            _defaultFOV = _freeLookCamera.m_Lens.FieldOfView;
    }

    private void OnEnable()
    {
        if (_hitEffectChannel != null)
            _hitEffectChannel.OnEventRaised += OnHitEffect;

        if (_heavyHitEffectChannel != null)
            _heavyHitEffectChannel.OnEventRaised += OnHeavyHitEffect;
    }

    private void OnDisable()
    {
        if (_hitEffectChannel != null)
            _hitEffectChannel.OnEventRaised -= OnHitEffect;

        if (_heavyHitEffectChannel != null)
            _heavyHitEffectChannel.OnEventRaised -= OnHeavyHitEffect;
    }

    #region Event Handlers

    private void OnHitEffect()
    {
        PlayZoom(_hitZoomAmount, _hitZoomInDuration, _hitZoomOutDuration);
        PlayShake(_hitShakeForce);
    }

    private void OnHeavyHitEffect()
    {
        PlayZoom(_heavyZoomAmount, _heavyZoomInDuration, _heavyZoomOutDuration);
        PlayShake(_heavyShakeForce);
    }

    #endregion

    #region Zoom

    public void PlayZoom(float zoomAmount, float zoomInDuration, float zoomOutDuration)
    {
        if (_freeLookCamera == null) return;

        if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
        _zoomCoroutine = StartCoroutine(ZoomRoutine(zoomAmount, zoomInDuration, zoomOutDuration));
    }

    private IEnumerator ZoomRoutine(float zoomAmount, float zoomInDuration, float zoomOutDuration)
    {
        float targetFOV = _defaultFOV - zoomAmount;
        float startFOV = _freeLookCamera.m_Lens.FieldOfView;

        //Zoom In
        float t = 0f;
        while (t < zoomInDuration)
        {
            t += Time.unscaledDeltaTime;
            _freeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t / zoomInDuration);
            yield return null;
        }

        _freeLookCamera.m_Lens.FieldOfView = targetFOV;

        //Zoom Out
        t = 0f;
        while (t < zoomOutDuration)
        {
            t += Time.unscaledDeltaTime;
            _freeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(targetFOV, _defaultFOV, t / zoomOutDuration);
            yield return null;
        }

        _freeLookCamera.m_Lens.FieldOfView = _defaultFOV;
        _zoomCoroutine = null;
    }

    #endregion

    #region Shake

    public void PlayShake(float force)
    {
        if (_impulseSource == null) return;
        _impulseSource.GenerateImpulse(force);
    }

    #endregion

    public void ResetFOV()
    {
        if (_zoomCoroutine != null)
        {
            StopCoroutine(_zoomCoroutine);
            _zoomCoroutine = null;
        }

        if (_freeLookCamera != null)
            _freeLookCamera.m_Lens.FieldOfView = _defaultFOV;
    }
}