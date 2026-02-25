using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;

public class SavePointUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShopManager _shopManager;

    [Header("Panels")]
    [SerializeField] private PanelFader _shopPanel;
    [SerializeField] private PanelFader _saveSlotPanel;
    [SerializeField] private PanelFader _confirmPopupPanel;

    [Header("Tab Buttons")]
    [SerializeField] private Button _statsTabButton;
    [SerializeField] private Button _comboTabButton;
    [SerializeField] private Button _closeShopButton;

    [Header("Tab Panels")]
    [SerializeField] private PanelFader _statsContent;
    [SerializeField] private PanelFader _comboContent;

    [Header("Stats Items (HP, Stamina, Attack 순서)")]
    [SerializeField] private ShopItemSO[] _statItems;

    [Header("Stats UI Elements")]
    [SerializeField] private Button[] _statBuyButtons;
    [SerializeField] private TextMeshProUGUI[] _statPriceTexts;
    [SerializeField] private TextMeshProUGUI[] _statDescTexts;

    [Header("Combo Items (순차 해금 순서대로 배열)")]
    [SerializeField] private ShopItemSO[] _comboItems;

    [Header("Combo UI Elements")]
    [SerializeField] private Button[] _comboBuyButtons;
    [SerializeField] private TextMeshProUGUI[] _comboPriceTexts;
    [SerializeField] private TextMeshProUGUI[] _comboDescTexts;

    [Header("Shared UI")]
    [SerializeField] private TextMeshProUGUI _dataChipsText;

    [Header("Save Slot UI")]
    [SerializeField] private Button[] _slotButtons;
    [SerializeField] private TextMeshProUGUI[] _slotTexts;
    [SerializeField] private Button _closeSaveButton;

    [Header("Confirm Popup")]
    [SerializeField] private TextMeshProUGUI _confirmText;
    [SerializeField] private Button _confirmYesButton;
    [SerializeField] private Button _confirmNoButton;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioClip _buySFX;
    [SerializeField] private AudioClip _errorSFX;
    [SerializeField] private AudioClip _saveSFX;

    [Header("GameState")]
    [SerializeField] private GameStateSO _gameState;

    private DataManager _dataManager;
    private int _pendingSaveSlot = -1;
    private bool _isStatsTab = true;

    private void Start()
    {
        _dataManager = GameCore.Instance?.DataManager;

        _shopPanel?.SetImmediateClosed();
        _statsContent?.SetImmediateClosed();
        _comboContent?.SetImmediateClosed();
        _saveSlotPanel?.SetImmediateClosed();
        _confirmPopupPanel?.SetImmediateClosed();

        if (_shopManager != null)
            _shopManager.AllSkillItems = _comboItems;

        //탭 버튼
        if (_statsTabButton != null)
            _statsTabButton.onClick.AddListener(ShowStatsTab);
        if (_comboTabButton != null)
            _comboTabButton.onClick.AddListener(ShowComboTab);
        if (_closeShopButton != null)
            _closeShopButton.onClick.AddListener(CloseShop);

        //Stats 구매 버튼
        for (int i = 0; i < _statBuyButtons.Length && i < _statItems.Length; i++)
        {
            int index = i;
            _statBuyButtons[i].onClick.AddListener(() => OnClickBuyStat(index));
        }

        //Combo 구매 버튼
        for (int i = 0; i < _comboBuyButtons.Length && i < _comboItems.Length; i++)
        {
            int index = i;
            _comboBuyButtons[i].onClick.AddListener(() => OnClickBuyCombo(index));
        }

        //세이브 슬롯
        for (int i = 0; i < _slotButtons.Length; i++)
        {
            int index = i;
            _slotButtons[i].onClick.AddListener(() => OnSaveSlotClicked(index));
        }

        if (_closeSaveButton != null)
            _closeSaveButton.onClick.AddListener(CloseSaveAndResume);
        if (_confirmYesButton != null)
            _confirmYesButton.onClick.AddListener(OnConfirmSaveYes);
        if (_confirmNoButton != null)
            _confirmNoButton.onClick.AddListener(OnConfirmSaveNo);

        //구매 이벤트 구독
        if (_shopManager != null)
        {
            _shopManager.OnPurchaseSuccess += HandlePurchaseSuccess;
            _shopManager.OnPurchaseFailed += HandlePurchaseFailed;
        }
    }

    private void OnDestroy()
    {
        if (_shopManager != null)
        {
            _shopManager.OnPurchaseSuccess -= HandlePurchaseSuccess;
            _shopManager.OnPurchaseFailed -= HandlePurchaseFailed;
        }
    }

    #region Cursor Control

    private void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    #endregion

    #region Shop Open / Close

    public void OpenShop()
    {
        if (_dataManager == null)
            _dataManager = GameCore.Instance?.DataManager;

        ShowCursor();
        ShowStatsTab();
        _shopPanel?.FadeIn();
    }

    public void CloseShop()
    {
        _statsContent?.SetImmediateClosed();
        _comboContent?.SetImmediateClosed();
        _shopPanel?.FadeOut();

        ApplyUpgradesToPlayer();

        RefreshSaveSlots();
        _saveSlotPanel?.FadeIn();
    }

    #endregion

    #region Tab Switch

    private void ShowStatsTab()
    {
        Debug.Log("[Shop] Stats 탭 전환");
        _isStatsTab = true;
        _statsContent?.SetImmediateOpened();
        _comboContent?.SetImmediateClosed();
        RefreshStatsUI();
        RefreshDataChips();
    }

    private void ShowComboTab()
    {
        _isStatsTab = false;
        _statsContent?.SetImmediateClosed();
        _comboContent?.SetImmediateOpened();
        RefreshComboUI();
        RefreshDataChips();
    }

    #endregion

    #region Stats Tab

    private void OnClickBuyStat(int index)
    {
        if (index >= _statItems.Length) return;
        _shopManager.TryPurchase(_statItems[index]);
    }

    private void RefreshStatsUI()
    {
        GameData data = _dataManager?.CurrentData;
        if (data == null) return;

        for (int i = 0; i < _statItems.Length && i < _statBuyButtons.Length; i++)
        {
            ShopItemSO item = _statItems[i];
            int price = _shopManager.GetCurrentPrice(item);
            int upgradeCount = _shopManager.GetUpgradeCount(data, item.Type);

            if (i < _statPriceTexts.Length && _statPriceTexts[i] != null)
                _statPriceTexts[i].text = $"{price}";

            if (i < _statDescTexts.Length && _statDescTexts[i] != null)
                _statDescTexts[i].text = GetStatDescription(item, data, upgradeCount);

            _statBuyButtons[i].interactable = data.DataChips >= price;
        }
    }

    private string GetStatDescription(ShopItemSO item, GameData data, int upgradeCount)
    {
        switch (item.Type)
        {
            case ShopItemType.HpUpgrade:
                float currentMaxHp = data.GetUpgradedMaxHp(100f);
                return $"{item.Description}\n현재 Lv.{upgradeCount} (최대 HP: {currentMaxHp:F0}) → +10";

            case ShopItemType.StaminaUpgrade:
                float currentMaxSt = data.GetUpgradedMaxStamina(100f);
                return $"{item.Description}\n현재 Lv.{upgradeCount} (최대 ST: {currentMaxSt:F0}) → +10";

            case ShopItemType.AttackUpgrade:
                float currentBonus = data.GetAttackBonus();
                return $"{item.Description}\n현재 Lv.{upgradeCount} (보너스: +{currentBonus:F0}) → +2";

            default:
                return item.Description;
        }
    }

    #endregion

    #region Combo Tab

    private void OnClickBuyCombo(int index)
    {
        if (index >= _comboItems.Length) return;
        _shopManager.TryPurchase(_comboItems[index]);
    }

    private void RefreshComboUI()
    {
        GameData data = _dataManager?.CurrentData;
        if (data == null) return;

        for (int i = 0; i < _comboItems.Length && i < _comboBuyButtons.Length; i++)
        {
            ShopItemSO item = _comboItems[i];
            bool isPurchased = item.IsPurchased(data);
            bool isAvailable = item.IsAvailableForPurchase(data, _comboItems);
            int price = _shopManager.GetCurrentPrice(item);

            if (i < _comboPriceTexts.Length && _comboPriceTexts[i] != null)
            {
                if (isPurchased)
                    _comboPriceTexts[i].text = "SOLD";
                else if (!isAvailable)
                    _comboPriceTexts[i].text = "LOCKED";
                else
                    _comboPriceTexts[i].text = $"{price}";
            }

            if (i < _comboDescTexts.Length && _comboDescTexts[i] != null)
            {
                if (isPurchased)
                    _comboDescTexts[i].text = GetComboDescription(item) + "\n<color=#00FF88>해금 완료</color>";
                else if (!isAvailable)
                    _comboDescTexts[i].text = "이전 스킬을 먼저 해금하세요";
                else
                    _comboDescTexts[i].text = GetComboDescription(item);
            }

            _comboBuyButtons[i].interactable = !isPurchased && isAvailable && data.DataChips >= price;
        }
    }

    private string GetComboDescription(ShopItemSO item)
    {
        string desc = $"배수: x{item.DamageMultiplier:F1}";

        if (item.StaminaCost > 0)
            desc += $" | 스태미나: -{item.StaminaCost:F0}";

        if (!string.IsNullOrEmpty(item.Description))
            desc += $"\n{item.Description}";

        return desc;
    }

    #endregion

    #region Purchase Callbacks

    private void HandlePurchaseSuccess(ShopItemSO item)
    {
        if (_buySFX != null)
            SoundManager.Instance?.PlayUI(_buySFX);

        if (_isStatsTab)
            RefreshStatsUI();
        else
            RefreshComboUI();

        RefreshDataChips();
    }

    private void HandlePurchaseFailed(string reason)
    {
        if (_errorSFX != null)
            SoundManager.Instance?.PlayUI(_errorSFX);

        Debug.Log($"[Shop] 구매 실패: {reason}");
    }

    private void RefreshDataChips()
    {
        GameData data = _dataManager?.CurrentData;
        if (_dataChipsText != null && data != null)
            _dataChipsText.text = $"{data.DataChips}";
    }

    private void ApplyUpgradesToPlayer()
    {
        if (GameCore.Instance?.CurrentPlayer == null) return;

        PlayerManager pm = GameCore.Instance.CurrentPlayer.GetComponent<PlayerManager>();
        if (pm == null) return;

        pm.ApplyUpgrades();
    }

    #endregion

    #region Save Slots

    private void OnSaveSlotClicked(int slotIndex)
    {
        bool hasData = _dataManager.HasSaveData(slotIndex);

        if (hasData)
        {
            _pendingSaveSlot = slotIndex;
            _confirmText.text = $"슬롯 {slotIndex + 1}번에 덮어쓰시겠습니까?";

            _saveSlotPanel?.FadeOut();
            _confirmPopupPanel?.FadeIn();
        }
        else
        {
            ExecuteSave(slotIndex);
        }
    }

    private void OnConfirmSaveYes()
    {
        if (_pendingSaveSlot != -1)
        {
            ExecuteSave(_pendingSaveSlot);
            _pendingSaveSlot = -1;
        }
    }

    private void OnConfirmSaveNo()
    {
        _pendingSaveSlot = -1;
        _confirmPopupPanel?.FadeOut();
        _saveSlotPanel?.FadeIn();
    }

    private void ExecuteSave(int slotIndex)
    {
        _dataManager.SaveGame(slotIndex);

        if (_saveSFX != null)
            SoundManager.Instance?.PlayUI(_saveSFX);

        Debug.Log($"[Save] 슬롯 {slotIndex + 1}번에 저장 완료!");

        _confirmPopupPanel?.FadeOut();
        _saveSlotPanel?.FadeOut();

        HideCursor();

        if (GameCore.Instance != null)
            GameCore.Instance.ResumeGame();
    }

    private void CloseSaveAndResume()
    {
        _saveSlotPanel?.FadeOut();
        _confirmPopupPanel?.FadeOut();

        HideCursor();

        if (GameCore.Instance != null)
            GameCore.Instance.ResumeGame();
    }

    private void RefreshSaveSlots()
    {
        for (int i = 0; i < _slotButtons.Length; i++)
        {
            bool hasData = _dataManager.HasSaveData(i);

            if (hasData)
            {
                GameData data = _dataManager.GetSaveData(i);
                string sceneName = !string.IsNullOrEmpty(data.LastSceneName) ? data.LastSceneName : "---";
                _slotTexts[i].text = $"Slot {i + 1}\n<size=80%>{sceneName} | {data.DataChips} Chips</size>";
            }
            else
            {
                _slotTexts[i].text = $"Slot {i + 1}\n<size=80%>비어있음</size>";
            }
        }
    }

    #endregion
}