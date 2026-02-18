using PlayerControllerScripts;
using UnityEngine;

public class PlayerCombatState : PlayerBaseState
{

    private CombatCommand _lastCommand;
    private bool _gotInput;
    private float _lastInputTime;
    private bool _isAttacking;

    private float _lastEvasionTime = -10f;
    private float _lastActionStartTime;
    private float _lastClickTime;
    private float _lastExecutionTime = -10f;

    private const float MaxComboDelay = 0.5f;
    private const float EvasionCooldown = 0.5f;
    private const float MinInputInterval = 0.1f;
    private const float PreventDuplicateInput = 1.0f;
    private const float MinActionDuration = 0.15f;


    public bool UseRootMotion { get; private set; }
    public PlayerCombatState(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        player.CombatSystem.ResetCombo();
        _gotInput = false;
        _isAttacking = false;

        DisableRootMotion();

        player.Animator.SetBool(PlayerController.AnimIDCombat, true);
        player.moveSpeed = player.playerStats.CombatWalkSpeed;

    }
    public override void OnUpdate()
    {
        if (TryExecutionInput()) return;

        HandleEvasionInput();
        HandleAttackInput();

        if (_isAttacking)
        {
            UpdateAttacking();
        }
        else
        {
            HandleStandardCombatMovement();
        }
    }
    public override void OnExit()
    {
        DisableRootMotion();
        player.Animator.SetBool(PlayerController.AnimIDCombat, false);
    }

    #region Input Handling
    private bool TryExecutionInput()
    {
        if (!Input.GetKeyDown(KeyCode.Q)) return false;
        if (Time.time - _lastExecutionTime < PreventDuplicateInput) return false;
        if (player.playerManager.CurrentStamina < player.playerStats.executionStaminaCost) return false;

        player.CombatSystem.ForceStopAttack();
        FreezeMovementAnimation();
        DisableRootMotion();

        _lastExecutionTime = Time.time;
        player.ChangeState(player.executionState);
        return true;
    }
    private void HandleEvasionInput()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;
        if (_isAttacking && IsEvasionCommand(_lastCommand)) return;
        if (Time.time - _lastEvasionTime < EvasionCooldown) return;

        if (!player.playerManager.UseStamina(player.playerStats.dashStaminaCost)) return;

        CombatCommand? evasionCmd = GetEvasionDirection();
        if(evasionCmd.HasValue)
        {
            _lastEvasionTime = Time.time;
            ExecuteCommand(evasionCmd.Value);
        }
    }
    private CombatCommand? GetEvasionDirection()
    {
        float x = player.InputVector.x;
        float y = player.InputVector.y;

        if (y < -0.1f) return CombatCommand.Evasion_Back;
        if (y > 0.1f) return CombatCommand.Evasion_Forward;
        if (x > 0.1f) return CombatCommand.Evasion_Right;
        if (x < -0.1f) return CombatCommand.Evasion_Left;

        return null;
    }
    private void HandleAttackInput()
    {
        if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1)) return;
        if (Time.time - _lastClickTime < MinInputInterval) return;

        _lastClickTime = Time.time;

        bool isRightClick = Input.GetMouseButtonDown(1);
        CombatCommand cmd = GetAttackCommand(isHeavy: isRightClick);
        ExecuteCommand(cmd);
    }
    #endregion

    #region Command Execution
    private void ExecuteCommand(CombatCommand cmd)
    {
        _gotInput = true;
        _lastCommand = cmd;
        _lastInputTime = Time.time;

        bool isEvasion = IsEvasionCommand(cmd);

        if(_isAttacking)
        {
            if(isEvasion)
            {
                _isAttacking = false;
                player.CombatSystem.ResetCombo();
            }
            else
            {
                if(player.CombatSystem.CanCombo)
                {
                    PerformAttack();
                }
                return;
            }
        }
        _lastActionStartTime = Time.time;
        _isAttacking = true;
        _gotInput = false;

        FreezeMovementAnimation();
        if (isEvasion)
        {
            RotateForEvasion();
            player.playerManager.SetInvincible(true);
        }
        else
        {
            AutoAimAndRotate();
        }
        EnableRootMotion();
        player.CombatSystem.ExecuteAttack(cmd);
    }
    /// <summary>
    /// 콤보 연계 시 공격 실행.
    /// </summary>
    private void PerformAttack()
    {
        AutoAimAndRotate();
        FreezeMovementAnimation();
        EnableRootMotion();

        _lastActionStartTime = Time.time;
        _isAttacking = true;
        _gotInput = false;

        player.CombatSystem.ResetComboWindow();
        player.CombatSystem.ExecuteAttack(_lastCommand);
    }
    #endregion


    #region Combat Movement
    private void UpdateAttacking()
    {
        FreezeMovementAnimation();

        if (!IsEvasionCommand(_lastCommand))
        {
            player.HandleAttackRotation();
        }
    }

    private void HandleStandardCombatMovement()
    {
        Vector3 dir = player.GetTargetDirection(player.InputVector);
        Transform target = player.CombatSystem.CurrentTarget;

        //스프린트 및 이동속도
        bool isSprinting = player.IsSprint;
        bool hasInput = player.InputVector.sqrMagnitude > 0;

        player.moveSpeed = hasInput
            ? (isSprinting ? player.playerStats.CombatRunSpeed : player.playerStats.CombatWalkSpeed)
            : 0f;

        float targetAnimSpeed = hasInput ? (isSprinting ? 1.0f : 0.5f) : 0f;

        if (target != null)
        {
            //타겟이 있으면 타겟을 향해 회전하며 이동
            Vector3 dirToTarget = target.position - player.transform.position;
            dirToTarget.y = 0;

            if (dirToTarget.sqrMagnitude > 0.001f)
            {
                player.HandleLockOnRotation();
            }
            player.HandlePosition(dir);

            Vector3 localDir = player.transform.InverseTransformDirection(dir);
            player.Animator.SetFloat(PlayerController.AnimIDSpeed, targetAnimSpeed, 0.1f, Time.deltaTime);
            player.Animator.SetFloat(PlayerController.AnimIDInputX, localDir.x, 0.1f, Time.deltaTime);
            player.Animator.SetFloat(PlayerController.AnimIDInputY, localDir.z, 0.1f, Time.deltaTime);
        }
        else
        {
            //타겟 없으면 카메라 방향 기준 이동
            Vector3 lookDir = player.MainCameraTransform.forward;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f) lookDir.Normalize();

            player.Animator.SetFloat(PlayerController.AnimIDSpeed, targetAnimSpeed, 0.1f, Time.deltaTime);
            player.Animator.SetFloat(PlayerController.AnimIDInputX, player.InputVector.x, 0.1f, Time.deltaTime);
            player.Animator.SetFloat(PlayerController.AnimIDInputY, player.InputVector.y, 0.1f, Time.deltaTime);

            if (lookDir != Vector3.zero)
            {
                player.HandleRotation(lookDir, isInstant: false);
            }
            player.HandlePosition(dir);
        }
    }
    #endregion

    #region Combo & Animation Callbacks
    public void OnComboCheck()
    {
        player.CombatSystem.SetComboWindow(1);

        if (_gotInput && (Time.time - _lastInputTime < MaxComboDelay))
        {
            PerformAttack();
        }
        else
        {
            _gotInput = false;
            DisableRootMotion();
        }
    }
    public void OnAnimationEnd()
    {
        if (Time.time - _lastActionStartTime < MinActionDuration) return;
        _isAttacking = false;
        player.playerManager.SetInvincible(false);
        DisableRootMotion();

        player.CombatSystem.ResetCombo();
    }
    #endregion

    #region Root Motion Control

    private void EnableRootMotion()
    {
        UseRootMotion = true;
        player.Animator.applyRootMotion = true;
    }
    public void DisableRootMotion()
    {
        UseRootMotion = false;
        player.Animator.applyRootMotion = false;
    }
    #endregion

    #region Utility

    private CombatCommand GetAttackCommand(bool isHeavy)
    {
        if (player.InputVector.y > 0.5f)
        {
            return isHeavy ? CombatCommand.Forward_Heavy : CombatCommand.Forward_Light;
        }
        return isHeavy ? CombatCommand.Stand_Heavy : CombatCommand.Stand_Light;

    }
    private bool IsEvasionCommand(CombatCommand cmd)
    {
        return cmd == CombatCommand.Evasion_Back || cmd == CombatCommand.Evasion_Forward || cmd == CombatCommand.Evasion_Right || cmd == CombatCommand.Evasion_Left;
    }

    private Vector3 GetSearchDirection()
    {
        if (player.InputVector.y < -0.1f || player.InputVector.sqrMagnitude < 0.01f)
        {
            return player.transform.forward;
        }
        return player.GetTargetDirection(player.InputVector);
    }
    private Vector3 GetAttackDirection(Transform target, Vector3 searchDir)
    {
        if(target != null)
        {
            Vector3 toTarget = target.position - player.transform.position;
            toTarget.y = 0;
            return toTarget.normalized;
        }
        Vector3 camForward = player.MainCameraTransform.forward;
        camForward.y = 0;
        if(camForward.sqrMagnitude < 0.01f)
        {
            return player.transform.forward;
        }
        return camForward.normalized;
    }
    private void AutoAimAndRotate()
    {
        Vector3 searchDir = GetSearchDirection();
        player.CombatSystem.UpdateTarget(player.transform.position, searchDir);

        Transform target = player.CombatSystem.CurrentTarget;
        Vector3 attackDir = GetAttackDirection(target, searchDir);

        if (attackDir != Vector3.zero)
        {
            player.HandleRotation(attackDir, isInstant: true);
        }
    }
    private void RotateForEvasion()
    {
        Vector3 searchDir = GetSearchDirection();
        Vector3 attackDir = GetAttackDirection(player.CombatSystem.CurrentTarget, searchDir);
        player.HandleRotation(attackDir, isInstant: true);
    }
    #endregion
}
