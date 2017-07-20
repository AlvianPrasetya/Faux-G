using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ThrowingRangeGameManager : GameManagerBase {

	public Spawner[] spawners;
	public Camera sceneCamera;
	public ThrowingRangeTarget throwingRangeTarget;
	
	public int pointsToWin;

	private GameObject localPlayer;
	private Camera playerCamera;

	private Dictionary<PhotonPlayer, int> standings;

	protected override void Awake() {
		base.Awake();

		throwingRangeTarget.AddTargetHitCallback(AddPoints);

		standings = new Dictionary<PhotonPlayer, int>();
	}

	protected override void Start() {
		base.Start();

		uiManager.Announce(string.Format("Reach {0} points to win", pointsToWin));
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer player) {
		// Remove player from standings when disconnected
		standings.Remove(player);
		UpdateStandings();
	}

	protected override void StartGame() {
		base.StartGame();

		Spawn();
		AddPoints(PhotonNetwork.player, 0);
	}

	protected override bool CheckForWinCondition() {
		foreach (KeyValuePair<PhotonPlayer, int> standingsEntry in standings) {
			PhotonPlayer player = standingsEntry.Key;
			int points = standingsEntry.Value;

			if (standingsEntry.Value >= pointsToWin) {
				AnnounceWinner(player);
				return true;
			}
		}

		return false;
	}

	private void Spawn() {
		int spawnerId = Random.Range(0, spawners.Length);

		localPlayer = spawners[spawnerId].NetworkedSpawn(Utils.Resource.PLAYER, 0);

		sceneCamera.gameObject.SetActive(false);
	}

	private void AddPoints(PhotonPlayer player, int points) {
		photonView.RPC("RpcAddPoints", PhotonTargets.AllBuffered, player.ID, points);
	}

	private void UpdateStandings() {
		// Copy standings to a key value pair list
		List<KeyValuePair<PhotonPlayer, int>> standingsList = standings.ToList();

		// Sort standings list by points (value)
		standingsList.Sort((x, y) => y.Value.CompareTo(x.Value));

		// Build standings text
		string standingsText = "Standings:\n";
		for (int i = 0; i < standingsList.Count; i++) {
			PhotonPlayer player = standingsList[i].Key;
			int points = standingsList[i].Value;

			standingsText += (i + 1) + ". " + player.NickName + " -- " + points + " pts\n";
		}

		if (standingsUpdatedCallback != null) {
			standingsUpdatedCallback(standingsText);
		}
	}

	private void AnnounceWinner(PhotonPlayer winningPlayer) {
		uiManager.Announce(winningPlayer.NickName + " wins!");
	}

	[PunRPC]
	private void RpcAddPoints(int playerId, int points) {
		PhotonPlayer targetPlayer = PhotonPlayer.Find(playerId);
		Logger.Log(string.Format("Adding {0} points for {1}", points, targetPlayer.NickName));

		int oldPoints;
		standings.TryGetValue(targetPlayer, out oldPoints);

		standings[targetPlayer] = oldPoints + points;

		UpdateStandings();
		if (CheckForWinCondition()) {
			EndGame();
		}
	}

}
