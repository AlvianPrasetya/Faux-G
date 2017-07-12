using UnityEngine;

/**
 * This abstract class describes the mandatory properties and behavioural skeletons of a throwable 
 * object. New throwables are to implement this abstract class accordingly.
 */
public abstract class ThrowableBase : MonoBehaviour {

    // The owner of this throwable object (the player spawning this object).
    private PhotonPlayer owner;

    public PhotonPlayer Owner {
        get {
            return owner;
        }

        set {
            owner = value;
        }
    }

    /**
     * This abstract method describes the "releasing" behaviour of this throwable (the moment 
     * when the throwable is released from the arms of the owner towards the targeted direction).
     */
    public abstract void Release(Vector3 throwPosition, Vector3 throwDirection, float throwForce);

    /**
     * This abstract method extends Unity's OnTriggerEnter that describes the behaviour of this 
     * throwable when colliding with another object. Explosion/other behaviours are to be 
     * described within this method.
     */
    protected abstract void OnTriggerEnter(Collider other);

}
