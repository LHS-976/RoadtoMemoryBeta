using System;
using UnityEngine;

/// <summary>
/// 상점 구매 로직.
/// SavePointUIManager에서 호출.
/// </summary>
public class ShopManager : MonoBehaviour
{
    private DataManager _dataManager;

    public event Action<ShopItemSO> OnPurchaseSuccess;
    public event Action<string> OnPurchaseFailed;

    /// <summary>
    /// 순차 해금 체크를 위한 전체 스킬 목록. SavePointUIManager에서 설정.
    /// </summary>
    [HideInInspector] public ShopItemSO[] AllSkillItems;

    private void Start()
    {
        _dataManager = Core.GameCore.Instance?.DataManager;
    }

    public bool TryPurchase(ShopItemSO item)
    {
        if (_dataManager == null || _dataManager.CurrentData == null)
        {
            OnPurchaseFailed?.Invoke("데이터 없음");
            return false;
        }

        GameData data = _dataManager.CurrentData;

        //스킬 해금: 중복 구매 방지
        if (item.Type == ShopItemType.SkillUnlock && item.IsPurchased(data))
        {
            OnPurchaseFailed?.Invoke("이미 해금된 스킬입니다.");
            return false;
        }

        //스킬 해금: 순차 해금 체크
        if (item.Type == ShopItemType.SkillUnlock && AllSkillItems != null)
        {
            if (!item.IsAvailableForPurchase(data, AllSkillItems))
            {
                OnPurchaseFailed?.Invoke("이전 스킬을 먼저 해금해야 합니다.");
                return false;
            }
        }

        int currentCount = GetUpgradeCount(data, item.Type);
        int price = item.GetPrice(currentCount);

        if (!_dataManager.UseDatachips(price))
        {
            OnPurchaseFailed?.Invoke("DataChips가 부족합니다.");
            return false;
        }

        ApplyPurchase(data, item);
        OnPurchaseSuccess?.Invoke(item);

        Debug.Log($"[Shop] 구매 성공: {item.ItemName} (비용: {price})");
        return true;
    }

    private void ApplyPurchase(GameData data, ShopItemSO item)
    {
        switch (item.Type)
        {
            case ShopItemType.HpUpgrade:
                data.HpUpgradeCount++;
                break;
            case ShopItemType.StaminaUpgrade:
                data.StaminaUpgradeCount++;
                break;
            case ShopItemType.AttackUpgrade:
                data.AttackUpgradeCount++;
                break;
            case ShopItemType.SkillUnlock:
                data.UnlockAttack(item.AttackNameToUnlock);
                break;
        }
    }

    public int GetCurrentPrice(ShopItemSO item)
    {
        if (_dataManager == null || _dataManager.CurrentData == null) return item.BasePrice;
        int count = GetUpgradeCount(_dataManager.CurrentData, item.Type);
        return item.GetPrice(count);
    }

    public int GetUpgradeCount(GameData data, ShopItemType type)
    {
        switch (type)
        {
            case ShopItemType.HpUpgrade: return data.HpUpgradeCount;
            case ShopItemType.StaminaUpgrade: return data.StaminaUpgradeCount;
            case ShopItemType.AttackUpgrade: return data.AttackUpgradeCount;
            default: return 0;
        }
    }
}