using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public enum GameState
    {
        InGame,
        Pause,
        Inventory,
        Statshop

    }
    [CreateAssetMenu(fileName = "New GameState", menuName = "InGameState/GameState Data")]
    public class GameStateSO : ScriptableObject
    {
       
    }
}
