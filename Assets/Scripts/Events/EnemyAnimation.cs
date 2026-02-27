using UnityEngine;
using EnemyControllerScripts;

public class EnemyAnimation : MonoBehaviour
{
    [field: SerializeField] public Animator Animator { get; private set; }
    [Header("Reference")]
    [SerializeField] private EnemyController _enemyController;


    private static readonly int AnimIDEnemySpeed = Animator.StringToHash("Speed");
    public static int AnimIDEnemyAttack = Animator.StringToHash("Attack");
    private static readonly int AnimIDEnemyGroggy = Animator.StringToHash("Groggy");
    private static readonly int AnimIDEnemyDie = Animator.StringToHash("Die");
    private static readonly int AnimIDEnemyHit = Animator.StringToHash("Hit");
    private void Awake()
    {
        if (Animator == null) Animator = GetComponent<Animator>();
        if(_enemyController == null) _enemyController = GetComponentInParent<EnemyController>();
    }

    public void UpdateMoveSpeed(float speed)
    {
        Animator.SetFloat(AnimIDEnemySpeed, speed);
    }
    public void PlayAttack()
    {
        Animator.SetTrigger(AnimIDEnemyAttack);
    }
    public void PlayHit()
    {
        Animator.SetTrigger(AnimIDEnemyHit);
    }
    public void PlayDie()
    {
        Animator.SetBool(AnimIDEnemyGroggy, false);
        Animator.SetTrigger(AnimIDEnemyDie);
    }
    public void OnlyPlayDie()
    {
        Animator.SetTrigger(AnimIDEnemyDie);
    }
    public void PlayGroggy()
    {
        Animator.SetBool(AnimIDEnemyGroggy, true);
    }
    public void ClearGroggy()
    {
        Animator.SetBool(AnimIDEnemyGroggy, false);
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
