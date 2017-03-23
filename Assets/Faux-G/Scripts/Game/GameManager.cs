using UnityEngine;

public class GameManager : MonoBehaviour {

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		PhotonNetwork.sendRate = Utils.SEND_RATE;
		PhotonNetwork.sendRateOnSerialize = Utils.SEND_RATE_ON_SERIALIZE;
	}

	void Start() {
		// Spawn player
		PhotonNetwork.Instantiate(Utils.Resource.PLAYER, Vector3.zero, Quaternion.identity, 0);

		// Hide cursor
		Cursor.visible = false;
	}

}
