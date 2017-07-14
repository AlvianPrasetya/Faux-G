using UnityEngine;
using System.Collections.Generic;

public class ThrowingRangeGameManager : GameManagerBase {
    
    public Spawner[] spawners;
    public Camera sceneCamera;
    public ThrowingRangeTarget throwingRangeTarget;

    private GameObject localPlayer;
    private Camera playerCamera;

    private Dictionary<PhotonPlayer, int> playerPoints;

    protected override void Awake() {
        base.Awake();

        throwingRangeTarget.HitCallback = AddPoints;

        playerPoints = new Dictionary<PhotonPlayer, int>();
    }

    protected override void CheckForWinCondition() {
        // TODO: Implement win condition
    }

    protected override void StartGame() {
        base.StartGame();

        Spawn();
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

    private void AddPoints(PhotonPlayer player, int points) {
        photonView.RPC("RpcAddPoints", PhotonTargets.All, player.ID, points);
    }

    [PunRPC]
    private void RpcAddPoints(int playerId, int points) {
        PhotonPlayer targetPlayer = PhotonPlayer.Find(playerId);
        Logger.Log(string.Format("Adding {0} points for {1}", points, targetPlayer.NickName));

        int oldPoints;
        playerPoints[targetPlayer] = playerPoints.TryGetValue(targetPlayer, out oldPoints) ? (oldPoints + points) : points;
    }

}
