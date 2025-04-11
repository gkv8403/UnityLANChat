using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the chat user interface, button interactions,
/// and communication with the player and chat manager.
/// </summary>
public class ChatUI : MonoBehaviour
{
    // Input field for user to type messages
    public TMP_InputField inputField;

    // Text field to display all chat messages
    public TextMeshProUGUI chatText;

    // Scroll view that holds the chat text area
    public ScrollRect scrollRect;

    // UI Buttons for host, join, sending messages, and toggling chat view
    public Button hostButton;
    public Button joinButton;
    public Button msgButton;
    public Button back;

    // Reference to the ChatManager
    public ChatManager chatManager;

    // UI containers for chat and player information
    public GameObject Chatarea;
    public GameObject PlayerInfo;

    /// <summary>
    /// Adds a new message to the chat box and scrolls to bottom.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public void AddMessage(string message)
    {
        chatText.text += message + "\n";                      // Append message to chat
        Canvas.ForceUpdateCanvases();                         // Force UI to update layout
        scrollRect.verticalNormalizedPosition = 1f;           // Auto-scroll to bottom
    }

    /// <summary>
    /// Logs the state of the NetworkManager and ChatManager at startup.
    /// Useful for debugging.
    /// </summary>
    private void Start()
    {
        Debug.Log("NetworkManager: " + NetworkManager.singleton);
        Debug.Log("ChatManager.Instance: " + ChatManager.Instance);
    }

    /// <summary>
    /// Called when the user presses the send button or hits enter.
    /// Sends the typed message to the server via PlayerController.
    /// </summary>
    public void OnSendMessage()
    {
        if (string.IsNullOrWhiteSpace(inputField.text)) return;

        var identity = NetworkClient.connection?.identity;
        if (identity != null && identity.TryGetComponent<PlayerController>(out var player))
        {
            player.CmdSendMessage(inputField.text);   // Send the message
            inputField.text = "";                     // Clear the input field
        }
    }

    /// <summary>
    /// Called when the Host button is clicked.
    /// Starts hosting and advertises server over LAN.
    /// </summary>
    public void OnHostClick()
    {
        NetworkManager.singleton.StartHost();                                // Start as host
        ChatManager.Instance.networkDiscovery.AdvertiseServer();             // LAN broadcast
        HideConnectionButtons();                                             // Hide UI buttons
    }

    /// <summary>
    /// Called when the Join button is clicked.
    /// Attempts to find and connect to a host on LAN.
    /// </summary>
    public void OnJoinClick()
    {
        HideConnectionButtons();  // Hide buttons early for better user experience

        if (chatManager != null)
            chatManager.TryAutoJoin();  // Try joining a discovered host
        else
            Debug.LogError("ChatManager.Instance is null!");
    }

    /// <summary>
    /// Hides host/join buttons and shows messaging button and player info.
    /// Called after successful host or join.
    /// </summary>
    public void HideConnectionButtons()
    {
        hostButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);

        msgButton.gameObject.SetActive(true);    // Show message toggle button
        PlayerInfo.SetActive(true);              // Show local player name UI
    }

    /// <summary>
    /// Opens the chat message area when the msgButton is clicked.
    /// </summary>
    public void OpenMSG()
    {
        msgButton.gameObject.SetActive(false);   // Hide button
        Chatarea.SetActive(true);                // Show chat panel
        back.gameObject.SetActive(true);         // Show back button
        PlayerInfo.SetActive(false);             // Hide player name temporarily
    }

    /// <summary>
    /// Closes the chat panel and brings back the message button.
    /// </summary>
    public void CloseMSG()
    {
        msgButton.gameObject.SetActive(true);    // Bring back message toggle button
        Chatarea.SetActive(false);               // Hide chat panel
        back.gameObject.SetActive(false);        // Hide back button
        PlayerInfo.SetActive(true);              // Show player info again
    }
}
