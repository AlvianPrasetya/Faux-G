using UnityEngine;

public class Weapon : MonoBehaviour {
	
	public ProjectileController prefabBullet;
	public AudioClip fireSound;
	public AudioClip reloadSound;

	public float cooldown;
	public int ammo;
	public float aimTime;
	public Vector3 weaponPosition;
	public Vector3 cameraPosition;
	public float cameraFieldOfView;
	public Vector3 aimWeaponPosition;
	public Vector3 aimCameraPosition;
	public float aimCameraFieldOfView;
	public float reloadTime;

}
