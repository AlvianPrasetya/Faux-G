using UnityEngine;

public class Utils : MonoBehaviour {

	public static readonly string GAME_VERSION = "v0.1";

	public static readonly int SEND_RATE = 10;
	public static readonly int SEND_RATE_ON_SERIALIZE = 10;

	public static readonly string SCENE_LOGIN = "Login";
	public static readonly string SCENE_LOBBY = "Lobby";
	public static readonly string SCENE_ROOM = "Room";
	public static readonly string SCENE_GAME = "Game";

	public static readonly string GAME_MODE = "game_mode";

	public static readonly int LAYER_TERRAIN = 1 << LayerMask.NameToLayer("Terrain");

	public static readonly float GRAVITY = 1.6f;
}

public enum GameMode {
	
	FREE_FOR_ALL = 0, 
	TEAM_DEATHMATCH = 1, 
	CAPTURE_THE_FLAG = 2

}