using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour {
	
	public PlayerEntry prefabPlayerEntry;

	public Text textRoomName;
	public RectTransform playerList;
	public Button buttonLeave;
	public Button buttonStartMatch;

	private List<PlayerEntry> playerEntries;

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

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

	/*
	 * PHOTON LIFECYCLE
	 */
	
	void OnLeftRoom() {
		Logger.Log("Left room");
		SceneManager.LoadScene(Utils.Scene.LOBBY);
	}

	void OnPhotonPlayerConnected(PhotonPlayer joiningPlayer) {
		RefreshPlayerList();
	}

	void OnPhotonPlayerDisconnected(PhotonPlayer leavingPlayer) {
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
		SceneManager.LoadScene(Utils.Scene.GAME);
	}

}
