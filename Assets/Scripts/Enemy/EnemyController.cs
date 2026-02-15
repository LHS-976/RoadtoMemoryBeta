using UnityEngine;
using UnityEngine.AI;

namespace EnemyControllerScripts
{
    public class EnemyController : MonoBehaviour, IWeaponHitRange
    {
        [SerializeField] public EnemyManager EnemyManager { get; private set; }
        [SerializeField] public NavMeshAgent Agent { get; private set; }
        [SerializeField] public EnemyAnimation EnemyAnim { get; private set; }
        [SerializeField] private WeaponTracer _weaponTracer;
        [SerializeField] private LayerMask _targetLayer; //같은편 때리기 방지

        public Transform targetTransform;
        public Vector3 spawnPosition;

        private EnemyBaseState currentState;
        public EnemyCombatState combatState;
        public EnemyPatrolState patrolState;
        public EnemyAttackState attackState;
        public EnemyHitState hitState;

        [HideInInspector] public Vector3 KnockbackForce;

        [Header("Enemy EyeSight")]
        private const float CLOSE_DETECTION_RANGE = 2.0f;
        private const float EYE_HEIGHT = 1.5f;
        private const float TARGET_CHEST_HEIGHT = 1.2f;
        private const float TARGET_HEAD_HEIGHT = 1.8f;
        private const float BODY_OFFSET = 0.2f;


        private void Awake()
        {
            if (Agent == null) Agent = GetComponent<NavMeshAgent>();
            if (EnemyAnim == null) EnemyAnim = GetComponentInChildren<EnemyAnimation>();
            if (EnemyManager == null) EnemyManager = GetComponent<EnemyManager>();
            if (_weaponTracer != null)
            {
                _weaponTracer.Initialize(this);
            }
            spawnPosition = transform.position;
            InitializeStates();
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
        private void InitializeStates()
        {
            combatState = new EnemyCombatState(this, EnemyAnim);
            patrolState = new EnemyPatrolState(this, EnemyAnim);
            attackState = new EnemyAttackState(this, EnemyAnim);
            hitState = new EnemyHitState(this, EnemyAnim);

            Agent.speed = EnemyManager.EnemyStats.moveSpeed;
            Agent.angularSpeed = EnemyManager.EnemyStats.rotationSpeed;
            Agent.stoppingDistance = EnemyManager.EnemyStats.attackTriggerRange;
        }

        //회전로직 (추격 중 사용)
        public void RotateToTarget()
        {
            if (targetTransform == null) return;

            Vector3 direction = (targetTransform.position - transform.position).normalized;
            direction.y = 0;

            if (direction.sqrMagnitude < 0.001f) return;
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * EnemyManager.EnemyStats.rotationSpeed);
        }
        public void RotateToTargetImmediate()
        {
            if (targetTransform == null) return;

            Vector3 direction = (targetTransform.position - transform.position).normalized;
            direction.y = 0;

            if (direction.sqrMagnitude < 0.001f) return;

            transform.rotation = Quaternion.LookRotation(direction);
        }

        public bool CanSeePlayer(Transform target)
        {
            //적과 플레이어 사이의 거리를 계산
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            //설정된 CLOSE_DETECTION_RANGE보다 가깝다면 시야각이나 장애물에 상관없이 즉시 감지.
            if (distanceToTarget < CLOSE_DETECTION_RANGE)
            {
                return true;
            }

            //시야각 확인
            //적이 바라보는 정면 방향과 플레이어를 향한 방향 사이의 각도재기
            //설정한 각도가 전체시야각 /2보다 작으면 시야 범위 안에 있는 것으로 감지.
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < EnemyManager.EnemyStats.viewAngle / 2f)
            {

                //오프셋 설정 - 플레이어의 몸 안쪽에서 레이가 겹쳐서 충돌 오류가 나는 것을 방지하기 위해, 타겟 지점을 적 쪽으로 살짝 당겨서 계산.
                //첫 번째 시도(가슴) - 적의 위치에서 플레이어의 가슴 높이 지점 사이에 장애물이 있는지 확인.
                //두 번쨰 시도(머리) - 가슴이 가려졌다면 적의 눈(myEyePos)에서 플레이어의 머리 높이 지점 사이에 장애물이 있는지 한 번 더 확인.
                Vector3 myEyePos = transform.position + Vector3.up * EYE_HEIGHT;
                Vector3 targetChestPos = target.position + Vector3.up * TARGET_CHEST_HEIGHT;

                Vector3 dirToMe = (transform.position - target.position).normalized;
                targetChestPos += dirToMe * BODY_OFFSET;

                if (!Physics.Linecast(transform.position + Vector3.up, target.position + Vector3.up, EnemyManager.EnemyStats.obstacleLayer))
                {
                    RotateToTarget();
                    return true; //플레이어를 찾음.
                }
                Vector3 targetHeadPos = target.position + Vector3.up * TARGET_HEAD_HEIGHT;
                targetHeadPos += dirToMe * BODY_OFFSET;

                if (!Physics.Linecast(myEyePos, targetHeadPos, EnemyManager.EnemyStats.obstacleLayer))
                {
                    RotateToTarget();
                    return true;//플레이어를 찾음.
                }
            }
            return false;
        }
        public void HandleStop()
        {
            if (Agent.isOnNavMesh)
            {
                Agent.isStopped = true;
                Agent.velocity = Vector3.zero;
                Agent.ResetPath();
            }
        }
        public void HandleNavRotationEnable()
        {
            Agent.isStopped = false;
            Agent.updatePosition = true;
            Agent.updateUpAxis = true;
            Agent.stoppingDistance = 0f;
        }
        public void HandleNavRotationDisable()
        {
            Agent.isStopped = false;
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;
            Agent.stoppingDistance = EnemyManager.EnemyStats.attackTriggerRange;
        }

        public void OnWeaponHit(IDamageable target, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (target.GetTransform() == transform) return;

            GameObject targetObj = target.GetTransform().gameObject;
            if (((1 << targetObj.layer) & _targetLayer) == 0)
            {
                return;
            }
            Vector3 pushDir = (target.GetTransform().position - transform.position).normalized;
            pushDir.y = 0;

            float finalDamage = EnemyManager.baseDamage * EnemyManager.EnemyStats.damageMultiplier;
            target.TakeDamage(finalDamage, pushDir);

            Debug.Log($"적 공격 적중! 데미지: {finalDamage}");
        }
        public void EnableWeaponTrace() => _weaponTracer.EnableTrace();
        public void DisableWeaponTrace() => _weaponTracer.DisableTrace();

        public void HandleHit()
        {
            DisableWeaponTrace();
            ChangeState(hitState);
        }
        public void HandleDie()
        {
            DisableWeaponTrace();

            if (Agent != null)
            {
                Agent.isStopped = true;
                Agent.velocity = Vector3.zero;
                Agent.enabled = false;
            }
            int deadLayer = LayerMask.NameToLayer("Dead");
            if (deadLayer != -1) gameObject.layer = deadLayer;

            EnemyAnim.PlayDie();
            this.enabled = false;
        }
    }
}
