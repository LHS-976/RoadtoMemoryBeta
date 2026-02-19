using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRouter : MonoBehaviour
{
    [SerializeField] private GameSceneEventChannelSO _loadMapChannel;

    private void OnEnable()
    {
        if (_loadMapChannel != null) _loadMapChannel.OnEventRaised += NavigateToLoadingScene;
    }

    private void OnDisable()
    {
        if (_loadMapChannel != null) _loadMapChannel.OnEventRaised -= NavigateToLoadingScene;
    }

    private void NavigateToLoadingScene(GameSceneSO targetScene)
    {
        LoadingSceneController.TargetScene = targetScene;
        SceneManager.LoadScene("LoadingScene");
    }
}