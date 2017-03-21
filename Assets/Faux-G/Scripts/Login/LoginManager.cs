using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour {

	public Text textStatus;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Awake() {
		PhotonNetwork.logLevel = PhotonLogLevel.ErrorsOnly;
		PhotonNetwork.autoJoinLobby = false;
	}

	void Start() {
		Logger.Log("Connecting to Server");
		textStatus.text = "Connecting to Server";
		PhotonNetwork.ConnectUsingSettings(Utils.GAME_VERSION);
	}

	/*
	 * PHOTON LIFECYCLE
	 */

	void OnConnectedToPhoton() {
		Logger.Log("Connected to Server");
		textStatus.text = "Connected to Server";
	}

	void OnConnectedToMaster() {
		Logger.Log("Joining Lobby");
		textStatus.text = "Joining Lobby";
		PhotonNetwork.JoinLobby();
	}

	void OnJoinedLobby() {
		Logger.Log("Joined Lobby");
		SceneManager.LoadScene(Utils.Scene.LOBBY);
	}

}
