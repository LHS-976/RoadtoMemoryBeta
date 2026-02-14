using UnityEngine;

public class EnemyHitState : EnemyBaseState
{
    private float _afterHitTime;

    private float _knockbackDuration;
    private float _knockbackForce;
    public EnemyHitState(EnemyController _enemyController, Animator _animator) : base(_enemyController, _animator)
    {
    }

    public override void OnEnter()
    {
        _afterHitTime = 0f;
        _knockbackDuration = enemyController.EnemyManager.EnemyStats.knockbackDuration;
        _knockbackForce = enemyController.EnemyManager.EnemyStats.knockbackPower;

        enemyController.HandleStop();
        animator.SetTrigger(EnemyController.AnimIDEnemyHit);

        enemyController.Agent.updatePosition = false;
    }
    public override void OnUpdate()
    {
        _afterHitTime += Time.deltaTime;

        if(_afterHitTime < _knockbackDuration)
        {
            MoveBackwards();
        }
        else
        {
            enemyController.ChangeState(enemyController.combatState);
        }
    }
    public override void OnExit()
    {
        enemyController.Agent.Warp(enemyController.transform.position);

        enemyController.Agent.updatePosition = true;
        enemyController.Agent.isStopped = false;
    }

    private void MoveBackwards()
    {
        float currentSpeed = Mathf.Lerp(_knockbackForce, 0f, _afterHitTime / _knockbackDuration);
        Vector3 moveDir = enemyController.KnockbackForce;

        enemyController.transform.position += moveDir * currentSpeed * Time.deltaTime;
    }
}
