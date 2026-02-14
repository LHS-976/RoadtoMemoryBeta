using UnityEngine;

public class HitTimerController : MonoBehaviour
{
    [SerializeField] private Animator _myAnimator;
    [SerializeField] private Animator _enemyAnimator;

    private float _stopTimer;
    private bool _isStopped;
    private float _restoreSpeed;

    private void Awake()
    {
        if (_myAnimator == null) _myAnimator = GetComponentInChildren<Animator>();
    }


    private void Update()
    {
        if (!_isStopped) return;

        _stopTimer -= Time.unscaledDeltaTime;

        if(_stopTimer <= 0f) 
        {
            RestoreHitStop();
        }
    }

    public void StartHitStop(float duration, Animator targetAnimator = null)
    {
        if (_isStopped) return;

        _restoreSpeed = _myAnimator.speed;
        _stopTimer = duration;
        _isStopped = true;
        _myAnimator.speed = 0f;

        if(targetAnimator != null)
        {
            _enemyAnimator = targetAnimator;
            _enemyAnimator.speed = 0f;
        }
    }
    private void RestoreHitStop()
    {
        _isStopped = false;
        _myAnimator.speed = _restoreSpeed;

        if(_enemyAnimator != null)
        {
            _enemyAnimator.speed = 1f;
            _enemyAnimator = null;
        }
    }

    public void ForceStop()
    {
        if(_isStopped)
        {
            _stopTimer = 0f;
            RestoreHitStop();
        }
    }
    private void OnDisable()
    {
        if(_isStopped)
        {
            RestoreHitStop();
        }
    }
}
