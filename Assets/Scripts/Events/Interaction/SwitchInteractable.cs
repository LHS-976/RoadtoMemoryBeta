using UnityEngine;
using UnityEngine.Events;

public class SwitchInteractable : MonoBehaviour,IInteractable
{
    [Header("Settings")]
    [SerializeField] private string _prompt = "작동";
    [SerializeField] private bool _isOneShot = false;

    [Header("Events")]
    [SerializeField] private UnityEvent _onActivated;
    [SerializeField] private UnityEvent _onDeactivated;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioClip _activateSFX;

    private bool _isActive = false;
    private bool _isUsed = false;

    public string InteractionPrompt => $"[F] {_prompt}";
    public bool CanInteract => !_isUsed;

    public Transform ObjectTransform => transform;

    public void Interact(GameObject interactor)
    {
        _isActive = !_isActive;

        if (_isActive) _onActivated?.Invoke();
        else _onDeactivated?.Invoke();

        if (_activateSFX != null) SoundManager.Instance?.PlaySFX(_activateSFX, transform.position);

        if (_isOneShot) _isUsed = true;
    }
}
