using UnityEngine;

/**
 * This class controls the health behaviour and logic of an entity. It also manages 
 * how damages and deaths are handled within different entities through the declared 
 * callbacks.
 */
public class Health : Photon.MonoBehaviour {

	public delegate void OnHealthUpdatedCallback(float currentHealth, float maxHealth);
	public delegate void OnDeathCallback();

	public float maxHealth;

	private float currentHealth;
	private bool dead;
	private PhotonPlayer lastDamager;
	private OnHealthUpdatedCallback healthUpdatedCallback;
	private OnDeathCallback deathCallback;

	void Awake() {
		currentHealth = maxHealth;
		dead = false;

		if (photonView.isMine) {
			healthUpdatedCallback(currentHealth, maxHealth);
		}
	}

	public float CurrentHealth {
		get {
			return currentHealth;
		}
	}

    public bool Dead {
        get {
            return dead;
        }
    }

    public OnHealthUpdatedCallback HealthUpdatedCallback {
        get {
            return healthUpdatedCallback;
        }

        set {
            healthUpdatedCallback = value;
        }
    }

    public OnDeathCallback DeathCallback {
        get {
            return deathCallback;
        }

        set {
            deathCallback = value;
        }
    }

	public void Damage(float damage, PhotonPlayer damager) {
		if (!photonView.isMine) {
			return;
		}

		photonView.RPC("RpcDamage", PhotonTargets.All, damage, damager.ID);
	}

	private void LocalDamage(float damage, PhotonPlayer damager) {
		
	}

    [PunRPC]
    private void RpcDamage(float damage, int damagerId) {
        if (dead) {
            return;
        }

        PhotonPlayer damagingPlayer = PhotonPlayer.Find(damagerId);

        currentHealth = Mathf.Clamp(currentHealth - damage, 0.0f, maxHealth);
        lastDamager = damagingPlayer;

        if (photonView.isMine) {
            if (healthUpdatedCallback != null) {
                healthUpdatedCallback(currentHealth, maxHealth);
            }

            if (currentHealth == 0.0f) {
                dead = true;
                if (deathCallback != null) {
                    deathCallback();
                }
            }
        }
    }

}
