using UnityEngine;


[CreateAssetMenu(fileName = "EnemyStats", menuName = "Enemy/Stats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 100f;

    [Header("FOV")]
    public float viewRadius = 10f;
    public float viewAngle = 80f;

    [Header("Combat Setting")]
    public float maxHealth = 100f;
    public float attackCooldown = 3f;
    public float attackRange = 1.5f;
    public float defenseArmor = 50f; //깨질때 특수처리할 예정
    public float damageMultiplier = 1.0f;

    [Header("Detection Setting")]
    public LayerMask playerMask;
    public LayerMask obstacleLayer;

    [Header("Patrol Settings")]
    public float patrolRadius = 10f;
    public float patrolidleTime = 2f;

    [Header("Hurt Settings")]
    public float knockbackDuration = 0.2f;
    public float knockbackPower = 10f;
}
