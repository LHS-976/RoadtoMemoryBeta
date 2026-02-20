using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core;

public class Bootstrap : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameSceneSO _titleScene;
    [SerializeField] private float _splashTime = 2.0f;

    [Header("Transition")]
    [SerializeField] private PanelFader _sceneFader;

    private void Start()
    {
        StartCoroutine(BootSequence());
    }
    private IEnumerator BootSequence()
    {
        GameCore.Instance.InitializeManagers();

        yield return new WaitForSeconds(_splashTime);

        if(_sceneFader != null)
        {
            _sceneFader.FadeIn();
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("[Bootstrap] 타이틀 씬으로 이동합니다.");

        string nextScene = _titleScene != null ? _titleScene.sceneName : "01_TitleScene";
        SceneManager.LoadSceneAsync(nextScene);
    }
}