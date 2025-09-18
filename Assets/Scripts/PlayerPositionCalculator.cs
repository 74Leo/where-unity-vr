using UnityEngine;
using SocketIOClient;

// Script pour calculer la position du joueur en pourcentage par rapport aux bords de la map
public class PlayerPositionCalculator : MonoBehaviour
{
    [Header("Coordonnées de la map")]
    // Point bas-gauche de la map (x1, y1)
    [SerializeField] private Vector2 mapBottomLeft = new Vector2(0, 0);
    // Point haut-droit de la map (x2, y2)
    [SerializeField] private Vector2 mapTopRight = new Vector2(100, 100);

    [Header("Position du joueur")]
    // Référence au Transform du joueur pour récupérer sa position
    [SerializeField] private Transform playerTransform;

    [Header("Résultats")]
    // Position normalisée sur l'axe X (0-1)
    [SerializeField] private float xPercentage;
    // Position normalisée sur l'axe Y (0-1)
    [SerializeField] private float yPercentage;

    [Header("Configuration Socket.IO")]
    [SerializeField] private string serverUrl = "";
    [SerializeField] private string roomId = "";
    [SerializeField] private string playerPseudo = "";

    [Header("État de connexion")]
    [SerializeField] private bool isConnected = false;
    [SerializeField] private bool isInRoom = false;

    private SocketIOUnity socket;

    void Start()
    {

    }

    void Update()
    {
        // on check que le joueur est assigné avant de calculer
        if (playerTransform != null)
        {
            CalculatePlayerPercentage();
        }


        if (Time.frameCount % 6 == 0 && isConnected && isInRoom)
        {
            SendPositionToWeb();
        }
    }

    // Méthode qui calcule la position du joueur en pourcentage
    void CalculatePlayerPercentage()
    {
        // on recupere la position actuelle du joueur en 2D (x3, y3)
        Vector2 playerPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);

        // on utilise InverseLerp pour calculer la position X (retourne 0-1)
        xPercentage = Mathf.InverseLerp(mapBottomLeft.x, mapTopRight.x, playerPosition.x);
        // on utilise InverseLerp pour calculer la position Y (retourne 0-1)
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

    public void ConnectToServer()
    {
        if (socket == null)
        {
            SetupSocketConnection();
        }
    }
    void SetupSocketConnection()
    {
        // Créer l'instance Socket.IO
        socket = new SocketIOUnity(serverUrl);

        // Événements de connexion
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("🔌 Connecté au serveur Socket.IO");
            isConnected = true;
        };

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log(" Déco du serveur Socket.IO");
            isConnected = false;
            isInRoom = false;
        };

        socket.On("room:joined", (response) =>
        {
            Debug.Log(" Rejoint la salle: " + roomId);
            isInRoom = true;
        });

        socket.On("room:left", (response) =>
        {
            Debug.Log(" Quitté la salle");
            isInRoom = false;
        });

        socket.Connect();
    }

    public void JoinRoom(string roomId, string pseudo)
    {
        this.roomId = roomId;
        this.playerPseudo = pseudo;

        if (socket != null && isConnected)
        {
            socket.Emit("room:join", new { roomId = roomId });
            socket.Emit("player:create", pseudo);
        }
    }

    public void SendPositionToWeb()
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

            socket.Emit("player:position", positionData);
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
        Debug.Log("Position envoyée manuellement");
    }

    [ContextMenu("Reconnecter Socket")]
    public void ReconnectSocket()
    {
        if (socket != null)
        {
            socket.Disconnect();
            socket.Dispose();
        }
        SetupSocketConnection();
    }

    void OnDestroy()
    {
        if (socket != null)
        {
            socket.Emit("room:leave", new { roomId = roomId });
            socket.Disconnect();
            socket.Dispose();
        }
    }

    void OnApplicationQuit()
    {
        if (socket != null)
        {
            socket.Emit("room:leave", new { roomId = roomId });
            socket.Disconnect();
            socket.Dispose();
        }
    }

    public bool IsConnected => isConnected;
    public bool IsInRoom => isInRoom;
    public string CurrentRoomId => roomId;
    public string CurrentPlayerPseudo => playerPseudo;
}