using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : Photon.PunBehaviour {

	public Text loginStatusText;

	void Awake() {
		PhotonNetwork.logLevel = PhotonLogLevel.ErrorsOnly;
		PhotonNetwork.autoJoinLobby = false;
	}

	void Start() {
		Logger.Log("Connecting to Server");
		loginStatusText.text = "Connecting to Server";
		PhotonNetwork.ConnectUsingSettings(Utils.GAME_VERSION);
	}

	public override void OnConnectedToPhoton() {
		Logger.Log("Connected to Server");
		loginStatusText.text = "Connected to Server";
	}

	public override void OnConnectedToMaster() {
		Logger.Log("Joining Lobby");
		loginStatusText.text = "Joining Lobby";
		PhotonNetwork.JoinLobby();
	}

	public override void OnJoinedLobby() {
		Logger.Log("Joined Lobby");
		SceneManager.LoadScene(Utils.Scene.LOBBY);
	}

}
