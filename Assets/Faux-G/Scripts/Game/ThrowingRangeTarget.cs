using UnityEngine;
using System.Collections;

public class ThrowingRangeTarget : Photon.MonoBehaviour {

	public delegate void OnTargetHitCallback(PhotonPlayer hittingPlayer, int points);

	public float maxRotateSpeed;
	public float changeAngularVelocityInterval;
	public float knockbackDistance;
	public Vector3 minPosition;
	public Vector3 maxPosition;

	public HitArea[] hitAreas;

	private Vector3 angularVelocity;
	private Vector3 targetPosition;
	private OnTargetHitCallback targetHitCallback;

	void Awake() {
		foreach (HitArea hitArea in hitAreas) {
			hitArea.AddHitAreaCollidedCallback(OnHitAreaHit);
		}

		targetPosition = transform.position;
	}

	void Start() {
		if (PhotonNetwork.isMasterClient) {
			StartCoroutine(ChangeAngularVelocityCoroutine());
		}
	}

	void FixedUpdate() {
		if (PhotonNetwork.isMasterClient) {
			transform.Rotate(transform.right, angularVelocity.x * Time.fixedDeltaTime, Space.World);
			transform.Rotate(transform.up, angularVelocity.y * Time.fixedDeltaTime, Space.World);
			transform.Rotate(transform.forward, angularVelocity.z * Time.fixedDeltaTime, Space.World);

			transform.Translate((targetPosition - transform.position) * Time.fixedDeltaTime, Space.World);
		}
	}

	public void AddTargetHitCallback(OnTargetHitCallback targetHitCallback) {
		if (this.targetHitCallback == null) {
			this.targetHitCallback = targetHitCallback;
		} else {
			this.targetHitCallback += targetHitCallback;
		}
	}

	private void OnHitAreaHit(PhotonPlayer hittingPlayer, Collision collision, int points) {
		if (targetHitCallback != null) {
			targetHitCallback(hittingPlayer, points);
		}

		photonView.RPC("RpcKnockback", PhotonNetwork.masterClient, -collision.contacts[0].normal);
	}

	private IEnumerator ChangeAngularVelocityCoroutine() {
		while (true) {
			angularVelocity = new Vector3(
				Random.Range(-maxRotateSpeed, maxRotateSpeed),
				Random.Range(-maxRotateSpeed, maxRotateSpeed),
				Random.Range(-maxRotateSpeed, maxRotateSpeed));

			yield return new WaitForSeconds(changeAngularVelocityInterval);
		}
	}

	[PunRPC]
	private void RpcKnockback(Vector3 knockbackDirection) {
		Vector3 knockbackVector = knockbackDirection * knockbackDistance;
		targetPosition = new Vector3(
			Mathf.Clamp(targetPosition.x + knockbackVector.x, minPosition.x, maxPosition.x),
			Mathf.Clamp(targetPosition.y + knockbackVector.y, minPosition.y, maxPosition.y),
			Mathf.Clamp(targetPosition.z + knockbackVector.z, minPosition.z, maxPosition.z));
	}

}
