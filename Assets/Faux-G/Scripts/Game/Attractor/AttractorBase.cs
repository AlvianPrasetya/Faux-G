using UnityEngine;

/**
 * This class describes the common behaviour of all attractor bodies.
 */
public abstract class AttractorBase : MonoBehaviour {

	// Mass of this attractor in kg
	public float mass;

	/**
	 * This method calculates the exerted gravitational force against a body of given mass and euclidean 
	 * position. The direction of force exerted depends on the topography of the implementing attractor.
	 * Classes implementing this abstract class are to implement this method depending on the gravitational 
	 * behaviour of the object.
	 */
	public abstract Vector3 CalculateGravitationalForce(Vector3 attractedPosition, float attractedMass);

	/**
	 * This method calculates the exerted gravitational force using Newtonian physics.
	 * All classes implementing the AttractorBase class should call this method at the very end 
	 * of the gravity calculation process.
	 */
	protected Vector3 CalculateGravitationalForce(Vector3 direction, float attractedMass, float distance) {
		return direction * Utils.Physics.G * mass * attractedMass / (distance * distance);
	}

}
