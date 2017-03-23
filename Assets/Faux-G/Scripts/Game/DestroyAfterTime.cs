using UnityEngine;
using System.Collections;

public class DestroyAfterTime : MonoBehaviour {

	public delegate void PreDestroyCallback();

	public float lifetime;
	
	private PreDestroyCallback preDestroyCallback;

	void Start() {
		StartCoroutine(WaitForDestroy());
	}

	public void SetPreDestroyCallback(PreDestroyCallback callback) {
		preDestroyCallback = callback;
	}

	private IEnumerator WaitForDestroy() {
		yield return new WaitForSeconds(lifetime);

		if (preDestroyCallback != null) {
			preDestroyCallback();
		}
		Destroy(gameObject);
	}

}
