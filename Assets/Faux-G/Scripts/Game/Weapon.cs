using UnityEngine;

public class Weapon : MonoBehaviour {

	public Sprite crosshairSprite;
	public Vector2 crosshairSize;

	public Mesh weaponMesh;
	public Material weaponMaterial;

	public Vector3 weaponMeshScale;
	public Vector3 weaponMeshRotation;

	public Vector3 weaponPosition;
	public Vector3 aimWeaponPosition;

	public Vector3 cameraPosition;
	public float cameraFieldOfView;

	public Vector3 aimCameraPosition;
	public float aimCameraFieldOfView;

	public Vector3 weaponMuzzlePosition;

	public float cooldown;
	public int ammo;
	public float toggleAimTime;
	public float changeWeaponTime;
	public float reloadTime;

	public ProjectileController prefabBullet;

	public AudioClip fireSound;
	public float fireVolume;

	public AudioClip reloadSound;
	public float reloadVolume;

	// The min/max recoil angle (degrees/projectile) in horizontal and vertical directions
	public Vector2 minRecoil;
	public Vector2 maxRecoil;
	// The recoil recovery angle (degrees/second) in horizontal and vertical directions
	public Vector2 recoilRecovery;

}
