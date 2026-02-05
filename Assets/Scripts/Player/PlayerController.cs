using System;
using UnityEngine;

namespace PlayerControllerScripts
{
    /// <summary>
    /// 유니티 엔진용 CharacterController 컴포넌트 사용
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("PlayerData Source")]
        public PlayerStatSO playerStats;

        [Header("Physics Settings")]
        public float gravity;
        public float initialJumpVelocity;

        public CharacterController Controller { get; private set; }
        public Animator Animator { get; private set; }
        public Vector2 InputVector { get; private set; }
        public Transform MainCameraTransform { get; private set; }


        private PlayerBaseState currentState;
        [Header("States")]
        public PlayerIdleState idleState;
        public PlayerMoveState moveState;
        public PlayerCombatState combatState;
        public bool IsSprint { get; set; }
        public int AnimIDSpeed { get; private set; }
        public bool IsCombatMode { get; private set; } = false;
        [HideInInspector] public int AnimIDCombat = Animator.StringToHash("IsCombat");
        [HideInInspector] public int AnimIDTriggerDraw = Animator.StringToHash("TriggerDraw");
        [HideInInspector] public int AnimIDTriggerSheath = Animator.StringToHash("TriggerSheath");
        [HideInInspector] public int AnimIDInputX { get; set; }
        [HideInInspector] public int AnimIDInputY { get; set; }

        private Vector3 velocity;
        [HideInInspector]public float MoveSpeed;

        [Header("Camera")]
        public PlayerCamera playerCamera;

        public WeaponHandler weaponHandler;
        public event Action<bool> OnCombatStateChanged;

        [Header("Visuals")]
        public Transform playerMesh;


        private void Awake()
        {
            if (Controller == null) Controller = GetComponent<CharacterController>();
            if (Animator == null) Animator = GetComponentInChildren<Animator>();
            if (weaponHandler == null) weaponHandler = GetComponent<WeaponHandler>();
            if (Camera.main != null) MainCameraTransform = Camera.main.transform;

            idleState = new PlayerIdleState(this, Animator);
            moveState = new PlayerMoveState(this, Animator);
            combatState = new PlayerCombatState(this, Animator);
            AnimIDSpeed = Animator.StringToHash("Speed");
            AnimIDInputX = Animator.StringToHash("InputX");
            AnimIDInputY = Animator.StringToHash("InputY");

            if (playerStats != null)
            {
                MoveSpeed = playerStats.WalkSpeed;
                setupJumpVariables();
            }
            else
            {
                return;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            ChangeState(idleState);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        // Update is called once per frame
        void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            InputVector = new Vector2(h, v);
            IsSprint = Input.GetKey(KeyCode.LeftShift);


            //수정예정
            if(Input.GetKeyDown(KeyCode.X))
            {
                SetCombatMode(!IsCombatMode);
                if (currentState == idleState) ChangeState(combatState);
            }

            currentState?.OnUpdate();

            ApplyGravity();
        }
        private void LateUpdate()
        {
            if(playerCamera != null)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                playerCamera.RotateCamera(new Vector2(mouseX, mouseY), true);
            }
        }

        public void ChangeState(PlayerBaseState newState)
        {
            if(currentState != null)
            {
                currentState.OnExit();
            }
            currentState = newState;
            currentState.OnEnter();
        }
        public void SetCombatMode(bool active)
        {
            if (IsCombatMode == active) return;
            IsCombatMode = active;

            if(IsCombatMode)
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

            //OnCombatStateChanged?.Invoke(IsCombatMode);
        }
        private void ApplyGravity()
        {
            if (Controller.isGrounded && velocity.y <0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
            Controller.Move(velocity * Time.deltaTime);
        }

        //중력값계산
        private void setupJumpVariables()
        {
            float timeToApex = playerStats.MaxJumpTime / 2;
            //"h = 1/2 * g * t^2" (물리 등가속도 공식)을 뒤집은 상태.
            gravity = (-2 * playerStats.MaxJumpHeight) / Mathf.Pow(timeToApex, 2);
            initialJumpVelocity = (2 * playerStats.MaxJumpHeight) / timeToApex;
        }

        public void HandleMovement(Vector3 inputVector)
        {
            //카메라의 앞/옆 방향 가져오기
            Vector3 camForward = MainCameraTransform.forward;
            Vector3 camRight = MainCameraTransform.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 targetDirection = (camForward * InputVector.y) + (camRight * InputVector.x);

            if (camForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(camForward);
                if (playerMesh != null)
                {
                    playerMesh.rotation = Quaternion.Slerp(playerMesh.rotation, targetRotation, Time.deltaTime * playerStats.RotateSpeed);
                }
            }

            Controller.Move(targetDirection * MoveSpeed * Time.deltaTime);
        }

        public void AE_DrawWeapon()
        {
            if (weaponHandler != null) weaponHandler.DrawWeapon();
        }
        public void AE_SheathWeapon()
        {
            if (weaponHandler != null) weaponHandler.SheathWeapon();
        }
    }
}
