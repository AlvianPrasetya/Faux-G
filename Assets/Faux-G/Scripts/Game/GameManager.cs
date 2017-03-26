using UnityEngine;

public class GameManager : Photon.MonoBehaviour {

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

		InvokeRepeating("SyncPing", 0.0f, Utils.SYNC_PING_INTERVAL);
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

	private void SyncPing() {
		ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
		customProperties[Utils.Key.PING] = PhotonNetwork.GetPing();
		PhotonNetwork.player.SetCustomProperties(customProperties);

		// Calculate max ping of players
		int maxPing = 0;
		foreach (PhotonPlayer player in PhotonNetwork.playerList) {
			object pingObject = player.CustomProperties[Utils.Key.PING];
			int playerPing = (pingObject != null) ? (int) pingObject : 0;

			maxPing = Mathf.Max(
				maxPing, 
				playerPing
			);
		}
		
		// Calculate sync delay
		Utils.CURRENT_SYNC_DELAY = Mathf.Min(
			Utils.MAX_SYNC_DELAY, 
			Utils.BASE_SYNC_DELAY + maxPing
		);
		Logger.Log("Current sync delay: {0}", Utils.CURRENT_SYNC_DELAY);
	}

}
