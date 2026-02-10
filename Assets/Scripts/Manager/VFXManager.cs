using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    private Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayVFX(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return;

        if(!_pools.ContainsKey(prefab))
        {
            _pools.Add(prefab, new Queue<GameObject>());
        }

        GameObject vfxInstance = null;

        while (_pools[prefab].Count >0)
        {
            vfxInstance = _pools[prefab].Dequeue();
            if(vfxInstance != null)
            {
                break;
            }
        }
        if(vfxInstance == null)
        {
            vfxInstance = CreateNewInstance(prefab);
        }
        vfxInstance.transform.position = position;
        vfxInstance.transform.rotation = rotation;
        vfxInstance.SetActive(true);

        vfxInstance.transform.SetParent(null);

    }
    private GameObject CreateNewInstance(GameObject prefab)
    {
        GameObject newObj = Instantiate(prefab);

        var returner = newObj.AddComponent<AutoReturnToPool>();
        returner.Initialize(prefab, this);

        return newObj;
    }

    public void ReturnToPool(GameObject prefab, GameObject instance)
    {
        if (instance == null) return;

        instance.SetActive(false);
        instance.transform.SetParent(transform);

        if(_pools.ContainsKey(prefab))
        {
            _pools[prefab].Enqueue(instance);
        }
        else
        {
            Destroy(instance);
        }
    }
}
