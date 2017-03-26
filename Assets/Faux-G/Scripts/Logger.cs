using UnityEngine;

public class Logger {

	private static bool enabled = true;

	public static bool Enabled {
		set {
			enabled = value;
		}
	}

	public static void Log(string log) {
		if (!enabled) {
			return;
		}

		Debug.Log(log);
	}

	public static void Log(string format, params object[] args) {
		if (!enabled) {
			return;
		}

		Debug.LogFormat(format, args);
	}

}