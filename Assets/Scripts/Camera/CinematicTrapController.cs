using System.Collections;
using UnityEngine;
using Cinemachine;
using Core;

public class CinematicTrapController : MonoBehaviour
{
    [Header("Camera Shake")]
    [SerializeField] private CinemachineImpulseSource _impulseSource;
    [SerializeField] private float _shakeDuration = 0.5f;

    [Header("Story Dialogue")]
    [SerializeField] private DialogueUIManager _dialogueUI;
    [SerializeField] private QuestSO _trapQuestSO;

    [Header("Quest Start")]
    [SerializeField] private StringEventChannelSO _questEventChannel;
    [SerializeField] private string _startQuestIfFail = "Input_Dodge";

    [Header("Trap Drop")]
    [SerializeField] private IntEventChannelSO _forceStartQuestChannel;
    [SerializeField] private int _escapeQuestTargetID;
    [SerializeField] private Rigidbody _obstacleRigidbody;

    [SerializeField] private float _timeLimit = 6.0f;

    [Header("Settings")]
    [SerializeField] private LayerMask _playerLayer;
    private bool _isSequenceStarted = false;
    private bool _isDodgeSuccessful = false;
    private Coroutine _timerCoroutine;


    private void Start()
    {
        GameData data = GameCore.Instance?.DataManager?.CurrentData;
        if (data != null && data.ClearedQuestIDs != null
            && data.ClearedQuestIDs.Contains(_escapeQuestTargetID))
        {
            _isSequenceStarted = true;

            if (_obstacleRigidbody != null)
            {
                _obstacleRigidbody.isKinematic = false;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_isSequenceStarted) return;
        if (((1 << other.gameObject.layer) & _playerLayer) == 0) return;

        GameData data = GameCore.Instance?.DataManager?.CurrentData;
        if (data != null && data.ClearedQuestIDs != null
            && data.ClearedQuestIDs.Contains(_escapeQuestTargetID))
        {
            return;
        }

        _isSequenceStarted = true;
        StartCoroutine(TrapSequenceRoutine());
    }

    private IEnumerator TrapSequenceRoutine()
    {
        //카메라 흔들림
        if (_impulseSource != null) _impulseSource.GenerateImpulse();

        yield return new WaitForSeconds(_shakeDuration);


        bool isDialogueFinished = false;

        if (_dialogueUI != null && _trapQuestSO != null && _trapQuestSO.IntroDialogues.Length > 0)
        {
            _dialogueUI.StartDialogue(_trapQuestSO.IntroDialogues, () => { isDialogueFinished = true; });
        }
        else
        {
            isDialogueFinished = true; //방어 로직
        }
        yield return new WaitUntil(() => isDialogueFinished);

        //퀘스트 알림
        if (_forceStartQuestChannel != null)
        {
            _forceStartQuestChannel.RaiseEvent(_escapeQuestTargetID);
        }


        //도망 퀘스트 클리어하는지 확인
        if (GameCore.Instance != null && GameCore.Instance.QuestManager != null)
        {
            GameCore.Instance.QuestManager.OnQuestCompleted += OnQuestClearedCheck;
        }
        _timerCoroutine = StartCoroutine(TimeLimitRoutine());
    }
    private IEnumerator TimeLimitRoutine()
    {
        yield return new WaitForSeconds(_timeLimit);

        if (!_isDodgeSuccessful)
        {
            //이벤트 구독 해제 (무한 감지 방지)
            if (GameCore.Instance != null && GameCore.Instance.QuestManager != null)
            {
                GameCore.Instance.QuestManager.OnQuestCompleted -= OnQuestClearedCheck;
            }

            if (_obstacleRigidbody != null)
            {
                _obstacleRigidbody.isKinematic = false;
            }
            if (_questEventChannel != null)
            {
                _questEventChannel.RaiseEvent("Input_Dodge");
                Debug.Log("[CinematicTrap] 타임아웃! 강제로 회피 퀘스트를 완료 처리합니다.");
            }
        }
    }


    //퀘스트 매니저가 "퀘스트 깼다"라고 방송할 때마다 실행되는 함수
    private void OnQuestClearedCheck(QuestSO clearedQuest)
    {
        //방금 깬 퀘스트가 우리가 기다리던 '도망 퀘스트'가 맞다면?
        if (clearedQuest != null && clearedQuest.ID == _escapeQuestTargetID)
        {
            _isDodgeSuccessful = true;

            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            //이벤트 구독 해제
            GameCore.Instance.QuestManager.OnQuestCompleted -= OnQuestClearedCheck;

            if (_obstacleRigidbody != null)
            {
                _obstacleRigidbody.isKinematic = false;
            }
        }
    }

    private void OnDestroy()
    {
        //씬이 넘어가거나 파괴될 때 안전장치
        if (GameCore.Instance != null && GameCore.Instance.QuestManager != null)
        {
            GameCore.Instance.QuestManager.OnQuestCompleted -= OnQuestClearedCheck;
        }
    }
}