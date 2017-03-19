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
			if (Physics.CheckSphere(transform.position, castRadius, Utils.LAYER_TERRAIN)) {
				upperBoundRadius = castRadius;
			} else {
				lowerBoundRadius = castRadius;
			}
		}

		RaycastHit hitInfo;
		Vector3 closestPoint = transform.position;
		float minSqrDist = Mathf.Infinity;
		for (int x = -2; x <= 2; x++) {
			for (int y = -2; y <= 2; y++) {
				for (int z = -2; z <= 2; z++) {
					Vector3 dir = new Vector3(x * 0.5f, y * 0.5f, z * 0.5f);

					if (Physics.SphereCast(transform.position, lowerBoundRadius, dir, 
						out hitInfo, 1e-3f, Utils.LAYER_TERRAIN)) {
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
