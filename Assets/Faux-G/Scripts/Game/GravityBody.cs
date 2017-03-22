using UnityEngine;

public class GravityBody : Photon.MonoBehaviour {

	public float rotateSpeed;

	private static readonly float BINARY_SEARCH_LOWER_BOUND = 0.0f;
	private static readonly float BINARY_SEARCH_UPPER_BOUND = 1000.0f;
	private static readonly float BINARY_SEARCH_EPSILON = 1e-4f;
	private static readonly float SPHERECAST_DISTANCE = 1e-3f;
	private static readonly Vector3 NUM_SPHERECAST_DIRECTIONS = new Vector3(4, 4, 4);

	private Vector3 gravityDirection;

	private new Rigidbody rigidbody;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		gravityDirection = Vector3.zero;

		rigidbody = GetComponent<Rigidbody>();
	}

	void Update() {
		// Adjust player rotation due to gravity
		AdjustRotation();
	}

	void FixedUpdate() {
		// Calculate gravity and apply to rigidbody
		UpdateGravityDirection();
		rigidbody.AddForce(gravityDirection * rigidbody.mass * Utils.GRAVITY, ForceMode.Force);
	}

	private void UpdateGravityDirection() {
		/*
		 * Binary search to obtain largest radius of sphere centered at the player that 
		 * does not come in contact with any terrain.
		 */
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
			/*
			 * Project the largest radius sphere radially to get the closest terrain 
			 * point to the player.
			 */
			RaycastHit hitInfo;
			Vector3 closestPoint = transform.position;
			float minSqrDist = Mathf.Infinity;
			for (int x = 0; x < NUM_SPHERECAST_DIRECTIONS.x; x++) {
				for (int y = 0; y < NUM_SPHERECAST_DIRECTIONS.y; y++) {
					for (int z = 0; z < NUM_SPHERECAST_DIRECTIONS.z; z++) {
						Quaternion targetRotation = Quaternion.Euler(
							x * 360.0f / NUM_SPHERECAST_DIRECTIONS.x, 
							y * 360.0f / NUM_SPHERECAST_DIRECTIONS.y, 
							z * 360.0f / NUM_SPHERECAST_DIRECTIONS.z);
						Vector3 targetVector = targetRotation * Vector3.forward;

						if (Physics.SphereCast(transform.position, lowerBoundRadius, targetVector,
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
