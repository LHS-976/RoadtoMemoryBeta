using System.Collections;
using UnityEngine;

public class PanelFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float fadeDuration = 0.35f;

    private Coroutine _currentCoroutine;

    private void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group == null) group = GetComponentInChildren<CanvasGroup>(true);

        SetImmediateClosed();
    }

    public void SetImmediateClosed()
    {
        StopCurrentCoroutine();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    public void SetImmediateOpened()
    {
        StopCurrentCoroutine();
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    public void FadeIn()
    {
        StopCurrentCoroutine();
        gameObject.SetActive(true);
        _currentCoroutine = StartCoroutine(CoFade(1f));
    }

    public void FadeOut()
    {
        StopCurrentCoroutine();
        _currentCoroutine = StartCoroutine(CoFade(0f));
    }

    private void StopCurrentCoroutine()
    {
        if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
        _currentCoroutine = null;
    }

    private IEnumerator CoFade(float targetAlpha)
    {
        group.interactable = false;
        group.blocksRaycasts = false;

        float start = group.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float n = t / fadeDuration;
            group.alpha = Mathf.Lerp(start, targetAlpha, Mathf.SmoothStep(0, 1, n));
            yield return null;
        }

        group.alpha = targetAlpha;

        if (targetAlpha >= 0.99f)
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        else
        {
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        _currentCoroutine = null;
    }
}