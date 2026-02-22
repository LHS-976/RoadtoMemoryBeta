using PlayerControllerScripts;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private KeyCode _interactKey = KeyCode.F;

    [Header("Broadcasting Channels (S0)")]
    [Tooltip("UI에 상호작용 텍스트를 띄우거나 숨길 때 사용채널")]
    [SerializeField] private StringEventChannelSO _promptChannel;

    [Tooltip("상호작용 사운드/VFX 채널")]
    [SerializeField] private VoidEventChannelSO _interactedChannel;

    private PlayerController _playerController;
    private List<IInteractable> _nearbyTargets = new List<IInteractable>();
    private IInteractable _closestTarget;

    private void Awake()
    {
        if(_playerController == null) _playerController = GetComponent<PlayerController>();
    }
    private void Update()
    {
       if(!CanPlayerInteract())
        {
            if(_closestTarget != null)
            {
                _closestTarget = null;
                _promptChannel?.RaiseEvent(null);
            }
            return;
        }
        UpdateClosestTarget();
        if(_closestTarget != null && Input.GetKeyDown(_interactKey))
        {
            if (_closestTarget.CanInteract)
            {
                _closestTarget.Interact(gameObject);
                _interactedChannel?.RaiseEvent();

                if (!_closestTarget.CanInteract)
                {
                    _nearbyTargets.Remove(_closestTarget);
                    _closestTarget = null;
                    _promptChannel?.RaiseEvent(null);
                }
            }
        }
    }
    #region Detection

    private void OnTriggerEnter(Collider other)
    {
        if (!IsInLayer(other.gameObject.layer)) return;

        IInteractable interactable = other.GetComponent<IInteractable>();
        if(interactable != null && !_nearbyTargets.Contains(interactable))
        {
            _nearbyTargets.Add(interactable);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!IsInLayer(other.gameObject.layer)) return;

        IInteractable interactable = other.GetComponent<IInteractable>();
        if(interactable != null)
        {
            _nearbyTargets.Remove(interactable);

            if(_closestTarget == interactable)
            {
                _closestTarget = null;
                _promptChannel?.RaiseEvent(null);
            }
        }
    }
    #endregion
    #region Target Selection
    private void UpdateClosestTarget()
    {
        _nearbyTargets.RemoveAll(t => t == null || (t as MonoBehaviour) == null);

        IInteractable best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < _nearbyTargets.Count; i++)
        {
            IInteractable target = _nearbyTargets[i];
            if (!target.CanInteract) continue;

            if (target.ObjectTransform == null) continue;

            float dist = Vector3.Distance(transform.position, target.ObjectTransform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = target;
            }
        }

        if (best != _closestTarget)
        {
            _closestTarget = best;

            _promptChannel?.RaiseEvent(_closestTarget?.InteractionPrompt);
        }
    }
    #endregion

    #region Validation

    private bool CanPlayerInteract()
    {
        if (_playerController == null) return false;

        PlayerBaseState state = _playerController.CurrentState;

        if (state is PlayerHitState) return false;
        if (state is PlayerExecutionState) return false;
        if (state is PlayerCombatState combat && combat.UseRootMotion) return false;

        return true;
    }

    private bool IsInLayer(int layer)
    {
        return (_interactableLayer & (1 << layer)) != 0;
    }

    public IInteractable GetClosestTarget() => _closestTarget;

    #endregion
}
