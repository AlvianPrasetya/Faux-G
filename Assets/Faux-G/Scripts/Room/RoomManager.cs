using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RoomManager : Photon.PunBehaviour {
	
	public PlayerEntry prefabPlayerEntry;

	public Text textRoomName;
	public RectTransform playerList;
	public Button buttonLeave;
	public Button buttonStartMatch;

	private List<PlayerEntry> playerEntries;

	void Awake() {
		PhotonNetwork.automaticallySyncScene = true;

		buttonLeave.onClick.AddListener(LeaveRoom);
		buttonStartMatch.onClick.AddListener(StartMatch);

		playerEntries = new List<PlayerEntry>();
	}

	void Start() {
		textRoomName.text = PhotonNetwork.room.Name;
		RefreshPlayerList();
	}
	
	public override void OnLeftRoom() {
		Logger.Log("Left room");
		SceneManager.LoadScene(Utils.Scene.LOBBY);
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer joiningPlayer) {
		RefreshPlayerList();
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer leavingPlayer) {
		RefreshPlayerList();
	}

	private void RefreshPlayerList() {
		Logger.Log("Refreshing player list");
		PhotonPlayer[] photonPlayer = PhotonNetwork.playerList;
		Logger.Log(photonPlayer.Length + " players found");

		// Clear the previous list of players
		foreach (PlayerEntry playerEntry in playerEntries) {
			Destroy(playerEntry.gameObject);
		}
		playerEntries.Clear();

		foreach (PhotonPlayer player in PhotonNetwork.playerList) {
			PlayerEntry playerEntry = Instantiate(prefabPlayerEntry, playerList, false);
			playerEntry.PlayerName = player.NickName;
			playerEntry.IsRoomMaster = player.IsMasterClient;
			playerEntries.Add(playerEntry);
		}
	}

	private void LeaveRoom() {
		Logger.Log("Leaving room");
		PhotonNetwork.LeaveRoom();
	}

	private void StartMatch() {
		Logger.Log("Starting match");
		PhotonNetwork.LoadLevel(Utils.Scene.GAME);
	}

}
