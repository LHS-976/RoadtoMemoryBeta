using UnityEngine;
using UnityEngine.Events;

public class SwitchInteractable : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private string _prompt = "작동";
    [SerializeField] private bool _isOneShot = false;

    [Header("Condition Settings (Optional)")]
    [Tooltip("체크하면 특정 퀘스트를 완료하기 전까지 단말기가 잠금 상태가 됩니다.")]
    [SerializeField] private bool _isConditional = false;
    [Tooltip("잠금 해제에 필요한 퀘스트 ID (조건부 켤 때만 작동)")]
    [SerializeField] private System.Collections.Generic.List<int> _unlockQuestIDs = new System.Collections.Generic.List<int>();

    [SerializeField] private string _lockedPrompt = "작동 불가 (권한 없음)";
    [SerializeField] private AudioClip _errorSFX;

    //퀘스트 완료 시 자동 작동
    [Header("Auto Trigger On Quest")]
    [Tooltip("체크하면 특정 퀘스트 완료 시 아래의 이벤트를 자동으로 실행합니다")]
    [SerializeField] private bool _autoTriggerOnQuest = false;
    [Tooltip("자동 실행을 발동시킬 퀘스트 ID")]
    [SerializeField] private int _autoTriggerQuestID;
    [SerializeField] private UnityEvent _onQuestAutoTriggered;

    [Header("Broadcast Channel")]
    [Tooltip("퀘스트 완료 방송 수신 채널")]
    [SerializeField] private IntEventChannelSO _questCompletedChannel;

    [Header("Events")]
    [SerializeField] private UnityEvent _onActivated;
    [SerializeField] private UnityEvent _onDeactivated;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioClip _activateSFX;

    private bool _isActive = false;
    private bool _isUsed = false;
    private bool _isUnlocked = true;

    public bool CanInteract => !_isUsed;

    public Transform ObjectTransform => transform;

    private void Awake()
    {
        if (_isConditional)
        {
            _isUnlocked = false;
        }
    }

    //실시간으로 세이브 데이터를 뜯어보는 판독기 함수를 만듭니다.
    private bool CheckIfUnlocked()
    {
        if (!_isConditional) return true; //접근 제한이 꺼져있으면 무조건 통과
        if (_isUnlocked) return true;     //게임 도중 방송을 듣고 열렸다면 통과

        //실시간으로 세이브 파일 확인
        if (Core.GameCore.Instance != null && Core.GameCore.Instance.DataManager != null)
        {
            GameData data = Core.GameCore.Instance.DataManager.CurrentData;
            if (data != null && data.ClearedQuestIDs != null)
            {
                bool allCleared = true;
                foreach (int requiredID in _unlockQuestIDs)
                {
                    if (!data.ClearedQuestIDs.Contains(requiredID))
                    {
                        allCleared = false;
                        break;
                    }
                }

                //모두 깼다면 잠금 해제
                if (allCleared)
                {
                    _isUnlocked = true;
                    Debug.Log($"[Switch] 요구된 {_unlockQuestIDs.Count}개의 퀘스트 클리어 확인. 스위치 잠금을 해제합니다.");
                    return true;
                }
            }
        }
        return false;
    }
    public string InteractionPrompt
    {
        get
        {
            if(CheckIfUnlocked())
            {
                return $"[F] {_prompt}";
            }
            int clearedCount = 0;
            if (Core.GameCore.Instance != null && Core.GameCore.Instance.DataManager != null)
            {
                GameData data = Core.GameCore.Instance.DataManager.CurrentData;
                if (data != null && data.ClearedQuestIDs != null)
                {
                    //요구하는 퀘스트 목록 중 클리어수 확인.
                    foreach (int id in _unlockQuestIDs)
                    {
                        if (data.ClearedQuestIDs.Contains(id))
                        {
                            clearedCount++;
                        }
                    }
                }
            }
            if (_unlockQuestIDs.Count > 1)
            {
                //다중 조건일 경우 진행도 확인
                return $"[X] {_lockedPrompt} ({clearedCount}/{_unlockQuestIDs.Count} 완료)";
            }
            else
            {
                //단일 조건일 경우, 잠금 메시지만 보여줌
                return $"[X] {_lockedPrompt}";
            }
        }
    }

    public void Interact(GameObject interactor)
    {
        if (!CheckIfUnlocked())
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

    //채널 구독 조건을 통합하여 안전하게 변경했습니다.
    private void OnEnable()
    {
        if (_questCompletedChannel != null)
        {
            _questCompletedChannel.OnEventRaised += HandleQuestCompleted;
        }
    }

    private void OnDisable()
    {
        if (_questCompletedChannel != null)
        {
            _questCompletedChannel.OnEventRaised -= HandleQuestCompleted;
        }
    }

    private void HandleQuestCompleted(int completedQuestID)
    {
        //잠금 해제 감지
        if (_isConditional && !_isUnlocked && _unlockQuestIDs.Contains(completedQuestID))
        {
            //요구 조건 중 하나를 방금 깼으니, 이제 '전부 다 깼는지' 한 번 검사해 봅니다.
            if (CheckIfUnlocked())
            {
                Debug.Log("[Switch] 방금 퀘스트로 모든 조건이 충족되었습니다. 스위치가 활성화됩니다.");
            }
        }

        //자동 작동 감지 (예: 문 닫기)
        if (_autoTriggerOnQuest && completedQuestID == _autoTriggerQuestID)
        {
            _onQuestAutoTriggered?.Invoke();
            Debug.Log($"[Switch] {completedQuestID}번 퀘스트 완료 감지! 자동 이벤트를 실행합니다.");

            //만약 닫힌 이후에 스위치 상태를 다시 꺼짐으로 초기화하고 싶다면:
            _isActive = false;
        }
    }

}