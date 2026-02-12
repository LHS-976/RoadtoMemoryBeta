using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : EnemyBaseState
{
    private float _waitTimer;
    public EnemyPatrolState(EnemyController _enemyController, Animator _animator) : base(_enemyController, _animator)
    {
    }

    public override void OnEnter()
    {
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
            if(_waitTimer >= enemyController.EnemyManager.EnemyStats.patrolidleTime)
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
        enemyController.Agent.stoppingDistance = enemyController.EnemyManager.EnemyStats.attackRange;
    }

    private void SetRandomDestination()
    {
        Vector3 randomPoint = enemyController.spwnPosition + Random.insideUnitSphere * enemyController.EnemyManager.EnemyStats.patrolRadius;

        NavMeshHit hit;
        if(NavMesh.SamplePosition(randomPoint, out hit, enemyController.EnemyManager.EnemyStats.patrolRadius, NavMesh.AllAreas))
        {
            enemyController.Agent.SetDestination(hit.position);
        }
    }
    private bool CheckForPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(enemyController.transform.position, enemyController.EnemyManager.EnemyStats.viewRadius,
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
