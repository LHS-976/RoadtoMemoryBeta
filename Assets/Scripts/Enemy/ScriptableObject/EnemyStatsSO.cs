using UnityEngine;


[CreateAssetMenu(fileName = "EnemyStats", menuName = "Enemy/Stats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Identity")]
    public string enemyID;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 100f;

    [Header("FOV")]
    public float viewRadius = 10f;
    public float viewAngle = 80f;

    [Header("Combat Settings")]
    public float maxHealth = 100f;
    public float baseDamage = 15f;
    public float attackCooldown = 3f;
    public float attackRange = 2.5f;
    public float attackTriggerRange = 1.8f;
    public float damageMultiplier = 1.0f;
    public float stopChaseDistance = 15.0f;

    [Header("Defense Settings")]
    public float defenseArmor = 100f; //깨질때 특수처리할 예정
    public float groggyDuration = 3.0f;
    public float damageReduce = 0.8f;
    public float groggyDamageMultiplier = 1.5f;

    [Header("Execution Settings")]
    public float canExecutionDuration = 4.0f;

    [Header("Detection Setting")]
    public LayerMask playerMask;
    public LayerMask obstacleLayer;

    [Header("Patrol Settings")]
    public float patrolRadius = 10f;
    public float patrolidleTime = 2f;

    [Header("Hurt Settings")]
    public float knockbackDuration = 0.2f;
    public float knockbackPower = 10f;

    [Header("Drop Settings")]
    public int dropDataChips = 10;
}
