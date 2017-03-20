using UnityEngine;
using System.Diagnostics;

public class PlayerController : Photon.MonoBehaviour {

	public float rotateSpeed;

	private Vector3 gravityAcceleration;

	// Cached components
	private new Rigidbody rigidbody;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
	}

	void Update() {
		if (!photonView.isMine) {
			return;
		}

		// Calculate gravity and apply to player
		gravityAcceleration = CalculateGravity();
		rigidbody.AddForce(rigidbody.mass * gravityAcceleration, ForceMode.Force);

		// Adjust player rotation due to gravity
		AdjustRotation();

		InputWalk();
		InputJump();
		InputShoot();
	}

	private Vector3 CalculateGravity() {
		float lowerBoundRadius = 0.0f;
		float upperBoundRadius = 100.0f;
		
		while (upperBoundRadius - lowerBoundRadius > 1e-6) {
			float castRadius = (lowerBoundRadius + upperBoundRadius) / 2.0f;
			if (Physics.CheckSphere(transform.position, castRadius, Utils.Layer.TERRAIN)) {
				upperBoundRadius = castRadius;
			} else {
				lowerBoundRadius = castRadius;
			}
		}

		RaycastHit hitInfo;
		Vector3 closestPoint = transform.position;
		float minSqrDist = Mathf.Infinity;
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				for (int z = -1; z <= 1; z++) {
					Vector3 dir = new Vector3(x, y, z);

					if (Physics.SphereCast(transform.position, lowerBoundRadius, dir, 
						out hitInfo, 1e-3f, Utils.Layer.TERRAIN)) {
						float dist = Vector3.SqrMagnitude(hitInfo.point - transform.position);
						if (dist < minSqrDist) {
							closestPoint = hitInfo.point;
							minSqrDist = dist;
						}
					}
				}
			}
		}

		return closestPoint - transform.position;
	}

	private void AdjustRotation() {
		Quaternion rotationDifference = Quaternion.FromToRotation(-transform.up, gravityAcceleration.normalized);
		Quaternion targetRotation = rotationDifference * transform.rotation;
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
	}

	private void InputWalk() {
		if (Input.GetKey(KeyCode.W)) {

		}

		if (Input.GetKey(KeyCode.A)) {

		}

		if (Input.GetKey(KeyCode.S)) {

		}

		if (Input.GetKey(KeyCode.D)) {

		}
	}

	private void InputJump() {

	}

	private void InputShoot() {

	}

}
