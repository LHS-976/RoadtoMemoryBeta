using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    public GameData CurrentData { get; private set; }

    private string _savePath;
    private void Awake()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "SaveData.json");
        LoadGame();
    }
    public void SaveGame()
    {
        string json = JsonUtility.ToJson(CurrentData, true);

        File.WriteAllText(_savePath, json);

        Debug.Log($"[DATA] 저장 완료:{_savePath}");
    }
    public void LoadGame()
    {
        if(File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);

            CurrentData = JsonUtility.FromJson<GameData>(json);
            Debug.Log("[DATA] 불러오기 성공");
        }
        else
        {
            CurrentData = new GameData();
            Debug.Log("[Data] 세이브 파일이 없어 새로 시작합니다.");
        }
    }
    public void DeleteSaveData()
    {
        if(File.Exists(_savePath))
        {
            File.Delete(_savePath);
            CurrentData = new GameData();
        }
    }
    public void AddDatachips(int amount)
    {
        CurrentData.DataChips += amount;
        //방송국 호출
    }
}
