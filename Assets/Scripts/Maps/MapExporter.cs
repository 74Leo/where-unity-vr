using UnityEngine;
using System.IO;

public class MapExporter : MonoBehaviour
{
    [Header("Racine de la carte (parent de Ground/Wall/...)")]
    public Transform mapRoot;

    [Header("Couches rendues (ex: Map)")]
    public LayerMask cullingMask = ~0;

    [Header("Résolution de sortie")]
    public int outWidth  = 1920;
    public int outHeight = 1080;

    [Header("Nom de l'image")]
    public string nameImage = "map_level_one_export";

    [ContextMenu("Exporter la carte en PNG")]
    public void Export()
    {
        if (!mapRoot)
        {
            Debug.LogError("Assigne 'mapRoot' (le parent de ta carte).");
            return;
        }

        var renderers = mapRoot.GetComponentsInChildren<Renderer>(includeInactive: false);
        if (renderers.Length == 0)
        {
            Debug.LogError("Aucun Renderer trouvé sous 'mapRoot'.");
            return;
        }

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        float widthUnits  = Mathf.Max(b.size.x, 0.001f);
        float heightUnits = Mathf.Max(b.size.y, 0.001f);

        var camGO = new GameObject("~TempExportCam");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic   = true;
        cam.cullingMask    = cullingMask;
        cam.clearFlags     = CameraClearFlags.SolidColor;
        cam.backgroundColor= new Color(0f, 0f, 0f, 0f);
        cam.transform.position = new Vector3(b.center.x, b.center.y, -10f);
        cam.transform.rotation = Quaternion.identity;

        float targetAspect = (float)outWidth / outHeight;
        cam.aspect = targetAspect;

        float sizeByHeight = heightUnits / 2f;
        float sizeByWidth  = (widthUnits / 2f) / targetAspect;
        cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);

        var rt = new RenderTexture(outWidth, outHeight, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;

        var tex = new Texture2D(outWidth, outHeight, TextureFormat.RGBA32, false);
        cam.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, outWidth, outHeight), 0, 0);
        tex.Apply();

        string folder = Path.Combine(Application.dataPath, "Images");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, nameImage + ".png");
        File.WriteAllBytes(path, tex.EncodeToPNG());

        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        DestroyImmediate(camGO);

        Debug.Log("✅ Export PNG : " + path);
    }
}
