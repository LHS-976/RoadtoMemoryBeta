using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : EnemyBaseState
{
    private float _waitTimer;

    [Header ("Caching")]
    private float _patrolidleTime;
    private float _attackRange;
    private float _patrolRadius;
    private float _viewRadius;
    public EnemyPatrolState(EnemyController _enemyController, Animator _animator) : base(_enemyController, _animator)
    {
    }
    public override void OnEnter()
    {
        _patrolidleTime = enemyController.EnemyManager.EnemyStats.patrolidleTime;
        _attackRange = enemyController.EnemyManager.EnemyStats.attackRange;
        _patrolRadius = enemyController.EnemyManager.EnemyStats.patrolRadius;
        _viewRadius = enemyController.EnemyManager.EnemyStats.viewRadius;
        enemyController.HandleInit();
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
            animator.SetFloat(EnemyController.AnimIDEnemySpeed, 0);

            _waitTimer += Time.deltaTime;
            if(_waitTimer >= _patrolidleTime)
            {
                SetRandomDestination();
                _waitTimer = 0;
            }
        }
        else
        {
            animator.SetFloat(EnemyController.AnimIDEnemySpeed, enemyController.Agent.velocity.magnitude);
        }
    }
    public override void OnExit()
    {
        enemyController.Agent.stoppingDistance = _attackRange;
    }

    private void SetRandomDestination()
    {
        Vector3 randomPoint = enemyController.spwnPosition + Random.insideUnitSphere * _patrolRadius;

        NavMeshHit hit;
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
                return true;
            }
        }
        return false;
    }
}
