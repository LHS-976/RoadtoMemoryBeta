using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Listening Channel (SO)")]
    [SerializeField] private StringEventChannelSO _promptChannel;

    private void OnEnable()
    {
        if (_promptChannel != null)
        {
            _promptChannel.OnEventRaised += HandlePromptChanged;
        }
    }
    private void OnDisable()
    {
        if(_promptChannel != null)
        {
            _promptChannel.OnEventRaised -= HandlePromptChanged;
        }
    }

    private void Start()
    {
        Hide();
    }

    private void HandlePromptChanged(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
        {
            Hide();
        }
        else
        {
            Show(prompt);
        }
    }

    private void Show(string text)
    {
        _promptText.text = text;
        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
    }

    private void Hide()
    {
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;
    }
}
