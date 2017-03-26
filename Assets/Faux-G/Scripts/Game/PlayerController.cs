using UnityEngine;

public class PlayerController : Photon.MonoBehaviour {

	public Transform playerHead;
	public Camera playerCamera;

	// Lookaround parameters
	public float lookAroundSpeed;
	public float maxLookUpAngle;
	public float maxLookDownAngle;

	// Walk/sprint parameters
	public float crouchSpeed;
	public float walkSpeed;
	public float sprintSpeed;

	// Jump parameters
	public float minJumpAcceleration;
	public float jumpAccelerationChargeRate;
	public float maxJumpAcceleration;

	// Cached components
	private new Rigidbody rigidbody;
	private GravityBody gravityBody;
	private WeaponController weaponController;

	private Vector2 lookAroundVector;
	private bool isCrouching;
	private bool isSprinting;
	private Vector3 moveVector;
	private float jumpAcceleration;
	private bool isJumpCharged;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
		gravityBody = GetComponent<GravityBody>();
		weaponController = GetComponent<WeaponController>();

		if (!photonView.isMine) {
			rigidbody.isKinematic = true;
			gravityBody.enabled = false;
			return;
		}
		
		Camera.main.gameObject.SetActive(false);
		playerCamera.gameObject.SetActive(true);

		lookAroundVector = Vector2.zero;
		isCrouching = false;
		isSprinting = false;
		moveVector = Vector3.zero;
		jumpAcceleration = 0.0f;
		isJumpCharged = false;
	}

	void Update() {
		if (!photonView.isMine) {
			return;
		}

		InputLookAround();
		InputCrouch();
		InputSprint();
		InputMove();
		InputJump();
		InputShoot();
		InputChangeWeapon();
		InputReload();
	}

	void FixedUpdate() {
		if (!photonView.isMine) {
			return;
		}

		LookAround();
		Move();
		Jump();
	}

	private void InputLookAround() {
		lookAroundVector = new Vector2(
			Input.GetAxis(Utils.Input.MOUSE_X), 
			Input.GetAxis(Utils.Input.MOUSE_Y)
		);
	}

	private void InputCrouch() {
		if (Input.GetKeyDown(KeyCode.LeftControl)) {
			isCrouching = true;
		}

		if (Input.GetKeyUp(KeyCode.LeftControl)) {
			isCrouching = false;
		}
	}

	private void InputSprint() {
		if (Input.GetKeyDown(KeyCode.LeftShift)) {
			isSprinting = true;
		}

		if (Input.GetKeyUp(KeyCode.LeftShift)) {
			isSprinting = false;
		}
	}

	private void InputMove() {
		moveVector = transform.forward * Input.GetAxis(Utils.Input.VERTICAL)
			+ transform.right * Input.GetAxis(Utils.Input.HORIZONTAL);
	}

	private void InputJump() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			jumpAcceleration = minJumpAcceleration;
		}

		if (Input.GetKey(KeyCode.Space)) {
			jumpAcceleration = Mathf.Clamp(
				jumpAcceleration + jumpAccelerationChargeRate * Time.deltaTime, 
				minJumpAcceleration, 
				maxJumpAcceleration
			);
			jumpAcceleration += jumpAccelerationChargeRate * Time.deltaTime;
		}

		if (Input.GetKeyUp(KeyCode.Space)) {
			isJumpCharged = true;
		}
	}

	private void InputShoot() {
		if (Input.GetMouseButton(Utils.Input.MOUSE_BUTTON_LEFT)) {
			weaponController.Shoot();
		}
	}

	private void InputChangeWeapon() {
		for (int i = 0; i < Utils.Input.KEY_CODES_CHANGE_WEAPON.Length; i++) {
			if (Input.GetKeyDown(Utils.Input.KEY_CODES_CHANGE_WEAPON[i])) {
				weaponController.ChangeWeapon(i);
				break;
			}
		}
	}

	private void InputReload() {
		if (Input.GetKeyDown(KeyCode.R)) {
			weaponController.Reload();
		}
	}

	private void LookAround() {
		LookLeftRight();
		LookUpDown();
	}

	private void LookLeftRight() {
		transform.Rotate(
			transform.up,
			lookAroundVector.x * lookAroundSpeed * Time.fixedDeltaTime,
			Space.World
		);
	}

	private void LookUpDown() {
		float minSpotlightRotateAngle = -Vector3.Angle(playerHead.transform.forward, -transform.up)
			- maxLookDownAngle + 90.0f;
		float maxSpotlightRotateAngle = Vector3.Angle(playerHead.transform.forward, transform.up)
			+ maxLookUpAngle - 90.0f;
		float rotateAngle = Mathf.Clamp(
			lookAroundVector.y * lookAroundSpeed * Time.fixedDeltaTime,
			minSpotlightRotateAngle,
			maxSpotlightRotateAngle
		);

		playerHead.transform.Rotate(
			-transform.right,
			rotateAngle,
			Space.World
		);
	}

	private void Move() {
		float moveSpeed;
		if (isCrouching) {
			moveSpeed = crouchSpeed;
		} else if (isSprinting) {
			moveSpeed = sprintSpeed;
		} else {
			moveSpeed = walkSpeed;
		}

		Vector3 moveVelocity = Vector3.ClampMagnitude(moveVector * moveSpeed, moveSpeed);

		transform.Translate(
			moveVelocity * Time.fixedDeltaTime, 
			Space.World
		);
	}

	private void Jump() {
		if (isJumpCharged) {
			Vector3 jumpForce = transform.up * rigidbody.mass * jumpAcceleration;
			rigidbody.AddForce(jumpForce, ForceMode.Impulse);
			isJumpCharged = false;
		}
	}

}
