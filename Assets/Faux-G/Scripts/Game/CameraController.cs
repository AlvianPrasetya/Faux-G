using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	/*
	 * MONOBEHAVIOUR LIFECYCLE
	 */

	void Update() {
		transform.Rotate(-transform.right, Input.GetAxis(Utils.Key.INPUT_MOUSE_Y), Space.World);
	}

}
