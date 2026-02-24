using Cinemachine;
using Core;
using UnityEngine;

public class LevelSetup : MonoBehaviour
{
    [Header("Level References")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private CinemachineFreeLook _freeLookCamera;

    [Header("Game State")]
    [SerializeField] private GameStateSO _gameState;

    private void Start()
    {
        SpawnSetUp();
        if (_gameState != null) _gameState.SetState(GameState.Gameplay);
        if (Core.GameCore.Instance != null && Core.GameCore.Instance.QuestManager != null)
        {
            if (Core.GameCore.Instance.QuestManager.CurrentQuest == null &&
                !Core.GameCore.Instance.QuestManager.AllQuestsCompleted)
            {
                Debug.Log("[InGameInitializer] 맵 로딩 완료. 첫 퀘스트(또는 세이브 데이터)를 불러옵니다.");
                Core.GameCore.Instance.QuestManager.LoadQuestProgress();
            }
        }
    }
    private void SpawnSetUp()
    {
        GameObject spawnedPlayer = GameCore.Instance.SpawnPlayer(_spawnPoint);

        GameData data = GameCore.Instance.DataManager?.CurrentData;
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (data != null && data.LastSceneName == currentScene)
        {
            Vector3 savedPos = new Vector3(data.PlayerPosX, data.PlayerPosY, data.PlayerPosZ);

            if (savedPos != Vector3.zero)
            {
                spawnedPlayer.transform.position = savedPos;
                Debug.Log($"[Save] 플레이어 위치를 세이브 포인트({savedPos})로 복구했습니다!");
                //UI이벤트 호출.
            }
        }
        if (spawnedPlayer != null && _freeLookCamera != null)
        {
            SetupCamera(spawnedPlayer.transform);
        }
    }

    private void SetupCamera(Transform playerTransform)
    {
        if (_freeLookCamera == null) return;

        Transform target = playerTransform.Find("PlayerRoot");

        if (target == null) target = playerTransform;

        _freeLookCamera.Follow = target;
        _freeLookCamera.LookAt = target;
        _freeLookCamera.m_XAxis.Value = playerTransform.eulerAngles.y + 180f;

        _freeLookCamera.m_YAxis.Value = 0.5f;

        _freeLookCamera.PreviousStateIsValid = false;
    }
}
