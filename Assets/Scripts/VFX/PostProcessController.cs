using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessController : MonoBehaviour
{
    [Header("Volume & Channels")]
    [SerializeField] private Volume _globalVolume;
    [SerializeField] private VoidEventChannelSO _playerHitChannel;

    [Header("Hit Effect Settings")]
    [SerializeField] private float _hitIntensity = 0.4f; //번쩍일 때의 강도
    [SerializeField] private float _hitDuration = 0.2f; //번쩍이는 데 걸리는 총 시간

    private Vignette _vignette;
    private Coroutine _hitEffectCoroutine;

    private void Awake()
    {
        if (_globalVolume != null && _globalVolume.profile.TryGet(out _vignette))
        {
            _vignette.intensity.value = 0f;
        }
    }
    private void OnEnable()
    {
        if (_playerHitChannel != null)
            _playerHitChannel.OnEventRaised += PlayHitEffect;
    }

    private void OnDisable()
    {
        if (_playerHitChannel != null)
            _playerHitChannel.OnEventRaised -= PlayHitEffect;
    }

    private void PlayHitEffect()
    {
        if (_vignette == null) return;

        //진행 중인 연출이 있으면 끄고 새로 시작
        if (_hitEffectCoroutine != null) StopCoroutine(_hitEffectCoroutine);

        _hitEffectCoroutine = StartCoroutine(HitEffectRoutine());
    }

    private IEnumerator HitEffectRoutine()
    {
        float halfDuration = _hitDuration / 2f;
        float time = 0f;

        //빨간색 퍼짐
        while (time < halfDuration)
        {
            time += Time.deltaTime;
            _vignette.intensity.value = Mathf.Lerp(0f, _hitIntensity, time / halfDuration);
            yield return null;
        }

        time = 0f;

        //다시 서서히 사라지기
        while (time < halfDuration)
        {
            time += Time.deltaTime;
            _vignette.intensity.value = Mathf.Lerp(_hitIntensity, 0f, time / halfDuration);
            yield return null;
        }

        //0으로 초기화
        _vignette.intensity.value = 0f;
    }
}
