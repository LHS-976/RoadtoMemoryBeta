using UnityEngine;

public class EnemyHitState : EnemyBaseState
{
    private float _afterHitTime;
    public EnemyHitState(EnemyController _enemyController, Animator _animator) : base(_enemyController, _animator)
    {
    }

    public override void OnEnter()
    {
        _afterHitTime = 0f;

        enemyController.HandleStop();
        animator.SetTrigger(EnemyController.AnimIDEnemyHit);

    }
    public override void OnUpdate()
    {
        _afterHitTime += Time.deltaTime;

        if(_afterHitTime >= enemyController.EnemyManager.EnemyStats.hitStunTime)
        {
            enemyController.ChangeState(enemyController.combatState);
        }
    }
    public override void OnExit()
    {
        enemyController.Agent.isStopped = false;
    }
}
