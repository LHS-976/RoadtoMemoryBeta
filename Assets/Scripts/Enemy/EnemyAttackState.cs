using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    private float _attackTimer;
    public EnemyAttackState(EnemyController _enemyController, Animator _animator) : base(_enemyController, _animator)
    {
    }

    public override void OnEnter()
    {
        _attackTimer = 0f;

        enemyController.HandleStop();
        LookAtTarget();

        animator.SetTrigger(EnemyController.AnimIDEnemyAttack);
    }
    public override void OnUpdate()
    {
        _attackTimer += Time.deltaTime;

        if(_attackTimer >= enemyController.EnemyManager.EnemyStats.attackCooldown)
        {
            enemyController.ChangeState(enemyController.combatState);
        }
    }
    public override void OnExit()
    {
        enemyController.Agent.isStopped = false;
    }

    private void LookAtTarget()
    {
        if (enemyController.targetTransform == null) return;
        Vector3 lookDir = (enemyController.targetTransform.position - enemyController.transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero) enemyController.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
