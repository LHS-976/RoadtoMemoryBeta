using System;
using System.Collections;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private QuestDatabase _database;
    [SerializeField] private DataManager _dataManager;

    private DialogueUIManager _dialogueManager;
    public QuestSO CurrentQuest { get; private set; }
    public int CurrentProgress { get; private set; }
    public bool AllQuestsCompleted { get; private set; }

    public event Action<QuestSO, int> OnQuestUpdated;
    public event Action<QuestSO> OnQuestCompleted;
    public event Action<QuestSO> OnQuestStarted;

    [Header("Listening Channels")]
    [SerializeField] private StringEventChannelSO _questKillChannel;
    [SerializeField] private StringEventChannelSO _questArriveChannel;
    [SerializeField] private StringEventChannelSO _questEventChannel;
    [SerializeField] private IntEventChannelSO _forceStartQuestChannel;

    [Header("Broadcasting Channels")]
    [SerializeField] private IntEventChannelSO _questCompletedChannel;
    [SerializeField] private IntEventChannelSO _questStartedChannel;

    private bool _isChangingQuest = false;

    #region Event callback
    private void OnEnable()
    {
        if (_questKillChannel != null)
            _questKillChannel.OnEventRaised += ReportKill;

        if (_questArriveChannel != null)
            _questArriveChannel.OnEventRaised += ReportArrive;

        if (_questEventChannel != null)
            _questEventChannel.OnEventRaised += ReportEvent;

        if (_forceStartQuestChannel != null)
            _forceStartQuestChannel.OnEventRaised += StartQuest;
    }
    private void OnDisable()
    {
        if (_questKillChannel != null)
            _questKillChannel.OnEventRaised -= ReportKill;

        if (_questArriveChannel != null)
            _questArriveChannel.OnEventRaised -= ReportArrive;

        if (_questEventChannel != null)
            _questEventChannel.OnEventRaised -= ReportEvent;
        if(_forceStartQuestChannel != null)
            _forceStartQuestChannel.OnEventRaised -= StartQuest;
    }
    #endregion
    #region Report API
    public void ReportKill(string enemyID)
    {
        if (!IsActiveQuest(QuestType.Kill)) return;
        if (CurrentQuest.TargetID != enemyID) return;

        AdvanceProgress();
    }

    public void ReportArrive(string sceneName)
    {
        if (!IsActiveQuest(QuestType.Arrive)) return;
        if (CurrentQuest.TargetScene != sceneName) return;

        CompleteCurrentQuest();
    }

    public void ReportEvent(string eventID)
    {
        if (!IsActiveQuest(QuestType.Event)) return;
        if (CurrentQuest.TargetID != eventID) return;

        AdvanceProgress();
    }

    #endregion

    #region Quest Flow

    public void StartQuest(int questID)
    {
        if (_dataManager != null && _dataManager.CurrentData != null)
        {
            if (_dataManager.CurrentData.ClearedQuestIDs != null &&
                _dataManager.CurrentData.ClearedQuestIDs.Contains(questID))
            {
                Debug.Log($"[QuestManager] {questID}번 퀘스트는 이미 클리어했습니다. 강제 시작 요청을 차단합니다.");
                return;
            }
        }
        if (CurrentQuest != null && CurrentQuest.ID == questID)
        {
            return;
        }

        QuestSO quest = _database.GetQuest(questID);
        if (quest == null)
        {
            Debug.LogWarning($"[Quest] ID {questID}을 찾을 수 없습니다.");
            return;
        }
        if(_isChangingQuest)
        {
            StopAllCoroutines();
            _isChangingQuest = false;
        }

        TryStartQuest(quest);
    }

    private void AdvanceProgress()
    {
        if (_isChangingQuest) return;
        CurrentProgress++;
        OnQuestUpdated?.Invoke(CurrentQuest, CurrentProgress);

        if (CurrentProgress >= CurrentQuest.TargetCount)
        {
            CompleteCurrentQuest();
        }
    }

    private void CompleteCurrentQuest()
    {
        if (CurrentQuest == null || _isChangingQuest) return;
        _isChangingQuest = true;

        if (_dataManager != null)
        {
            _dataManager.AddDatachips(CurrentQuest.RewardDataChips);

            GameData data = _dataManager.CurrentData;
            if (data != null)
            {
                if (data.ClearedQuestIDs == null)
                {
                    data.ClearedQuestIDs = new System.Collections.Generic.List<int>();
                }

                if (!data.ClearedQuestIDs.Contains(CurrentQuest.ID))
                {
                    data.ClearedQuestIDs.Add(CurrentQuest.ID);
                }
            }
        }

        OnQuestCompleted?.Invoke(CurrentQuest);
        Debug.Log($"[Quest] 완료: {CurrentQuest.Title}");

        int nextID = CurrentQuest.NextQuestID;
        StartCoroutine(ProceedToNextQuestRoutine(CurrentQuest.ID, nextID));
    }

    #endregion
    #region New / Save / Load
    public void LoadQuestProgress()
    {
        if (_dataManager == null || _database == null) return;

        GameData data = _dataManager.CurrentData;

        //저장된 진행 중인 퀘스트가 있으면 이어서
        if (data.CurrentQuestID > 0)
        {
            QuestSO saved = _database.GetQuest(data.CurrentQuestID);
            if (saved != null)
            {
                CurrentQuest = saved;
                CurrentProgress = data.CurrentQuestProgress;

                OnQuestStarted?.Invoke(CurrentQuest);
                Debug.Log($"[Quest] 이어서 진행: {CurrentQuest.Title} ({CurrentProgress}/{CurrentQuest.TargetCount})");
                return;
            }
        }
        if (data.ClearedQuestIDs == null)
        {
            data.ClearedQuestIDs = new System.Collections.Generic.List<int>();
        }

        //클리어 기록으로 다음 퀘스트 탐색
        if (data.ClearedQuestIDs.Count > 0)
        {
            int lastID = data.ClearedQuestIDs[data.ClearedQuestIDs.Count - 1];
            QuestSO last = _database.GetQuest(lastID);

            if (last != null && last.NextQuestID > 0)
            {
                StartQuest(last.NextQuestID);
                return;
            }

            AllQuestsCompleted = true;
            Debug.Log("[Quest] 모든 퀘스트가 이미 완료되었습니다.");
            return;
        }

        //신규 시작
        QuestSO first = _database.GetFirstMainQuest();
        if (first != null)
        {
            StartQuest(first.ID);
        }
    }

    #endregion
    #region CallBack Quset UI 

    private bool IsActiveQuest(QuestType type)
    {
        return CurrentQuest != null && CurrentQuest.Type == type;
    }
    private IEnumerator ProceedToNextQuestRoutine(int completedQuestID, int nextQuestID)
    {
        _questCompletedChannel?.RaiseEvent(completedQuestID);

        yield return new WaitForSecondsRealtime(1.5f);
        if(nextQuestID >0)
        {
            StartQuest(nextQuestID);
        }
        else
        {
            CurrentQuest = null;
            CurrentProgress = 0;
            AllQuestsCompleted = true;
            Debug.Log("[Quest] 모든 퀘스트 완료. 더 이상 진행할 퀘스트가 없습니다.");
            OnQuestUpdated?.Invoke(null, 0);
        }
        _isChangingQuest = false;
    }
    public void RegisterDialogueUI(DialogueUIManager dialogueUIManager)
    {
        _dialogueManager = dialogueUIManager;
    }
    public void TryStartQuest(QuestSO newQuest)
    {
        if(_dialogueManager != null && newQuest.IntroDialogues != null && newQuest.IntroDialogues.Length >0)
        {
            _dialogueManager.StartDialogue(newQuest.IntroDialogues, () => StartQuestActual(newQuest));
        }
        else
        {
            StartQuestActual(newQuest);
        }
    }
    private void StartQuestActual(QuestSO newQuest)
    {
        CurrentQuest = newQuest;
        CurrentProgress = 0;
        AllQuestsCompleted = false;

        OnQuestStarted?.Invoke(newQuest);
        if (_questStartedChannel != null)
        {
            _questStartedChannel.RaiseEvent(CurrentQuest.ID);
        }
        Debug.Log($"[Quest] 시작: {CurrentQuest.Title}");
    }
    #endregion
}
