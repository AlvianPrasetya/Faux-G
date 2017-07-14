using UnityEngine;
using UnityEngine.UI;

/**
 * This class controls UI behaviour during the game.
 */
public class UIManager : MonoBehaviour {
    
    public Text targetInfoText;
    public Text standingsText;
    public Text announcementText;

    private static UIManager instance;

    private bool isCursorLocked;
    private Camera playerCamera;

    void Awake() {
        instance = this;

        isCursorLocked = true;
    }

	void Update() {
        InputToggleCursor();

        /* 
         * Update cursor state must be called on every Update() to overcome cursor flickering
         * issue on WebGL.
         */
        UpdateCursorState();
        UpdateTargetInfoText();
    }

    public static UIManager Instance {
        get {
            return instance;
        }
    }

    public Camera PlayerCamera {
        set {
            playerCamera = value;
        }
    }

    public void ResetCursor() {
        isCursorLocked = false;
        UpdateCursorState();
    }

    private void InputToggleCursor() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            isCursorLocked = false;
        } else if (Input.anyKeyDown) {
            isCursorLocked = true;
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

    /**
     * This method updates the text that indicates target's name based on the entity
     * currently pointed by the camera.
     */
    private void UpdateTargetInfoText() {
        if (playerCamera == null) {
            return;
        }

        targetInfoText.text = "";

        RaycastHit hitInfo;
        bool hitSomething = Physics.Raycast(
            playerCamera.transform.position,
            playerCamera.transform.forward,
            out hitInfo,
            Mathf.Infinity,
            Utils.Layer.TERRAIN | Utils.Layer.DETECT_THROWABLE);
        if (hitSomething) {
            PlayerController targetPlayerController = hitInfo.transform.GetComponentInParent<PlayerController>();
            if (targetPlayerController != null) {
                targetInfoText.text = targetPlayerController.NickName;
            }
        }
    }

}
