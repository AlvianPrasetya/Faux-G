using UnityEngine;

public class DummyThrowable : IThrowable {

    private new Rigidbody rigidbody;
    private GravityBody gravityBody;

    private void Awake() {
        rigidbody = GetComponent<Rigidbody>();
        gravityBody = GetComponent<GravityBody>();
    }

    public override void Release(Vector3 throwPosition, Vector3 throwDirection, float throwForce) {
        transform.parent = null;
        transform.position = throwPosition;
        
        rigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        gravityBody.enabled = true;
    }

    protected override void OnTriggerEnter(Collider other) {
        // Does nothing on collision
    }

}
