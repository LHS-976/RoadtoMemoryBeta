using Core;
using EnemyControllerScripts;
using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable
{
    [field: SerializeField] public EnemyStatsSO EnemyStats { get; private set; }
    [SerializeField] private EnemyController _enemyController;
    [SerializeField] private EnemyAnimation _enemyAnim;
    [SerializeField] private Transform _headExecutionUITransform;

    [Header("Quest Info")]
    public string enemyID = "Robot_01";

    [Header("Broadcasting")]
    [SerializeField] private StringEventChannelSO _questKillChannel;

    public float CurrentHealth { get; private set; }
    public float CurrentArmor { get; private set; }
    public bool IsExecutionTime { get; private set; }
    public bool IsGroggy { get; private set; }
    public bool isDead = false;

    private const float DestroyDelay = 4.5f;

    private void Awake()
    {
        if (_enemyController == null) _enemyController = GetComponent<EnemyController>();
        if (_enemyAnim == null) _enemyAnim = GetComponentInChildren<EnemyAnimation>();

        if (_headExecutionUITransform == null)
        {
            Animator anim = GetComponentInChildren<Animator>();
            if (anim != null)
            {
                _headExecutionUITransform = anim.GetBoneTransform(HumanBodyBones.Head);
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
        if (IsGroggy || IsExecutionTime) return;

        CurrentArmor -= amount;

        if (CurrentArmor <= 0)
        {
            CurrentArmor = 0;
            TriggerExecutionPossible();
        }
    }

    #region Execution & Groggy

    private void TriggerExecutionPossible()
    {
        IsExecutionTime = true;
        GameEventManager.TriggerExecutionWindowOpen(_headExecutionUITransform);
        StartCoroutine(ExecutionPossibleTime());
    }

    private IEnumerator ExecutionPossibleTime()
    {
        yield return new WaitForSeconds(EnemyStats.canExecutionDuration);

        if (IsExecutionTime && !IsGroggy)
        {
            ExecutionFailure();
        }
    }

    private void ExecutionFailure()
    {
        GameEventManager.TriggerExecutionWindowClose(_headExecutionUITransform);
        IsExecutionTime = false;
        CurrentArmor = EnemyStats.defenseArmor;
    }

    public void HandleExecutionHit()
    {
        StopAllCoroutines();
        GameEventManager.TriggerExecutionWindowClose(_headExecutionUITransform);
        TriggerGroggy();
    }

    private void TriggerGroggy()
    {
        IsGroggy = true;
        IsExecutionTime = false;
        CurrentArmor = 0;

        _enemyController.HandleGroggy();
    }

    public void RecoverFromGroggy()
    {
        IsGroggy = false;
        CurrentArmor = 0;
    }

    #endregion

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        if (_enemyController != null)
        {
            _enemyController.HandleDie();
        }

        if (_questKillChannel != null)
        {
            _questKillChannel.RaiseEvent(enemyID);
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