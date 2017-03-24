using UnityEngine;

public class GameManager : MonoBehaviour {

	private bool isCursorLocked;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		PhotonNetwork.sendRate = Utils.SEND_RATE;
		PhotonNetwork.sendRateOnSerialize = Utils.SEND_RATE_ON_SERIALIZE;

		isCursorLocked = true;
	}

	void Start() {
		// Spawn player
		PhotonNetwork.Instantiate(Utils.Resource.PLAYER, Vector3.zero, Quaternion.identity, 0);
	}

	void Update() {
		InputToggleCursor();

		if (isCursorLocked) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		} else {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	private void InputToggleCursor() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			isCursorLocked = false;
		} else if (Input.anyKeyDown) {
			isCursorLocked = true;
		}
	}

}
