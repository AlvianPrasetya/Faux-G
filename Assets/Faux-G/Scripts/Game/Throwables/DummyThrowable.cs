using System;
using UnityEngine;

public class DummyThrowable : ThrowableBase, IPoolable {

    private new Collider collider;
    private new Rigidbody rigidbody;
    private GravityBody gravityBody;

    public void Pool() {
        // TODO: Implement pooling routine
    }

    void Awake() {
        collider = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();
        gravityBody = GetComponent<GravityBody>();
        
        // Disable physics before release
        collider.enabled = false;
        rigidbody.isKinematic = true;
        gravityBody.enabled = false;
    }

    public override void Release(Vector3 throwPosition, Vector3 throwDirection, float throwForce) {
        transform.parent = null;
        transform.position = throwPosition;

        // Enable physics upon release
        collider.enabled = true;
        rigidbody.isKinematic = false;
        gravityBody.enabled = true;

        rigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }

    protected override void OnTriggerEnter(Collider other) {
        // Does nothing on collision
    }

}
