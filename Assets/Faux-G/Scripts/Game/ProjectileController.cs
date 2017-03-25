using UnityEngine;

public class ProjectileController : MonoBehaviour {

	public GameObject prefabExplosion;
	
	public float acceleration;
	public float damage;

	private new Rigidbody rigidbody;
	private DestroyAfterTime destroyAfterTime;

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
		destroyAfterTime = GetComponent<DestroyAfterTime>();
		destroyAfterTime.SetPreDestroyCallback(Explode);
	}

	void Start() {
		rigidbody.AddForce(transform.forward * rigidbody.mass * acceleration, ForceMode.Impulse);
	}

	void OnTriggerEnter(Collider other) {
		Explode();
		ApplyDamage(other);
		Destroy(gameObject);
	}

	void Explode() {
		if (prefabExplosion == null) {
			return;
		}

		Instantiate(prefabExplosion, transform.position, transform.rotation);
	}

	void ApplyDamage(Collider target) {
		if (damage == 0.0f) {
			return;
		}

		Health healthComponent = target.GetComponent<Health>();
		if (healthComponent != null) {
			healthComponent.Damage(damage);
		}
	}

}
