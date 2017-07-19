using System.Collections;

public class ChatManager : Photon.PunBehaviour {

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
        // Sending a private message
        if (message.StartsWith("@")) {
            foreach(PhotonPlayer targetPlayer in PhotonNetwork.otherPlayers) {
                if (message.StartsWith("@" + targetPlayer.NickName)){
                    photonView.RPC("RpcSendChatMessage", targetPlayer, sendingPlayer.ID, message);
                }
            }
        } else {
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
