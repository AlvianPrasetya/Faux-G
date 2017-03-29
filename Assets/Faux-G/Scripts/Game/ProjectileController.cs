using UnityEngine;

public class ProjectileController : MonoBehaviour {

	public Explosion prefabExplosion;
	
	public float speed;
	public float damage;

	// Cached components
	private new Rigidbody rigidbody;
	private DestroyAfterTime destroyAfterTime;

	private PhotonPlayer owner;

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
		destroyAfterTime = GetComponent<DestroyAfterTime>();
		destroyAfterTime.SetPreDestroyCallback(Explode);
	}

	void Start() {
		rigidbody.AddForce(transform.forward * speed, ForceMode.VelocityChange);
	}

	void OnTriggerEnter(Collider other) {
		PhotonView otherPhotonView = other.gameObject.GetPhotonView();
		if (otherPhotonView != null && otherPhotonView.owner == owner) {
			// Ignore collision with owner
			return;
		}

		Explode();

		Health healthComponent = other.GetComponentInParent<Health>();
		if (healthComponent != null) {
			healthComponent.Damage(damage, owner);
		}

		Destroy(gameObject);
	}

	public PhotonPlayer Owner {
		get {
			return owner;
		}

		set {
			owner = value;
		}
	}

	public void Explode() {
		if (prefabExplosion == null) {
			return;
		}

		Explosion explosion = Instantiate(prefabExplosion, transform.position, transform.rotation);
		explosion.Owner = owner;
	}

}
