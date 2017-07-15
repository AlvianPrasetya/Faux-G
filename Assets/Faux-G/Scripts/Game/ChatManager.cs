using System.Collections;

public class ChatManager : Photon.PunBehaviour {

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

    private static ChatManager instance;

    private Queue messageQueue;

    public static ChatManager Instance {
        get {
            return instance;
        }
    }

    void Awake() {
        instance = this;

        messageQueue = new Queue();
    }

    public void SendChatMessage(PhotonPlayer sendingPlayer, string message) {
        // TODO: Message preprocessing and recipient identification before sending

        photonView.RPC("RpcSendChatMessage", PhotonTargets.All, sendingPlayer.ID, message);
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

        string chatText = "";
        foreach (Message message in messageQueue) {
            chatText += message.sendingPlayer.NickName + ": " + message.message + "\n";
        }

        UIManager.Instance.chatText.text = chatText;
    }

    [PunRPC]
    private void RpcSendChatMessage(int sendingPlayerId, string message) {
        PhotonPlayer sendingPlayer = PhotonPlayer.Find(sendingPlayerId);

        QueueMessage(new Message(sendingPlayer, message));
    }

}
