using UnityEngine;
using System.Collections.Generic;

/**
 * This class manages the lifecycle of object pools and acts as a "facade" 
 * to those object pools.
 */
public class ObjectPoolManager : MonoBehaviour {

	public ObjectPool objectPoolPrefab;

	private Dictionary<PoolableBase, ObjectPool> poolDictionary;

	void Awake() {
		poolDictionary = new Dictionary<PoolableBase, ObjectPool>();
	}

	/**
	 * This method gets the object pool corresponding to a poolable prefab, 
	 * and creates one when the object pool does not exist.
	 */
	public ObjectPool GetObjectPool(PoolableBase poolablePrefab) {
		ObjectPool objectPool;
		if (!poolDictionary.TryGetValue(poolablePrefab, out objectPool)) {
			// Create a new ObjectPool if one does not exist
			objectPool = Instantiate(objectPoolPrefab, Vector3.zero, Quaternion.identity);
			objectPool.poolablePrefab = poolablePrefab;

			// Add new object pool to pool dictionary
			poolDictionary.Add(poolablePrefab, objectPool);
		}

		return objectPool;
	}

}
