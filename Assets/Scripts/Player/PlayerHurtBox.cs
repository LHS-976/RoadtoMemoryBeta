using UnityEngine;

public class PlayerHurtBox : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    private void Awake()
    {
        if (playerManager == null) playerManager = GetComponentInParent<PlayerManager>();
    }
    public void OnHit(float damage)
    {
        playerManager.TakeDamage(damage);
    }
}
