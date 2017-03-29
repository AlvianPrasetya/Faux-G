using UnityEngine;

public class HitArea : MonoBehaviour {

	public Health health;
	public float damageMultiplier;

	public void Hit(float damage, PhotonPlayer owner) {
		health.Damage(damage * damageMultiplier, owner);
	}

}
