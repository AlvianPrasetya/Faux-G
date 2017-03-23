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

	void Start() {
		StartCoroutine(WaitForExplosion());
	}

	private IEnumerator WaitForExplosion() {
		yield return new WaitForSeconds(delay);

		ApplyExplosionForce();
	}

	private void ApplyExplosionForce() {
		Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
		foreach (Collider collider in colliders) {
			if (collider.isTrigger) {
				continue;
			}

			float distance = Vector3.Magnitude(collider.transform.position - transform.position);

			RaycastHit hitInfo;
			Physics.Raycast(
				transform.position, 
				collider.transform.position - transform.position, 
				out hitInfo, 
				distance
			);

			float explosionForce;
			float explosionDamage;
			if (hitInfo.collider == collider) { // Direct hit
				explosionForce = directForce;
				explosionDamage = directDamage;
			} else { // Indirect hit
				explosionForce = indirectForce;
				explosionDamage = indirectDamage;
			}

			Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
			if (rigidbody != null) {
				rigidbody.AddExplosionForce(
					explosionForce,
					transform.position,
					radius,
					upwardsBias,
					ForceMode.Impulse
				);
			}

			Health health = collider.GetComponent<Health>();
			if (health != null) {
				health.Damage(explosionDamage / distance);
			}
		}
	}

}
