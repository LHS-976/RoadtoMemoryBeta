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
    public bool IsParryTime { get; private set; }
    public bool IsGroggy { get; private set; }
    public bool isDead = false;

    private const float DestroyDelay = 4.5f;

    private void Awake()
    {
        if (_enemyController == null) _enemyController = GetComponent<EnemyController>();
        if (_enemyAnim == null) _enemyAnim = GetComponentInChildren<EnemyAnimation>();

        if (_headParryUITransform != null)
        {
            Animator anim = GetComponentInChildren<Animator>();
            if (anim != null)
            {
                _headParryUITransform = anim.GetBoneTransform(HumanBodyBones.Head);
            }
        }

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

        if (IsGroggy)
        {
            finalHealthDamage *= EnemyStats.groggyDamageMultiplier;
        }
        else if (CurrentArmor > 0)
        {
            finalHealthDamage *= 1.0f - EnemyStats.damageReduce;
        }

        CurrentHealth -= finalHealthDamage;

        if (!IsGroggy)
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
        if (IsGroggy || IsParryTime) return;

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
        IsParryTime = true;
        GameEventManager.TriggerParryWindowOpen(_headParryUITransform);
        StartCoroutine(ParryPossibleTime());
    }

    private IEnumerator ParryPossibleTime()
    {
        yield return new WaitForSeconds(EnemyStats.canParryDuration);

        if (IsParryTime && !IsGroggy)
        {
            ParryFailure();
        }
    }

    private void ParryFailure()
    {
        GameEventManager.TriggerParryWindowClose(_headParryUITransform);
        IsParryTime = false;
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
        IsGroggy = true;
        IsParryTime = false;
        CurrentArmor = 0;

        _enemyController.HandleGroggy();
    }

    public void RecoverFromGroggy()
    {
        IsGroggy = false;
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