using UnityEngine;

public class Utils : MonoBehaviour {
	
	public static readonly string GAME_VERSION = "v0.1";

	public static readonly int SEND_RATE = 15;
	public static readonly int SEND_RATE_ON_SERIALIZE = 15;

	// The minimum delay (ms) used to sync RPCs and serializations between clients
	public static readonly int BASE_SYNC_DELAY = 100;
	public static readonly int MAX_SYNC_DELAY = 400;
	public static int CURRENT_SYNC_DELAY = 200;

	// The interval (s) used to sync ping (RTT) between clients
	public static readonly float SYNC_PING_INTERVAL = 10.0f;

	public class Scene {
		public static readonly string LOGIN = "Login";
		public static readonly string LOBBY = "Lobby";
		public static readonly string ROOM = "Room";
		public static readonly string GAME = "Game";
	}

	public class Resource {
		public static readonly string PLAYER = "Player";
	}

	public class Layer {
		public static readonly int TERRAIN = 1 << LayerMask.NameToLayer("Terrain");
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

	public static readonly float GRAVITY = 9.81f;

	public static readonly float PI = Mathf.Acos(-1);
}

public enum GameMode {
	
	FREE_FOR_ALL = 0, 
	TEAM_DEATHMATCH = 1, 
	CAPTURE_THE_FLAG = 2

}