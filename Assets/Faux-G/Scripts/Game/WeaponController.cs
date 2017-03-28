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
	private Coroutine changeWeaponCoroutine;
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
		changeWeaponCoroutine = null;
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
		if (isReloading) {
			CancelReload();
		}

		if (isAiming) {
			ToggleAim();
		}

		int changeWeaponTime = PhotonNetwork.ServerTimestamp + Utils.SYNC_DELAY;
		photonView.RPC("RpcChangeWeapon", PhotonTargets.AllViaServer, changeWeaponTime, currentWeaponId, weaponId);

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
			audioSource.volume = weapons[weaponId].fireVolume;
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

		toggleAimCoroutine = StartCoroutine(ToggleAimCoroutine(weaponId, isAiming));
	}

	private IEnumerator ToggleAimCoroutine(int weaponId, bool isAiming) {
		StartCoroutine(TransformLerpPosition(
			weaponTransform, 
			weaponTransform.localPosition, 
			(isAiming) ? weapons[weaponId].weaponPosition : weapons[weaponId].aimWeaponPosition, 
			weapons[weaponId].toggleAimTime
		));

		StartCoroutine(TransformLerpPosition(
			playerCamera.transform, 
			playerCamera.transform.localPosition, 
			(isAiming) ? weapons[weaponId].cameraPosition : weapons[weaponId].aimCameraPosition, 
			weapons[weaponId].toggleAimTime
		));

		yield return StartCoroutine(CameraLerpFieldOfView(
			playerCamera,
			playerCamera.fieldOfView,
			(isAiming) ? weapons[weaponId].cameraFieldOfView : weapons[weaponId].aimCameraFieldOfView,
			weapons[weaponId].toggleAimTime
		));

		toggleAimCoroutine = null;
	}

	[PunRPC]
	private void RpcChangeWeapon(int changeWeaponTime, int startWeaponId, int endWeaponId) {
		float secondsToChangeWeapon = (changeWeaponTime - PhotonNetwork.ServerTimestamp) / 1000.0f;
		StartCoroutine(WaitForChangeWeapon(secondsToChangeWeapon, startWeaponId, endWeaponId));
	}

	private IEnumerator WaitForChangeWeapon(float secondsToChangeWeapon, int startWeaponId, int endWeaponId) {
		if (secondsToChangeWeapon > 0.0f) {
			yield return new WaitForSecondsRealtime(secondsToChangeWeapon);
		}

		LocalChangeWeapon(startWeaponId, endWeaponId);
	}

	private void LocalChangeWeapon(int startWeaponId, int endWeaponId) {
		if (changeWeaponCoroutine != null) {
			StopCoroutine(changeWeaponCoroutine);
		}

		changeWeaponCoroutine = StartCoroutine(ChangeWeaponCoroutine(startWeaponId, endWeaponId));
	}

	private IEnumerator ChangeWeaponCoroutine(int startWeaponId, int endWeaponId) {
		yield return StartCoroutine(TransformSlerpRotation(
			weaponTransform, 
			weaponTransform.localRotation,
			Quaternion.Euler(45.0f, 0.0f, 0.0f),
			weapons[startWeaponId].changeWeaponTime
		));

		GameManager.Instance.Crosshair.sprite = weapons[endWeaponId].crosshairSprite;
		GameManager.Instance.Crosshair.rectTransform.sizeDelta = weapons[endWeaponId].crosshairSize;
		weaponTransform.GetComponentInChildren<MeshFilter>().mesh = weapons[endWeaponId].weaponMesh;
		weaponTransform.GetComponentInChildren<MeshRenderer>().material = weapons[endWeaponId].weaponMaterial;

		StartCoroutine(TransformLerpPosition(
			weaponTransform, 
			weaponTransform.localPosition,
			weapons[endWeaponId].weaponPosition,
			weapons[endWeaponId].changeWeaponTime
		));

		StartCoroutine(TransformSlerpRotation(
			weaponTransform, 
			weaponTransform.localRotation,
			Quaternion.Euler(0.0f, 0.0f, 0.0f),
			weapons[endWeaponId].changeWeaponTime
		));

		StartCoroutine(TransformLerpPosition(
			playerCamera.transform,
			playerCamera.transform.localPosition,
			weapons[endWeaponId].cameraPosition,
			weapons[endWeaponId].changeWeaponTime
		));

		yield return StartCoroutine(CameraLerpFieldOfView(
			playerCamera, 
			playerCamera.fieldOfView,
			weapons[endWeaponId].cameraFieldOfView,
			weapons[endWeaponId].changeWeaponTime
		));

		changeWeaponCoroutine = null;
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

	/*
	 * UTILITY METHODS
	 */

	private IEnumerator TransformLerpPosition(Transform targetTransform, Vector3 startPosition,
		Vector3 endPosition, float lerpTime) {
		float time = 0.0f;
		while (time < 1.0f) {
			targetTransform.localPosition = Vector3.Lerp(
				startPosition, 
				endPosition, 
				time
			);
			
			time += Time.deltaTime / lerpTime;
			yield return null;
		}

		targetTransform.localPosition = endPosition;
	}

	private IEnumerator TransformSlerpRotation(Transform targetTransform, Quaternion startRotation,
		Quaternion endRotation, float slerpTime) {
		float time = 0.0f;
		while (time < 1.0f) {
			targetTransform.localRotation = Quaternion.Slerp(
				startRotation, 
				endRotation, 
				time
			);

			time += Time.deltaTime / slerpTime;
			yield return null;
		}

		targetTransform.localRotation = endRotation;
	}

	private IEnumerator CameraLerpFieldOfView(Camera targetCamera, float startFieldOfView,
		float endFieldOfView, float lerpTime) {
		float time = 0.0f;
		while (time < 1.0f) {
			targetCamera.fieldOfView = Mathf.Lerp(
				startFieldOfView,
				endFieldOfView,
				time
			);

			time += Time.deltaTime / lerpTime;
			yield return null;
		}

		targetCamera.fieldOfView = endFieldOfView;
	}

}
