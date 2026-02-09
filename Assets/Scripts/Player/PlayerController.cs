using System;
using UnityEngine;

namespace PlayerControllerScripts
{
    public class PlayerController : MonoBehaviour
    {
        [field: SerializeField] public PlayerStatSO playerStats { get; private set; }
        [field: SerializeField] public PlayerManager playerManager { get; private set; }

        //public TestPlayerCamera playerCamera;
        [SerializeField] private Transform playerMesh;

        public CharacterController Controller { get; private set; }
        public Animator Animator { get; private set; }
        public Transform MainCameraTransform { get; private set; }
        public PlayerCombatSystem CombatSystem { get; private set; }
        public WeaponHandler WeaponHandler { get; private set; }

        private PlayerBaseState _currentState;
        public PlayerIdleState idleState;
        public PlayerMoveState moveState;
        public PlayerCombatState combatState;

        public Vector2 InputVector { get; private set; }
        public bool IsSprint { get; private set; }
        public bool IsCombatMode;
        public float MoveSpeed;
        public float _lastAttackTime;

        [HideInInspector] public static readonly int AnimIDSpeed = Animator.StringToHash("Speed");
        [HideInInspector] public static readonly int AnimIDInputX = Animator.StringToHash("InputX");
        [HideInInspector] public static readonly int AnimIDInputY = Animator.StringToHash("InputY");
        [HideInInspector] public static readonly int AnimIDCombat = Animator.StringToHash("IsCombat");
        [HideInInspector] public static readonly int AnimIDTriggerDraw = Animator.StringToHash("TriggerDraw");
        [HideInInspector] public static readonly int AnimIDTriggerSheath = Animator.StringToHash("TriggerSheath");
        [HideInInspector] public static readonly int AnimIDAttack = Animator.StringToHash("Attack");
        [HideInInspector] public static readonly int AnimIDComboCount = Animator.StringToHash("ComboCount");
        [HideInInspector] public static readonly int AnimIDHit = Animator.StringToHash("Hit"); //히트 애니메이션 추가.
        [HideInInspector] public static readonly int AnimIDDie = Animator.StringToHash("Die"); //다이 애니메이션 추가.


        private Vector3 _velocity;
        private float _gravity;
        private float _initialJumpVelocity; //점프 구현
        public event Action<bool> OnCombatStateChanged; //변경
        private bool _isSheathing = false;


        private void Awake()
        {
            if(Controller == null) Controller = GetComponent<CharacterController>();
            if(Animator == null) Animator = GetComponentInChildren<Animator>();
            if(CombatSystem == null) CombatSystem = GetComponent<PlayerCombatSystem>();
            if(WeaponHandler == null) WeaponHandler = GetComponent<WeaponHandler>();
            if(playerManager == null) playerManager = GetComponent<PlayerManager>();

            if (Camera.main != null) MainCameraTransform = Camera.main.transform;

            idleState = new PlayerIdleState(this, Animator);
            moveState = new PlayerMoveState(this, Animator);
            combatState = new PlayerCombatState(this, Animator);

            CombatSystem.Initialize(this, Animator);
            if (playerStats != null)
            {
                MoveSpeed = playerStats.WalkSpeed;
                SetupJumpVariables();
            }
            else
            {
                return;
            }
        }

        private void Start()
        {
            ChangeState(idleState);
        }

        private void Update()
        {
            HandleInput();
            _currentState?.OnUpdate();
            ApplyGravity();
        }
        private void HandleInput()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            InputVector = new Vector2(h, v);
            bool sprintInput = Input.GetKey(KeyCode.LeftShift);
            
            if(sprintInput && InputVector.sqrMagnitude > 0 && playerManager.CurrentStamina > 0)
            {
                IsSprint = true;
                playerManager.ConsumeStamina(playerStats.sprintStaminaCost * Time.deltaTime);
            }
            else
            {
                IsSprint = false;
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                if(!IsCombatMode)
                {
                    _lastAttackTime = Time.time;
                }
                ToggleCombatMode();
            }
            if(IsCombatMode)
            {
                if(!_isSheathing && Time.time - _lastAttackTime > 8.0f) //자동 납도기능
                {
                    _lastAttackTime = Time.time;
                    ToggleCombatMode();
                }
            }
        }

        public void ChangeState(PlayerBaseState newState)
        {
            _currentState?.OnExit();
            _currentState = newState;
            _currentState.OnEnter();
        }
        public void ToggleCombatMode()
        {
            if(_isSheathing) return;
            if (IsCombatMode)
            {
                _isSheathing = true;
                Animator.SetTrigger(AnimIDTriggerSheath);
            }
            else
            {
                IsCombatMode = true;
                _lastAttackTime = Time.time;
                Animator.SetBool(AnimIDCombat, true);
                Animator.SetTrigger(AnimIDTriggerDraw);
                ChangeState(combatState);
            }
        }

        private void ApplyGravity()
        {
            if (Controller.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            _velocity.y += _gravity * Time.deltaTime;
            if (IsCombatMode && combatState != null && combatState.UseRootMotion)
            {
                return;
            }
            Controller.Move(_velocity * Time.deltaTime);
        }

        private void SetupJumpVariables()
        {
            float timeToApex = playerStats.MaxJumpTime / 2;
            _gravity = (-2 * playerStats.MaxJumpHeight) / Mathf.Pow(timeToApex, 2);
            _initialJumpVelocity = (2 * playerStats.MaxJumpHeight) / timeToApex;
        }
        public void CheckCombo()
        {
            if (_currentState is PlayerCombatState combatState)
            {
                combatState.OnComboCheck();
            }
        }
        public void OnAnimationEnd()
        {
            if(_currentState is PlayerCombatState combatState)
            {
                combatState.OnAnimationEnd();
            }
        }

        public void OnSheathComplete()
        {
            IsCombatMode = false;
            _isSheathing = false;
            Animator.SetBool(AnimIDCombat, false);
            ChangeState(idleState);
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
            if (targetDirection == Vector3.zero || playerMesh == null) return;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            if (isInstant)
            {
                //combatmode용 회전
                playerMesh.rotation = targetRotation;
            }
            else
            {
                //이동용 회전
                playerMesh.rotation = Quaternion.Slerp(playerMesh.rotation, targetRotation, Time.deltaTime * playerStats.RotateSpeed);
            }
        }
        //적 감지 회전
        public void HandleAttackRotation()
        {
            if (CombatSystem.CurrentTarget == null) return;

            Vector3 dirTotarget = CombatSystem.CurrentTarget.position - transform.position;
            dirTotarget.y = 0;

            if(dirTotarget.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dirTotarget.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15.0f);
            }
        }
        public void HandlePosition(Vector3 targetDirection)
        {
            Controller.Move(targetDirection * MoveSpeed * Time.deltaTime);
        }
        public void OnAnimatorMoveManual()
        {
            if (_currentState is PlayerCombatState combat && combat.UseRootMotion)
            {
                Vector3 velocity = Animator.deltaPosition;

                velocity.y = _velocity.y * Time.deltaTime;
                Controller.Move(velocity);
                transform.rotation *= Animator.deltaRotation;
            }
        }
        /*
        public void OnAnimatorMoveManual()
        {
            if (_currentState is PlayerCombatState combat && combat.UseRootMotion)
            {
                Vector3 velocity = Animator.deltaPosition;

                if(CombatSystem.CurrentTarget != null)
                {
                    float dist = Vector3.Distance(transform.position, CombatSystem.CurrentTarget.position);
                    if(dist > 1.5f && dist < 3.0f)
                    {
                        velocity *= 1.2f;
                    }
                }
                velocity.y = _velocity.y * Time.deltaTime;
                Controller.Move(velocity);
                transform.rotation *= Animator.deltaRotation;
            }
        }
        */
    }
}