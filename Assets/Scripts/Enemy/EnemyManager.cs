using Core;
using EnemyControllerScripts;
using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable
{
    [field: SerializeField] public EnemyStatsSO EnemyStats { get; private set; }
    [SerializeField] private EnemyController _enemyController;
    [SerializeField] private EnemyAnimation _enemyAnim;
    [SerializeField] private Transform _headParryUITransform;

    public float CurrentHealth { get; private set; }
    public float CurrentArmor { get; private set; }
    public bool isParryTime { get; private set; }
    public bool isGroggy { get; private set; }
    public bool isDead = false;

    private const float DestroyDelay = 4.5f;

    private void Awake()
    {
        if (_enemyController == null) _enemyController = GetComponent<EnemyController>();
        if (_enemyAnim == null) _enemyAnim = GetComponentInChildren<EnemyAnimation>();

        if (EnemyStats != null)
        {
            CurrentHealth = EnemyStats.maxHealth;
            CurrentArmor = EnemyStats.defenseArmor;
        }
    }

    public void TakeDamage(float damage, Vector3 knockBackDir)
    {
        if (isDead) return;

        TakeArmorDamage(damage);

        float finalHealthDamage = damage;

        if (isGroggy)
        {
            finalHealthDamage *= EnemyStats.groggyDamageMultiplier;
        }
        else if (CurrentArmor > 0)
        {
            finalHealthDamage *= 1.0f - EnemyStats.damageReduce;
        }

        CurrentHealth -= finalHealthDamage;

        if (!isGroggy)
        {
            _enemyController.KnockbackForce = knockBackDir;
            _enemyController.HandleHit();
        }

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeArmorDamage(float amount)
    {
        if (isGroggy || isParryTime) return;

        CurrentArmor -= amount;

        if (CurrentArmor <= 0)
        {
            CurrentArmor = 0;
            TriggerParryPossible();
        }
    }

    #region Parry & Groggy

    private void TriggerParryPossible()
    {
        isParryTime = true;
        GameEventManager.TriggerParryWindowOpen(_headParryUITransform);
        StartCoroutine(ParryPossibleTime());
    }

    private IEnumerator ParryPossibleTime()
    {
        yield return new WaitForSeconds(EnemyStats.canParryDuration);

        if (isParryTime && !isGroggy)
        {
            ParryFailure();
        }
    }

    private void ParryFailure()
    {
        GameEventManager.TriggerParryWindowClose(_headParryUITransform);
        isParryTime = false;
        CurrentArmor = EnemyStats.defenseArmor;
    }

    public void HandleParryHit()
    {
        StopAllCoroutines();
        GameEventManager.TriggerParryWindowClose(_headParryUITransform);
        TriggerGroggy();
    }

    private void TriggerGroggy()
    {
        isGroggy = true;
        isParryTime = false;
        CurrentArmor = 0;

        _enemyController.HandleGroggy();
    }

    public void RecoverFromGroggy()
    {
        isGroggy = false;
        CurrentArmor = EnemyStats.defenseArmor;
    }

    #endregion

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        if (_enemyController != null)
        {
            _enemyController.HandleDie();
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, DestroyDelay);
    }

    public Transform GetTransform()
    {
        return transform;
    }
}