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
    [SerializeField] private PanelFader _settingsPanel;


    private static bool _hasPassedPressAnyKey = false;


    private void Start()
    {
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

            //GameCore.Instance.SoundManager.PlaySFX(클릭효과음);
        }
    }

    #region Main Menu Buttons

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

    public void OnClickSettings()
    {
        _mainMenuPanel.FadeOut();
        _settingsPanel.FadeIn();
    }

    public void OnClickQuit()
    {
        Debug.Log("[Title] 게임 종료");
        _mainMenuPanel.FadeOut();
        Application.Quit();
    }

    #endregion

    #region Settings Panel Buttons

    public void OnClickCloseSettings()
    {
        _settingsPanel.FadeOut();
        _mainMenuPanel.FadeIn();
    }

    #endregion
}