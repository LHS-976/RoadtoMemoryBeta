using UnityEngine;

public class PlayerHurtBox : MonoBehaviour
{
    [SerializeField] private PlayerManager _playerManager;
    private void Awake()
    {
        if (_playerManager == null) _playerManager = GetComponentInParent<PlayerManager>();
    }
    public void OnHit(float damage)
    {
        _playerManager.TakeDamage(damage);
    }
}
