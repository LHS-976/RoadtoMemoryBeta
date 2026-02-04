using UnityEngine;

namespace PlayerControllerScripts
{
    /// <summary>
    /// 유니티 엔진용 CharacterController 컴포넌트 사용
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Jump Settings")]
        public float maxJumpHeight = 2.0f;
        public float maxJumpTime = 0.5f;

        [Header("Physics Settings")]
        public float gravity;
        public float initialJumpVelocity;
        public float MoveSpeed = 6f;
        public float RotateSpeed = 10f;

        public CharacterController Controller { get; private set; }
        public Animator Animator { get; private set; }
        public Vector2 InputVector { get; private set; }
        public Transform MainCameraTransform { get; private set; }

        private PlayerBaseState currentState;
        public PlayerIdleState idleState;
        public PlayerMoveState moveState;

        private Vector3 velocity;

        //private PlayerBattleState battleState;

        private void Awake()
        {
            if (Controller == null) Controller = GetComponent<CharacterController>();
            if (Animator == null) Animator = GetComponent<Animator>();
            if (Camera.main != null) MainCameraTransform = Camera.main.transform;

            idleState = new PlayerIdleState(this, Animator);
            moveState = new PlayerMoveState(this, Animator);
            setupJumpVariables();
        }

        // Start is called before the first frame update
        void Start()
        {
            ChangeState(idleState);
        }
        // Update is called once per frame
        void Update()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            InputVector = new Vector2(h, v);

            currentState?.OnUpdate();

            ApplyGravity();
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
            float timeToApex = maxJumpTime / 2;
            //"h = 1/2 * g * t^2" (물리 등가속도 공식)을 뒤집은 상태.
            gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
            initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
        }
    }
}
