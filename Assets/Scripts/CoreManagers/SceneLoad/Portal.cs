using UnityEngine;
using Core;

public class Portal : MonoBehaviour
{
    [Header("Routing Settings")]
    [SerializeField] private GameSceneSO _nextSceneData;
    [SerializeField] private GameSceneEventChannelSO _loadMapChannel;
    [SerializeField] private LayerMask _targetLayers;

    private bool _hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered) return;

        if (((1 << other.gameObject.layer) & _targetLayers) != 0)
        {
            _hasTriggered = true;
            if (GameCore.Instance?.CurrentPlayer != null && GameCore.Instance?.DataManager != null)
            {
                PlayerManager pm = GameCore.Instance.CurrentPlayer.GetComponent<PlayerManager>();
                if (pm != null)
                {
                    GameCore.Instance.DataManager.CurrentData.CurrentHealth = pm.CurrentHp;
                    GameCore.Instance.DataManager.CurrentData.CurrentStamina = pm.CurrentStamina;
                }
            }
            if (_nextSceneData != null)
            {
                _loadMapChannel.RaiseEvent(_nextSceneData);
            }
        }
    }
}