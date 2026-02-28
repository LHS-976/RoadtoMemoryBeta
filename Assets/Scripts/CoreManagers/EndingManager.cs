using UnityEngine;
using TMPro;
using UnityEngine.Video;
using Core;
using System.Collections;

public class EndingManager : MonoBehaviour
{
    [SerializeField] private GameStateSO _gameState;

    [Header("Video Settings")]
    [SerializeField] private VideoPlayer _endingVideo;
    [SerializeField] private PanelFader _videoPanel;

    [Header("Result UI Elements")]
    [SerializeField] private PanelFader _resultPanel;
    [SerializeField] private TextMeshProUGUI _totalChipsText;
    [SerializeField] private TextMeshProUGUI _playTimeText;
    [SerializeField] private UnityEngine.UI.Button _goToTitleButton;

    [Header("BGM")]
    [SerializeField] private AudioClip _endingBGM;

    private void Start()
    {
        // 초기 UI 상태 설정
        _resultPanel.SetImmediateClosed();

        if (_goToTitleButton != null)
            _goToTitleButton.onClick.AddListener(OnClickGoToTitle);

        //엔딩 시퀀스 시작
        StartCoroutine(EndingSequenceRoutine());
    }

    private IEnumerator EndingSequenceRoutine()
    {
        //영상 재생
        _videoPanel.FadeIn();

        _endingVideo.Prepare();
        while (!_endingVideo.isPrepared)
        {
            yield return null;
        }
        _endingVideo.Play();
        yield return null;
        while (_endingVideo.isPlaying)
        {
            if (Input.anyKeyDown) break;
            yield return null;
        }

        _endingVideo.Stop();
        _videoPanel.FadeOut();
        yield return new WaitForSeconds(1f); // 자연스러운 전환을 위한 대기

        ShowFinalResultAndClearData();
    }

    private void ShowFinalResultAndClearData()
    {
        if (GameCore.Instance != null && GameCore.Instance.DataManager != null)
        {
            var dataManager = GameCore.Instance.DataManager;
            var data = dataManager.CurrentData;

            if (data != null)
            {
                if (_totalChipsText != null) _totalChipsText.text = $"{data.DataChips} CHIPS";
                if (_playTimeText != null) _playTimeText.text = FormatPlayTime(data.PlayTime);

                dataManager.DeleteCurrentSlot();
            }
        }

        if (_gameState != null)
        {
            _gameState.SetState(GameState.EndingState);
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _resultPanel?.FadeIn();
    }

    private void OnClickGoToTitle()
    {
        if (GameCore.Instance != null && GameCore.Instance.Scene != null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("01_TitleScene");
        }
    }

    private string FormatPlayTime(float seconds)
    {
        int h = Mathf.FloorToInt(seconds / 3600f);
        int m = Mathf.FloorToInt((seconds % 3600f) / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
    }
}