using UnityEngine;

/// <summary>
/// 삭제예정
/// </summary>
public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] private float _damage = 10f; //EnemySO로 바꿀예정
    [SerializeField] private Collider _weaponCollider;

    [SerializeField] private LayerMask _targetLayer;

    private void Awake()
    {
        if (_weaponCollider == null) _weaponCollider = GetComponent<Collider>();

        //_weaponCollider.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if((_targetLayer.value & (1<< other.gameObject.layer)) !=0)
        {
            PlayerHurtBox hurtBox = other.GetComponentInParent<PlayerHurtBox>();

            if (hurtBox != null)
            {
                hurtBox.OnHit(_damage);
                //피격이벤트(이펙트) 추가하기.
            }
        }
    }
    public void EnableHitbox()
    {
        _weaponCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        _weaponCollider.enabled = false;
    }
}
