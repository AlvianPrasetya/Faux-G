using UnityEngine;
using System.Collections;

public class Utils : MonoBehaviour {
	
	public static readonly string GAME_VERSION = "v0.1";

	public static readonly float RESPAWN_TIME = 5.0f;

	public class Network {
		public static readonly int SEND_RATE = 15;
		public static readonly int SEND_RATE_ON_SERIALIZE = 15;

		// The base delay (ms) used to sync serializations between clients
		public static int BASE_SYNC_DELAY = 67;
		public static int SYNC_BUFFER_SIZE = 10;
	}

	public class Scene {
		public static readonly string LOGIN = "Login";
		public static readonly string LOBBY = "Lobby";
		public static readonly string ROOM = "Room";
		public static readonly string GAME = "Throwing Range";
	}

	public class Resource {
		public static readonly string PLAYER = "Thrower";
	}

	public class Tag {
		public static readonly string TERRAIN = "Terrain";
	}

	public class Layer {
		public static readonly int TERRAIN = 1 << LayerMask.NameToLayer("Terrain");
		public static readonly int THROWABLE = 1 << LayerMask.NameToLayer("Throwable");
		public static readonly int DETECT_TERRAIN = 1 << LayerMask.NameToLayer("Detect Terrain");
		public static readonly int DETECT_THROWABLE = 1 << LayerMask.NameToLayer("Detect Throwable");
	}

	public class Key {
		public static readonly string GAME_MODE = "GameMode";
		public static readonly string PING = "Ping";
	}

	public class Input {
		public static readonly string HORIZONTAL = "Horizontal";
		public static readonly string VERTICAL = "Vertical";
		public static readonly string MOUSE_X = "Mouse X";
		public static readonly string MOUSE_Y = "Mouse Y";
		public static readonly int MOUSE_BUTTON_LEFT = 0;
		public static readonly int MOUSE_BUTTON_RIGHT = 1;
		public static readonly int MOUSE_BUTTON_MIDDLE = 2;
		public static readonly KeyCode[] KEY_CODES_CHANGE_WEAPON = {
			KeyCode.Alpha1, 
			KeyCode.Alpha2, 
			KeyCode.Alpha3
		};
	}

	public class Physics {
		// G is scaled 5 * 1e5 times to compensate the mass and radii difference between the real and in-game worlds
		public static readonly float G = 3.37e-5f;

		// Golden ratio (Greek Phi)
		public static readonly float PHI = 1.618f;
	}

	public class Value {
		public static readonly float PI = Mathf.Acos(-1);
	}

	public static IEnumerator TransformLerpPosition(Transform targetTransform, Vector3 startPosition,
		Vector3 endPosition, float lerpTime) {
		float time = 0.0f;
		while (time < 1.0f) {
			targetTransform.localPosition = Vector3.Lerp(
				startPosition, 
				endPosition, 
				time
			);

			time += Time.deltaTime / lerpTime;
			yield return null;
		}

		targetTransform.localPosition = endPosition;
	}

	public static IEnumerator TransformSlerpRotation(Transform targetTransform, Quaternion startRotation,
		Quaternion endRotation, float slerpTime) {
		float time = 0.0f;
		while (time < 1.0f) {
			targetTransform.localRotation = Quaternion.Slerp(
				startRotation, 
				endRotation, 
				time
			);

			time += Time.deltaTime / slerpTime;
			yield return null;
		}

		targetTransform.localRotation = endRotation;
	}

	public static IEnumerator CameraLerpFieldOfView(Camera targetCamera, float startFieldOfView,
		float endFieldOfView, float lerpTime) {
		float time = 0.0f;
		while (time < 1.0f) {
			targetCamera.fieldOfView = Mathf.Lerp(
				startFieldOfView,
				endFieldOfView,
				time
			);

			time += Time.deltaTime / lerpTime;
			yield return null;
		}

		targetCamera.fieldOfView = endFieldOfView;
	}

}

public enum GameMode {
	
	FREE_FOR_ALL = 0, 
	TEAM_DEATHMATCH = 1, 
	CAPTURE_THE_FLAG = 2

}