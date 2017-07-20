using UnityEngine;

/**
 * This class controls the jumping behaviour of an entity and the game controls 
 * bound to such behaviour.
 */
public class JumpController : Photon.MonoBehaviour {

	public enum JUMP_STATE {
		IDLE, // IDLE state, currently not charging a jump
		CHARGING // CHARGING state, currently charging a jump
	}

	// The increment in relative jump force per second of charging
	public float relativeJumpForcePerSecond;

	public float minJumpForce;
	public float maxJumpForce;

	public float groundedDistanceTolerance;

	private new Rigidbody rigidbody;

	private JUMP_STATE jumpState;

	// Relative jump force ranging from 0 ~ 1 (0 = minJumpForce, 1 = maxJumpForce)
	private float relativeJumpForce;
	
	private bool grounded;

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();

		jumpState = JUMP_STATE.IDLE;
		grounded = false;
	}
	
	void Update() {
		if (photonView.isMine) {
			InputChargeJump();
			InputReleaseJump();
		}
	}

	void FixedUpdate() {
		UpdateRelativeJumpForce();
		UpdateGroundedState();
	}

	private void InputChargeJump() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			ChargeJump();
		}
	}

	private void InputReleaseJump() {
		if (Input.GetKeyUp(KeyCode.Space)) {
			ReleaseJump();
		}
	}

	/**
	 * This method attempts to do a networked charging of jump action.
	 */
	private void ChargeJump() {
		if (jumpState == JUMP_STATE.IDLE) {
			photonView.RPC("RpcChargeJump", PhotonTargets.All,
				PhotonNetwork.ServerTimestamp);
		}
	}

	/**
	 * This method attempts to do a networked release of jump action.
	 */
	private void ReleaseJump() {
		if (jumpState == JUMP_STATE.CHARGING && grounded) {
			// The jump force is only applied locally as entity transforms are synchronized through 
			// the network by other means (SyncTransform, etc.)
			Vector3 jumpDirection = transform.up;
			float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, relativeJumpForce);
			rigidbody.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);

			photonView.RPC("RpcReleaseJump", PhotonTargets.All,
				PhotonNetwork.ServerTimestamp);
		}
	}

	/**
	 * This method updates the relative jumping force based on the value of 
	 * relativeJumpForcePerSecond if and only if the jump state is at CHARGING
	 * (the character is currently charging a jump).
	 */
	private void UpdateRelativeJumpForce() {
		if (jumpState == JUMP_STATE.CHARGING) {
			relativeJumpForce = Mathf.Clamp(
				relativeJumpForce + relativeJumpForcePerSecond * Time.fixedDeltaTime, 
				0.0f, 
				1.0f);
		}
	}

	/**
	 * This method checks the grounded status of this entity by casting a raycast downwards, 
	 * looking for a TERRAIN layer object within groundedDistanceTolerance.
	 */
	private void UpdateGroundedState() {
		if (Physics.Raycast(transform.position, -transform.up, groundedDistanceTolerance, Utils.Layer.TERRAIN)) {
			grounded = true;
		} else {
			grounded = false;
		}
	}

	[PunRPC]
	private void RpcChargeJump(int eventTimeMs) {
		// TODO: Trigger charging jump animation

		jumpState = JUMP_STATE.CHARGING;
	}

	[PunRPC]
	private void RpcReleaseJump(int eventTimeMs) {
		// TODO: Trigger jumping animation

		jumpState = JUMP_STATE.IDLE;
		relativeJumpForce = 0.0f;
	}

}
