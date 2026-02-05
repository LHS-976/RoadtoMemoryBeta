using PlayerControllerScripts;
using UnityEngine;

public class PlayerCombatState : PlayerBaseState
{
    private bool _gotInput;
    private float _lastInputTime;
    private float _maxComboDelay = 1.0f;

    private bool _isAttacking;
    public PlayerCombatState(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        player.CombatSystem.ResetCombo();
        _gotInput = false;
        _isAttacking = false;

        player.Animator.SetBool(PlayerController.AnimIDCombat, true);
        player.MoveSpeed = player.playerStats.CombatWalkSpeed;

    }
    public override void OnUpdate()
    {
        if(!player.IsCombatMode)
        {
            player.ChangeState(player.idleState);
            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            _gotInput = true;
            _lastInputTime = Time.time;
        }
        if(_isAttacking)
        {
            player.MoveSpeed = 0f;
            player.Animator.SetFloat(PlayerController.AnimIDSpeed, 0f);
        }
        else
        {
            if (_gotInput && (Time.time - _lastInputTime < _maxComboDelay))
            {
                StartAttack();
            }
            else
            {
                HandleCombatMovement();
            }
        }


    }
    public override void OnExit()
    {
        player.Animator.SetBool(PlayerController.AnimIDCombat, false);
    }
    private void StartAttack()
    {
        _isAttacking = true;
        _gotInput = false;

        player.CombatSystem.ExecuteAttack();
    }
    private void HandleCombatMovement()
    {
        bool isSprinting = player.IsSprint;

        if (player.InputVector.sqrMagnitude > 0)
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
        player.Animator.SetFloat(PlayerController.AnimIDCombat, targetAnimSpeed, 0.1f, Time.deltaTime);
        player.Animator.SetFloat(PlayerController.AnimIDInputX, player.InputVector.x, 0.1f, Time.deltaTime);
        player.Animator.SetFloat(PlayerController.AnimIDInputY, player.InputVector.y, 0.1f, Time.deltaTime);

        player.HandleMovement(player.InputVector);
    }

    public void OnComboCheck()
    {
        if(_gotInput && (Time.time - _lastInputTime < _maxComboDelay))
        {
            StartAttack();
        }
        else
        {
            _isAttacking = false;
            _gotInput = false;
            player.CombatSystem.ResetCombo();
        }
    }
}
