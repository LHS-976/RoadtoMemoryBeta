using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Data Settings")]
        [SerializeField] private GameStateSO _gameState;
        [SerializeField] private VoidEventChannelSO _pauseChannel;

        public int CurrentDataChips { get; private set; }
        public int CurrentStageIndex { get; private set; }


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
    }
}
