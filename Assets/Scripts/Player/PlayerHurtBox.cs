using UnityEngine;

public class PlayerHurtBox : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerManager _playerManager;

    public Transform HitTransform => transform;
    private void Awake()
    {
        if (_playerManager == null) _playerManager = GetComponentInParent<PlayerManager>();
    }
    public void TakeDamage(float damage, Vector3 knockBackDir)
    {
        _playerManager.TakeDamage(damage, knockBackDir);
    }
    public Transform GetTransform()
    {
        return transform;
    }
}
