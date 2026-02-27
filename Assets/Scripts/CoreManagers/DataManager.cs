using UnityEngine;
using System.IO;
using System;

public class DataManager : MonoBehaviour
{
    public GameData CurrentData { get; private set; }
    public int CurrentSlotIndex { get; private set; } = -1;

    private const int MAX_SLOTS = 3;

    private string GetSavePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveData_Slot{slotIndex}.json");
    }

    #region DataChips

    public void AddDatachips(int amount)
    {
        if (CurrentData != null)
        {
            CurrentData.DataChips += amount;
            Debug.Log($"[DATA] 데이터 칩 획득: +{amount} / 총 보유량: {CurrentData.DataChips}");
        }
    }

    public bool UseDatachips(int amount)
    {
        if (CurrentData != null && CurrentData.DataChips >= amount)
        {
            CurrentData.DataChips -= amount;
            Debug.Log($"[DATA] 데이터 칩 사용: -{amount} / 남은 보유량: {CurrentData.DataChips}");
            return true;
        }
        Debug.LogWarning("[DATA] 데이터 칩이 부족하여 사용할 수 없습니다!");
        return false;
    }

    #endregion

    #region New / Save / Load / Delete

    public void StartNewGame(int slotIndex)
    {
        CurrentSlotIndex = slotIndex;
        CurrentData = new GameData();

        string path = GetSavePath(slotIndex);
        if (File.Exists(path)) File.Delete(path);

        Debug.Log($"[DATA] 슬롯 {slotIndex}번으로 새 게임 준비 완료.");
    }

    /// <summary>
    /// 세이브 포인트에서 호출.
    /// 위치는 SavePointInteractable.Interact()에서 미리 기록해둔 값 사용.
    /// </summary>
    public void SaveGame(int slotIndex)
    {
        CurrentSlotIndex = slotIndex;

        //씬 이름
        CurrentData.LastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        //HP, 스태미나
        if (Core.GameCore.Instance?.CurrentPlayer != null)
        {
            PlayerManager pm = Core.GameCore.Instance.CurrentPlayer.GetComponent<PlayerManager>();
            if (pm != null)
            {
                CurrentData.CurrentHealth = pm.CurrentHp;
                CurrentData.CurrentStamina = pm.CurrentStamina;
            }
        }

        // 퀘스트
        if (Core.GameCore.Instance?.QuestManager != null)
        {
            CurrentData.CurrentQuestID = Core.GameCore.Instance.QuestManager.CurrentQuest != null
                ? Core.GameCore.Instance.QuestManager.CurrentQuest.ID
                : -1;
            CurrentData.CurrentQuestProgress = Core.GameCore.Instance.QuestManager.CurrentProgress;
        }

        // 위치는 SavePointInteractable.Interact()에서 이미 기록됨
        // PlayerPosX/Y/Z를 여기서 덮어쓰지 않음

        CurrentData.LastSaveTimeTicks = DateTime.Now.Ticks;

        string json = JsonUtility.ToJson(CurrentData, true);
        File.WriteAllText(GetSavePath(slotIndex), json);
        Debug.Log($"[DATA] 슬롯 {slotIndex} 저장 완료! 위치: ({CurrentData.PlayerPosX:F1}, {CurrentData.PlayerPosY:F1}, {CurrentData.PlayerPosZ:F1})");
    }

    public void LoadGame(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            CurrentData = JsonUtility.FromJson<GameData>(json);
            CurrentSlotIndex = slotIndex;
            Debug.Log($"[DATA] 슬롯 {slotIndex} 불러오기 성공 | 씬: {CurrentData.LastSceneName} | 위치: ({CurrentData.PlayerPosX:F1}, {CurrentData.PlayerPosY:F1}, {CurrentData.PlayerPosZ:F1})");
        }
        else
        {
            Debug.LogWarning($"[DATA] 슬롯 {slotIndex}이 비어있습니다!");
        }
    }

    public bool HasSaveData(int slotIndex)
    {
        return File.Exists(GetSavePath(slotIndex));
    }

    public int GetLatestSaveSlot()
    {
        int latestSlot = -1;
        long maxTicks = 0;

        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (HasSaveData(i))
            {
                string json = File.ReadAllText(GetSavePath(i));
                GameData tempData = JsonUtility.FromJson<GameData>(json);

                if (tempData.LastSaveTimeTicks > maxTicks)
                {
                    maxTicks = tempData.LastSaveTimeTicks;
                    latestSlot = i;
                }
            }
        }
        return latestSlot;
    }

    public void DeleteSaveData(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[DATA] 슬롯 {slotIndex} 데이터 삭제 완료");

            if (CurrentSlotIndex == slotIndex)
            {
                CurrentSlotIndex = -1;
                CurrentData = new GameData();
            }
        }
    }

    public GameData GetSaveData(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameData>(json);
        }
        return null;
    }

    #endregion

    #region Utility

    /// <summary>
    /// 세이브 포인트 위치를 GameData에 기록.
    /// SavePointInteractable.Interact()에서 호출.
    /// </summary>
    public void SetSavePointPosition(Vector3 position)
    {
        if (CurrentData == null) return;

        CurrentData.PlayerPosX = position.x;
        CurrentData.PlayerPosY = position.y;
        CurrentData.PlayerPosZ = position.z;
    }

    //엔딩 후 삭제요청
    public void DeleteCurrentSlot()
    {
        if(CurrentSlotIndex != -1)
        {
            DeleteSaveData(CurrentSlotIndex);
        }
    }

    #endregion
}