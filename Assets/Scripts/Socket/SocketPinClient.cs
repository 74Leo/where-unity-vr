using System;
using System.Collections.Generic;
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
    public TextMeshProUGUI playerCountText;

    private SocketIOClient.SocketIO client;
    private bool connected = false;

    async void Start()
    {
        Debug.Log("[SocketPinClient] Initialisation du client Socket.IO vers: " + serverUrl);
        
        client = new SocketIOClient.SocketIO(serverUrl, new SocketIOClient.SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        client.OnConnected += (s, e) =>
        {
            Debug.Log("[SocketPinClient] ✅ Connecté au serveur");
            connected = true;
            RequestPin();
        };

        client.OnDisconnected += (s, e) =>
        {
            Debug.LogWarning("[SocketPinClient] ❌ Déconnecté du serveur");
            connected = false;
            UpdatePinText("PIN: ----");
        };

        client.On("room:players", HandleRoomPlayers);
        client.On("room:created", HandleRoomCreated);
        Debug.Log("[SocketPinClient] Écouteurs d'événements configurés");

        try
        {
            Debug.Log("[SocketPinClient] Tentative de connexion...");
            await client.ConnectAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError("[SocketPinClient] ❌ Erreur de connexion: " + ex.Message);
            UpdatePinText("PIN: ERREUR");
        }
    }

    private void HandleRoomCreated(SocketIOResponse response)
    {
        Debug.Log("[SocketPinClient] 📨 Événement room créé reçu");
        try
        {
            var dict = response.GetValue<Dictionary<string, object>>();
            string pin = dict?.ContainsKey("roomId") == true ? dict["roomId"]?.ToString() : response.ToString();
            
            Debug.Log("[SocketPinClient] PIN extrait: " + pin);
            
            if (!string.IsNullOrEmpty(pin))
            {
                UpdatePinText("PIN: " + pin);
                Debug.Log("[SocketPinClient] ✅ PIN mis à jour dans l'UI: " + pin);
            }
            else
            {
                Debug.LogWarning("[SocketPinClient] ⚠️ PIN vide ou null");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[SocketPinClient] ❌ Erreur lors du traitement du PIN: " + ex.Message);
        }
    }

    private void HandleRoomPlayers(SocketIOResponse response)
    {
        Debug.Log("[SocketPinClient] 👥 Liste des joueurs reçue");
        try
        {
            var playersList = response.GetValue<List<Dictionary<string, object>>>();
            if (playersList != null)
            {
                int playerCount = playersList.Count;
                UpdatePlayerCount(playerCount);
                Debug.Log("[SocketPinClient] ✅ Nombre de joueurs mis à jour: " + playerCount);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[SocketPinClient] ❌ Erreur lors du traitement room:players: " + ex.Message);
        }
    }

    private void RequestPin()
    {
        if (!connected) 
        {
            Debug.LogWarning("[SocketPinClient] ⚠️ Impossible de demander un PIN - non connecté");
            return;
        }

        try
        {
            Debug.Log("[SocketPinClient] 📤 Envoi de la demande de PIN (room:create)");
            client.EmitAsync("room:create");
            Debug.Log("[SocketPinClient] ✅ Demande de PIN envoyée");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[SocketPinClient] ❌ Erreur lors de la demande de PIN: " + ex.Message);
        }
    }

    private void UpdatePinText(string text)
    {
        if (pinText != null)
        {
            pinText.text = text;
            Debug.Log("[SocketPinClient] 🎨 UI mise à jour: " + text);
        }
        else
        {
            Debug.LogWarning("[SocketPinClient] ⚠️ pinText non assigné - impossible de mettre à jour l'UI");
        }
    }

    private void UpdatePlayerCount(int count)
    {
        if (playerCountText != null)
        {
            playerCountText.text = count + " Connectes";
            Debug.Log("[SocketPinClient] 👥 Nombre de joueurs mis à jour: " + count);
        }
        else
        {
            Debug.LogWarning("[SocketPinClient] ⚠️ playerCountText non assigné - impossible de mettre à jour le compteur");
        }
    }

    async void OnDestroy()
    {
        Debug.Log("[SocketPinClient] 🗑️ Destruction du client Socket.IO");
        if (client != null)
        {
            try
            {
                Debug.Log("[SocketPinClient] 📤 Déconnexion en cours...");
                await client.DisconnectAsync();
                client.Dispose();
                Debug.Log("[SocketPinClient] ✅ Déconnexion terminée");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SocketPinClient] ❌ Erreur lors de la déconnexion: " + ex.Message);
            }
        }
    }
}
