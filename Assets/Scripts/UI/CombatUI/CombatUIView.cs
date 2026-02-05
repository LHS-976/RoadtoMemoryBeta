using PlayerControllerScripts;
using UnityEngine;


/// <summary>
/// crosshair 추가예정
/// </summary>
public class CombatUIView : MonoBehaviour
{
    public PlayerController player;
    public GameObject Crosshair;
    public GameObject WeaponIcon;

    void Start()
    {
        if (player != null)
        {
            player.OnCombatStateChanged += UpdateUI;
            UpdateUI(player.IsCombatMode);
        }
    }
    void UpdateUI(bool isCombat)
    {
        if (Crosshair != null) Crosshair.SetActive(isCombat);
        if (WeaponIcon != null) WeaponIcon.SetActive(isCombat);
    }

    void OnDisable()
    {
        if (player != null) player.OnCombatStateChanged -= UpdateUI;
    }
}