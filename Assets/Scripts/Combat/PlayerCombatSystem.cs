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

    public void SetTarget(Transform target)
    {
        CurrentTarget = target;
    }

    [SerializeField] private float _detectRadius = 5.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float _autoAimSpeed = 15.0f;

    private Collider[] _enemyBuffer = new Collider[20];

    public void Initialize(PlayerController controller, Animator animator)
    {
        _controller = controller;
        _animator = animator;
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
        ComboConnection connection = null;
        for (int i=0; i< currentAction.nextCombos.Count; i++)
        {
            if (currentAction.nextCombos[i].commandType == commandType)
            {
                connection = currentAction.nextCombos[i];
                break;
            }
        }
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

        //
        if (action.hitStopDuration > 0) StartCoroutine(HitStopRoutine(action.hitStopDuration));

        if (action.hitVFX != null)
        {
            VFXManager.Instance.PlayVFX(action.hitVFX, hitPoint, Quaternion.LookRotation(transform.forward));
        }
    }
    //
    public Transform GetNearestEnemy(Vector3 playerPos, Vector3 inputDir)
    {
        int count = Physics.OverlapSphereNonAlloc(playerPos, _detectRadius, _enemyBuffer,enemyLayer);
        Transform nearestTarget = null;
        float minDistance = float.MaxValue;

        for (int i=0; i< count; i++)
        {
            Collider collider = _enemyBuffer[i];
            if (!collider.gameObject.activeSelf) continue;
            
            Vector3 toEnemy = collider.transform.position - playerPos;
            Vector3 checkDir = inputDir == Vector3.zero ? transform.forward : inputDir;
            float angle = Vector3.Angle(checkDir, toEnemy);
            if(angle < 60f)
            {
                float dist = toEnemy.sqrMagnitude;
                if(dist < minDistance)
                {
                    minDistance = dist;
                    nearestTarget = collider.transform;
                }
            }
        }
        return nearestTarget;
    }
    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1.0f;
    }

    public void SetComboWindow(int state) => CanCombo = (state == 1);
    public void ResetComboWindow() => CanCombo = false;

    public void ResetCombo()
    {
        _currentActionIndex = -1;
        _animator.SetInteger(PlayerController.AnimIDComboCount, 0);
    }
    private void Update()
    {
        if (CurrentTarget == null) return;

        if(!CurrentTarget.gameObject.activeSelf || Vector3.Distance(transform.position, CurrentTarget.position) > _detectRadius * 1.2f)
        {
            CurrentTarget = null;
            return;
        }
    }
}