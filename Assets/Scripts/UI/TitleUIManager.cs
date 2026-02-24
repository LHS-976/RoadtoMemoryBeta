using Core;
using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class TitleUIManager : MonoBehaviour
{
    [Header("Scene Routing")]
    [SerializeField] private GameSceneEventChannelSO _loadSceneChannel;
    [SerializeField] private GameSceneSO _firstStageScene;
    [SerializeField] private GameStateSO _gameState;
    [SerializeField] private SaveSlotUIManager _slotManager;

    [Header("UI Panels")]
    [SerializeField] private PanelFader _pressAnyKeyPanel;
    [SerializeField] private PanelFader _mainMenuPanel;
    [SerializeField] private PanelFader _gameModePanel;
    [SerializeField] private PanelFader _settingsPanel;
    [SerializeField] private PanelFader _quitConfirmPanel;

    [Header("Cinematic Video")]
    [SerializeField] private PanelFader _videoPanel;
    [SerializeField] private VideoPlayer _videoPlayer;

    [Header("Scene Transition")]
    [SerializeField] private PanelFader _sceneFader;
    
    private const float _fadeWaitTime = 0.5f;

    private bool _isPlayingVideo = false;

    private PanelFader _currentPanel;
    private static bool _hasPassedPressAnyKey = false;


    private void Awake()
    {
        if (_gameState == null) _gameState = GetComponent<GameStateSO>();

        if(_gameState != null)
        {
            _gameState.SetState(GameState.Title);
        }
    }

    private void Start()
    {
        _gameModePanel?.SetImmediateClosed();
        _settingsPanel?.SetImmediateClosed();
        _quitConfirmPanel?.SetImmediateClosed();
        _videoPanel?.SetImmediateClosed();

        if (!_hasPassedPressAnyKey)
        {
            if (_pressAnyKeyPanel != null) _pressAnyKeyPanel.SetImmediateOpened();
            if (_mainMenuPanel != null) _mainMenuPanel.SetImmediateClosed();
            if (_settingsPanel != null) _settingsPanel.SetImmediateClosed();
        }
        else
        {
            if (_pressAnyKeyPanel != null) _pressAnyKeyPanel.SetImmediateClosed();
            if (_mainMenuPanel != null) _mainMenuPanel.SetImmediateOpened();
            _currentPanel = _mainMenuPanel;
        }
    }
    private void Update()
    {
        if (!_hasPassedPressAnyKey && Input.anyKeyDown)
        {
            _hasPassedPressAnyKey = true;
            if (_pressAnyKeyPanel != null) _pressAnyKeyPanel.FadeOut();
            if (_mainMenuPanel != null) _mainMenuPanel.FadeIn();
            _currentPanel = _mainMenuPanel;
            //GameCore.Instance.SoundManager.PlaySFX(클릭효과음);
        }
        if (_isPlayingVideo && Input.anyKeyDown)
        {
            SkipVideo();
        }
    }
    #region Panel Navigation
    private void SwitchPanel(PanelFader next)
    {
        _currentPanel?.FadeOut();
        next?.FadeIn();
        _currentPanel = next;
    }
    private void BackToMainMenu()
    {
        SwitchPanel(_mainMenuPanel);
    }
    #endregion

    #region Main Menu Buttons

    public void OnClickStart()
    {
        SwitchPanel(_gameModePanel);
    }
    public void OnClickOption()
    {
        SwitchPanel(_settingsPanel);
    }
    public void OnClickExit()
    {
        SwitchPanel(_quitConfirmPanel);
    }
    #endregion

    #region GameMode Panel Buttons(New/Continue/Load)
    public void OnClickNewGame()
    {
        Debug.Log("새 게임 시작!");
        if (_slotManager != null) _slotManager.OpenSlotMenu(SlotMenuMode.NewGame);
        _currentPanel?.FadeOut();
    }

    public void OnClickContinue()
    {
        int latestSlot = GameCore.Instance.DataManager.GetLatestSaveSlot();

        if (latestSlot != -1)
        {
            Debug.Log($"[Title] 최근 세이브(슬롯 {latestSlot + 1})를 이어서 시작합니다!");
            GameCore.Instance.DataManager.LoadGame(latestSlot);

            _currentPanel?.FadeOut();
        }
        else
        {
            Debug.LogWarning("[Title] 저장된 데이터가 전혀 없습니다! 새 게임 창으로 유도합니다.");
            if (_slotManager != null) _slotManager.OpenSlotMenu(SlotMenuMode.NewGame);
        }
    }
    public void OnClickLoad()
    {
        Debug.Log("불러오기 슬롯 선택 창 열기");
        if (_slotManager != null) _slotManager.OpenSlotMenu(SlotMenuMode.LoadGame);
    }
    public void OnClickBackFromGameMode()
    {
        BackToMainMenu();
    }
    public void PlayCinematic()
    {
        _currentPanel?.FadeOut();
        if(_videoPanel != null && _videoPlayer != null)
        {
            StartCoroutine(PlayCinematicRoutine());
        }
        else
        {
            StartCoroutine(TransitionToLoadScene());
        }
    }
    #endregion


    //버튼 이벤트 추가예정
    #region Settings Panel Buttons
    public void OnClickCloseSettings()
    {
        BackToMainMenu();
    }
    #endregion

    #region Quit Confirm Panel Buttons

    public void OnClickQuitConfirm()
    {
        Debug.Log("게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void OnClickQuitCancel()
    {
        BackToMainMenu();
    }
    #endregion

    #region Scene Loading

    private void LoadTargetScene()
    {
        _currentPanel?.FadeOut();
        _currentPanel = null;

        if (_loadSceneChannel != null && _firstStageScene != null)
        {
            _loadSceneChannel.RaiseEvent(_firstStageScene);
        }
    }
    #endregion

    #region Video
    private void OnVideoFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoFinished;
        SkipVideo();
    }
    private void SkipVideo()
    {
        if (!_isPlayingVideo) return;
        _isPlayingVideo = false;

        _videoPlayer.Stop();
        StartCoroutine(TransitionToLoadScene());
    }
    private IEnumerator PlayCinematicRoutine()
    {
        if(_sceneFader != null)
        {
            _sceneFader.FadeIn();
            yield return new WaitForSeconds(_fadeWaitTime);
        }
        _isPlayingVideo = true;
        _videoPanel.FadeIn();
        _videoPlayer.Prepare();
        while(!_videoPlayer.isPrepared)
        {
            yield return null;
        }
        _videoPlayer.loopPointReached += OnVideoFinished;
        _videoPlayer.Play();

        if(_sceneFader != null)
        {
            _sceneFader.FadeOut();
        }
    }
    private IEnumerator TransitionToLoadScene()
    {
        if(_sceneFader != null)
        {
            _sceneFader.FadeIn();
            yield return new WaitForSeconds(_fadeWaitTime);
        }
        LoadTargetScene();
    }
    #endregion
}