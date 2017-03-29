using UnityEngine;
using System.Collections;

public class Health : Photon.MonoBehaviour {

	public delegate void HealthUpdateCallback(float health);
	public delegate void DeathCallback();

	public float maxHealth;

	private float currentHealth;
	private PhotonPlayer lastDamager;
	private HealthUpdateCallback healthUpdateCallback;
	private DeathCallback deathCallback;

	void Awake() {
		currentHealth = maxHealth;

		if (photonView.isMine) {
			healthUpdateCallback(currentHealth);
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

		int damageTime = PhotonNetwork.ServerTimestamp + Utils.SYNC_DELAY;
		photonView.RPC("RpcDamage", PhotonTargets.AllViaServer, damageTime, damage, damager.ID);
	}

	[PunRPC]
	private void RpcDamage(int damageTime, float damage, int damagerId) {
		float secondsToDamage = (damageTime - PhotonNetwork.ServerTimestamp) / 1000.0f;
		PhotonPlayer damager = PhotonPlayer.Find(damagerId);
		StartCoroutine(WaitForDamage(secondsToDamage, damage, damager));
	}

	private IEnumerator WaitForDamage(float secondsToDamage, float damage, PhotonPlayer damager) {
		if (secondsToDamage > 0.0f) {
			yield return new WaitForSecondsRealtime(secondsToDamage);
		}

		currentHealth = Mathf.Clamp(currentHealth - damage, 0.0f, maxHealth);
		lastDamager = damager;

		if (photonView.isMine) {
			if (healthUpdateCallback != null) {
				healthUpdateCallback(currentHealth);
			}
			
			if (currentHealth == 0.0f && deathCallback != null) {
				deathCallback();
			}
		}
	}

}
