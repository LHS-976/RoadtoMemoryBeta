using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ForceQuestStartTrigger : MonoBehaviour
{
    [Header("Quest Start Settings")]
    [Tooltip("강제로 시작할 퀘스트의 ID 번호")]
    [SerializeField] private int _questIDToStart;

    [Tooltip("퀘스트 강제 시작 명령을 수신하는 채널 (ForceStartQuestChannel 연결)")]
    [SerializeField] private IntEventChannelSO _forceStartQuestChannel;

    [Header("Trigger Settings")]
    [Tooltip("트리거를 밟을 수 있는 대상 (Player 레이어 선택)")]
    [SerializeField] private LayerMask _playerLayer;

    [Tooltip("체크하면 한 번만 실행되고 다시 밟아도 무시됩니다.")]
    [SerializeField] private bool _isOneShot = true;

    private bool _hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered && _isOneShot) return;

        if (((1 << other.gameObject.layer) & _playerLayer) != 0)
        {
            if (_forceStartQuestChannel != null)
            {
                _forceStartQuestChannel.RaiseEvent(_questIDToStart);
                Debug.Log($"[Trigger] {_questIDToStart}번 퀘스트 강제 시작 명령 송출!");

                _hasTriggered = true;
            }
            else
            {
                Debug.LogWarning("[Trigger] ForceStartQuestChannel이 연결되지 않았습니다!", this);
            }
        }
    }
}