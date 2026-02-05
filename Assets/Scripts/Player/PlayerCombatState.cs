using PlayerControllerScripts;
using UnityEngine;

public class PlayerCombatState : PlayerBaseState
{
    public PlayerCombatState(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        player.MoveSpeed = player.playerStats.CombatWalkSpeed;
        player.Animator.SetBool(player.AnimIDCombat, true);

    }
    public override void OnUpdate()
    {
        if(!player.IsCombatMode)
        {
            player.ChangeState(player.idleState);
            return;
        }
        bool isSprinting = player.IsSprint;

        if(player.InputVector.sqrMagnitude > 0)
        {
            player.MoveSpeed = isSprinting ? player.playerStats.CombatRunSpeed : player.playerStats.CombatWalkSpeed;      
        }
        else
        {
            player.MoveSpeed = 0f;
        }

        float targetAnimSpeed = 0f;
        if(player.InputVector.sqrMagnitude > 0)
        {
            targetAnimSpeed = isSprinting ? 1.0f : 0.5f;
        }
        player.Animator.SetFloat(player.AnimIDSpeed, targetAnimSpeed, 0.1f, Time.deltaTime);
        player.Animator.SetFloat(player.AnimIDInputX, player.InputVector.x, 0.1f, Time.deltaTime);
        player.Animator.SetFloat(player.AnimIDInputY, player.InputVector.y, 0.1f, Time.deltaTime);

        player.HandleMovement(player.InputVector);
    }
    public override void OnExit()
    {
        player.Animator.SetBool(player.AnimIDCombat, false);
    }
}
