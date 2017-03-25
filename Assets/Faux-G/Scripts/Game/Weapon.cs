using UnityEngine;

public class Weapon : MonoBehaviour {
	
	public ProjectileController prefabBullet;
	public AudioClip fireSound;
	public AudioClip reloadSound;

	public float cooldown;
	public int ammo;
	public float reloadTime;

}
