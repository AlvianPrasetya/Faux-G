using UnityEngine;

public class AttractorManager : MonoBehaviour {

	public AttractorBase[] attractors;

	private static AttractorManager instance;

	void Awake() {
		instance = this;
	}

	public static AttractorManager Instance {
		get {
			return instance;
		}
	}

}
