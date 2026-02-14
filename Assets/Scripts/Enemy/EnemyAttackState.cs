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
        if(enemyController.Agent != null)
        {
            enemyController.Agent.velocity = Vector3.zero;
        }
        animator.SetFloat(EnemyController.AnimIDEnemySpeed, 0f);
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
    }
}
