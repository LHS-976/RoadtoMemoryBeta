using UnityEngine;

/// <summary>
/// 테스트용 스크립트 combat/battle state일때 이벤트로 바꿀예정
/// </summary>
public class WeaponHandler : MonoBehaviour
{
    [Header("Weapon Models View")]
    public GameObject weaponHand;
    public GameObject weaponBack;

    private void Start()
    {
        SheathWeapon();
    }

    public void DrawWeapon()
    {
        if (weaponBack != null) weaponBack.SetActive(false);
        if (weaponHand != null) weaponHand.SetActive(true);
    }

    public void SheathWeapon()
    {
        if (weaponHand != null) weaponHand.SetActive(false);
        if (weaponBack != null) weaponBack.SetActive(true);
    }
}
