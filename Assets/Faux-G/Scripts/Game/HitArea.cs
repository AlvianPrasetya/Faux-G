using UnityEngine;

public class HitArea : MonoBehaviour {

    public delegate void OnHitAreaCollidedCallback(PhotonPlayer hittingPlayer, Collision collision, int value);

    // The points/damage multiplier when this hit area is collided with
    public int onHitMultiplier;

    private OnHitAreaCollidedCallback hitAreaCollidedCallback;

    public void AddHitAreaCollidedCallback(OnHitAreaCollidedCallback hitAreaCollidedCallback) {
        if (this.hitAreaCollidedCallback == null) {
            this.hitAreaCollidedCallback = hitAreaCollidedCallback;
        } else {
            this.hitAreaCollidedCallback += hitAreaCollidedCallback;
        }
    }

    public void OnThrowableCollision(ThrowableBase collidingThrowable, Collision collision) {
        PhotonPlayer collidingThrowableOwner = collidingThrowable.Owner;
        int onHitValue = collidingThrowable.onHitValue;

        if (hitAreaCollidedCallback != null) {
            hitAreaCollidedCallback(collidingThrowableOwner, collision, onHitValue * onHitMultiplier);
        }
    }

}
