using UnityEngine;
using EnemyControllerScripts;
public class EnemyCombatState : EnemyBaseState
{
    private float _repathTimer;
    private const float REPATH_INTERVAL = 0.25f;
    public EnemyCombatState(EnemyController _enemyController, EnemyAnimation _enemyAnimation) : base(_enemyController, _enemyAnimation) { }

    public override void OnEnter()
    {
        enemyController.HandleNavRotationDisable();

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
            //경로 갱신을 0.25초 간격으로 제한
            _repathTimer += Time.deltaTime;
            if (_repathTimer >= REPATH_INTERVAL)
            {
                _repathTimer = 0f;
                enemyController.Agent.SetDestination(enemyController.targetTransform.position);
            }

            //Agent 경로 방향으로 회전
            Vector3 desiredDir = enemyController.Agent.desiredVelocity;
            desiredDir.y = 0;
            if (desiredDir.sqrMagnitude > 0.01f)
            {
                Quaternion lookRot = Quaternion.LookRotation(desiredDir.normalized);
                enemyController.transform.rotation = Quaternion.Slerp(
                    enemyController.transform.rotation, lookRot,
                    Time.deltaTime * enemyController.EnemyManager.EnemyStats.rotationSpeed
                );
            }
            enemyAnimation.UpdateMoveSpeed(enemyController.Agent.velocity.magnitude);
        }
    }
    public override void OnExit()
    {
        _repathTimer = 0f;
    }
}
