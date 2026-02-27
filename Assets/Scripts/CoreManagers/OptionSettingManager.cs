using Core;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionSettingsManager : MonoBehaviour
{
    public static OptionSettingsManager Instance { get; private set; }

    #region Tab UI
    [Header("Tab Buttons")]
    [SerializeField] private Button _soundTabButton;
    [SerializeField] private Button _cameraTabButton;
    [SerializeField] private Button _displayTabButton;
    [SerializeField] private Button _closeButton;

    [Header("Tab Panels")]
    [SerializeField] private PanelFader _soundContent;
    [SerializeField] private PanelFader _cameraContent;
    [SerializeField] private PanelFader _displayContent;
    #endregion

    #region Sound UI
    [Header("Sound Sliders")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Slider _uiSlider;

    [Header("Sound Value Texts")]
    [SerializeField] private TextMeshProUGUI _masterValueText;
    [SerializeField] private TextMeshProUGUI _bgmValueText;
    [SerializeField] private TextMeshProUGUI _sfxValueText;
    [SerializeField] private TextMeshProUGUI _uiValueText;
    #endregion

    #region Camera UI
    [Header("Camera Settings - Horizontal (X)")]
    [SerializeField] private Slider _horizontalSensitivitySlider;
    [SerializeField] private TextMeshProUGUI _horizontalSensitivityText;

    [Header("Camera Settings - Vertical (Y)")]
    [SerializeField] private Slider _verticalSensitivitySlider;
    [SerializeField] private TextMeshProUGUI _verticalSensitivityText;

    [Header("System Buttons")]
    [SerializeField] private Button _quitGameButton;

    public static float HorizontalSensitivity { get; private set; } = 1.0f;
    public static float VerticalSensitivity { get; private set; } = 1.0f;

    public event Action OnOptionClosed;
    #endregion

    #region Display UI
    [Header("Display")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private Button _applyDisplayButton;
    #endregion

    [Header("UI Sounds")]
    [SerializeField] private AudioClip _clickSound;
    [SerializeField] private AudioClip _cancelSound;
    [SerializeField] private AudioClip _panelOpenSound;

    private readonly Vector2Int[] _resolutions = new Vector2Int[]
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
    };
    private int _selectedResolutionIndex;

    [SerializeField] private GameStateSO _gameState;
    [SerializeField] private PanelFader _optionPanel;

    private bool _isOpen = false;

    #region Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadAllSettings();
    }

    private void Start()
    {
        _soundContent?.SetImmediateClosed();
        _cameraContent?.SetImmediateClosed();
        _displayContent?.SetImmediateClosed();

        _soundTabButton?.onClick.AddListener(ShowSoundTab);
        _cameraTabButton?.onClick.AddListener(ShowCameraTab);
        _displayTabButton?.onClick.AddListener(ShowDisplayTab);

        //닫기 버튼 연동
        _closeButton?.onClick.AddListener(OnClickClose);

        InitializeSoundSliders();
        InitializeCameraUI();
        InitializeDisplayUI();

        if (_quitGameButton != null)
            _quitGameButton.onClick.AddListener(OnClickQuitGame);
    }

    //GameState 변화 구독 (InGameUIManager와 동일한 원리)
    private void OnEnable()
    {
        if (_gameState != null) _gameState.OnStateChange += HandleStateChange;
    }

    private void OnDisable()
    {
        if (_gameState != null) _gameState.OnStateChange -= HandleStateChange;
    }

    private void HandleStateChange(GameState state)
    {
        //GameCore가 ESC를 눌러 State를 Option으로 바꾸면 UI를 켭니다.
        if (state == GameState.Option)
        {
            ShowUI();
        }
        //GameCore가 ResumeGame()을 호출해 State를 Gameplay로 바꾸면 UI를 끕니다.
        else if (state == GameState.Gameplay)
        {
            HideUI();
        }
    }

    #endregion

    #region Open & Close Logic

    //외부(타이틀 화면 등)에서 옵션창을 열고 싶을 때 호출
    public void OpenOptions()
    {
        if (_panelOpenSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlayUI(_panelOpenSound);

        if (_gameState != null && _gameState.CurrentState == GameState.Title)
        {
            // 타이틀 씬에서는 State 변경 없이 UI만 켭니다.
            ShowUI();
        }
        else if (Core.GameCore.Instance != null)
        {
            //인게임에서는 GameCore에게 일시정지 요청
            //GameCore가 State를 Option으로 바꾸면 HandleStateChange가 작동해 창이 열림
            Core.GameCore.Instance.TogglePause();
        }
    }

    private void OnClickClose()
    {
        if (_panelOpenSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlayUI(_cancelSound);

        if (_gameState != null && _gameState.CurrentState == GameState.Title)
        {
            //타이틀 씬에서 UI닫기.
            HideUI();
        }
        else if (Core.GameCore.Instance != null)
        {
            Core.GameCore.Instance.ResumeGame();
        }
    }

    //실제 패널을 켜는 로직
    private void ShowUI()
    {
        if (_isOpen) return;
        _isOpen = true;
        _optionPanel?.FadeIn();
        ShowSoundTab();
    }

    //실제 패널을 끄는 로직
    private void HideUI()
    {
        if (!_isOpen) return;
        _isOpen = false;

        SaveAllSettings(); //닫을 때 저장

        _soundContent?.SetImmediateClosed();
        _cameraContent?.SetImmediateClosed();
        _displayContent?.SetImmediateClosed();
        _optionPanel?.FadeOut();

        OnOptionClosed?.Invoke();
    }

    #endregion

    #region Tab Switch
    public void ShowSoundTab()
    {
        PlayClickSound();
        _soundContent?.SetImmediateOpened();
        _cameraContent?.SetImmediateClosed();
        _displayContent?.SetImmediateClosed();
    }
    public void ShowCameraTab()
    {
        PlayClickSound();
        _soundContent?.SetImmediateClosed();
        _cameraContent?.SetImmediateOpened();
        _displayContent?.SetImmediateClosed();
    }
    public void ShowDisplayTab()
    {
        PlayClickSound();
        _soundContent?.SetImmediateClosed();
        _cameraContent?.SetImmediateClosed();
        _displayContent?.SetImmediateOpened();
    }
    #endregion


    #region Sound
    private void InitializeSoundSliders()
    {
        if (SoundManager.Instance == null) return;
        if (_masterSlider != null) { _masterSlider.value = SoundManager.Instance.GetMasterVolume(); _masterSlider.onValueChanged.AddListener(OnMasterChanged); UpdateVolumeText(_masterValueText, _masterSlider.value); }
        if (_bgmSlider != null) { _bgmSlider.value = SoundManager.Instance.GetBGMVolume(); _bgmSlider.onValueChanged.AddListener(OnBGMChanged); UpdateVolumeText(_bgmValueText, _bgmSlider.value); }
        if (_sfxSlider != null) { _sfxSlider.value = SoundManager.Instance.GetSFXVolume(); _sfxSlider.onValueChanged.AddListener(OnSFXChanged); UpdateVolumeText(_sfxValueText, _sfxSlider.value); }
        if (_uiSlider != null) { _uiSlider.value = SoundManager.Instance.GetUIVolume(); _uiSlider.onValueChanged.AddListener(OnUIChanged); UpdateVolumeText(_uiValueText, _uiSlider.value); }
    }
    private void OnMasterChanged(float value) { SoundManager.Instance?.SetMasterVolume(value); UpdateVolumeText(_masterValueText, value); }
    private void OnBGMChanged(float value) { SoundManager.Instance?.SetBGMVolume(value); UpdateVolumeText(_bgmValueText, value); }
    private void OnSFXChanged(float value) { SoundManager.Instance?.SetSFXVolume(value); UpdateVolumeText(_sfxValueText, value); }
    private void OnUIChanged(float value) { SoundManager.Instance?.SetUIVolume(value); UpdateVolumeText(_uiValueText, value); }
    private void UpdateVolumeText(TextMeshProUGUI text, float value) { if (text != null) text.text = $"{Mathf.RoundToInt(value * 100)}"; }
    #endregion

    #region Camera (Mouse Sensitivity)
    private void InitializeCameraUI()
    {
        if (_horizontalSensitivitySlider != null) { _horizontalSensitivitySlider.minValue = 0.1f; _horizontalSensitivitySlider.maxValue = 3.0f; _horizontalSensitivitySlider.value = HorizontalSensitivity; _horizontalSensitivitySlider.onValueChanged.AddListener(OnHorizontalSensitivityChanged); UpdateSensitivityText(_horizontalSensitivityText, HorizontalSensitivity); }
        if (_verticalSensitivitySlider != null) { _verticalSensitivitySlider.minValue = 0.1f; _verticalSensitivitySlider.maxValue = 3.0f; _verticalSensitivitySlider.value = VerticalSensitivity; _verticalSensitivitySlider.onValueChanged.AddListener(OnVerticalSensitivityChanged); UpdateSensitivityText(_verticalSensitivityText, VerticalSensitivity); }
    }
    private void OnHorizontalSensitivityChanged(float value) { HorizontalSensitivity = value; UpdateSensitivityText(_horizontalSensitivityText, value); }
    private void OnVerticalSensitivityChanged(float value) { VerticalSensitivity = value; UpdateSensitivityText(_verticalSensitivityText, value); }
    private void UpdateSensitivityText(TextMeshProUGUI text, float value) { if (text != null) text.text = value.ToString("F1"); }
    #endregion

    #region Display
    private void InitializeDisplayUI()
    {
        if (_resolutionDropdown != null)
        {
            _resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            int currentIndex = 0;
            for (int i = 0; i < _resolutions.Length; i++) { options.Add($"{_resolutions[i].x} x {_resolutions[i].y}"); if (Screen.width == _resolutions[i].x && Screen.height == _resolutions[i].y) currentIndex = i; }
            _resolutionDropdown.AddOptions(options); _resolutionDropdown.value = currentIndex; _selectedResolutionIndex = currentIndex;
            _resolutionDropdown.onValueChanged.AddListener((index) => _selectedResolutionIndex = index);
        }
        if (_fullscreenToggle != null) _fullscreenToggle.isOn = Screen.fullScreen;
        if (_applyDisplayButton != null) _applyDisplayButton.onClick.AddListener(ApplyDisplaySettings);
    }
    private void ApplyDisplaySettings()
    {
        Vector2Int res = _resolutions[_selectedResolutionIndex];
        bool fullscreen = _fullscreenToggle != null ? _fullscreenToggle.isOn : Screen.fullScreen;
        Screen.SetResolution(res.x, res.y, fullscreen);
        PlayerPrefs.SetInt("ResolutionIndex", _selectedResolutionIndex);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
    }
    #endregion

    #region Save / Load
    private void SaveAllSettings()
    {
        SoundManager.Instance?.SaveVolumeSettings();
        PlayerPrefs.SetFloat("MouseSensitivityX", HorizontalSensitivity);
        PlayerPrefs.SetFloat("MouseSensitivityY", VerticalSensitivity);
        PlayerPrefs.SetInt("ResolutionIndex", _selectedResolutionIndex);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }
    private void LoadAllSettings()
    {
        HorizontalSensitivity = PlayerPrefs.GetFloat("MouseSensitivityX", 1.0f);
        VerticalSensitivity = PlayerPrefs.GetFloat("MouseSensitivityY", 1.0f);
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            int resIndex = PlayerPrefs.GetInt("ResolutionIndex", 2);
            bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            if (resIndex >= 0 && resIndex < _resolutions.Length)
            {
                Vector2Int res = _resolutions[resIndex];
                Screen.SetResolution(res.x, res.y, fullscreen);
                _selectedResolutionIndex = resIndex;
            }
        }
    }
    private void OnClickQuitGame()
    {
        PlayClickSound();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion
    public void PlayClickSound()
    {
        if (_clickSound != null) SoundManager.Instance.PlayUI(_clickSound);
    }
}