using UnityEngine;

/**
 * This class manages the camera lifecycles throughout the entire game.
 * All callbacks related to camera events are to be directed to a method 
 * within this class.
 */
public class CameraManager : MonoBehaviour {

	public Camera sceneCamera;

	private Camera playerCamera;

	public Camera PlayerCamera {
		set {
			playerCamera = value;
		}
	}

	void Start() {
		sceneCamera.gameObject.SetActive(true);
	}

	public void OnPlayerSpawned(GameObject playerEntity, bool isLocalInstance) {
		playerCamera = playerEntity.GetComponentInChildren<Camera>();

		if (isLocalInstance) {
			// Disable scene camera on local instance
			sceneCamera.gameObject.SetActive(false);
			// Enable player camera on local instance
			playerCamera.gameObject.SetActive(true);
		} else {
			// Disable player camera on remote instances
			playerCamera.gameObject.SetActive(false);
		}
	}

}
