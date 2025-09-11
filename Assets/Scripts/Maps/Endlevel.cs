using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class EndLevel : MonoBehaviour
{
    public string levelToLoad;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Le joueur a terminé le niveau !");
            SceneManager.LoadScene(levelToLoad);
        }
    }
}
