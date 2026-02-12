using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] public EnemyManager EnemyManager { get; private set; }
    [SerializeField] public NavMeshAgent Agent { get; private set; }
    [SerializeField] public Animator Animator { get; private set; }

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
        if (Agent == null) Agent = GetComponent<NavMeshAgent>();
        if (Animator == null) Animator = GetComponentInChildren<Animator>();
        if (EnemyManager == null) EnemyManager = GetComponent<EnemyManager>();
        spwnPosition = transform.position;
        Initialize();
    }
    private void Start()
    {
        ChangeState(patrolState);
    }
    private void Update()
    {
        if (EnemyManager.isDead) return;
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
        if(Vector3.Angle(transform.forward, dirToTarget) < EnemyManager.EnemyStats.viewAngle / 2f)
        {
            if(!Physics.Linecast(transform.position + Vector3.up, target.position + Vector3.up, EnemyManager.EnemyStats.obstacleLayer))
            {
                return true;
            }
        }
        return false;
    }
    private void Initialize()
    {
        combatState = new EnemyCombatState(this, Animator);
        patrolState = new EnemyPatrolState(this, Animator);
        attackState = new EnemyAttackState(this, Animator);
        hitState = new EnemyHitState(this, Animator);
        Agent.speed = EnemyManager.EnemyStats.moveSpeed;
        Agent.angularSpeed = EnemyManager.EnemyStats.rotationSpeed;
        Agent.stoppingDistance = EnemyManager.EnemyStats.attackRange;
    }
    public void HandleHit()
    {
        ChangeState(hitState);
    }
    public void HandleInit()
    {
        Agent.isStopped = false;
        Agent.speed = EnemyManager.EnemyStats.moveSpeed;
        Agent.stoppingDistance = 0f;
    }
    public void HandleStop()
    {
        Agent.isStopped = true;
        Agent.velocity = Vector3.zero;
    }
}
