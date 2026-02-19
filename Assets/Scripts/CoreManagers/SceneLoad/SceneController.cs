using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider _progressBar;
    [SerializeField] private CanvasGroup _LoadingScene;

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
        System.GC.Collect();
        Resources.UnloadUnusedAssets();

        AsyncOperation op = SceneManager.LoadSceneAsync(TargetScene.sceneName);
        op.allowSceneActivation = false;

        float timer = 0.0f;

        while (!op.isDone)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;

            if (op.progress < 0.9f)
            {
                _progressBar.value = Mathf.Lerp(_progressBar.value, op.progress, timer);
                if (_progressBar.value >= op.progress) timer = 0f;
            }
            else
            {
                _progressBar.value = Mathf.Lerp(_progressBar.value, 1f, timer);
                if (_progressBar.value >= 1.0f)
                {
                    //페이드 아웃 연출
                    op.allowSceneActivation = true;
                }
            }
        }
    }
}