using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponController : Photon.MonoBehaviour {

	public Transform weaponTransform;
	public Transform weaponMuzzle;
	public Transform playerHead;
	public Camera playerCamera;
	public List<Weapon> weapons;

	// Cached components
	private AudioSource audioSource;

	private int currentWeaponId;
	private bool[] isOnCooldown;
	private int[] ammo;
	private bool isAiming;
	private Coroutine toggleAimCoroutine;
	private bool isReloading;
	private Coroutine reloadCoroutine;
	private Vector2 currentRecoil;

	void Awake() {
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null) {
			audioSource = gameObject.AddComponent<AudioSource>();

			audioSource.pitch = Time.timeScale;
			audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
			audioSource.spatialBlend = 1.0f;
			audioSource.spatialize = true;
		}

		if (!photonView.isMine) {
			return;
		}

		currentWeaponId = 0;
		isOnCooldown = new bool[weapons.Count];
		ammo = new int[weapons.Count];
		isAiming = false;
		toggleAimCoroutine = null;
		isReloading = false;
		reloadCoroutine = null;

		for (int i = 0; i < weapons.Count; i++) {
			isOnCooldown[i] = false;
			ammo[i] = weapons[i].ammo;
		}
	}

	void Update() {
		if (!photonView.isMine) {
			return;
		}

		RecoverRecoil(weapons[currentWeaponId].recoilRecovery * Time.deltaTime);
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

		int shootTime = PhotonNetwork.ServerTimestamp + Utils.SYNC_DELAY;
		Vector3 shootPosition = weaponMuzzle.transform.position;
		Quaternion shootDirection = Quaternion.LookRotation(playerCamera.transform.forward);
		photonView.RPC("RpcShoot", PhotonTargets.AllViaServer,
			shootTime, currentWeaponId, shootPosition, shootDirection);

		isOnCooldown[currentWeaponId] = true;
		StartCoroutine(WaitForCooldown(currentWeaponId));

		ammo[currentWeaponId] = ammo[currentWeaponId] - 1;
	}

	public void ToggleAim() {
		if (isReloading) {
			return;
		}

		int toggleAimTime = PhotonNetwork.ServerTimestamp + Utils.SYNC_DELAY;
		photonView.RPC("RpcToggleAim", PhotonTargets.AllViaServer, 
			toggleAimTime, currentWeaponId, isAiming);

		isAiming = !isAiming;
	}

	public void ChangeWeapon(int weaponId) {
		if (weaponId == currentWeaponId) {
			return;
		}

		if (isReloading) {
			CancelReload();
		}

		if (isAiming) {
			ToggleAim();
		}

		int changeWeaponTime = PhotonNetwork.ServerTimestamp + Utils.SYNC_DELAY;
		photonView.RPC("RpcChangeWeapon", PhotonTargets.AllViaServer, changeWeaponTime, weaponId);

		currentWeaponId = weaponId;
	}

	public void Reload() {
		if (isReloading) {
			return;
		}

		if (isAiming) {
			ToggleAim();
		}

		int reloadTime = PhotonNetwork.ServerTimestamp + Utils.SYNC_DELAY;
		photonView.RPC("RpcReload", PhotonTargets.AllViaServer, reloadTime, currentWeaponId);

		isReloading = true;
		reloadCoroutine = StartCoroutine(WaitForReloadComplete(currentWeaponId));
	}

	public void CancelReload() {
		if (!isReloading) {
			return;
		}
		
		int cancelReloadTime = PhotonNetwork.ServerTimestamp + Utils.SYNC_DELAY;
		photonView.RPC("RpcCancelReload", PhotonTargets.AllViaServer, cancelReloadTime, currentWeaponId);

		isReloading = false;
		if (reloadCoroutine != null) {
			StopCoroutine(reloadCoroutine);
			reloadCoroutine = null;
		}
	}

	[PunRPC]
	private void RpcShoot(int shootTime, int weaponId, Vector3 shootPosition, Quaternion shootDirection) {
		float secondsToShoot = (shootTime - PhotonNetwork.ServerTimestamp) / 1000.0f;
		StartCoroutine(WaitForShoot(secondsToShoot, weaponId, shootPosition, shootDirection));
	}

	private IEnumerator WaitForShoot(float secondsToShoot, int weaponId, Vector3 shootPosition, Quaternion shootDirection) {
		if (secondsToShoot > 0.0f) {
			yield return new WaitForSecondsRealtime(secondsToShoot);
		}

		LocalShoot(weaponId, shootPosition, shootDirection);
	}

	private void LocalShoot(int weaponId, Vector3 shootPosition, Quaternion shootDirection) {
		Instantiate(
			weapons[weaponId].prefabBullet,
			shootPosition,
			shootDirection
		);

		if (weapons[weaponId].fireSound != null) {
			audioSource.PlayOneShot(weapons[weaponId].fireSound);
		}

		if (photonView.isMine) {
			// Only apply recoil to local player since position and rotation data are synced anyway
			ApplyRecoil(Vector2.Lerp(weapons[weaponId].minRecoil, weapons[weaponId].maxRecoil, Random.value));
		}
	}

	[PunRPC]
	private void RpcToggleAim(int toggleAimTime, int weaponId, bool isAiming) {
		float secondsToToggleAim = (toggleAimTime - PhotonNetwork.ServerTimestamp) / 1000.0f;
		StartCoroutine(WaitForToggleAim(secondsToToggleAim, weaponId, isAiming));
	}

	private IEnumerator WaitForToggleAim(float secondsToToggleAim, int weaponId, bool isAiming) {
		if (secondsToToggleAim > 0.0f) {
			yield return new WaitForSecondsRealtime(secondsToToggleAim);
		}

		LocalToggleAim(weaponId, isAiming);
	}

	private void LocalToggleAim(int weaponId, bool isAiming) {
		if (toggleAimCoroutine != null) {
			StopCoroutine(toggleAimCoroutine);
		}

		if (isAiming) {
			// Aiming to not aiming
			toggleAimCoroutine = StartCoroutine(AimCoroutine(
				weapons[weaponId].aimTime, 
				weapons[weaponId].weaponPosition, 
				weapons[weaponId].cameraPosition, 
				weapons[weaponId].cameraFieldOfView
			));
		} else {
			// Not aiming to aiming
			toggleAimCoroutine = StartCoroutine(AimCoroutine(
				weapons[weaponId].aimTime, 
				weapons[weaponId].aimWeaponPosition,
				weapons[weaponId].aimCameraPosition,
				weapons[weaponId].aimCameraFieldOfView
			));
		}
	}

	private IEnumerator AimCoroutine(float aimTime, Vector3 weaponEndPosition, Vector3 cameraEndPosition, float cameraEndFieldOfView) {
		Vector3 weaponStartPosition = weaponTransform.localPosition;
		Vector3 cameraStartPosition = playerCamera.transform.localPosition;
		float cameraStartFieldOfView = playerCamera.fieldOfView;

		float time = 0.0f;
		while (time < aimTime) {
			weaponTransform.localPosition = Vector3.Lerp(weaponStartPosition, weaponEndPosition, time / aimTime);
			playerCamera.transform.localPosition = Vector3.Lerp(cameraStartPosition, cameraEndPosition, time / aimTime);
			playerCamera.fieldOfView = Mathf.Lerp(cameraStartFieldOfView, cameraEndFieldOfView, time / aimTime);
			time += Time.deltaTime;
			yield return null;
		}
		weaponTransform.localPosition = weaponEndPosition;
		playerCamera.transform.localPosition = cameraEndPosition;
		playerCamera.fieldOfView = cameraEndFieldOfView;

		toggleAimCoroutine = null;
	}

	[PunRPC]
	private void RpcChangeWeapon(int changeWeaponTime, int weaponId) {
		float secondsToChangeWeapon = (changeWeaponTime - PhotonNetwork.ServerTimestamp) / 1000.0f;
		StartCoroutine(WaitForChangeWeapon(secondsToChangeWeapon, weaponId));
	}

	private IEnumerator WaitForChangeWeapon(float secondsToChangeWeapon, int weaponId) {
		if (secondsToChangeWeapon > 0.0f) {
			yield return new WaitForSecondsRealtime(secondsToChangeWeapon);
		}

		LocalChangeWeapon(weaponId);
	}

	private void LocalChangeWeapon(int weaponId) {
		weaponTransform.GetComponent<MeshFilter>().mesh = weapons[weaponId].weaponMesh;
		weaponTransform.GetComponent<MeshRenderer>().material = weapons[weaponId].weaponMaterial;
		weaponTransform.localPosition = weapons[weaponId].weaponPosition;
		playerCamera.transform.localPosition = weapons[weaponId].cameraPosition;
		playerCamera.fieldOfView = weapons[weaponId].cameraFieldOfView;
	}

	[PunRPC]
	private void RpcReload(int reloadTime, int weaponId) {
		float secondsToReload = (reloadTime - PhotonNetwork.ServerTimestamp) / 1000.0f;
		StartCoroutine(WaitForReload(secondsToReload, weaponId));
	}

	private IEnumerator WaitForReload(float secondsToReload, int weaponId) {
		if (secondsToReload > 0.0f) {
			yield return new WaitForSecondsRealtime(secondsToReload);
		}

		LocalReload(weaponId);
	}

	private void LocalReload(int weaponId) {
		if (weapons[weaponId].reloadSound != null) {
			audioSource.PlayOneShot(weapons[weaponId].reloadSound);
		}
	}

	[PunRPC]
	private void RpcCancelReload(int cancelReloadTime, int weaponId) {
		float secondsToCancelReload = (cancelReloadTime - PhotonNetwork.ServerTimestamp) / 1000.0f;
		StartCoroutine(WaitForCancelReload(secondsToCancelReload, weaponId));
	}

	private IEnumerator WaitForCancelReload(float secondsToCancelReload, int weaponId) {
		if (secondsToCancelReload > 0.0f) {
			yield return new WaitForSecondsRealtime(secondsToCancelReload);
		}

		LocalCancelReload(weaponId);
	}

	private void LocalCancelReload(int weaponId) {
		if (weapons[weaponId].reloadSound != null) {
			audioSource.Stop();
		}
	}

	private IEnumerator WaitForCooldown(int weaponId) {
		yield return new WaitForSeconds(weapons[weaponId].cooldown);

		isOnCooldown[weaponId] = false;
	}

	private IEnumerator WaitForReloadComplete(int weaponId) {
		yield return new WaitForSeconds(weapons[weaponId].reloadTime);

		ammo[weaponId] = weapons[weaponId].ammo;
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

}
