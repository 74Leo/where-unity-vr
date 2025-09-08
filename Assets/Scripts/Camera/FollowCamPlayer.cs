using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FollowCamPlayer : MonoBehaviour
{
    [Tooltip("Transform du joueur à suivre")]
    public Transform target;

    [Tooltip("Temps (en secondes) que met la caméra pour rattraper la cible — plus petit,  plus réactif")]
    public float smoothTime = 0.15f;

    [Tooltip("Décalage de la caméra par rapport à la position du joueur (ex: (0,0,-10) pour une 2D)")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        desiredPosition.z = offset.z;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}
/*
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}*/
