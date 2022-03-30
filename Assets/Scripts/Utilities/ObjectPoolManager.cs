using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PoolData
{
	public int initialCount;
	public int maxCount;
	public Transform parent;
	public GameObject prefab;
}

public class ObjectPool
{
	int m_CreateCount = 5; //오브젝트가 없을 경우에 새롭게 생성하는 오브젝트 수
	PoolData m_PoolData;
	Queue<GameObject> m_ObjectPool = new Queue<GameObject>();

	public ObjectPool(int initialCount, int maxCount, Transform parent, GameObject prefab)
	{
		m_PoolData.initialCount = initialCount;
		m_PoolData.maxCount = maxCount;
		m_PoolData.parent = parent;
		m_PoolData.prefab = prefab;
	}

	public void Clear() => m_ObjectPool.Clear();
	public int Count() => m_ObjectPool.Count;

	public GameObject Create()
	{
		GameObject go = new GameObject(nameof(m_PoolData.prefab));
		go.transform.SetParent(m_PoolData.parent);
		go.SetActive(false);
		m_ObjectPool.Enqueue(go);
		return go;
	}

	public void Initialize()
	{
		int count = m_PoolData.initialCount;
		for (int i = 0; i < count; i++)
		{
			GameObject go = new GameObject(nameof(m_PoolData.prefab));
			go.transform.SetParent(m_PoolData.parent);
			m_ObjectPool.Enqueue(go);
		}
	}

	public GameObject Spawn()
	{
		if (m_ObjectPool.Count == 0)
		{
			for (int i = 0; i < m_CreateCount; i++)
			{
				GameObject go = new GameObject(nameof(m_PoolData.prefab));
				go.transform.SetParent(m_PoolData.parent);
				m_ObjectPool.Enqueue(go);
			}
		}

		return m_ObjectPool.Dequeue();
	}

	public void Despawn(GameObject go)
	{
		if (go.activeSelf) { go.SetActive(false); }
		m_ObjectPool.Enqueue(go);
	}
}

public class ObjectPoolManager : Singleton<ObjectPoolManager>
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

	public void Remove(string key, ObjectPool value)
	{
		ObjectPool pool = null;
		if (!m_ObjectPoolManager.TryGetValue(key, out pool))
		{
			Debug.LogError(string.Format("There isn't {0}", key));
			return;
		}

		m_ObjectPoolManager.Remove(key);
	}

	public bool Has(string key) => m_ObjectPoolManager.ContainsKey(key);

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

	public void Initialize(string key)
	{
		if (!m_ObjectPoolManager.ContainsKey(key))
		{
			Debug.LogError(string.Format("There isn't {0}", key));
			return;
		}

		ObjectPool pool = m_ObjectPoolManager[key];
		pool.Initialize();
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
}
