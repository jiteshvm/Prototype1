using UnityEngine;
using System.Collections.Generic;

public class PoolData
{
	// name of the pool
	public string name;

	// the prefab that this pool will spawn
	public GameObject prefab;

	public bool isInfinite;

	public int maxPoolSize;

	// a queue to hold all the recycled objects
	public Queue<GameObject> recycledObjects;

	// a list to hold all the spawned objects
	public List<GameObject> spawnedObjects;


	public PoolData(GameObject prefab) :
		this(prefab, 1, true, 0)
	{ }

	// creates a pool of the specified size
	public PoolData(GameObject prefab, int initPoolSize) : 
        this(prefab, initPoolSize, true, 0)
	{ }

	public PoolData(GameObject prefab, bool isInfinite, int maxPoolSize) : 
        this(prefab, 1, isInfinite, maxPoolSize)
	{ }


	public PoolData(GameObject prefab, int initPoolSize, bool isInfinite, int maxPoolSize)
	{
		this.prefab = prefab;
		this.isInfinite = isInfinite;
        this.maxPoolSize = maxPoolSize;
        recycledObjects = new Queue<GameObject>(initPoolSize);
		spawnedObjects = new List<GameObject>();
	}
}
