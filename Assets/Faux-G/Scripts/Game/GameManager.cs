using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public Image crosshairImage;
	public Text healthText;
	public List<Transform> spawnPoints;

	private static GameManager instance;
	private bool isCursorLocked;
	private GameObject localPlayer;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		PhotonNetwork.sendRate = Utils.SEND_RATE;
		PhotonNetwork.sendRateOnSerialize = Utils.SEND_RATE_ON_SERIALIZE;

		instance = this;
		isCursorLocked = true;
	}

	void Start() {
		Spawn();
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

	public static GameManager Instance {
		get {
			return instance;
		}
	}

	public Image Crosshair {
		get {
			return crosshairImage;
		}
	}

	public void UpdateHealth(float health) {
		healthText.text = string.Format("{0:0} / 100", health);
	}

	public void Respawn() {
		if (localPlayer != null) {
			PhotonNetwork.Destroy(localPlayer);
		}

		StartCoroutine(WaitForSpawn(Utils.RESPAWN_TIME));
	}

	private void Spawn() {
		int spawnPointId = Random.Range(0, spawnPoints.Count);
		localPlayer = PhotonNetwork.Instantiate(
			Utils.Resource.PLAYER,
			spawnPoints[spawnPointId].position,
			spawnPoints[spawnPointId].rotation,
			0
		);
	}

	private void InputToggleCursor() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			isCursorLocked = false;
		} else if (Input.anyKeyDown) {
			isCursorLocked = true;
		}
	}

	private IEnumerator WaitForSpawn(float waitTime) {
		yield return new WaitForSeconds(waitTime);
		Spawn();
	}

}
