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
	private Camera sceneCamera;
	private GameObject localPlayer;
	private Camera playerCamera;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		PhotonNetwork.sendRate = Utils.SEND_RATE;
		PhotonNetwork.sendRateOnSerialize = Utils.SEND_RATE_ON_SERIALIZE;

		instance = this;
		isCursorLocked = true;
		sceneCamera = Camera.main;
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

	public void UpdateHealth(float currentHealth, float maxHealth) {
		healthText.text = string.Format("{0:0} / {1:0}", currentHealth, maxHealth);
	}

	public void Respawn() {
		if (localPlayer != null) {
			playerCamera.gameObject.SetActive(false);
			sceneCamera.gameObject.SetActive(true);

			sceneCamera.transform.position = playerCamera.transform.position;
			sceneCamera.transform.rotation = playerCamera.transform.rotation;

			Vector3 targetPosition = localPlayer.transform.position + localPlayer.transform.up * 20.0f;
			Quaternion targetRotation = Quaternion.LookRotation(localPlayer.transform.position - sceneCamera.transform.position);
			StartCoroutine(Utils.TransformLerpPosition(
				sceneCamera.transform, 
				sceneCamera.transform.position, 
				targetPosition, 
				Utils.RESPAWN_TIME
			));

			StartCoroutine(Utils.TransformSlerpRotation(
				sceneCamera.transform, 
				sceneCamera.transform.rotation, 
				targetRotation, 
				Utils.RESPAWN_TIME
			));
			
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
		playerCamera = localPlayer.GetComponentInChildren<Camera>();

		sceneCamera.gameObject.SetActive(false);
		playerCamera.gameObject.SetActive(true);
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
