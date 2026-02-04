using PlayerControllerScripts;
using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerController player;
    protected Animator animator;

    public PlayerBaseState(PlayerController player, Animator animator)
    {
        this.player = player;
        this.animator = animator;
    }
    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();
}
