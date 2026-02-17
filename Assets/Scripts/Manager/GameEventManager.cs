using System;
using UnityEngine;


namespace Core
{
    public  static class GameEventManager
    {
        public static event Action<Vector3> OnParrySuccess;

        public static event Action<Transform> OnParryWindowOpen;
        public static event Action<Transform> OnParryWindowClose;

        public static void TriggerParryWindowOpen(Transform target)
        {
            OnParryWindowOpen?.Invoke(target);
        }
        public static void TriggerParryWindowClose(Transform target)
        {
            OnParryWindowClose?.Invoke(target);
        }
        public static void TriggerParrySuccess(Vector3 hitPoint)
        {
            OnParrySuccess?.Invoke(hitPoint);
        }
    }
}
