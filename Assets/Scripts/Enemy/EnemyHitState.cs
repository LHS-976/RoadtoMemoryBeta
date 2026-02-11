using UnityEngine;

public class EnemyHitState : EnemyBaseState
{
    private float afterHitTime;
    public EnemyHitState(EnemyController _enemyController, Animator _animator) : base(_enemyController, _animator)
    {
    }

    public override void OnEnter()
    {
        afterHitTime = 0f;

        _enemyController.HandleStop();
        _animator.SetTrigger(EnemyController.AnimIDEnemyHit);

    }
    public override void OnUpdate()
    {
        afterHitTime += Time.deltaTime;

        if(afterHitTime >= _enemyController.enemyManager._enemyStats.hitStunTime)
        {
            _enemyController.ChangeState(_enemyController.combatState);
        }
    }
    public override void OnExit()
    {
        _enemyController.agent.isStopped = false;
    }
}
