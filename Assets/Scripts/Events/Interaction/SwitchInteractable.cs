using UnityEngine;
using UnityEngine.Events;

public class SwitchInteractable : MonoBehaviour,IInteractable
{
    [Header("Settings")]
    [SerializeField] private string _prompt = "작동";
    [SerializeField] private bool _isOneShot = false;

    [Header("Condition Settings (Optional)")]
    [Tooltip("체크하면 특정 퀘스트를 완료하기 전까지 단말기가 잠금 상태가 됩니다.")]
    [SerializeField] private bool _isConditional = false;
    [Tooltip("잠금 해제에 필요한 퀘스트 ID (조건부 켤 때만 작동)")]
    [SerializeField] private int _unlockQuestID;
    [Tooltip("퀘스트 완료 방송 수신 채널 (조건부 켤 때만 작동)")]
    [SerializeField] private IntEventChannelSO _questCompletedChannel;
    [SerializeField] private string _lockedPrompt = "작동 불가 (권한 없음)";
    [SerializeField] private AudioClip _errorSFX;


    [Header("Events")]
    [SerializeField] private UnityEvent _onActivated;
    [SerializeField] private UnityEvent _onDeactivated;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioClip _activateSFX;

    private bool _isActive = false;
    private bool _isUsed = false;

    private bool _isUnlocked = true;

    public string InteractionPrompt => _isUnlocked ? $"[F] {_prompt}" : $"[X] {_lockedPrompt}";
    public bool CanInteract => !_isUsed;

    public Transform ObjectTransform => transform;


    private void Awake()
    {
        if(_isConditional)
        {
            _isUnlocked = false;
        }
    }
    private void OnEnable()
    {
        if (_isConditional && _questCompletedChannel != null)
        {
            _questCompletedChannel.OnEventRaised += HandleQuestCompleted;
        }
    }
    private void OnDisable()
    {
        if (_isConditional && _questCompletedChannel != null)
        {
            _questCompletedChannel.OnEventRaised -= HandleQuestCompleted;
        }
    }
    private void HandleQuestCompleted(int completedQuestID)
    {
        if(completedQuestID == _unlockQuestID)
        {
            _isUnlocked = true;
        }
    }
    public void Interact(GameObject interactor)
    {
        if(!_isUnlocked)
        {
            if (_errorSFX != null) SoundManager.Instance?.PlaySFX(_errorSFX, transform.position);
            return;
        }
        _isActive = !_isActive;

        if (_isActive) _onActivated?.Invoke();
        else _onDeactivated?.Invoke();

        if (_activateSFX != null) SoundManager.Instance?.PlaySFX(_activateSFX, transform.position);

        if (_isOneShot) _isUsed = true;
    }
}
