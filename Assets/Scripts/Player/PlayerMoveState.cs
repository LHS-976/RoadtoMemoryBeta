using PlayerControllerScripts;
using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    public Vector3 _targetDirection;
    public PlayerMoveState(PlayerController player, Animator animator) : base(player, animator)
    {
    }
    public override void OnEnter()
    {
        player.MoveSpeed = player.playerStats.WalkSpeed;
    }
    public override void OnUpdate()
    {
        if (player.InputVector.sqrMagnitude <= 0.01f)
        {
            player.ChangeState(player.idleState);
            return;
        }

        bool isSprinting = player.IsSprint;

        player.MoveSpeed = isSprinting ? player.playerStats.RunSpeed : player.playerStats.WalkSpeed;

        float targetAnimSpeed = isSprinting ? 1.0f : 0.5f;
        if (player.InputVector.sqrMagnitude == 0) targetAnimSpeed = 0f;
        player.Animator.SetFloat(player.AnimIDSpeed, targetAnimSpeed, 0.1f, Time.deltaTime);

        player.Animator.SetFloat(player.AnimIDInputX, player.InputVector.x, 0.1f, Time.deltaTime);
        player.Animator.SetFloat(player.AnimIDInputY, player.InputVector.y, 0.1f, Time.deltaTime);

        player.HandleMovement(player.InputVector);
    }
    public override void OnExit()
    {
    }
}
