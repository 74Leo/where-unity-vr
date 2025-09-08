using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public bool requirePlayerTag = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (requirePlayerTag && !other.CompareTag("Player")) return;

        Debug.Log("[GoalTrigger] Joueur arrivé à la fin du niveau !");
    }
}