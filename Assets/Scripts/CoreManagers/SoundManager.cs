using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 게임 내 모든 사운드를 총괄하는 매니저.
/// GameCore 하위에 배치.
///
/// BGM:
///   SoundManager.Instance.PlayBGM(audioClip);
///   SoundManager.Instance.StopBGM();
///
/// UI SFX (2D):
///   SoundManager.Instance.PlayUI(clipReference);
///
/// 3D SFX (위치 기반):
///   SoundManager.Instance.PlaySFX(clip, position);
///   SoundManager.Instance.PlaySFX(clip, position, volume, pitch);
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer (Optional)")]
    [SerializeField] private AudioMixerGroup _bgmMixerGroup;
    [SerializeField] private AudioMixerGroup _sfxMixerGroup;
    [SerializeField] private AudioMixerGroup _uiMixerGroup;

    [Header("BGM Settings")]
    [SerializeField] private float _bgmVolume = 0.5f;
    [SerializeField] private float _crossFadeDuration = 1.5f;

    [Header("SFX Pool Settings")]
    [SerializeField] private int _initialPoolSize = 10;
    [SerializeField] private int _maxPoolSize = 20;

    [Header("Mixer Parameter Names")]
    [SerializeField] private string _masterParam = "MasterVolume";
    [SerializeField] private string _bgmParam = "BGMVolume";
    [SerializeField] private string _sfxParam = "SFXVolume";
    [SerializeField] private string _uiParam = "UIVolume";

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer _audioMixer;


    private AudioSource _bgmSourceA;
    private AudioSource _bgmSourceB;
    private bool _isBgmA = true;
    private Coroutine _crossFadeCoroutine;

    private AudioSource _uiSource;

    private Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
    private List<AudioSource> _activeSfxSources = new List<AudioSource>();
    private Transform _poolParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeBGM();
        InitializeUI();
        InitializeSFXPool();
    }

    private void Update()
    {
        ReturnFinishedSources();
    }

    #region Initialization

    private void InitializeBGM()
    {
        _bgmSourceA = CreateAudioSource("BGM_A");
        _bgmSourceB = CreateAudioSource("BGM_B");

        ConfigureBGMSource(_bgmSourceA);
        ConfigureBGMSource(_bgmSourceB);
    }

    private void ConfigureBGMSource(AudioSource source)
    {
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D
        source.volume = 0f;
        if (_bgmMixerGroup != null) source.outputAudioMixerGroup = _bgmMixerGroup;
    }

    private void InitializeUI()
    {
        _uiSource = CreateAudioSource("UI_SFX");
        _uiSource.loop = false;
        _uiSource.playOnAwake = false;
        _uiSource.spatialBlend = 0f;
        if (_uiMixerGroup != null) _uiSource.outputAudioMixerGroup = _uiMixerGroup;
    }

    private void InitializeSFXPool()
    {
        _poolParent = new GameObject("SFX_Pool").transform;
        _poolParent.SetParent(transform);

        for (int i = 0; i < _initialPoolSize; i++)
        {
            _sfxPool.Enqueue(CreatePooledSource());
        }
    }

    private AudioSource CreateAudioSource(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        return go.AddComponent<AudioSource>();
    }

    private AudioSource CreatePooledSource()
    {
        GameObject go = new GameObject("SFX_Source");
        go.transform.SetParent(_poolParent);
        go.SetActive(false);

        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f; // 3D
        source.minDistance = 1f;
        source.maxDistance = 30f;
        source.rolloffMode = AudioRolloffMode.Linear;
        if (_sfxMixerGroup != null) source.outputAudioMixerGroup = _sfxMixerGroup;

        return source;
    }

    #endregion

    #region BGM

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        AudioSource current = _isBgmA ? _bgmSourceA : _bgmSourceB;

        // 같은 곡이면 무시
        if (current.clip == clip && current.isPlaying) return;

        AudioSource next = _isBgmA ? _bgmSourceB : _bgmSourceA;
        _isBgmA = !_isBgmA;

        next.clip = clip;
        next.Play();

        if (_crossFadeCoroutine != null) StopCoroutine(_crossFadeCoroutine);
        _crossFadeCoroutine = StartCoroutine(CrossFadeRoutine(current, next));
    }

    public void StopBGM()
    {
        if (_crossFadeCoroutine != null) StopCoroutine(_crossFadeCoroutine);

        StartCoroutine(FadeOutRoutine(_isBgmA ? _bgmSourceA : _bgmSourceB));
    }

    private IEnumerator CrossFadeRoutine(AudioSource fadeOut, AudioSource fadeIn)
    {
        float t = 0f;
        float startVolumeOut = fadeOut.volume;

        while (t < _crossFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / _crossFadeDuration;

            fadeOut.volume = Mathf.Lerp(startVolumeOut, 0f, progress);
            fadeIn.volume = Mathf.Lerp(0f, _bgmVolume, progress);

            yield return null;
        }

        fadeOut.volume = 0f;
        fadeOut.Stop();
        fadeOut.clip = null;

        fadeIn.volume = _bgmVolume;
        _crossFadeCoroutine = null;
    }

    private IEnumerator FadeOutRoutine(AudioSource source)
    {
        float t = 0f;
        float startVolume = source.volume;

        while (t < _crossFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, t / _crossFadeDuration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
        source.clip = null;
    }

    #endregion

    #region UI SFX

    public void PlayUI(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        _uiSource.PlayOneShot(clip, volume);
    }

    #endregion

    #region 3D SFX

    /// <summary>
    /// 지정 위치에서 3D 사운드 재생.
    /// </summary>
    public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetPooledSource();
        if (source == null) return;

        source.transform.position = position;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.gameObject.SetActive(true);
        source.Play();

        _activeSfxSources.Add(source);
    }

    /// <summary>
    /// 특정 Transform에 붙어서 따라가는 3D 사운드.
    /// 캐릭터 발소리 등에 사용.
    /// </summary>
    public void PlaySFXAttached(AudioClip clip, Transform parent, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || parent == null) return;

        AudioSource source = GetPooledSource();
        if (source == null) return;

        source.transform.SetParent(parent);
        source.transform.localPosition = Vector3.zero;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.gameObject.SetActive(true);
        source.Play();

        _activeSfxSources.Add(source);
    }

    private AudioSource GetPooledSource()
    {
        if (_sfxPool.Count > 0)
        {
            return _sfxPool.Dequeue();
        }

        //풀이 비었으면 최대치 내에서 새로 생성
        if (_activeSfxSources.Count < _maxPoolSize)
        {
            return CreatePooledSource();
        }

        Debug.LogWarning("[Sound] SFX 풀이 가득 찼습니다.");
        return null;
    }

    /// <summary>
    /// 재생 끝난 AudioSource를 풀로 반환.
    /// </summary>
    private void ReturnFinishedSources()
    {
        for (int i = _activeSfxSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = _activeSfxSources[i];

            if (source == null)
            {
                _activeSfxSources.RemoveAt(i);
                continue;
            }

            if (!source.isPlaying)
            {
                source.clip = null;
                source.transform.SetParent(_poolParent);
                source.gameObject.SetActive(false);

                _sfxPool.Enqueue(source);
                _activeSfxSources.RemoveAt(i);
            }
        }
    }
    #endregion
    #region Vol Regulate
    public void SetMasterVolume(float value)
    {
        _audioMixer.SetFloat(_masterParam, ConvertToDecibel(value));
    }

    public void SetBGMVolume(float value)
    {
        _audioMixer.SetFloat(_bgmParam, ConvertToDecibel(value));
    }

    public void SetSFXVolume(float value)
    {
        _audioMixer.SetFloat(_sfxParam, ConvertToDecibel(value));
    }

    public void SetUIVolume(float value)
    {
        _audioMixer.SetFloat(_uiParam, ConvertToDecibel(value));
    }
    private float ConvertToDecibel(float value)
    {
        if (value <= 0.0001f) return -80f;
        return Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
    }
    #endregion
    #region Vol Save/Restore
    public float GetMasterVolume()
    {
        _audioMixer.GetFloat(_masterParam, out float db);
        return ConvertFromDecibel(db);
    }

    public float GetBGMVolume()
    {
        _audioMixer.GetFloat(_bgmParam, out float db);
        return ConvertFromDecibel(db);
    }

    public float GetSFXVolume()
    {
        _audioMixer.GetFloat(_sfxParam, out float db);
        return ConvertFromDecibel(db);
    }

    public float GetUIVolume()
    {
        _audioMixer.GetFloat(_uiParam, out float db);
        return ConvertFromDecibel(db);
    }

    private float ConvertFromDecibel(float db)
    {
        return Mathf.Pow(10f, db / 20f);
    }
    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVol", GetMasterVolume());
        PlayerPrefs.SetFloat("BGMVol", GetBGMVolume());
        PlayerPrefs.SetFloat("SFXVol", GetSFXVolume());
        PlayerPrefs.SetFloat("UIVol", GetUIVolume());
        PlayerPrefs.Save();
    }

    public void LoadVolumeSettings()
    {
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVol", 1f));
        SetBGMVolume(PlayerPrefs.GetFloat("BGMVol", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVol", 1f));
        SetUIVolume(PlayerPrefs.GetFloat("UIVol", 1f));
    }
    #endregion
}
