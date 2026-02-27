using UnityEngine;
using TMPro;
using System;
using System.Collections;
using Core;

public class DialogueUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private PanelFader _dialoguePanel;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private GameStateSO _currentGameState;

    [Header("Settings")]
    [SerializeField] private float _typingSpeed = 0.05f;
    [SerializeField] private float _fadeOutTime = 0.4f;

    private string[] _currentDialogues;
    private int _currentIndex = 0;
    private Action _onDialogueFinished;
    private bool _showDialogue = false;

    private Coroutine _typingCoroutine;
    private bool _isTyping = false;


    private void Awake()
    {
        if(Core.GameCore.Instance != null && Core.GameCore.Instance.QuestManager != null)
        {
            Core.GameCore.Instance.QuestManager.RegisterDialogueUI(this);
        }
    }
    private void Update()
    {
        if (_showDialogue && (Input.GetKeyDown(KeyCode.Z)))
        {
            if (_isTyping)
            {
                if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
                _dialogueText.text = _currentDialogues[_currentIndex];
                _isTyping = false;
            }
            else
            {
                ShowNextDialogue();
            }
        }
    }

    public void StartDialogue(string[] dialogues, Action onFinished)
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            onFinished?.Invoke();
            return;
        }
        _currentGameState.SetState(GameState.Dialogue);

        if(Core.GameCore.Instance != null && Core.GameCore.Instance.TimeManager != null)
        {
            Core.GameCore.Instance.TimeManager.PauseTime();
        }

        _currentDialogues = dialogues;
        _currentIndex = 0;
        _onDialogueFinished = onFinished;

        _dialoguePanel.FadeIn();
        _showDialogue = true;
        UpdateDialogueUI();
    }

    private void ShowNextDialogue()
    {
        _currentIndex++;

        if (_currentIndex >= _currentDialogues.Length)
        {
            EndDialogue();
        }
        else
        {
            UpdateDialogueUI();
        }
    }

    private void UpdateDialogueUI()
    {
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _typingCoroutine = StartCoroutine(TypewriterRoutine(_currentDialogues[_currentIndex]));
    }
    private IEnumerator TypewriterRoutine(string line)
    {
        _isTyping = true;
        _dialogueText.text = "";

        foreach (char c in line.ToCharArray())
        {
            _dialogueText.text += c;
            yield return new WaitForSecondsRealtime(_typingSpeed);
        }

        _isTyping = false;
    }
    private void CallFinish()
    {
        if (Core.GameCore.Instance != null && Core.GameCore.Instance.TimeManager != null)
        {
            Core.GameCore.Instance.TimeManager.ForceRestoreTime();
        }
        _currentGameState.RestorePreviousState();
        _onDialogueFinished?.Invoke();
    }
    private void EndDialogue()
    {
        _dialoguePanel.FadeOut();
        _showDialogue = false;
        StartCoroutine(WaitAndCallFinishRoutine());
    }
    private IEnumerator WaitAndCallFinishRoutine()
    {
        yield return new WaitForSecondsRealtime(_fadeOutTime);

        CallFinish();
    }
}