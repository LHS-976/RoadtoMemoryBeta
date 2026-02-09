using UnityEngine;

//테스트용

public class SandBagEnemy : MonoBehaviour
{
    public LayerMask _targetLayer;
    public float maxHp = 100;

    public void TakeDamage(float damage)
    {
        maxHp -= damage;
        Debug.Log($"샌드백이 {damage}의 데미지를 입었습니다!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((_targetLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            PlayerCombatSystem attacker = other.GetComponentInParent<PlayerCombatSystem>();

            if(attacker != null)
            {
                TakeDamage(attacker.currentStrategy.baseDamage);
            }
        }
    }
}