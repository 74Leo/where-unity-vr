using UnityEngine;
using UnityEngine.Tilemaps;
using SocketIOClient;
using System.Collections.Generic;

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
    
    private int serverPlayerCount = 0;

    void Start()
    {
        SetAllObstaclesInvisible();
        SubscribeToSocketEvents();
    }

    void SubscribeToSocketEvents()
    {
        if (SocketPinClient.Instance != null)
        {
            SocketPinClient.Instance.SubscribeToRoomPlayers(OnRoomPlayersUpdate);
        }
    }

    void OnRoomPlayersUpdate(SocketIOResponse response)
    {
        try
        {
            var players = response.GetValue<List<object>>();
            if (players != null)
            {
                serverPlayerCount = players.Count;
                UpdateObstacleVisibility();
                Debug.Log($"[TilemapVisibilityController] Serveur: {serverPlayerCount} joueurs");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[TilemapVisibilityController] Erreur parsing room:players: {ex.Message}");
        }
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


    void UpdateObstacleVisibility()
    {
        SetTilemapsVisibility(wallsMaps, serverPlayerCount >= 1);
        SetTilemapsVisibility(boxMaps, serverPlayerCount >= 2);
        SetTilemapsVisibility(box2Maps, serverPlayerCount >= 3);
        SetTilemapsVisibility(ladderMaps, serverPlayerCount >= 4);
        SetTilemapsVisibility(vaseMaps, serverPlayerCount >= 5);
        SetTilemapsVisibility(box3Maps, serverPlayerCount >= 6);
        SetTilemapsVisibility(chestMaps, serverPlayerCount >= 7);
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
                tilemap.color = new Color(1f, 1f, 1f, 0f);
            }
            else
            {
                tilemap.color = new Color(1f, 1f, 1f, 1f);
            }
            EnableTilemapCollisions(tilemap);
        }
        else
        {
            tilemap.color = new Color(1f, 1f, 1f, 0f);
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
    }
}
