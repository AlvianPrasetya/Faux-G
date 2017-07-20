using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {

	public PoolableBase poolablePrefab;

	private Queue<PoolableBase> poolables;

	private void Awake() {
		poolables = new Queue<PoolableBase>();
	}

	/**
	 * This method cleans up and pools the specified poolable back into the pool queue.
	 * Returns true upon successful pooling, false otherwise.
	 */
	public bool Pool(PoolableBase poolable) {
		if (poolable.GetType().Equals(poolablePrefab.GetType())) {
			// Clean up before pooling
			poolable.CleanUp();

			// Deactivate
			poolable.gameObject.SetActive(false);

			poolables.Enqueue(poolable);
			return true;
		}

		return false;
	}

	/**
	 * This method unpools a poolable from the pool queue and initializes it if it is
	 * not empty, otherwise it will instantiate a new poolable object and returns it.
	 */
	public PoolableBase Unpool(Vector3 position, Quaternion rotation, Transform parent = null) {
		PoolableBase poolable;
		if (poolables.Count != 0) {
			poolable = poolables.Dequeue();

			// Activate
			poolable.gameObject.SetActive(true);

			// Set position and rotation
			poolable.transform.position = position;
			poolable.transform.rotation = rotation;
			poolable.transform.parent = parent;
		} else {
			poolable = Instantiate(poolablePrefab, position, rotation, parent);
		}

		// Initialize after unpooling
		poolable.Initialize();

		// Set source pool to this ObjectPool
		poolable.SourcePool = this;

		return poolable;
	}

}
