using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("핵심 데이터 연결")]
        [SerializeField] private GameStateSO _gameState = default;
        [SerializeField] private IntEventChannelSO _dataChipChannel = default;
        [SerializeField] private VoidEventChannelSO _pauseChannel = default;

        public int CurrentDataChips { get; private set; }
        public int CurrentStageIndex { get; private set; }


        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        private void OnEnable()
        {
            if(_dataChipChannel != null)
            {

            }
            if(_pauseChannel != null)
            {

            }
        }
        private void OnDisable()
        {
            if (_dataChipChannel != null)
            {

            }
            if (_pauseChannel != null)
            {

            }
        }
        private void AddDataChips(int amount)
        {
            CurrentDataChips += amount;
            //세이브 추가
        }
        public void CompleteStage()
        {
            CurrentStageIndex++;
            //씬 로드 추가
        }
        public void GameOver()
        {
            //세이브포인트 스폰지점 추가
        }
        void StartGame()
        {
        }
        void Init()
        {

        }
        void TogglePause()
        {
            bool isPaused = Time.timeScale == 0;
            Time.timeScale = isPaused ? 1 : 0;
            //UI매니저 호출 로직 추가.
        }
        void Start()
        {
            StartGame();
        }
    }
}
