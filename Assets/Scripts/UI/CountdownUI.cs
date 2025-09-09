using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    [Header("Durée du chrono (en secondes)")]
    public float duration = 60f;

    [Header("Références UI")]
    public TMP_Text timerText;
    public Image timerFill;

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
