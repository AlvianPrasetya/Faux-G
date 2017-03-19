using UnityEngine;

public class Logger : MonoBehaviour {

	public static void D(string debug) {
		Debug.Log(debug);
	}

	public static void E(string error) {
		Debug.LogError(error);
	}

	public static void E(System.Exception ex) {
		Debug.LogException(ex);
	}

}
