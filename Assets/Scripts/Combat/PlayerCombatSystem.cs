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

    public void SetTarget(Transform target)
    {
        CurrentTarget = target;
    }

    [Header("Target Detect")]
    [SerializeField] private float _detectRadius = 5.5f; //적 탐지 반경
    [SerializeField] private float _targetLockAngle = 60f; //적을 인식하는 각도범위
    [SerializeField] private float _targetLostMultiplier = 1.2f; //타겟을 잃는 거리배율
    [SerializeField] private LayerMask _enemyLayer;

    [SerializeField] private float _hitStopTimeScale = 0.05f;
    [SerializeField] private bool _useHitTimer = true;
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
    public void UpdateActionMap()
    {
        _actionMap.Clear();
        if (currentStrategy == null) return;

        foreach(var action in currentStrategy.actions)
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

    private void CheckAttackHitWindow()
    {
        if(currentStrategy == null || _actionMap.Count == 0)
        {
            IsDamageActive = false;
            return;
        }
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextInfo = _animator.GetNextAnimatorStateInfo(0);
        bool isTransitioning = _animator.IsInTransition(0);

        int targetHash = 0;
        float currentNormalizedTime = 0f;
        bool isAttackState = false;

        if (isTransitioning)
        {
            targetHash = nextInfo.shortNameHash;
            currentNormalizedTime = nextInfo.normalizedTime;
        }
        else
        {
            targetHash = stateInfo.shortNameHash;
            currentNormalizedTime = stateInfo.normalizedTime;
        }    
        if (_actionMap.TryGetValue(targetHash, out AttackAction currentAction))
        {
            isAttackState = true;
            float time = currentNormalizedTime % 1.0f;

            if(time >= currentAction.startFrameHit && time <= currentAction.endFrameHit)
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
        if(!isAttackState)
        {
            DisableDamage();
        }
    }
    private void EnableDamage()
    {
        if(!IsDamageActive)
        {
            IsDamageActive = true;
            _weaponTracer.EnableTrace();
        }
    }
    private void DisableDamage()
    {
        if(IsDamageActive)
        {
            IsDamageActive = false;
            _weaponTracer.DisableTrace();
        }
    }

    //피격, 회피등으로 캔슬될 때 외부에서 호출
    public void ForceStopAttack()
    {
        DisableDamage();
        ResetCombo();
        _currentActionIndex = -1;
    }

    //타겟 갱신. 기존 타겟이 유효하면 유지하고 아니면 새로찾는다.
    public void UpdateTarget(Vector3 playerPos, Vector3 searchDir)
    {
        if (IsValidTarget(CurrentTarget, playerPos))
        {
            return;
        }
        CurrentTarget = GetNearestEnemy(playerPos, searchDir);
    }
    private bool IsValidTarget(Transform target, Vector3 playerPos)
    {
        if (target == null || !target.gameObject.activeSelf) return false;

        SandBagEnemy enemy = target.GetComponent<SandBagEnemy>();
        if(enemy != null && enemy.IsDead)
        {
            return false;
        }
        float distance = Vector3.Distance(playerPos, target.position);
        return distance <= _detectRadius * _targetLostMultiplier;
    }
    public Transform GetNearestEnemy(Vector3 playerPos, Vector3 searchDir)
    {
        int count = Physics.OverlapSphereNonAlloc(playerPos, _detectRadius, _enemyBuffer, _enemyLayer);
        Transform nearestTarget = null;
        float minDistance = float.MaxValue;

        //탐색방향 결정
        Vector3 checkDir = searchDir.sqrMagnitude > 0.01f ? searchDir : transform.forward;

        for (int i = 0; i < count; i++)
        {
            Collider collider = _enemyBuffer[i];
            if (!collider.gameObject.activeSelf) continue;

            Vector3 toEnemy = collider.transform.position - playerPos;
            toEnemy.y = 0;

            float angle = Vector3.Angle(checkDir, toEnemy);
            if (angle > _targetLockAngle) continue;


            //근접한 적 선택
            float dist = toEnemy.sqrMagnitude;
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestTarget = collider.transform;
            }
        }
        return nearestTarget;
    }
    public void ClearTarget()
    {
        CurrentTarget = null;
    }

    public void OnWeaponHit(IDamageable target, Vector3 hitPoint, Vector3 hitDirection)
    {

        if(_controller != null)
        {
            _controller.playerManager.RestoreStamina(_controller.playerStats.staminaRecoveryOnHit);
        }

        AttackAction action = _activeAttackAction;
        if (action == null) action = GetCurrentAttackAction();
        if (action == null) return;

        Vector3 targetPos = target.GetTransform().position;
        Vector3 knockBackDir = (targetPos - transform.position).normalized;
        knockBackDir.y = 0;

        float finalDamage = currentStrategy.baseDamage * action.damageMultiplier;
        target.TakeDamage(finalDamage, knockBackDir);
        Debug.Log($"데미지 적용 : {finalDamage} -> {target}");

        if(action.hitStopDuration > 0 && _hitTimer != null)
        {
            //GetComponentInChildren<Animator>();
            Animator enemyAnim = (target as Component)?.GetComponent<Animator>();
            _hitTimer.StartHitStop(action.hitStopDuration, enemyAnim);
        }
        if (action.hitVFX != null)
        {
            VFXManager.Instance.PlayVFX(action.hitVFX, hitPoint, Quaternion.LookRotation(transform.forward));
        }
    }
    private void PlayAttackAnim(int index)
    {
        if (index >= currentStrategy.actions.Count) return;

        _currentActionIndex = index;
        AttackAction action = currentStrategy.actions[index];

        _animator.ResetTrigger(action.attackName);
        _animator.SetTrigger(action.attackName);
        _animator.SetInteger(PlayerController.AnimIDComboCount, index);

        CanCombo = false;
    }


    public void ExecuteAttack(CombatCommand commandType)
    {
        if (currentStrategy == null) return;
        if (_currentActionIndex == -1)
        {
            int startIndex = currentStrategy.GetStartingIndex(commandType);
            if(startIndex != -1)
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

    public AttackAction GetCurrentAttackAction()
    {
        int currentHash = 0;
        if (_animator.IsInTransition(0))
        {
            currentHash = _animator.GetNextAnimatorStateInfo(0).shortNameHash;
        }
        else
        {
            currentHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        }

        if(_actionMap.TryGetValue(currentHash, out AttackAction action))
        {
            return action;
        }
        return null;

    }
    private ComboConnection FindNextCombo(AttackAction action, CombatCommand commandType)
    {
        foreach (var combo in action.nextCombos)
        {
            if (combo.commandType == commandType)
            {
                return combo;
            }
        }
        return null;
    }
    public void SetComboWindow(int state) => CanCombo = (state == 1);
    public void ResetComboWindow() => CanCombo = false;

    public void ResetCombo()
    {
        _currentActionIndex = -1;
        _animator.SetInteger(PlayerController.AnimIDComboCount, 0);
    }
}