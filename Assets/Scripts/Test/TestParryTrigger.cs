using Core;
using UnityEngine;

public class TestParryTrigger : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private Transform _headPos;

    private void Start()
    {
        if (_headPos == null) _headPos = transform;
    }

    private void Update()
    {
        // [T] 키를 누르면 -> 패리 UI 켜기 (아머 파괴 시뮬레이션)
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Test: 패리 기회 발생!");
            GameEventManager.TriggerExecutionWindowOpen(_headPos);
        }

        // [Y] 키를 누르면 -> 패리 UI 끄기 (시간 초과/성공 시뮬레이션)
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("Test: 패리 기회 종료!");
            GameEventManager.TriggerExecutionWindowClose(_headPos);
        }
    }
}