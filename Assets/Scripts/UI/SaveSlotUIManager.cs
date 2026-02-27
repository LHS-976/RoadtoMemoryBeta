using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;
using System.Collections;

public enum SlotMenuMode { None, NewGame, LoadGame }

public class SaveSlotUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private PanelFader _slotPanelFader;
    [SerializeField] private PanelFader _confirmPopupFader;

    [Header("Slots UI")]
    [SerializeField] private Button[] _slotButtons;
    [SerializeField] private TextMeshProUGUI[] _slotTexts;
    [SerializeField] private Button[] _deleteButtons;

    [Header("Confirm Popup UI")]
    [SerializeField] private TextMeshProUGUI _confirmText;
    [SerializeField] private Button _confirmYesBtn;
    [SerializeField] private Button _confirmNoBtn;

    [SerializeField] private TitleUIManager _titleUIManager;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip _clickSound;
    [SerializeField] private AudioClip _cancelSound;
    [SerializeField] private AudioClip _panelOpenSound;
    [SerializeField] private AudioClip _saveSFX;

    private SlotMenuMode _currentMode;
    private int _pendingSlotIndex = -1;

    private void Start()
    {
        _slotPanelFader?.SetImmediateClosed();
        _confirmPopupFader?.SetImmediateClosed();

        _confirmYesBtn.onClick.AddListener(OnConfirmYes);
        _confirmNoBtn.onClick.AddListener(OnConfirmNo);

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            _slotButtons[i].onClick.AddListener(() => OnSlotClicked(index));
            _deleteButtons[i].onClick.AddListener(() => OnDeleteClicked(index));
        }
    }
    public void RetunrToMain()
    {
        PlayCancelSound();
        _slotPanelFader?.FadeOut();
        _confirmPopupFader?.FadeOut();
        _titleUIManager.OnClickBackFromGameMode();
    }

    public void OpenSlotMenu(SlotMenuMode mode)
    {
        PlayPanelOpenSound();
        _currentMode = mode;
        RefreshSlots();
        _slotPanelFader.FadeIn();
    }

    public void CloseSlotMenu()
    {
        PlayCancelSound();
        _slotPanelFader.FadeOut();
        _confirmPopupFader.FadeOut();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            bool hasData = GameCore.Instance.DataManager.HasSaveData(i);
            _deleteButtons[i].gameObject.SetActive(hasData);

            if (hasData)
            {
                _slotTexts[i].text = $"Slot {i + 1}\n<size=80%>저장된 데이터 있음</size>";
            }
            else
            {
                _slotTexts[i].text = $"Slot {i + 1}\n<size=80%>비어있음</size>";
            }
        }
    }

    private void OnSlotClicked(int slotIndex)
    {
        PlayClickSound();
        if (_currentMode == SlotMenuMode.NewGame)
        {
            GameCore.Instance.DataManager.StartNewGame(slotIndex);
            StartGame();
        }
        else if (_currentMode == SlotMenuMode.LoadGame)
        {
            if (GameCore.Instance.DataManager.HasSaveData(slotIndex))
            {
                GameCore.Instance.DataManager.LoadGame(slotIndex);
                StartGame();
            }
            else
            {
                Debug.LogWarning("비어있는 슬롯은 불러올 수 없습니다!");
            }
        }
    }

    private void OnDeleteClicked(int slotIndex)
    {
        PlayClickSound();
        _pendingSlotIndex = slotIndex;
        _confirmText.text = $"정말 슬롯 {slotIndex + 1}번의 데이터를 삭제하시겠습니까?";

        _slotPanelFader.FadeOut();
        _confirmPopupFader.FadeIn();
    }

    private void OnConfirmYes()
    {
        PlayClickSound();
        if (_pendingSlotIndex != -1)
        {
            GameCore.Instance.DataManager.DeleteSaveData(_pendingSlotIndex);
            _pendingSlotIndex = -1;

            _confirmPopupFader.FadeOut();
            RefreshSlots();
            _slotPanelFader.FadeIn();
        }
    }

    private void OnConfirmNo()
    {
        PlayCancelSound();
        _pendingSlotIndex = -1;
        _confirmPopupFader.FadeOut();
        _slotPanelFader.FadeIn();
    }

    private void StartGame()
    {
        CloseSlotMenu();
        StartCoroutine(StartGameRoutine());
    }
    private IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(0.4f);

        if(_titleUIManager != null)
        {
            bool isNewGame = (_currentMode == SlotMenuMode.NewGame);
            _titleUIManager.PlayCinematic(!isNewGame);
        }
    }
    #region Play Sound
    public void PlayClickSound()
    {
        if (_clickSound != null) SoundManager.Instance.PlayUI(_clickSound);
    }

    public void PlayCancelSound()
    {
        if (_cancelSound != null) SoundManager.Instance.PlayUI(_cancelSound);
    }

    public void PlayPanelOpenSound()
    {
        if (_panelOpenSound != null) SoundManager.Instance.PlayUI(_panelOpenSound);
    }
    public void PlaySaveSound()
    {
        if (_saveSFX != null) SoundManager.Instance.PlayUI(_saveSFX);
    }
    #endregion
}