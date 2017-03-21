using UnityEngine;
using System.Collections;

public class PlayerController : GravityBody {

	public Rigidbody prefabProjectile;

	public Camera playerCamera;

	// Walk/sprint parameters
	public float walkSpeed;
	public float sprintSpeed;

	// Jump parameters
	public float minJumpAcceleration;
	public float jumpAccelerationChargeRate;
	public float maxJumpAcceleration;

	// Shoot parameters
	public float shootAcceleration;

	// The delay (ms) used to sync RPCs between clients
	public int rpcSyncDelay;

	private Vector2 lookAroundVector;
	private Vector3 walkVector;
	private bool isSprinting;
	private float jumpAcceleration;
	private bool isJumpCharging;
	private bool isJumpCharged;

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

		lookAroundVector = Vector2.zero;
		walkVector = Vector3.zero;
		isSprinting = false;
		jumpAcceleration = 0.0f;
		isJumpCharging = false;
		isJumpCharged = false;
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

		LookAround();
		Walk();
		Jump();
	}

	private void InputLookAround() {
		lookAroundVector = new Vector2(
			Input.GetAxis(Utils.Input.MOUSE_X), 
			Input.GetAxis(Utils.Input.MOUSE_Y)
		);
	}

	private void InputWalk() {
		if (Input.GetKeyDown(KeyCode.LeftShift)) {
			isSprinting = true;
		}

		if (Input.GetKeyUp(KeyCode.LeftShift)) {
			isSprinting = false;
		}

		walkVector = transform.forward * Input.GetAxis(Utils.Input.VERTICAL)
			+ transform.right * Input.GetAxis(Utils.Input.HORIZONTAL);
	}

	private void InputJump() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			isJumpCharging = true;
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
			isJumpCharging = false;
			isJumpCharged = true;
		}
	}

	private void InputShoot() {
		if (Input.GetMouseButtonDown(Utils.Input.MOUSE_BUTTON_LEFT)) {
			Shoot();
		}
	}

	private void LookAround() {
		transform.Rotate(transform.up, lookAroundVector.x, Space.World);
		playerCamera.transform.Rotate(-transform.right, lookAroundVector.y, Space.World);
	}

	private void Walk() {
		float moveSpeed = (isSprinting) ? sprintSpeed : walkSpeed;

		transform.Translate(
			Vector3.ClampMagnitude(walkVector * moveSpeed, moveSpeed) * Time.fixedDeltaTime, 
			Space.World
		);
	}

	private void Jump() {
		if (isJumpCharged) {
			int jumpTime = PhotonNetwork.ServerTimestamp + rpcSyncDelay;
			Vector3 jumpForce = transform.up * rigidbody.mass * jumpAcceleration;
			photonView.RPC("RpcJump", PhotonTargets.AllViaServer, jumpTime, jumpForce);
			isJumpCharged = false;
		}
	}

	private void Shoot() {
		int shootTime = PhotonNetwork.ServerTimestamp + rpcSyncDelay;
		Vector3 shootDirection = playerCamera.transform.forward;
		photonView.RPC("RpcShoot", PhotonTargets.AllViaServer, shootTime, shootDirection);
	}

	[PunRPC]
	private void RpcJump(int jumpTime, Vector3 jumpForce) {
		float secondsToJump = (jumpTime - PhotonNetwork.ServerTimestamp) / 1000.0f;
		StartCoroutine(WaitForJump(secondsToJump, jumpForce));
	}

	private IEnumerator WaitForJump(float secondsToJump, Vector3 jumpForce) {
		if (secondsToJump > 0.0f) {
			yield return new WaitForSecondsRealtime(secondsToJump);
		}

		rigidbody.AddForce(jumpForce, ForceMode.Impulse);
	}

	[PunRPC]
	private void RpcShoot(int shootTime, Vector3 shootDirection) {
		float secondsToShoot = (shootTime - PhotonNetwork.ServerTimestamp) / 1000.0f;
		StartCoroutine(WaitForShoot(secondsToShoot, shootDirection));
	}

	private IEnumerator WaitForShoot(float secondsToShoot, Vector3 shootDirection) {
		if (secondsToShoot > 0.0f) {
			yield return new WaitForSecondsRealtime(secondsToShoot);
		}

		Rigidbody projectile = Instantiate(
			prefabProjectile, 
			transform.position + transform.forward, 
			Quaternion.identity
		);
		projectile.AddForce(shootDirection * projectile.mass * shootAcceleration, ForceMode.Impulse);
	}

}
