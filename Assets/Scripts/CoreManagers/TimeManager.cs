using System.Collections;
using UnityEngine;
using Core;

/// <summary>
/// 게임 내 시간 연출을 전담하는 매니저.
/// 패링 슬로우모션 Time.timeScale 조작이 필요한 모든 상황에서 사용.
/// </summary>
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Listening Channels")]
    [SerializeField] private VoidEventChannelSO _executionSuccessChannel;

    [Header("Execution SlowMotion Settings")]
    [SerializeField] private float _executionSlowScale = 0.1f;
    [SerializeField] private float _executionSlowDuration = 0.3f;

    private Coroutine _activeSlowMotion;
    private float _originalFixedDeltaTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void OnEnable()
    {
        if (_executionSuccessChannel != null)
            _executionSuccessChannel.OnEventRaised += OnExecutionSuccess;
    }

    private void OnDisable()
    {
        if (_executionSuccessChannel != null)
            _executionSuccessChannel.OnEventRaised -= OnExecutionSuccess;
    }

    private void OnExecutionSuccess()
    {
        DoSlowMotion(_executionSlowScale, _executionSlowDuration);
    }

    /// <summary>
    /// 슬로우모션 실행. 외부에서 직접 호출도 가능.
    /// </summary>
    /// <param name="timeScale">느려질 비율 (0.1f = 10% 속도)</param>
    /// <param name="duration">실제 경과 시간 기준 지속 시간</param>
    public void DoSlowMotion(float timeScale, float duration)
    {
        //if (GameManager.Instance.IsPaused) return;

        if (_activeSlowMotion != null)
        {
            StopCoroutine(_activeSlowMotion);
            RestoreTime();
        }
        _activeSlowMotion = StartCoroutine(SlowMotionRoutine(timeScale, duration));
    }

    private IEnumerator SlowMotionRoutine(float timeScale, float duration)
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = _originalFixedDeltaTime * timeScale;

        yield return new WaitForSecondsRealtime(duration);
        //if(!GameManager.Instance.IsPaused){ RestoreTime(); }
        RestoreTime();
        _activeSlowMotion = null;
    }

    private void RestoreTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _originalFixedDeltaTime;
    }

    /// <summary>
    /// 슬로우모션 강제 종료(씬 전환, 일시정지 등)
    /// </summary>
    public void ForceRestoreTime()
    {
        if (_activeSlowMotion != null)
        {
            StopCoroutine(_activeSlowMotion);
            _activeSlowMotion = null;
        }
        RestoreTime();
    }
}