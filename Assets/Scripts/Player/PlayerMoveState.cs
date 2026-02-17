using PlayerControllerScripts;
using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    public PlayerMoveState(PlayerController player, Animator animator) : base(player, animator)
    {
    }
    public override void OnEnter()
    {
        player.moveSpeed = player.playerStats.WalkSpeed;
    }
    public override void OnUpdate()
    {
        Vector3 dir = player.GetTargetDirection(player.InputVector);
        if (player.InputVector.sqrMagnitude <= 0.01f)
        {
            player.ChangeState(player.idleState);
            return;
        }

        bool isSprinting = player.IsSprint;

        player.moveSpeed = isSprinting ? player.playerStats.RunSpeed : player.playerStats.WalkSpeed;

        float targetAnimSpeed = isSprinting ? 1.0f : 0.5f;
        if (player.InputVector.sqrMagnitude == 0) targetAnimSpeed = 0f;

        player.Animator.SetFloat(PlayerController.AnimIDSpeed, targetAnimSpeed, 0.1f, Time.deltaTime);
        player.HandleRotation(dir, isInstant: false);
        player.HandlePosition(dir);
    }
    public override void OnExit()
    {
    }
}
