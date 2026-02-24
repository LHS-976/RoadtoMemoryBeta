using System.Collections;
using UnityEngine;


public class AutoReturnToPool : MonoBehaviour
{
    private GameObject _originalPrefab;
    private VFXManager vfxManager;


    [SerializeField] private float _fallbackLifeTime = 1.0f;

    public void Initialize(GameObject prefab, VFXManager manager)
    {
        _originalPrefab = prefab;
        vfxManager = manager;

        CalculateLifeTime();
    }
    private void CalculateLifeTime()
    {
        var particles = GetComponentsInChildren<ParticleSystem>();
        float maxDuration = 0f;

        foreach (var ps in particles)
        {
            if (ps.main.loop)
            {
                return;
            }
            float time = ps.main.duration + ps.main.startLifetime.constantMax;
            if(time > maxDuration)
            {
                maxDuration = time;
            }

        }
        if(maxDuration > 0)
        {
            //기존 파티클
            _fallbackLifeTime = maxDuration + 0.2f;
            return;
        }
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(SafeReturnRoutine());
    }
    private IEnumerator SafeReturnRoutine()
    {

        yield return null;
        yield return new WaitForSeconds(_fallbackLifeTime);

        if(vfxManager != null)
        {
            vfxManager.ReturnToPool(_originalPrefab, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
