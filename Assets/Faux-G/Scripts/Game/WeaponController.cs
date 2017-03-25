using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponController : Photon.MonoBehaviour {
	
	public Camera playerCamera;
	public List<Weapon> weapons;

	// Cached components
	private AudioSource audioSource;

	private int currentWeaponId;
	private bool[] isOnCooldown;
	private int[] ammo;
	private bool isReloading;
	private Coroutine reloadCoroutine;

	void Awake() {
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null) {
			audioSource = gameObject.AddComponent<AudioSource>();

			audioSource.pitch = Time.timeScale;
			audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
			audioSource.spatialBlend = 1.0f;
			audioSource.spatialize = true;
		}

		currentWeaponId = 0;
		isOnCooldown = new bool[weapons.Count];
		ammo = new int[weapons.Count];
		isReloading = false;
		reloadCoroutine = null;

		for (int i = 0; i < weapons.Count; i++) {
			isOnCooldown[i] = false;
			ammo[i] = weapons[i].ammo;
		}
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

		int shootTime = PhotonNetwork.ServerTimestamp + Utils.RPC_SYNC_DELAY;
		Vector3 shootPosition = playerCamera.transform.position + playerCamera.transform.forward;
		Quaternion shootDirection = Quaternion.LookRotation(playerCamera.transform.forward);
		photonView.RPC("RpcShoot", PhotonTargets.AllViaServer,
			shootTime, currentWeaponId, shootPosition, shootDirection);

		isOnCooldown[currentWeaponId] = true;
		StartCoroutine(WaitForCooldown(currentWeaponId));

		ammo[currentWeaponId] = ammo[currentWeaponId] - 1;
	}

	public void ChangeWeapon(int weaponId) {
		if (weaponId == currentWeaponId) {
			return;
		}

		if (isReloading) {
			CancelReload();
		}

		int changeWeaponTime = PhotonNetwork.ServerTimestamp + Utils.RPC_SYNC_DELAY;
		photonView.RPC("RpcChangeWeapon", PhotonTargets.AllViaServer, changeWeaponTime, weaponId);

		currentWeaponId = weaponId;
	}

	public void Reload() {
		if (isReloading) {
			return;
		}

		int reloadTime = PhotonNetwork.ServerTimestamp + Utils.RPC_SYNC_DELAY;
		photonView.RPC("RpcReload", PhotonTargets.AllViaServer, reloadTime, currentWeaponId);

		isReloading = true;
		reloadCoroutine = StartCoroutine(WaitForReloadComplete(currentWeaponId));
	}

	public void CancelReload() {
		if (!isReloading) {
			return;
		}
		
		int cancelReloadTime = PhotonNetwork.ServerTimestamp + Utils.RPC_SYNC_DELAY;
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
		// Change weapon mesh here
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

}
