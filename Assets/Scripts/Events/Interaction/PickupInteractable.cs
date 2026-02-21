using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 추후 추가내용.
/// </summary>
public class PickupInteractable : MonoBehaviour, IInteractable
{
    [Header("Pickup Settings")]
    [SerializeField] private string _prompt = "줍기";

    [Header("Events")]
    [SerializeField] private UnityEvent _onPickedUp;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioClip _pickupSFX;

    private bool _isPickedUp = false;

    public string InteractionPrompt => $"[F] {_prompt}";
    public bool CanInteract => !_isPickedUp;

    public Transform ObjectTransform => transform;

    public void Interact(GameObject interactor)
    {
        _isPickedUp = true;


        //데이터칩 대신 다른 오브젝트 사용할예정
        /*
        if (_dataChipsAmount > 0 && GameCore.Instance != null)
        {
            GameCore.Instance.DataManager.AddDatachips(_dataChipsAmount);
        }
        */

        _onPickedUp?.Invoke();
        if (_pickupSFX != null) SoundManager.Instance?.PlaySFX(_pickupSFX, transform.position);

        Destroy(gameObject, 0.1f);
    }
}
