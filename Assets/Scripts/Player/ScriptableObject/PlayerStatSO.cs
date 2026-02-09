using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/PlayerStats")]
public class PlayerStatSO : ScriptableObject
{
    //RootMotion 없을때 속도.
    [Header("BaseMovement")]
    public float WalkSpeed = 2f; 
    public float RunSpeed = 6f;
    public float RotateSpeed = 10f;

    //RootMotion 없을때 속도.
    [Header("CombatMovement")]
    public float CombatWalkSpeed = 3f;
    public float CombatRunSpeed = 5f;

    [Header("Jump")]
    public float MaxJumpHeight = 2.0f;
    public float MaxJumpTime = 0.5f;

    [Tooltip("체력, 공격력, 스태미나")]
    public float playerMaxHp = 100f;
    public float playerMaxStamina = 100f;

    public float staminaRegenrate = 10f;
    public float sprintStaminaCost = 10f;
    public float dashStaminaCost = 20f;
}
