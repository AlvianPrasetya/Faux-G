using UnityEngine;

/**
 * This class controls the movement behaviour of an entity and the game controls 
 * bound to such behaviour.
 */
public class MoveController : Photon.MonoBehaviour {

	public float moveSpeed;

	private new Rigidbody rigidbody;

	private Vector3 moveVector;

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();

		moveVector = Vector3.zero;
	}
	
	void Update() {
		if (photonView.isMine) {
			InputMove();
		}
	}

	void FixedUpdate() {
		if (photonView.isMine) {
			Move();
		}
	}

	private void InputMove() {
		moveVector = transform.forward * Input.GetAxis(Utils.Input.VERTICAL)
			+ transform.right * Input.GetAxis(Utils.Input.HORIZONTAL);
	}

	private void Move() {
		Vector3 moveVelocity = Vector3.ClampMagnitude(moveVector * moveSpeed, moveSpeed);
		rigidbody.MovePosition(transform.position + moveVelocity * Time.fixedDeltaTime);
	}

}
