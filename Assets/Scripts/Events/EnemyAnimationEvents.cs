using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    [SerializeField] private EnemyController _enemyController;
    private void Awake()
    {
        if(_enemyController == null) _enemyController = GetComponentInParent<EnemyController>();
    }
    public void EnableWeaponTrace()
    {
        if (_enemyController != null) _enemyController.EnableWeaponTrace();
    }

    public void DisableWeaponTrace()
    {
        if (_enemyController != null) _enemyController.DisableWeaponTrace();
    }
}
