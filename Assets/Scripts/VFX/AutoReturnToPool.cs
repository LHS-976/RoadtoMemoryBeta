using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoReturnToPool : MonoBehaviour
{
    private GameObject _originalPrefab;
    private VFXManager vfxManager;
    private float _lifeTime = 1.0f;

    public void Initialize(GameObject prefab, VFXManager manager)
    {
        _originalPrefab = prefab;
        vfxManager = manager;

        CalculateLifeTime();
    }
    private void CalculateLifeTime()
    {
        var particles = GetComponentsInChildren<ParticleSystem>();
        float maxDuartion = 0f;

        foreach (var ps in particles)
        {
            if (ps.main.loop)
            {
                return;
            }
            float time = ps.main.duration + ps.main.startLifetime.constantMax;
            if(time > maxDuartion)
            {
                maxDuartion = time;
            }

        }
        if(maxDuartion > 0)
        {
            _lifeTime = maxDuartion + 0.2f;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(ReturnRoutine());
    }
    private IEnumerator ReturnRoutine()
    {
        yield return new WaitForSeconds(_lifeTime);

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
