using UnityEngine;

/**
 * This class describes the gravitational behaviour of a ring attractor.
 */
public class RingAttractor : AttractorBase {

	public float radius;

	public override Vector3 CalculateGravitationalForce(Vector3 attractedPosition, float attractedMass) {
		Vector3 normalVector = transform.up;

		Vector3 centerToAttractedVector = attractedPosition - transform.position;

		// Make a copy of centerToAttractedVector
		Vector3 normalizedCenterToAttractedOrthoVector = centerToAttractedVector;

		// Calculate normalizedCenterToAttractedVector (normalized, orthogonal against the surface normal)
		Vector3.OrthoNormalize(ref normalVector, ref normalizedCenterToAttractedOrthoVector);

		// Calculate closest point on ring
		Vector3 closestPoint = transform.position + normalizedCenterToAttractedOrthoVector * radius;

		// Calculate gravity direction
		Vector3 gravityDirection = (closestPoint - attractedPosition).normalized;

		return CalculateGravitationalForce(gravityDirection, attractedMass, (attractedPosition - closestPoint).magnitude);
	}

}
