using UnityEngine;
using System.Collections;

public class Health : Photon.MonoBehaviour {

	public delegate void HealthUpdateCallback(float currentHealth, float maxHealth);
	public delegate void DeathCallback();

	public float maxHealth;

	private float currentHealth;
	private bool isDead;
	private PhotonPlayer lastDamager;
	private HealthUpdateCallback healthUpdateCallback;
	private DeathCallback deathCallback;

	void Awake() {
		currentHealth = maxHealth;
		isDead = false;

		if (photonView.isMine) {
			healthUpdateCallback(currentHealth, maxHealth);
		}
	}

	public float CurrentHealth {
		get {
			return currentHealth;
		}
	}

	public void SetHealthUpdateCallback(HealthUpdateCallback callback) {
		healthUpdateCallback = callback;
	}

	public void SetDeathCallback(DeathCallback callback) {
		deathCallback = callback;
	}

	public void Damage(float damage, PhotonPlayer damager) {
		if (!photonView.isMine) {
			return;
		}

		photonView.RPC("RpcDamage", PhotonTargets.All, damage, damager.ID);
	}

	[PunRPC]
	private void RpcDamage(float damage, int damagerId) {
		PhotonPlayer damager = PhotonPlayer.Find(damagerId);
		LocalDamage(damage, damager);
	}

	private void LocalDamage(float damage, PhotonPlayer damager) {
		if (isDead) {
			return;
		}

		currentHealth = Mathf.Clamp(currentHealth - damage, 0.0f, maxHealth);
		lastDamager = damager;

		if (photonView.isMine) {
			if (healthUpdateCallback != null) {
				healthUpdateCallback(currentHealth, maxHealth);
			}

			if (currentHealth == 0.0f) {
				isDead = true;
				if (deathCallback != null) {
					deathCallback();
				}
			}
		}
	}

}
