using PlayerControllerScripts;
using UnityEngine;

public class PlayerCombatState : PlayerBaseState
{

    private CombatCommand _lastCommand;
    private bool _gotInput;
    private float _lastInputTime;
    private float _maxComboDelay = 0.5f;
    private bool _isAttacking;

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
        player.MoveSpeed = player.playerStats.CombatWalkSpeed;

    }
    public override void OnUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            float x = player.InputVector.x;
            float y = player.InputVector.y;
            if (y < -0.1f)
            {
                ExecuteCommand(CombatCommand.Evasion_Back);
            }
            else if(y > 0.1f)
            {
                ExecuteCommand(CombatCommand.Evasion_Forward);
            }
            else if (x > 0.1f)
            {
                ExecuteCommand(CombatCommand.Evasion_Right);
            }
            else if(x<-0.1f)
            {
                ExecuteCommand(CombatCommand.Evasion_Left);
            }
            else
            {
                ExecuteCommand(CombatCommand.Evasion_Back);
            }
        }
        if(Input.GetMouseButtonDown(0))
        {
            CombatCommand cmd = GetCommand(isHeavy: false);
            ExecuteCommand(cmd);
        }
        else if(Input.GetMouseButtonDown(1))
        {
            CombatCommand cmd = GetCommand(isHeavy: true);
            ExecuteCommand(cmd);
        }
        if(_isAttacking)
        {
            player.MoveSpeed = 0f;
            player.Animator.SetFloat(PlayerController.AnimIDSpeed, 0f);
            player.Animator.SetFloat(PlayerController.AnimIDInputX, 0f);
            player.Animator.SetFloat(PlayerController.AnimIDInputY, 0f);
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
    private void EnableRootMotion()
    {
        UseRootMotion = true;
        player.Animator.applyRootMotion = true;
    }
    private void DisableRootMotion()
    {
        UseRootMotion = false;
        player.Animator.applyRootMotion = false;
    }
    private void HandleStandardCombatMovement()
    {
        Vector3 dir = player.GetTargetDirection(player.InputVector);
        Vector3 lookDir = player.MainCameraTransform.forward;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.01f) lookDir.Normalize();

        bool isSprinting = player.IsSprint;
        if(dir.sqrMagnitude >0)
        {
            player.MoveSpeed = isSprinting ? player.playerStats.CombatRunSpeed : player.playerStats.CombatWalkSpeed;
        }
        else
        {
            player.MoveSpeed = 0f;
        }

        float targetAnimSpeed = 0f;
        if (player.InputVector.sqrMagnitude > 0)
        {
            targetAnimSpeed = isSprinting ? 1.0f : 0.5f;
        }
        player.Animator.SetFloat(PlayerController.AnimIDSpeed, targetAnimSpeed, 0.1f, Time.deltaTime);
        player.Animator.SetFloat(PlayerController.AnimIDInputX, player.InputVector.x, 0.1f, Time.deltaTime);
        player.Animator.SetFloat(PlayerController.AnimIDInputY, player.InputVector.y, 0.1f, Time.deltaTime);

        if(lookDir != Vector3.zero)
        {
            player.HandleRotation(lookDir, isInstant: false);
        }
        player.HandlePosition(dir);
    }

    private CombatCommand GetCommand(bool isHeavy)
    {
        if(player.InputVector.y > 0.5f)
        {
            return isHeavy ? CombatCommand.Forward_Heavy : CombatCommand.Forward_Light;
        }
        return isHeavy ? CombatCommand.Stand_Heavy : CombatCommand.Stand_Light;

    }
    private void ExecuteCommand(CombatCommand cmd)
    {
        player._lastAttackTime = Time.time; //자동납도기능 테스트용
        _gotInput = true;
        _lastCommand = cmd;
        _lastInputTime = Time.time;
        if(_isAttacking)
        {
            return;
        }
        _isAttacking = true;
        _gotInput = false;

        Vector3 attackDir = player.GetTargetDirection(player.InputVector);
        Transform enemy = player.CombatSystem.GetNearestEnemy(player.transform.position, attackDir);
        if(enemy != null)
        {
            Vector3 toEnemy = enemy.position - player.transform.position;
            toEnemy.y = 0;
            attackDir = toEnemy.normalized;
        }
        bool isEvasion = (cmd == CombatCommand.Evasion_Back ||
                          cmd == CombatCommand.Evasion_Forward ||
                          cmd == CombatCommand.Evasion_Right ||
                          cmd == CombatCommand.Evasion_Left);

        if(!isEvasion && attackDir != Vector3.zero)
        {
            player.HandleRotation(attackDir, isInstant: true);
        }
        else if(isEvasion && enemy != null)
        {
            player.HandleRotation(attackDir, isInstant: true);
        }
        EnableRootMotion();
        player.CombatSystem.ExecuteAttack(cmd);
    }
    private void StartAttack()
    {
        EnableRootMotion();
        _isAttacking = true;
        _gotInput = false;
        player.CombatSystem.ResetComboWindow();
        player.CombatSystem.ExecuteAttack(_lastCommand);
    }

    public void OnComboCheck()
    {
        player.CombatSystem.SetComboWindow(1);

        if (_gotInput && (Time.time - _lastInputTime < _maxComboDelay))
        {
            StartAttack();
        }
        else
        {
            _isAttacking = false;
            _gotInput = false;
            player.CombatSystem.ResetCombo();
            DisableRootMotion();
        }
    }
    public void OnAnimationEnd()
    {
        _isAttacking = false;
        DisableRootMotion();

        player.CombatSystem.ResetCombo();
    }
}
