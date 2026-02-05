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
        player.Animator.SetFloat(player.AnimIDSpeed, 0.5f);
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
        player.Animator.SetFloat(player.AnimIDSpeed, targetAnimSpeed, 0.1f, Time.deltaTime);

        HandleRotationAndMove();
    }
    public override void OnExit()
    {
    }
    void HandleRotationAndMove()
    {
        Vector3 camForward = player.MainCameraTransform.forward;
        Vector3 camRight = player.MainCameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        _targetDirection = (camForward * player.InputVector.y) + (camRight * player.InputVector.x);

        if (_targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.deltaTime * player.playerStats.RotateSpeed);
        }
        player.Controller.Move(_targetDirection * player.MoveSpeed * Time.deltaTime);
    }
}
