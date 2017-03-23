using UnityEngine;
using System.Collections;

public class Health : Photon.MonoBehaviour {

	public float maxHealth;

	private float currentHealth;

	void Awake() {
		currentHealth = maxHealth;
	}

	public float CurrentHealth {
		get {
			return currentHealth;
		}
	}

	public void Damage(float damage) {
		if (!photonView.isMine) {
			return;
		}

		int damageTime = PhotonNetwork.ServerTimestamp + Utils.RPC_SYNC_DELAY;
		photonView.RPC("RpcDamage", PhotonTargets.AllViaServer, damageTime, damage);
	}

	[PunRPC]
	private void RpcDamage(int damageTime, float damage) {
		float secondsToDamage = (damageTime - PhotonNetwork.ServerTimestamp) / 1000.0f;
		StartCoroutine(WaitForDamage(secondsToDamage, damage));
	}

	private IEnumerator WaitForDamage(float secondsToDamage, float damage) {
		if (secondsToDamage > 0.0f) {
			yield return new WaitForSecondsRealtime(secondsToDamage);
		}

		currentHealth = Mathf.Clamp(currentHealth - damage, 0.0f, maxHealth);
	}

}
