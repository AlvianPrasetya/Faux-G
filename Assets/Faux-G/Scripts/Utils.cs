using UnityEngine;

public class Utils : MonoBehaviour {

	public static readonly string GAME_VERSION = "v0.1";

	public static readonly int SEND_RATE = 10;
	public static readonly int SEND_RATE_ON_SERIALIZE = 10;

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
		public static readonly string GAME_MODE = "game_mode";
	}

	public static readonly float GRAVITY = 1.635f;
}

public enum GameMode {
	
	FREE_FOR_ALL = 0, 
	TEAM_DEATHMATCH = 1, 
	CAPTURE_THE_FLAG = 2

}