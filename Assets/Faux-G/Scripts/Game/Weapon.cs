using UnityEngine;

public class Weapon : MonoBehaviour {

	public Sprite crosshairSprite;
	public Mesh weaponMesh;
	public Material weaponMaterial;
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

	// The min/max recoil angle (degrees/projectile) in horizontal and vertical directions
	public Vector2 minRecoil;
	public Vector2 maxRecoil;
	// The recoil recovery angle (degrees/second) in horizontal and vertical directions
	public Vector2 recoilRecovery;

}
