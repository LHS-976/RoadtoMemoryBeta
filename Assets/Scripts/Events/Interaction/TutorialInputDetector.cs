using UnityEngine;

public class TutorialInputDetector : MonoBehaviour
{
    [Header("Broadcasting Channel")]
    [SerializeField] private StringEventChannelSO _questEventChannel;

    private void Update()
    {
        if (_questEventChannel == null) return;
        MoveTutorial();
        RunTutorial();
    }
    private void MoveTutorial()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            _questEventChannel.RaiseEvent("Input_Move");
        }
    }
    private void RunTutorial()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            _questEventChannel.RaiseEvent("Input_Sprint");
        }
    }
}
