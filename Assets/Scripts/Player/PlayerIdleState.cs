using PlayerControllerScripts;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerController player, Animator animator) : base(player, animator)
    {
    }
    public override void OnEnter()
    {
        animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
    }

    public override void OnUpdate()
    {
        if(player.InputVector.magnitude > 0.1f)
        {
            player.ChangeState(player.moveState);
        }
    }
    public override void OnExit()
    {
    }
}
