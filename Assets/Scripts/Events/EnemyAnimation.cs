using UnityEngine;
using EnemyControllerScripts;

public class EnemyAnimation : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Animator _animator;
    [SerializeField] private EnemyController _enemyController;

    private static readonly int AnimIDEnemySpeed = Animator.StringToHash("Speed");
    private static readonly int AnimIDEnemyAttack = Animator.StringToHash("Attack");
    private static readonly int AnimIDEnemyGroggy = Animator.StringToHash("Groggy");
    private static readonly int AnimIDEnemyDie = Animator.StringToHash("Die");
    private static readonly int AnimIDEnemyHit = Animator.StringToHash("Hit");
    private void Awake()
    {
        if (_animator == null) _animator = GetComponent<Animator>();
        if(_enemyController == null) _enemyController = GetComponentInParent<EnemyController>();
    }

    public void UpdateMoveSpeed(float speed)
    {
        _animator.SetFloat(AnimIDEnemySpeed, speed);
    }
    public void PlayAttack()
    {
        _animator.SetTrigger(AnimIDEnemyAttack);
    }
    public void PlayHit()
    {
        _animator.SetTrigger(AnimIDEnemyHit);
    }
    public void PlayDie()
    {
        _animator.SetBool(AnimIDEnemyGroggy, false);
        _animator.SetTrigger(AnimIDEnemyDie);
    }
    public void PlayGroggy()
    {
        _animator.SetBool(AnimIDEnemyGroggy, true);
    }
    public void ClearGroggy()
    {
        _animator.SetBool(AnimIDEnemyGroggy, false);
    }

    //애니메이션 함수
    public void EnableWeaponTrace()
    {
        if (_enemyController != null) _enemyController.EnableWeaponTrace();
    }

    public void DisableWeaponTrace()
    {
        if (_enemyController != null) _enemyController.DisableWeaponTrace();
    }
}
