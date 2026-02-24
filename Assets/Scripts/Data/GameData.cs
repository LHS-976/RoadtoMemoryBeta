using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class GameData
{
    [Header("Player Info")]
    public string PlayerName;
    public int Level;
    public float CurrentHealth;
    public float CurrentStamina;

    [Header("Currency")]
    public int DataChips;

    [Header("Location Info")]
    public string LastSceneName;
    public float PlayerPosX, PlayerPosY, PlayerPosZ;


    [Header("Quest Progress")]
    public List<int> ClearedQuestIDs = new List<int>();
    public int CurrentQuestID;
    public int CurrentQuestProgress;

    public long LastSaveTimeTicks;

    public GameData()
    {
        PlayerName = "Test subject_01";
        Level = 1;
        DataChips = 0;
        CurrentHealth = 100f;
        CurrentQuestID = -1;
        CurrentQuestProgress = 0;
    }
}
