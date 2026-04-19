using UnityEngine;
using PlayerControllerScripts;

/// <summary>
/// 루프 사운드 조절 실패(추후 추가)
/// </summary>
public class DistanceFootstep : MonoBehaviour
{
    [Header("Walk Settings")]
    [SerializeField] private AudioClip[] _walkSounds;
    [Tooltip("걷기 보폭")]
    [SerializeField] private float _walkStepDistance = 2f;

    [Header("Run Settings")]
    [SerializeField] private AudioClip[] _runSounds;
    [Tooltip("뛰기 보폭")]
    [SerializeField] private float _runStepDistance = 3f;

    [Header("General Settings")]
    [SerializeField] private float _volume = 1f;

    private Vector3 _lastPosition;
    private float _accumulatedDistance;

    private float _lastPlayTime;
    private float _soundCooldown = 2.5f;

    private PlayerController _playerController;
    private CharacterController _characterController;

    private void Start()
    {
        _lastPosition = GetHorizontalPosition();
        _playerController = GetComponent<PlayerController>();
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector3 currentPosition = GetHorizontalPosition();
        float distanceMoved = Vector3.Distance(currentPosition, _lastPosition);

        //bool isGrounded = _characterController != null ? _characterController.isGrounded : true;
        bool isGrounded = true;
        bool isSprinting = _playerController != null && _playerController.IsSprint;

        float currentStepDistance = isSprinting ? _runStepDistance : _walkStepDistance;

        if (isGrounded && distanceMoved > 0.001f)
        {
            _accumulatedDistance += distanceMoved;

            if (_accumulatedDistance >= currentStepDistance)
            {
                PlayFootstepSound(isSprinting);
                _accumulatedDistance -= currentStepDistance;
            }
        }
        else if (!isGrounded)
        {
            _accumulatedDistance = 0f;
        }

        _lastPosition = currentPosition;
    }

    private void PlayFootstepSound(bool isSprinting)
    {
        if (Time.time - _lastPlayTime < _soundCooldown) return;
        _lastPlayTime = Time.time;

        AudioClip[] currentSounds = isSprinting ? _runSounds : _walkSounds;

        if (currentSounds == null || currentSounds.Length == 0) return;
        if (SoundManager.Instance == null) return;

        int randomIndex = Random.Range(0, currentSounds.Length);
        AudioClip clipToPlay = currentSounds[randomIndex];

        float randomPitch = Random.Range(0.9f, 1.1f);

        SoundManager.Instance.PlaySFX(clipToPlay, transform.position, _volume);
    }

    private Vector3 GetHorizontalPosition()
    {
        return new Vector3(transform.position.x, 0f, transform.position.z);
    }
}