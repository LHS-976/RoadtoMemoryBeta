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

            DataManager.LoadGame();
            //SoundManager.InitializeVolume();
        }
        public void TogglePause()
        {
            if (_gameState == null) return;

            if (_gameState.CurrentState == GameState.Gameplay)
            {
                _gameState.SetState(GameState.Option);
            }
            else if (_gameState.CurrentState == GameState.Option)
            {
                _gameState.SetState(GameState.Gameplay);
            }
            else if(_gameState.CurrentState == GameState.PlayerInfo)
            {
                _gameState.SetState(GameState.Gameplay);
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
                TogglePause();
            }
        }
    }
}
