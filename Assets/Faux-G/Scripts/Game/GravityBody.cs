using UnityEngine;

public class GravityBody : Photon.MonoBehaviour {

	public float gravityChangeAngleThreshold;
	public float minRotateSpeed;
	public float maxRotateSpeed;

	private new Rigidbody rigidbody;
	
	private Vector3 lastGravityDirection;
	private Vector3 gravityDirection;
	private float referenceAngle;
	
	void Awake() {
		rigidbody = GetComponent<Rigidbody>();

		lastGravityDirection = Vector3.zero;
		gravityDirection = Vector3.zero;
	}

	void FixedUpdate() {
		lastGravityDirection = gravityDirection;

		// Apply gravitational force
		ApplyGravitationalForce();

		// Apply gravitational torque
		ApplyGravitationalTorque();
	}

	private void ApplyGravitationalForce() {
		Vector3 netForceVector = Vector3.zero;
		foreach (AttractorBase attractor in AttractorManager.Instance.attractors) {
			netForceVector += attractor.CalculateGravitationalForce(transform.position, rigidbody.mass);
		}

		rigidbody.AddForce(netForceVector, ForceMode.Force);

		// Update gravity direction
		gravityDirection = netForceVector.normalized;
	}

	private void ApplyGravitationalTorque() {
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
