using UnityEngine;

public class EnemyCombatState : EnemyBaseState
{
    public EnemyCombatState(EnemyController _enemyController, Animator _animator) : base(_enemyController,_animator) { }

    public override void OnEnter()
    {
        _enemyController.agent.isStopped = false;
    }
    public override void OnUpdate()
    {
        if(_enemyController.targetTransform == null)
        {
            _enemyController.ChangeState(_enemyController.patrolState);
            return;
        }
        float distance = Vector3.Distance(_enemyController.transform.position, _enemyController.targetTransform.position);

        if(distance <= _enemyController.enemyManager._enemyStats.attackRange)
        {
            _enemyController.ChangeState(_enemyController.attackState);
        }
        else
        {
            _enemyController.agent.SetDestination(_enemyController.targetTransform.position);
            _animator.SetFloat(EnemyController.AnimIDEnemySpeed, _enemyController.agent.velocity.magnitude);
        }
    }
    public override void OnExit()
    {
        _enemyController.agent.isStopped = true;
    }
}
