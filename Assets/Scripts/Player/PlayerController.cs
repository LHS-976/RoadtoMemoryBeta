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

        public bool IsSprint { get; set; }
        public int AnimIDSpeed { get; private set; }

        private PlayerBaseState currentState;
        public PlayerIdleState idleState;
        public PlayerMoveState moveState;

        private Vector3 velocity;
        [HideInInspector]public float MoveSpeed;

        //private PlayerBattleState battleState;

        [Header("Camera")]
        public PlayerCamera playerCamera;


        private void Awake()
        {
            if (Controller == null) Controller = GetComponent<CharacterController>();
            if (Animator == null) Animator = GetComponent<Animator>();
            if (Camera.main != null) MainCameraTransform = Camera.main.transform;

            idleState = new PlayerIdleState(this, Animator);
            moveState = new PlayerMoveState(this, Animator);
            AnimIDSpeed = Animator.StringToHash("Speed");

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
        void setupJumpVariables()
        {
            float timeToApex = playerStats.MaxJumpTime / 2;
            //"h = 1/2 * g * t^2" (물리 등가속도 공식)을 뒤집은 상태.
            gravity = (-2 * playerStats.MaxJumpHeight) / Mathf.Pow(timeToApex, 2);
            initialJumpVelocity = (2 * playerStats.MaxJumpHeight) / timeToApex;
        }
    }
}
