using UnityEngine;

public class HitArea : MonoBehaviour {

    public delegate void OnHitAreaHitCallback(PhotonPlayer hittingPlayer, int value);

    // The points/damage multiplier when this hit area is collided with
    public int onHitMultiplier;

    private OnHitAreaHitCallback hitAreaHitCallback;

    public void AddHitAreaHitCallback(OnHitAreaHitCallback hitAreaHitCallback) {
        if (this.hitAreaHitCallback == null) {
            this.hitAreaHitCallback = hitAreaHitCallback;
        } else {
            this.hitAreaHitCallback += hitAreaHitCallback;
        }
    }

    public void OnThrowableCollision(ThrowableBase collidingThrowable) {
        PhotonPlayer collidingThrowableOwner = collidingThrowable.Owner;
        int onHitValue = collidingThrowable.onHitValue;

        if (hitAreaHitCallback != null) {
            hitAreaHitCallback(collidingThrowableOwner, onHitValue * onHitMultiplier);
        }
    }

}
