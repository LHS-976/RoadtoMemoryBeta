using System;
using UnityEngine;

namespace PlayerControllerScripts
{
    public class PlayerController : MonoBehaviour
    {
        [field: SerializeField] public PlayerStatSO playerStats { get; private set; }
        [field: SerializeField] public PlayerManager playerManager { get; private set; }

        [SerializeField] private Transform _playerMesh;

        public CharacterController Controller { get; private set; }
        public Animator Animator { get; private set; }
        public Transform MainCameraTransform { get; private set; }
        public PlayerCombatSystem CombatSystem { get; private set; }
        public WeaponTracer WeaponTracer { get; private set; }

        public PlayerBaseState CurrentState { get; private set; }
        public PlayerIdleState idleState;
        public PlayerMoveState moveState;
        public PlayerCombatState combatState;
        public PlayerHitState hitState;
        public PlayerExecutionState executionState;

        public Vector2 InputVector { get; private set; }
        public bool IsSprint { get; private set; }
        public bool isCombatMode;
        public float moveSpeed;
        [HideInInspector] public Vector3 KnockBackForce;

        //애니메이션을 Enemy처럼 다른 스크립트로 분리하려고 했지만
        //유니티의 실행순서로 인해 Animator.SetFloat는 호출 즉시 값이 반영X
        //Animator평가 시점(내부적으로 Update후반 또는 LateUpdate 근처)에 처리된다.
        //그래서 같은 프레임에 FreezeMovementAnimation() 호출 후 
        //EnableRootMotion()을 켜면 Animator가 아직 이전 Speed값으로 평가하는 타이밍이 생김
        //이로 인해 Input기반 이동과 루트모션이 한 프레임 동안 동시에 적용되서 분리X
        [HideInInspector] public static readonly int AnimIDSpeed = Animator.StringToHash("Speed");
        [HideInInspector] public static readonly int AnimIDInputX = Animator.StringToHash("InputX");
        [HideInInspector] public static readonly int AnimIDInputY = Animator.StringToHash("InputY");
        [HideInInspector] public static readonly int AnimIDCombat = Animator.StringToHash("IsCombat");
        [HideInInspector] public static readonly int AnimIDTriggerDraw = Animator.StringToHash("TriggerDraw");
        [HideInInspector] public static readonly int AnimIDTriggerSheath = Animator.StringToHash("TriggerSheath");
        [HideInInspector] public static readonly int AnimIDAttack = Animator.StringToHash("Attack");
        [HideInInspector] public static readonly int AnimIDComboCount = Animator.StringToHash("ComboCount");
        [HideInInspector] public static readonly int AnimIDHit = Animator.StringToHash("Hit");
        [HideInInspector] public static readonly int AnimIDDie = Animator.StringToHash("Die");
        [HideInInspector] public static readonly int AnimIDExecution = Animator.StringToHash("Execution");

        private Vector3 _velocity;
        private float _gravity;
        private float _initialJumpVelocity; //점프 구현
        private bool _isSheathing = false;

        private const float Grounded_Velocity = -2f;


        private void Awake()
        {
            if(Controller == null) Controller = GetComponent<CharacterController>();
            if(Animator == null) Animator = GetComponentInChildren<Animator>();
            if(CombatSystem == null) CombatSystem = GetComponent<PlayerCombatSystem>();
            if(WeaponTracer == null) WeaponTracer = GetComponentInChildren<WeaponTracer>();
            if(playerManager == null) playerManager = GetComponent<PlayerManager>();

            if (Camera.main != null) MainCameraTransform = Camera.main.transform;

            if (playerStats == null)
            {
                Debug.LogError("playerStats가 할당되지 않았습니다!!");
                enabled = false;
                return;
            }
            InitializeStates();
            InitializeStats();
        }
        private void InitializeStates()
        {
            idleState = new PlayerIdleState(this, Animator);
            moveState = new PlayerMoveState(this, Animator);
            combatState = new PlayerCombatState(this, Animator);
            hitState = new PlayerHitState(this, Animator);
            executionState = new PlayerExecutionState(this, Animator);
        }
        private void InitializeStats()
        {
            moveSpeed = playerStats.WalkSpeed;
            SetupJumpVariables();
            CombatSystem.Initialize(this, Animator);
        }

        private void Start()
        {
            ChangeState(idleState);
        }

        private void Update()
        {
            HandleInput();
            CurrentState?.OnUpdate();
            ApplyGravity();
        }
        private void HandleInput()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            float staminaRequired = playerStats.sprintStaminaCost * Time.deltaTime;
            InputVector = new Vector2(h, v);
            bool sprintInput = Input.GetKey(KeyCode.LeftShift);
            
            if(sprintInput && InputVector.sqrMagnitude > 0 && playerManager.CurrentStamina >= staminaRequired)
            {
                IsSprint = true;
                playerManager.ConsumeStamina(staminaRequired);
            }
            else
            {
                IsSprint = false;
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                ToggleCombatMode();
            }
        }

        public void ChangeState(PlayerBaseState newState)
        {
            if(newState == null)
            {
                Debug.LogError("null 상태로 전환시도");
                return;
            }

            CurrentState?.OnExit();
            CurrentState = newState;
            CurrentState.OnEnter();
        }
        public void ToggleCombatMode()
        {
            if(_isSheathing) return;
            if (isCombatMode)
            {
                _isSheathing = true;
                Animator.SetTrigger(AnimIDTriggerSheath);
            }
            else
            {
                isCombatMode = true;
                Animator.SetBool(AnimIDCombat, true);
                Animator.SetTrigger(AnimIDTriggerDraw);
                ChangeState(combatState);
            }
        }
        private void ApplyGravity()
        {
            if (Controller.isGrounded && _velocity.y < 0)
            {
                _velocity.y = Grounded_Velocity;
            }
            _velocity.y += _gravity * Time.deltaTime;

            if(!IsRootMotionActive())
            {
                Controller.Move(_velocity * Time.deltaTime);
            }
        }
        private bool IsRootMotionActive()
        {
            return CurrentState is PlayerCombatState combat && combat.UseRootMotion;
        }

        private void SetupJumpVariables()
        {
            float timeToApex = playerStats.MaxJumpTime / 2;
            _gravity = (-2 * playerStats.MaxJumpHeight) / Mathf.Pow(timeToApex, 2);
            _initialJumpVelocity = (2 * playerStats.MaxJumpHeight) / timeToApex;
        }
        public void CheckCombo()
        {
            if (CurrentState is PlayerCombatState combatState)
            {
                combatState.OnComboCheck();
            }
        }
        public void OnAnimationEnd()
        {
            if(CurrentState is PlayerCombatState combatState)
            {
                combatState.OnAnimationEnd();
            }
        }
        public void OnSheathComplete()
        {
            isCombatMode = false;
            _isSheathing = false;
            Animator.SetBool(AnimIDCombat, false);
            ChangeState(idleState);
        }
        public void OnHit(Vector3 knockBackDir)
        {
            if (playerManager.IsInvincible) return;

            KnockBackForce = knockBackDir;
            ChangeState(hitState);
        }
        public void HandleDie()
        {
            if (!enabled) return;

            moveSpeed = 0f;
            _velocity = Vector3.zero;
            InputVector = Vector2.zero;

            if (Controller != null) Controller.enabled = false;
            this.enabled = false;

            if(Animator != null)
            {
                Animator.applyRootMotion = false;
                Animator.SetTrigger(AnimIDDie);
            }
            if (WeaponTracer != null) WeaponTracer.DisableTrace();

            Debug.Log("플레이어 사망");
            //UI 이벤트 호출 추가.
        }

        //카메라 기준 방향으로 변환
        public Vector3 GetTargetDirection(Vector2 input)
        {
            if (MainCameraTransform == null) return Vector3.zero;

            Vector3 camForward = MainCameraTransform.forward;
            Vector3 camRight = MainCameraTransform.right;

            camForward.y = 0;
            camRight.y = 0;

            if (camForward.sqrMagnitude > 0.01f) camForward.Normalize();
            if (camRight.sqrMagnitude > 0.01f) camRight.Normalize();

            return (camForward * input.y) + (camRight * input.x);
        }
        //플레이어 회전
        public void HandleRotation(Vector3 targetDirection, bool isInstant = false)
        {
            if (targetDirection == Vector3.zero || _playerMesh == null) return;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            if (isInstant)
            {
                //combatmode용 회전
                transform.rotation = targetRotation;
                _playerMesh.rotation = targetRotation;
               
            }
            else
            {
                //이동용 회전
                _playerMesh.rotation = Quaternion.Slerp(_playerMesh.rotation, targetRotation, 
                                                        Time.deltaTime * playerStats.RotateSpeed);
            }
        }
        //적 감지 회전, 에임고정을 위한 회전
        public void HandleAttackRotation()
        {
            if (CombatSystem.CurrentTarget == null) return;

            Vector3 dirToTarget = CombatSystem.CurrentTarget.position - transform.position;
            dirToTarget.y = 0;

            if (dirToTarget.sqrMagnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(dirToTarget.normalized);
            }
        }
        public void HandleLockOnRotation()
        {
            if (CombatSystem.CurrentTarget == null) return;

            Vector3 dirToTarget = CombatSystem.CurrentTarget.position - transform.position;
            dirToTarget.y = 0;

            if (dirToTarget.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dirToTarget.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRotation,
                    Time.deltaTime * playerStats.AttackRotationSpeed * playerStats.LockOnRotationSpeed
                );
            }
        }
        public void HandlePosition(Vector3 targetDirection)
        {
            Controller.Move(targetDirection * moveSpeed * Time.deltaTime);
        }
        public void OnAnimatorMoveManual()
        {
            if (IsRootMotionActive())
            {
                Vector3 velocity = Animator.deltaPosition;

                velocity.y = _velocity.y * Time.deltaTime;
                Controller.Move(velocity);
                transform.rotation *= Animator.deltaRotation;
            }
        }
    }
}