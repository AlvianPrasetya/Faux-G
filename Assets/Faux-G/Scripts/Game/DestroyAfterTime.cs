using UnityEngine;
using System.Collections;

public class DestroyAfterTime : MonoBehaviour {

	public float lifetime;

	void Start() {
		StartCoroutine(WaitForDestroy());
	}

	private IEnumerator WaitForDestroy() {
		yield return new WaitForSeconds(lifetime);

		Destroy(gameObject);
	}

}
