using System.Collections;
using UnityEngine;

public class BulletController : MonoBehaviour {

	public GameObject prefabExplosion;
	
	public float acceleration;

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
		if (other.isTrigger) {
			return;
		}

		Explode();
		Destroy(gameObject);
	}

	void Explode() {
		Instantiate(prefabExplosion, transform.position, transform.rotation);
	}

}
