using UnityEngine;

public class DoorAnimatorHelper : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _boolParameterName = "Opened";

    private void Awake()
    {
        if (_animator == null) _animator = GetComponent<Animator>();
    }
    public void OpenDoor()
    {
        _animator.SetBool(_boolParameterName, true);
    }

    public void ClosedDoor()
    {
        _animator.SetBool(_boolParameterName, false);
    }
}
