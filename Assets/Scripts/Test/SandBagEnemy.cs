using UnityEngine;

//테스트용

public class SandBagEnemy : MonoBehaviour, IDamageable
{
    [SerializeField] PlayerCombatSystem playerSystem;
    public float maxHp = 100;

    private void Start()
    {
        if(playerSystem == null)
        {
            playerSystem = FindObjectOfType<PlayerCombatSystem>();
        }
    }
    public void TakeDamage(float damage)
    {
        maxHp -= damage;
        if(maxHp <= 0)
        {
            maxHp = 0;
            Die();
        }
        Debug.Log($"샌드백이 {damage}의 데미지를 입었습니다!");
    }
    private void Die()
    {
        if(playerSystem != null && playerSystem.CurrentTarget == transform)
        {
            playerSystem.SetTarget(null);
        }
        gameObject.SetActive(false);
    }
}