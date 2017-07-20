using System.Collections;

public class ChatManager : Photon.MonoBehaviour {

	public delegate void OnMessageQueuedCallback(string chatString);

	private struct Message {

		public PhotonPlayer sendingPlayer;
		public string message;

		public Message(PhotonPlayer sendingPlayer, string message) {
			this.sendingPlayer = sendingPlayer;
			this.message = message;
		}

	}

	// The maximum amount of chat messages before older messages are deleted
	public int maxChatMessages;

	private Queue messageQueue;
	private OnMessageQueuedCallback messageQueuedCallback;

	void Awake() {
		messageQueue = new Queue();
	}

	public void AddMessageQueuedCallback(OnMessageQueuedCallback messageQueuedCallback) {
		if (this.messageQueuedCallback == null) {
			this.messageQueuedCallback = messageQueuedCallback;
		} else {
			this.messageQueuedCallback += messageQueuedCallback;
		}
	}

	public void SendChatMessage(PhotonPlayer sendingPlayer, string message) {
		// TODO: Message preprocessing and recipient identification before sending

		bool privateMessage = false;
		foreach (PhotonPlayer targetPlayer in PhotonNetwork.otherPlayers) {
			if (message.Contains("@" + targetPlayer.NickName)) {
				// Send private message to target
				photonView.RPC("RpcSendChatMessage", targetPlayer, sendingPlayer.ID, message);
				privateMessage = true;
			}
		}

		if (privateMessage) {
			// Send a copy of private message to self as well
			photonView.RPC("RpcSendChatMessage", PhotonNetwork.player, sendingPlayer.ID, message);
		} else {
			// Send normal chat message to all players
			photonView.RPC("RpcSendChatMessage", PhotonTargets.All, sendingPlayer.ID, message);
		}
	}

	/**
	 * This method queues the specified message into the message queue for display on 
	 * the chat tab. It will also delete the oldest message when the message count 
	 * reaches maxChatMessages.
	 */
	private void QueueMessage(Message queuedMessage) {
		messageQueue.Enqueue(queuedMessage);
		if (messageQueue.Count > maxChatMessages) {
			messageQueue.Dequeue();
		}

		string chatString = "";
		foreach (Message message in messageQueue) {
			chatString += message.sendingPlayer.NickName + ": " + message.message + "\n";
		}

		if (messageQueuedCallback != null) {
			messageQueuedCallback(chatString);
		}
	}

	[PunRPC]
	private void RpcSendChatMessage(int sendingPlayerId, string message) {
		PhotonPlayer sendingPlayer = PhotonPlayer.Find(sendingPlayerId);

		QueueMessage(new Message(sendingPlayer, message));
	}

}
