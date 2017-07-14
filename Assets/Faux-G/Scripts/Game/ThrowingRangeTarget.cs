using UnityEngine;

public class ThrowingRangeTarget : MonoBehaviour {

    public delegate void OnTargetHitCallback(PhotonPlayer hittingPlayer, int points);

    public HitArea[] hitAreas;

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

    private void OnHitAreaHit(PhotonPlayer hittingPlayer, int points) {
        hitCallback(hittingPlayer, points);
    }

}
