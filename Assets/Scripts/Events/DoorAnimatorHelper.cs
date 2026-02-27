using UnityEngine;

public class DoorAnimatorHelper : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _TriggerParameterName = "Opened";

    [SerializeField] private AudioClip _openSound;

    private void Awake()
    {
        if (_animator == null) _animator = GetComponent<Animator>();
    }
    public void OpenDoor()
    {
        if (_openSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFXAttached(_openSound, transform);
        _animator.SetBool(_TriggerParameterName, true);
    }

    public void ClosedDoor()
    {
        _animator.SetBool(_TriggerParameterName, false);
    }
}
