using UnityEngine;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
//using UnityEngine.UI;
//using TMPro;

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

        if (Time.time - lastSendTime >= sendInterval)
        {
            StartCoroutine(SendTimerUpdate());
            lastSendTime = Time.time;
        }

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            isRunning = false;

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

/*
public class CountdownUI : MonoBehaviour
{
    [Header("Durée du chrono (en secondes)")]
    public float duration = 60f;

    [Header("Références UI")]
    public TMP_Text timerText;
    public Image timerFill;

    [Header("Style du texte")]
    public TMP_FontAsset countdownFont;
    public Color countdownColor = Color.white;

    [Header("Référence du joueur")]
    public Transform player;
    
    [Header("Position de respawn")]
    [Tooltip("Position où le joueur sera placé quand le chrono arrive à 0")]
    public Vector3 respawnPosition = new Vector3(5f, 3f, 6f);

    private float timeLeft;
    private bool isRunning = false;

    void Start()
    {
        if (timerText != null)
        {
            if (countdownFont != null)
                timerText.font = countdownFont;

            timerText.color = countdownColor;
        }
        
        StartCountdown();
    }

    void Update()
    {
        if (!isRunning) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            isRunning = false;

            if (player != null)
            {
                player.position = respawnPosition;
            }

            StartCountdown();
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (timerText != null)
            timerText.text = FormatTime(timeLeft);

        if (timerFill != null)
            timerFill.fillAmount = timeLeft / duration;
    }

    string FormatTime(float t)
    {
        int totalSeconds = Mathf.CeilToInt(t);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    public void StartCountdown()
    {
        timeLeft = duration;
        isRunning = true;
        UpdateUI();
    }
}
*/