using UnityEngine;

public class ThrowingRangeGameManager : GameManagerBase {
    
    public Spawner[] spawners;
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
        int spawnerId = Random.Range(0, spawners.Length);

        localPlayer = spawners[spawnerId].NetworkedSpawn(Utils.Resource.PLAYER, 0);

        playerCamera = localPlayer.GetComponentInChildren<Camera>();
        UIManager.Instance.PlayerCamera = playerCamera;

        sceneCamera.gameObject.SetActive(false);
    }

}
