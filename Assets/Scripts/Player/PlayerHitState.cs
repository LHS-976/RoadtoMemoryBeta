using PlayerControllerScripts;
using UnityEngine;

public class PlayerHitState : PlayerBaseState
{
    private float _afterHitTime;
    private float _knockBackDuration;
    private float _knockBackForce;
    public PlayerHitState(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        _afterHitTime = 0;
        _knockBackDuration = player.playerStats.knockbackDuration;
        _knockBackForce = player.playerStats.knockbackPower;

        player.Animator.SetTrigger(PlayerController.AnimIDHit);
        player.CombatSystem.ForceStopAttack();
        player.combatState.DisableRootMotion();

        player.moveSpeed = 0f;
    }

    public override void OnUpdate()
    {
        _afterHitTime += Time.deltaTime;

        if(_afterHitTime < _knockBackDuration)
        {
            ApplyKnockback();
        }
        else
        {
            if(player.isCombatMode)
            {
                player.ChangeState(player.combatState);
            }
            else
            {
                player.ChangeState(player.idleState);
            }
        }
    }

    public override void OnExit()
    {
        player.moveSpeed = player.playerStats.WalkSpeed;
    }

    private void ApplyKnockback()
    {
        float currentSpeed = Mathf.Lerp(_knockBackForce, 0f, _afterHitTime / _knockBackDuration);

        Vector3 velocity = player.KnockBackForce * currentSpeed;

        player.Controller.Move(velocity * Time.deltaTime);
    }
}
