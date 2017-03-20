using UnityEngine;
using System.Diagnostics;

public class PlayerController : Photon.MonoBehaviour {

	public float rotateSpeed;

	private Vector3 gravityDirection;
	private Vector3 walkVector;

	// Cached components
	private new Rigidbody rigidbody;
	private Camera camera;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
		camera = Camera.main;
	}

	void Update() {
		if (!photonView.isMine) {
			return;
		}

		// Adjust player rotation due to gravity
		AdjustRotation();

		InputLookAround();
		InputWalk();
		InputJump();
		InputShoot();
	}

	void FixedUpdate() {
		// Calculate gravity and apply to player
		CalculateGravityDirection();
		rigidbody.AddForce(gravityDirection * Utils.GRAVITY, ForceMode.Acceleration);

		rigidbody.AddForce(walkVector, ForceMode.Acceleration);
	}

	private void CalculateGravityDirection() {
		float lowerBoundRadius = 0.0f;
		float upperBoundRadius = 100.0f;
		bool isTerrainFound = false;
		while (upperBoundRadius - lowerBoundRadius > 1e-4f) {
			float castRadius = (lowerBoundRadius + upperBoundRadius) / 2.0f;
			if (Physics.CheckSphere(transform.position, castRadius, Utils.Layer.TERRAIN)) {
				upperBoundRadius = castRadius;
				isTerrainFound = true;
			} else {
				lowerBoundRadius = castRadius;
			}
		}

		if (isTerrainFound) {
			RaycastHit hitInfo;
			Vector3 closestPoint = transform.position;
			float minSqrDist = Mathf.Infinity;
			for (int x = -1; x <= 1; x++) {
				for (int y = -1; y <= 1; y++) {
					for (int z = -1; z <= 1; z++) {
						Vector3 dir = new Vector3(x, y, z);

						if (Physics.SphereCast(transform.position, lowerBoundRadius, dir, 
							out hitInfo, 1e-3f, Utils.Layer.TERRAIN)) {
							float sqrDist = Vector3.SqrMagnitude(hitInfo.point - transform.position);
							if (sqrDist < minSqrDist) {
								closestPoint = hitInfo.point;
								minSqrDist = sqrDist;
							}
						}
					}
				}
			}
			gravityDirection = (closestPoint - transform.position).normalized;
		} else {
			gravityDirection = Vector3.zero;
		}
	}

	private void AdjustRotation() {
		Quaternion rotationDifference = Quaternion.FromToRotation(-transform.up, gravityDirection);
		Quaternion targetRotation = rotationDifference * transform.rotation;
		Quaternion newRotation = Quaternion.RotateTowards(
			transform.rotation, 
			targetRotation, 
			rotateSpeed * Time.deltaTime
		);
		rigidbody.MoveRotation(newRotation);
	}

	private void InputLookAround() {
		transform.Rotate(transform.up, Input.GetAxis(Utils.Key.INPUT_MOUSE_X), Space.World);
		camera.transform.Rotate(-transform.right, Input.GetAxis(Utils.Key.INPUT_MOUSE_Y), Space.World);
	}

	private void InputWalk() {
		walkVector = transform.forward * Input.GetAxis(Utils.Key.INPUT_VERTICAL)
			+ transform.right * Input.GetAxis(Utils.Key.INPUT_HORIZONTAL);
	}

	private void InputJump() {

	}

	private void InputShoot() {

	}

}
