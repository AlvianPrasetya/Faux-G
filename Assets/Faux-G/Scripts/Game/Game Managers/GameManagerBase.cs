using UnityEngine;
using System.Collections;

/**
 * This is the base abstract class for game managers. The implementing class
 * controls all the lifecycles of the game and what to do on such events.
 * New game managers are to implement this abstract class accordingly.
 */
public abstract class GameManagerBase : MonoBehaviour {

    public enum GAME_STATE {
        WAITING, // WAITING state, the state before the game starts (waiting for players)
        RUNNING, // RUNNING state, the normal running state of the game
        PAUSED, // PAUSED state, the game is paused for one reason or another
        ENDED // ENDED state, the game has ended due to reaching a winning condition
    }

    public float winCheckInterval;

    protected GAME_STATE gameState;

    private static GameManagerBase instance;

    public static GameManagerBase Instance {
        get {
            return instance;
        }
    }

    protected virtual void Awake() {
        PhotonNetwork.sendRate = Utils.SEND_RATE;
        PhotonNetwork.sendRateOnSerialize = Utils.SEND_RATE_ON_SERIALIZE;

        gameState = GAME_STATE.WAITING;
        instance = this;

        StartCoroutine(CheckForWinConditionCoroutine());
    }

    protected virtual void Start() {
        StartGame();
    }

    /**
     * This method checks for a winning condition within the current game.
     * Classes implementing this abstract class should override this method 
     * to implement the checking logic and call EndGame() if such winning 
     * condition is found.
     */
    protected abstract void CheckForWinCondition();

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
    }

    /**
     * This IEnumerator periodically checks for winning condition every 
     * winCheckInterval seconds and while the gameState is not ENDED.
     */
    private IEnumerator CheckForWinConditionCoroutine() {
        while (gameState != GAME_STATE.ENDED) {
            CheckForWinCondition();

            yield return new WaitForSeconds(winCheckInterval);
        }
    }

}
