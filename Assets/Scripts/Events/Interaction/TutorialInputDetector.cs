using Core;
using UnityEngine;

public class TutorialInputDetector : MonoBehaviour
{
    [Header("Broadcasting Channel")]
    [SerializeField] private StringEventChannelSO _questEventChannel;

    private bool _hasTriggeredSprint = false;
    private bool _hasTriggeredInfo = false;
    private void Update()
    {
        if (_questEventChannel == null) return;
        MoveTutorial();
        RunTutorial();
        InfoTutorial();
    }

    private bool IsQuestActive(string targetEventID)
    {
        if (GameCore.Instance == null || GameCore.Instance.QuestManager == null) return false;
        QuestSO currentQuest = Core.GameCore.Instance.QuestManager.CurrentQuest;

        return currentQuest != null && currentQuest.Type == QuestType.Event && currentQuest.TargetID == targetEventID;
    }
    private void MoveTutorial()
    {
        if (!IsQuestActive("Input_Move")) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            _questEventChannel.RaiseEvent("Input_Move");
        }
    }
    private void RunTutorial()
    {
        if (_hasTriggeredSprint || !IsQuestActive("Input_Sprint")) return;
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            _questEventChannel.RaiseEvent("Input_Sprint");
            _hasTriggeredSprint = true;
        }
    }
    private void InfoTutorial()
    {
        if (_hasTriggeredInfo || !IsQuestActive("Input_Info")) return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            _questEventChannel.RaiseEvent("Input_Info");
            _hasTriggeredInfo = true;
        }
    }
}
