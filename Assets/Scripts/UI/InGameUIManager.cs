using Core;
using UnityEngine;

public class InGameUIManager : MonoBehaviour
{
    [Header("Channels")]
    [SerializeField] private GameStateSO _gameState;

    [Header("Panels")]
    [SerializeField] private PanelFader _optionPanel;
    [SerializeField] private PanelFader _hudPanel;


    private void Awake()
    {
        if(_gameState == null)
        {
            Debug.LogWarning("GameStateSO 데이터 없음.");
        }
    }
    private void OnEnable()
    {
        if (_gameState != null)
            _gameState.OnStateChange += HandleStateChange;
    }
    private void OnDisable()
    {
        if(_gameState != null)
            _gameState.OnStateChange -= HandleStateChange;
    }
    private void HandleStateChange(GameState state)
    {
        _optionPanel.FadeOut();
        _hudPanel.FadeOut();

        switch(state)
        {
            case GameState.Gameplay:
                _hudPanel.FadeIn();
                break;
            case GameState.Option:
                _optionPanel.FadeIn();
                break;
        }
    }
}
