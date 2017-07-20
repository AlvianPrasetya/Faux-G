using UnityEngine;

/**
 * This abstract class describes the mandatory properties and behavioural skeletons of a throwable 
 * object. New throwables are to implement this abstract class accordingly.
 */
public abstract class ThrowableBase : PoolableBase {

	// The points/damage value when this throwable collides with something
	public int onHitValue;

	protected new Collider collider;
	protected new Rigidbody rigidbody;
	protected GravityBody gravityBody;

	// The owner of this throwable object (the player spawning this object).
	private PhotonPlayer owner;

	protected virtual void Awake() {
		collider = GetComponent<Collider>();
		rigidbody = GetComponent<Rigidbody>();
		gravityBody = GetComponent<GravityBody>();

		// Disable physics before release
		collider.enabled = false;
		rigidbody.isKinematic = true;
		gravityBody.enabled = false;
	}

	/**
	 * This virtual method describes the "releasing" behaviour of this throwable (the moment 
	 * when the throwable is released from the arms of the owner towards the targeted direction).
	 */
	public virtual void Release(Vector3 throwPosition, Quaternion throwRotation,
		Vector3 throwDirection, float throwForce) {
		// Enable physics upon release
		collider.enabled = true;
		rigidbody.isKinematic = false;
		gravityBody.enabled = true;
	}

	/**
	 * This abstract method extends Unity's OnCollisionEnter that describes the behaviour of this 
	 * throwable when colliding with another object. Explosion/other behaviours are to be 
	 * described within this method.
	 */
	protected abstract void OnCollisionEnter(Collision collision);

	public PhotonPlayer Owner {
		get {
			return owner;
		}

		set {
			owner = value;
		}
	}

	public Collider Collider {
		get {
			return collider;
		}
	}

	public Rigidbody Rigidbody {
		get {
			return rigidbody;
		}
	}

}