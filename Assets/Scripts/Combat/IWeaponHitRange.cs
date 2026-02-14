using UnityEngine;

public interface IWeaponHitRange
{
    void OnWeaponHit(IDamageable target, Vector3 hitPoint, Vector3 hitDirection);
}
