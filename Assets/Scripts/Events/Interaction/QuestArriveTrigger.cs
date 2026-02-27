using UnityEngine;

public class QuestArriveTrigger : MonoBehaviour
{
    [Header("Quest Arrive Settings")]
    [SerializeField] private string _arriveTargetName = "Stage02";

    [Tooltip("도착 신호를 보낼 방송 채널")]
    [SerializeField] private StringEventChannelSO _questArriveChannel;

    [Header("Trigger Mode")]
    [SerializeField] private bool _triggerOnStart = true;

    [Header("Layer Settings")]
    [SerializeField] private LayerMask _targetLayer;

    private void Start()
    {
        if (_triggerOnStart)
        {
            Invoke(nameof(SendArriveSignal), 0.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggerOnStart) return;

        if (((1 << other.gameObject.layer) & _targetLayer) != 0)
        {
            SendArriveSignal();

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }

    private void SendArriveSignal()
    {
        if (_questArriveChannel != null)
        {
            _questArriveChannel.RaiseEvent(_arriveTargetName);
        }
    }
}