using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * This class controls the behaviour of a Tracer (trace spawner) that indicates 
 * the trajectory of the current Throwable based on the charged throw force.
 */
public class Tracer : ThrowableBase {

	public GameObject tracePrefab;

	public float traceSpawnInterval;

	private long lastTraceSpawnTimeMs;
	private bool spawnTrace;
	private Queue<GameObject> traces;

	public override void Initialize() {
		Awake();
	}

	public override void CleanUp() {
		traces = null;
	}

	protected override void Awake() {
		base.Awake();

		lastTraceSpawnTimeMs = PhotonNetwork.ServerTimestamp;
		spawnTrace = false;
		traces = new Queue<GameObject>();
	}
	
	void FixedUpdate() {
		if (spawnTrace) {
			SpawnTraceOnSpawnInterval();
		}
	}

	public override void Release(Vector3 throwPosition, Quaternion throwRotation,
		Vector3 throwDirection, float throwForce) {
		base.Release(throwPosition, throwRotation, throwDirection, throwForce);

		transform.parent = null;
		transform.position = throwPosition;
		transform.rotation = throwRotation;

		rigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);

		spawnTrace = true;
	}

	protected override void OnCollisionEnter(Collision collision) {
		// TODO: Disable physics upon impact
	}

	public void Despawn() {
		spawnTrace = false;
		StartCoroutine(DespawnCoroutine());
	}

	private void SpawnTraceOnSpawnInterval() {
		long currentTimeMs = PhotonNetwork.ServerTimestamp;
		if (currentTimeMs - lastTraceSpawnTimeMs >= traceSpawnInterval * 1000) {
			traces.Enqueue(Instantiate(tracePrefab, transform.position, transform.rotation));
			lastTraceSpawnTimeMs = currentTimeMs;
		}
	}

	private IEnumerator DespawnCoroutine() {
		while (traces.Count != 0) {
			Destroy(traces.Dequeue());
			yield return new WaitForSeconds(traceSpawnInterval);
		}

		Pool();
	}

}
