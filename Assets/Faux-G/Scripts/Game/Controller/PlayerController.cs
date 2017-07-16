using UnityEngine;

public class PlayerController : Photon.MonoBehaviour {

	public Camera playerCamera;
    
	private new Rigidbody rigidbody;
	private GravityBody gravityBody;

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
		gravityBody = GetComponent<GravityBody>();

        if (photonView.isMine) {
            // Enable camera on local instance
            playerCamera.gameObject.SetActive(true);

            // Enable physics on local instance
            rigidbody.isKinematic = false;
            gravityBody.enabled = true;
        } else {
            // Disable camera on remote instances
            playerCamera.gameObject.SetActive(false);

            // Disable physics on remote instances
            rigidbody.isKinematic = true;
            gravityBody.enabled = false;
        }
	}

    public string NickName {
        get {
            return photonView.owner.NickName;
        }
    }

}
