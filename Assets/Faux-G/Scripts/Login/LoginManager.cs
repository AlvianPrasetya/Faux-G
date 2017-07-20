using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : Photon.PunBehaviour {

	public Text playerNameText;
	public InputField playerNameField;
	public Button loginButton;
	public Text loginStatusText;

	private string confirmedPlayerName;

	void Awake() {
		PhotonNetwork.logLevel = PhotonLogLevel.ErrorsOnly;
		PhotonNetwork.autoJoinLobby = false;

		playerNameText.gameObject.SetActive(true);
		playerNameField.gameObject.SetActive(true);
		loginButton.gameObject.SetActive(true);
		loginStatusText.gameObject.SetActive(false);

		loginButton.onClick.AddListener(Connect);
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

		// Change networked name to the confirmed player name
		PhotonNetwork.playerName = confirmedPlayerName;

		// Load lobby
		SceneManager.LoadScene(Utils.Scene.LOBBY);
	}

	private void Connect() {
		confirmedPlayerName = playerNameField.text;

		playerNameText.gameObject.SetActive(false);
		playerNameField.gameObject.SetActive(false);
		loginButton.gameObject.SetActive(false);
		loginStatusText.gameObject.SetActive(true);

		Logger.Log("Connecting to Server");
		loginStatusText.text = "Connecting to Server";
		PhotonNetwork.ConnectUsingSettings(Utils.GAME_VERSION);
	}

}
