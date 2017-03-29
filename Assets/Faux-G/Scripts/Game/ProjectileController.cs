using UnityEngine;
using System.Collections.Generic;

public class ProjectileController : MonoBehaviour {

	public Explosion prefabExplosion;
	
	public float speed;
	public float damage;

	// Cached components
	private new Collider collider;
	private new Rigidbody rigidbody;
	private DestroyAfterTime destroyAfterTime;

	private PhotonPlayer owner;
	private List<Collider> ownerColliders;

	void Awake() {
		collider = GetComponent<Collider>();
		rigidbody = GetComponent<Rigidbody>();
		destroyAfterTime = GetComponent<DestroyAfterTime>();
		destroyAfterTime.SetPreDestroyCallback(Explode);
	}

	void Start() {
		foreach (Collider ownerCollider in ownerColliders) {
			Physics.IgnoreCollision(collider, ownerCollider);
		}
		rigidbody.AddForce(transform.forward * speed, ForceMode.VelocityChange);
	}

	void OnCollisionEnter(Collision collision) {
		Logger.Log("Collision {0}", collision.gameObject.name);
		Explode();

		HitArea targetHitArea = collision.gameObject.GetComponent<HitArea>();
		if (targetHitArea != null) {
			targetHitArea.Hit(damage, owner);
		}

		Destroy(gameObject);
	}

	public void SetOwner(PhotonPlayer owner, List<Collider> ownerColliders) {
		this.owner = owner;
		this.ownerColliders = ownerColliders;
	}

	public void Explode() {
		if (prefabExplosion == null) {
			return;
		}

		Explosion explosion = Instantiate(prefabExplosion, transform.position, transform.rotation);
		explosion.Owner = owner;
	}

}
