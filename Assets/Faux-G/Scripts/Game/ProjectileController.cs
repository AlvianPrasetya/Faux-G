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

	void OnCollisionEnter(Collision collision) {
		PhotonView otherPhotonView = collision.gameObject.GetComponentInParent<PhotonView>();
		if (otherPhotonView != null && otherPhotonView.owner == owner) {
			// Ignore collision with owner
			return;
		}

		Explode();

		HitArea targetHitArea = collision.gameObject.GetComponent<HitArea>();
		if (targetHitArea != null) {
			targetHitArea.Hit(damage, owner);
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
