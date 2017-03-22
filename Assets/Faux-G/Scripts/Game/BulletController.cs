using System.Collections;
using UnityEngine;

public class BulletController : MonoBehaviour {

	public GameObject prefabExplosion;
	
	public float acceleration;

	private new Rigidbody rigidbody;

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
	}

	void Start() {
		rigidbody.AddForce(transform.forward * rigidbody.mass * acceleration, ForceMode.Impulse);
	}

	void OnDestroy() {
		Instantiate(prefabExplosion, transform.position, transform.rotation);
	}

	void OnTriggerEnter(Collider other) {
		Destroy(gameObject);
	}

}
