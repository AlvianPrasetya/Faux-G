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

	// Check grounded parameters
	public float maxDistanceToGround;

	// Cached components
	private new Rigidbody rigidbody;
	private GravityBody gravityBody;
	private WeaponController weaponController;
	private Health health;

	private Vector2 lookAroundVector;
	private bool isCrouching;
	private bool isSprinting;
	private Vector3 moveVector;
	private float jumpAcceleration;
	private bool isJumpCharged;
	private bool isGrounded;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
		gravityBody = GetComponent<GravityBody>();
		weaponController = GetComponent<WeaponController>();
		weaponController.SetAmmoUpdateCallback(GameManager.Instance.UpdateAmmo);
		health = GetComponent<Health>();
		health.SetHealthUpdateCallback(GameManager.Instance.UpdateHealth);
		health.SetDeathCallback(GameManager.Instance.Respawn);

		if (!photonView.isMine) {
			rigidbody.isKinematic = true;
			gravityBody.enabled = false;
			playerCamera.gameObject.SetActive(false);
			weaponController.scopeCamera.gameObject.SetActive(false);
			return;
		}

		lookAroundVector = Vector2.zero;
		isCrouching = false;
		isSprinting = false;
		moveVector = Vector3.zero;
		jumpAcceleration = 0.0f;
		isJumpCharged = false;
		isGrounded = false;
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
		InputToggleAim();
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

		CheckGrounded();
	}

	public string GetNickName() {
		return photonView.owner.NickName;
	}

	public float GetCurrentHealth() {
		return health.CurrentHealth;
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

	private void InputToggleAim() {
		if (Input.GetMouseButtonDown(Utils.Input.MOUSE_BUTTON_RIGHT)) {
			weaponController.ToggleAim();
		}
	}

	private void InputChangeWeapon() {
		for (int i = 0; i < Utils.Input.KEY_CODES_CHANGE_WEAPON.Length; i++) {
			if (Input.GetKeyDown(Utils.Input.KEY_CODES_CHANGE_WEAPON[i])) {
				weaponController.ChangeWeapon(i);
				break;
			}
		}

		if (Input.GetKeyDown(KeyCode.Q)) {
			weaponController.CycleWeapon();
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
		float minSpotlightRotateAngle = -Vector3.Angle(playerHead.forward, -transform.up)
			- maxLookDownAngle + 90.0f;
		float maxSpotlightRotateAngle = Vector3.Angle(playerHead.forward, transform.up)
			+ maxLookUpAngle - 90.0f;
		float rotateAngle = Mathf.Clamp(
			lookAroundVector.y * lookAroundSpeed * Time.fixedDeltaTime,
			minSpotlightRotateAngle,
			maxSpotlightRotateAngle
		);

		playerHead.Rotate(
			-playerHead.right,
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
		
		rigidbody.MovePosition(transform.position + moveVelocity * Time.fixedDeltaTime);
	}

	private void Jump() {
		if (isJumpCharged) {
			if (isGrounded) {
				// Allow jump only when player is grounded
				Vector3 jumpForce = transform.up * rigidbody.mass * jumpAcceleration;
				rigidbody.AddForce(jumpForce, ForceMode.Impulse);
			}
			isJumpCharged = false;
		}
	}

	private void CheckGrounded() {
		if (Physics.Raycast(transform.position, -transform.up, maxDistanceToGround, Utils.Layer.TERRAIN)) {
			isGrounded = true;
		} else {
			isGrounded = false;
		}
	}

}
