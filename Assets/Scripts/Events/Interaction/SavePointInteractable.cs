using Core;
using PlayerControllerScripts;
using UnityEngine;
using UnityEngine.Events;

public class SavePointInteractable : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private string _prompt = "세이브 포인트";

    [Header("Events")]
    [SerializeField] private UnityEvent _onOpenShop;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioClip _interactSFX;

    [Header("GameState")]
    [SerializeField] private GameStateSO _gameState;

    [Header("Broadcast Channel")]
    [SerializeField] private StringEventChannelSO _promptChannel;

    public string InteractionPrompt => $"F {_prompt}";
    public bool CanInteract => true;
    public Transform ObjectTransform => transform;

    public void Interact(GameObject interactor)
    {
        if (_interactSFX != null)
            SoundManager.Instance?.PlaySFX(_interactSFX, transform.position);

        _promptChannel?.RaiseEvent("");
        GameData data = Core.GameCore.Instance?.DataManager?.CurrentData;
        if(data != null)
        {
            data.PlayerPosX = transform.position.x;
            data.PlayerPosY = transform.position.y;
            data.PlayerPosZ = transform.position.z;
        }
        //게임 일시정지
        if (_gameState != null)
            _gameState.SetState(GameState.StatShop);
        if (Core.GameCore.Instance?.TimeManager != null)
            Core.GameCore.Instance.TimeManager.PauseTime();

        _onOpenShop?.Invoke();
    }
}