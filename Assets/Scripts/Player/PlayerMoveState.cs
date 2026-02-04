using PlayerControllerScripts;
using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    public PlayerMoveState(PlayerController player, Animator animator) : base(player, animator)
    {
    }
    public override void OnEnter()
    {
        player.Animator.SetFloat("Speed", 1);
    }
    public override void OnUpdate()
    {
        handleRotation();
        if(player.InputVector.sqrMagnitude == 0)
        {
            player.ChangeState(player.idleState);
        }
    }
    public override void OnExit()
    {
    }
    void handleRotation()
    {
        Vector3 characterMovementInput = new Vector3(player.InputVector.x, 0, player.InputVector.y);
        Vector3 currentPosToLook = player.MainCameraTransform.forward * characterMovementInput.z;

        if (player.InputVector.sqrMagnitude > 0)
        {
            Vector3 camRight = player.MainCameraTransform.right;
            currentPosToLook += characterMovementInput.x * camRight;
            currentPosToLook.y = 0;
            currentPosToLook.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(currentPosToLook);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.deltaTime * 15.0f);
        }
        player.Controller.Move(currentPosToLook * player.MoveSpeed * Time.deltaTime);
    }
}
