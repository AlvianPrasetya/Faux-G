using UnityEngine;
using UnityEngine.UI;

public class RoomEntry : MonoBehaviour {

	public Text textRoomNo;
	public Text textRoomName;
	public Text textGameMode;
	public Text textNumPlayers;

	private static readonly string formatNumPlayers = "{0} / {1}";
	
	public int RoomNo {
		set {
			textRoomNo.text = value.ToString();
		}
	}

	public string RoomName {
		set {
			textRoomName.text = value;
		}
	}

	public GameMode GameMode {
		set {
			textGameMode.text = value.ToString();
		}
	}

	public void SetNumPlayers(int numPlayers, int maxPlayers) {
		textNumPlayers.text = string.Format(formatNumPlayers, numPlayers, maxPlayers);
	}

	void Awake() {
		GetComponent<Button>().onClick.AddListener(JoinRoom);
	}

	private void JoinRoom() {
		PhotonNetwork.JoinRoom(textRoomName.text);
	}

}
