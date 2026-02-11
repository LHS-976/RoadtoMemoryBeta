using UnityEngine;

public abstract class EnemyBaseState
{
    protected EnemyController _enemyController;
    protected Animator _animator;

    public EnemyBaseState(EnemyController _enemyController, Animator _animator)
    {
        this._enemyController = _enemyController;
        this._animator = _animator;
    }
    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();
}
