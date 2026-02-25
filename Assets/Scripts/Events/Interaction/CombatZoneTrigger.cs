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

        //플레이어가 구역에 닿았는지 확인
        if (((1 << other.gameObject.layer) & _targetLayers) != 0)
        {
            _isTriggered = true;

            //퀘스트 매니저에게 구역 진입을 제보
            if (_questEventChannel != null)
            {
                _questEventChannel.RaiseEvent(_enterEventID);
                Debug.Log($"[CombatZone] 퀘스트 이벤트 발생: {_enterEventID}");
            }

            //플레이어에게 무기 사용 허가 신호 송출
            if (_enableCombatChannel != null)
            {
                _enableCombatChannel.RaiseEvent();
                Debug.Log("[CombatZone] 전투 모드 활성화 신호 송출!");
            }
        }
    }
}