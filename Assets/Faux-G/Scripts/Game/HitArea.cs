using UnityEngine;

public class HitArea : MonoBehaviour {

    public delegate void OnHitAreaHitCallback(PhotonPlayer hittingPlayer, int value);

    // The points/damage multiplier when this hit area is collided with
    public int onHitMultiplier;

    private OnHitAreaHitCallback hitCallback;

    public OnHitAreaHitCallback HitCallback {
        set {
            hitCallback = value;
        }
    }

    public void OnThrowableCollision(ThrowableBase collidingThrowable) {
        PhotonPlayer collidingThrowableOwner = collidingThrowable.Owner;
        int onHitValue = collidingThrowable.onHitValue;

        hitCallback(collidingThrowableOwner, onHitValue * onHitMultiplier);
    }

}
