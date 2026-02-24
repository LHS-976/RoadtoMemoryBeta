using Core;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    [Header("Channels")]
    [SerializeField] private GameStateSO _gameState;

    [Header("Panels")]
    [SerializeField] private PanelFader _optionPanel;
    [SerializeField] private PanelFader _hudPanel;

    [Header("Listening Channel")]
    [SerializeField] private PlayerUIEventChannelSO _playerUIChannel;

    [Header("UI References")]
    [SerializeField] private RectTransform _hpBackgroundRect;
    [SerializeField] private Image _hpMaskImage;
    [SerializeField] private RectTransform _staminaBackgroundRect;
    [SerializeField] private Image _staminaMaskImage;

    [Header("Animation Settings")]
    [SerializeField] private float _barLerpSpeed = 15f;

    [Header("Bar Add Size Settings")]
    private const float _hpWidthMultiplier = 72f;
    private const float _staminaWidthMultiplier = 72f;


    private float _targetHpFill = 1f;
    private float _targetStaminaFill = 1f;
    private void Awake()
    {
        if (_gameState != null)
        {
            HandleStateChange(_gameState.CurrentState);
        }
        else
        {
            Debug.LogWarning("_gameState가 비어있습니다.");
        }

    }
    private void OnEnable()
    {
        if (_gameState != null)
            _gameState.OnStateChange += HandleStateChange;
        if (_playerUIChannel != null)
            _playerUIChannel.OnEventRaised += OnPlayerUIUpdated;
    }
    private void OnDisable()
    {
        if(_gameState != null)
            _gameState.OnStateChange -= HandleStateChange;
        if (_playerUIChannel != null)
            _playerUIChannel.OnEventRaised -= OnPlayerUIUpdated;
    }
    private void HandleStateChange(GameState state)
    {
        switch(state)
        {
            case GameState.Gameplay:
                _hudPanel.FadeIn();
                _optionPanel.FadeOut();
                break;
            case GameState.Option:
                _hudPanel.FadeOut();
                _optionPanel.FadeIn();
                break;
            case GameState.Dialogue:
                _hudPanel.FadeOut();
                _optionPanel.FadeOut();
                break;
        }
    }

    private void OnPlayerUIUpdated(PlayerUIPayload payload)
    {
        if(payload.maxHp > 0)
        {
            _targetHpFill = payload.currentHp / payload.maxHp;
            if(_hpBackgroundRect != null)
            {
                float exactWidth = (payload.maxHp/10) * _hpWidthMultiplier;
                _hpBackgroundRect.sizeDelta = new Vector2(exactWidth, _hpBackgroundRect.sizeDelta.y);
            }
        }
        if(payload.maxStamina >0)
        {
            _targetStaminaFill = payload.currentStamina / payload.maxStamina;
            if (_staminaBackgroundRect != null)
            {
                float exactWidth = (payload.maxStamina / 10) * _staminaWidthMultiplier;
                _staminaBackgroundRect.sizeDelta = new Vector2(exactWidth, _staminaBackgroundRect.sizeDelta.y);
            }
        }
    }
    private void Update()
    {
        if(_hpMaskImage != null)
        {
            _hpMaskImage.fillAmount = Mathf.Lerp(_hpMaskImage.fillAmount, _targetHpFill, Time.deltaTime * _barLerpSpeed);
        }
        if(_staminaMaskImage != null)
        {
            _staminaMaskImage.fillAmount = Mathf.Lerp(_staminaMaskImage.fillAmount, _targetStaminaFill, Time.deltaTime * _barLerpSpeed);
        }
    }
}
