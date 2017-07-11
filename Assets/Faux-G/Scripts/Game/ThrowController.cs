using UnityEngine;

/**
 * This class controls throw events and all actions related to it.
 * Throwable instantiation and control pre-release are also handled by this class.
 */
public class ThrowController : Photon.MonoBehaviour {

    public delegate void AmmoUpdateCallback(int currentAmmo, int maxAmmo);
    public enum THROWABLE_STATE {
        IDLE, // IDLE state, no throwables is currently at hand
        PREPARED, // PREPARED state, currently holding a throwable, not yet charging
        CHARGING // CHARGING state, currently holding a throwable and charging it before throwing
    }

    public Transform throwableSpawner;
    public Transform armPivot;
    public Camera playerCamera;

    public IThrowable[] throwables;

    // The increment in relative throwing force per second of charging
    public float relativeThrowForcePerSecond;

    // The increment in relative reload progress per second of waiting
    public float relativeReloadProgressPerSecond;

    // The minimum and maximum throwing force in Newton (affected by throwable mass)
    public float minThrowForce;
    public float maxThrowForce;

    // Minimum and maximum arm angle when charging on the "right" axis
    public float minArmAngle; // Arm angle when relative throw force is 1
    public float maxArmAngle; // Arm angle when relative throw force is 0

    private THROWABLE_STATE throwableState;
    private IThrowable preparedThrowable;

    // Relative throw force ranging from 0 ~ 1 (0 = minThrowForce, 1 = maxThrowForce)
    private float relativeThrowForce;

    // Relative reload progress ranging from 0 ~ 1 (0 = reload hasn't started, 1 = reload has finished)
    private float relativeReloadProgress;

	void Awake() {
        throwableState = THROWABLE_STATE.IDLE;
        preparedThrowable = null;
    }

    void Update() {
        if (photonView.isMine) {
            InputChargeThrowable();
            InputReleaseThrowable();
        }
    }

    void FixedUpdate() {
        UpdateRelativeThrowForce();
        UpdateRelativeReloadProgress();
        UpdateArmRotation();
    }

    public void PrepareThrowable(int throwableId) {
        if (throwableState == THROWABLE_STATE.IDLE) {
            photonView.RPC(
                "RpcPrepareThrowable", PhotonTargets.All,
                PhotonNetwork.ServerTimestamp, throwableId);
        }
    }

    public void ChargeThrowable() {
        if (throwableState == THROWABLE_STATE.PREPARED) {
            photonView.RPC(
                "RpcChargeThrowable", PhotonTargets.All,
                PhotonNetwork.ServerTimestamp);
        }
    }

    public void ReleaseThrowable() {
        if (throwableState == THROWABLE_STATE.CHARGING && preparedThrowable != null) {
            // Calculate throwing force
            float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, relativeThrowForce);

            photonView.RPC(
                "RpcReleaseThrowable", PhotonTargets.All, 
                PhotonNetwork.ServerTimestamp, 
                throwableSpawner.position, throwableSpawner.forward, 
                throwForce);
        }
    }

    private void InputChargeThrowable() {
        if (throwableState == THROWABLE_STATE.PREPARED && Input.GetMouseButtonDown(Utils.Input.MOUSE_BUTTON_LEFT)) {
            ChargeThrowable();
        }
    }

    private void InputReleaseThrowable() {
        if (throwableState == THROWABLE_STATE.CHARGING && Input.GetMouseButtonUp(Utils.Input.MOUSE_BUTTON_LEFT)) {
            ReleaseThrowable();
        }
    }

    private void UpdateRelativeThrowForce() {
        if (throwableState == THROWABLE_STATE.CHARGING) {
            relativeThrowForce += Mathf.Clamp(relativeThrowForcePerSecond * Time.fixedDeltaTime, 0.0f, 1.0f);
        }
    }

    private void UpdateRelativeReloadProgress() {
        if (throwableState == THROWABLE_STATE.IDLE) {
            relativeReloadProgress += Mathf.Clamp(relativeReloadProgressPerSecond * Time.fixedDeltaTime, 0.0f, 1.0f);
            if (relativeReloadProgress >= 1.0f) {
                PrepareThrowable(0);
            }
        }
    }

    private void UpdateArmRotation() {
        float currentArmAngle;
        switch (throwableState) {
            case THROWABLE_STATE.CHARGING:
                currentArmAngle = Mathf.Lerp(maxArmAngle, minArmAngle, relativeThrowForce);
                armPivot.localRotation = Quaternion.Euler(currentArmAngle, 0.0f, 0.0f);
                break;
            case THROWABLE_STATE.IDLE:
                currentArmAngle = Mathf.Lerp(minArmAngle, maxArmAngle, relativeReloadProgress);
                armPivot.localRotation = Quaternion.Euler(currentArmAngle, 0.0f, 0.0f);
                break;
        }
    }

    [PunRPC]
    private void RpcPrepareThrowable(int eventTimeMs, int throwableId) {
        // TODO: Trigger preparing throwable animation
        IThrowable throwable = Instantiate(throwables[throwableId],
            throwableSpawner.position, throwableSpawner.rotation, throwableSpawner);
        throwable.Owner = photonView.owner;

        throwableState = THROWABLE_STATE.PREPARED;
        preparedThrowable = throwable;
        relativeThrowForce = 0.0f;
    }

    [PunRPC]
    private void RpcChargeThrowable(int eventTimeMs) {
        // TODO: Trigger charging throw animation

        throwableState = THROWABLE_STATE.CHARGING;
    }

    [PunRPC]
    private void RpcReleaseThrowable(int eventTimeMs, Vector3 throwPosition, Vector3 throwDirection, float throwForce) {
        // TODO: Extrapolation logic
        /*float secondsSinceEvent = (PhotonNetwork.ServerTimestamp - eventTimeMs) / 1000.0f;
        Vector3 extrapolatedPosition = throwPosition
            + throwRotation * Vector3.forward * preparedThrowable.speed;*/
        
        preparedThrowable.Release(throwPosition, throwDirection, throwForce);

        throwableState = THROWABLE_STATE.IDLE;
        preparedThrowable = null;
        relativeReloadProgress = 0.0f;
    }

}
