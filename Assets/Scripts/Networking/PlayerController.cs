using Mirror;
using UnityEngine;
using System.Collections; 
/// <summary>
/// Controls the player behavior in a LAN multiplayer chat application using Mirror.
/// Handles player name assignment and sending messages through commands and RPCs.
/// </summary>
public class PlayerController : NetworkBehaviour
{
    // SyncVar ensures this variable is synchronized across all clients when changed on the server
    [SyncVar] public string playerName;

    [SyncVar]
    public bool isHost;

    /// <summary>
    /// Command sent from client to server to broadcast a chat message.
    /// The server then relays this message to all clients via an RPC.
    /// </summary>
    /// <param name="message">The chat message sent by the player.</param>
    [Command]
    public void CmdSendMessage(string message)
    {
        // Broadcast the message to all clients
        ChatManager.Instance.RpcReceiveMessage($"{playerName}: {message}");
    }

    /// <summary>
    /// Called when this client becomes the local player.
    /// Assigns a random name and informs the server.
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        Debug.Log("OnStartLocalPlayer called for: " + netId);

        string randomName = "Player_" + Random.Range(1000, 9999);
        CmdSetPlayerName(randomName);
    }


    /// <summary>
    /// Command to set the player name on the server.
    /// Also registers the player with the ChatManager.
    /// </summary>
    /// <param name="name">The randomly generated player name.</param>
    [Command]
    void CmdSetPlayerName(string name)
    {
        playerName = name;

        if (NetworkServer.connections.Count == 1)
            isHost = true;

        Debug.Log($"CmdSetPlayerName called on server for {playerName}, isHost: {isHost}");

        // Register on server
        ChatManager.Instance.RegisterPlayer(this);

        // Also inform client manually
        TargetForceRegisterPlayer(connectionToClient);
    }

    [TargetRpc]
    void TargetForceRegisterPlayer(NetworkConnection target)
    {
        Debug.Log("TargetForceRegisterPlayer called on client");

        ChatManager.Instance.RegisterPlayer(this);

        // Delay the player list update to ensure isLocalPlayer is true
        StartCoroutine(DelayedUpdatePlayerList());
    }

    IEnumerator DelayedUpdatePlayerList()
    {
        yield return new WaitForSeconds(0.2f); // slight delay to ensure local player is ready
        ChatManager.Instance.UpdatePlayerList();
    }



    /// <summary>
    /// Called on every client when the player object is initialized.
    /// Ensures the player is registered with the ChatManager.
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();

        // Register the player with ChatManager (safety check for non-server clients)
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.RegisterPlayer(this);
        }
    }
}
