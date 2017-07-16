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

        throwingRangeTarget.HitCallback = AddPoints;

        standings = new Dictionary<PhotonPlayer, int>();
    }

    protected override void Start() {
        base.Start();

        UIManager.Instance.announcementText.text = string.Format("First to reach {0} points win!", pointsToWin);
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer player) {
        // Remove player from standings when disconnected
        standings.Remove(player);
        OnStandingsUpdated();
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

        playerCamera = localPlayer.GetComponentInChildren<Camera>();
        UIManager.Instance.PlayerCamera = playerCamera;

        sceneCamera.gameObject.SetActive(false);
    }

    private void OnStandingsUpdated() {
        // Copy standings to a key value pair list
        List<KeyValuePair<PhotonPlayer, int>> standingsList = standings.ToList<KeyValuePair<PhotonPlayer, int>>();

        // Sort standings list by points (value)
        standingsList.Sort((x, y) => y.Value.CompareTo(x.Value));

        // Build standings text
        string standingsText = "Standings:\n";
        for (int i = 0; i < standingsList.Count; i++) {
            PhotonPlayer player = standingsList[i].Key;
            int points = standingsList[i].Value;

            standingsText += (i + 1) + ". " + player.NickName + " -- " + points + " pts\n";
        }

        UIManager.Instance.standingsText.text = standingsText;

        CheckForWinCondition();
    }

    private void AddPoints(PhotonPlayer player, int points) {
        photonView.RPC("RpcAddPoints", PhotonTargets.AllBuffered, player.ID, points);
    }

    private void AnnounceWinner(PhotonPlayer winningPlayer) {
        UIManager.Instance.announcementText.text = winningPlayer.NickName + " wins!";
    }

    [PunRPC]
    private void RpcAddPoints(int playerId, int points) {
        PhotonPlayer targetPlayer = PhotonPlayer.Find(playerId);
        Logger.Log(string.Format("Adding {0} points for {1}", points, targetPlayer.NickName));

        int oldPoints;
        standings.TryGetValue(targetPlayer, out oldPoints);

        standings[targetPlayer] = oldPoints + points;
        
        // Callback
        OnStandingsUpdated();
    }

}
