using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EndLevel : MonoBehaviour
{
    [Tooltip("Nom de la scène cible")]
    public string targetSceneName;

    [Tooltip("Bloque les retriggers pendant le chargement.")]
    public bool oneShot = true;

    [Tooltip("URL du serveur")]
    public string serverUrl = "https://where-server-1.onrender.com";

    bool used = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used && oneShot) return;
        if (!other.CompareTag("Player")) return;

        used = true;

        StartCoroutine(SendVictoryMessage());

        LoadNextScene();
    }

    IEnumerator SendVictoryMessage()
    {
        string jsonData = "{\"event\":\"game:victory\",\"message\":\"Vous avez gagné!\"}";
        
        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/victory", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[EndLevel] Message de victoire envoyé avec succès au serveur web");
            }
            else
            {
                Debug.LogError("[EndLevel] Erreur lors de l'envoi du message: " + request.error);
            }
        }
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
