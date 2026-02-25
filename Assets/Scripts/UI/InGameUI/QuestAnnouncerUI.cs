using System.Collections;
using UnityEngine;
using TMPro;
using Core;

public class QuestAnnouncerUI : MonoBehaviour
{
    [SerializeField] private PanelFader _announcerFader;
    [SerializeField] private TextMeshProUGUI _announceTitleText;
    [SerializeField] private TextMeshProUGUI _announceSubText;
    [SerializeField] private QuestManager _questManager;
    [SerializeField] private GameStateSO _gameState;

    private const float _fadeOut = 1.2f;

    private Coroutine _announceCoroutine;

    private void Awake()
    {
        if (_questManager == null && Core.GameCore.Instance != null)
        {
            _questManager = Core.GameCore.Instance.QuestManager;
        }
    }

    private void Start()
    {
        if(_questManager == null || _questManager.CurrentQuest == null || _questManager.AllQuestsCompleted)
        {
            return;
        }

        if (_announcerFader != null && _questManager != null && _questManager.CurrentQuest != null)
        {
            _announceTitleText.text = "현재 목표";
            _announceSubText.text = _questManager.CurrentQuest.Description;

            if (_announceCoroutine != null) StopCoroutine(_announceCoroutine);
            _announceCoroutine = StartCoroutine(ShowAndHideRoutine());
        }
    }

    private void OnEnable()
    {
        if (_questManager != null)
        {
            _questManager.OnQuestStarted += ShowQuestStart;
            _questManager.OnQuestCompleted += ShowQuestComplete;
        }
        if (_gameState != null)
            _gameState.OnStateChange += HandleStateChange;
    }

    private void OnDisable()
    {
        if (_questManager != null)
        {
            _questManager.OnQuestStarted -= ShowQuestStart;
            _questManager.OnQuestCompleted -= ShowQuestComplete;
        }
        if (_gameState != null)
            _gameState.OnStateChange -= HandleStateChange;
    }

    private void ShowQuestStart(QuestSO quest)
    {
        if (quest == null) return;

        _announceTitleText.text = "퀘스트 시작";
        _announceSubText.text = quest.Description;

        if (_announceCoroutine != null) StopCoroutine(_announceCoroutine);
        _announceCoroutine = StartCoroutine(ShowAndHideRoutine());
    }

    private void ShowQuestComplete(QuestSO quest)
    {
        if (quest == null) return;

        _announceTitleText.text = "퀘스트 완료";
        _announceSubText.text = $"보상: {quest.RewardDataChips} DataChips 획득.";

        if (_announceCoroutine != null) StopCoroutine(_announceCoroutine);
        _announceCoroutine = StartCoroutine(ShowAndHideRoutine());
    }

    private IEnumerator ShowAndHideRoutine()
    {
        _announcerFader.FadeIn();
        yield return new WaitForSeconds(_fadeOut);
        _announcerFader.FadeOut();
    }

    //스토리 대화박스 중에 퀘스트알림 지우기.
    private void HandleStateChange(GameState state)
    {
        if (state == GameState.Dialogue)
        {
            if (_announceCoroutine != null) StopCoroutine(_announceCoroutine);
            _announcerFader.FadeOut();
        }
    }
}