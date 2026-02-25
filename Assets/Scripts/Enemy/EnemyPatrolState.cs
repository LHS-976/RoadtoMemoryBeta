using UnityEngine;
using UnityEngine.AI;
using EnemyControllerScripts;

public class EnemyPatrolState : EnemyBaseState
{
    private float _waitTimer;

    [Header ("Caching")]
    private float _patrolidleTime;
    private float _attackRange;
    private float _patrolRadius;
    private float _viewRadius;
    public EnemyPatrolState(EnemyController _enemyController, EnemyAnimation _enemyAnimation) : base(_enemyController, _enemyAnimation)
    {
    }
    public override void OnEnter()
    {
        _patrolidleTime = enemyController.EnemyManager.EnemyStats.patrolidleTime;
        _attackRange = enemyController.EnemyManager.EnemyStats.attackRange;
        _patrolRadius = enemyController.EnemyManager.EnemyStats.patrolRadius;
        _viewRadius = enemyController.EnemyManager.EnemyStats.viewRadius;

        enemyController.HandleNavRotationEnable();
        enemyController.Agent.speed = enemyController.EnemyManager.EnemyStats.moveSpeed;

        SetRandomDestination();
    }
    public override void OnUpdate()
    {
        if(CheckForPlayer())
        {
            enemyController.ChangeState(enemyController.combatState);
            return;
        }
        if(!enemyController.Agent.pathPending && enemyController.Agent.remainingDistance <= enemyController.Agent.stoppingDistance + 0.1f)
        {
            enemyAnimation.UpdateMoveSpeed(0f);

            _waitTimer += Time.deltaTime;
            if(_waitTimer >= _patrolidleTime)
            {
                SetRandomDestination();
                _waitTimer = 0f;
            }
        }
        else
        {
            enemyAnimation.UpdateMoveSpeed(enemyController.Agent.velocity.magnitude);
        }
    }
    public override void OnExit()
    {
        _waitTimer = 0f;
        enemyController.Agent.stoppingDistance = _attackRange;
    }

    private void SetRandomDestination()
    {
        // Y축 제거 — 수평 원형 범위에서만 랜덤
        Vector2 randomCircle = Random.insideUnitCircle * _patrolRadius;
        Vector3 randomPoint = enemyController.spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 2f, enemyController.Agent.areaMask))
        {
            // 실제로 Agent가 도달 가능한 경로인지 확인
            NavMeshPath path = new NavMeshPath();
            if (enemyController.Agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                enemyController.Agent.SetDestination(hit.position);
            }
        }
    }
    private bool CheckForPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(enemyController.transform.position, _viewRadius,
            enemyController.EnemyManager.EnemyStats.playerMask);

        if(colliders.Length > 0)
        {
            Transform target = colliders[0].transform;

            if(enemyController.CanSeePlayer(target))
            {
                enemyController.targetTransform = target;
                enemyController.RotateToTargetImmediate();
                return true;
            }
        }
        return false;
    }
}
