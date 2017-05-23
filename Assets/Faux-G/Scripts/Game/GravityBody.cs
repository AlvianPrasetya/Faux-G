using UnityEngine;
using System.Collections.Generic;

public class GravityBody : Photon.MonoBehaviour {

	public float gravityChangeAngleThreshold;
	public float minRotateSpeed;
	public float maxRotateSpeed;

	private new Rigidbody rigidbody;
	private List<Collider> terrainColliders;
	
	private Vector3 lastGravityDirection;
	private Vector3 gravityDirection;
	private float referenceAngle;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
		terrainColliders = new List<Collider>();
		foreach (GameObject terrain in GameObject.FindGameObjectsWithTag(Utils.Tag.TERRAIN)) {
			terrainColliders.Add(terrain.GetComponent<Collider>());
		}

		lastGravityDirection = Vector3.zero;
		gravityDirection = Vector3.zero;
	}

	void FixedUpdate() {
		// Calculate gravity and apply to rigidbody
		UpdateGravityDirection();
		rigidbody.AddForce(gravityDirection * Utils.GRAVITY, ForceMode.Acceleration);

		// Adjust player rotation due to gravity
		AdjustRotation();
	}

	private void UpdateGravityDirection() {
		lastGravityDirection = gravityDirection;

		Vector3 closestTerrainPoint = transform.position;
		float closestTerrainPointSqrDist = Mathf.Infinity;
		foreach (Collider terrainCollider in terrainColliders) {
			Vector3 terrainPoint = terrainCollider.ClosestPoint(transform.position);
			if (Vector3.SqrMagnitude(terrainPoint - transform.position) < closestTerrainPointSqrDist) {
				closestTerrainPoint = terrainPoint;
				closestTerrainPointSqrDist = Vector3.SqrMagnitude(terrainPoint - transform.position);
			}
		}

		gravityDirection = Vector3.Normalize(closestTerrainPoint - transform.position);
	}

	private void AdjustRotation() {
		float gravityChangeAngle = Vector3.Angle(lastGravityDirection, gravityDirection);
		if (gravityChangeAngle > gravityChangeAngleThreshold) {
			referenceAngle = gravityChangeAngle;
		}

		Quaternion rotationDifference = Quaternion.FromToRotation(-transform.up, gravityDirection);
		Quaternion targetRotation = rotationDifference * transform.rotation;

		float angleDifference = Vector3.Angle(-transform.up, gravityDirection);
		// Calculate rotate speed based on a parabolic (dome-shaped) function of angle difference
		float rotateSpeed = Mathf.Lerp(
			minRotateSpeed,
			maxRotateSpeed,
			1 - 4 * Mathf.Pow(angleDifference - referenceAngle / 2, 2) / Mathf.Pow(referenceAngle, 2)
		);
		
		transform.rotation = Quaternion.RotateTowards(
			transform.rotation,
			targetRotation,
			rotateSpeed * Time.fixedDeltaTime
		);
	}

}
