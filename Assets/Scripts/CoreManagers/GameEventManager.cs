using System;
using UnityEngine;


namespace Core
{
    public  static class GameEventManager
    {
        public static event Action<Vector3> OnExecutionSuccess;

        public static event Action<Transform> OnExecutionWindowOpen;
        public static event Action<Transform> OnExecutionWindowClose;

        public static void TriggerExecutionWindowOpen(Transform target)
        {
            OnExecutionWindowOpen?.Invoke(target);
        }
        public static void TriggerExecutionWindowClose(Transform target)
        {
            OnExecutionWindowClose?.Invoke(target);
        }
        public static void TriggerExecutionSuccess(Vector3 hitPoint)
        {
            OnExecutionSuccess?.Invoke(hitPoint);
        }
    }
}
