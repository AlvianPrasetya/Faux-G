using UnityEngine;

public abstract class PoolableBase : MonoBehaviour {

	private ObjectPool sourcePool;

	public ObjectPool SourcePool {
		set {
			sourcePool = value;
		}
	}

	/**
	 * This method pools this poolable entity into an ObjectPool and does all the 
	 * necessary cleanups before deactivating the entity.
	 */
	public virtual void Pool() {
		sourcePool.Pool(this);
	}

	/**
	 * This method initializes this poolable entity after taken out of the pool.
	 * Classes implementing this abstract class must override this method to 
	 * initialize this entity after being unpooled.
	 */
	public abstract void Initialize();

	/**
	 * This method cleans up this poolable entity before pooling it back to the pool.
	 * Classes implementing this abstract class must override this method to do
	 * cleanups before this entity is pooled.
	 */
	public abstract void CleanUp();

}
