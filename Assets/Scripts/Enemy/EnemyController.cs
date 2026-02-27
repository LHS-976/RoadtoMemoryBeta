using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyControllerScripts
{
    public class EnemyController : MonoBehaviour, IWeaponHitRange
    {
        [field: SerializeField] public EnemyManager EnemyManager { get; private set; }
        [field: SerializeField] public NavMeshAgent Agent { get; private set; }
        [field: SerializeField] public EnemyAnimation EnemyAnim { get; private set; }
        [SerializeField] private WeaponTracer _weaponTracer;
        [SerializeField] private LayerMask _targetLayer;

        public Transform targetTransform;
        public Vector3 spawnPosition;

        private EnemyBaseState currentState;
        public EnemyCombatState combatState;
        public EnemyPatrolState patrolState;
        public EnemyAttackState attackState;
        public EnemyHitState hitState;
        public EnemyGroggyState groggyState;

        [HideInInspector] public Vector3 KnockbackForce;


        [Header("Broad Channel")]
        [SerializeField] private StringEventChannelSO _questKillChannel;

        [Header("Caching Enemy EyeSight")]
        private float CLOSE_DETECTION_RANGE;
        private float EYE_HEIGHT;
        private float TARGET_CHEST_HEIGHT;
        private float TARGET_HEAD_HEIGHT;
        private float BODY_OFFSET;



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
            CLOSE_DETECTION_RANGE = EnemyManager.EnemyStats.closeDetectionRange;
            EYE_HEIGHT = EnemyManager.EnemyStats.eyeHeight;
            TARGET_CHEST_HEIGHT = EnemyManager.EnemyStats.targetChestHeight;
            TARGET_HEAD_HEIGHT = EnemyManager.EnemyStats.targetHeadHeight;
            BODY_OFFSET = EnemyManager.EnemyStats.bodyOffset;
            InitializeStates();
        }

        private void Start()
        {
            GameData data = Core.GameCore.Instance?.DataManager?.CurrentData;
            if (data != null && !string.IsNullOrEmpty(EnemyManager.EnemyUID)
                && data.DefeatedEnemyIDs.Contains(EnemyManager.EnemyUID))
            {
                gameObject.SetActive(false);
                return;
            }
            StartCoroutine(WaitForPlayerSpawnRoutine());
            ChangeState(patrolState);
        }

        private IEnumerator WaitForPlayerSpawnRoutine()
        {
            while (targetTransform == null)
            {

                if (Core.GameCore.Instance?.CurrentPlayer != null)
                {
                    targetTransform = Core.GameCore.Instance.CurrentPlayer.transform;
                    break;
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        private void Update()
        {
            if (EnemyManager.isDead) return;

            //NavMesh 이탈 시 강제 복귀
            if(Agent.enabled && !Agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 3f, Agent.areaMask))
                {
                    Agent.Warp(hit.position);
                }
            }

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
            groggyState = new EnemyGroggyState(this, EnemyAnim);

            Agent.speed = EnemyManager.EnemyStats.moveSpeed;
            Agent.angularSpeed = EnemyManager.EnemyStats.rotationSpeed;
            Agent.stoppingDistance = EnemyManager.EnemyStats.attackTriggerRange;

            //회피 설정 보장
            Agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            Agent.avoidancePriority = 50;
        }

        #region Rotation

        private bool TryGetDirectionToTarget(out Vector3 direction)
        {
            direction = Vector3.zero;
            if (targetTransform == null) return false;

            direction = targetTransform.position - transform.position;
            direction.y = 0;

            return direction.sqrMagnitude > 0.001f;
        }
        public void RotateToTarget()
        {
            if (!TryGetDirectionToTarget(out Vector3 dir)) return;

            Quaternion lookRot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, lookRot,
                Time.deltaTime * EnemyManager.EnemyStats.rotationSpeed
            );
        }

        public void RotateToTargetImmediate()
        {
            if (!TryGetDirectionToTarget(out Vector3 dir)) return;

            transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        #endregion

        #region Detection

        public bool CanSeePlayer(Transform target)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget < CLOSE_DETECTION_RANGE)
            {
                return true;
            }
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) >= EnemyManager.EnemyStats.viewAngle / 2f)
            {
                return false;
            }
            Vector3 dirToMe = (transform.position - target.position).normalized;

            //첫 번째 시도: 가슴 높이
            Vector3 myEyePos = transform.position + Vector3.up * EYE_HEIGHT;
            Vector3 targetChestPos = target.position + Vector3.up * TARGET_CHEST_HEIGHT;
            targetChestPos += dirToMe * BODY_OFFSET;

            if (!Physics.Linecast(myEyePos, targetChestPos, EnemyManager.EnemyStats.obstacleLayer))
            {
                RotateToTarget();
                return true;
            }

            //두 번째 시도: 머리 높이
            Vector3 targetHeadPos = target.position + Vector3.up * TARGET_HEAD_HEIGHT;
            targetHeadPos += dirToMe * BODY_OFFSET;

            if (!Physics.Linecast(myEyePos, targetHeadPos, EnemyManager.EnemyStats.obstacleLayer))
            {
                RotateToTarget();
                return true;
            }

            return false;
        }

        #endregion

        #region NavMesh Control

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
            Agent.updateRotation = true;
            Agent.updateUpAxis = true;
            Agent.stoppingDistance = 0f;
        }

        public void HandleNavRotationDisable()
        {
            Agent.isStopped = false;
            Agent.updateRotation = false;
            Agent.stoppingDistance = EnemyManager.EnemyStats.attackTriggerRange;
        }

        #endregion

        #region Combat

        public void OnWeaponHit(IDamageable target, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (target.GetTransform() == transform) return;

            GameObject targetObj = target.GetTransform().gameObject;
            if (((1 << targetObj.layer) & _targetLayer) == 0) return;

            Vector3 pushDir = (target.GetTransform().position - transform.position).normalized;
            pushDir.y = 0;

            float finalDamage = EnemyManager.EnemyStats.baseDamage * EnemyManager.EnemyStats.damageMultiplier;
            target.TakeDamage(finalDamage, pushDir);

            Debug.Log($"적 공격 적중! 데미지: {finalDamage}");
        }

        public void EnableWeaponTrace() => _weaponTracer.EnableTrace();
        public void DisableWeaponTrace() => _weaponTracer.DisableTrace();

        #endregion

        #region State Triggers

        public void HandleGroggy()
        {
            DisableWeaponTrace();
            ChangeState(groggyState);
        }

        public void HandleHit()
        {
            DisableWeaponTrace();
            ChangeState(hitState);
        }

        public void HandleDie()
        {
            DisableWeaponTrace();
            if(_questKillChannel != null)
            {
                _questKillChannel.RaiseEvent(EnemyManager.EnemyStats.enemyID);
            }

            if (Agent != null)
            {
                Agent.isStopped = true;
                Agent.velocity = Vector3.zero;
                Agent.enabled = false;
            }

            int deadLayer = LayerMask.NameToLayer("Dead");
            if (deadLayer != -1) gameObject.layer = deadLayer;

            if(EnemyManager.EnemyStats.enemyID == "Zombie")
            {
                EnemyAnim.OnlyPlayDie();
            }
            else
            {
                EnemyAnim.PlayDie();
            }
            this.enabled = false;
        }

        #endregion
    }
}