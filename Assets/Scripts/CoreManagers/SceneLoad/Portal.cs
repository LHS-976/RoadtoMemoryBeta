using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private GameSceneSO _nextSceneData;

    [SerializeField] private StringEventChannelSO _loadMapChannel;
    [SerializeField] private LayerMask _targetLayers;
    
    private void OnTriggerEnter(Collider other)
    {
        if(((1<<other.gameObject.layer) & _targetLayers) != 0)
        {
            _loadMapChannel.RaiseEvent(_nextSceneData.sceneName);
        }
    }
}
