using System.Collections.Generic;
using UnityEngine;

public class WeaponHitBox : MonoBehaviour
{
    private PlayerCombatSystem _playerSystem;
    private float _currentDamage;

    private List<GameObject> _hitList = new List<GameObject>();
    public LayerMask targetLayer;

    private void Awake()
    {
        if (_playerSystem == null) _playerSystem = GetComponentInParent<PlayerCombatSystem>();
    }
    public void SetDamage(float damage)
    {
        _currentDamage = damage;
        _hitList.Clear();
    }

    private void OnEnable()
    {
        _hitList.Clear();
    }
    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            if (_hitList.Contains(other.gameObject)) return;

            _hitList.Add(other.gameObject);

            var enemy = other.GetComponent<IDamageable>();
            if(enemy != null)
            {
                enemy.TakeDamage(_currentDamage);
                if(_playerSystem != null)
                {
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    _playerSystem.OnHit(hitPoint);
                }
            }
        }
    }

}
