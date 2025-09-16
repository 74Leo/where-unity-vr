using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scenes")]
    public string levelToLoad;

    [Header("Windows")]
    public GameObject settingsWindow;
    
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
}
