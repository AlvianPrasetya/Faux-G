using UnityEngine;
using System.Collections;

/**
 * This is the base abstract class for game managers. The implementing class
 * controls all the lifecycles of the game and what to do on such events.
 * New game managers are to implement this abstract class accordingly.
 */
public abstract class GameManagerBase : Photon.PunBehaviour {

    public enum GAME_STATE {
        WAITING, // WAITING state, the state before the game starts (waiting for players)
        RUNNING, // RUNNING state, the normal running state of the game
        PAUSED, // PAUSED state, the game is paused for one reason or another
        ENDED // ENDED state, the game has ended due to reaching a winning condition
    }

    public int delayToCountdownToStartGame;
    public int countdownToStartGame;
    public int delayToCountdownToLeave;
    public int countdownToLeave;
    
    protected GAME_STATE gameState;

    private static GameManagerBase instance;

    public static GameManagerBase Instance {
        get {
            return instance;
        }
    }

    protected virtual void Awake() {
        PhotonNetwork.sendRate = Utils.Network.SEND_RATE;
        PhotonNetwork.sendRateOnSerialize = Utils.Network.SEND_RATE_ON_SERIALIZE;
        
        gameState = GAME_STATE.WAITING;

        instance = this;
    }

    protected virtual void Start() {
        StartCoroutine(CountdownToStartGameCoroutine());
    }

    public override void OnLeftRoom() {
        PhotonNetwork.LoadLevel(Utils.Scene.LOBBY);
    }

    /**
     * This method starts the currently waiting game.
     * Classes implementing this abstract class should override this method 
     * to implement more start game logic.
     */
    protected virtual void StartGame() {
        gameState = GAME_STATE.RUNNING;
    }

    /**
     * This method ends the currently running game.
     * Classes implementing this abstract class should override this method 
     * to implement more end game logic.
     */
    protected virtual void EndGame() {
        gameState = GAME_STATE.ENDED;

        StartCoroutine(CountdownToLeaveRoom());
    }

    /**
     * This method checks for a winning condition within the current game.
     * Classes implementing this abstract class should override this method 
     * to implement the checking logic and return a boolean indicating whether 
     * the game has been won.
     * This method should be called whenever the game score state is changed.
     */
    protected abstract bool CheckForWinCondition();

    /**
     * This method makes the local player leaves the room back to the lobby.
     * This method will always be called after the game has ended and the
     * countdown to leave has elapsed.
     */
    private void LeaveRoom() {
        UIManager.Instance.ResetCursor();

        PhotonNetwork.LeaveRoom();
    }

    /**
     * This method does a countdown before starting the game while announcing 
     * the current countdown progress to the UIManager.
     */
    private IEnumerator CountdownToStartGameCoroutine() {
        yield return new WaitForSecondsRealtime(delayToCountdownToStartGame);

        for (int i = countdownToStartGame; i > 0; i--) {
            UIManager.Instance.announcementText.text = "Game starting in\n" + i.ToString();
            yield return new WaitForSecondsRealtime(1.0f);
        }
        UIManager.Instance.announcementText.text = "";

        StartGame();
    }

    /**
     * This method does a countdown before leaving the room back to lobby 
     * while announcing the current countdown progress to the UIManager.
     */
    private IEnumerator CountdownToLeaveRoom() {
        yield return new WaitForSecondsRealtime(delayToCountdownToLeave);

        for (int i = countdownToLeave; i > 0; i--) {
            UIManager.Instance.announcementText.text = "Leaving room in\n" + i.ToString();
            yield return new WaitForSecondsRealtime(1.0f);
        }
        UIManager.Instance.announcementText.text = "";

        LeaveRoom();
    }

}
