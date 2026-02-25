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

        //Continue/Load: 저장된 씬과 현재 씬이 같고, 위치가 유효하면 세이브 포인트로 이동
        if (data != null && !string.IsNullOrEmpty(data.LastSceneName) && data.LastSceneName == currentScene)
        {
            Vector3 savedPos = new Vector3(data.PlayerPosX, data.PlayerPosY, data.PlayerPosZ);

            //위치가 (0,0,0)이 아닌 유효한 값일 때만 복구
            if (savedPos.sqrMagnitude > 0.01f)
            {
                CharacterController cc = spawnedPlayer.GetComponent<CharacterController>();

                //CharacterController가 있으면 비활성화 후 위치 이동 (Move 충돌 방지)
                if (cc != null) cc.enabled = false;
                spawnedPlayer.transform.position = savedPos;
                if (cc != null) cc.enabled = true;

                Debug.Log($"[Save] 세이브 포인트({savedPos})로 복구 완료!");
            }
            else
            {
                Debug.Log("[Save] 저장된 위치가 (0,0,0)이므로 기본 스폰 포인트 사용.");
            }
        }
        else
        {
            Debug.Log("[Save] 새 게임 또는 다른 씬이므로 기본 스폰 포인트 사용.");
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