using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    private float attackTimer;
    public EnemyAttackState(EnemyController _enemyController, Animator _animator) : base(_enemyController, _animator)
    {
    }

    public override void OnEnter()
    {
        attackTimer = 0f;

        _enemyController.HandleStop();
        LookAtTarget();

        _animator.SetTrigger(EnemyController.AnimIDEnemyAttack);
    }
    public override void OnUpdate()
    {
        attackTimer += Time.deltaTime;

        if(attackTimer >= _enemyController.enemyManager._enemyStats.attackCooldown)
        {
            _enemyController.ChangeState(_enemyController.combatState);
        }
    }
    public override void OnExit()
    {
        _enemyController.agent.isStopped = false;
    }

    private void LookAtTarget()
    {
        if (_enemyController.targetTransform == null) return;
        Vector3 lookDir = (_enemyController.targetTransform.position - _enemyController.transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero) _enemyController.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
