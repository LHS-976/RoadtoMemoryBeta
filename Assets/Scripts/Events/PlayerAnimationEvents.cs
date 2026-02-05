using UnityEngine;
using PlayerControllerScripts;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerController _controller;

    private void Awake()
    {
        _controller = GetComponentInParent<PlayerController>();
    }

    public void AE_DrawWeapon()
    {
        if (_controller != null) _controller.AE_DrawWeapon();
    }

    public void AE_SheathWeapon()
    {
        if (_controller != null) _controller.AE_SheathWeapon();
    }
}