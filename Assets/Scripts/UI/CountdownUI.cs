using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CountdownUI : MonoBehaviour
{
    [Header("Référence du joueur")]
    public Transform player;

    [Header("Position de respawn")]
    [Tooltip("Position où le joueur sera placé quand le chrono arrive à 0")]
    public Vector3 respawnPosition = new Vector3(5f, 3f, 6f);

    [Header("Référence du Timer")]
    [Tooltip("GameObject contenant le script Timer")]
    public Timer timerScript;

    [Header("Configuration Web")]
    [Tooltip("URL du serveur")]
    public string serverUrl = "https://where-server-1.onrender.com";
    
    [Tooltip("Intervalle d'envoi du timer (en secondes)")]
    public float sendInterval = 1f;

    private float timeLeft;
    private bool isRunning = false;
    private float lastSendTime = 0f;
    
    void Start()
    {
        StartCountdown();
    }

    void Update()
    {
        if (!isRunning) return;

        timeLeft -= Time.deltaTime;

        // Envoyer le temps restant au serveur à intervalles réguliers
        if (Time.time - lastSendTime >= sendInterval)
        {
            StartCoroutine(SendTimerUpdate());
            lastSendTime = Time.time;
        }

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            isRunning = false;

            // Envoyer que le timer est à 0
            StartCoroutine(SendTimerUpdate());

            if (player != null)
            {
                player.position = respawnPosition;
            }

            if (timerScript != null)
            {
                timerScript.StartTimer();
            }

            StartCountdown();
        }
    }

    public void StartCountdown()
    {
        if (timerScript != null)
        {
            timeLeft = timerScript.ReturnTotalSeconds();
        }
        else
        {
            timeLeft = 60f;
        }
        isRunning = true;
        
        // Envoyer le début du nouveau countdown
        StartCoroutine(SendTimerUpdate());
    }

    IEnumerator SendTimerUpdate()
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        
        string jsonData = $"{{\"event\":\"timer:update\",\"timeLeft\":{timeLeft},\"minutes\":{minutes},\"seconds\":{seconds},\"isRunning\":{(isRunning ? "true" : "false")}}}";
        
        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/timer", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[CountdownUI] Erreur lors de l'envoi du timer: " + request.error);
            }
        }
    }
}