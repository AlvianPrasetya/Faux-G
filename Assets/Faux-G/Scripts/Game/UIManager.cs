using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/**
 * This class controls UI behaviour during the game.
 */
public class UIManager : MonoBehaviour {

    public delegate void OnChatMessageSentCallback(PhotonPlayer sendingPlayer, string message);

    public Text targetInfoText;
    public Text standingsText;
    public Text announcementText;
    public Text chatText;
    public InputField chatInputField;

    public float announcementFadeTime;
    
    private bool isCursorLocked;
    private OnChatMessageSentCallback chatMessageSentCallback;

    void Awake() {
        chatInputField.interactable = false;

        isCursorLocked = true;
    }

	void Update() {
        InputToggleCursor();
        InputCheckChatInputField();

        /* 
         * Update cursor state must be called on every Update() to overcome cursor flickering
         * issue on WebGL.
         */
        UpdateCursorState();
    }

    public void AddChatMessageSentCallback(OnChatMessageSentCallback chatMessageSentCallback) {
        if (this.chatMessageSentCallback == null) {
            this.chatMessageSentCallback = chatMessageSentCallback;
        } else {
            this.chatMessageSentCallback += chatMessageSentCallback;
        }
    }

    public void Announce(string announcementString) {
        announcementText.text = announcementString;
        StartCoroutine(FadeAnnouncementCoroutine());
    }

    public void ResetCursor() {
        isCursorLocked = false;
        UpdateCursorState();
    }

    public void UpdateStandingsText(string standingsString) {
        standingsText.text = standingsString;
    }

    public void UpdateChatText(string chatString) {
        chatText.text = chatString;
    }

    private void InputToggleCursor() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            isCursorLocked = false;
        } else if (Input.anyKeyDown) {
            isCursorLocked = true;
        }
    }

    private void InputCheckChatInputField() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            CheckChatInputField();
        }
    }

    /**
     * This method updates the cursor state (lockability and visibility).
     */
    private void UpdateCursorState() {
        if (isCursorLocked) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void CheckChatInputField() {
        if (chatInputField.interactable) {
            SendChatMessage();
            chatInputField.DeactivateInputField();
            chatInputField.interactable = false;
        } else {
            chatInputField.interactable = true;
            chatInputField.Select();
            chatInputField.ActivateInputField();
        }
    }

    private void SendChatMessage() {
        if (chatInputField.text != "") {
            Logger.Log("Sending chat message " + chatInputField.text);

            if (chatMessageSentCallback != null) {
                chatMessageSentCallback(PhotonNetwork.player, chatInputField.text);
            }

            chatInputField.text = "";
        }
    }

    private IEnumerator FadeAnnouncementCoroutine() {
        float currentFadeTime = announcementFadeTime;

        while (currentFadeTime > 0.0f) {
            announcementText.color = new Color(
                announcementText.color.r, 
                announcementText.color.g, 
                announcementText.color.b, 
                currentFadeTime / announcementFadeTime);
            currentFadeTime -= Time.deltaTime;
            yield return null;
        }

        // Ensure alpha is 0 before coroutine is finished
        announcementText.color = new Color(
                announcementText.color.r,
                announcementText.color.g,
                announcementText.color.b,
                0.0f);
    }

}
