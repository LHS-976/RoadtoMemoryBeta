using System;
using System.Collections;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private QuestDatabase _database;
    [SerializeField] private DataManager _dataManager;

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

    [Header("Broadcasting Channels")]
    [SerializeField] private IntEventChannelSO _questCompletedChannel;
    [SerializeField] private IntEventChannelSO _questStartedChannel;

    private void Start()
    {
        LoadQuestProgress();
    }
    #region Event callback
    private void OnEnable()
    {
        if (_questKillChannel != null)
            _questKillChannel.OnEventRaised += ReportKill;

        if (_questArriveChannel != null)
            _questArriveChannel.OnEventRaised += ReportArrive;

        if (_questEventChannel != null)
            _questEventChannel.OnEventRaised += ReportEvent;
    }
    private void OnDisable()
    {
        if (_questKillChannel != null)
            _questKillChannel.OnEventRaised -= ReportKill;

        if (_questArriveChannel != null)
            _questArriveChannel.OnEventRaised -= ReportArrive;

        if (_questEventChannel != null)
            _questEventChannel.OnEventRaised -= ReportEvent;
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
        QuestSO quest = _database.GetQuest(questID);
        if (quest == null)
        {
            Debug.LogWarning($"[Quest] ID {questID}을 찾을 수 없습니다.");
            return;
        }

        CurrentQuest = quest;
        CurrentProgress = 0;
        AllQuestsCompleted = false;

        SaveProgress();
        OnQuestStarted?.Invoke(CurrentQuest);
        if (_questStartedChannel != null)
        {
            _questStartedChannel.RaiseEvent(CurrentQuest.ID);
        }
        Debug.Log($"[Quest] 시작: {CurrentQuest.Title}");
    }

    private void AdvanceProgress()
    {
        CurrentProgress++;
        SaveProgress();
        OnQuestUpdated?.Invoke(CurrentQuest, CurrentProgress);

        if (CurrentProgress >= CurrentQuest.TargetCount)
        {
            CompleteCurrentQuest();
        }
    }

    private void CompleteCurrentQuest()
    {
        if (CurrentQuest == null) return;

        _dataManager?.AddDatachips(CurrentQuest.RewardDataChips);

        GameData data = _dataManager?.CurrentData;
        if (data != null && !data.ClearedQuestIDs.Contains(CurrentQuest.ID))
        {
            data.ClearedQuestIDs.Add(CurrentQuest.ID);
        }

        OnQuestCompleted?.Invoke(CurrentQuest);
        Debug.Log($"[Quest] 완료: {CurrentQuest.Title}");

        int nextID = CurrentQuest.NextQuestID;
        StartCoroutine(ProceedToNextQuestRoutine(CurrentQuest.ID, nextID));
    }

    #endregion
    #region New / Save / Load


    public void ResetForNewGame()
    {
        CurrentQuest = null;
        CurrentProgress = 0;
        AllQuestsCompleted = false;

        QuestSO first = _database.GetFirstMainQuest();
        if(first != null)
        {
            StartQuest(first.ID);
        }
    }
    private void SaveProgress()
    {
        if (_dataManager == null) return;

        GameData data = _dataManager.CurrentData;
        data.CurrentQuestID = CurrentQuest != null ? CurrentQuest.ID : -1;
        data.CurrentQuestProgress = CurrentProgress;
        _dataManager.SaveGame();
    }

    private void LoadQuestProgress()
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

    private bool IsActiveQuest(QuestType type)
    {
        return CurrentQuest != null && CurrentQuest.Type == type;
    }
    private IEnumerator ProceedToNextQuestRoutine(int completedQuestID, int nextQuestID)
    {
        _questCompletedChannel?.RaiseEvent(completedQuestID);

        yield return new WaitForSeconds(3.5f);
        if(nextQuestID >0)
        {
            StartQuest(nextQuestID);
        }
        else
        {
            CurrentQuest = null;
            CurrentProgress = 0;
            AllQuestsCompleted = true;
            SaveProgress();
            Debug.Log("[Quest] 모든 퀘스트 완료.");
        }
    }
}
