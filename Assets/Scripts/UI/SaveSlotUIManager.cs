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
        _slotPanelFader?.FadeOut();
        _confirmPopupFader?.FadeOut();
        _titleUIManager.OnClickBackFromGameMode();
    }

    public void OpenSlotMenu(SlotMenuMode mode)
    {
        _currentMode = mode;
        RefreshSlots();
        _slotPanelFader.FadeIn();
    }

    public void CloseSlotMenu()
    {
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
        _pendingSlotIndex = slotIndex;
        _confirmText.text = $"정말 슬롯 {slotIndex + 1}번의 데이터를 삭제하시겠습니까?";

        _slotPanelFader.FadeOut();
        _confirmPopupFader.FadeIn();
    }

    private void OnConfirmYes()
    {
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
}