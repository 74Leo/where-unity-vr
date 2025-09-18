using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using SocketIOClient;

public class RealtimeSender : MonoBehaviour
{

    [SerializeField]

    private Transform playerTransform;

    public string serverUrl = "https://where-server-1.onrender.com";
    public string roomId = "room-001";
    public float sendHz = 15f;

    public bool autoFindTraps = true;

    private SocketIOClient.SocketIO client;
    private List<Trap> traps = new();

    IEnumerator Start()
    {
        Debug.Log("RealtimeSender starting...");

        Application.runInBackground = true;

        if (autoFindTraps)
        {
            traps.AddRange(FindObjectsByType<Trap>(FindObjectsSortMode.None));
        }

        client = new SocketIOClient.SocketIO(serverUrl, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        client.OnConnected += async (s, e) =>
        {
            Debug.Log("Socket.IO connected");
            await client.EmitAsync("join", new { roomId, role = "broadcaster" });
        };

        client.OnDisconnected += (s, e) => Debug.Log("Socket.IO disconnected");


        var connectTask = client.ConnectAsync();
        while (!connectTask.IsCompleted) yield return null;

        var interval = 1f / Mathf.Max(1f, sendHz);
        while (client.Connected)
        {
            _ = client.EmitAsync("state", playerTransform.position.x, playerTransform.position.y);
            yield return new WaitForSeconds(interval);
        }
    }

    private async void OnApplicationQuit()
    {
        if (client != null && client.Connected)
        {
            try { await client.DisconnectAsync(); } catch { }
        }
    }

}