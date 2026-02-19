using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    /// <summary>
    /// 게임 내부 상태관리.
    /// </summary>
    public enum GameState
    {
        Gameplay,
        Option,
        PlayerInfo,
        StatShop,
        Cutscene,
        Loading

    }
    [CreateAssetMenu(fileName = "GameStateChannel", menuName = "Events/GameState Data")]
    public class GameStateSO : ScriptableObject
    {
        [field: SerializeField] public GameState CurrentState { get; private set; }

        public GameState PreviousState { get; private set; }
        public UnityAction<GameState> OnStateChange;

        public void Init(GameState startState)
        {
            CurrentState = startState;
            PreviousState = startState;
        }

        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            PreviousState = CurrentState;
            CurrentState = newState;

            OnStateChange?.Invoke(CurrentState);
            Debug.Log($"[Game State] Changed: {PreviousState} -> {CurrentState}");
        }
        public void RestorePreviousState()
        {
            SetState(PreviousState);
        }
    }
}
