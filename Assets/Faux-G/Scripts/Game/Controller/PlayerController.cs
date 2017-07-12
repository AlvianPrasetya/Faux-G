using UnityEngine;

public class PlayerController : Photon.MonoBehaviour {

	public Camera playerCamera;
    
	private new Rigidbody rigidbody;
	private GravityBody gravityBody;
	private Health health;

	public string NickName {
        get {
            return photonView.owner.NickName;
        }
    }

    public float CurrentHealth {
        get {
            return health.CurrentHealth;
        }
    }

	void Awake() {
		rigidbody = GetComponent<Rigidbody>();
		gravityBody = GetComponent<GravityBody>();
		health = GetComponent<Health>();
		health.SetHealthUpdateCallback(UIManager.Instance.UpdateHealthText);
		health.SetDeathCallback(GameManager.Instance.Respawn);

        if (!photonView.isMine) {
            // Disable camera on remote instances
            playerCamera.gameObject.SetActive(false);
            
            // Disable physics on remote instances
            rigidbody.isKinematic = true;
            gravityBody.enabled = false;
        }
	}

}
