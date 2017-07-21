using UnityEngine;

public class PlayerController : Photon.MonoBehaviour {

	public delegate void OnPlayerSpawnedCallback(GameObject playerEntity, bool isLocalInstance);
	
	private new Rigidbody rigidbody;
	private GravityBody gravityBody;

	private OnPlayerSpawnedCallback playerSpawnedCallback;

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
		gravityBody = GetComponent<GravityBody>();

		AddPlayerSpawnedCallback(GameManagerBase.Instance.cameraManager.OnPlayerSpawned);
		AddPlayerSpawnedCallback(OnPlayerSpawned);
	}

	public void Start() {
		if (playerSpawnedCallback != null) {
			playerSpawnedCallback(gameObject, photonView.isMine);
		}
	}

	public string NickName {
		get {
			return photonView.owner.NickName;
		}
	}

	public void AddPlayerSpawnedCallback(OnPlayerSpawnedCallback playerSpawnedCallback) {
		if (this.playerSpawnedCallback == null) {
			this.playerSpawnedCallback = playerSpawnedCallback;
		} else {
			this.playerSpawnedCallback += playerSpawnedCallback;
		}
	}

	private void OnPlayerSpawned(GameObject playerEntity, bool isLocalInstance) {
		if (isLocalInstance) {
			// Enable physics on local instance
			rigidbody.isKinematic = false;
			gravityBody.enabled = true;
		} else {
			// Disable physics on remote instances
			rigidbody.isKinematic = true;
			gravityBody.enabled = false;
		}
	}

}
