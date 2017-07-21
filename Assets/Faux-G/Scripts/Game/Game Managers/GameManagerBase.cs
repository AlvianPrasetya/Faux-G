using UnityEngine;
using System.Collections;

/**
 * This is the base abstract class for game managers. The implementing class
 * controls all the lifecycles of the game and what to do on such events.
 * New game managers are to implement this abstract class accordingly.
 */
public abstract class GameManagerBase : Photon.PunBehaviour {

	public delegate void OnStandingsUpdatedCallback(string standingsString);
	public delegate void OnLeftRoomCallback();

	public enum GAME_STATE {
		WAITING, // WAITING state, the state before the game starts (waiting for players)
		RUNNING, // RUNNING state, the normal running state of the game
		PAUSED, // PAUSED state, the game is paused for one reason or another
		ENDED // ENDED state, the game has ended due to reaching a winning condition
	}

	public NetworkManager networkManager;
	public UIManager uiManager;
	public ChatManager chatManager;
	public AttractorManager attractorManager;
	public ObjectPoolManager objectPoolManager;
	public CameraManager cameraManager;

	public int delayToStartGameCountdown;
	public int startGameCountdown;
	public int delayToLeaveRoomCountdown;
	public int leaveRoomCountdown;
	
	protected GAME_STATE gameState;
	
	protected OnStandingsUpdatedCallback standingsUpdatedCallback;
	private OnLeftRoomCallback leftRoomCallback;

	private static GameManagerBase instance;

	protected virtual void Awake() {
		PhotonNetwork.sendRate = Utils.Network.SEND_RATE;
		PhotonNetwork.sendRateOnSerialize = Utils.Network.SEND_RATE_ON_SERIALIZE;

		AddLeftRoomCallback(uiManager.ResetCursor);
		AddStandingsUpdatedCallback(uiManager.UpdateStandingsText);
		uiManager.AddChatMessageSentCallback(chatManager.SendChatMessage);
		chatManager.AddMessageQueuedCallback(uiManager.UpdateChatText);

		gameState = GAME_STATE.WAITING;

		instance = this;
	}

	protected virtual void Start() {
		StartCoroutine(StartGameCountdownCoroutine());
	}

	public override void OnLeftRoom() {
		if (leftRoomCallback != null) {
			leftRoomCallback();
		}

		PhotonNetwork.LoadLevel(Utils.Scene.LOBBY);
	}

	public static GameManagerBase Instance {
		get {
			return instance;
		}
	}

	public void AddStandingsUpdatedCallback(OnStandingsUpdatedCallback standingsUpdatedCallback) {
		if (this.standingsUpdatedCallback == null) {
			this.standingsUpdatedCallback = standingsUpdatedCallback;
		} else {
			this.standingsUpdatedCallback += standingsUpdatedCallback;
		}
	}

	public void AddLeftRoomCallback(OnLeftRoomCallback leftRoomCallback) {
		if (this.leftRoomCallback == null) {
			this.leftRoomCallback = leftRoomCallback;
		} else {
			this.leftRoomCallback += leftRoomCallback;
		}
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

		StartCoroutine(LeaveRoomCountdownCoroutine());
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
		PhotonNetwork.LeaveRoom();
	}

	/**
	 * This method does a countdown before starting the game while announcing 
	 * the current countdown progress to the UIManager.
	 */
	private IEnumerator StartGameCountdownCoroutine() {
		yield return new WaitForSecondsRealtime(delayToStartGameCountdown);

		for (int i = startGameCountdown; i > 0; i--) {
			uiManager.Announce("Game starting in...\n" + i.ToString());
			yield return new WaitForSecondsRealtime(1.0f);
		}
		uiManager.Announce("START");

		StartGame();
	}

	/**
	 * This method does a countdown before leaving the room back to lobby 
	 * while announcing the current countdown progress to the UIManager.
	 */
	private IEnumerator LeaveRoomCountdownCoroutine() {
		yield return new WaitForSecondsRealtime(delayToLeaveRoomCountdown);

		for (int i = leaveRoomCountdown; i > 0; i--) {
			uiManager.Announce("Leaving room in...\n" + i.ToString());
			yield return new WaitForSecondsRealtime(1.0f);
		}

		LeaveRoom();
	}

}
