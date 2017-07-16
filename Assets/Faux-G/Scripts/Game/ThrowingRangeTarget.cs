using UnityEngine;
using System.Collections;

public class ThrowingRangeTarget : MonoBehaviour {

    public delegate void OnTargetHitCallback(PhotonPlayer hittingPlayer, int points);

    public float maxRotateSpeed;
    public float changeAngularVelocityInterval;

    public HitArea[] hitAreas;

    private Vector3 angularVelocity;

    private OnTargetHitCallback targetHitCallback;

    void Awake() {
        foreach (HitArea hitArea in hitAreas) {
            hitArea.AddHitAreaHitCallback(OnHitAreaHit);
        }
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
        }
    }

    public void AddTargetHitCallback(OnTargetHitCallback targetHitCallback) {
        if (this.targetHitCallback == null) {
            this.targetHitCallback = targetHitCallback;
        } else {
            this.targetHitCallback += targetHitCallback;
        }
    }

    private void OnHitAreaHit(PhotonPlayer hittingPlayer, int points) {
        if (targetHitCallback != null) {
            targetHitCallback(hittingPlayer, points);
        }
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

}
