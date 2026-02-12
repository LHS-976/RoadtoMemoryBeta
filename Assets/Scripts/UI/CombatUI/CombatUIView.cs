using PlayerControllerScripts;
using UnityEngine;


/// <summary>
/// crosshair 추가, 추후 이벤트 연결예정
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
            UpdateUI(player.isCombatMode);
        }
    }
    void UpdateUI(bool isCombat)
    {
        if (Crosshair != null) Crosshair.SetActive(isCombat);
    }

    void OnDisable()
    {
        if (player != null) player.OnCombatStateChanged -= UpdateUI;
    }
}