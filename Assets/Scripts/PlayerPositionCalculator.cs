using UnityEngine;
using SocketIOClient;
using System;
using System.Threading.Tasks;

public class PlayerPositionCalculator : MonoBehaviour
{
    [Header("Coordonnées de la map")]
    [SerializeField] private Vector2 mapBottomLeft = new Vector2(0, 0);
    [SerializeField] private Vector2 mapTopRight = new Vector2(100, 100);

    [Header("Position du joueur")]
    [SerializeField] private Transform playerTransform;

    [Header("Résultats")]
    [SerializeField] private float xPercentage;
    [SerializeField] private float yPercentage;

    [Header("Configuration Socket.IO")]
    [SerializeField] private string serverUrl = "http://localhost:3000"; // à changer par ton serveur
    [SerializeField] private string roomId = "";
    [SerializeField] private string playerPseudo = "";

    [Header("État de connexion")]
    [SerializeField] private bool isConnected = false;
    [SerializeField] private bool isInRoom = false;

    private SocketIOClient.SocketIO socket;

    void Start()
    {
        // optionnel : connexion auto au lancement
        // ConnectToServer();
    }

    void Update()
    {
        if (playerTransform != null)
        {
            CalculatePlayerPercentage();
        }

        if (Time.frameCount % 6 == 0 && isConnected && isInRoom)
        {
            SendPositionToWeb();
        }
    }

    void CalculatePlayerPercentage()
    {
        Vector2 playerPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);
        xPercentage = Mathf.InverseLerp(mapBottomLeft.x, mapTopRight.x, playerPosition.x);
        yPercentage = Mathf.InverseLerp(mapBottomLeft.y, mapTopRight.y, playerPosition.y);
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
        });

        socket.On("room:left", response =>
        {
            Debug.Log("🚪 Quitté la salle");
            isInRoom = false;
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

            await socket.EmitAsync("player:position", positionData);
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
}
