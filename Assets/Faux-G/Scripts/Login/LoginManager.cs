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
		Logger.D("Connecting to Server");
		textStatus.text = "Connecting to Server";
		PhotonNetwork.ConnectUsingSettings(Utils.GAME_VERSION);
	}

	/*
	 * PHOTON LIFECYCLE
	 */

	void OnConnectedToPhoton() {
		Logger.D("Connected to Server");
		textStatus.text = "Connected to Server";
	}

	void OnConnectedToMaster() {
		Logger.D("Joining Lobby");
		textStatus.text = "Joining Lobby";
		PhotonNetwork.JoinLobby();
	}

	void OnJoinedLobby() {
		Logger.D("Joined Lobby");
		SceneManager.LoadScene(Utils.Scene.LOBBY);
	}

}
