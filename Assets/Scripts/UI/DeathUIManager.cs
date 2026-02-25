using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Core;

public class DeathUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private PanelFader _screenFader;
    [SerializeField] private PanelFader _deathPanel;

    [Header("Buttons")]
    [SerializeField] private Button _respawnButton;
    [SerializeField] private Button _titleButton;

    [Header("Channels")]
    [SerializeField] private VoidEventChannelSO _deathChannel;
    [SerializeField] private GameSceneEventChannelSO _loadSceneChannel;

    [Header("Scene Data")]
    [SerializeField] private GameSceneSO _titleScene;
    [SerializeField] private GameSceneSO[] _allScenes;

    [Header("GameState")]
    [SerializeField] private GameStateSO _gameState;

    [Header("Timing")]
    [SerializeField] private float _deathAnimWait = 2.0f;
    [SerializeField] private float _fadeWait = 0.5f;

    private void Start()
    {
        _deathPanel?.SetImmediateClosed();

        if (_respawnButton != null)
            _respawnButton.onClick.AddListener(OnClickRespawn);
        if (_titleButton != null)
            _titleButton.onClick.AddListener(OnClickTitle);
    }

    private void OnEnable()
    {
        if (_deathChannel != null)
            _deathChannel.OnEventRaised += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        if (_deathChannel != null)
            _deathChannel.OnEventRaised -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSecondsRealtime(_deathAnimWait);

        if (_screenFader != null)
        {
            _screenFader.FadeIn();
            yield return new WaitForSecondsRealtime(_fadeWait);
        }

        //커서 표시
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        _deathPanel?.FadeIn();

        bool hasSave = GameCore.Instance?.DataManager != null
                    && GameCore.Instance.DataManager.CurrentSlotIndex >= 0;

        if (_respawnButton != null)
            _respawnButton.interactable = hasSave;
    }

    private void OnClickRespawn()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        _deathPanel?.FadeOut();
        yield return new WaitForSecondsRealtime(_fadeWait);

        DataManager dataManager = GameCore.Instance?.DataManager;
        if (dataManager == null || dataManager.CurrentSlotIndex < 0)
        {
            Debug.LogError("[Death] 저장된 슬롯이 없습니다!");
            yield break;
        }

        dataManager.LoadGame(dataManager.CurrentSlotIndex);
        GameData data = dataManager.CurrentData;

        //커서 숨기기
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //TimeManager 복구
        if (GameCore.Instance.TimeManager != null)
            GameCore.Instance.TimeManager.ForceRestoreTime();

        //기존 플레이어 정리
        if (GameCore.Instance.CurrentPlayer != null)
            Destroy(GameCore.Instance.CurrentPlayer);

        //SceneRouter를 통해 로딩씬 거쳐서 이동
        if (_loadSceneChannel != null && !string.IsNullOrEmpty(data.LastSceneName))
        {
            GameSceneSO targetScene = FindSceneSOByName(data.LastSceneName);
            if (targetScene != null)
            {
                _loadSceneChannel.RaiseEvent(targetScene);
            }
            else
            {
                Debug.LogWarning($"[Death] {data.LastSceneName} SO를 찾을 수 없습니다. 직접 로드합니다.");
                SceneManager.LoadScene(data.LastSceneName);
            }
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void OnClickTitle()
    {
        StartCoroutine(TitleRoutine());
    }

    private IEnumerator TitleRoutine()
    {
        _deathPanel?.FadeOut();
        yield return new WaitForSecondsRealtime(_fadeWait);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (GameCore.Instance?.TimeManager != null)
            GameCore.Instance.TimeManager.ForceRestoreTime();

        if (GameCore.Instance?.CurrentPlayer != null)
            Destroy(GameCore.Instance.CurrentPlayer);

        if (_loadSceneChannel != null && _titleScene != null)
        {
            _loadSceneChannel.RaiseEvent(_titleScene);
        }
        else
        {
            SceneManager.LoadScene("01_TitleScene");
        }
    }

    private GameSceneSO FindSceneSOByName(string sceneName)
    {
        if (_allScenes == null) return null;

        foreach (var scene in _allScenes)
        {
            if (scene.sceneName == sceneName)
                return scene;
        }
        return null;
    }
}