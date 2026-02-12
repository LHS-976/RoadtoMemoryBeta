using PlayerControllerScripts;
using System.Collections;
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

public class PlayerCombatSystem : MonoBehaviour
{
    [SerializeField] private PlayerController _controller;
    [SerializeField] private Animator _animator;
    [SerializeField] private WeaponHandler _weaponHandler;

    [Header("Strategy Data")]
    public ComboStrategySO currentStrategy;
    public List<ComboStrategySO> availableStyles;

    private int _currentActionIndex = -1;
    public bool CanCombo { get; private set; }
    public Transform CurrentTarget { get; private set; }
    private Coroutine _currentHitStop;

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
    private Collider[] _enemyBuffer = new Collider[20];

    public void Initialize(PlayerController controller, Animator animator)
    {
        _controller = controller;
        _animator = animator;

        if(_weaponHandler == null)
        {
            _weaponHandler = GetComponent<WeaponHandler>();
        }
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
            //테스트용
            SandBagEnemy enemyScript = collider.GetComponent<SandBagEnemy>();
            if (enemyScript != null && enemyScript.IsDead) continue;
            //

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

    private void PlayAttack(int index)
    {
        if (index >= currentStrategy.actions.Count) return;

        _currentActionIndex = index;
        AttackAction action = currentStrategy.actions[index];

        float finalDamage = currentStrategy.baseDamage * action.damageMultiplier;

        var hitBox = _weaponHandler.GetComponentInChildren<WeaponHitBox>();
        if (hitBox != null)
        {
            hitBox.SetDamage(finalDamage);
        }
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
                PlayAttack(startIndex);
            }
            return;
        }
        AttackAction currentAction = currentStrategy.actions[_currentActionIndex];
        ComboConnection connection = FindNextCombo(currentAction, commandType);

        if (connection != null)
        {
            PlayAttack(connection.nextComboIndex);
        }
    }

    public AttackAction GetCurrentAttackAction()
    {
        if (currentStrategy == null || _currentActionIndex == -1) return null;
        if (_currentActionIndex >= currentStrategy.actions.Count) return null;
        return currentStrategy.actions[_currentActionIndex];
    }
    public void OnHit(Vector3 hitPoint)
    {
        if(_controller != null && _controller.playerManager != null)
        {
            _controller.playerManager.RestoreStamina(_controller.playerStats.staminaRecoveryOnHit);
        }
        AttackAction action = GetCurrentAttackAction();
        if (action == null) return;

        if (action.hitStopDuration > 0)
        {
            _currentHitStop = StartCoroutine(HitStopRoutine(action.hitStopDuration));
        }
        if (action.hitVFX != null)
        {
            VFXManager.Instance.PlayVFX(action.hitVFX, hitPoint, Quaternion.LookRotation(transform.forward));
        }
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
    private IEnumerator HitStopRoutine(float duration)
    {
        if(_currentHitStop != null)
        {
            StopCoroutine(_currentHitStop);
        }

        float originalTimeScale = Time.timeScale;
        Time.timeScale = _hitStopTimeScale;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
        _currentHitStop = null;
    }
    private void OnDisable()
    {
        if(_currentHitStop != null)
        {
            StopCoroutine(_currentHitStop);
            Time.timeScale = 1f;
        }
    }

    public void SetComboWindow(int state) => CanCombo = (state == 1);
    public void ResetComboWindow() => CanCombo = false;

    public void ResetCombo()
    {
        _currentActionIndex = -1;
        _animator.SetInteger(PlayerController.AnimIDComboCount, 0);
    }
}