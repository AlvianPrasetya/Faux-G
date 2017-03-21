using UnityEngine;

public class PlayerController : GravityBody {

	public Camera playerCamera;

	public float walkSpeed;
	public float sprintSpeed;
	public float jumpAcceleration;
	
	private Vector3 walkVector;
	private bool isSprinting;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	protected override void Awake() {
		if (!photonView.isMine) {
			return;
		}

		base.Awake();
		
		Camera.main.gameObject.SetActive(false);
		playerCamera.gameObject.SetActive(true);

		walkVector = Vector3.zero;
		isSprinting = false;
	}

	protected override void Update() {
		if (!photonView.isMine) {
			return;
		}

		base.Update();

		InputLookAround();
		InputWalk();
		InputJump();
		InputShoot();
	}

	protected override void FixedUpdate() {
		if (!photonView.isMine) {
			return;
		}

		base.FixedUpdate();

		float moveSpeed = (isSprinting) ? sprintSpeed : walkSpeed;

		transform.Translate(
			Vector3.ClampMagnitude(walkVector * moveSpeed, moveSpeed) * Time.fixedDeltaTime, 
			Space.World
		);
	}

	private void InputLookAround() {
		transform.Rotate(transform.up, Input.GetAxis(Utils.Key.INPUT_MOUSE_X), Space.World);
		playerCamera.transform.Rotate(-transform.right, Input.GetAxis(Utils.Key.INPUT_MOUSE_Y), Space.World);
	}

	private void InputWalk() {
		if (Input.GetKeyDown(KeyCode.LeftShift)) {
			isSprinting = true;
		}

		if (Input.GetKeyUp(KeyCode.LeftShift)) {
			isSprinting = false;
		}

		walkVector = transform.forward * Input.GetAxis(Utils.Key.INPUT_VERTICAL)
			+ transform.right * Input.GetAxis(Utils.Key.INPUT_HORIZONTAL);
	}

	private void InputJump() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			rigidbody.AddForce(transform.up * rigidbody.mass * jumpAcceleration, ForceMode.Impulse);
		}
	}

	private void InputShoot() {

	}

}
