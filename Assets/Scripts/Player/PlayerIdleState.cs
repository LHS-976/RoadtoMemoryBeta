using PlayerControllerScripts;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{

    public PlayerIdleState(PlayerController player, Animator animator) : base(player, animator)
    {
    }
    public override void OnEnter()
    {
    }

    public override void OnUpdate()
    {
        player.Animator.SetFloat(PlayerController.AnimIDSpeed, 0f, 0.1f, Time.deltaTime);
        if (player.InputVector.sqrMagnitude > 0.01f)
        {
            player.ChangeState(player.moveState);
        }
    }
    public override void OnExit()
    {
    }
}
