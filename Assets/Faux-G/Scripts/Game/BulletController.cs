using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {

	public float lifetime;
	public float acceleration;

	private Coroutine waitForLifetime;

	private new Rigidbody rigidbody;

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
	}

	void Start() {
		rigidbody.AddForce(transform.forward * rigidbody.mass * acceleration, ForceMode.Impulse);
		waitForLifetime = StartCoroutine(WaitForLifetime());
	}

	void OnCollisionEnter(Collision collision) {
		StopCoroutine(waitForLifetime);
		Destroy(gameObject);
	}

	private IEnumerator WaitForLifetime() {
		yield return new WaitForSeconds(lifetime);
		Destroy(gameObject);
	}

}
