using UnityEngine;

public abstract class GravityBody : Photon.MonoBehaviour {

	public float rotateSpeed;

	private static readonly float BINARY_SEARCH_LOWER_BOUND = 0.0f;
	private static readonly float BINARY_SEARCH_UPPER_BOUND = 100.0f;
	private static readonly float BINARY_SEARCH_EPSILON = 1e-5f;
	private static readonly float SPHERECAST_DISTANCE = 1e-4f;

	protected Vector3 gravityDirection;

	protected new Rigidbody rigidbody;

	protected virtual void Awake() {
		if (!photonView.isMine) {
			return;
		}

		gravityDirection = Vector3.zero;

		rigidbody = GetComponent<Rigidbody>();
	}

	protected virtual void Update() {
		if (!photonView.isMine) {
			return;
		}

		// Adjust player rotation due to gravity
		AdjustRotation();
	}

	protected virtual void FixedUpdate() {
		if (!photonView.isMine) {
			return;
		}

		// Calculate gravity and apply to rigidbody
		CalculateGravityDirection();
		rigidbody.AddForce(gravityDirection * rigidbody.mass * Utils.GRAVITY, ForceMode.Force);
	}

	private void CalculateGravityDirection() {
		float lowerBoundRadius = BINARY_SEARCH_LOWER_BOUND;
		float upperBoundRadius = BINARY_SEARCH_UPPER_BOUND;
		bool isTerrainFound = false;
		while (upperBoundRadius - lowerBoundRadius > BINARY_SEARCH_EPSILON) {
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
							out hitInfo, SPHERECAST_DISTANCE, Utils.Layer.TERRAIN)) {
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
		transform.rotation = newRotation;
	}

}
