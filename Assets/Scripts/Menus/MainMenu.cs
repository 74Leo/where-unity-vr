using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scenes")]
    public string levelToLoad;

    [Header("Windows")]
    public GameObject settingsWindow;

    [Header("PIN")]
    [Tooltip("Composant Text qui affichera le PIN (ex: \"PIN: 1234\")")]
    public Text pinText;

    [Tooltip("Nombre de chiffres du PIN")]
    [Range(1, 9)]
    public int pinLength = 4;

    private string currentPin;

    void Start()
    {
        GeneratePin();
        UpdatePinUI();
    }

    public void StartGame()
    {
        if (!string.IsNullOrEmpty(levelToLoad))
            SceneManager.LoadScene(levelToLoad);
        else
            Debug.LogWarning("[MainMenu] levelToLoad non configuré.");
    }

    public void SettingsButton()
    {
        if (settingsWindow != null)
            settingsWindow.SetActive(true);
    }

    public void CloseSettingsWindow()
    {
        if (settingsWindow != null)
            settingsWindow.SetActive(false);
    }

    public void LoadCreditsScene()
    {
        SceneManager.LoadScene("Credits");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RegeneratePin()
    {
        GeneratePin();
        UpdatePinUI();
    }

    private void GeneratePin()
    {
        int maxExclusive = 1;
        for (int i = 0; i < pinLength; i++) maxExclusive *= 10;

        int value = Random.Range(0, maxExclusive);
        currentPin = value.ToString("D" + pinLength);
    }

    private void UpdatePinUI()
    {
        if (pinText != null)
            pinText.text = "PIN: " + currentPin;
        else
            Debug.LogWarning("[MainMenu] pinText non assigné dans l'inspector.");
    }

    public string GetCurrentPin() => currentPin;
}
