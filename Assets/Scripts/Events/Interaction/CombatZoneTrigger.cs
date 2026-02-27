using UnityEngine;

public class CombatZoneTrigger : MonoBehaviour
{
    [Header("Quest Event")]
    [SerializeField] private StringEventChannelSO _questEventChannel;
    [SerializeField] private string _enterEventID = "Enter_TrainingGround";

    [Header("Can Combat State")]
    [SerializeField] private VoidEventChannelSO _enableCombatChannel;

    [Header("Settings")]
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private bool _isOneShot = true;

    private bool _isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isTriggered && _isOneShot) return;

        if (((1 << other.gameObject.layer) & _targetLayers) != 0)
        {
            _isTriggered = true;
            if (_questEventChannel != null)
            {
                _questEventChannel.RaiseEvent(_enterEventID);
            }

            if (_enableCombatChannel != null)
            {
                _enableCombatChannel.RaiseEvent();
            }
        }
    }
}