using UnityEngine;
using TMPro;

public class QuestTrackerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private PanelFader _questPanelFader;
    [SerializeField] private TextMeshProUGUI _questTitleText;
    [SerializeField] private TextMeshProUGUI _questDescText;
    [SerializeField] private TextMeshProUGUI _questProgressText;

    [Header("Settings")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.F1;

    [Header("Data & Manager")]
    [SerializeField] private QuestManager _questManager;

    private bool _isToggledOn = true;

    private void Awake()
    {
        if (_questPanelFader == null) _questPanelFader = GetComponent<PanelFader>();

        if (_questManager == null && Core.GameCore.Instance != null)
        {
            _questManager = Core.GameCore.Instance.QuestManager;
        }
    }

    private void OnEnable()
    {
        if (_questManager != null)
        {
            _questManager.OnQuestStarted += HandleQuestStarted;
            _questManager.OnQuestUpdated += HandleQuestUpdated;
            _questManager.OnQuestCompleted += HandleQuestCompleted;
        }
    }

    private void OnDisable()
    {
        if (_questManager != null)
        {
            _questManager.OnQuestStarted -= HandleQuestStarted;
            _questManager.OnQuestUpdated -= HandleQuestUpdated;
            _questManager.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    private void Start()
    {
        Invoke(nameof(ForceUpdateUI), 0.1f);
    }

    private void ForceUpdateUI()
    {
        if (_questManager != null && _questManager.CurrentQuest != null)
        {
            UpdateQuestUI(_questManager.CurrentQuest, _questManager.CurrentProgress);
            ShowUI();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            _isToggledOn = !_isToggledOn;
            if (_isToggledOn) ShowUI();
            else HideUI();
        }
    }

    private void HandleQuestStarted(QuestSO newQuest)
    {
        UpdateQuestUI(newQuest, 0);
        ShowUI();
    }

    private void HandleQuestUpdated(QuestSO quest, int progress)
    {
        if (quest == null) return;
        UpdateQuestUI(quest, progress);
        if(_isToggledOn)
        {
            ShowUI();
        }
    }

    private void HandleQuestCompleted(QuestSO quest)
    {
        if (quest == null) return;

        if (_questProgressText != null) _questProgressText.text = "완료";
        if(_isToggledOn)
        {
            ShowUI();
        }
    }

    private void UpdateQuestUI(QuestSO quest, int progress)
    {
        if (quest == null || (_questManager != null && _questManager.AllQuestsCompleted))
        {
            HideUI();
            return;
        }

        if (_questTitleText != null) _questTitleText.text = quest.Title;
        if (_questDescText != null) _questDescText.text = quest.Description;

        int displayProgress = Mathf.Min(progress, quest.TargetCount);

        if (_questProgressText != null)
            _questProgressText.text = $"({progress} / {quest.TargetCount})";

        Debug.Log($"[UI] 퀘스트 텍스트 갱신 완료: {quest.Title}");
    }

    private void ShowUI()
    {
        _isToggledOn = true;
        _questPanelFader.FadeIn();
    }

    private void HideUI()
    {
        _isToggledOn = false;
        _questPanelFader.FadeOut();
    }
}