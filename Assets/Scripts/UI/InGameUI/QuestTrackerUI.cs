using System.Collections;
using UnityEngine;

public class QuestTrackerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private PanelFader _questPanelFader;

    [Header("Settings")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.F1;
    [SerializeField] private float _autoHideTime = 3.0f;

    [Header("Data & Channels")]
    [Tooltip("퀘스트 시작 방송을 수신할 채널 (Int Channel)")]
    [SerializeField] private IntEventChannelSO _questStartedChannel;

    private Coroutine _autoShowCoroutine;
    private bool _isToggledOn = false;

    private void Awake()
    {
        if (_questPanelFader == null) _questPanelFader = GetComponent<PanelFader>();
    }

    private void OnEnable()
    {
        if (_questStartedChannel != null)
            _questStartedChannel.OnEventRaised += HandleQuestStarted;
    }

    private void OnDisable()
    {
        if (_questStartedChannel != null)
            _questStartedChannel.OnEventRaised -= HandleQuestStarted;
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            ToggleTrackerUI();
        }
    }

    private void HandleQuestStarted(int newQuestID)
    {
        if (_autoShowCoroutine != null) StopCoroutine(_autoShowCoroutine);
        _autoShowCoroutine = StartCoroutine(AutoShowRoutine());
    }

    private IEnumerator AutoShowRoutine()
    {
        _isToggledOn = true;
        _questPanelFader.FadeIn();

        yield return new WaitForSecondsRealtime(_autoHideTime);

        if (_isToggledOn)
        {
            _isToggledOn = false;
            _questPanelFader.FadeOut();
        }
    }

    private void ToggleTrackerUI()
    {
        if (_autoShowCoroutine != null)
        {
            StopCoroutine(_autoShowCoroutine);
            _autoShowCoroutine = null;
        }

        _isToggledOn = !_isToggledOn;

        if (_isToggledOn) _questPanelFader.FadeIn();
        else _questPanelFader.FadeOut();
    }
}