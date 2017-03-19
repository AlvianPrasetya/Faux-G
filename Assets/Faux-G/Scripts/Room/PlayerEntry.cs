using UnityEngine;
using UnityEngine.UI;

public class PlayerEntry : MonoBehaviour {

	public Text textPlayerName;
	public Image imageRoomMaster;

	public string PlayerName {
		set {
			textPlayerName.text = value;
		}
	}

	public bool IsRoomMaster {
		set {
			imageRoomMaster.enabled = value;
		}
	}

}
