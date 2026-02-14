using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable
{
    public EnemyStatsSO EnemyStats;
    [SerializeField] private EnemyController _enemyController;

    private float _currentHealth;
    public bool isDead = false;
    private float _destroyCollider = 5f; //죽는 애니메이션 추가시 활용
    [HideInInspector] public float baseDamage = 10f;


    private void Awake()
    {
        if(_enemyController == null) _enemyController = GetComponent<EnemyController>();
        if(EnemyStats != null) _currentHealth = EnemyStats.maxHealth;
    }

    public void TakeDamage(float damage, Vector3 knockBackDir)
    {
        if (isDead) return;

        _currentHealth -= damage;

        if (_enemyController != null)
        {
            _enemyController.KnockbackForce = knockBackDir;
            _enemyController.HandleHit();
        }

        if(_currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        isDead = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject);
    }

    //내 위치 넘기기
    public Transform GetTransform()
    {
        return transform;
    }
}
