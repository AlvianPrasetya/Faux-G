using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ThrowingRangeGameManager : GameManagerBase {
    
    public Spawner[] spawners;
    public Camera sceneCamera;
    public ThrowingRangeTarget throwingRangeTarget;

    private GameObject localPlayer;
    private Camera playerCamera;

    private Dictionary<PhotonPlayer, int> standings;

    protected override void Awake() {
        base.Awake();

        throwingRangeTarget.HitCallback = AddPoints;

        standings = new Dictionary<PhotonPlayer, int>();
    }

    protected override void CheckForWinCondition() {
        // TODO: Implement win condition
    }

    protected override void StartGame() {
        base.StartGame();

        Spawn();
        AddPoints(PhotonNetwork.player, 0);
    }

    protected override void EndGame() {
        base.EndGame();
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
        standingsList.Sort((x, y) => x.Value.CompareTo(y.Value));

        // Build standings text
        string standingsText = "Standings:\n";
        for (int i = 0; i < standingsList.Count; i++) {
            PhotonPlayer player = standingsList[i].Key;
            int points = standingsList[i].Value;

            standingsText += (i + 1) + ". " + player.NickName + " -- " + points + " pts\n";
        }

        UIManager.Instance.standingsText.text = standingsText;
    }

    private void AddPoints(PhotonPlayer player, int points) {
        photonView.RPC("RpcAddPoints", PhotonTargets.AllBuffered, player.ID, points);
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
