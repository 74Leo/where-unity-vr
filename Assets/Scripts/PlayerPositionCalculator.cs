using UnityEngine;

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
    
    // Méthode appelée à chaque frame pour mettre à jour les calculs
    void Update()
    {
        // on check que le joueur est assigné avant de calculer
        if (playerTransform != null)
        {
            CalculatePlayerPercentage();
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
        
        // InverseLerp gère automatiquement les limites
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
    public void SendPositionToWeb()
    {

    }
    
    [ContextMenu("Afficher Position")]
    public void DisplayPosition()
    {
        Debug.Log($"Position du joueur: X={xPercentage:F2}, Y={yPercentage:F2}");
        Debug.Log($"Coordonnées réelles: X={playerTransform.position.x:F2}, Y={playerTransform.position.y:F2}");
    }
}