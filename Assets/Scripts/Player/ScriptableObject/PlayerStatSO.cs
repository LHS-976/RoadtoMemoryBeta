using UnityEngine;


[CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/PlayerStats")]
public class PlayerStatSO : ScriptableObject
{
    [Header("BaseMovement")]
    public float WalkSpeed = 2f; 
    public float RunSpeed = 6f;
    public float RotateSpeed = 10f;

    [Header("CombatMovement")]
    public float CombatWalkSpeed = 3f;
    public float CombatRunSpeed = 5f;

    [Header("Jump")]
    public float MaxJumpHeight = 2.0f;
    public float MaxJumpTime = 0.5f;

    //테스트용
    /*
    [Tooltip("체력, 공격력, 스태미나")]
    private float playerMaxHp = 100;
    private float playerMaxStamina = 100;

    private float playerMaxAtk = 20;
    */

}
