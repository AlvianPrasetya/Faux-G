using UnityEngine;
using System.Collections.Generic;

public class AttractorManager : MonoBehaviour {

    public AttractorBase[] attractors;

    private static AttractorManager instance;

    public static AttractorManager Instance {
        get {
            return instance;
        }
    }

    void Awake() {
        instance = this;
    }

}
