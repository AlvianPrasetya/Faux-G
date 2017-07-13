using UnityEngine;

public abstract class AttractorBase : MonoBehaviour {

    protected new Rigidbody rigidbody;

    protected virtual void Awake() {
        rigidbody = GetComponent<Rigidbody>();
    }

    public abstract Vector3 CalculateGravitationalForce(Vector3 attractedPosition, float attractedMass);

    /**
     * This method does calculation of gravitational force using modified "Newtonian" physics.
     */
    protected Vector3 CalculateGravitationalForce(Vector3 direction, float attractedMass, float distance) {
        return direction * Utils.G * rigidbody.mass* attractedMass / Mathf.Pow(distance, Utils.PHI);
    }

}
