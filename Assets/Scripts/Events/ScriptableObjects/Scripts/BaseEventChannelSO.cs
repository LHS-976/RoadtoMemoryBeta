using UnityEngine;
using UnityEngine.Events;

public abstract class BaseEventChannelSO<T> : ScriptableObject
{
    public UnityAction<T> OnEventRaised;

    public void RaiseEvent(T value)
    {
        OnEventRaised?.Invoke(value);
    }
}

