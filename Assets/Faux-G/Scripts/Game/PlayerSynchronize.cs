using UnityEngine;

public class PlayerSynchronize : Photon.MonoBehaviour {

	private Vector3 lastPosition;
	private Vector3 newPosition;
	private Quaternion lastRotation;
	private Quaternion newRotation;

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
		} else {
			Vector3 newPosition = (Vector3) stream.ReceiveNext();
			Quaternion newRotation = (Quaternion) stream.ReceiveNext();
		}
	}

	void Update() {

	}

}
