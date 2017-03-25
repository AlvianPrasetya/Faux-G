using UnityEngine;
using System.Collections;

public class PlayerController : Photon.MonoBehaviour {

	public Rigidbody prefabBullet;

	public GameObject spotlight;
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

	private Vector2 lookAroundVector;
	private bool isCrouching;
	private bool isSprinting;
	private Vector3 moveVector;
	private float jumpAcceleration;
	private bool isJumpCharged;

	private new Rigidbody rigidbody;
	private GravityBody gravityBody;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
		gravityBody = GetComponent<GravityBody>();

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
		if (Input.GetMouseButtonDown(Utils.Input.MOUSE_BUTTON_LEFT)) {
			Shoot();
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
		float minSpotlightRotateAngle = -Vector3.Angle(spotlight.transform.forward, -transform.up)
			- maxLookDownAngle + 90.0f;
		float maxSpotlightRotateAngle = Vector3.Angle(spotlight.transform.forward, transform.up)
			+ maxLookUpAngle - 90.0f;
		float rotateAngle = Mathf.Clamp(
			lookAroundVector.y * lookAroundSpeed * Time.fixedDeltaTime,
			minSpotlightRotateAngle,
			maxSpotlightRotateAngle
		);

		spotlight.transform.Rotate(
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

	private void Shoot() {
		int shootTime = PhotonNetwork.ServerTimestamp + Utils.RPC_SYNC_DELAY;
		Vector3 shootPosition = playerCamera.transform.position + playerCamera.transform.forward;
		Quaternion shootDirection = Quaternion.LookRotation(playerCamera.transform.forward);
		photonView.RPC("RpcShoot", PhotonTargets.AllViaServer, 
			shootTime, shootPosition, shootDirection);
	}

	[PunRPC]
	private void RpcShoot(int shootTime, Vector3 shootPosition, Quaternion shootDirection) {
		float secondsToShoot = (shootTime - PhotonNetwork.ServerTimestamp) / 1000.0f;
		StartCoroutine(WaitForShoot(secondsToShoot, shootPosition, shootDirection));
	}

	private IEnumerator WaitForShoot(float secondsToShoot, Vector3 shootPosition, Quaternion shootDirection) {
		if (secondsToShoot > 0.0f) {
			yield return new WaitForSecondsRealtime(secondsToShoot);
		}

		Instantiate(
			prefabBullet, 
			shootPosition, 
			shootDirection
		);
	}

}
