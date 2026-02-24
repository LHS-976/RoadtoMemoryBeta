using UnityEngine;

[System.Serializable]
public struct PlayerUIPayload
{
    public float currentHp;
    public float maxHp;
    public float currentStamina;
    public float maxStamina;
}
[CreateAssetMenu(fileName = "NewPlayerUIChannel", menuName = "Events/Player UI Event Channel")]
public class PlayerUIEventChannelSO : BaseEventChannelSO<PlayerUIPayload>
{   
}
