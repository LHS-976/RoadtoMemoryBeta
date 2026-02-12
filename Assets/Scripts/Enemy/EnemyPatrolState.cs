using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : EnemyBaseState
{
    private float waitTimer;
    public EnemyPatrolState(EnemyController _enemyController, Animator _animator) : base(_enemyController, _animator)
    {
    }

    public override void OnEnter()
    {
        _enemyController.HandleInit();
        SetRandomDestination();
    }
    public override void OnUpdate()
    {
        if(CheckForPlayer())
        {
            _enemyController.ChangeState(_enemyController.combatState);
            return;
        }
        if(!_enemyController.agent.pathPending && _enemyController.agent.remainingDistance <= _enemyController.agent.stoppingDistance + 0.1f)
        {
            _animator.SetFloat(EnemyController.AnimIDEnemySpeed, 0);

            waitTimer += Time.deltaTime;
            if(waitTimer >= _enemyController.enemyManager.enemyStats.patrolidleTime)
            {
                SetRandomDestination();
                waitTimer = 0;
            }
        }
        else
        {
            _animator.SetFloat(EnemyController.AnimIDEnemySpeed, _enemyController.agent.velocity.magnitude);
        }
    }
    public override void OnExit()
    {
        _enemyController.agent.stoppingDistance = _enemyController.enemyManager.enemyStats.attackRange;
    }

    private void SetRandomDestination()
    {
        Vector3 randomPoint = _enemyController.spwnPosition + Random.insideUnitSphere * _enemyController.enemyManager.enemyStats.patrolRadius;

        NavMeshHit hit;
        if(NavMesh.SamplePosition(randomPoint, out hit, _enemyController.enemyManager.enemyStats.patrolRadius, NavMesh.AllAreas))
        {
            _enemyController.agent.SetDestination(hit.position);
        }
    }
    private bool CheckForPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(_enemyController.transform.position, _enemyController.enemyManager.enemyStats.viewRadius,
            _enemyController.enemyManager.enemyStats.playerMask);

        if(colliders.Length > 0)
        {
            Transform target = colliders[0].transform;

            if(_enemyController.CanSeePlayer(target))
            {
                _enemyController.targetTransform = target;
                return true;
            }
        }
        return false;
    }
}
