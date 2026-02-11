using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [SerializeField] public EnemyManager enemyManager { get; private set; }
    [SerializeField] public NavMeshAgent agent { get; private set; }
    [SerializeField] public Animator animator { get; private set; }

    public Transform targetTransform;

    private EnemyBaseState currentState;

    public EnemyCombatState combatState;
    public EnemyPatrolState patrolState;
    public EnemyAttackState attackState;
    public EnemyHitState hitState;

    public Vector3 spwnPosition;

    [HideInInspector] public static readonly int AnimIDEnemySpeed = Animator.StringToHash("Speed");
    [HideInInspector] public static readonly int AnimIDEnemyAttack = Animator.StringToHash("Attack");
    [HideInInspector] public static readonly int AnimIDEnemyDie = Animator.StringToHash("Die");
    [HideInInspector] public static readonly int AnimIDEnemyHit = Animator.StringToHash("Hit");

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (enemyManager == null) enemyManager = GetComponent<EnemyManager>();
        spwnPosition = transform.position;
        Initialize();
    }
    private void Start()
    {
        ChangeState(patrolState);
    }
    private void Update()
    {
        if (enemyManager.isDead) return;
        currentState?.OnUpdate();
    }
    public void ChangeState(EnemyBaseState newState)
    {
        if (currentState == newState) return;

        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }
    public bool CanSeePlayer(Transform target)
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;

        //시야각
        if(Vector3.Angle(transform.forward, dirToTarget) < enemyManager._enemyStats.viewAngle / 2f)
        {
            if(!Physics.Linecast(transform.position + Vector3.up, target.position + Vector3.up, enemyManager._enemyStats.obstacleLayer))
            {
                return true;
            }
        }
        return false;
    }
    private void Initialize()
    {
        combatState = new EnemyCombatState(this, animator);
        patrolState = new EnemyPatrolState(this, animator);
        attackState = new EnemyAttackState(this, animator);
        hitState = new EnemyHitState(this, animator);
        agent.speed = enemyManager._enemyStats.moveSpeed;
        agent.angularSpeed = enemyManager._enemyStats.rotationSpeed;
        agent.stoppingDistance = enemyManager._enemyStats.attackRange;
    }
    public void HandleHit()
    {
        ChangeState(hitState);
    }
    public void HandleInit()
    {
        agent.isStopped = false;
        agent.speed = enemyManager._enemyStats.moveSpeed;
        agent.stoppingDistance = 0f;
    }
    public void HandleStop()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }
}
