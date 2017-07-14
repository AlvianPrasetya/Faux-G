using UnityEngine;

public abstract class AttractorBase : MonoBehaviour {

    protected new Rigidbody rigidbody;

    protected virtual void Awake() {
        rigidbody = GetComponent<Rigidbody>();
    }

    public abstract Vector3 CalculateGravitationalForce(Vector3 attractedPosition, float attractedMass);

    /**
     * This method calculates the exerted gravitational force using modified "Newtonian" physics.
     * All classes implementing the AttractorBase class should call this method at the very end 
     * of the gravity calculation process.
     */
    protected Vector3 CalculateGravitationalForce(Vector3 direction, float attractedMass, float distance) {
        return direction * Utils.Physics.G * rigidbody.mass* attractedMass
            / Mathf.Pow(distance, Utils.Physics.PHI);
    }

}
