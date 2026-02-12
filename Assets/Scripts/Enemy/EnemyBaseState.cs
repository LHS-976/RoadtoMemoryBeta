using UnityEngine;

public abstract class EnemyBaseState
{
    protected EnemyController enemyController;
    protected Animator animator;

    public EnemyBaseState(EnemyController enemyController, Animator animator)
    {
        this.enemyController = enemyController;
        this.animator = animator;
    }
    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();
}
