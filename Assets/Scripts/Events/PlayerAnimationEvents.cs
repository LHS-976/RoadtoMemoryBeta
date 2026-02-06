using UnityEngine;
using PlayerControllerScripts;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] private PlayerController _controller;
    [SerializeField] private PlayerCombatSystem _combatSystem;
    [SerializeField] private WeaponHandler _weaponHandler;

    private void Awake()
    {
        if(_controller == null) _controller = GetComponentInParent<PlayerController>();
        if (_combatSystem == null) _combatSystem = GetComponentInParent<PlayerCombatSystem>();
        if (_weaponHandler == null) _weaponHandler = GetComponentInParent<WeaponHandler>();
    }

    private void OnAnimatorMoveManual()
    {
        if (_controller != null)
        {
            _controller.OnAnimatorMoveManual();
        }
    }

    public void CheckCombo()
    {
        _controller?.CheckCombo();
    }
    public void OnAnimationEnd()
    {
        _controller?.OnAnimationEnd();
    }

    public void ApplyHitStop()
    {
        _combatSystem?.ApplyHitStop();
    }
    public void AE_DrawWeapon()
    {
        _weaponHandler?.DrawWeapon();
    }

    public void AE_SheathWeapon()
    {
        _weaponHandler?.SheathWeapon();
    }
}