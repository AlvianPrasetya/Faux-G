using UnityEngine;
using System.Collections;

public class DummyThrowable : ThrowableBase {

	public float timeBeforeDespawn;

	private Behaviour halo;

	// Stale dummy throwable could not interact with hit areas
	private bool stale;

	public override void Initialize() {
		Awake();
	}

	public override void CleanUp() {
	}

	protected override void Awake() {
		base.Awake();

		halo = (Behaviour) GetComponent("Halo");
		halo.enabled = false;
	}

	public override void Release(Vector3 throwPosition, Quaternion throwRotation, 
		Vector3 throwDirection, float throwForce) {
		base.Release(throwPosition, throwRotation, throwDirection, throwForce);

		transform.parent = null;
		transform.position = throwPosition;
		transform.rotation = throwRotation;

		// Enable halo upon release
		stale = false;
		halo.enabled = true;

		rigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);

		StartCoroutine(DespawnCoroutine());
	}

	protected override void OnCollisionEnter(Collision collision) {
		GameObject collidingGameObject = collision.gameObject;

		HitArea collidingHitArea = collidingGameObject.GetComponent<HitArea>();
		if (collidingHitArea != null && !stale) {
			// Disable halo upon collision with hit area
			stale = true;
			halo.enabled = false;

			if (Owner.IsLocal) {
				// Only evaluate collisions with hit area locally (authoritative observer rule)
				collidingHitArea.OnThrowableCollision(this, collision);
			}
		}
	}

	private IEnumerator DespawnCoroutine() {
		float timeBeforeDespawn = this.timeBeforeDespawn;
		while (timeBeforeDespawn >= 0.0f) {
			timeBeforeDespawn -= Time.deltaTime;
			yield return null;
		}

		Pool();
	}

}
