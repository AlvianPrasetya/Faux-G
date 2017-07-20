using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * This class synchronizes the network behaviours of all player instances.
 */
public class NetworkManager : Photon.MonoBehaviour {

	public float syncLatencyInterval;

	private Dictionary<PhotonPlayer, int> playerLatency;

	private void Awake() {
		playerLatency = new Dictionary<PhotonPlayer, int>();
	}

	void Start() {
		StartCoroutine(SyncLatencyCoroutine());
	}

	/**
	 * This method tries to get the latency of a specified PhotonPlayer, 
	 * returns 0 if that player's latency information does not exist.
	 */
	public int GetPlayerLatency(PhotonPlayer player) {
		int latency;
		playerLatency.TryGetValue(player, out latency);

		return latency;
	}

	private IEnumerator SyncLatencyCoroutine() {
		while (true) {
			photonView.RPC("RpcSyncLatency", PhotonTargets.All, PhotonNetwork.player.ID, PhotonNetwork.GetPing() / 2);
			yield return new WaitForSeconds(syncLatencyInterval);
		}
	}

	[PunRPC]
	private void RpcSyncLatency(int playerId, int localLatency) {
		PhotonPlayer player = PhotonPlayer.Find(playerId);

		playerLatency[player] = localLatency;
	}

}
