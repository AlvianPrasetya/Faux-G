using System;
using UnityEngine;

public class DummyThrowable : ThrowableBase, IPoolable {
    
    public void Pool() {
        // TODO: Implement pooling routine
    }

    public override void Release(Vector3 throwPosition, Quaternion throwRotation, 
        Vector3 throwDirection, float throwForce) {
        transform.parent = null;
        transform.position = throwPosition;
        transform.rotation = throwRotation;

        // Enable physics upon release
        collider.enabled = true;
        rigidbody.isKinematic = false;
        gravityBody.enabled = true;

        rigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }

    protected override void OnCollisionEnter(Collision collision) {
        // Does nothing on collision
    }

}
