using System.Collections.Generic;
using UnityEngine;
using Core;


/// <summary>
/// 오브젝트 사용이유. 누구를 패리해야 하는지 직관적으로 보여주기 위함.
/// </summary>
public class ParryUIController : MonoBehaviour
{
    [SerializeField] private GameObject _parryIndicatorPrefab;

    private Dictionary<Transform, GameObject> _activeIndicators = new Dictionary<Transform, GameObject>();

    private void OnEnable()
    {
        GameEventManager.OnParryWindowOpen += ShowIndicator;
        GameEventManager.OnParryWindowClose += HideIndicator;
    }

    private void OnDisable()
    {
        GameEventManager.OnParryWindowOpen -= ShowIndicator;
        GameEventManager.OnParryWindowClose -= HideIndicator;
    }

    private void ShowIndicator(Transform target)
    {
        if (_activeIndicators.ContainsKey(target)) return;

        GameObject ui = Instantiate(_parryIndicatorPrefab, target.position + Vector3.up * 0.5f, Quaternion.identity);
        ui.transform.SetParent(target);

        _activeIndicators.Add(target, ui);
    }

    private void HideIndicator(Transform target)
    {
        if(_activeIndicators.TryGetValue(target, out GameObject ui))
        {
            if (ui != null) Destroy(ui);
            _activeIndicators.Remove(target);
        }
    }
}
