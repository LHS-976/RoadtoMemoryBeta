using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    public GameData CurrentData { get; private set; }

    private string _savePath;


    private void Awake()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "SaveData.json");

        //게임테스트할땐 지우기.
        //Sample씬 테스트에서만 사용
        //Bootstrap에서 실행되므로 사용안함
        //LoadGame();
    }
    public void SaveGame()
    {
        UpdatePlayerDataBeforeSave();
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
    private void UpdatePlayerDataBeforeSave()
    {
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
