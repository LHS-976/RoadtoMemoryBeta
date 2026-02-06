using System;
using UnityEngine;

namespace PlayerControllerScripts
{
    public class PlayerController : MonoBehaviour
    {
        [field: SerializeField] public PlayerStatSO playerStats { get; private set; }

        [Header("References")]
        public PlayerCamera playerCamera;
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


        private Vector3 _velocity;
        private float _gravity;
        private float _initialJumpVelocity; //점프 구현
        public event Action<bool> OnCombatStateChanged; //변경


        private void Awake()
        {
            Controller = GetComponent<CharacterController>();
            Animator = GetComponentInChildren<Animator>();
            CombatSystem = GetComponent<PlayerCombatSystem>();
            WeaponHandler = GetComponent<WeaponHandler>();

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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
            IsSprint = Input.GetKey(KeyCode.LeftShift);

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
                if(Time.time - _lastAttackTime > 8.0f) //자동 납도기능
                {
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
            IsCombatMode = !IsCombatMode;
            if (IsCombatMode)
            {
                Animator.SetBool(AnimIDCombat, true);
                Animator.SetTrigger(AnimIDTriggerDraw);
                ChangeState(combatState);
            }
            else
            {
                Animator.SetBool(AnimIDCombat, false);
                Animator.SetTrigger(AnimIDTriggerSheath);
                ChangeState(idleState);
            }
        }

        private void ApplyGravity()
        {
            if (Controller.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            _velocity.y += _gravity * Time.deltaTime;
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
                //전투용 회전
                playerMesh.rotation = targetRotation;
            }
            else
            {
                //이동용 회전
                playerMesh.rotation = Quaternion.Slerp(playerMesh.rotation, targetRotation, Time.deltaTime * playerStats.RotateSpeed);
            }
        }
        public void HandlePosition(Vector3 targetDirection)
        {
            Controller.Move(targetDirection * MoveSpeed * Time.deltaTime);
        }
        private void OnAnimatorMove()
        {
            if(IsCombatMode && _currentState == combatState && combatState.UseRootMotion)
            {
                Vector3 velocity = Animator.deltaPosition;

                velocity.y = _velocity.y * Time.deltaTime;
                Controller.Move(velocity);
            }
        }
    }
}