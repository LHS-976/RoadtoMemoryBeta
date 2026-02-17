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
        Vector3 randomPoint = enemyController.spawnPosition + Random.insideUnitSphere * _patrolRadius;

        NavMeshHit hit;
        //NavMesh.AllAreas - 어떤 속성(Area)의 땅이든 상관없이 위치 잡기
        if(NavMesh.SamplePosition(randomPoint, out hit, _patrolRadius, NavMesh.AllAreas))
        {
            enemyController.Agent.SetDestination(hit.position);
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
