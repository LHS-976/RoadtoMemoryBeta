using PlayerControllerScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CombatCommand
{
    Stand_Light,
    Forward_Light,
    Stand_Heavy,
    Forward_Heavy,

    Evasion_Back,
    Evasion_Forward,
    Evasion_Right,
    Evasion_Left
}

public class PlayerCombatSystem : MonoBehaviour
{
    [SerializeField] private PlayerController _controller;
    [SerializeField] private Animator _animator;

    [Header("Strategy Data")]
    public ComboStrategySO currentStrategy;
    public List<ComboStrategySO> availableStyles;

    private int _comboCount;
    private int _currentActionIndex = -1;

    public bool CanCombo { get; private set; }

    [SerializeField] private float _detectRadius = 5.0f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float _autoAimSpeed = 15.0f;

    private Collider[] _enemyBuffer = new Collider[20];

    public void Initialize(PlayerController controller, Animator animator)
    {
        _controller = controller;
        _animator = animator;
        _comboCount = 0;
    }

    private void PlayAttack(int index)
    {
        if (index >= currentStrategy.actions.Count) return;

        _currentActionIndex = index;
        AttackAction action = currentStrategy.actions[index];

        _animator.SetTrigger(action.attackName);
        _animator.SetInteger(PlayerController.AnimIDComboCount, index);
    }

    public void ExecuteAttack(CombatCommand commandType)
    {
        if (currentStrategy == null) return;
        if (_currentActionIndex == -1)
        {
            if(commandType == CombatCommand.Evasion_Back)
            {
                PlayAttack(5);
                return;
            }
            /*
            else if(commandType == CombatCommand.Evasion_Forward)
            {
                PlayAttack(6);
                return;
            }
            */
        }
        AttackAction currentAction = currentStrategy.actions[_currentActionIndex];
        ComboConnection connection = null;

        for(int i=0; i< currentAction.nextCombos.Count; i++)
        {
            if (currentAction.nextCombos[i].commandType == commandType)
            {
                connection = currentAction.nextCombos[i];
                break;
            }
        }
        if (connection != null)
        {
            PlayAttack(connection.nextComboIndex);
        }
    }
    public Transform GetNearestEnemy(Vector3 playerPos, Vector3 inputDir)
    {
        int count = Physics.OverlapSphereNonAlloc(playerPos, _detectRadius, _enemyBuffer,enemyLayer);
        Transform nearestTarget = null;
        float minDistance = float.MaxValue;

        for(int i=0; i< count; i++)
        {
            Collider collider = _enemyBuffer[i];
            Vector3 toEnemy = collider.transform.position - playerPos;

            Vector3 checkDir = inputDir == Vector3.zero ? transform.forward : inputDir;
            float angle = Vector3.Angle(checkDir, toEnemy);

            if(angle < 60f)
            {
                float dist = toEnemy.sqrMagnitude;
                if(dist < minDistance)
                {
                    minDistance = dist;
                    nearestTarget = collider.transform;
                }
            }
        }
        return nearestTarget;
    }
    public void ApplyHitStop(float duration = 0.1f)
    {
        StartCoroutine(HitStopRoutine(duration));
    }
    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1.0f;
    }

    public void SetComboWindow(int state)
    {
        CanCombo = (state == 1);
    }
    public void ResetComboWindow()
    {
        CanCombo = false;
    }

    public void ResetCombo()
    {
        _currentActionIndex = -1;
        _animator.SetInteger(PlayerController.AnimIDComboCount, 0);
    }
}