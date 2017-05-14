using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponController : Photon.MonoBehaviour {

	public delegate void AmmoUpdateCallback(int currentHealth, int maxHealth);

	public Transform weaponPivot;
	public Transform weaponTransform;
	public Transform weaponMuzzle;
	public Transform playerHead;
	public Camera playerCamera;
	public List<Weapon> weapons;
	public List<Collider> hitColliders;

	// Cached components
	private AudioSource audioSource;
	private new Rigidbody rigidbody;

	private int currentWeaponId;
	private bool[] isOnCooldown;
	private int[] ammo;
	private bool isAiming;
	private List<Coroutine> toggleAimCoroutines;
	private List<Coroutine> changeWeaponCoroutines;
	private List<Coroutine> reloadCoroutines;
	private bool isReloading;
	private Coroutine reloadCoroutine;
	private Vector2 currentRecoil;
	private AmmoUpdateCallback ammoUpdateCallback;

	void Awake() {
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null) {
			audioSource = gameObject.AddComponent<AudioSource>();

			audioSource.pitch = Time.timeScale;
			audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
			audioSource.spatialBlend = 1.0f;
			audioSource.spatialize = true;
		}
		rigidbody = GetComponent<Rigidbody>();

		toggleAimCoroutines = new List<Coroutine>();
		changeWeaponCoroutines = new List<Coroutine>();
		reloadCoroutines = new List<Coroutine>();

		if (!photonView.isMine) {
			return;
		}

		currentWeaponId = 0;
		isOnCooldown = new bool[weapons.Count];
		ammo = new int[weapons.Count];
		isAiming = false;
		isReloading = false;
		reloadCoroutine = null;

		for (int i = 0; i < weapons.Count; i++) {
			isOnCooldown[i] = false;
			ammo[i] = weapons[i].ammo;
		}
	}

	void Start() {
		if (!photonView.isMine) {
			return;
		}

		ChangeWeapon(currentWeaponId);
	}

	void Update() {
		if (!photonView.isMine) {
			return;
		}

		RecoverRecoil(weapons[currentWeaponId].recoilRecovery * Time.deltaTime);
	}

	public void SetAmmoUpdateCallback(AmmoUpdateCallback callback) {
		ammoUpdateCallback = callback;
	}

	public void Shoot() {
		if (isOnCooldown[currentWeaponId]) {
			return;
		}

		if (ammo[currentWeaponId] == 0) {
			Reload();
			return;
		}

		if (isReloading) {
			return;
		}

		int shootTime = PhotonNetwork.ServerTimestamp;
		Vector3 shootPosition = weaponMuzzle.transform.position;
		Quaternion shootDirection = Quaternion.LookRotation(playerCamera.transform.forward);
		photonView.RPC("RpcShoot", PhotonTargets.All,
			shootTime, currentWeaponId, shootPosition, shootDirection);

		isOnCooldown[currentWeaponId] = true;
		StartCoroutine(WaitForCooldown(currentWeaponId));

		ammo[currentWeaponId] = ammo[currentWeaponId] - 1;
		ammoUpdateCallback(ammo[currentWeaponId], weapons[currentWeaponId].ammo);
	}

	public void ToggleAim() {
		if (isReloading) {
			return;
		}
		
		photonView.RPC("RpcToggleAim", PhotonTargets.All, currentWeaponId, isAiming);

		isAiming = !isAiming;
	}

	public void ChangeWeapon(int weaponId) {
		if (isReloading) {
			CancelReload();
		}

		if (isAiming) {
			ToggleAim();
		}

		photonView.RPC("RpcChangeWeapon", PhotonTargets.All, currentWeaponId, weaponId);

		currentWeaponId = weaponId;
		ammoUpdateCallback(ammo[currentWeaponId], weapons[currentWeaponId].ammo);
	}

	public void CycleWeapon() {
		ChangeWeapon((currentWeaponId + 1) % weapons.Count);
	}

	public void Reload() {
		if (isReloading) {
			return;
		}

		if (isAiming) {
			ToggleAim();
		}
		
		photonView.RPC("RpcReload", PhotonTargets.All, currentWeaponId);

		isReloading = true;
		reloadCoroutine = StartCoroutine(WaitForReloadComplete(currentWeaponId));
	}

	public void CancelReload() {
		if (!isReloading) {
			return;
		}

		photonView.RPC("RpcCancelReload", PhotonTargets.All, currentWeaponId);

		isReloading = false;
		if (reloadCoroutine != null) {
			StopCoroutine(reloadCoroutine);
			reloadCoroutine = null;
		}
	}

	[PunRPC]
	private void RpcShoot(int shootTime, int weaponId, Vector3 shootPosition, Quaternion shootDirection) {
		LocalShoot(shootTime, weaponId, shootPosition, shootDirection);
	}

	private void LocalShoot(int shootTime, int weaponId, Vector3 shootPosition, Quaternion shootDirection) {
		float timeSinceShoot = (PhotonNetwork.ServerTimestamp - shootTime) / 1000.0f;
		Vector3 extrapolatedPosition = shootPosition + shootDirection * Vector3.forward * weapons[weaponId].prefabProjectile.speed * timeSinceShoot;

		RaycastHit[] hitInfos = Physics.RaycastAll(shootPosition, (extrapolatedPosition - shootPosition).normalized, (extrapolatedPosition - shootPosition).magnitude, 1 << Utils.Layer.DETECT_PROJECTILE | 1 << Utils.Layer.TERRAIN);
		float closestHitDistance = Mathf.Infinity;
		GameObject closestHitObject = null;
		foreach (RaycastHit hitInfo in hitInfos) {
			float distanceToObject = (hitInfo.transform.position - extrapolatedPosition).magnitude;
			if (distanceToObject < closestHitDistance) {
				closestHitDistance = distanceToObject;
				closestHitObject = hitInfo.transform.gameObject;
			}
		}

		if (closestHitObject != null) {
			HitArea hitArea = closestHitObject.GetComponent<HitArea>();
			if (hitArea != null) {
				hitArea.Hit(weapons[weaponId].prefabProjectile.maxDamage, photonView.owner);
			}
		}

		ProjectileController projectile = Instantiate(
			weapons[weaponId].prefabProjectile,
			extrapolatedPosition,
			shootDirection
		);
		projectile.SetOwner(photonView.owner, hitColliders);

		if (weapons[weaponId].fireSound != null) {
			audioSource.volume = weapons[weaponId].fireVolume;
			audioSource.PlayOneShot(weapons[weaponId].fireSound);
		}

		if (photonView.isMine) {
			// Only apply recoil to local player since position and rotation data are synced anyway
			ApplyRecoil(Vector2.Lerp(weapons[weaponId].minRecoil, weapons[weaponId].maxRecoil, Random.value));
			// Only apply knockback to local player since position data are synced anyway
			ApplyKnockback(Mathf.Lerp(weapons[weaponId].minKnockback, weapons[weaponId].maxKnockback, Random.value));
		}
	}

	[PunRPC]
	private void RpcToggleAim(int weaponId, bool isAiming) {
		LocalToggleAim(weaponId, isAiming);
	}

	private void LocalToggleAim(int weaponId, bool isAiming) {
		if (toggleAimCoroutines.Count > 0) {
			foreach (Coroutine coroutine in toggleAimCoroutines) {
				StopCoroutine(coroutine);
			}
			toggleAimCoroutines.Clear();
		}

		toggleAimCoroutines.Add(
			StartCoroutine(ToggleAimCoroutine(weaponId, isAiming))
		);
	}

	private IEnumerator ToggleAimCoroutine(int weaponId, bool isAiming) {
		toggleAimCoroutines.Add(
			StartCoroutine(Utils.TransformLerpPosition(
				weaponTransform, 
				weaponTransform.localPosition, 
				(isAiming) ? weapons[weaponId].weaponPosition : weapons[weaponId].aimWeaponPosition, 
				weapons[weaponId].toggleAimTime
			))
		);

		toggleAimCoroutines.Add(
			StartCoroutine(Utils.TransformLerpPosition(
				playerCamera.transform, 
				playerCamera.transform.localPosition, 
				(isAiming) ? weapons[weaponId].cameraPosition : weapons[weaponId].aimCameraPosition, 
				weapons[weaponId].toggleAimTime
			))
		);

		Coroutine blockingCoroutine = StartCoroutine(Utils.CameraLerpFieldOfView(
			playerCamera,
			playerCamera.fieldOfView,
			(isAiming) ? weapons[weaponId].cameraFieldOfView : weapons[weaponId].aimCameraFieldOfView,
			weapons[weaponId].toggleAimTime
		));
		toggleAimCoroutines.Add(blockingCoroutine);
		yield return blockingCoroutine;

		toggleAimCoroutines.Clear();
	}

	[PunRPC]
	private void RpcChangeWeapon(int startWeaponId, int endWeaponId) {
		LocalChangeWeapon(startWeaponId, endWeaponId);
	}

	private void LocalChangeWeapon(int startWeaponId, int endWeaponId) {
		if (changeWeaponCoroutines.Count > 0) {
			foreach (Coroutine coroutine in changeWeaponCoroutines) {
				StopCoroutine(coroutine);
			}
			changeWeaponCoroutines.Clear();
		}

		changeWeaponCoroutines.Add(
			StartCoroutine(ChangeWeaponCoroutine(startWeaponId, endWeaponId))
		);
	}

	private IEnumerator ChangeWeaponCoroutine(int startWeaponId, int endWeaponId) {
		Coroutine firstBlockingCoroutine = StartCoroutine(Utils.TransformSlerpRotation(
			weaponPivot,
			weaponPivot.localRotation,
			Quaternion.Euler(45.0f, 0.0f, 0.0f),
			weapons[startWeaponId].changeWeaponTime
		));
		changeWeaponCoroutines.Add(firstBlockingCoroutine);
		yield return firstBlockingCoroutine;

		if (photonView.isMine) {
			GameManager.Instance.Crosshair.sprite = weapons[endWeaponId].crosshairSprite;
			GameManager.Instance.Crosshair.rectTransform.sizeDelta = weapons[endWeaponId].crosshairSize;
		}
		weaponTransform.localScale = weapons[endWeaponId].weaponMeshScale;
		weaponTransform.localRotation = Quaternion.Euler(weapons[endWeaponId].weaponMeshRotation);
		weaponTransform.GetComponent<MeshFilter>().mesh = weapons[endWeaponId].weaponMesh;
		weaponTransform.GetComponent<MeshRenderer>().material = weapons[endWeaponId].weaponMaterial;
		weaponMuzzle.localPosition = weapons[endWeaponId].weaponMuzzlePosition;

		changeWeaponCoroutines.Add(
			StartCoroutine(Utils.TransformLerpPosition(
				weaponTransform, 
				weaponTransform.localPosition,
				weapons[endWeaponId].weaponPosition,
				weapons[endWeaponId].changeWeaponTime
			))
		);

		changeWeaponCoroutines.Add(
			StartCoroutine(Utils.TransformSlerpRotation(
				weaponPivot,
				weaponPivot.localRotation,
				Quaternion.Euler(0.0f, 0.0f, 0.0f),
				weapons[endWeaponId].changeWeaponTime
			))
		);

		changeWeaponCoroutines.Add(
			StartCoroutine(Utils.TransformLerpPosition(
				playerCamera.transform,
				playerCamera.transform.localPosition,
				weapons[endWeaponId].cameraPosition,
				weapons[endWeaponId].changeWeaponTime
			))
		);

		Coroutine secondBlockingCoroutine = StartCoroutine(Utils.CameraLerpFieldOfView(
			playerCamera, 
			playerCamera.fieldOfView,
			weapons[endWeaponId].cameraFieldOfView,
			weapons[endWeaponId].changeWeaponTime
		));
		changeWeaponCoroutines.Add(secondBlockingCoroutine);
		yield return secondBlockingCoroutine;

		changeWeaponCoroutines.Clear();
	}

	[PunRPC]
	private void RpcReload(int weaponId) {
		LocalReload(weaponId);
	}

	private void LocalReload(int weaponId) {
		if (weapons[weaponId].reloadSound != null) {
			audioSource.volume = weapons[weaponId].reloadVolume;
			audioSource.PlayOneShot(weapons[weaponId].reloadSound);
		}

		reloadCoroutines.Add(StartCoroutine(ReloadCoroutine(weaponId)));
	}

	private IEnumerator ReloadCoroutine(int weaponId) {
		Coroutine blockingCoroutine = StartCoroutine(Utils.TransformSlerpRotation(
			weaponPivot,
			weaponPivot.localRotation,
			Quaternion.Euler(45.0f, 0.0f, 0.0f),
			weapons[weaponId].changeWeaponTime
		));
		reloadCoroutines.Add(blockingCoroutine);
		yield return blockingCoroutine;

		yield return new WaitForSeconds(weapons[weaponId].reloadTime - 2 * weapons[weaponId].changeWeaponTime);

		Coroutine secondBlockingCoroutine = StartCoroutine(Utils.TransformSlerpRotation(
			weaponPivot,
			weaponPivot.localRotation,
			Quaternion.Euler(0.0f, 0.0f, 0.0f),
			weapons[weaponId].changeWeaponTime
		));
		reloadCoroutines.Add(secondBlockingCoroutine);
		yield return secondBlockingCoroutine;

		reloadCoroutines.Clear();
	}

	[PunRPC]
	private void RpcCancelReload(int weaponId) {
		LocalCancelReload(weaponId);
	}

	private void LocalCancelReload(int weaponId) {
		if (weapons[weaponId].reloadSound != null) {
			audioSource.Stop();
		}

		if (reloadCoroutines.Count > 0) {
			foreach (Coroutine coroutine in reloadCoroutines) {
				StopCoroutine(coroutine);
			}
			reloadCoroutines.Clear();
		}

		StartCoroutine(CancelReloadCoroutine(weaponId));
	}

	private IEnumerator CancelReloadCoroutine(int weaponId) {
		yield return StartCoroutine(Utils.TransformSlerpRotation(
			weaponPivot,
			weaponPivot.localRotation,
			Quaternion.Euler(0.0f, 0.0f, 0.0f),
			weapons[weaponId].changeWeaponTime
		));
	}

	private IEnumerator WaitForCooldown(int weaponId) {
		yield return new WaitForSeconds(weapons[weaponId].cooldown);

		isOnCooldown[weaponId] = false;
	}

	private IEnumerator WaitForReloadComplete(int weaponId) {
		yield return new WaitForSeconds(weapons[weaponId].reloadTime);

		ammo[weaponId] = weapons[weaponId].ammo;
		ammoUpdateCallback(ammo[currentWeaponId], weapons[currentWeaponId].ammo);
		isReloading = false;
		reloadCoroutine = null;
	}

	private void ApplyRecoil(Vector2 angle) {
		currentRecoil = currentRecoil + angle;
		transform.Rotate(transform.up, angle.x, Space.World);
		playerHead.Rotate(-playerHead.right, angle.y, Space.World);
	}

	private void RecoverRecoil(Vector2 angle) {
		Vector2 clampedAngle = Vector2.Min(
			angle, 
			new Vector2(Mathf.Abs(currentRecoil.x), Mathf.Abs(currentRecoil.y))
		);

		Vector2 deltaAngle = new Vector2(
			(currentRecoil.x > 0) ? -clampedAngle.x : clampedAngle.x, 
			(currentRecoil.y > 0) ? -clampedAngle.y : clampedAngle.y
		);

		currentRecoil = currentRecoil + deltaAngle;
		transform.Rotate(transform.up, deltaAngle.x, Space.World);
		playerHead.Rotate(-playerHead.right, deltaAngle.y, Space.World);
	}

	private void ApplyKnockback(float knockback) {
		// Direction of knockback is to the back of the weapon
		rigidbody.AddForce(
			-weaponPivot.forward * rigidbody.mass * knockback, 
			ForceMode.Impulse
		);
	}

}
