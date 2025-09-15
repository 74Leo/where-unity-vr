using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using SocketIOClient;

public class SocketPinClient : MonoBehaviour
{
    [Header("Socket.IO")]
    public string serverUrl = "http://localhost:3001";

    [Header("Debug / UI")]
    public bool logPin = true;

    public event Action<string> OnPinReceived;

    private SocketIOClient.SocketIO client;
    private bool connected = false;
    private string currentPin;

    IEnumerator Start()
    {
        client = new SocketIOClient.SocketIO(serverUrl, new SocketIOClient.SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        client.OnConnected += (s, e) =>
        {
            Debug.Log("[SocketPinClient] Connected to server.");
            connected = true;
        };

        client.OnDisconnected += (s, e) =>
        {
            Debug.LogWarning("[SocketPinClient] Disconnected from server.");
            connected = false;
        };

        client.On("room:created", response =>
        {
            try
            {
                var dict = response.GetValue<Dictionary<string, object>>();
                if (dict != null && dict.ContainsKey("roomId"))
                {
                    HandlePin(dict["roomId"]?.ToString());
                    return;
                }

                var raw = response.ToString();
                HandlePin(raw);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SocketPinClient] room:created handler failed: " + ex.Message);
            }
        });

        client.On("room-created", response =>
        {
            try
            {
                var dict = response.GetValue<Dictionary<string, object>>();
                if (dict != null && dict.ContainsKey("roomId"))
                {
                    HandlePin(dict["roomId"]?.ToString());
                    return;
                }
                HandlePin(response.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SocketPinClient] room-created handler failed: " + ex.Message);
            }
        });

        var connectTask = client.ConnectAsync();
        while (!connectTask.IsCompleted) yield return null;

        if (connectTask.IsFaulted)
        {
            Debug.LogError("[SocketPinClient] ConnectAsync failed: " + connectTask.Exception?.Flatten().Message);
            yield break;
        }

        RequestPinFromServer();
    }

    private void RequestPinFromServer()
    {
        if (client == null || !connected)
        {
            Debug.LogWarning("[SocketPinClient] Client non connecté.");
            return;
        }

        try
        {
             var emitMethod = client.GetType().GetMethod("EmitAsync", new Type[] { typeof(string), typeof(Func<SocketIOResponse, Task>) });
            if (emitMethod != null)
            {
                Func<SocketIOResponse, Task> ack = (SocketIOResponse resp) =>
                {
                    try
                    {
                        var dict = resp.GetValue<Dictionary<string, object>>();
                        if (dict != null && dict.ContainsKey("roomId"))
                        {
                            HandlePin(dict["roomId"]?.ToString());
                            return Task.CompletedTask;
                        }

                        HandlePin(resp.ToString());
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("[SocketPinClient] ack handler error: " + ex.Message);
                    }
                    return Task.CompletedTask;
                };

                emitMethod.Invoke(client, new object[] { "room:create", ack });
                Debug.Log("[SocketPinClient] Emitted 'room:create' via EmitAsync with ack (reflection).");
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[SocketPinClient] EmitAsync with callback not available or failed: " + ex.Message);
        }

        try
        {
            var emitNoAck = client.GetType().GetMethod("EmitAsync", new Type[] { typeof(string), typeof(object[]) })
                            ?? client.GetType().GetMethod("EmitAsync", new Type[] { typeof(string), typeof(object) });

            if (emitNoAck != null)
            {
                try
                {
                    emitNoAck.Invoke(client, new object[] { "room:create", Array.Empty<object>() });
                }
                catch
                {
                    try { emitNoAck.Invoke(client, new object[] { "room:create", null }); } catch { }
                }

                Debug.Log("[SocketPinClient] Emitted 'room:create' without ack — waiting for server 'room:created' event.");
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[SocketPinClient] EmitAsync without ack failed: " + ex.Message);
        }

        Debug.LogWarning("[SocketPinClient] Impossible d'appeler EmitAsync automatiquement — vérifie la version de SocketIOClient. " +
            "Si nécessaire, écoute un événement serveur (room:created) côté serveur ou adapte le client à ton package.");
    }

    private void HandlePin(string pin)
    {
        if (string.IsNullOrEmpty(pin)) return;
        currentPin = pin;
        if (logPin) Debug.Log("[SocketPinClient] PIN reçu : " + currentPin);
        try { OnPinReceived?.Invoke(currentPin); } catch { }
    }

    public void RegeneratePin()
    {
        RequestPinFromServer();
    }

    private async void OnApplicationQuit()
    {
        if (client != null)
        {
            try
            {
                var disconnectMethod = client.GetType().GetMethod("DisconnectAsync", Type.EmptyTypes);
                if (disconnectMethod != null)
                {
                    var taskObj = disconnectMethod.Invoke(client, null) as Task;
                    if (taskObj != null) await taskObj;
                }
                client.Dispose();
                client = null;
                Debug.Log("[SocketPinClient] Déconnecté proprement.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SocketPinClient] Erreur lors de la déconnexion: " + ex.Message);
            }
        }
    }

    public string GetCurrentPin() => currentPin;
}
