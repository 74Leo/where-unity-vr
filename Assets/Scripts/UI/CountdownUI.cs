using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    [Header("Référence du joueur")]
    public Transform player;

    [Header("Position de respawn")]
    [Tooltip("Position où le joueur sera placé quand le chrono arrive à 0")]
    public Vector3 respawnPosition = new Vector3(5f, 3f, 6f);

    [Header("Durée du chrono (en secondes)")]
    public float duration = 120f;

    private float timeLeft;
    private bool isRunning = false;
    
    void Start()
    {
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
    }

    public void StartCountdown()
    {
        timeLeft = duration;
        isRunning = true;
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