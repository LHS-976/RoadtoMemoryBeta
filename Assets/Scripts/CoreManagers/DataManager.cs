using UnityEngine;
using System.IO;
using System;
public class DataManager : MonoBehaviour
{
    public GameData CurrentData { get; private set; }

    public int CurrentSlotIndex { get; private set; } = -1;

    private const int MAX_SLOTS = 3;

    //슬롯 번호에 맞춰서 파일 경로를 만들어주는 함수
    private string GetSavePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveData_Slot{slotIndex}.json");
    }
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
    public void StartNewGame(int slotIndex)
    {
        CurrentSlotIndex = slotIndex;
        CurrentData = new GameData();

        string path = GetSavePath(slotIndex);
        if (File.Exists(path)) File.Delete(path);

        Debug.Log($"[DATA] 슬롯 {slotIndex}번으로 새 게임 준비 완료.");
    }

    public void SaveGame(int slotIndex)
    {
        CurrentSlotIndex = slotIndex;

        UpdatePlayerDataBeforeSave();
        if (Core.GameCore.Instance != null && Core.GameCore.Instance.QuestManager != null)
        {
            CurrentData.CurrentQuestID = Core.GameCore.Instance.QuestManager.CurrentQuest != null ? Core.GameCore.Instance.QuestManager.CurrentQuest.ID : -1;
            CurrentData.CurrentQuestProgress = Core.GameCore.Instance.QuestManager.CurrentProgress;
        }

        CurrentData.LastSaveTimeTicks = DateTime.Now.Ticks;

        string json = JsonUtility.ToJson(CurrentData, true);
        File.WriteAllText(GetSavePath(slotIndex), json);
        Debug.Log($"[DATA] 슬롯 {slotIndex} 저장 완료!");
    }

    public void LoadGame(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            CurrentData = JsonUtility.FromJson<GameData>(json);
            CurrentSlotIndex = slotIndex;
            Debug.Log($"[DATA] 슬롯 {slotIndex} 불러오기 성공");
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
        return latestSlot; // 세이브가 아예 없으면 -1 반환
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

    private void UpdatePlayerDataBeforeSave()
    {
        //(기존 플레이어 위치, 체력 등 저장 로직 유지)
        if (Core.GameCore.Instance != null && Core.GameCore.Instance.CurrentPlayer != null)
        {
            Transform playerTransform = Core.GameCore.Instance.CurrentPlayer.transform;
            CurrentData.PlayerPosX = playerTransform.position.x;
            CurrentData.PlayerPosY = playerTransform.position.y;
            CurrentData.PlayerPosZ = playerTransform.position.z;

            CurrentData.LastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            PlayerManager pm = Core.GameCore.Instance.CurrentPlayer.GetComponent<PlayerManager>();
            if (pm != null) CurrentData.CurrentHealth = pm.CurrentHp;
            if (pm != null) CurrentData.CurrentStamina = pm.CurrentStamina;
        }
    }
}