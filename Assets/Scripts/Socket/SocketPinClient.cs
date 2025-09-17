using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIOClient;

public class SocketPinClient : MonoBehaviour
{
    [Header("Socket.IO")]
    public string serverUrl = "http://localhost:3001";

    [Header("UI")]
    public Text pinText;
    public TextMeshProUGUI playersCountText;

    private SocketIOClient.SocketIO client;
    private bool connected = false;
    private SynchronizationContext unitySyncContext;
    
    public static SocketPinClient Instance { get; private set; }
    private string currentRoomId = null;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LogMain("[SocketPinClient] Instance singleton créée et persistante");
        }
        else
        {
            LogMain("[SocketPinClient] Instance déjà existante, destruction du doublon");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (connected && client != null)
        {
            LogMain("[SocketPinClient] Client déjà connecté, réutilisation de la connexion");
            return;
        }

        unitySyncContext = SynchronizationContext.Current;

        LogMain("[SocketPinClient] Initialisation du client Socket.IO vers: " + serverUrl);
        
        PostToMain(() => UpdatePinText("PIN: ----"));
        PostToMain(() => UpdatePlayersCountText("-- Connectes"));

        client = new SocketIOClient.SocketIO(serverUrl, new SocketIOClient.SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        client.OnConnected += (s, e) =>
        {
            LogMain("[SocketPinClient] ✅ Connecté au serveur");
            connected = true;
            _ = SendCreateRoomEventAsync();
        };

        client.On("room:players", (response) =>
        {
            try
            {
                var players = response.GetValue<List<object>>();
                if (players != null)
                {
                    int playerCount = players.Count;
                    PostToMain(() => UpdatePlayersCountText(playerCount + " Connectes"));
                    PostToMain(() => Debug.Log("[SocketPinClient] 👥 Nombre de joueurs mis à jour: " + playerCount));
                }
            }
            catch (Exception ex)
            {
                PostToMain(() => Debug.LogWarning("[SocketPinClient] ❌ Erreur parsing room:players: " + ex.Message));
            }
        });

        client.OnDisconnected += (s, e) =>
        {
            LogMainWarning("[SocketPinClient] ❌ Déconnecté du (serveur");
            connected = false;
            PostToMain(() => UpdatePinText("PIN: ----"));
            PostToMain(() => UpdatePlayersCountText("-- Connectes"));
        };

        _ = ConnectAsync();
    }

    private async Task ConnectAsync()
    {
        try
        {
            LogMain("[SocketPinClient] Tentative de connexion...");
            await client.ConnectAsync();
        }
        catch (Exception ex)
        {
            LogMainError("[SocketPinClient] ❌ Erreur de connexion: " + ex.Message);
            PostToMain(() => UpdatePinText("PIN: ERREUR"));
            PostToMain(() => UpdatePlayersCountText("** Connectes"));
        }
    }

    private async Task SendCreateRoomEventAsync()
    {
        if (!connected)
        {
            LogMainWarning("[SocketPinClient] ⚠️ Impossible d'envoyer room:create — non connecté");
            return;
        }

        var tcs = new TaskCompletionSource<string>();
        Action<SocketIOResponse> handler = null;

        handler = (response) =>
        {
            try
            {
                string roomId = null;
                
                try
                {
                    var rawResponse = response.ToString();
                    PostToMain(() => Debug.Log("[SocketPinClient] 📥 Réponse brute: " + rawResponse));
                    
                    if (rawResponse.Contains("roomId"))
                    {
                        var startIndex = rawResponse.IndexOf("\"roomId\":\"") + 10;
                        var endIndex = rawResponse.IndexOf("\"", startIndex);
                        if (startIndex > 9 && endIndex > startIndex)
                        {
                            roomId = rawResponse.Substring(startIndex, endIndex - startIndex);
                        }
                    }
                }
                catch (Exception ex1)
                {
                    PostToMain(() => Debug.LogWarning("[SocketPinClient] ⚠️ Méthode 1 échouée: " + ex1.Message));
                }
                
                if (string.IsNullOrEmpty(roomId))
                {
                    try
                    {
                        var dict = response.GetValue<Dictionary<string, object>>();
                        if (dict != null && dict.ContainsKey("roomId"))
                        {
                            roomId = dict["roomId"]?.ToString();
                        }
                    }
                    catch (Exception ex2)
                    {
                        PostToMain(() => Debug.LogWarning("[SocketPinClient] ⚠️ Méthode 2 échouée: " + ex2.Message));
                    }
                }
                
                if (string.IsNullOrEmpty(roomId))
                {
                    try
                    {
                        var obj = response.GetValue<object>();
                        if (obj != null)
                        {
                            var objStr = obj.ToString();
                            PostToMain(() => Debug.Log("[SocketPinClient] 📥 Objet reçu: " + objStr));
                            
                            // Extraction simple
                            var startIndex = objStr.IndexOf("\"roomId\":\"") + 10;
                            var endIndex = objStr.IndexOf("\"", startIndex);
                            if (startIndex > 9 && endIndex > startIndex)
                            {
                                roomId = objStr.Substring(startIndex, endIndex - startIndex);
                            }
                        }
                    }
                    catch (Exception ex3)
                    {
                        PostToMain(() => Debug.LogWarning("[SocketPinClient] ⚠️ Méthode 3 échouée: " + ex3.Message));
                    }
                }
                
                if (!string.IsNullOrEmpty(roomId))
                {
                    tcs.TrySetResult(roomId);
                    return;
                }
                
                PostToMain(() => Debug.LogWarning("[SocketPinClient] ❌ Impossible d'extraire le roomId"));
            }
            catch (Exception ex)
            {
                PostToMain(() => Debug.LogWarning("[SocketPinClient] ❌ Erreur parsing réponse: " + ex.Message));
            }
            
            tcs.TrySetResult("ERREUR");
        };

        try
        {
            PostToMain(() => Debug.Log("[SocketPinClient] 📤 Emission de 'room:create' et attente de réponse..."));

            client.On("room:create:response", handler);

            await client.EmitAsync("room:create");

            var timeoutMs = 5000;
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));

            try
            {
                client.Off("room:create:response");
            }
            catch (Exception)
            {
            }

            if (completed == tcs.Task)
            {
                var pin = tcs.Task.Result;
                currentRoomId = pin;
                PostToMain(() => UpdatePinText("PIN: " + pin));
                PostToMain(() => Debug.Log("[SocketPinClient] ✅ PIN reçu: " + pin));
            }
            else
            {
                PostToMain(() => Debug.LogWarning("[SocketPinClient] ⚠️ Timeout en attente de réponse"));
                PostToMain(() => UpdatePinText("PIN: TIMEOUT"));
            }
        }
        catch (Exception ex)
        {
            PostToMain(() => Debug.LogWarning("[SocketPinClient] ❌ Erreur lors de l'émission: " + ex.Message));
            try { client.Off("room:create:response"); } catch { }
            PostToMain(() => UpdatePinText("PIN: ***"));
        }
    }


    private void PostToMain(Action action)
    {
        if (unitySyncContext != null)
        {
            unitySyncContext.Post(_ => action(), null);
        }
        else
        {
            try { action(); } catch { }
        }
    }
    
    private void LogMain(string msg) => PostToMain(() => Debug.Log(msg));
    private void LogMainWarning(string msg) => PostToMain(() => Debug.LogWarning(msg));
    private void LogMainError(string msg) => PostToMain(() => Debug.LogError(msg));

    private void UpdatePinText(string text)
    {
        if (pinText != null)
        {
            pinText.text = text;
            Debug.Log("[SocketPinClient] 🎨 UI PIN mise à jour: " + text);
        }
        else
        {
            Debug.LogWarning("[SocketPinClient] ⚠️ pinText non assigné - impossible de mettre à jour l'UI");
        }
    }

    private void UpdatePlayersCountText(string text)
    {
        if (playersCountText != null)
        {
            playersCountText.text = text;
            Debug.Log("[SocketPinClient] 🎨 UI Joueurs mise à jour: " + text);
        }
        else
        {
            Debug.LogWarning("[SocketPinClient] ⚠️ playersCountText non assigné - impossible de mettre à jour l'UI");
        }
    }

    public void LaunchGame()
    {
        if (!connected)
        {
            LogMainWarning("[SocketPinClient] ⚠️ Impossible de lancer la partie — non connecté");
            return;
        }

        _ = LaunchGameInternalAsync();
    }

    private async Task LaunchGameInternalAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        Action<SocketIOResponse> handler = null;

        handler = (response) =>
        {
            try
            {
                var dict = response.GetValue<Dictionary<string, object>>();
                if (dict != null && dict.ContainsKey("ok"))
                {
                    bool success = Convert.ToBoolean(dict["ok"]);
                    tcs.TrySetResult(success);
                    
                    if (success)
                    {
                        PostToMain(() => Debug.Log("[SocketPinClient] ✅ Partie lancée avec succès - obstacles assignés aux guides"));
                    }
                    else
                    {
                        string error = dict.ContainsKey("error") ? dict["error"].ToString() : "Erreur inconnue";
                        PostToMain(() => Debug.LogWarning("[SocketPinClient] ❌ Erreur lors du lancement: " + error));
                    }
                }
                else
                {
                    PostToMain(() => Debug.LogWarning("[SocketPinClient] ❌ Réponse invalide du serveur"));
                    tcs.TrySetResult(false);
                }
            }
            catch (Exception ex)
            {
                PostToMain(() => Debug.LogWarning("[SocketPinClient] ❌ Erreur parsing réponse game:launch: " + ex.Message));
                tcs.TrySetResult(false);
            }
        };

        try
        {
            PostToMain(() => Debug.Log("[SocketPinClient] 📤 Lancement de la partie (Unity)..."));

            await client.EmitAsync("game:launch", (response) =>
            {
                try
                {
                    var dict = response.GetValue<Dictionary<string, object>>();
                    if (dict != null && dict.ContainsKey("ok"))
                    {
                        bool success = Convert.ToBoolean(dict["ok"]);
                        
                        if (success)
                        {
                            PostToMain(() => Debug.Log("[SocketPinClient] ✅ Partie lancée avec succès - obstacles assignés aux guides"));
                        }
                        else
                        {
                            string error = dict.ContainsKey("error") ? dict["error"].ToString() : "Erreur inconnue";
                            PostToMain(() => Debug.LogWarning("[SocketPinClient] ❌ Erreur lors du lancement: " + error));
                        }
                    }
                    else
                    {
                        PostToMain(() => Debug.LogWarning("[SocketPinClient] ❌ Réponse invalide du serveur"));
                    }
                }
                catch (Exception ex)
                {
                    PostToMain(() => Debug.LogWarning("[SocketPinClient] ❌ Erreur parsing réponse game:launch: " + ex.Message));
                }
            });
        }
        catch (Exception ex)
        {
            PostToMain(() => Debug.LogWarning("[SocketPinClient] ❌ Erreur lors de l'émission game:launch: " + ex.Message));
        }
    }

    public void OnSceneChanged()
    {
        LogMain("[SocketPinClient] 🔄 Changement de scène détecté - connexion maintenue");
    }

    public string GetCurrentRoomId()
    {
        return currentRoomId;
    }

    public bool IsConnected()
    {
        return connected;
    }

    public async Task DisconnectAsync()
    {
        LogMain("[SocketPinClient] 📤 Déconnexion forcée...");
        if (client != null)
        {
            try
            {
                await client.DisconnectAsync();
                client.Dispose();
                connected = false;
                currentRoomId = null;
                LogMain("[SocketPinClient] ✅ Déconnexion terminée");
            }
            catch (Exception ex)
            {
                LogMainWarning("[SocketPinClient] ❌ Erreur lors de la déconnexion: " + ex.Message);
            }
        }
    }

    async void OnDestroy()
    {
        if (Instance == this)
        {
            LogMain("[SocketPinClient] 🗑️ Destruction du singleton - connexion maintenue");
            return;
        }

        LogMain("[SocketPinClient] 🗑️ Destruction du client Socket.IO");
        if (client != null)
        {
            try
            {
                LogMain("[SocketPinClient] 📤 Déconnexion en cours...");
                await client.DisconnectAsync();
                client.Dispose();
                LogMain("[SocketPinClient] ✅ Déconnexion terminée");
            }
            catch (Exception ex)
            {
                LogMainWarning("[SocketPinClient] ❌ Erreur lors de la déconnexion: " + ex.Message);
            }
        }
    }
}