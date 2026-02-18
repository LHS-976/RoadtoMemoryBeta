using PlayerControllerScripts;
using System.Collections.Generic;
using UnityEngine;

public enum CombatCommand
{
    Stand_Light,
    Forward_Light,
    Stand_Heavy,
    Forward_Heavy,
    Evasion_Back,
    Evasion_Forward,
    Evasion_Right,
    Evasion_Left
}

public class PlayerCombatSystem : MonoBehaviour, IWeaponHitRange
{
    [SerializeField] private PlayerController _controller;
    [SerializeField] private Animator _animator;

    [SerializeField] private WeaponTracer _weaponTracer;
    [SerializeField] private HitTimerController _hitTimer;

    [Header("Strategy Data")]
    public ComboStrategySO currentStrategy;
    public List<ComboStrategySO> availableStyles;

    private int _currentActionIndex = -1;
    private AttackAction _activeAttackAction;
    public bool CanCombo { get; private set; }
    public Transform CurrentTarget { get; private set; }

    [Header("Damage Settings")]
    [SerializeField] private float _heavyDefenseAttackDmgMultiplier = 2.0f;

    [Header("Target Detect")]
    [SerializeField] private float _detectRadius = 5.5f;
    [SerializeField] private float _targetLockAngle = 60f;
    [SerializeField] private float _targetLostMultiplier = 1.2f;
    [SerializeField] private LayerMask _enemyLayer;

    private Collider[] _enemyBuffer = new Collider[20];

    public bool IsDamageActive { get; private set; }

    private Dictionary<int, AttackAction> _actionMap = new Dictionary<int, AttackAction>();

    public void Initialize(PlayerController controller, Animator animator)
    {
        _controller = controller;
        _animator = animator;

        if (_weaponTracer == null) _weaponTracer = GetComponentInChildren<WeaponTracer>();
        if (_hitTimer == null) _hitTimer = GetComponent<HitTimerController>();
        if (_weaponTracer != null) _weaponTracer.Initialize(this);
        if (currentStrategy != null) UpdateActionMap();
    }

    public void SetTarget(Transform target)
    {
        CurrentTarget = target;
    }

    public void UpdateActionMap()
    {
        _actionMap.Clear();
        if (currentStrategy == null) return;

        foreach (var action in currentStrategy.actions)
        {
            int hash = Animator.StringToHash(action.attackName);
            if (!_actionMap.ContainsKey(hash))
            {
                _actionMap.Add(hash, action);
            }
        }
    }

    private void Update()
    {
        CheckAttackHitWindow();
    }

    #region Hit Window & Damage

    private void CheckAttackHitWindow()
    {
        if (currentStrategy == null || _actionMap.Count == 0)
        {
            IsDamageActive = false;
            return;
        }

        //트랜지션 중에는 다음 상태를, 아니면 현재 상태를 기준으로 판정
        bool isTransitioning = _animator.IsInTransition(0);
        AnimatorStateInfo stateInfo = isTransitioning
            ? _animator.GetNextAnimatorStateInfo(0)
            : _animator.GetCurrentAnimatorStateInfo(0);

        if (_actionMap.TryGetValue(stateInfo.shortNameHash, out AttackAction currentAction))
        {
            float time = stateInfo.normalizedTime % 1.0f;

            if (time >= currentAction.startFrameHit && time <= currentAction.endFrameHit)
            {
                _activeAttackAction = currentAction;
                EnableDamage();
            }
            else
            {
                DisableDamage();
                _activeAttackAction = null;
            }
        }
        else
        {
            DisableDamage();
        }
    }

    private void EnableDamage()
    {
        if (!IsDamageActive)
        {
            IsDamageActive = true;
            _weaponTracer.EnableTrace();
        }
    }

    private void DisableDamage()
    {
        if (IsDamageActive)
        {
            IsDamageActive = false;
            _weaponTracer.DisableTrace();
        }
    }

    #endregion

    #region Attack Execution

    public void ExecuteAttack(CombatCommand commandType)
    {
        if (currentStrategy == null) return;

        if (_currentActionIndex == -1)
        {
            int startIndex = currentStrategy.GetStartingIndex(commandType);
            if (startIndex != -1)
            {
                PlayAttackAnim(startIndex);
            }
            return;
        }

        AttackAction currentAction = currentStrategy.actions[_currentActionIndex];
        ComboConnection connection = FindNextCombo(currentAction, commandType);

        if (connection != null)
        {
            PlayAttackAnim(connection.nextComboIndex);
        }
    }

    private void PlayAttackAnim(int index)
    {
        if (index >= currentStrategy.actions.Count) return;

        _currentActionIndex = index;
        AttackAction action = currentStrategy.actions[index];

        _animator.CrossFadeInFixedTime(action.attackName, 0.1f);
        _animator.SetInteger(PlayerController.AnimIDComboCount, index);
        _animator.speed = currentStrategy.attackSpeed;

        CanCombo = false;
    }
    public void ForceStopAttack()
    {
        DisableDamage();
        ResetCombo();
        _currentActionIndex = -1;
    }

    #endregion

    #region Weapon Hit Callback

    public void OnWeaponHit(IDamageable target, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (_controller != null)
        {
            _controller.playerManager.RestoreStamina(_controller.playerStats.staminaRecoveryOnHit);
        }

        AttackAction action = _activeAttackAction ?? GetCurrentAttackAction();
        if (action == null) return;

        Vector3 knockBackDir = (target.GetTransform().position - transform.position).normalized;
        knockBackDir.y = 0;

        float finalDamage = currentStrategy.baseDamage * action.damageMultiplier;

        ApplyArmorDamage(target, action, finalDamage);
        target.TakeDamage(finalDamage, knockBackDir);

        ApplyHitStop(target, action);
        ApplyHitVFX(action, hitPoint);

        Debug.Log($"데미지 적용 : {finalDamage} -> {target}");
    }

    private void ApplyArmorDamage(IDamageable target, AttackAction action, float baseDamage)
    {
        EnemyManager enemy = target.GetTransform().GetComponent<EnemyManager>();
        if (enemy == null) return;

        if (action.attackName.Contains("Heavy"))
        {
            float bonusArmorDmg = baseDamage * (_heavyDefenseAttackDmgMultiplier - 1.0f);
            if (bonusArmorDmg > 0)
            {
                enemy.TakeArmorDamage(bonusArmorDmg);
            }
        }
    }

    private void ApplyHitStop(IDamageable target, AttackAction action)
    {
        if (action.hitStopDuration <= 0 || _hitTimer == null) return;

        Animator enemyAnim = target.GetTransform().GetComponent<Animator>();
        if (enemyAnim == null) enemyAnim = target.GetTransform().GetComponentInChildren<Animator>();

        _hitTimer.StartHitStop(action.hitStopDuration, enemyAnim);
    }

    private void ApplyHitVFX(AttackAction action, Vector3 hitPoint)
    {
        if (action.hitVFX != null)
        {
            VFXManager.Instance.PlayVFX(action.hitVFX, hitPoint, Quaternion.LookRotation(transform.forward));
        }
    }

    #endregion

    #region Target Detection

    /// <summary>
    ///타겟 갱신. 기존 타겟이 유효하면 유지하고 아니면 새로 찾는다.
    /// </summary>
    public void UpdateTarget(Vector3 playerPos, Vector3 searchDir)
    {
        if (IsValidTarget(CurrentTarget, playerPos)) return;

        CurrentTarget = GetNearestEnemy(playerPos, searchDir);
    }

    private bool IsValidTarget(Transform target, Vector3 playerPos)
    {
        if (target == null || !target.gameObject.activeSelf) return false;

        //IDamageable 기반으로 사망 판별
        EnemyManager enemy = target.GetComponent<EnemyManager>();
        if (enemy != null && enemy.isDead) return false;

        float distance = Vector3.Distance(playerPos, target.position);
        return distance <= _detectRadius * _targetLostMultiplier;
    }

    public Transform GetNearestEnemy(Vector3 playerPos, Vector3 searchDir)
    {
        int count = Physics.OverlapSphereNonAlloc(playerPos, _detectRadius, _enemyBuffer, _enemyLayer);
        Transform nearestTarget = null;
        float minDistance = float.MaxValue;

        Vector3 checkDir = searchDir.sqrMagnitude > 0.01f ? searchDir : transform.forward;

        for (int i = 0; i < count; i++)
        {
            Collider col = _enemyBuffer[i];
            if (!col.gameObject.activeSelf) continue;

            Vector3 toEnemy = col.transform.position - playerPos;
            toEnemy.y = 0;

            float angle = Vector3.Angle(checkDir, toEnemy);
            if (angle > _targetLockAngle) continue;

            float dist = toEnemy.sqrMagnitude;
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestTarget = col.transform;
            }
        }
        return nearestTarget;
    }

    public void ClearTarget()
    {
        CurrentTarget = null;
    }

    #endregion

    #region Combo Control

    public AttackAction GetCurrentAttackAction()
    {
        int currentHash;
        if (_animator.IsInTransition(0))
        {
            currentHash = _animator.GetNextAnimatorStateInfo(0).shortNameHash;
        }
        else
        {
            currentHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        }

        return _actionMap.TryGetValue(currentHash, out AttackAction action) ? action : null;
    }

    private ComboConnection FindNextCombo(AttackAction action, CombatCommand commandType)
    {
        if (action.nextCombos == null) return null;

        foreach (var combo in action.nextCombos)
        {
            if (combo.commandType == commandType) return combo;
        }
        return null;
    }

    public void SetComboWindow(int state) => CanCombo = (state == 1);
    public void ResetComboWindow() => CanCombo = false;

    public void ResetCombo()
    {
        _currentActionIndex = -1;
        _animator.SetInteger(PlayerController.AnimIDComboCount, 0);
        _animator.speed = 1f;
    }

    #endregion
}