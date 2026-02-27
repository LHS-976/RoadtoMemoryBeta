using UnityEngine;

public class StageStartQuestTrigger : MonoBehaviour
{
    [Header("Quest Settings")]
    [Tooltip("이 씬에 로드되었을 때 자동으로 시작을 요청할 퀘스트 ID")]
    [SerializeField] private int _startQuestID;

    private void Start()
    {
        Invoke(nameof(CallStartQuest), 0.1f);
    }

    private void CallStartQuest()
    {
        if (Core.GameCore.Instance != null && Core.GameCore.Instance.QuestManager != null)
        {
            Core.GameCore.Instance.QuestManager.StartQuest(_startQuestID);
        }
    }
}