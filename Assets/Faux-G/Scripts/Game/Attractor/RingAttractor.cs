using UnityEngine;

public class RingAttractor : AttractorBase {

    public float radius;

    public override Vector3 CalculateForceVector(Vector3 attractedPosition, float attractedMass) {
        // TODO: Take transform rotation into account

        Vector3 normalVector = Vector3.up;

        Vector3 centerToAttractedVector = attractedPosition - transform.position;

        // Make a copy of centerToAttractedVector
        Vector3 normalizedCenterToAttractedOrthoVector = centerToAttractedVector;

        // Calculate normalizedCenterToAttractedVector (normalized, orthogonal against the surface normal)
        Vector3.OrthoNormalize(ref normalVector, ref normalizedCenterToAttractedOrthoVector);

        // Calculate closest point on ring
        Vector3 closestPoint = transform.position + normalizedCenterToAttractedOrthoVector * radius;

        // Calculate gravity direction
        Vector3 gravityDirection = (closestPoint - attractedPosition).normalized;

        // F = G * m1 * m2 / d^1.5
        return gravityDirection * Utils.G * rigidbody.mass * attractedMass
            / Mathf.Pow((attractedPosition - closestPoint).magnitude, Utils.PHI);
    }

}
