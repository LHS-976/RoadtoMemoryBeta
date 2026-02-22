using UnityEngine;
using EnemyControllerScripts;
public class EnemyCombatState : EnemyBaseState
{

    public EnemyCombatState(EnemyController _enemyController, EnemyAnimation _enemyAnimation) : base(_enemyController, _enemyAnimation) { }

    public override void OnEnter()
    {
        enemyController.HandleNavRotationDisable();
        enemyController.RotateToTargetImmediate();

        float chaseSpeed = enemyController.EnemyManager.EnemyStats.chaseMoveSpeed;
        enemyController.Agent.speed = chaseSpeed;
        if(enemyController.targetTransform != null)
        {
            enemyController.Agent.SetDestination(enemyController.targetTransform.position);
        }
    }
    public override void OnUpdate()
    {
        if(enemyController.targetTransform == null)
        {
            enemyController.ChangeState(enemyController.patrolState);
            return;
        }
        enemyController.RotateToTarget();

        float distance = Vector3.Distance(enemyController.transform.position, enemyController.targetTransform.position);

        if(distance <= enemyController.EnemyManager.EnemyStats.attackTriggerRange)
        {
            enemyController.ChangeState(enemyController.attackState);
        }
        else if (distance > enemyController.EnemyManager.EnemyStats.stopChaseDistance)
        {
            enemyController.targetTransform = null;
            enemyController.Agent.ResetPath();
            enemyController.ChangeState(enemyController.patrolState);
        }
        else
        {
            enemyController.Agent.SetDestination(enemyController.targetTransform.position);
            enemyAnimation.UpdateMoveSpeed(enemyController.Agent.velocity.magnitude);
        }
    }
    public override void OnExit()
    {
    }
}
