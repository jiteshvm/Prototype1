using UnityEngine;
using System.Collections.Generic;


public class PoolManager : Singleton<PoolManager>
{
    protected PoolManager() { }
	
    private Dictionary<string, PoolData> PoolDictionary;
    public int DefaultPoolSize = 1;

    public void Awake()
    {
        PoolDictionary = new Dictionary<string, PoolData>();
    }
    public PoolData CreatePool(GameObject poolObj, int poolSize = 1, bool isInfinite = true, int maxPoolSize = 0)
	{

		return CreatePool(poolObj.name, poolObj, poolSize, isInfinite, maxPoolSize);
	}

	public PoolData CreatePool(string poolName, GameObject poolObj, int poolSize = 1, bool isInfinite = true, int maxPoolSize = 0)
	{
		PoolData result = null;
		if (!TryGetPool(poolName, out result))
		{
			result = new PoolData(poolObj, poolSize, isInfinite, maxPoolSize);
			FillPool(result, poolSize);
            PoolDictionary.Add(poolName, result);
			//Debug.Log("[PoolManager] : Pool " + poolName + " created successfully");
		}
		return result;

	}

	// Fills the pool with the given number of objects
	void FillPool(PoolData pool, int number)
	{
		for (int i = 0; i < number; ++i)
		{
			GameObject gobj = (GameObject)Instantiate(pool.prefab);
			gobj.SetActive(false);
			gobj.name = pool.prefab.name;
            pool.recycledObjects.Enqueue(gobj);
		}
	}

	public void DestroyPool(GameObject poolObj)
	{

		DestroyPool(poolObj.name);

	}

	public void DestroyPool(string poolName)
	{
		PoolData pool = null;
		if (TryGetPool(poolName, out pool))
		{
			foreach (GameObject g in pool.recycledObjects)
				Destroy(g);
            foreach (GameObject g in pool.spawnedObjects)
				Destroy(g);

			pool.recycledObjects.Clear();
            pool.spawnedObjects.Clear();
			PoolDictionary.Remove(poolName);
            Debug.Log("[PoolManager] : Pool " + poolName + " destroyed successfully");
		}

	}

	public void DestroyAllPools()
	{
		foreach (KeyValuePair<string, PoolData> kv in PoolDictionary)
		{
            foreach (GameObject g in kv.Value.recycledObjects)
				Destroy(g);
            foreach (GameObject g in kv.Value.spawnedObjects)
				Destroy(g);

            kv.Value.recycledObjects.Clear();
            kv.Value.spawnedObjects.Clear();
		}

		PoolDictionary.Clear();
		//Debug.Log ("[PoolManager] : All Pools destroyed");
	}

	private GameObject SpawnFromPool(PoolData pool)
	{

		GameObject spawnedObj;
        if (pool.recycledObjects.Count > 0)
            spawnedObj = pool.recycledObjects.Dequeue();
		else
		{
			FillPool(pool, 1);
            spawnedObj = pool.recycledObjects.Dequeue();
		}
		// Add it to the list of spawnedObjects
        pool.spawnedObjects.Add(spawnedObj);

		return spawnedObj;
	}

	public GameObject Spawn(GameObject poolObj)
	{
		GameObject spawnedObj;

		// if pool doesnt exist , create a pool with the default size
		PoolData pool = null;
		if (!TryGetPool(poolObj.name, out pool))
		{
            pool = CreatePool(poolObj, DefaultPoolSize);
		}

		spawnedObj = SpawnFromPool(pool);
		spawnedObj.SetActive(true);

		return spawnedObj;

	}

	public GameObject Spawn(GameObject poolObj, Vector3 position, Quaternion rotation)
	{
		GameObject spawnedObj;

		// if pool doesnt exist , create a pool with the default size
		PoolData pool = null;
		if (!TryGetPool(poolObj.name, out pool))
		{
			pool = CreatePool(poolObj, DefaultPoolSize);
		}

		spawnedObj = SpawnFromPool(pool);
		if (spawnedObj == null)
			return null;
		spawnedObj.transform.position = position;
		spawnedObj.transform.rotation = rotation;
		spawnedObj.SetActive(true);

		return spawnedObj;

	}


	public GameObject Spawn(string poolName)
	{

		GameObject spawnedObj;

		// if pool doesnt exist , create a pool with the default size
		PoolData pool = null;
		if (TryGetPool(poolName, out pool))
		{

			spawnedObj = SpawnFromPool(pool);
			spawnedObj.SetActive(true);

			return spawnedObj;
		}
		else
		{
            Debug.LogError("Unable to spawn from " + poolName + " because it doesnt exist ");
			return null;
		}
	}

	public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
	{
		GameObject spawnedObj;

		// if pool doesnt exist , create a pool with the default size
		PoolData pool = null;
		if (TryGetPool(poolName, out pool))
		{

			spawnedObj = SpawnFromPool(pool);
			spawnedObj.transform.position = position;
			spawnedObj.transform.rotation = rotation;
			spawnedObj.SetActive(true);

			return spawnedObj;
		}
		else
		{
            Debug.LogError("[PoolManager] : Unable to spawn from " + poolName + " because it doesnt exist ");
			return null;
		}
	}

	// Despawns the object and puts it back in the recycle queue
	public void Despawn(GameObject poolObj)
	{
		PoolData pool = null;
		if (TryGetPool(poolObj.name, out pool))
		{
			poolObj.SetActive(false);
            if (pool.spawnedObjects.Remove(poolObj))
			{
                if (pool.isInfinite || pool.recycledObjects.Count < pool.maxPoolSize)
				{
                    pool.recycledObjects.Enqueue(poolObj);
				}
				else
				{
					Destroy(poolObj);
				}
				//Debug.Log("[PoolManager] : " + poolObj.name + " despawned");
			}
		}
		else
		{
			Destroy(poolObj);
		}
	}

	//public static void DespawnAfterSeconds(GameObject poolObj, )
	// despawns all the objects that are spawned and puts it back in the recycle queue
	public void DespawnAll(GameObject poolObj)
	{
		PoolData pool = null;
		if (TryGetPool(poolObj.name, out pool))
		{
            foreach (GameObject g in pool.spawnedObjects)
			{
				g.SetActive(false);
                if (pool.isInfinite || pool.recycledObjects.Count < pool.maxPoolSize)
				{
                    pool.recycledObjects.Enqueue(poolObj);
				}
				else
				{
					Destroy(poolObj);
				}
				//Debug.Log("[PoolManager] : " + g.name + " despawned");
			}
            pool.spawnedObjects.Clear();
		}
		else
		{
			Destroy(poolObj);
		}


	}

	public bool ContainsPool(string poolName)
	{
		if (PoolDictionary.ContainsKey(poolName))
			return true;
		else
			return false;
	}

	public bool TryGetPool(string poolName, out PoolData pool)
	{
		return PoolDictionary.TryGetValue(poolName, out pool);
	}
}
