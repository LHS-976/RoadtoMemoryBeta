using UnityEngine;

public enum ShopItemType
{
    HpUpgrade,
    StaminaUpgrade,
    AttackUpgrade,
    SkillUnlock
}

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Shop Item")]
public class ShopItemSO : ScriptableObject
{
    [Header("Display")]
    public string ItemName;
    [TextArea] public string Description;

    [Header("Cost")]
    public int BasePrice;
    [Tooltip("업그레이드 횟수당 가격 증가량. SkillUnlock은 무시됨.")]
    public int PricePerLevel;

    [Header("Type")]
    public ShopItemType Type;

    [Header("Skill Unlock (SkillUnlock 타입일 때만)")]
    [Tooltip("AttackAction.attackName과 동일하게 설정")]
    public string AttackNameToUnlock;

    [Tooltip("순차 해금 순서. 0부터 시작. 이전 순서가 해금되어야 구매 가능.")]
    public int UnlockOrder;

    [Header("Combo Info (SkillUnlock 표시용)")]
    [Tooltip("공격 배수 (1.5)")]
    public float DamageMultiplier;
    [Tooltip("스태미나 소모량")]
    public float StaminaCost;

    public int GetPrice(int currentUpgradeCount)
    {
        if (Type == ShopItemType.SkillUnlock)
        {
            return BasePrice;
        }

        return BasePrice + (PricePerLevel * currentUpgradeCount);
    }

    public bool IsPurchased(GameData data)
    {
        if (Type == ShopItemType.SkillUnlock)
        {
            return data.IsAttackUnlocked(AttackNameToUnlock);
        }
        return false;
    }

    /// <summary>
    /// 순차 해금 조건 확인.
    /// UnlockOrder가 0이면 항상 구매 가능.
    /// 1 이상이면 이전 순서의 스킬이 해금되어야 함.
    /// </summary>
    public bool IsAvailableForPurchase(GameData data, ShopItemSO[] allSkillItems)
    {
        if (Type != ShopItemType.SkillUnlock) return true;
        if (IsPurchased(data)) return false;
        if (UnlockOrder <= 0) return true;

        //이전 순서의 스킬이 해금되었는지 확인
        foreach (var item in allSkillItems)
        {
            if (item.Type == ShopItemType.SkillUnlock && item.UnlockOrder == UnlockOrder - 1)
            {
                return item.IsPurchased(data);
            }
        }

        return false;
    }
}