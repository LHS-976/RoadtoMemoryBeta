using UnityEngine;

/// <summary>
/// 칼 오브젝트 발도/납도 
/// </summary>
public class WeaponHandler : MonoBehaviour
{
    [Header("Weapon Models View")]
    public GameObject weaponHand;
    public GameObject weaponBack;

    [Header("Effects")]
    public ParticleSystem drawVFX;
    public ParticleSystem sheathVFX;

    private void Start()
    {
        SheathWeapon();
    }

    public void DrawWeapon()
    {
        if (weaponBack != null) weaponBack.SetActive(false);
        if (weaponHand != null) weaponHand.SetActive(true);

        if (drawVFX != null) drawVFX.Play();
    }

    public void SheathWeapon()
    {
        if (weaponHand != null) weaponHand.SetActive(false);
        if (weaponBack != null) weaponBack.SetActive(true);

        if (sheathVFX != null) sheathVFX.Play();
    }
}
