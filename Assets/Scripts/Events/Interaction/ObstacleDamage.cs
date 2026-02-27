using PlayerControllerScripts;
using UnityEngine;

public class ObstacleDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float _damage = 30f;
    [Tooltip("플레이어를 밀쳐낼 힘")]
    [SerializeField] private float _knockbackForce = 15f;
    [SerializeField] private LayerMask _targetLayer;

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & _targetLayer) != 0)
        {
            IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                Vector3 knockbackDir = transform.forward * _knockbackForce;

                knockbackDir.y = 0;

                damageable.TakeDamage(_damage, knockbackDir);
            }
        }
    }
}
