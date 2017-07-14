using UnityEngine;

public class ThrowingRangeGameManager : GameManagerBase {
    
    public Transform[] spawnPoints;
    public Camera sceneCamera;

    private GameObject localPlayer;
    private Camera playerCamera;

    protected override void Awake() {
        base.Awake();
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
        int spawnPointId = Random.Range(0, spawnPoints.Length);
        localPlayer = PhotonNetwork.Instantiate(
            Utils.Resource.PLAYER,
            spawnPoints[spawnPointId].position,
            spawnPoints[spawnPointId].rotation,
            0
        );
        playerCamera = localPlayer.GetComponentInChildren<Camera>();
        UIManager.Instance.PlayerCamera = playerCamera;

        sceneCamera.gameObject.SetActive(false);
    }

}
