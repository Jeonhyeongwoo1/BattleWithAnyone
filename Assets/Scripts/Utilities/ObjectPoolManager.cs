using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Photon.Pun;
using Photon.Realtime;

public class ObjectPoolHelper : MonoBehaviourPunCallbacks
{
    private static Transform poolParent;

    public static GameObject CreatePrefab(GameObject prefab, Transform parent)
    {
        GameObject go = Instantiate(prefab, parent, false);
        return go;
    }

    public static IEnumerator PoolCleaner(GameObject go, int count, float interval, UnityAction done = null)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(interval);
        for (int i = 0; i < count; i++)
        {
            if (go != null)
            {
                Destroy(go);
            }

            yield return waitForSeconds;
        }
        done?.Invoke();
    }

    public static void PoolClean(GameObject go)
    {
        Destroy(go);
    }
}

public class ObjectPool
{
    [Serializable]
    public class PoolData
    {
        public int initialCount;
        public int maxCount;
        public Transform parent;
        public GameObject prefab;
    }

    //float m_PoolingCleanInterval = 0.1f;
    int m_CreateCount = 5; //오브젝트가 없을 경우에 새롭게 생성하는 오브젝트 수
    PoolData m_PoolData = new PoolData();
    Queue<GameObject> m_ObjectPool = new Queue<GameObject>();

    public ObjectPool(int initialCount, int maxCount, Transform parent, GameObject prefab = null)
    {
        m_PoolData.initialCount = initialCount;
        m_PoolData.maxCount = maxCount;
        m_PoolData.parent = parent;
        m_PoolData.prefab = prefab;
    }

	public PoolData Get() => m_PoolData;
    public void Clear() => m_ObjectPool.Clear();
    public int Count() => m_ObjectPool.Count;
	public void Enqueue(GameObject go) => m_ObjectPool.Enqueue(go);

    public GameObject Create()
    {
        GameObject go = ObjectPoolHelper.CreatePrefab(m_PoolData.prefab, m_PoolData.parent);
        go.SetActive(false);
        m_ObjectPool.Enqueue(go);
        return go;
    }

    public void Initialize()
    {
        int count = m_PoolData.initialCount;
        for (int i = 0; i < count; i++)
        {
            Create();
        }
    }

    public GameObject Spawn()
    {
        if (m_ObjectPool.Count == 0)
        {
            for (int i = 0; i < m_CreateCount; i++)
            {
                Create();
            }
        }

        return m_ObjectPool.Dequeue();
    }

    public void Despawn(GameObject go)
    {
        
        if (m_PoolData.maxCount < m_ObjectPool.Count)
        {
            ObjectPoolHelper.PoolClean(go);
            return;
        }

        if (go.activeSelf) { go.SetActive(false); }
        m_ObjectPool.Enqueue(go);
    }
}

public class ObjectPoolManager : MonoBehaviourPunCallbacks
{
    Dictionary<string, ObjectPool> m_ObjectPoolManager = new Dictionary<string, ObjectPool>();

    public void Add(string key, ObjectPool value)
    {
        ObjectPool pool = null;
        if (m_ObjectPoolManager.TryGetValue(key, out pool))
        {
            Debug.LogError(string.Format("{0} already Added", key));
            return;
        }

        m_ObjectPoolManager.Add(key, value);
    }

    public void Remove(string key)
    {
        ObjectPool pool = null;
        if (!m_ObjectPoolManager.TryGetValue(key, out pool))
        {
            Debug.LogWarning(string.Format("There isn't {0}", key));
            return;
        }

        m_ObjectPoolManager.Remove(key);
    }

    public bool Has(string key) => m_ObjectPoolManager.ContainsKey(key);
    public int Count() => m_ObjectPoolManager.Count;

    public int ObjectPoolCount(string key)
    {
        if (!m_ObjectPoolManager.ContainsKey(key))
        {
            Debug.LogError(string.Format("There isn't {0}", key));
            return 0;
        }

        return m_ObjectPoolManager[key].Count();
    }

    public void ClearPooledObject(string key)
    {
        if (!m_ObjectPoolManager.ContainsKey(key))
        {
            Debug.LogError(string.Format("There isn't {0}", key));
            return;
        }

        ObjectPool pool = m_ObjectPoolManager[key];
        pool.Clear();
    }

    public void Initialize(string key, string photonNetworkInstantiatePath = null)
    {
        if (!m_ObjectPoolManager.ContainsKey(key))
        {
            Debug.LogError(string.Format("There isn't {0}", key));
            return;
        }

        ObjectPool pool = m_ObjectPoolManager[key];

        if (string.IsNullOrEmpty(photonNetworkInstantiatePath))
        {
            pool.Initialize();
        }
        else
        {
            CreatePhotonPoolObj(pool, photonNetworkInstantiatePath);
        }
    }

    public GameObject Spawn(string key)
    {
        if (!m_ObjectPoolManager.ContainsKey(key))
        {
            Debug.LogError(string.Format("There isn't {0}", key));
            return null;
        }

        ObjectPool pool = m_ObjectPoolManager[key];
        return pool.Spawn();
    }

    public void Despawn(string key, GameObject go)
    {
        if (!m_ObjectPoolManager.ContainsKey(key))
        {
            Debug.LogError(string.Format("There isn't {0}", key));
            return;
        }

        ObjectPool pool = m_ObjectPoolManager[key];
        pool.Despawn(go);
    }

	public ObjectPool GetPool(string key)
	{
        if (!m_ObjectPoolManager.ContainsKey(key))
        {
            Debug.LogError(string.Format("There isn't {0}", key));
            return null;
        }

        ObjectPool pool = m_ObjectPoolManager[key];
        return pool;
	}

    void CreatePhotonPoolObj(ObjectPool pool, string path)
    {
		ObjectPool.PoolData data = pool.Get();
        int count = data.initialCount;
        for (int i = 0; i < count; i++)
        {
			GameObject go = PhotonNetwork.Instantiate(path, Vector3.zero, Quaternion.identity, 0);
			pool.Enqueue(go);
        }
    }
}