using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider _progressBar;
    [SerializeField] private PanelFader _sceneFader;

    private const float _fadeWaitTime = 0.35f;
    private const float _minimumLoadingTime = 2.5f;
    private const float _fillLoadingSpeed = 5f;

    //다른 씬에서 로딩 씬으로 넘어오기 전에, 목적지를 여기에 적어둡니다.
    public static GameSceneSO TargetScene;

    private void Start()
    {
        if (TargetScene != null)
        {
            StartCoroutine(LoadTargetSceneAsync());
        }
        else
        {
            Debug.LogError("[Loading] 목표 씬 데이터(SO)가 없습니다!");
        }
    }

    private IEnumerator LoadTargetSceneAsync()
    {
        if(_sceneFader != null)
        {
            _sceneFader.SetImmediateClosed();
            _sceneFader.FadeIn();
            yield return new WaitForSeconds(_fadeWaitTime);
        }

        System.GC.Collect();
        Resources.UnloadUnusedAssets();

        AsyncOperation op = SceneManager.LoadSceneAsync(TargetScene.sceneName);
        op.allowSceneActivation = false;

        float timer = 0.0f;
        bool isComplete = false;
        while (!op.isDone)
        {
            yield return null;
            if (isComplete) continue;
            timer += Time.unscaledDeltaTime;

            float realProgress = Mathf.Clamp01(op.progress / 0.9f);
            float fakeProgress = Mathf.Clamp01(timer / _minimumLoadingTime);
            float targetProgress = Mathf.Min(realProgress, fakeProgress);

            _progressBar.value = Mathf.Lerp(_progressBar.value, targetProgress, Time.unscaledDeltaTime * _fillLoadingSpeed);
            if (_progressBar.value >= 0.99f && realProgress >= 1f && fakeProgress >= 1f)
            {
                isComplete = true;
                _progressBar.value = 1f;

                if(_sceneFader != null)
                {
                    _sceneFader.FadeOut();
                    yield return new WaitForSeconds(_fadeWaitTime);
                }
                op.allowSceneActivation = true;
            }
        }
    }
}