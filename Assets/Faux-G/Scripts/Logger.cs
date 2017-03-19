using UnityEngine;

public class Logger : MonoBehaviour {

	public static void A(string s) {
		Debug.LogAssertion(s);
	}

	public static void A(string format, params object[] args) {
		Debug.LogAssertionFormat(format, args);
	}

	public static void D(string s) {
		Debug.Log(s);
	}

	public static void D(string format, params object[] args) {
		Debug.LogFormat(format, args);
	}

	public static void W(string s) {
		Debug.LogWarning(s);
	}

	public static void W(string format, params object[] args) {
		Debug.LogWarningFormat(format, args);
	}

	public static void E(string s) {
		Debug.LogError(s);
	}

	public static void E(string format, params object[] args) {
		Debug.LogErrorFormat(format, args);
	}

	public static void E(System.Exception ex) {
		Debug.LogException(ex);
	}

}
