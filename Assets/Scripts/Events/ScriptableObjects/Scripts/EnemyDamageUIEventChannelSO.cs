using UnityEngine;

[System.Serializable]
public struct DamageUIPayLoad
{
    public Transform targetEnemy;
    public float damageAmount;
    public float currentHealth;
    public float maxHealth;
    public Vector3 hitPoint;
}
[CreateAssetMenu(fileName = "NewEnemyDamageUIChannel", menuName = "Events/Enemy Damage UI Event Channel")]
public class EnemyDamageUIEventChannelSO : BaseEventChannelSO<DamageUIPayLoad>
{
}
