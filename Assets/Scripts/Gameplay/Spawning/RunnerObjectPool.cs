using System.Collections.Generic;
using UnityEngine;

public class RunnerObjectPool : MonoBehaviour
{
    private static RunnerObjectPool _instance;

    public static RunnerObjectPool Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("RunnerObjectPool");
                _instance = go.AddComponent<RunnerObjectPool>();
            }

            return _instance;
        }
    }

    private readonly Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public GameObject Get(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
    {
        int id = prefab.GetInstanceID();

        if (!_pools.TryGetValue(id, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            _pools[id] = queue;
        }

        GameObject instance;

        if (queue.Count > 0)
        {
            instance = queue.Dequeue();

            if (instance == null)
            {
                instance = Instantiate(prefab);
            }
        }
        else
        {
            instance = (GameObject)Object.Instantiate((Object)prefab);
        }

        instance.transform.SetParent(parent, false);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.SetActive(true);

        return instance;
    }

    public void Release(GameObject instance, GameObject prefab)
    {
        if (instance == null)
        {
            return;
        }

        instance.SetActive(false);
        instance.transform.SetParent(transform, false);

        int id = prefab.GetInstanceID();

        if (!_pools.TryGetValue(id, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            _pools[id] = queue;
        }

        queue.Enqueue(instance);
    }

    public void ClearAll()
    {
        foreach (Queue<GameObject> queue in _pools.Values)
        {
            while (queue.Count > 0)
            {
                GameObject instance = queue.Dequeue();

                if (instance != null)
                {
                    Destroy(instance);
                }
            }
        }

        _pools.Clear();
    }
}
