using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable
{
    public EnemyStatsSO _enemyStats;
    [SerializeField] private EnemyController _enemyController;

    private float currentHealth;
    public bool isDead = false;
    private float destroyCollider = 5f; //죽는 애니메이션 추가시 활용

    private void Awake()
    {
        if(_enemyController) _enemyController = GetComponent<EnemyController>();
        if(_enemyStats != null) currentHealth = _enemyStats.maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (_enemyController != null) _enemyController.HandleHit();

        if(currentHealth <= 0)
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
