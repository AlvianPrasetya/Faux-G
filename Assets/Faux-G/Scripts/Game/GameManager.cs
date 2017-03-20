using UnityEngine;

public class GameManager : MonoBehaviour {

	private GameObject player;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		PhotonNetwork.sendRate = Utils.SEND_RATE;
		PhotonNetwork.sendRateOnSerialize = Utils.SEND_RATE_ON_SERIALIZE;
	}

	void Start() {
		// Spawn player
		player = PhotonNetwork.Instantiate(Utils.Resource.PLAYER, Vector3.zero, Quaternion.identity, 0);
		Camera.main.transform.parent = player.transform;
		Camera.main.transform.localPosition = Vector3.zero;
	}

}
