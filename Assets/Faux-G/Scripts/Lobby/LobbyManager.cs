using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbyManager : Photon.PunBehaviour {

	public RoomEntry prefabRoomEntry;

	public Button buttonRoomNumber;
	public Button buttonRoomName;
	public Button buttonGameMode;
	public Button buttonNumPlayers;
	public RectTransform roomList;
	public Text textNoRooms;
	public Button buttonCreateNewRoom;

	// Create room overlay
	public GameObject overlayCreateRoom;
	public InputField inputRoomName;
	public Dropdown dropdownGameMode;
	public Dropdown dropdownMaxPlayers;
	public Button buttonCancel;
	public Button buttonCreate;

	public float roomRefreshDelay;
	
	private List<RoomEntry> roomEntries;

	void Awake() {
		buttonRoomNumber.onClick.AddListener(SortByRoomNumber);
		buttonRoomName.onClick.AddListener(SortByRoomName);
		buttonGameMode.onClick.AddListener(SortByGameMode);
		buttonNumPlayers.onClick.AddListener(SortByNumPlayers);
		buttonCreateNewRoom.onClick.AddListener(CreateNewRoom);

		buttonCancel.onClick.AddListener(CancelRoom);
		buttonCreate.onClick.AddListener(CreateRoom);

		roomEntries = new List<RoomEntry>();
	}

	void Start() {
		RefreshRoomList();
	}

	public override void OnReceivedRoomListUpdate() {
		RefreshRoomList();
	}
	
	public override void OnCreatedRoom() {
		Logger.Log("Created room");
	}

	public override void OnPhotonCreateRoomFailed(object[] codeAndMsg) {
		Logger.Log("Failed to create room");
	}

	public override void OnJoinedRoom() {
		Logger.Log("Joined room");
		SceneManager.LoadScene(Utils.Scene.ROOM);
	}

	public override void OnPhotonJoinRoomFailed(object[] codeAndMsg) {
		Logger.Log("Failed to join room");
	}

	private void SortByRoomNumber() {
		Logger.Log("Sorting by room number");
	}

	private void SortByRoomName() {
		Logger.Log("Sorting by room name");
	}

	private void SortByGameMode() {
		Logger.Log("Sorting by game mode");
	}

	private void SortByNumPlayers() {
		Logger.Log("Sorting by num players");
	}

	private void RefreshRoomList() {
		Logger.Log("Refreshing room list");
		RoomInfo[] roomsInfo = PhotonNetwork.GetRoomList();
		Logger.Log(roomsInfo.Length + " rooms found");

		// Clear the previous list of rooms
		foreach (RoomEntry roomEntry in roomEntries) {
			Destroy(roomEntry.gameObject);
		}
		roomEntries.Clear();

		if (roomsInfo.Length == 0) {
			textNoRooms.enabled = true;
		} else {
			textNoRooms.enabled = false;
			// List out all the rooms found
			int ctr = 0;
			foreach (RoomInfo roomInfo in roomsInfo) {
				RoomEntry roomEntry = Instantiate(prefabRoomEntry, roomList, false);
				roomEntry.RoomNo = ++ctr;
				roomEntry.RoomName = roomInfo.Name;
				roomEntry.GameMode = (GameMode) roomInfo.CustomProperties[Utils.Key.GAME_MODE];
				roomEntry.SetNumPlayers(roomInfo.PlayerCount, roomInfo.MaxPlayers);
				roomEntries.Add(roomEntry);
			}
		}
	}

	private void CreateNewRoom() {
		overlayCreateRoom.SetActive(true);
	}

	private void CancelRoom() {
		inputRoomName.text = "";
		dropdownGameMode.value = 0;
		dropdownMaxPlayers.value = 0;

		overlayCreateRoom.SetActive(false);
	}

	private void CreateRoom() {
		Logger.Log("Creating and joining room");

		RoomOptions roomOptions = new RoomOptions();
		roomOptions.IsOpen = true;
		roomOptions.IsVisible = true;
		roomOptions.MaxPlayers = (byte) (dropdownMaxPlayers.value * 2 + 2);

		ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable();
		customRoomProperties.Add(Utils.Key.GAME_MODE, (GameMode) dropdownGameMode.value);
		roomOptions.CustomRoomProperties = customRoomProperties;
		roomOptions.CustomRoomPropertiesForLobby = new string[] { Utils.Key.GAME_MODE };

		PhotonNetwork.CreateRoom(inputRoomName.text, roomOptions, TypedLobby.Default);
	}

}
