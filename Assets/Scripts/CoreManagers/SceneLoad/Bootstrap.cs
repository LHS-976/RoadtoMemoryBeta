using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core;

public class Bootstrap : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameSceneSO _titleScene;
    [SerializeField] private float _splashTime = 2.0f;

    private void Start()
    {
        StartCoroutine(BootSequence());
    }
    private IEnumerator BootSequence()
    {
        GameCore.Instance.InitializeManagers();

        yield return new WaitForSeconds(_splashTime);

        Debug.Log("[Bootstrap] 타이틀 씬으로 이동합니다.");

        string nextScene = _titleScene != null ? _titleScene.sceneName : "01_TitleScene";
        SceneManager.LoadSceneAsync(nextScene);
    }
}