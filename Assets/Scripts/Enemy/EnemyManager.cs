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

    [Header("Broadcasting")]
    [SerializeField] private StringEventChannelSO _questKillChannel;
    [SerializeField] private EnemyDamageUIEventChannelSO _damageUIChannel;

    [field: SerializeField] public string EnemyUID { get; private set; }

    private const float _corpseObstacle = 1.5f;

    public float CurrentHealth { get; private set; }
    public float CurrentArmor { get; private set; }
    public bool IsExecutionTime { get; private set; }
    public bool IsGroggy { get; private set; }
    public bool isDead = false;

    private bool _hasBeenGroggy = false;
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

        if(_damageUIChannel != null)
        {
            DamageUIPayLoad payLoad = new DamageUIPayLoad
            {
                targetEnemy = this.transform,
                damageAmount = finalHealthDamage,
                currentHealth = CurrentHealth,
                maxHealth = EnemyStats.maxHealth,
                hitPoint = _headExecutionUITransform != null ? _headExecutionUITransform.position : this.transform.position
            };
            _damageUIChannel.RaiseEvent(payLoad);
        }
        if(CurrentHealth <= 0)
        {
            Die();
        }
        else if(!IsGroggy)
        {
            _enemyController.KnockbackForce = knockBackDir;
            _enemyController.HandleHit();
        }
    }

    public void TakeArmorDamage(float amount)
    {
        if (IsGroggy || IsExecutionTime || _hasBeenGroggy) return;
        if (EnemyStats.defenseArmor <= 0) return;

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

        _hasBeenGroggy = true;
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
            _questKillChannel.RaiseEvent(EnemyStats.enemyID);
        }
        StartCoroutine(ObstacleRoutine());
        Destroy(gameObject, DestroyDelay);

        GameData data = Core.GameCore.Instance?.DataManager?.CurrentData;
        if (data != null && !string.IsNullOrEmpty(EnemyUID))
        {
            if (!data.DefeatedEnemyIDs.Contains(EnemyUID))
                data.DefeatedEnemyIDs.Add(EnemyUID);
        }

    }

    public Transform GetTransform()
    {
        return transform;
    }
    //공격중 통과를 살짝 막기.
    private IEnumerator ObstacleRoutine()
    {
        yield return new WaitForSeconds(_corpseObstacle);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}