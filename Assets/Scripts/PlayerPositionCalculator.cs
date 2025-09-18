using UnityEngine;
using SocketIOClient;
using System;
using System.Threading.Tasks;

public class PlayerPositionCalculator : MonoBehaviour
{
    [Header("Coordonnées de la map")]
    [SerializeField] private Vector2 mapBottomLeft = new Vector2(0.48f, 1.24f);
    [SerializeField] private Vector2 mapTopRight = new Vector2(64.71f, 37.88f);
    
    [Header("Auto-détection")]
    [SerializeField] private bool autoDetectMapBounds = true;

    [Header("Position du joueur")]
    [SerializeField] private Transform playerTransform;

    [Header("Résultats")]
    [SerializeField] private float xPercentage;
    [SerializeField] private float yPercentage;

    [Header("Configuration Socket.IO")]
    [SerializeField] private string serverUrl = "http://localhost:3001";
    [SerializeField] private string roomId = "";
    [SerializeField] private string playerPseudo = "";

    [Header("État de connexion")]
    private bool isConnected = false;
    private bool isInRoom = false;
    private bool joiningRoom = false;

    private SocketIOClient.SocketIO socket;

    void Start()
    {
        // Auto-détecter les limites de la map si activé
        if (autoDetectMapBounds)
        {
            AutoDetectMapBounds();
        }

        // Connexion automatique au serveur
        ConnectToServer();
    }

    void Update()
    {
        if (playerTransform != null)
        {
            CalculatePlayerPercentage();
        }

        // Récupérer le roomId depuis SocketPinClient si disponible
        if (string.IsNullOrEmpty(roomId) && SocketPinClient.Instance != null)
        {
            roomId = SocketPinClient.Instance.GetCurrentRoomId();
            if (!string.IsNullOrEmpty(roomId))
            {
                Debug.Log($"[PlayerPositionCalculator] RoomId récupéré depuis SocketPinClient: {roomId}");
            }
        }
            // Rejoindre la room si on a le roomId et qu'on est connecté mais pas encore dans la room
        // (une seule fois, pas en boucle)
        if (!string.IsNullOrEmpty(roomId) && isConnected && !isInRoom && !joiningRoom)
        {
            Debug.Log($"[PlayerPositionCalculator] 🔗 Rejoindre automatiquement la room {roomId}");
            joiningRoom = true;
            JoinRoom(roomId, "JoueurUnity");
        }

        if (Time.frameCount % 6 == 0)
        {
            if (socket != null && isConnected && isInRoom && !string.IsNullOrEmpty(roomId))
            {
                SendPositionToWeb();
            }
            else
            {
                if (socket == null) Debug.LogWarning("[PlayerPositionCalculator] Socket null");
                if (!isConnected) Debug.LogWarning("[PlayerPositionCalculator] Pas connecté au serveur");
                if (!isInRoom) Debug.LogWarning("[PlayerPositionCalculator] Pas dans une room");
                if (string.IsNullOrEmpty(roomId)) Debug.LogWarning("[PlayerPositionCalculator] RoomId vide");
            }
        }
    }

    void CalculatePlayerPercentage()
    {
        Vector2 playerPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);
        xPercentage = Mathf.InverseLerp(mapBottomLeft.x, mapTopRight.x, playerPosition.x);
        yPercentage = 1.0f -Mathf.InverseLerp(mapBottomLeft.y, mapTopRight.y, playerPosition.y);
    }

    public Vector2 GetPlayerPercentage()
    {
        return new Vector2(xPercentage, yPercentage);
    }

    public void SetMapBounds(Vector2 bottomLeft, Vector2 topRight)
    {
        mapBottomLeft = bottomLeft;
        mapTopRight = topRight;
    }

    public void SetServerUrl(string url)
    {
        serverUrl = url;
    }

    public void SetRoomId(string room)
    {
        roomId = room;
    }

    public void SetPlayerPseudo(string pseudo)
    {
        playerPseudo = pseudo;
    }

    public async void ConnectToServer()
    {
        if (socket == null)
        {
            await SetupSocketConnection();
        }
    }

    private async Task SetupSocketConnection()
    {
        socket = new SocketIOClient.SocketIO(serverUrl);

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("🔌 Connecté au serveur Socket.IO");
            isConnected = true;
            
            // Attendre un peu que le roomId soit disponible
            StartCoroutine(JoinRoomWhenReady());
        };

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("❌ Déconnecté du serveur Socket.IO");
            isConnected = false;
            isInRoom = false;
        };

        socket.On("room:joined", response =>
        {
            Debug.Log("✅ Rejoint la salle: " + roomId);
            isInRoom = true;
            joiningRoom = false; // Reset du flag

        });

        socket.On("room:left", response =>
        {
            Debug.Log("🚪 Quitté la salle");
            isInRoom = false;
            joiningRoom = false; // Reset du flag
        });

        // Écouter room:players pour confirmer qu'on est dans la room
        socket.On("room:players", response =>
        {
            Debug.Log("✅ Reçu room:players - confirmé dans la room");
            isInRoom = true;
            joiningRoom = false; // Reset du flag
        });

        await socket.ConnectAsync();
    }

    public async void JoinRoom(string roomId, string pseudo)
    {
        this.roomId = roomId;
        this.playerPseudo = pseudo;

        if (socket != null && isConnected)
        {
            await socket.EmitAsync("room:join", new { roomId = roomId });
            await socket.EmitAsync("player:create", pseudo);
        }
    }

    public async void SendPositionToWeb()
    {
        if (socket != null && isConnected && isInRoom)
        {
            Vector2 position = GetPlayerPercentage();

            var positionData = new
            {
                roomId = roomId,
                pseudo = playerPseudo,
                position = new
                {
                    x = position.x,
                    y = position.y
                },
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            Debug.Log($"[PlayerPositionCalculator] 📤 Envoi position: {position.x:F2}, {position.y:F2} vers room {roomId}");
            await socket.EmitAsync("player:position", positionData);
        }
        else
        {
            Debug.LogWarning("[PlayerPositionCalculator] ⚠️ Impossible d'envoyer position - socket null ou pas connecté");
        }
    }

    [ContextMenu("Afficher Position")]
    public void DisplayPosition()
    {
        Debug.Log($"Position du joueur: X={xPercentage:F2}, Y={yPercentage:F2}");
        Debug.Log($"Coordonnées réelles: X={playerTransform.position.x:F2}, Y={playerTransform.position.y:F2}");
    }

    [ContextMenu("Envoyer Position Manuellement")]
    public void SendPositionManually()
    {
        SendPositionToWeb();
        Debug.Log("📤 Position envoyée manuellement");
    }

    [ContextMenu("Reconnecter Socket")]
    public async void ReconnectSocket()
    {
        if (socket != null)
        {
            await socket.DisconnectAsync();
            socket.Dispose();
        }
        await SetupSocketConnection();
    }

    private async void OnDestroy()
    {
        if (socket != null)
        {
            await socket.EmitAsync("room:leave", new { roomId = roomId });
            await socket.DisconnectAsync();
            socket.Dispose();
        }
    }

    private async void OnApplicationQuit()
    {
        if (socket != null)
        {
            await socket.EmitAsync("room:leave", new { roomId = roomId });
            await socket.DisconnectAsync();
            socket.Dispose();
        }
    }

    public bool IsConnected => isConnected;
    public bool IsInRoom => isInRoom;
    public string CurrentRoomId => roomId;
    public string CurrentPlayerPseudo => playerPseudo;

    private System.Collections.IEnumerator JoinRoomWhenReady()
    {
        // Attendre que le roomId soit disponible
        while (string.IsNullOrEmpty(roomId))
        {
            yield return new WaitForSeconds(0.1f);
            roomId = SocketPinClient.Instance?.GetCurrentRoomId();
        }
        
        // Rejoindre la room dès qu'on a le roomId
        if (!string.IsNullOrEmpty(roomId) && !isInRoom)
        {
            Debug.Log($"[PlayerPositionCalculator] 🔗 Rejoindre la room {roomId} dès la connexion");
            JoinRoom(roomId, "JoueurUnity");
        }
    }

    [ContextMenu("Auto-détecter les bords de la map")]
    private void AutoDetectMapBounds()
    {
        Debug.Log("[PlayerPositionCalculator] 🔍 Auto-détection des bords de la map...");
        
        // Trouver tous les colliders visibles de la map
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
        
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        int mapCollidersFound = 0;
        
        foreach (Collider2D collider in allColliders)
        {
            // Ignorer les colliders du joueur et des objets non-visibles
            if (collider.gameObject == playerTransform.gameObject || 
                collider.isTrigger || 
                collider.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
                continue;
                
            Bounds bounds = collider.bounds;
            minX = Mathf.Min(minX, bounds.min.x);
            minY = Mathf.Min(minY, bounds.min.y);
            maxX = Mathf.Max(maxX, bounds.max.x);
            maxY = Mathf.Max(maxY, bounds.max.y);
            mapCollidersFound++;
        }
        
        if (mapCollidersFound > 0 && minX != float.MaxValue)
        {
            mapBottomLeft = new Vector2(minX, minY);
            mapTopRight = new Vector2(maxX, maxY);
            Debug.Log($"[PlayerPositionCalculator] ✅ Bords auto-détectés: BottomLeft({minX:F2}, {minY:F2}) TopRight({maxX:F2}, {maxY:F2})");
            Debug.Log($"[PlayerPositionCalculator] 📊 {mapCollidersFound} colliders de map trouvés");
        }
        else
        {
            Debug.LogWarning("[PlayerPositionCalculator] ⚠️ Aucun collider de map trouvé. Utilisation des coordonnées manuelles.");
        }
    }
}
