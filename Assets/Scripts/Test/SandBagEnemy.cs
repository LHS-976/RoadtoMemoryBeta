using UnityEngine;


public class SandBagEnemy : MonoBehaviour, IDamageable
{
    [Header("Broad Channel")]
    [SerializeField] private StringEventChannelSO _questKillChannel;


    [SerializeField] private string _eventID = "Traning_Dummy";
    [SerializeField] private PlayerCombatSystem playerSystem;
    public float maxHp = 40;
    public bool IsDead { get; private set; }

    public void TakeDamage(float damage, Vector3 knockBackDir)
    {
        if (IsDead) return;
        maxHp -= damage;
        if(maxHp <= 0)
        {
            maxHp = 0;
            IsDead = true;
            Die();
        }
        Debug.Log($"샌드백이 {damage}의 데미지를 입었습니다!");
    }
    private void Die()
    {
        if (playerSystem == null)
        {
            playerSystem = FindObjectOfType<PlayerCombatSystem>();
        }
        if (playerSystem != null && playerSystem.CurrentTarget == transform)
        {
            playerSystem.SetTarget(null);
        }
        if(_questKillChannel != null)
        {
            _questKillChannel.RaiseEvent(_eventID);
        }
        gameObject.layer = LayerMask.NameToLayer("Dead");
        Destroy(gameObject);
    }
    public Transform GetTransform()
    {
        return transform;
    }
}