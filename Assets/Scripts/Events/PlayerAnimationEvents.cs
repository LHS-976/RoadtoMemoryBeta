using UnityEngine;
using PlayerControllerScripts;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] private PlayerController _controller;
    [SerializeField] private PlayerCombatSystem _combatSystem;
    [SerializeField] private WeaponTracer _weaponTracer;


    public float AttackMoveSpeed;

    private void Awake()
    {
        if(_controller == null) _controller = GetComponentInParent<PlayerController>();
        if (_combatSystem == null) _combatSystem = GetComponentInParent<PlayerCombatSystem>();
        if (_weaponTracer == null) _weaponTracer = GetComponent<WeaponTracer>();
    }

    private void OnAnimatorMove()
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

    public void AE_DrawWeapon()
    {
        _weaponTracer?.DrawWeapon();
    }

    public void AE_SheathWeapon()
    {
        _weaponTracer?.SheathWeapon();
    }
    public void AE_SheathComplete()
    {
        if(_controller != null)
        {
            _controller.OnSheathComplete();
        }
    }
    public void AE_PlayFootstep()
    {
        if (_controller != null)
        {
            _controller.PlayFootstepSound();
        }
    }

    public void PlaySwordASwingSound()
    {
        if (_controller != null)
        {
            _controller.PlaySwordASwingSound();
        }
    }

    public void PlaySwordBSwingSound()
    {
        if (_controller != null)
        {
            _controller.PlaySwordBSwingSound();
        }
    }
}