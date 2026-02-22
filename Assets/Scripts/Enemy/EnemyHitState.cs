using UnityEngine;
using EnemyControllerScripts;
public class EnemyHitState : EnemyBaseState
{
    private float _afterHitTime;

    private float _knockbackDuration;
    private float _knockbackForce;

    private const float WallCheckRadius = 0.3f;
    private const float WallCheckOffset = 0.5f;
    public EnemyHitState(EnemyController _enemyController, EnemyAnimation _enemyAnimation) : base(_enemyController, _enemyAnimation)
    {
    }

    public override void OnEnter()
    {
        _afterHitTime = 0f;
        _knockbackDuration = enemyController.EnemyManager.EnemyStats.knockbackDuration;
        _knockbackForce = enemyController.EnemyManager.EnemyStats.knockbackPower;

        enemyController.HandleStop();
        enemyController.DisableWeaponTrace();
        if(enemyAnimation.Animator != null)
        {
            enemyAnimation.Animator.ResetTrigger("Attack");
        }
        enemyAnimation.PlayHit();
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
        Vector3 displacement = moveDir * currentSpeed * Time.deltaTime;

        Vector3 origin = enemyController.transform.position + Vector3.up * WallCheckOffset;

        RaycastHit hit;
        if (Physics.SphereCast(origin, WallCheckRadius, displacement.normalized, out hit, 
            displacement.magnitude + WallCheckRadius, enemyController.EnemyManager.EnemyStats.obstacleLayer ))
        {
            float safeDistance = Mathf.Max(0f, hit.distance - WallCheckRadius);
            displacement = displacement.normalized * safeDistance;
        }
        enemyController.transform.position += displacement;
    }
}
