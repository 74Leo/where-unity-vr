using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EndLevel : MonoBehaviour
{
    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Le joueur a terminé le niveau !");
        }
    }
}
