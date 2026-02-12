using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable
{
    [SerializeField] public EnemyStatsSO EnemyStats { get; private set; }
    [SerializeField] private EnemyController _enemyController;

    private float _currentHealth;
    public bool isDead = false;
    private float _destroyCollider = 5f; //죽는 애니메이션 추가시 활용


    private void Awake()
    {
        if(_enemyController) _enemyController = GetComponent<EnemyController>();
        if(EnemyStats != null) _currentHealth = EnemyStats.maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        _currentHealth -= damage;

        if (_enemyController != null) _enemyController.HandleHit();

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
}
