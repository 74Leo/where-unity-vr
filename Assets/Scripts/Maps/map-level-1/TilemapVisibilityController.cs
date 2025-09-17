using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisibilityController : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap[] wallsMaps;
    public Tilemap[] boxMaps;
    public Tilemap[] box2Maps;
    public Tilemap[] ladderMaps;
    public Tilemap[] vaseMaps;
    public Tilemap[] box3Maps;
    public Tilemap[] chestMaps;

    [Header("Configuration")]
    public float updateInterval = 1f;
    public bool hideAllByDefault = true;
    
    private int currentPlayerCount = 0;

    void Start()
    {
        SetAllObstaclesInvisible();
        InvokeRepeating(nameof(UpdatePlayerCount), 1f, updateInterval);
    }

    void SetAllObstaclesInvisible()
    {
        SetTilemapsVisibility(wallsMaps, false);
        SetTilemapsVisibility(boxMaps, false);
        SetTilemapsVisibility(box2Maps, false);
        SetTilemapsVisibility(ladderMaps, false);
        SetTilemapsVisibility(vaseMaps, false);
        SetTilemapsVisibility(box3Maps, false);
        SetTilemapsVisibility(chestMaps, false);
    }

    void UpdatePlayerCount()
    {
        if (SocketPinClient.Instance != null && SocketPinClient.Instance.IsConnected())
        {
            int newCount = GetPlayerCountFromSocket();
            if (newCount != currentPlayerCount)
            {
                currentPlayerCount = newCount;
                UpdateObstacleVisibility();
                Debug.Log($"[TilemapVisibilityController] Joueurs: {currentPlayerCount}");
            }
        }
    }

    int GetPlayerCountFromSocket()
    {
        try
        {
            var socketClient = SocketPinClient.Instance;
            if (socketClient != null && socketClient.IsConnected())
            {
                string roomId = socketClient.GetCurrentRoomId();
                if (!string.IsNullOrEmpty(roomId))
                {
                    return CountPlayersInRoom(roomId);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[TilemapVisibilityController] Erreur: {ex.Message}");
        }
        return 0;
    }

    int CountPlayersInRoom(string roomId)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        return players.Length;
    }

    void UpdateObstacleVisibility()
    {
        SetTilemapsVisibility(wallsMaps, currentPlayerCount >= 1);
        SetTilemapsVisibility(boxMaps, currentPlayerCount >= 2);
        SetTilemapsVisibility(box2Maps, currentPlayerCount >= 3);
        SetTilemapsVisibility(ladderMaps, currentPlayerCount >= 4);
        SetTilemapsVisibility(vaseMaps, currentPlayerCount >= 5);
        SetTilemapsVisibility(box3Maps, currentPlayerCount >= 6);
        SetTilemapsVisibility(chestMaps, currentPlayerCount >= 7);
    }

    void SetTilemapsVisibility(Tilemap[] tilemaps, bool visible)
    {
        if (tilemaps == null) return;

        foreach (Tilemap tilemap in tilemaps)
        {
            SetTilemapVisibility(tilemap, visible);
        }
    }

    void SetTilemapVisibility(Tilemap tilemap, bool visible)
    {
        if (tilemap == null) return;

        if (visible)
        {
            if (hideAllByDefault)
            {
                tilemap.color = new Color(255f, 255f, 255f, 0f);
            }
            else
            {
                tilemap.color = new Color(255f, 255f, 255f, 1f);
            }
            EnableTilemapCollisions(tilemap);
        }
        else
        {
            tilemap.color = new Color(255f, 255f, 255f, 0f);
            DisableTilemapCollisions(tilemap);
        }
    }

    void EnableTilemapCollisions(Tilemap tilemap)
    {
        Collider2D collider = tilemap.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    void DisableTilemapCollisions(Tilemap tilemap)
    {
        Collider2D collider = tilemap.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(UpdatePlayerCount));
    }
}
