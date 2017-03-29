using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {

	public float delay;
	public float directForce;
	public float directDamage;
	public float indirectForce;
	public float indirectDamage;
	public float radius;
	public float upwardsBias;

	private PhotonPlayer owner;

	void Start() {
		StartCoroutine(WaitForExplosion());
	}

	public PhotonPlayer Owner {
		get {
			return owner;
		}

		set {
			owner = value;
		}
	}

	private IEnumerator WaitForExplosion() {
		yield return new WaitForSeconds(delay);

		Explode();
	}

	private void Explode() {
		foreach (Collider collider in GetCollidersInRange()) {
			float distance = Vector3.Magnitude(collider.transform.position - transform.position);
			bool isDirectHit = IsDirectHit(collider.transform, distance);

			float explosionForce = (isDirectHit) ? directForce : indirectForce;
			float explosionDamage = (isDirectHit) ? directDamage : indirectDamage;

			Rigidbody targetRigidbody = collider.GetComponent<Rigidbody>();
			if (targetRigidbody != null && !targetRigidbody.isKinematic) {
				ApplyExplosionForce(targetRigidbody, explosionForce);
			}

			HitArea targetHitArea = collider.GetComponent<HitArea>();
			if (targetHitArea != null) {
				ApplyExplosionDamage(targetHitArea, explosionDamage, distance);
			}
		}
	}

	private Collider[] GetCollidersInRange() {
		return Physics.OverlapSphere(
			transform.position,
			radius,
			~(Utils.Layer.TERRAIN | Utils.Layer.PROJECTILE)
		);
	}

	private bool IsDirectHit(Transform targetTransform, float distance) {
		RaycastHit hitInfo;
		Physics.Raycast(
			transform.position,
			targetTransform.position - transform.position,
			out hitInfo,
			distance,
			~(Utils.Layer.IGNORE_PROJECTILE | Utils.Layer.PROJECTILE)
		);

		if (hitInfo.transform == targetTransform) {
			return true;
		}

		return false;
	}

	private void ApplyExplosionForce(Rigidbody targetRigidbody, float explosionForce) {
		targetRigidbody.AddExplosionForce(
			explosionForce,
			transform.position,
			radius,
			upwardsBias,
			ForceMode.Impulse
		);
	}

	private void ApplyExplosionDamage(HitArea targetHitArea, float explosionDamage, float distance) {
		// Damage falls off linearly with distance
		targetHitArea.Hit((1.0f - distance / radius) * explosionDamage, owner);
	}

}
