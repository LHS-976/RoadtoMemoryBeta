using UnityEngine;

/// <summary>
/// 테스트용 스크립트 combat/battle state일때 이벤트로 바꿀예정
/// </summary>
public class WeaponSwap : MonoBehaviour
{
    [Header("Weapon Models View")]
    public GameObject weaponHand;
    public GameObject weaponBack;

    private void Start()
    {
        SetCombatMode(false);
    }

    public void SetCombatMode(bool isCombat)
    {
        if(isCombat)
        {
            weaponBack.SetActive(false);
            weaponBack.SetActive(true);
        }
        else
        {
            weaponHand.SetActive(false);
            weaponHand.SetActive(true);
        }
    }
}
