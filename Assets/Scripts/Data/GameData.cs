using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class GameData
{
    [Header("Player Info")]
    public string PlayerName;
    //public int Level;
    public float CurrentHealth;
    public float CurrentStamina;
    public float PlayTime;

    [Header("Currency")]
    public int DataChips;

    [Header("Location Info")]
    public string LastSceneName;
    public float PlayerPosX, PlayerPosY, PlayerPosZ;


    [Header("Quest Progress")]
    public List<int> ClearedQuestIDs = new List<int>();
    public int CurrentQuestID;
    public int CurrentQuestProgress;

    [Header("Upgrades")]
    public int HpUpgradeCount;
    public int StaminaUpgradeCount;
    public int AttackUpgradeCount;

    [Header("Unlocked Skills")]
    public List<string> UnlockedAttacks = new List<string>();

    [Header("Save Info")]
    public long LastSaveTimeTicks;

    [Header("Combat")]
    public bool IsCombatUnlocked;

    [Header("Defeated Enemies")]
    public List<string> DefeatedEnemyIDs = new List<string>();

    public GameData()
    {
        PlayerName = "Test subject_01";
        //Level = 1;
        DataChips = 0;
        CurrentHealth = 100f;
        CurrentQuestID = -1;
        CurrentQuestProgress = 0;
        HpUpgradeCount = 0;
        StaminaUpgradeCount = 0;
        AttackUpgradeCount = 0;
    }
    public float GetUpgradedMaxHp(float baseHp)
    {
        return baseHp + (HpUpgradeCount * 10f);
    }
    public float GetUpgradedMaxStamina(float baseStamina)
    {
        return baseStamina + (StaminaUpgradeCount * 10f);
    }
    public float GetAttackBonus()
    {
        return AttackUpgradeCount * 2f;
    }

    public bool IsAttackUnlocked(string attackName)
    {
        return UnlockedAttacks.Contains(attackName);
    }

    public void UnlockAttack(string attackName)
    {
        if (!UnlockedAttacks.Contains(attackName))
        {
            UnlockedAttacks.Add(attackName);
        }
    }
}
