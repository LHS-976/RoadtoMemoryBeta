using UnityEngine;

public class EnemyCombatState : EnemyBaseState
{

    public EnemyCombatState(EnemyController _enemyController, Animator _animator) : base(_enemyController,_animator) { }

    public override void OnEnter()
    {
        if (enemyController.Agent != null)
        {
            enemyController.Agent.updateRotation = false;
            enemyController.Agent.updateUpAxis = false;
        }
        enemyController.Agent.isStopped = false;
    }
    public override void OnUpdate()
    {
        if(enemyController.targetTransform == null)
        {
            enemyController.ChangeState(enemyController.patrolState);
            return;
        }
        float distance = Vector3.Distance(enemyController.transform.position, enemyController.targetTransform.position);

        if(distance <= enemyController.EnemyManager.EnemyStats.attackRange)
        {
            enemyController.ChangeState(enemyController.attackState);
        }
        else
        {
            enemyController.Agent.SetDestination(enemyController.targetTransform.position);
            animator.SetFloat(EnemyController.AnimIDEnemySpeed, enemyController.Agent.velocity.magnitude);
        }
    }
    public override void OnExit()
    {
        enemyController.Agent.isStopped = true;
    }
}
