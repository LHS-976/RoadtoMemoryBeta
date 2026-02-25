using UnityEngine;
using UnityEngine.Events;

public class QuestEventInteractable : MonoBehaviour, IInteractable
{
    [Header("Quest Settings")]
    [SerializeField] private string _eventID;
    [SerializeField] private string _prompt = "조사";
    [SerializeField] private bool _isOneShot = true;

    [Header("Broadcasting Channel (SO)")]
    [SerializeField] private StringEventChannelSO _questEventChannel;

    [Header("Events (Optional)")]
    [SerializeField] private UnityEvent _onInteracted;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioClip _interactSFX;

    private bool _isUsed = false;

    public string InteractionPrompt => $"[F] {_prompt}";
    public bool CanInteract => !_isUsed;
    public Transform ObjectTransform => transform;

    public void Interact(GameObject interactor)
    {

        if(_questEventChannel != null)
        {
            _questEventChannel.RaiseEvent(_eventID);
        }
        _onInteracted?.Invoke();

        if (_interactSFX != null) SoundManager.Instance?.PlaySFX(_interactSFX, transform.position);

        if (_isOneShot) _isUsed = true;
    }
}