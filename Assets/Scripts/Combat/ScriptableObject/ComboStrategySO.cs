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
    public float staminaCost = 0f;
    public AudioClip hitSound;
    [Range(0f, 1f)] public float hitVolume = 0.5f;
    public AudioClip swingSound;
    [Range(0f, 1f)] public float swingVolume = 1.0f;

    [Header("Unlock Settings")]
    public bool requiresUnlock = false;

    [Header("정확한 프레임 계산: 공격 판정이 켜지는 시점 / 120frame")]
    public float startFrameHit;
    public float endFrameHit;

    public List<ComboConnection> nextCombos;
    [Header ("VFX")]
    public GameObject hitVFX;
    public GameObject slashVFX;
    public Vector3 slashRotationOffset;
    public float slashForwardOffset = 0.5f;

    public float hitStopDuration;
}

[CreateAssetMenu(fileName = "NewComboStrategy", menuName = "Combat Strategy")]
public class ComboStrategySO : ScriptableObject
{
    [Header("Settings")]
    public string comboName;
    public float baseDamage = 10f;
    public float attackSpeed = 1.5f;

    public List<ComboConnection> combos;

    [Header("Combo Chain")]
    public List<AttackAction> actions;

    public int GetStartingIndex(CombatCommand command)
    {
        foreach(var next in combos)
        {
            if (next.commandType == command)
                return next.nextComboIndex;
        }
        return -1;
    }
}
