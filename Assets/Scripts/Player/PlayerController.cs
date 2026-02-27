using System;
using UnityEngine;

namespace PlayerControllerScripts
{
    public class PlayerController : MonoBehaviour
    {
        [field: SerializeField] public PlayerStatSO playerStats { get; private set; }
        [field: SerializeField] public PlayerManager playerManager { get; private set; }

        [SerializeField] private Transform _playerMesh;
        [SerializeField] private LayerMask _groundLayer;

        public CharacterController Controller { get; private set; }
        public Animator Animator { get; private set; }
        public Transform MainCameraTransform { get; set; }
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


        [Header("Broadcasting Channel")]
        [SerializeField] private VoidEventChannelSO _enableCombatChannel;
        [SerializeField] protected VoidEventChannelSO _deathChannel;
        [SerializeField] private VoidEventChannelSO _playerHitChannel;

        [SerializeField]private bool _canUseCombatMode = false;


        private Vector3 _velocity;
        private Vector3 _pendingMovement;
        private bool _isSheathing = false;

        private const float Grounded_Velocity = -2f;
        private const float _gravity = -9.81f;
        private const float _groundCheckDistance = 0.2f;
        private const float _slopeCheckDistance = 2.0f;

        private void Awake()
        {
            if (Controller == null) Controller = GetComponent<CharacterController>();
            if (Animator == null) Animator = GetComponentInChildren<Animator>();
            if (CombatSystem == null) CombatSystem = GetComponent<PlayerCombatSystem>();
            if (WeaponTracer == null) WeaponTracer = GetComponentInChildren<WeaponTracer>();
            if (playerManager == null) playerManager = GetComponent<PlayerManager>();

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
            CombatSystem.Initialize(this, Animator);
        }
        private void OnEnable()
        {
            if (_enableCombatChannel != null)
                _enableCombatChannel.OnEventRaised += UnlockCombatMode;
        }

        private void OnDisable()
        {
            if (_enableCombatChannel != null)
                _enableCombatChannel.OnEventRaised -= UnlockCombatMode;
        }

        private void Start()
        {
            ChangeState(idleState);
            GameData data = Core.GameCore.Instance?.DataManager?.CurrentData;
            if (data != null && data.IsCombatUnlocked)
            {
                _canUseCombatMode = true;
            }
        }

        private void Update()
        {
            _pendingMovement = Vector3.zero;

            HandleInput();
            CurrentState?.OnUpdate();
            ApplyMovementAndGravity();
        }

        private void HandleInput()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            float staminaRequired = playerStats.sprintStaminaCost * Time.deltaTime;
            InputVector = new Vector2(h, v);
            bool sprintInput = Input.GetKey(KeyCode.LeftShift);

            if (sprintInput && InputVector.sqrMagnitude > 0 && playerManager.CurrentStamina >= staminaRequired)
            {
                IsSprint = true;
                playerManager.ConsumeStamina(staminaRequired);
            }
            else
            {
                IsSprint = false;
            }
            if (Input.GetKeyDown(KeyCode.X)&&_canUseCombatMode)
            {
                ToggleCombatMode();
            }
        }

        public void ChangeState(PlayerBaseState newState)
        {
            if (newState == null)
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
            if (_isSheathing) return;
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

        #region Movement & Gravity
        private void ApplyMovementAndGravity()
        {
            if (IsRootMotionActive()) return;

            bool grounded = CheckStableGround();

            if (grounded)
            {
                if (_velocity.y < 0)
                {
                    _velocity.y = Grounded_Velocity;
                }
            }
            else
            {
                _velocity.y += _gravity * Time.deltaTime;
            }

            Vector3 finalMovement = _pendingMovement;

            if (grounded && finalMovement.sqrMagnitude > 0.001f)
            {
                finalMovement = AdjustMovementToSlope(finalMovement);
            }

            finalMovement.y += _velocity.y * Time.deltaTime;
            Controller.Move(finalMovement);
        }

        private Vector3 AdjustMovementToSlope(Vector3 movement)
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _slopeCheckDistance, _groundLayer))
            {
                float angle = Vector3.Angle(Vector3.up, hit.normal);

                if (angle > 0.5f && angle < Controller.slopeLimit)
                {
                    return Vector3.ProjectOnPlane(movement, hit.normal);
                }
            }

            return movement;
        }

        private bool CheckStableGround()
        {
            if (Controller.isGrounded) return true;

            float checkRadius = Controller.radius * 0.9f;
            Vector3 origin = transform.position + Vector3.up * (Controller.radius + Controller.skinWidth);

            return Physics.SphereCast(
                origin,
                checkRadius,
                Vector3.down,
                out RaycastHit hit,
                _groundCheckDistance,
                _groundLayer
            );
        }

        private bool IsRootMotionActive()
        {
            return CurrentState is PlayerCombatState combat && combat.UseRootMotion;
        }

        #endregion

        #region Public Movement API

        public void HandlePosition(Vector3 targetDirection)
        {
            _pendingMovement = targetDirection * moveSpeed * Time.deltaTime;
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

        #endregion

        #region Animation Callbacks

        public void CheckCombo()
        {
            if (CurrentState is PlayerCombatState combatState)
            {
                combatState.OnComboCheck();
            }
        }

        public void OnAnimationEnd()
        {
            if (CurrentState is PlayerCombatState combatState)
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

        #endregion

        #region Hit & Die

        public void OnHit(Vector3 knockBackDir)
        {
            if (playerManager.IsInvincible) return;

            KnockBackForce = knockBackDir;
            ChangeState(hitState);
            if(_playerHitChannel != null)
            {
                _playerHitChannel.RaiseEvent();
            }
        }

        public void HandleDie()
        {
            if (!enabled) return;

            moveSpeed = 0f;
            _velocity = Vector3.zero;
            InputVector = Vector2.zero;

            if (Controller != null) Controller.enabled = false;
            this.enabled = false;

            if (Animator != null)
            {
                Animator.applyRootMotion = false;
                Animator.SetTrigger(AnimIDDie);
            }
            if (WeaponTracer != null) WeaponTracer.DisableTrace();

            _deathChannel?.RaiseEvent();
            Debug.Log("플레이어 사망");
        }

        #endregion

        #region Direction & Rotation

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

        public void HandleRotation(Vector3 targetDirection, bool isInstant = false)
        {
            if (targetDirection == Vector3.zero || _playerMesh == null) return;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            if (isInstant)
            {
                transform.rotation = targetRotation;
                _playerMesh.rotation = targetRotation;
            }
            else
            {
                _playerMesh.rotation = Quaternion.Slerp(_playerMesh.rotation, targetRotation,
                                                        Time.deltaTime * playerStats.RotateSpeed);
            }
        }

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
        #endregion
        #region Broad Event
        private void UnlockCombatMode()
        {
            _canUseCombatMode = true;
            GameData data = Core.GameCore.Instance?.DataManager?.CurrentData;
            if (data != null) data.IsCombatUnlocked = true;

        }

        #endregion
    }
}