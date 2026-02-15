using UnityEngine;
using EnemyControllerScripts;
public class EnemyAttackState : EnemyBaseState
{
    private float _attackTimer;
    public EnemyAttackState(EnemyController _enemyController, EnemyAnimation _enemyAnimation) : base(_enemyController, _enemyAnimation)
    {
    }

    public override void OnEnter()
    {
        _attackTimer = 0f;

        enemyController.HandleStop();
        enemyAnimation.UpdateMoveSpeed(0f);
        enemyController.RotateToTargetImmediate();
        enemyAnimation.PlayAttack();
    }
    public override void OnUpdate()
    {
        _attackTimer += Time.deltaTime;

        if(_attackTimer >= enemyController.EnemyManager.EnemyStats.attackCooldown)
        {
            enemyController.ChangeState(enemyController.combatState);
            return;
        }
        float distance = Vector3.Distance(enemyController.transform.position, enemyController.targetTransform.position);
        if(distance <= enemyController.EnemyManager.EnemyStats.attackRange)
        {
            enemyController.RotateToTarget();
        }
    }
    public override void OnExit()
    {
    }
}
