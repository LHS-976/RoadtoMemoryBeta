using System.Collections.Generic;
using UnityEngine;


public class AttackInfo
{
    public string attackName;
    public int comboStateIndex;
    public float damageMultiplier;
    public float damage;
}

[CreateAssetMenu(fileName = "NewComboStrategy", menuName = "Combat Strategy")]
public class ComboStrategySO : ScriptableObject
{
    [Header("Settings")]
    public string comboName;
    public float baseDamage = 10f;
    public float attackSpeed = 1.0f;

    [Header("Combo Chain")]
    public List<AttackInfo> comboAttacks;
}
