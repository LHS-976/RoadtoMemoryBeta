using PlayerControllerScripts;
using UnityEngine;

public class PlayerCombatState : PlayerBaseState
{

    private CombatCommand _lastCommand;
    private bool _gotInput;
    private float _lastInputTime;
    private float _maxComboDelay = 0.5f;
    private bool _isAttacking;

    private float _evasionCooldown = 0.5f;
    private float _lastEvasionTime = -10f;

    private float _minInputInterval = 0.1f;
    private float _lastClickTIme;

    private float _fixedMaxTarget = 100.0f;
    private float _clearMaxDis = 3.0f;

    public bool UseRootMotion { get; private set; }
    public PlayerCombatState(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        player.CombatSystem.ResetCombo();
        _gotInput = false;
        _isAttacking = false;

        DisableRootMotion();

        player.Animator.SetBool(PlayerController.AnimIDCombat, true);
        player.moveSpeed = player.playerStats.CombatWalkSpeed;

    }
    public override void OnUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(Time.time - _lastEvasionTime < _evasionCooldown)
            {
                return;
            }
            if(player.playerManager.UseStamina(player.playerStats.dashStaminaCost))
            {
                float x = player.InputVector.x;
                float y = player.InputVector.y;
                if (y < -0.1f)
                {
                    ExecuteCommand(CombatCommand.Evasion_Back);
                }
                else if (y > 0.1f)
                {
                    ExecuteCommand(CombatCommand.Evasion_Forward);
                }
                else if (x > 0.1f)
                {
                    ExecuteCommand(CombatCommand.Evasion_Right);
                }
                else if (x < -0.1f)
                {
                    ExecuteCommand(CombatCommand.Evasion_Left);
                }
                else if(player.playerManager.CurrentStamina <= 0)
                {
                    Debug.Log("스태미나 부족, 추후 UI추가");
                }
            }
        }
        if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if(Time.time - _lastClickTIme < _minInputInterval)
            {
                return;
            }
            _lastClickTIme = Time.time;

            bool isRightClick = Input.GetMouseButtonDown(1);
            CombatCommand cmd = GetCommand(isHeavy: isRightClick);

            ExecuteCommand(cmd);
        }
        if(_isAttacking)
        {
            player.moveSpeed = 0f;
            player.Animator.SetFloat(PlayerController.AnimIDSpeed, 0f);
            player.Animator.SetFloat(PlayerController.AnimIDInputX, 0f);
            player.Animator.SetFloat(PlayerController.AnimIDInputY, 0f);

            if(_lastCommand != CombatCommand.Evasion_Back && _lastCommand != CombatCommand.Evasion_Forward && 
                _lastCommand != CombatCommand.Evasion_Right && _lastCommand != CombatCommand.Evasion_Left)
            {
                player.HandleAttackRotation();
            }
        }
        else
        {
            HandleStandardCombatMovement();
        }
    }
    public override void OnExit()
    {
        DisableRootMotion();
        player.Animator.SetBool(PlayerController.AnimIDCombat, false);
    }
    private void EnableRootMotion()
    {
        UseRootMotion = true;
        player.Animator.applyRootMotion = true;
    }
    private void DisableRootMotion()
    {
        UseRootMotion = false;
        player.Animator.applyRootMotion = false;
    }
    private void HandleStandardCombatMovement()
    {
        Vector3 dir = player.GetTargetDirection(player.InputVector);
        Transform target = player.CombatSystem.CurrentTarget;
        if(target != null)
        {
            Vector3 dirToTarget = target.position - player.transform.position;
            dirToTarget.y = 0;
            
            //회전튀는현상 방지용, 다리꼬임 방지
            if(dirToTarget.sqrMagnitude > 0.001f)
            {
                player.HandleAttackRotation();
            }
            bool isSprinting = player.IsSprint;
            if (dir.sqrMagnitude > 0)
            {
                player.moveSpeed = isSprinting ? player.playerStats.CombatRunSpeed : player.playerStats.CombatWalkSpeed;
            }
            else
            {
                player.moveSpeed = 0f;
            }
            player.HandlePosition(dir);

            Vector3 localDir = player.transform.InverseTransformDirection(dir);
            float targetAnimSpeed = 0f;
            if (player.InputVector.sqrMagnitude > 0)
            {
                targetAnimSpeed = isSprinting ? 1.0f : 0.5f;
            }
            player.Animator.SetFloat(PlayerController.AnimIDSpeed, targetAnimSpeed, 0.1f, Time.deltaTime);
            player.Animator.SetFloat(PlayerController.AnimIDInputX, localDir.x, 0.1f, Time.deltaTime);
            player.Animator.SetFloat(PlayerController.AnimIDInputY, localDir.z, 0.1f, Time.deltaTime);
        }
        else
        {
            Vector3 lookDir = player.MainCameraTransform.forward;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f) lookDir.Normalize();

            bool isSprinting = player.IsSprint;
            if (dir.sqrMagnitude > 0)
            {
                player.moveSpeed = isSprinting ? player.playerStats.CombatRunSpeed : player.playerStats.CombatWalkSpeed;
            }
            else
            {
                player.moveSpeed = 0f;
            }

            float targetAnimSpeed = 0f;
            if (player.InputVector.sqrMagnitude > 0)
            {
                targetAnimSpeed = isSprinting ? 1.0f : 0.5f;
            }
            player.Animator.SetFloat(PlayerController.AnimIDSpeed, targetAnimSpeed, 0.1f, Time.deltaTime);
            player.Animator.SetFloat(PlayerController.AnimIDInputX, player.InputVector.x, 0.1f, Time.deltaTime);
            player.Animator.SetFloat(PlayerController.AnimIDInputY, player.InputVector.y, 0.1f, Time.deltaTime);

            if (lookDir != Vector3.zero)
            {
                player.HandleRotation(lookDir, isInstant: false);
            }
            player.HandlePosition(dir);
        }
    }

    private CombatCommand GetCommand(bool isHeavy)
    {
        if(player.InputVector.y > 0.5f)
        {
            return isHeavy ? CombatCommand.Forward_Heavy : CombatCommand.Forward_Light;
        }
        return isHeavy ? CombatCommand.Stand_Heavy : CombatCommand.Stand_Light;

    }
    private void ExecuteCommand(CombatCommand cmd)
    {
        player.UpdateLastAttackTime();//자동납도기능 테스트용
        _gotInput = true;
        _lastCommand = cmd;
        _lastInputTime = Time.time;

        bool isEvasion = (cmd == CombatCommand.Evasion_Back || cmd == CombatCommand.Evasion_Forward ||
                          cmd == CombatCommand.Evasion_Right || cmd == CombatCommand.Evasion_Left);

        if(_isAttacking)
        {
            if(isEvasion)
            {
                _isAttacking = false;
                player.CombatSystem.ResetCombo();
            }
            else
            {
                if(player.CombatSystem.CanCombo)
                {
                    StartAttack();
                }
                return;
            }
        }
        _isAttacking = true;
        _gotInput = false;


        Transform enemy = player.CombatSystem.CurrentTarget;
        Vector3 inputDir = player.GetTargetDirection(player.InputVector);
        if(inputDir.sqrMagnitude > 0.01f && enemy != null)
        {
            Vector3 toEnemy = (enemy.position - player.transform.position);
            float dist = toEnemy.magnitude;
            toEnemy.Normalize();
            float angle = Vector3.Angle(inputDir, toEnemy);

            if(player.InputVector.y >= 0.1f)
            {
                if (angle > _fixedMaxTarget)
                {
                    enemy = null;
                }
            }
            else
            {
                if (dist > _clearMaxDis) enemy = null;
            }
        }
        if(enemy == null || !enemy.gameObject.activeSelf) //적이 죽었을때 방지추가
        {
            Vector3 searchDir;
            if (player.InputVector.y < -0.1f || player.InputVector.sqrMagnitude < 0.01f)
            {
                searchDir = player.transform.forward;
            }
            else
            {
                searchDir = player.GetTargetDirection(player.InputVector);
            }
            enemy = player.CombatSystem.GetNearestEnemy(player.transform.position, searchDir);
        }
        player.CombatSystem.SetTarget(enemy);


        Vector3 attackDir = Vector3.zero;

        if(enemy != null)
        {
            Vector3 toEnemy = enemy.position - player.transform.position;
            toEnemy.y = 0;
            attackDir = toEnemy.normalized;
        }
        else
        {
            if(player.InputVector.y < -0.1f)
            {
                attackDir = player.transform.forward;
            }
            else
            {
                attackDir = player.GetTargetDirection(player.InputVector);

                if(attackDir == Vector3.zero)
                {
                    //수정예정.
                    Vector3 camForward = player.MainCameraTransform.forward;
                    camForward.y = 0;
                    attackDir = camForward.normalized;
                }
            }
        }
        if(isEvasion)
        {
            player.playerManager.SetInvincible(true);
            //애니메이션 이벤트로 끄는 기능 추가예정
        }
        if(!isEvasion && attackDir != Vector3.zero)
        {
            player.HandleRotation(attackDir, isInstant: true);
        }
        else if(isEvasion && enemy != null)
        {
            player.HandleRotation(attackDir, isInstant: true);
        }
        EnableRootMotion();
        player.CombatSystem.ExecuteAttack(cmd);
    }
    private void StartAttack()
    {
        EnableRootMotion();
        _isAttacking = true;
        _gotInput = false;
        player.CombatSystem.ResetComboWindow();
        player.CombatSystem.ExecuteAttack(_lastCommand);
    }

    public void OnComboCheck()
    {
        player.CombatSystem.SetComboWindow(1);

        if (_gotInput && (Time.time - _lastInputTime < _maxComboDelay))
        {
            StartAttack();
        }
        else
        {
            _gotInput = false;
            DisableRootMotion();
        }
    }
    public void OnAnimationEnd()
    {
        _isAttacking = false;
        player.playerManager.SetInvincible(false);
        DisableRootMotion();

        player.CombatSystem.ResetCombo();
    }
}
