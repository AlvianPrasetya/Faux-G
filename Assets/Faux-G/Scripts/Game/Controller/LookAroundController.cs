using UnityEngine;

/**
 * This class controls the look around behaviour of an entity as a function of 
 * mouse movement.
 */
public class LookAroundController : Photon.MonoBehaviour {
	
	public Transform verticalLookAroundPivot;
	public float lookAroundSpeed;

	// The absolute clamping angle when looking upwards w.r.t the line of horizon
	public float maxLookUpAngle;

	// The absolute clamping angle when looking downwards w.r.t the line of horizon
	public float maxLookDownAngle;

	private Vector2 lookAroundVector;

	void Awake() {
		lookAroundVector = Vector2.zero;
	}
	
	void Update() {
		if (photonView.isMine) {
			InputLookAround();
		}
	}

	void FixedUpdate() {
		if (photonView.isMine) {
			LookAround();
		}
	}

	private void InputLookAround() {
		lookAroundVector = new Vector2(
			Input.GetAxis(Utils.Input.MOUSE_X),
			Input.GetAxis(Utils.Input.MOUSE_Y)
		);
	}

	private void LookAround() {
		HorizontalLookAround();
		VerticalLookAround();
	}

	private void HorizontalLookAround() {
		transform.Rotate(
			transform.up,
			lookAroundVector.x * lookAroundSpeed * Time.fixedDeltaTime,
			Space.World
		);
	}

	private void VerticalLookAround() {
		float minRotateAngle = -Vector3.Angle(verticalLookAroundPivot.forward, -transform.up)
			- maxLookDownAngle + 90.0f;
		float maxRotateAngle = Vector3.Angle(verticalLookAroundPivot.forward, transform.up)
			+ maxLookUpAngle - 90.0f;

		float rotateAngle = Mathf.Clamp(
			lookAroundVector.y * lookAroundSpeed * Time.fixedDeltaTime,
			minRotateAngle,
			maxRotateAngle
		);

		verticalLookAroundPivot.Rotate(
			-verticalLookAroundPivot.right,
			rotateAngle,
			Space.World
		);
	}

}
