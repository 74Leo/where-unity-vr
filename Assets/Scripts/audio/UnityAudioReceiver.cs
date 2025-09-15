using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using Unity.WebRTC;

public class UnityAudioReceiver : MonoBehaviour
{
    public string serverUrl = "https://where-server-1.onrender.com";
    public string roomId = "room-001";

    private SocketIOClient.SocketIO client;
    private RTCPeerConnection peerConnection;
    private AudioSource audioSource;

    IEnumerator Start()
    {
        Debug.Log("UnityAudioReceiver starting...");

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = true;

        client = new SocketIOClient.SocketIO(serverUrl, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        client.OnConnected += async (s, e) =>
        {
            Debug.Log("✅ Socket.IO connected to server");
            await client.EmitAsync("join", new { roomId, role = "listener" });
            Debug.Log("📤 Join room emitted");
            SetupPeerConnection();
        };

        client.OnDisconnected += (s, e) =>
        {
            Debug.LogWarning("⚠ Socket.IO disconnected");
        };

        client.On("webrtc-offer", async response =>
        {
            Debug.Log("📩 Offer received from server");
        });

        client.On("webrtc-candidate", response =>
        {
            Debug.Log("🧩 Candidate received from server");
        });

        var connectTask = client.ConnectAsync();
        while (!connectTask.IsCompleted) yield return null;
        Debug.Log("🔗 ConnectAsync completed");
    }

    private void SetupPeerConnection()
    {
        Debug.Log("🔧 Setting up PeerConnection...");
        peerConnection = new RTCPeerConnection();
        peerConnection.OnTrack = e => Debug.Log("🎵 OnTrack triggered");
        peerConnection.OnIceCandidate = candidate => Debug.Log($"💡 OnIceCandidate: {candidate?.Candidate}");
        Debug.Log("✅ PeerConnection ready");
    }

    private async void OnApplicationQuit()
    {
        Debug.Log("🛑 Application quitting...");

        if (peerConnection != null)
        {
            peerConnection.Close();
            peerConnection.Dispose();
            Debug.Log("✅ PeerConnection disposed");
        }

        if (client != null && client.Connected)
        {
            try
            {
                await client.DisconnectAsync();
                Debug.Log("✅ Socket.IO disconnected");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error disconnecting Socket.IO: {ex.Message}");
            }
        }
    }
}
