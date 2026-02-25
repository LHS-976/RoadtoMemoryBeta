using UnityEngine;

namespace Core
{
    public class GameCore : MonoBehaviour
    {
        public static GameCore Instance { get; private set; }


        [Header("Essential Managers")]
        public SoundManager SoundManager;
        public TimeManager TimeManager;
        public VFXManager VFXManager;
        public SceneRouter Scene;
        public DataManager DataManager;
        public QuestManager QuestManager;


        [Header("Data Settings")]
        [SerializeField] private GameStateSO _gameState;

        [Header("Player Spawning")]
        [SerializeField] private GameObject _playerPrefab;
        public GameObject CurrentPlayer { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void InitializeManagers()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            //SoundManager.InitializeVolume();
        }
        public void TogglePause()
        {
            if (_gameState == null) return;

            if (_gameState.CurrentState == GameState.Gameplay)
            {
                _gameState.SetState(GameState.Option);

                if (TimeManager != null)
                {
                    TimeManager.PauseTime();
                }
            }
            else if (_gameState.CurrentState == GameState.Option || _gameState.CurrentState == GameState.PlayerInfo)
            {
                ResumeGame();
            }
        }

        public void ResumeGame()
        {
            if(_gameState != null)
            {
                _gameState.SetState(GameState.Gameplay);
            }

            if(TimeManager != null)
            {
                TimeManager.ForceRestoreTime();
            }
        }
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if (_gameState != null &&
                   (_gameState.CurrentState == GameState.Title || _gameState.CurrentState == GameState.Dialogue ||
                   _gameState.CurrentState == GameState.StatShop || _gameState.CurrentState == GameState.PlayerInfo))
                {
                    return;
                }

                TogglePause();
            }
        }

        #region PlayerSpawnManage
        public GameObject SpawnPlayer(Transform spawnPoint)
        {
            if (_playerPrefab == null) return null;

            CurrentPlayer = Instantiate(_playerPrefab, spawnPoint.position, spawnPoint.rotation);
            return CurrentPlayer;
        }
        #endregion
    }
}
