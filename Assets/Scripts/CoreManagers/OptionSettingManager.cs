using Core;
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
    [SerializeField] private Button _keyBindTabButton;
    [SerializeField] private Button _displayTabButton;
    [SerializeField] private Button _closeButton;

    [Header("Tab Panels (PanelFader + CanvasGroup)")]
    [SerializeField] private PanelFader _soundContent;
    [SerializeField] private PanelFader _keyBindContent;
    [SerializeField] private PanelFader _displayContent;

    #endregion

    #region Sound UI

    [Header("Sound Sliders")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Slider _uiSlider;

    [Header("Sound Value Texts (Optional)")]
    [SerializeField] private TextMeshProUGUI _masterValueText;
    [SerializeField] private TextMeshProUGUI _bgmValueText;
    [SerializeField] private TextMeshProUGUI _sfxValueText;
    [SerializeField] private TextMeshProUGUI _uiValueText;

    #endregion

    #region KeyBind UI

    [Header("KeyBind Buttons (텍스트가 현재 키를 표시)")]
    [SerializeField] private Button _keyCombatToggle;
    [SerializeField] private Button _keyExecution;
    [SerializeField] private Button _keyInteract;
    [SerializeField] private Button _keyDodge;
    [SerializeField] private Button _keySprint;
    [SerializeField] private Button _keyPlayerInfo;
    [SerializeField] private Button _keyQuestToggle;

    [Header("KeyBind Waiting Overlay (Optional)")]
    [SerializeField] private PanelFader _keyWaitOverlay;
    [SerializeField] private TextMeshProUGUI _keyWaitText;

    #endregion

    #region Display UI

    [Header("Display")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private Button _applyDisplayButton;

    #endregion

    //키 바인딩 데이터
    private static Dictionary<string, KeyCode> _keyBindings = new Dictionary<string, KeyCode>();
    private string _waitingForAction = null;
    private bool _isWaitingForKey = false;

    //해상도 데이터
    private readonly Vector2Int[] _resolutions = new Vector2Int[]
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
    };
    private int _selectedResolutionIndex;

    //기본 키 설정
    private static readonly Dictionary<string, KeyCode> DEFAULT_KEYS = new Dictionary<string, KeyCode>()
    {
        { "CombatToggle", KeyCode.X },
        { "Execution",    KeyCode.Q },
        { "Interact",     KeyCode.F },
        { "Dodge",        KeyCode.Space },
        { "Sprint",       KeyCode.LeftShift },
        { "PlayerInfo",   KeyCode.I },
        { "QuestToggle",  KeyCode.F1 },
    };
    [SerializeField] private GameStateSO _gameState;
    [SerializeField] private PanelFader _optionPanel;

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
        //탭 패널 초기 상태
        _soundContent?.SetImmediateClosed();
        _keyBindContent?.SetImmediateClosed();
        _displayContent?.SetImmediateClosed();
        _keyWaitOverlay?.SetImmediateClosed();

        //탭 버튼
        _soundTabButton?.onClick.AddListener(ShowSoundTab);
        _keyBindTabButton?.onClick.AddListener(ShowKeyBindTab);
        _displayTabButton?.onClick.AddListener(ShowDisplayTab);
        _closeButton?.onClick.AddListener(OnClickClose);

        //사운드 슬라이더
        InitializeSoundSliders();

        //키 바인딩 버튼
        InitializeKeyBindButtons();

        //해상도
        InitializeDisplayUI();
    }

    private void Update()
    {
        if (_isWaitingForKey)
        {
            DetectKeyInput();
        }
    }

    #endregion

    #region Tab Switch

    public void ShowSoundTab()
    {
        _soundContent?.SetImmediateOpened();
        _keyBindContent?.SetImmediateClosed();
        _displayContent?.SetImmediateClosed();
    }

    public void ShowKeyBindTab()
    {
        _soundContent?.SetImmediateClosed();
        _keyBindContent?.SetImmediateOpened();
        _displayContent?.SetImmediateClosed();
        RefreshKeyBindUI();
    }

    public void ShowDisplayTab()
    {
        _soundContent?.SetImmediateClosed();
        _keyBindContent?.SetImmediateClosed();
        _displayContent?.SetImmediateOpened();
    }

    /// <summary>
    /// 외부에서 옵션 패널을 열 때 호출.
    /// </summary>
    public void OpenOptions()
    {
        _optionPanel?.FadeIn();
        ShowSoundTab();
    }

    private void OnClickClose()
    {
        SaveAllSettings();

        _soundContent?.SetImmediateClosed();
        _keyBindContent?.SetImmediateClosed();
        _displayContent?.SetImmediateClosed();
        _optionPanel?.FadeOut();

        if (_gameState != null && _gameState.CurrentState == GameState.Title) return;

        //GameCore에서 ResumeGame 호출하도록 연결
        if (Core.GameCore.Instance != null)
            Core.GameCore.Instance.ResumeGame();
    }

    #endregion

    #region Sound

    private void InitializeSoundSliders()
    {
        if (SoundManager.Instance == null) return;

        //초기값
        if (_masterSlider != null)
        {
            _masterSlider.value = SoundManager.Instance.GetMasterVolume();
            _masterSlider.onValueChanged.AddListener(OnMasterChanged);
            UpdateVolumeText(_masterValueText, _masterSlider.value);
        }
        if (_bgmSlider != null)
        {
            _bgmSlider.value = SoundManager.Instance.GetBGMVolume();
            _bgmSlider.onValueChanged.AddListener(OnBGMChanged);
            UpdateVolumeText(_bgmValueText, _bgmSlider.value);
        }
        if (_sfxSlider != null)
        {
            _sfxSlider.value = SoundManager.Instance.GetSFXVolume();
            _sfxSlider.onValueChanged.AddListener(OnSFXChanged);
            UpdateVolumeText(_sfxValueText, _sfxSlider.value);
        }
        if (_uiSlider != null)
        {
            _uiSlider.value = SoundManager.Instance.GetUIVolume();
            _uiSlider.onValueChanged.AddListener(OnUIChanged);
            UpdateVolumeText(_uiValueText, _uiSlider.value);
        }
    }

    private void OnMasterChanged(float value)
    {
        SoundManager.Instance?.SetMasterVolume(value);
        UpdateVolumeText(_masterValueText, value);
    }

    private void OnBGMChanged(float value)
    {
        SoundManager.Instance?.SetBGMVolume(value);
        UpdateVolumeText(_bgmValueText, value);
    }

    private void OnSFXChanged(float value)
    {
        SoundManager.Instance?.SetSFXVolume(value);
        UpdateVolumeText(_sfxValueText, value);
    }

    private void OnUIChanged(float value)
    {
        SoundManager.Instance?.SetUIVolume(value);
        UpdateVolumeText(_uiValueText, value);
    }

    private void UpdateVolumeText(TextMeshProUGUI text, float value)
    {
        if (text != null)
            text.text = $"{Mathf.RoundToInt(value * 100)}";
    }

    #endregion

    #region Key Binding

    private void InitializeKeyBindButtons()
    {
        BindKeyButton(_keyCombatToggle, "CombatToggle");
        BindKeyButton(_keyExecution, "Execution");
        BindKeyButton(_keyInteract, "Interact");
        BindKeyButton(_keyDodge, "Dodge");
        BindKeyButton(_keySprint, "Sprint");
        BindKeyButton(_keyPlayerInfo, "PlayerInfo");
        BindKeyButton(_keyQuestToggle, "QuestToggle");

        RefreshKeyBindUI();
    }

    private void BindKeyButton(Button button, string actionName)
    {
        if (button == null) return;
        button.onClick.AddListener(() => StartWaitForKey(actionName));
    }

    private void StartWaitForKey(string actionName)
    {
        _waitingForAction = actionName;
        _isWaitingForKey = true;

        if (_keyWaitOverlay != null)
        {
            if (_keyWaitText != null)
                _keyWaitText.text = "키를 입력하세요...\n(ESC: 취소)";
            _keyWaitOverlay.SetImmediateOpened();
        }
    }

    private void DetectKeyInput()
    {
        //ESC로 취소
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelWaitForKey();
            return;
        }

        //마우스 버튼 제외 (UI 클릭과 충돌 방지)
        //키보드 + 일부 특수키만 허용
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            //마우스 버튼, 조이스틱은 제외
            if (key == KeyCode.None) continue;
            if (key == KeyCode.Escape) continue;
            if (key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6) continue;
            if (key >= KeyCode.JoystickButton0) continue;

            if (Input.GetKeyDown(key))
            {
                //중복 체크: 다른 액션에 이미 사용 중인 키인지
                string conflictAction = FindActionByKey(key);
                if (conflictAction != null && conflictAction != _waitingForAction)
                {
                    //기존 바인딩 해제(스왑)
                    KeyCode oldKey = _keyBindings[_waitingForAction];
                    _keyBindings[conflictAction] = oldKey;
                }

                _keyBindings[_waitingForAction] = key;
                FinishWaitForKey();
                return;
            }
        }
    }

    private void FinishWaitForKey()
    {
        _isWaitingForKey = false;
        _waitingForAction = null;
        _keyWaitOverlay?.SetImmediateClosed();
        RefreshKeyBindUI();
    }

    private void CancelWaitForKey()
    {
        _isWaitingForKey = false;
        _waitingForAction = null;
        _keyWaitOverlay?.SetImmediateClosed();
    }

    private string FindActionByKey(KeyCode key)
    {
        foreach (var pair in _keyBindings)
        {
            if (pair.Value == key)
                return pair.Key;
        }
        return null;
    }

    private void RefreshKeyBindUI()
    {
        UpdateKeyButtonText(_keyCombatToggle, "CombatToggle");
        UpdateKeyButtonText(_keyExecution, "Execution");
        UpdateKeyButtonText(_keyInteract, "Interact");
        UpdateKeyButtonText(_keyDodge, "Dodge");
        UpdateKeyButtonText(_keySprint, "Sprint");
        UpdateKeyButtonText(_keyPlayerInfo, "PlayerInfo");
        UpdateKeyButtonText(_keyQuestToggle, "QuestToggle");
    }

    private void UpdateKeyButtonText(Button button, string actionName)
    {
        if (button == null) return;

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null && _keyBindings.ContainsKey(actionName))
        {
            text.text = GetKeyDisplayName(_keyBindings[actionName]);
        }
    }

    /// <summary>
    /// KeyCode를 사용자 친화적 이름으로 변환.
    /// </summary>
    private string GetKeyDisplayName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Space: return "Space";
            case KeyCode.LeftShift: return "L-Shift";
            case KeyCode.RightShift: return "R-Shift";
            case KeyCode.LeftControl: return "L-Ctrl";
            case KeyCode.RightControl: return "R-Ctrl";
            case KeyCode.LeftAlt: return "L-Alt";
            case KeyCode.RightAlt: return "R-Alt";
            case KeyCode.Return: return "Enter";
            case KeyCode.BackQuote: return "`";
            case KeyCode.Tab: return "Tab";
            default: return key.ToString();
        }
    }

    /// <summary>
    /// 다른 스크립트에서 바인딩된 키를 가져올 때 사용.
    /// Input.GetKeyDown(OptionSettingsManager.GetKey("CombatToggle"))
    /// </summary>
    public static KeyCode GetKey(string actionName)
    {
        if (_keyBindings.TryGetValue(actionName, out KeyCode key))
            return key;

        //바인딩이 없으면 기본값 반환
        if (DEFAULT_KEYS.TryGetValue(actionName, out KeyCode defaultKey))
            return defaultKey;

        Debug.LogWarning($"[Option] 알 수 없는 액션: {actionName}");
        return KeyCode.None;
    }

    /// <summary>
    /// 기본 키 설정으로 초기화.
    /// </summary>
    public void ResetKeyBindings()
    {
        _keyBindings.Clear();
        foreach (var pair in DEFAULT_KEYS)
        {
            _keyBindings[pair.Key] = pair.Value;
        }
        RefreshKeyBindUI();
    }

    #endregion

    #region Display

    private void InitializeDisplayUI()
    {
        if (_resolutionDropdown != null)
        {
            _resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();

            int currentIndex = 0;
            for (int i = 0; i < _resolutions.Length; i++)
            {
                options.Add($"{_resolutions[i].x} x {_resolutions[i].y}");

                if (Screen.width == _resolutions[i].x && Screen.height == _resolutions[i].y)
                    currentIndex = i;
            }

            _resolutionDropdown.AddOptions(options);
            _resolutionDropdown.value = currentIndex;
            _selectedResolutionIndex = currentIndex;

            _resolutionDropdown.onValueChanged.AddListener((index) => _selectedResolutionIndex = index);
        }

        if (_fullscreenToggle != null)
        {
            _fullscreenToggle.isOn = Screen.fullScreen;
        }

        if (_applyDisplayButton != null)
        {
            _applyDisplayButton.onClick.AddListener(ApplyDisplaySettings);
        }
    }

    private void ApplyDisplaySettings()
    {
        Vector2Int res = _resolutions[_selectedResolutionIndex];
        bool fullscreen = _fullscreenToggle != null ? _fullscreenToggle.isOn : Screen.fullScreen;

        Screen.SetResolution(res.x, res.y, fullscreen);

        //저장
        PlayerPrefs.SetInt("ResolutionIndex", _selectedResolutionIndex);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);

        Debug.Log($"[Option] 해상도 변경: {res.x}x{res.y} / 전체화면: {fullscreen}");
    }

    #endregion

    #region Save / Load (PlayerPrefs)

    private void SaveAllSettings()
    {
        //사운드
        SoundManager.Instance?.SaveVolumeSettings();

        //키 바인딩
        foreach (var pair in _keyBindings)
        {
            PlayerPrefs.SetInt($"Key_{pair.Key}", (int)pair.Value);
        }

        //해상도
        PlayerPrefs.SetInt("ResolutionIndex", _selectedResolutionIndex);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);

        PlayerPrefs.Save();
        Debug.Log("[Option] 설정 저장 완료.");
    }

    private void LoadAllSettings()
    {
        //키 바인딩 로드
        _keyBindings.Clear();
        foreach (var pair in DEFAULT_KEYS)
        {
            int savedKey = PlayerPrefs.GetInt($"Key_{pair.Key}", (int)pair.Value);
            _keyBindings[pair.Key] = (KeyCode)savedKey;
        }

        //해상도 로드
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
    #endregion
}