using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ComboConnection
{
    public CombatCommand commandType;
    public int nextComboIndex;
}
[System.Serializable]
public class AttackAction
{
    public string attackName;
    public float damageMultiplier;
    public List<ComboConnection> nextCombos;

    //추가사항
    public GameObject hitVFX;
    public float hitStopDuration;
    public float cameraShakePower;
}

[CreateAssetMenu(fileName = "NewComboStrategy", menuName = "Combat Strategy")]
public class ComboStrategySO : ScriptableObject
{
    [Header("Settings")]
    public string comboName;
    public float baseDamage = 10f;
    public float attackSpeed = 1.0f;

    [Header("Combo Chain")]
    public List<AttackAction> actions;
}
