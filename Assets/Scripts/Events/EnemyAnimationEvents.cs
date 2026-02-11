using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    [SerializeField] private EnemyWeapon _enemyWeapon;

    private void Awake()
    {
        if(_enemyWeapon == null) _enemyWeapon = GetComponent<EnemyWeapon>();
    }
    public void AE_EnableHitbox()
    {
        _enemyWeapon.EnableHitbox();
    }

    public void AE_DisableHitbox()
    {
        _enemyWeapon.DisableHitbox();
    }
}
