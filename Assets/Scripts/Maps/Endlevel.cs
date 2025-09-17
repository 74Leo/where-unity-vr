using UnityEngine;

public class EndLevel : MonoBehaviour
{
    [Tooltip("Nom de la scène cible")]
    public string targetSceneName;

    [Tooltip("Bloque les retriggers pendant le chargement.")]
    public bool oneShot = true;

    bool used = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used && oneShot) return;
        if (!other.CompareTag("Player")) return;

        used = true;
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[EndLevel] targetSceneName est vide.");
            used = false;
            return;
        }

        FadeScreen.LoadSceneWithFadeAsync(targetSceneName);
        
        if (!oneShot) used = false;
    }
}
