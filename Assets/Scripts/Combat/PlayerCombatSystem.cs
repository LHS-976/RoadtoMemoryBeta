using PlayerControllerScripts;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatSystem : MonoBehaviour
{
    [SerializeField] private PlayerController _controller;
    [SerializeField] private Animator _animator;

    [Header("Strategy Data")]
    public ComboStrategySO currentStrategy;
    public List<ComboStrategySO> availableStyles;

    private int _comboCount;

    public void Initialize(PlayerController controller, Animator animator)
    {
        _controller = controller;
        _animator = animator;
        _comboCount = 0;
    }

    public void ChangeCombatStyle(int index)
    {
        if (index < 0 || index >= availableStyles.Count) return;
        if (availableStyles[index] == null) return;
        if (currentStrategy == availableStyles[index]) return;

        currentStrategy = availableStyles[index];
        ResetCombo();
        Debug.Log($"[CombatSystem] 스타일 변경: {currentStrategy.name}");
    }

    public void ExecuteAttack()
    {
        if (currentStrategy == null) return;

        _comboCount++;

        if (_comboCount > currentStrategy.comboAttacks.Count)
        {
            _comboCount = 1;
        }

        AttackInfo info = currentStrategy.comboAttacks[_comboCount - 1];

        _animator.SetInteger(PlayerController.AnimIDComboCount, info.comboStateIndex);
        _animator.SetTrigger(PlayerController.AnimIDAttack);

    }

    public void ResetCombo()
    {
        _comboCount = 0;
        _animator.SetInteger(PlayerController.AnimIDComboCount, 0);
    }
}