using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileController : MonoBehaviour {

	public Explosion prefabExplosion;
	
	public float speed;
	public float minDamage;
	public float maxDamage;
	public float maxRange;

	// Cached components
	private new Collider collider;
	private new Rigidbody rigidbody;

	private PhotonPlayer owner;
	private Vector3 startPosition;
	private Vector3 lastPosition;
	private float totalDistance;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		collider = GetComponent<Collider>();
		rigidbody = GetComponent<Rigidbody>();
	}

	void Start() {
		startPosition = lastPosition = transform.position;
		rigidbody.AddForce(transform.forward * speed, ForceMode.VelocityChange);
		StartCoroutine(DestroyOnMaxRange());
	}

	void OnCollisionEnter(Collision collision) {
		HitArea targetHitArea = collision.gameObject.GetComponent<HitArea>();
		if (targetHitArea != null) {
			targetHitArea.Hit(CalculateDamageBasedOnDistance(totalDistance), owner);
		}

		ExplodeAndDestroy();
	}

	public void SetOwner(PhotonPlayer owner, List<Collider> ownerColliders) {
		this.owner = owner;
		foreach (Collider ownerCollider in ownerColliders) {
			Physics.IgnoreCollision(collider, ownerCollider);
		}
	}

	public float CalculateDamageBasedOnDistance(float distanceTraveled) {
		return Mathf.Lerp(maxDamage, minDamage, distanceTraveled / maxRange);
	}

	private IEnumerator DestroyOnMaxRange() {
		while (true) {
			totalDistance += Vector3.Magnitude(transform.position - lastPosition);
			lastPosition = transform.position;

			if (totalDistance > maxRange) {
				ExplodeAndDestroy();
			}
			yield return null;
		}
	}

	private void ExplodeAndDestroy() {
		Explode();
		Destroy(gameObject);
	}

	private void Explode() {
		if (prefabExplosion == null) {
			return;
		}

		Explosion explosion = Instantiate(prefabExplosion, transform.position, transform.rotation);
		explosion.Owner = owner;
	}

}
