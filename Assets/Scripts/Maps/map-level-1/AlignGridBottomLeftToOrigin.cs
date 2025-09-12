using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class AlignGridBottomLeftToOrigin : MonoBehaviour
{
    [Tooltip("Grid à déplacer (laisse vide pour prendre celui de ce GameObject).")]
    public GridLayout grid;

    [Tooltip("Compresser les bounds des Tilemaps avant le calcul.")]
    public bool compressBounds = true;

    [Tooltip("Inclure aussi les Tilemaps désactivées.")]
    public bool includeInactive = true;

    void Reset()
    {
        if (!grid) grid = GetComponent<GridLayout>();
    }

    [ContextMenu("Align Now")]
    public void AlignNow()
    {
        if (!grid) grid = GetComponent<GridLayout>();
        if (!grid) { Debug.LogError("[AlignGrid] Pas de GridLayout trouvé."); return; }

        var tilemaps = grid.GetComponentsInChildren<Tilemap>(includeInactive);
        if (tilemaps == null || tilemaps.Length == 0)
        {
            Debug.LogError("[AlignGrid] Aucune Tilemap sous la Grid.");
            return;
        }

        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;
        bool foundAny = false;

        foreach (var tm in tilemaps)
        {
            if (!tm) continue;
            if (compressBounds) tm.CompressBounds();

            // Ignorer les tilemaps vides
            if (tm.localBounds.size.sqrMagnitude < 1e-6f) continue;

            Vector3 localMin = tm.localBounds.min;
            localMin.z = 0f;
            Vector3 worldMin = tm.transform.TransformPoint(localMin);

            if (worldMin.x < minX) minX = worldMin.x;
            if (worldMin.y < minY) minY = worldMin.y;
            foundAny = true;
        }

        if (!foundAny)
        {
            Debug.LogWarning("[AlignGrid] Toutes les Tilemaps semblent vides.");
            return;
        }

        Vector3 delta = new Vector3(-minX, -minY, 0f);
        grid.transform.position += delta;
    }
}
