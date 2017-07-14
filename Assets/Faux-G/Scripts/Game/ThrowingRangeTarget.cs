using UnityEngine;
using System.Collections;

public class ThrowingRangeTarget : MonoBehaviour {

    public delegate void OnTargetHitCallback(PhotonPlayer hittingPlayer, int points);

    public float maxRotateSpeed;
    public float changeAngularVelocityInterval;

    public HitArea[] hitAreas;

    private Vector3 angularVelocity;

    private OnTargetHitCallback hitCallback;

    public OnTargetHitCallback HitCallback {
        set {
            hitCallback = value;
        }
    }

    void Awake() {
        foreach (HitArea hitArea in hitAreas) {
            hitArea.HitCallback = OnHitAreaHit;
        }
    }

    void Start() {
        if (PhotonNetwork.isMasterClient) {
            StartCoroutine(ChangeAngularVelocityCoroutine());
        }
    }

    private void FixedUpdate() {
        if (PhotonNetwork.isMasterClient) {
            transform.Rotate(transform.right, angularVelocity.x * Time.fixedDeltaTime, Space.World);
            transform.Rotate(transform.up, angularVelocity.y * Time.fixedDeltaTime, Space.World);
            transform.Rotate(transform.forward, angularVelocity.z * Time.fixedDeltaTime, Space.World);
        }
    }

    private void OnHitAreaHit(PhotonPlayer hittingPlayer, int points) {
        hitCallback(hittingPlayer, points);
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
