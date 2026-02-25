using UnityEngine;

public class QuestZoneTrigger : MonoBehaviour
{
    [Header("Quest Event")]
    [Tooltip("퀘스트 이벤트를 제보할 방송국")]
    [SerializeField] private StringEventChannelSO _questEventChannel;

    [Tooltip("퀘스트 SO에 적어둔 Target ID")]
    [SerializeField] private string _zoneEventID;

    [Header("Settings")]
    [Tooltip("감지할 대상의 레이어 (Player)")]
    [SerializeField] private LayerMask _targetLayers;
    [Tooltip("한 번만 작동할지 여부")]
    [SerializeField] private bool _isOneShot = true;

    private bool _hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered && _isOneShot) return;

        if (((1 << other.gameObject.layer) & _targetLayers) != 0)
        {
            _hasTriggered = true;

            if (_questEventChannel != null)
            {
                _questEventChannel.RaiseEvent(_zoneEventID);
                Debug.Log($"[QuestZone] 구역 도달 이벤트 제보: {_zoneEventID}");
            }
        }
    }
}