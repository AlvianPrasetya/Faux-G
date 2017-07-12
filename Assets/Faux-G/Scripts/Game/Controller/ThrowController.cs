using UnityEngine;

/**
 * This class controls throw events and all actions related to it.
 * Throwable instantiation and control pre-release are also handled by this class.
 */
public class ThrowController : Photon.MonoBehaviour {
    
    public enum THROWABLE_STATE {
        IDLE, // IDLE state, no throwables is currently at hand
        PREPARED, // PREPARED state, currently holding a throwable, not yet charging
        CHARGING // CHARGING state, currently holding a throwable and charging it before throwing
    }

    public Transform throwableSpawner;
    public Transform armPivot;
    public Camera playerCamera;

    public ThrowableBase[] throwables;

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
    private ThrowableBase preparedThrowable;

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
            PrepareThrowableOnReloadFinish();
        }
    }

    void FixedUpdate() {
        UpdateRelativeThrowForce();
        UpdateRelativeReloadProgress();
        UpdateArmRotation();
    }

    private void InputChargeThrowable() {
        if (Input.GetMouseButtonDown(Utils.Input.MOUSE_BUTTON_LEFT)) {
            ChargeThrowable();
        }
    }

    private void InputReleaseThrowable() {
        if (Input.GetMouseButtonUp(Utils.Input.MOUSE_BUTTON_LEFT)) {
            ReleaseThrowable();
        }
    }

    private void PrepareThrowableOnReloadFinish() {
        if (throwableState == THROWABLE_STATE.IDLE && preparedThrowable == null && relativeReloadProgress == 1.0f) {
            // TODO: Randomize the next throwable to prepare
            PrepareThrowable(0);
        }
    }

    /**
     * This method attempts to do a networked preparation of throwable.
     */
    private void PrepareThrowable(int throwableId) {
        if (throwableState == THROWABLE_STATE.IDLE) {
            photonView.RPC(
                "RpcPrepareThrowable", PhotonTargets.All,
                PhotonNetwork.ServerTimestamp, throwableId);
        }
    }

    /**
     * This method attempts to do a networked charging of the current prepared throwable.
     */
    private void ChargeThrowable() {
        if (throwableState == THROWABLE_STATE.PREPARED) {
            photonView.RPC(
                "RpcChargeThrowable", PhotonTargets.All,
                PhotonNetwork.ServerTimestamp);
        }
    }

    /**
     * This method attempts to do a networked release of the current charged throwable.
     */
    private void ReleaseThrowable() {
        if (throwableState == THROWABLE_STATE.CHARGING && preparedThrowable != null) {
            // Calculate throwing force
            float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, relativeThrowForce);

            photonView.RPC(
                "RpcReleaseThrowable", PhotonTargets.All,
                PhotonNetwork.ServerTimestamp,
                throwableSpawner.position, throwableSpawner.rotation,
                throwableSpawner.forward, throwForce);
        }
    }

    /**
     * This method updates the relative throwing force based on the value of relativeThrowForcePerSecond 
     * if and only if the throwable state is at CHARGING (the character is charging a throw).
     */
    private void UpdateRelativeThrowForce() {
        if (throwableState == THROWABLE_STATE.CHARGING) {
            relativeThrowForce += Mathf.Clamp(relativeThrowForcePerSecond * Time.fixedDeltaTime, 0.0f, 1.0f);
        }
    }

    /**
     * This method updates the relative reload progress based on the value of relativeReloadProgressPerSecond 
     * if and only if the throwable state is at IDLE (the character has no active throwable).
     */
    private void UpdateRelativeReloadProgress() {
        if (throwableState == THROWABLE_STATE.IDLE) {
            relativeReloadProgress = Mathf.Clamp(
                relativeReloadProgress + relativeReloadProgressPerSecond * Time.fixedDeltaTime, 
                0.0f, 
                1.0f);
        }
    }

    /**
     * This method updates the arm rotation based on the current relative throwing force or relative 
     * reload progress, whichever is relevant to the current state. The arm rotation, in turn, determines 
     * the trajectory of the throwable upon release.
     */
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
        ThrowableBase throwable = Instantiate(throwables[throwableId],
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
    private void RpcReleaseThrowable(int eventTimeMs, Vector3 throwPosition, Quaternion throwRotation, 
        Vector3 throwDirection, float throwForce) {
        // TODO: Extrapolation logic
        /*float secondsSinceEvent = (PhotonNetwork.ServerTimestamp - eventTimeMs) / 1000.0f;
        Vector3 extrapolatedPosition = throwPosition
            + throwRotation * Vector3.forward * preparedThrowable.speed;*/
        
        preparedThrowable.Release(throwPosition, throwRotation, throwDirection, throwForce);

        throwableState = THROWABLE_STATE.IDLE;
        preparedThrowable = null;
        relativeReloadProgress = 0.0f;
    }

}
