using Core;
using UnityEngine;

public class TitleUIManager : MonoBehaviour
{
    [Header("Scene Routing")]
    [SerializeField] private GameSceneEventChannelSO _loadSceneChannel;
    [SerializeField] private GameSceneSO _firstStageScene;

    [Header("UI Panels")]
    [SerializeField] private PanelFader _pressAnyKeyPanel;
    [SerializeField] private PanelFader _mainMenuPanel;
    [SerializeField] private PanelFader _gameModePanel;
    [SerializeField] private PanelFader _settingsPanel;
    [SerializeField] private PanelFader _quitConfirmPanel;

    private PanelFader _currentPanel;
    private static bool _hasPassedPressAnyKey = false;


    private void Start()
    {
        _gameModePanel?.SetImmediateClosed();
        _settingsPanel?.SetImmediateClosed();
        _quitConfirmPanel?.SetImmediateClosed();

        if (_settingsPanel != null) _settingsPanel.SetImmediateClosed();

        if (_hasPassedPressAnyKey)
        {
            if (_pressAnyKeyPanel != null) _pressAnyKeyPanel.SetImmediateOpened();
            if (_mainMenuPanel != null) _mainMenuPanel.SetImmediateClosed();
        }
        else
        {
            if (_pressAnyKeyPanel != null) _pressAnyKeyPanel.SetImmediateClosed();
            if (_mainMenuPanel != null) _mainMenuPanel.SetImmediateOpened();
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
        Debug.Log("[Title] 새 게임 시작!");
        GameCore.Instance.DataManager.DeleteSaveData();
        GameCore.Instance.QuestManager.ResetForNewGame();
        _mainMenuPanel.FadeOut();
        if (_loadSceneChannel != null && _firstStageScene != null)
        {
            _loadSceneChannel.RaiseEvent(_firstStageScene);
        }
    }

    public void OnClickContinue()
    {
        if (GameCore.Instance.DataManager.CurrentData != null)
        {
            Debug.Log("[Title] 이어하기!");
            _mainMenuPanel.FadeOut();
            _loadSceneChannel.RaiseEvent(_firstStageScene);
        }
        else
        {
            Debug.LogWarning("[Title] 저장된 데이터가 없습니다!");
            //"저장된 데이터가 없습니다" 팝업(PanelFader)을 FadeIn() 추가.
        }
    }
    public void OnClickLoad()
    {
        //로드 슬롯 UI구현 시 확장
        OnClickContinue();
    }
    public void OnClickBackFromGameMode()
    {
        BackToMainMenu();
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
        Debug.Log("[Title] 게임 종료");
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
}