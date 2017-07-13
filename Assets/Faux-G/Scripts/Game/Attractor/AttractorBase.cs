using UnityEngine;

public abstract class AttractorBase : MonoBehaviour {

    protected new Rigidbody rigidbody;

    protected virtual void Awake() {
        rigidbody = GetComponent<Rigidbody>();
    }

    public abstract Vector3 CalculateForceVector(Vector3 attractedPosition, float attractedMass);

}
