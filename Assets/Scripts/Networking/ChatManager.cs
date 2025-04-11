using Mirror;
using Mirror.Discovery;
using Mirror.Examples.Chat;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages chat communication, player registration, UI updates,
/// and LAN host discovery using Mirror Networking and NetworkDiscovery.
/// </summary>
public class ChatManager : NetworkBehaviour
{
    // Singleton instance for easy access across the project
    public static ChatManager Instance;

    // Reference to the NetworkDiscovery component for LAN host discovery
    [SerializeField] public NetworkDiscovery networkDiscovery;

    // List to keep track of all registered players
    public List<PlayerController> players = new List<PlayerController>();

    // Reference to the UI manager that handles chat and connection buttons
    public ChatUI chatUI;

    // UI elements for displaying the list of players and the local player's name
    public TMP_Text playersListText;
    public TMP_Text myNameText;

    /// <summary>
    /// Initializes the singleton instance and sets up network discovery.
    /// </summary>
    private void Awake()
    {
        Instance = this;
        Debug.Log("ChatManager Awake called");

        // Automatically find the NetworkDiscovery object if not set in inspector
        if (networkDiscovery == null)
        {
            networkDiscovery = FindObjectOfType<NetworkDiscovery>();
            if (networkDiscovery == null)
            {
                Debug.LogError("NetworkDiscovery not found in scene!");
            }
            else
            {
                Debug.Log("NetworkDiscovery found");
            }
        }
    }

    /// <summary>
    /// Registers a new player with the chat manager and updates the player list UI.
    /// </summary>
    /// <param name="pc">The PlayerController to register.</param>
    public void RegisterPlayer(PlayerController player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            Debug.Log("Player registered: " + player.playerName);
        }
        else
        {
            Debug.Log("Player already registered: " + player.playerName);
        }

        UpdatePlayerList();
    }


    /// <summary>
    /// Called on all clients to display a received chat message.
    /// </summary>
    /// <param name="msg">The message to show.</param>
    [ClientRpc]
    public void RpcReceiveMessage(string msg)
    {
        chatUI.AddMessage(msg);
    }

    /// <summary>
    /// Called on all clients to update the player list UI.
    /// </summary>
    [ClientRpc]
    void RpcUpdatePlayerList()
    {
        UpdatePlayerList();
    }

    /// <summary>
    /// Updates the player list UI with all current players and highlights the local player.
    /// </summary>
    public void UpdatePlayerList()
    {
        if (playersListText == null || myNameText == null) return;

        string allPlayers = "Players in Room:\n";

        foreach (var p in players)
        {
            if (p != null)
            {
                string name = p.playerName;
                if (p.isHost)
                {
                    name += " (Host)";
                }

                allPlayers += "- " + name + "\n";
            }
        }

        playersListText.text = allPlayers;

        // Find local player from list
        var local = players.Find(p => p != null && p.isLocalPlayer);
        if (local != null)
        {
            myNameText.text = "You are: " + local.playerName;
        }
        else
        {
            Debug.LogWarning("Local player not found in player list!");
            myNameText.text = "You are: ???";
        }
    }



    /// <summary>
    /// Starts the application as a Host and advertises the server over LAN.
    /// </summary>
    public void StartAsHost()
    {
        NetworkManager.singleton.StartHost();           // Start server and client on the same machine
        networkDiscovery.AdvertiseServer();             // Make host discoverable over LAN
        Debug.Log("Started as Host");
    }

    /// <summary>
    /// Searches for an available host on the local network and connects if found.
    /// </summary>
    public void TryAutoJoin()
    {
        Debug.Log("TryAutoJoin called...");

        if (networkDiscovery == null)
        {
            Debug.LogError("NetworkDiscovery is null!");
            return;
        }

        // Remove any previous listeners to avoid duplicate callbacks
        networkDiscovery.OnServerFound.RemoveAllListeners();

        // Listen for a discovered host
        networkDiscovery.OnServerFound.AddListener((response) =>
        {
            Debug.Log("Found host at: " + response.EndPoint.Address);
            NetworkManager.singleton.networkAddress = response.EndPoint.Address.ToString(); // Set network address
            NetworkManager.singleton.StartClient();                                         // Start as client
            chatUI.HideConnectionButtons();                                                 // Hide connection UI
        });

        Debug.Log("Searching for Host...");
        networkDiscovery.StartDiscovery(); // Begin scanning for available servers
    }

    /// <summary>
    /// (Unused) Separate callback method for host discovery (kept for clarity or future use).
    /// </summary>
    void OnServerFound(ServerResponse info)
    {
        Debug.Log("Found Host at: " + info.EndPoint.Address);
        NetworkManager.singleton.networkAddress = info.EndPoint.Address.ToString();
        NetworkManager.singleton.StartClient();
        chatUI.HideConnectionButtons();
    }
}
