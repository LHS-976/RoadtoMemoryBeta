using UnityEngine;

public interface IInteractable
{
    string InteractionPrompt { get; }
    bool CanInteract { get; }

    Transform ObjectTransform { get; }
    void Interact(GameObject interactor);
}
