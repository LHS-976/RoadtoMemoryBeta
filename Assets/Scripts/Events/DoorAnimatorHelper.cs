using UnityEngine;

public class DoorAnimatorHelper : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _TriggerParameterName = "Opened";

    private void Awake()
    {
        if (_animator == null) _animator = GetComponent<Animator>();
    }
    public void OpenDoor()
    {
        _animator.SetBool(_TriggerParameterName, true);
    }

    public void ClosedDoor()
    {
        _animator.SetBool(_TriggerParameterName, false);
    }
}
